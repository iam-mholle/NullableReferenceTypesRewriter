using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class InheritanceParameterRewriter : RewriterBase
  {
    public InheritanceParameterRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var parents = CurrentNode.Parents
          .Where(d => d.DependencyType == DependencyType.Inheritance)
          .Select(d => d.From)
          .OfType<Method>();

      var res = node.ParameterList.Parameters
          .Select((_, i) => i)
          .Where(i => parents.Any(m => IsParameterNullable(m, i)))
          .ToArray();

      var newList = node.ParameterList;

      foreach (var parameterIndex in res)
      {
        var existingParameter = newList.Parameters[parameterIndex];

        if (existingParameter.HasNotNullAttribute())
        {
          Console.WriteLine($"ERROR: Trying to annotate NotNull parameter {existingParameter.ToString()} in {CurrentNode}");
        }
        else
        {
          newList = newList.ReplaceNode(
              existingParameter,
              existingParameter.WithType (NullUtilities.ToNullableWithGenericsCheck (SemanticModel, node, existingParameter.Type!)));
        }
      }

      return node.WithParameterList(newList);
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode method)
    {
      return base.GetAdditionalRewrites(method)
          .Concat(
              method.Children
                  .Select(p => p.To)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ParameterChange)))
          .ToArray();
    }

    private bool IsParameterNullable(Method method, int argumentIndex)
    {
      var syntax = method.MethodDeclaration;

      if (syntax.ParameterList.Parameters.Count == 0)
        return false;

      return syntax.ParameterList.Parameters[argumentIndex].Type is NullableTypeSyntax;
    }
  }
}
