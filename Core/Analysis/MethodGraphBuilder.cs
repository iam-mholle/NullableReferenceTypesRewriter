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

    public override void VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var symbol = GetSemanticModel(node).GetDeclaredSymbol (node);

      if (symbol == null) throw new InvalidOperationException();

      _graph.AddMethod(UniqueSymbolNameGenerator.Generate(symbol), symbol);

      if (TryGetInterfaceMethods(symbol, out var interfaceMethods))
      {
        foreach (var interfaceMethod in interfaceMethods)
        {
          _graph.AddDependency(
              UniqueSymbolNameGenerator.Generate(interfaceMethod),
              UniqueSymbolNameGenerator.Generate(symbol));
        }
      }

      if (TryGetOverriddenMethod(symbol, out var overriddenMethod))
      {
        _graph.AddDependency (
            UniqueSymbolNameGenerator.Generate (overriddenMethod!),
            UniqueSymbolNameGenerator.Generate (symbol));
      }

      base.VisitMethodDeclaration (node);
    }

    private bool TryGetInterfaceMethods(IMethodSymbol method, out IReadOnlyCollection<IMethodSymbol> interfaceMethods)
    {
      var methodsOfContainingType = method.ContainingType.AllInterfaces;
      interfaceMethods = methodsOfContainingType
          .SelectMany(@interface => @interface.GetMembers().OfType<IMethodSymbol>())
          .Where(interfaceMethod => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod), method))
          .ToArray();

      return interfaceMethods.Any();
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

    private SemanticModel GetSemanticModel (SyntaxNode node)
    {
      return _compilation.GetSemanticModel (node.SyntaxTree);
    }

    public override void VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax> ();

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
            UniqueSymbolNameGenerator.Generate (invokedMethodSymbol));
      }

      base.VisitInvocationExpression (node);
    }

    public override void VisitIdentifierName (IdentifierNameSyntax node)
    {
      var containingMethodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();

      if (containingMethodDeclaration != null)
      {
        var containingMethodSymbol = GetSemanticModel(node).GetDeclaredSymbol (containingMethodDeclaration);

        if (containingMethodSymbol == null) throw new InvalidOperationException();

        var symbolInfoCandidate = GetSemanticModel (node).GetSymbolInfo (node);
        if (symbolInfoCandidate.Symbol is IFieldSymbol fieldSymbol)
        {
          _graph.AddDependency(
              UniqueSymbolNameGenerator.Generate(containingMethodSymbol),
              UniqueSymbolNameGenerator.Generate(fieldSymbol));
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
  }
}