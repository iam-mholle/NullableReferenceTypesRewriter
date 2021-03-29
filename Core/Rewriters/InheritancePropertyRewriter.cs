// Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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
