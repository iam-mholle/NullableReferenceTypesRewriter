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
  // TODO: InheritancePropertyRewriter (downward propagation)
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
      var semanticModel = CurrentProperty.SemanticModel;

      if (IsValueType(semanticModel, node))
        return node;

      if (IsAbstract(node))
        return node;

      if (IsNullable(semanticModel, node))
        return node;

      if (IsInitializedToNotNull(semanticModel, node))
        return node;

      if (IsInitializedToNull(semanticModel, node))
        return node.WithType(NullUtilities.ToNullable(node.Type));

      if (node.Ancestors().FirstOrDefault(a => a.IsKind(SyntaxKind.InterfaceDeclaration)) != null)
        return node;

      var classSyntax = (ClassDeclarationSyntax) node.Ancestors().First(a => a.IsKind(SyntaxKind.ClassDeclaration));
      var constructors = classSyntax.ChildNodes()
          .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
          .Cast<ConstructorDeclarationSyntax>()
          .ToArray();

      var isInitializedToNotNull = constructors.All(c => PropertyInitializedToNotNullInCtorChain(semanticModel, c, node));

      if (IsAutoProperty(node) && (constructors.Length == 0 || !isInitializedToNotNull))
        return node.WithType(NullUtilities.ToNullable(node.Type));

      return base.VisitPropertyDeclaration(node);
    }

    private bool PropertyInitializedToNotNullInCtorChain(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, PropertyDeclarationSyntax property)
    {
      var isInitializedToNotNullInCurrent = PropertyInitializedToNotNullInCtor(semanticModel, constructor, property);

      if (isInitializedToNotNullInCurrent)
        return true;

      if (constructor.Initializer is null)
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

    private bool IsValueType(SemanticModel semanticModel, PropertyDeclarationSyntax declaration)
      => IsValueType(semanticModel, declaration.Type);

    private bool IsValueType (SemanticModel semanticModel, TypeSyntax declaration)
    {
      if (declaration is ArrayTypeSyntax)
        return false;

      var typeSymbol = semanticModel.GetTypeInfo (declaration).Type as INamedTypeSymbol;

      return typeSymbol == null || (typeSymbol.IsValueType);
    }

    private bool IsNullable(SemanticModel semanticModel, PropertyDeclarationSyntax syntax)
      => semanticModel.GetDeclaredSymbol(syntax)?.Type.NullableAnnotation == NullableAnnotation.Annotated;

    private bool IsAbstract(PropertyDeclarationSyntax syntax)
      => syntax.Modifiers.Any(SyntaxKind.AbstractKeyword);

    private bool IsInitializedToNull (SemanticModel semanticModel, PropertyDeclarationSyntax propertyDeclaration)
    {
      if (propertyDeclaration.Initializer != null)
      {
        return NullUtilities.CanBeNull (propertyDeclaration.Initializer.Value, semanticModel);
      }

      return false;
    }

    private bool IsInitializedToNotNull (SemanticModel semanticModel, PropertyDeclarationSyntax propertyDeclaration)
    {
      if (propertyDeclaration.Initializer != null)
      {
        return !NullUtilities.CanBeNull (propertyDeclaration.Initializer.Value, semanticModel);
      }

      return false;
    }

    private bool IsAutoProperty(PropertyDeclarationSyntax propertyDeclarationSyntax)
    {
      return propertyDeclarationSyntax.AccessorList?.Accessors.All(a => a.Body is null) ?? false;
    }
  }
}
