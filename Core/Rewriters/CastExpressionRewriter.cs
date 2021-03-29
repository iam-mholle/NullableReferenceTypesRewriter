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
  public class CastExpressionRewriter : RewriterBase
  {
    public CastExpressionRewriter (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitCastExpression (CastExpressionSyntax node)
    {
      var possiblyRewrittenNode = (CastExpressionSyntax) base.VisitCastExpression(node)!;

      if (possiblyRewrittenNode != node
          && possiblyRewrittenNode.DescendantNodes().Any(n => n.IsKind(SyntaxKind.CastExpression)))
      {
        // Recompilation needed.
        AddDeferredRewrite((IRewritable)CurrentNode, RewriteCapability.ReturnValueChange);
        return possiblyRewrittenNode;
      }

      var type = node.Type;
      if (type is NullableTypeSyntax)
        return node;

      return NullUtilities.CanBeNull (node.Expression, SemanticModel)
          ? node.WithType (ToNullableWithFittingContext (type))
          : node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode node)
    {
      return base.GetAdditionalRewrites(node)
          .Concat(node.Parents.Select(p => p.From).OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ReturnValueChange)))
          .Concat(node.Children
              .Select(c => c.To)
              .OfType<IRewritable>()
              .Select(r => (r, RewriteCapability.ParameterChange)))
          .ToArray();
    }

    private TypeSyntax ToNullableWithFittingContext(TypeSyntax typeSyntax)
    {
      Func<TypeSyntax>? annotator = null;

      var containingMethod = typeSyntax.Ancestors()
          .Where(a => a.IsKind(SyntaxKind.MethodDeclaration))
          .Cast<MethodDeclarationSyntax>()
          .SingleOrDefault();

      if (containingMethod is null)
      {
        var containingClass = typeSyntax.Ancestors()
            .Where(a => a.IsKind(SyntaxKind.ClassDeclaration))
            .Cast<ClassDeclarationSyntax>()
            .First();

        annotator = () => NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingClass, typeSyntax);
      }
      else
      {
        annotator = () => NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingMethod, typeSyntax);
      }

      return annotator();
    }
  }
}
