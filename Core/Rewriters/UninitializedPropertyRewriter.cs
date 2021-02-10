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
      var semanticModel = CurrentProperty.SemanticModel;

      if (node.Type.IsValueType(semanticModel))
        return node;

      if (node.IsAbstract())
        return node;

      if (IsNullable(semanticModel, node))
        return node;

      if (node.IsInitializedToNotNull(semanticModel))
        return node;

      if (node.IsInitializedToNull(semanticModel))
        return node.WithType(NullUtilities.ToNullable(node.Type));

      if (node.Ancestors().FirstOrDefault(a => a.IsKind(SyntaxKind.InterfaceDeclaration)) != null)
        return node;

      var classSyntax = (ClassDeclarationSyntax) node.Ancestors().First(a => a.IsKind(SyntaxKind.ClassDeclaration));
      var constructors = classSyntax.ChildNodes()
          .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
          .Cast<ConstructorDeclarationSyntax>()
          .ToArray();

      var isInitializedToNotNull = constructors.All(c => PropertyInitializedToNotNullInCtorChain(semanticModel, c, node));

      if (node.IsAutoProperty() && (constructors.Length == 0 || !isInitializedToNotNull))
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

    private bool IsNullable(SemanticModel semanticModel, PropertyDeclarationSyntax syntax)
      => semanticModel.GetDeclaredSymbol(syntax)?.Type.NullableAnnotation == NullableAnnotation.Annotated;
  }
}
