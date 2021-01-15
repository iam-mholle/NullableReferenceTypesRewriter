using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class InheritanceParameterRewriter : RewriterBase
  {
    public InheritanceParameterRewriter(Action<RewriterBase, IReadOnlyCollection<IRewritable>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var inheritors = CurrentMethod.Children
          .Where(d => d.DependencyType == DependencyType.Inheritance)
          .Select(d => d.To)
          .OfType<Method>();

      var res = node.ParameterList.Parameters
          .Select((_, i) => i)
          .Where(i => inheritors.Any(m => IsParameterNullable(m, i)))
          .ToArray();

      var newList = node.ParameterList;

      foreach (var parameterIndex in res)
      {
        var existingParameter = newList.Parameters[parameterIndex];
        newList = newList.ReplaceNode(
            existingParameter,
            existingParameter.WithType (NullUtilities.ToNullable (existingParameter.Type!)));
      }

      return node.WithParameterList(newList);
    }

    public bool IsParameterNullable(Method method, int argumentIndex)
    {
      var syntax = method.MethodDeclaration;

      if (syntax.ParameterList.Parameters.Count == 0)
        return false;

      return syntax.ParameterList.Parameters[argumentIndex].Type is NullableTypeSyntax;
    }
  }
}
