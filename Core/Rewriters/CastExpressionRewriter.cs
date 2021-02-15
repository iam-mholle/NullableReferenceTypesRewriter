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
  public class CastExpressionRewriter : RewriterBase
  {
    public CastExpressionRewriter (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitCastExpression (CastExpressionSyntax node)
    {
      var type = node.Type;
      if (type is NullableTypeSyntax)
        return node;

      return NullUtilities.CanBeNull (node.Expression, SemanticModel)
          ? node.WithType (ToNullableWithFittingContext (type))
          : node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode node)
    {
      return node.Parents.Select(p => p.From).OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ReturnValueChange))
          .Concat(node.Children
              .Select(c => c.To)
              .OfType<IRewritable>()
              .Select(r => (r, RewriteCapability.ParameterChange)))
          .ToArray();
    }

    private TypeSyntax ToNullableWithFittingContext(TypeSyntax typeSyntax)
    {
      Func<TypeSyntax>? annotator = null;

      var containingMethod = typeSyntax.Ancestors()
          .Where(a => a.IsKind(SyntaxKind.MethodDeclaration))
          .Cast<MethodDeclarationSyntax>()
          .SingleOrDefault();

      if (containingMethod is null)
      {
        var containingClass = typeSyntax.Ancestors()
            .Where(a => a.IsKind(SyntaxKind.ClassDeclaration))
            .Cast<ClassDeclarationSyntax>()
            .First();

        annotator = () => NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingClass, typeSyntax);
      }
      else
      {
        annotator = () => NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingMethod, typeSyntax);
      }

      return annotator();
    }
  }
}