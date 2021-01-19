using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class DefaultParameterRewriter : RewriterBase
  {
    public DefaultParameterRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      return RewriteMethod(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      return RewriteMethod(node);
    }

    private static SyntaxNode RewriteMethod(BaseMethodDeclarationSyntax node)
    {
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var res = node.ParameterList;

      foreach (var parameter in node.ParameterList.Parameters)
      {
        if (parameter.Default is { Value: LiteralExpressionSyntax { Token: { Text: "null" } } })
        {
          res = res.ReplaceNode(parameter.Type!, NullUtilities.ToNullableWithGenericsCheck(parameter.Type!));
        }
      }

      return node.WithParameterList(res);
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(Method method)
    {
      return method.Parents
          .Select(p => p.From)
          .OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ParameterChange))
          .ToArray();
    }
  }
}
