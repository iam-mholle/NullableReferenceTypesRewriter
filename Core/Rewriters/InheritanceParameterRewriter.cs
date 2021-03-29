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
