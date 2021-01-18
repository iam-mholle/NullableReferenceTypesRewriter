using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  // TODO: Generic handling not working properly
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

      var semanticModel = CurrentMethod.SemanticModel;

      return NullUtilities.CanBeNull (node.Expression, semanticModel)
          ? node.WithType (NullUtilities.ToNullable (type))
          : node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (Method method)
    {
      return method.Parents.Select(p => p.From).OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ReturnValueChange))
          .Concat(method.Children
              .Select(c => c.To)
              .OfType<IRewritable>()
              .Select(r => (r, RewriteCapability.ParameterChange)))
          .ToArray();
    }
  }
}