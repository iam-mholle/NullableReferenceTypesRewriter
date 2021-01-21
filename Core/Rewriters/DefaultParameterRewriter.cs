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

      foreach (var (parameter, index) in node.ParameterList.Parameters.Select((p, i) => (p, i)))
      {
        if (IsParameterDefaultNullLiteral(parameter)
            || IsParameterDefaultDefaultLiteral(semanticModel, parameter))
        {
          res = res.ReplaceNode(res.Parameters[index].Type!, NullUtilities.ToNullable(parameter.Type!));
        }
        else if (IsParameterDefaultDefaultExpression(semanticModel, parameter))
        {
          res = res.ReplaceNode(res.Parameters[index].Type!, NullUtilities.ToNullable(parameter.Type!));
          var defaultExpression = (DefaultExpressionSyntax) res.Parameters[index].Default!.Value;
          res = res.ReplaceNode(defaultExpression.Type!, NullUtilities.ToNullable(defaultExpression.Type!));
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

    private static bool IsParameterDefaultNullLiteral(ParameterSyntax parameterSyntax)
      => parameterSyntax.Default is { Value: LiteralExpressionSyntax { Token: { Text: "null" } } };

    private static bool IsParameterDefaultDefaultLiteral(SemanticModel semanticModel, ParameterSyntax parameterSyntax)
      => semanticModel.GetTypeInfo(parameterSyntax.Type!).Type!.IsReferenceType
         && parameterSyntax.Default is { Value: LiteralExpressionSyntax { Token: { Text: "default" } } };

    private static bool IsParameterDefaultDefaultExpression(SemanticModel semanticModel, ParameterSyntax parameterSyntax)
      => semanticModel.GetTypeInfo(parameterSyntax.Type!).Type!.IsReferenceType
         && parameterSyntax.Default is { Value: DefaultExpressionSyntax _ };
  }
}
