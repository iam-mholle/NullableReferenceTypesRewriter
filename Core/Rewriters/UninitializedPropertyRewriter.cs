﻿// Copyright (c) rubicon IT GmbH
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
  /// - Value-type properties are ignored<br/>
  /// - Abstract properties are ignored<br/>
  /// - Nullable reference type properties are ignored<br/>
  /// - Properties declared in an interface are ignored<br/>
  /// - Properties initialized to non-null values are ignored<br/>
  /// - Properties initialized to nullable values are rewritten to return a nullable reference type<br/>
  /// - Uninitialized auto-properties are rewritten to return a nullable reference type
  /// </summary>
  public class UninitializedPropertyRewriter : RewriterBase
  {
    public UninitializedPropertyRewriter(Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (node.Type.IsValueType(SemanticModel))
        return node;

      if (node.IsAbstract())
        return node;

      if (IsNullable(SemanticModel, node))
        return node;

      if (node.IsInitializedToNotNull(SemanticModel))
        return node;

      if (node.IsInitializedToNull(SemanticModel))
        return node.WithType(NullUtilities.ToNullable(node.Type));

      if (node.Ancestors().FirstOrDefault(a => a.IsKind(SyntaxKind.InterfaceDeclaration)) != null)
        return node;

      var classSyntax = (TypeDeclarationSyntax) node.Ancestors().First(a => a.IsKind(SyntaxKind.ClassDeclaration) || a.IsKind(SyntaxKind.StructDeclaration));
      var constructors = classSyntax.ChildNodes()
          .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
          .Cast<ConstructorDeclarationSyntax>()
          .ToArray();

      var isInitializedToNotNull = constructors.All(c => PropertyInitializedToNotNullInCtorChain(SemanticModel, c, node));

      if (node.IsAutoProperty() && (constructors.Length == 0 || !isInitializedToNotNull))
        return node.WithType(NullUtilities.ToNullable(node.Type));

      return base.VisitPropertyDeclaration(node);
    }

    private bool PropertyInitializedToNotNullInCtorChain(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, PropertyDeclarationSyntax property)
    {
      var isInitializedToNotNullInCurrent = PropertyInitializedToNotNullInCtor(semanticModel, constructor, property);

      if (isInitializedToNotNullInCurrent)
        return true;

      if (constructor.Initializer is null || constructor.Initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword))
        return false;

      var thisConstructorSymbol = semanticModel.GetSymbolInfo(constructor.Initializer).Symbol ?? throw new InvalidOperationException();

      if (thisConstructorSymbol.DeclaringSyntaxReferences.IsEmpty)
        return false;

      var thisConstructorSyntax = (ConstructorDeclarationSyntax) thisConstructorSymbol.DeclaringSyntaxReferences.First().GetSyntax();

      return PropertyInitializedToNotNullInCtorChain(semanticModel, thisConstructorSyntax, property);
    }

    private bool PropertyInitializedToNotNullInCtor(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, PropertyDeclarationSyntax property)
    {
      var assignments = constructor.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().ToArray();

      var propertySymbol = semanticModel.GetDeclaredSymbol(property);
      var propertyAssignments = assignments
          .Where(a =>
              semanticModel.SyntaxTree.GetRoot().Contains(a.Left)
              && SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(a.Left).Symbol, propertySymbol))
          .ToArray();

      if (propertyAssignments.Length == 0)
      {
        return false;
      }

      return propertyAssignments.All(a => !NullUtilities.CanBeNull(a.Right, semanticModel));
    }

    private bool IsNullable(SemanticModel semanticModel, PropertyDeclarationSyntax syntax)
      => semanticModel.GetDeclaredSymbol(syntax)?.Type.NullableAnnotation == NullableAnnotation.Annotated;
  }
}
