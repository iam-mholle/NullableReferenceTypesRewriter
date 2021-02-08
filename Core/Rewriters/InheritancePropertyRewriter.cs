using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Rewriters
{
  public class InheritancePropertyRewriter : RewriterBase
  {
    public InheritancePropertyRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (node.Type is NullableTypeSyntax)
        return node;

      var current = CurrentProperty;

      if (current.Children
          .Where(c => c.DependencyType == DependencyType.Inheritance)
          .Select(c => c.To)
          .OfType<Property>()
          .Any(p => p.PropertyDeclarationSyntax.Type is NullableTypeSyntax))
      {
        return node.WithType(NullUtilities.ToNullable(node.Type));
      }

      return node;
    }
  }
}
