using System;
using System.Collections.Generic;
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
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var res = node.ParameterList;

      foreach (var parameter in node.ParameterList.Parameters)
      {
        if (parameter.Default is { Value: LiteralExpressionSyntax { Token: { Text: "null" } } })
        {
          res = res.ReplaceNode(parameter.Type!, NullUtilities.ToNullable(parameter.Type!));
        }
      }

      return node.WithParameterList(res);
    }
  }
}
