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
  public class CanBeNullRewriter : RewriterBase
  {
    public CanBeNullRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      var resultNode = node;

      if (node.HasCanBeNullAttribute())
      {
        resultNode = NullUtilities.ToNullReturning(SemanticModel, node);
      }

      var resultParameterList = node.ParameterList;

      foreach (var (parameter, index) in node.ParameterList.Parameters.Select((p, i) => (p, i)))
      {
        if (parameter.Type is {} && parameter.Type.IsValueType(SemanticModel))
          continue;

        if (parameter.HasCanBeNullAttribute())
        {
          resultParameterList = resultParameterList.ReplaceNode(resultParameterList.Parameters[index].Type!, NullUtilities.ToNullableWithGenericsCheck(SemanticModel, node, parameter.Type!));
        }
      }

      if (resultParameterList != node.ParameterList)
      {
        resultNode = resultNode.WithParameterList(resultParameterList);
      }

      return resultNode;
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      var resultParameterList = node.ParameterList;

      foreach (var (parameter, index) in node.ParameterList.Parameters.Select((p, i) => (p, i)))
      {
        if (parameter.Type is {} && parameter.Type.IsValueType(SemanticModel))
          continue;

        if (parameter.HasCanBeNullAttribute())
        {
          resultParameterList = resultParameterList.ReplaceNode(resultParameterList.Parameters[index].Type!, NullUtilities.ToNullableWithGenericsCheck(SemanticModel, node, parameter.Type!));
        }
      }

      if (resultParameterList != node.ParameterList)
      {
        return node.WithParameterList(resultParameterList);
      }

      return node;
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (node.HasCanBeNullAttribute())
        return node.WithType(NullUtilities.ToNullable(node.Type));

      return node;
    }

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
      if (node.HasCanBeNullAttribute())
        return node.WithDeclaration(node.Declaration.WithType(NullUtilities.ToNullable(node.Declaration.Type)));

      return node;
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode node)
    {
      var children = node.Children.Select(d => d.To);
      var parents = node.Parents.Select(d => d.From);
      return base.GetAdditionalRewrites(node)
          .Concat(
              children.Concat(parents)
                  .OfType<IRewritable>()
                  .Select(n => (n, RewriteCapability.ParameterChange | RewriteCapability.ReturnValueChange)))
          .ToArray();
    }
  }
}
