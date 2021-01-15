﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
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
        newList = newList.ReplaceNode(
            existingParameter,
            existingParameter.WithType (NullUtilities.ToNullable (existingParameter.Type!)));
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
  }
}