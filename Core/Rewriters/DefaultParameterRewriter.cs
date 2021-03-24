using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class DefaultParameterRewriter : RewriterBase
  {
    public DefaultParameterRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      return RewriteMethod(SemanticModel, node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      return RewriteMethod(SemanticModel, node);
    }

    private static SyntaxNode RewriteMethod(SemanticModel semanticModel, BaseMethodDeclarationSyntax node)
    {
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var res = node.ParameterList;

      foreach (var (parameter, index) in node.ParameterList.Parameters.Select((p, i) => (p, i)))
      {
        if (parameter.Type is {} && parameter.Type.IsValueType(semanticModel))
          continue;

        if (parameter.IsDefaultNullLiteral()
            || parameter.IsDefaultDefaultLiteral())
        {
          res = res.ReplaceNode(res.Parameters[index].Type!, NullUtilities.ToNullable(parameter.Type!));
        }
        else if (parameter.IsDefaultDefaultExpression())
        {
          res = res.ReplaceNode(res.Parameters[index].Type!, NullUtilities.ToNullable(parameter.Type!));
          var defaultExpression = (DefaultExpressionSyntax) res.Parameters[index].Default!.Value;
          res = res.ReplaceNode(defaultExpression.Type!, NullUtilities.ToNullable(defaultExpression.Type!));
        }
      }

      return node.WithParameterList(res);
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode method)
    {
      return base.GetAdditionalRewrites(method)
          .Concat(
              method.Parents
                  .Select(p => p.From)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ParameterChange)))
          .ToArray();
    }
  }
}
