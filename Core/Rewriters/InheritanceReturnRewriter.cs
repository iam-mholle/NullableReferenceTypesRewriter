using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class InheritanceReturnRewriter : RewriterBase
  {
    public InheritanceReturnRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      if (node.ParameterList.Parameters.Count == 0)
        return node;

      var isAnyChildNullable = CurrentMethod.Children
          .Where(d => d.DependencyType == DependencyType.Inheritance)
          .Select(d => d.To)
          .OfType<Method>()
          .Any(IsReturnTypeNullable);

      if (isAnyChildNullable)
      {
        return node.WithReturnType(NullUtilities.ToNullable(node.ReturnType));
      }

      return node;
    }

    private bool IsReturnTypeNullable(Method method)
    {
      var syntax = method.MethodDeclaration;

      return syntax is MethodDeclarationSyntax { ReturnType: NullableTypeSyntax _ };
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
