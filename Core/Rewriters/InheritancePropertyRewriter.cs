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

      if (CurrentNode.Children
          .Where(c => c.DependencyType == DependencyType.Inheritance)
          .Select(c => c.To)
          .OfType<Property>()
          .Any(p => p.PropertyDeclarationSyntax.Type is NullableTypeSyntax))
      {
        var propertyContainer = node.Ancestors().FirstOrDefault(a => a.IsKind(SyntaxKind.ClassDeclaration) || a.IsKind(SyntaxKind.InterfaceDeclaration)) as TypeDeclarationSyntax;

        if (propertyContainer is null)
          return node;

        return node.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, propertyContainer, node.Type));
      }

      return node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode node)
    {
      return base.GetAdditionalRewrites(node)
          .Concat(
              node.Parents
                  .Select(p => p.From)
                  .OfType<IRewritable>()
                  .Select(p => (p, RewriteCapability.ReturnValueChange)))
          .ToList();
    }
  }
}
