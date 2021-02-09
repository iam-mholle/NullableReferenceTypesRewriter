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
  // TODO: Does not work with .ctor calls
  // TODO: Somehow doesn't work sometimes: [CanBeNull] IFluentScreenshotElement parent = null
  public class MethodArgumentRewriter : RewriterBase
  {
    public MethodArgumentRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      var symbol = ModelExtensions.GetDeclaredSymbol(CurrentMethod.SemanticModel, node);
      var parents = CurrentMethod.Parents.Select(p => p.From).ToArray();
      var parametersToAnnotate = new List<int>();
      for (var i = 0; i < node.ParameterList.Parameters.Count; i++)
      {
        foreach (var parent in parents)
        {
          if (parent is Method parentMethod)
          {
            var isArgumentPossiblyNull = IsArgumentPossiblyNull(symbol!, parentMethod, i);
            if (isArgumentPossiblyNull)
            {
              parametersToAnnotate.Add(i);
            }
          }
        }
      }

      if (parametersToAnnotate.Count == 0)
        return node;

      var newList = node.ParameterList;

      foreach (var parameterIndex in parametersToAnnotate)
      {
        var existingParameter = newList.Parameters[parameterIndex];

        if (existingParameter.HasNotNullAttribute())
        {
          Console.WriteLine($"ERROR: Trying to annotate NotNull parameter {existingParameter.ToString()} in {CurrentMethod}");
        }
        else
        {
          newList = newList.ReplaceNode(
              existingParameter,
              existingParameter.WithType(NullUtilities.ToNullable(existingParameter.Type!)));
        }
      }

      return node.WithParameterList(newList);
    }

    public bool IsArgumentPossiblyNull(ISymbol symbol, Method method, int argumentIndex)
    {
      var syntax = method.MethodDeclaration;

      if (syntax.Body is null)
        return false;

      var invocations = syntax.Body.DescendantNodes()
          .Where(n => n.IsKind(SyntaxKind.InvocationExpression))
          .Cast<InvocationExpressionSyntax>()
          .Where(i => SymbolEqualityComparer.Default.Equals(method.SemanticModel.GetSymbolInfo(i.Expression).Symbol, symbol))
          .ToArray();

      return invocations.Any(i =>
      {
        if (i.ArgumentList.Arguments.Count <= argumentIndex)
        {
          return false;
        }

        return NullUtilities.CanBeNull (i.ArgumentList.Arguments[argumentIndex].Expression, method.SemanticModel);
      });
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode method)
    {
      return method.Parents
          .Select(p => p.From)
          .OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ParameterChange))
          .ToArray();
    }
  }
}
