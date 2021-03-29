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

namespace NullableReferenceTypesRewriter.Analysis
{
  public class MethodGraphBuilder : CSharpSyntaxWalker
  {
    private readonly SharedCompilation _compilation;
    private readonly MethodGraph _graph;

    public IMethodGraph Graph => _graph;

    public MethodGraphBuilder(SharedCompilation compilation)
    {
      _compilation = compilation;
      _graph = new MethodGraph(compilation);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      var symbol = GetSemanticModel(node).GetDeclaredSymbol (node);

      if (symbol == null) throw new InvalidOperationException();

      _graph.AddMethod(UniqueSymbolNameGenerator.Generate(symbol), symbol);

      base.VisitConstructorDeclaration(node);
    }

    public override void VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var symbol = GetSemanticModel(node).GetDeclaredSymbol (node);

      if (symbol == null) throw new InvalidOperationException();

      _graph.AddMethod(UniqueSymbolNameGenerator.Generate(symbol), symbol);

      if (TryGetInterfaceMembers<IMethodSymbol>(symbol, out var interfaceMethods))
      {
        foreach (var interfaceMethod in interfaceMethods)
        {
          _graph.AddDependency(
              UniqueSymbolNameGenerator.Generate(interfaceMethod),
              UniqueSymbolNameGenerator.Generate(symbol),
              DependencyType.Inheritance);
        }
      }

      if (TryGetOverriddenMethod(symbol, out var overriddenMethod))
      {
        _graph.AddDependency (
            UniqueSymbolNameGenerator.Generate (overriddenMethod!),
            UniqueSymbolNameGenerator.Generate (symbol),
            DependencyType.Inheritance);
      }

      base.VisitMethodDeclaration (node);
    }

    private bool TryGetInterfaceMembers<TSymbol>(ISymbol method, out IReadOnlyCollection<TSymbol> interfaceMembers)
        where TSymbol : ISymbol
    {
      var methodsOfContainingType = method.ContainingType.AllInterfaces;
      interfaceMembers = methodsOfContainingType
          .SelectMany(@interface => @interface.GetMembers().OfType<TSymbol>())
          .Where(interfaceMember => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(interfaceMember), method))
          .ToArray();

