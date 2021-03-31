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
  public class MethodArgumentRewriter : RewriterBase
  {
    public MethodArgumentRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      return VisitBaseMethodDeclaration(node, false);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      return VisitBaseMethodDeclaration(node, true);
    }

    public SyntaxNode? VisitBaseMethodDeclaration(BaseMethodDeclarationSyntax node, bool isCtor)
    {
      var symbol = (IMethodSymbol) ModelExtensions.GetDeclaredSymbol(SemanticModel, node)!;
      var parents = CurrentNode.Parents.Select(p => p.From).ToArray();
      var parametersToAnnotate = new List<int>();
      for (var i = 0; i < node.ParameterList.Parameters.Count; i++)
      {
        var parameter = node.ParameterList.Parameters[i];

        if (parameter.Type!.IsValueType(SemanticModel))
          continue;

        foreach (var parent in parents)
        {
          if (parent is Method parentMethod)
          {
            var isArgumentPossiblyNull = IsArgumentPossiblyNull(symbol, isCtor, parentMethod, i);
            if (isArgumentPossiblyNull)
            {
              parametersToAnnotate.Add(i);
            }
          }
        }
      }

      if (parametersToAnnotate.Count == 0)
        return node;

      var newList = node.ParameterList;

      foreach (var parameterIndex in parametersToAnnotate)
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
              existingParameter.WithType(NullUtilities.ToNullableWithGenericsCheck(SemanticModel, node, existingParameter.Type!)));
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

    private bool IsArgumentPossiblyNull(IMethodSymbol symbol, bool isCtor, Method method, int argumentIndex)
    {
      var syntax = method.MethodDeclaration;

      if (syntax.Body is null)
        return false;

      return isCtor switch
      {
          false => IsInvocationArgumentNullable(symbol, method, syntax, argumentIndex),
          true => IsObjectCreationArgumentNullable(symbol, method, syntax, argumentIndex),
      };
    }

    private bool IsInvocationArgumentNullable(ISymbol symbol, Method method, BaseMethodDeclarationSyntax syntax, int argumentIndex)
    {
      var invocations = syntax.Body!.DescendantNodes()
          .Where(n => n.IsKind(SyntaxKind.InvocationExpression))
          .Cast<InvocationExpressionSyntax>()
          .Where(i => SymbolEqualityComparer.Default.Equals(method.SemanticModel.GetSymbolInfo(i.Expression).Symbol, symbol))
          .ToArray();

      return invocations.Any(i =>
      {
        if (i.ArgumentList.Arguments.Count <= argumentIndex)
        {
          return false;
        }

        return NullUtilities.CanBeNull (i.ArgumentList.Arguments[argumentIndex].Expression, method.SemanticModel);
      });
    }

    private bool IsObjectCreationArgumentNullable(ISymbol symbol, Method method, BaseMethodDeclarationSyntax syntax, int argumentIndex)
    {
      var creations = syntax.Body!.DescendantNodes()
          .Where(n => n.IsKind(SyntaxKind.ObjectCreationExpression))
          .Cast<ObjectCreationExpressionSyntax>()
          .Where(i => SymbolEqualityComparer.Default.Equals(method.SemanticModel.GetSymbolInfo(i).Symbol, symbol))
          .ToArray();

      return creations.Any(i =>
      {
        if (i.ArgumentList is null || i.ArgumentList.Arguments.Count <= argumentIndex)
        {
          return false;
        }

        return NullUtilities.CanBeNull (i.ArgumentList.Arguments[argumentIndex].Expression, method.SemanticModel);
      });
    }
  }
}
