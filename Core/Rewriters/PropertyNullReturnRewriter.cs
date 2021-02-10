using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  /// <summary>
  /// Specs:<br/>
  /// - Expression-bodied properties returning a nullable value are rewritten to a nullable reference type<br/>
  /// - Non-expression-bodied, non-auto-properties returning a nullable value are rewritten to a nullable reference type
  /// </summary>
  public class PropertyNullReturnRewriter : RewriterBase
  {
    public PropertyNullReturnRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (node.IsExpressionBodied() && NullUtilities.CanBeNull(node.ExpressionBody!.Expression, SemanticModel))
      {
        var containingClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        return node.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingClass!, node.Type));
      }

      if (node.HasNonAutoGetter())
      {
        var getter = node.AccessorList!.Accessors.Single(a => a.Keyword.IsKind(SyntaxKind.GetKeyword));

        var hasNullReturningExpressionBody = getter.ExpressionBody != null && NullUtilities.CanBeNull(getter.ExpressionBody.Expression, SemanticModel);
        var hasNullReturningStatementBody = getter.Body != null && NullUtilities.ReturnsNull(getter.Body.Statements, SemanticModel);
        var isNullReturning = hasNullReturningExpressionBody || hasNullReturningStatementBody;

        if (isNullReturning)
        {
          var containingClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
          if (containingClass != null)
          {
            return node.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingClass, node.Type));
          }
        }
      }

      return base.VisitPropertyDeclaration(node);
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode method)
    {
      return method.Parents
          .Select (p => p.From)
          .OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ReturnValueChange))
          .ToArray();
    }
  }
}
