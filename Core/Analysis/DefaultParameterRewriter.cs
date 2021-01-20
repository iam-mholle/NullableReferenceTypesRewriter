using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  // TODO: if DefaultExpressionSyntax is used, also adapt passed type
  public class DefaultParameterRewriter : RewriterBase
  {
    public DefaultParameterRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      return RewriteMethod(CurrentMethod.SemanticModel, node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      return RewriteMethod(CurrentMethod.SemanticModel, node);
    }

    private static SyntaxNode RewriteMethod(SemanticModel semanticModel, BaseMethodDeclarationSyntax node)
    {
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var res = node.ParameterList;

      foreach (var parameter in node.ParameterList.Parameters)
      {
        if (IsParameterDefaultNull(parameter)
            || IsParameterDefaultDefault(semanticModel, parameter))
        {
          res = res.ReplaceNode(parameter.Type!, NullUtilities.ToNullable(parameter.Type!));
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

    private static bool IsParameterDefaultNull(ParameterSyntax parameterSyntax)
      => parameterSyntax.Default is { Value: LiteralExpressionSyntax { Token: { Text: "null" } } };

    private static bool IsParameterDefaultDefault(SemanticModel semanticModel, ParameterSyntax parameterSyntax)
      => semanticModel.GetTypeInfo(parameterSyntax.Type!).Type!.IsReferenceType
         && (parameterSyntax.Default is { Value: LiteralExpressionSyntax { Token: { Text: "default" } } }
             || parameterSyntax.Default is { Value: DefaultExpressionSyntax _ });
  }
}
