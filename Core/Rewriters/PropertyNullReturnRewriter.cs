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
  /// <summary>
  /// Specs:<br/>
  /// - Expression-bodied properties returning a nullable value are rewritten to a nullable reference type<br/>
  /// - Non-expression-bodied, non-auto-properties returning a nullable value are rewritten to a nullable reference type
  /// </summary>
  public class PropertyNullReturnRewriter : RewriterBase
  {
    public PropertyNullReturnRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (node.IsExpressionBodied() && NullUtilities.CanBeNull(node.ExpressionBody!.Expression, SemanticModel))
      {
        var containingClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        return node.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingClass!, node.Type));
      }

      if (node.HasNonAutoGetter())
      {
        var getter = node.AccessorList!.Accessors.Single(a => a.Keyword.IsKind(SyntaxKind.GetKeyword));

        var hasNullReturningExpressionBody = getter.ExpressionBody != null && NullUtilities.CanBeNull(getter.ExpressionBody.Expression, SemanticModel);
        var hasNullReturningStatementBody = getter.Body != null && NullUtilities.ReturnsNull(getter.Body.Statements, SemanticModel);
        var isNullReturning = hasNullReturningExpressionBody || hasNullReturningStatementBody;

        if (isNullReturning)
        {
          var containingClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
          if (containingClass != null)
          {
            return node.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, containingClass, node.Type));
          }
        }
      }

      return base.VisitPropertyDeclaration(node);
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites (INode method)
    {
      return base.GetAdditionalRewrites(method)
          .Concat(
              method.Parents
                  .Select(p => p.From)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ReturnValueChange)))
          .ToArray();
    }
  }
}