      return interfaceMembers.Any();
    }

    private bool TryGetOverriddenMethod (IMethodSymbol method, out IMethodSymbol? overriddenMethod)
    {
      if (method.IsOverride)
      {
        overriddenMethod = method.OverriddenMethod;
        return true;
      }

      overriddenMethod = null;
      return false;
    }

    private bool TryGetOverriddenProperty (IPropertySymbol property, out IPropertySymbol? overriddenProperty)
    {
      if (property.IsOverride)
      {
        overriddenProperty = property.OverriddenProperty;
        return true;
      }

      overriddenProperty = null;
      return false;
    }

    private SemanticModel GetSemanticModel (SyntaxNode node)
    {
      return _compilation.GetSemanticModel (node.SyntaxTree);
    }

    public override void VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax> ();

      var symbolInfoCandidate = GetSemanticModel(node).GetSymbolInfo (node.Expression);
      if (symbolInfoCandidate.Symbol is IMethodSymbol invokedMethodSymbol && containingMethodDeclaration != null)
      {
        var containingMethodSymbol = GetSemanticModel(node).GetDeclaredSymbol (containingMethodDeclaration);

        if (containingMethodSymbol == null) throw new InvalidOperationException();

        if (invokedMethodSymbol.DeclaringSyntaxReferences.IsEmpty)
        {
          _graph.AddExternalMethod (UniqueSymbolNameGenerator.Generate (invokedMethodSymbol), invokedMethodSymbol);
        }

        _graph.AddDependency (
            UniqueSymbolNameGenerator.Generate (containingMethodSymbol),
            UniqueSymbolNameGenerator.Generate (invokedMethodSymbol),
            DependencyType.Usage);
      }

      base.VisitInvocationExpression (node);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax> ();

      var symbolInfoCandidate = GetSemanticModel(node).GetSymbolInfo (node);
      if (symbolInfoCandidate.Symbol is IMethodSymbol invokedCtorSymbol && containingMethodDeclaration != null)
      {
        var containingMethodSymbol = GetSemanticModel(node).GetDeclaredSymbol (containingMethodDeclaration);

        if (containingMethodSymbol == null) throw new InvalidOperationException();

        if (invokedCtorSymbol.DeclaringSyntaxReferences.IsEmpty)
        {
          _graph.AddExternalMethod (UniqueSymbolNameGenerator.Generate (invokedCtorSymbol), invokedCtorSymbol);
        }

        _graph.AddDependency (
            UniqueSymbolNameGenerator.Generate (containingMethodSymbol),
            UniqueSymbolNameGenerator.Generate (invokedCtorSymbol),
            DependencyType.Usage);
      }

      base.VisitObjectCreationExpression(node);
    }

    public override void VisitIdentifierName (IdentifierNameSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
      var containingPropertyDeclaration = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
      var containingMethodSymbol = containingMethodDeclaration != null ? GetSemanticModel(node).GetDeclaredSymbol (containingMethodDeclaration) : null;
      var containingPropertySymbol = containingPropertyDeclaration != null ? GetSemanticModel(node).GetDeclaredSymbol(containingPropertyDeclaration) : null;

      var containingSymbolUniqueName = (containingMethodDeclaration != null, containingPropertyDeclaration != null) switch
      {
          (true, false) => UniqueSymbolNameGenerator.Generate(containingMethodSymbol!),
          (false, true) => UniqueSymbolNameGenerator.Generate(containingPropertySymbol!),
          _ => null,
      };

      if (containingSymbolUniqueName != null)
      {
        var symbolInfoCandidate = GetSemanticModel (node).GetSymbolInfo (node);
        if (symbolInfoCandidate.Symbol is IFieldSymbol fieldSymbol)
        {
          _graph.AddDependency(
              containingSymbolUniqueName,
              UniqueSymbolNameGenerator.Generate(fieldSymbol),
              DependencyType.Usage);
        }
        else if (symbolInfoCandidate.Symbol is IPropertySymbol propertySymbol)
        {
          _graph.AddDependency(
              containingSymbolUniqueName,
              UniqueSymbolNameGenerator.Generate(propertySymbol),
              DependencyType.Usage);
        }
        else if (symbolInfoCandidate.Symbol is IEventSymbol eventSymbol)
        {
          _graph.AddDependency(
              containingSymbolUniqueName,
              UniqueSymbolNameGenerator.Generate(eventSymbol),
              DependencyType.Usage);
        }
      }

      base.VisitIdentifierName (node);
    }

    public override void VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      foreach (var declaredField in node.Declaration.Variables)
      {
        var symbol = (IFieldSymbol?) GetSemanticModel(node).GetDeclaredSymbol (declaredField);

        if (symbol == null) throw new InvalidOperationException();

        // TODO: Do I want to pass in node here?
        _graph.AddField (UniqueSymbolNameGenerator.Generate (symbol), symbol);
      }

      base.VisitFieldDeclaration (node);
    }

    public override void VisitEventDeclaration(EventDeclarationSyntax node)
    {
      var symbol = GetSemanticModel(node).GetDeclaredSymbol(node);

      if (symbol == null) throw new InvalidOperationException();

      _graph.AddEvent(UniqueSymbolNameGenerator.Generate(symbol), symbol);

      base.VisitEventDeclaration(node);
    }

    public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
      foreach (var eventFieldVariable in node.Declaration.Variables)
      {
        var symbol = (IEventSymbol?) GetSemanticModel(node).GetDeclaredSymbol(eventFieldVariable);

        if (symbol == null) throw new InvalidOperationException();

        _graph.AddEvent(UniqueSymbolNameGenerator.Generate(symbol), symbol);
      }

      base.VisitEventFieldDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      var symbol = GetSemanticModel(node).GetDeclaredSymbol (node);

      if (symbol == null) throw new InvalidOperationException();

      _graph.AddProperty(UniqueSymbolNameGenerator.Generate(symbol), symbol);

      if (TryGetInterfaceMembers<IPropertySymbol>(symbol, out var interfaceMethods))
      {
        foreach (var interfaceMethod in interfaceMethods)
        {
          _graph.AddDependency(
              UniqueSymbolNameGenerator.Generate(interfaceMethod),
              UniqueSymbolNameGenerator.Generate(symbol),
              DependencyType.Inheritance);
        }
      }

      if (TryGetOverriddenProperty(symbol, out var overriddenMethod))
      {
        _graph.AddDependency (
            UniqueSymbolNameGenerator.Generate (overriddenMethod!),
            UniqueSymbolNameGenerator.Generate (symbol),
            DependencyType.Inheritance);
      }

      base.VisitPropertyDeclaration(node);
    }
  }
}