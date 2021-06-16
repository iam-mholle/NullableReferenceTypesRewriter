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
  public class ExternalAnnotatedSymbolRewriter : RewriterBase
  {
    public ExternalAnnotatedSymbolRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites) : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      var methodSymbol = (IMethodSymbol) SemanticModel.GetDeclaredSymbol(node)!;

      var interfaceMethods = GetImplementedMethods(methodSymbol);

      if (interfaceMethods.Count > 1)
      {
        if (!interfaceMethods.Skip(1).All(i => HasEquivalentNullability(interfaceMethods.First(), i)))
        {
          // log warning
          return base.VisitMethodDeclaration(node);
        }
      }

      var overridden = methodSymbol.OverriddenMethod;
      var implementedMethod = interfaceMethods.FirstOrDefault();

      if (overridden is {})
      {
        if (HasEquivalentNullability(overridden, methodSymbol))
        {
          return base.VisitMethodDeclaration(node);
        }
        else
        {
          return AdaptNullabilityFromSymbol(node, overridden);
        }
      }


      if (implementedMethod is {})
      {
        if (HasEquivalentNullability(implementedMethod, methodSymbol))
        {
          return base.VisitMethodDeclaration(node);
        }
        else
        {
          return AdaptNullabilityFromSymbol(node, implementedMethod);
        }
      }

      return base.VisitMethodDeclaration(node);
    }

    private MethodDeclarationSyntax AdaptNullabilityFromSymbol(MethodDeclarationSyntax input, IMethodSymbol symbol)
    {
      var result = input;

      var isSyntaxReturnTypeNullable = IsSyntaxNullable(input.ReturnType, SemanticModel);
      var isSymbolReturnTypeNullable = IsSymbolNullable(symbol.ReturnType);

      if (isSymbolReturnTypeNullable && !isSyntaxReturnTypeNullable)
      {
        result = result.WithReturnType(NullUtilities.ToNullable(result.ReturnType));
      }

      var syntaxParameterNullability = input.ParameterList.Parameters.Select(p => IsSyntaxNullable(p.Type!, SemanticModel));
      var symbolParameterNullability = symbol.Parameters.Select(p => IsSymbolNullable(p.Type));

      var nullabilityDifferences = syntaxParameterNullability.Zip(
              symbolParameterNullability,
              (syn, sym) => sym && !syn)
          .ToArray();

      if (nullabilityDifferences.Any(d => d))
      {
        var indexes = nullabilityDifferences
            .Select((b, i) => (b, i))
            .Where(p => p.Item1)
            .Select(p => p.Item2);
        foreach (var index in indexes)
        {
          var parameter = result.ParameterList.Parameters[index];
          result = result.ReplaceNode(parameter, parameter.WithType(NullUtilities.ToNullable(parameter.Type!)));
        }
      }

      return result;

      static bool IsSyntaxNullable(TypeSyntax syntax, SemanticModel model) => syntax.IsReferenceType(model) && syntax is NullableTypeSyntax;
      static bool IsSymbolNullable(ITypeSymbol symbol) => symbol.NullableAnnotation == NullableAnnotation.Annotated;
    }

    private static bool HasEquivalentNullability(IMethodSymbol a, IMethodSymbol b)
    {
      var equivalentReturnTypeNullability = !a.ReturnsVoid && HasEquivalentTypeNullability(a.ReturnType, b.ReturnType);
      var equivalentParameterNullability = a.Parameters.Zip(b.Parameters, (p1, p2) => HasEquivalentTypeNullability(p1.Type, p2.Type)).All(b => b);

      return equivalentParameterNullability && equivalentReturnTypeNullability;

      static bool HasEquivalentTypeNullability(ITypeSymbol t1, ITypeSymbol t2) =>
          t1.IsReferenceType
          && t2.IsReferenceType
          && t1.NullableAnnotation == t2.NullableAnnotation;
    }

    private static IReadOnlyCollection<IMethodSymbol> GetImplementedMethods(IMethodSymbol method)
    {
      return method
          .ContainingType
          .AllInterfaces
          .SelectMany(@interface =>
              @interface.GetMembers()
                  .OfType<IMethodSymbol>())
          .Where(interfaceMethod =>
              SymbolEqualityComparer.Default.Equals(
                  method.ContainingType?.FindImplementationForInterfaceMember(interfaceMethod),
                  method))
          .ToArray();
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(INode node)
    {
      return base.GetAdditionalRewrites(node)
          .Concat(
              node.Children
                  .Select(p => p.To)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ParameterChange | RewriteCapability.ReturnValueChange)))
          .Concat(
              node.Parents
                  .Select(p => p.From)
                  .OfType<IRewritable>()
                  .Select(r => (r, RewriteCapability.ParameterChange | RewriteCapability.ReturnValueChange)))
          .ToArray();
    }
  }
}
