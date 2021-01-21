using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
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

      if (IsNullable(semanticModel, node))
        return node;

      if (IsInitializedToNotNull(semanticModel, node))
        return node;

      if (IsInitializedToNull(semanticModel, node))
        return node.WithType(NullUtilities.ToNullable(node.Type));

      var classSyntax = (ClassDeclarationSyntax) node.Ancestors().First(a => a.IsKind(SyntaxKind.ClassDeclaration));
      var constructors = classSyntax.ChildNodes()
          .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
          .Cast<ConstructorDeclarationSyntax>()
          .ToArray();

      var isInitializedToNotNull = constructors.All(c => PropertyInitializedToNotNullInCtorChain(semanticModel, c, node));

      if (constructors.Length == 0 || !isInitializedToNotNull)
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
      var thisConstructorSyntax = (ConstructorDeclarationSyntax) thisConstructorSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

      return PropertyInitializedToNotNullInCtorChain(semanticModel, thisConstructorSyntax, property);
    }

    private bool PropertyInitializedToNotNullInCtor(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, PropertyDeclarationSyntax property)
    {
      var assignments = constructor.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().ToArray();

      var propertySymbol = semanticModel.GetDeclaredSymbol(property);
      var propertyAssignments = assignments.Where(a => SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(a.Left).Symbol, propertySymbol)).ToArray();

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
      var typeSymbol = semanticModel.GetTypeInfo (declaration).Type as INamedTypeSymbol;

      return typeSymbol == null || typeSymbol.IsValueType;
    }

    private bool IsNullable(SemanticModel semanticModel, PropertyDeclarationSyntax syntax)
      => semanticModel.GetDeclaredSymbol(syntax)?.Type.NullableAnnotation == NullableAnnotation.Annotated;


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
  }
}
