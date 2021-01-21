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
  public class UninitializedFieldRewriter : RewriterBase
  {
    public UninitializedFieldRewriter (Action<RewriterBase, IReadOnlyCollection<(IRewritable, RewriteCapability)>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      var semanticModel = CurrentField.SemanticModel;

      if (IsValueType(semanticModel, node))
        return node;

      if (IsNullable(semanticModel, node))
        return node;

      if (node.Declaration.Variables.All(d => IsInitializedToNotNull(semanticModel, d)))
        return node;

      var classSyntax = (ClassDeclarationSyntax) node.Ancestors().First(a => a.IsKind(SyntaxKind.ClassDeclaration));

      if (node.Declaration.Variables.Any(d => IsInitializedToNull(semanticModel, d)))
        return ToNullable(node);

      var symbol = semanticModel.GetDeclaredSymbol(node.Declaration.Variables.First());

      var constructors = classSyntax.ChildNodes()
          .Where(n => n.IsKind(SyntaxKind.ConstructorDeclaration))
          .Cast<ConstructorDeclarationSyntax>()
          .ToArray();

      var isInitializedToNotNull = constructors.All(c => VariableInitializedToNotNullInCtorChain(semanticModel, c, node.Declaration.Variables.First()));

      if (constructors.Length == 0 || !isInitializedToNotNull)
        return ToNullable(node);

      return node;

      SyntaxNode? ToNullable(FieldDeclarationSyntax node) => node.WithDeclaration(node.Declaration.WithType(NullUtilities.ToNullableWithGenericsCheck(CurrentField.SemanticModel, classSyntax, node.Declaration.Type)));
    }

    protected override IReadOnlyCollection<(IRewritable, RewriteCapability)> GetAdditionalRewrites(Method method)
    {
      return method.Parents
          .Select(p => p.From)
          .OfType<IRewritable>()
          .Select(r => (r, RewriteCapability.ReturnValueChange))
          .ToArray();
    }

    private bool VariableInitializedToNotNullInCtorChain(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, VariableDeclaratorSyntax variable)
    {
      var isInitializedToNotNullInCurrent = VariableInitializedToNotNull(semanticModel, constructor, variable);

      if (isInitializedToNotNullInCurrent)
        return true;

      if (constructor.Initializer is null)
        return false;

      var thisConstructorSymbol = semanticModel.GetSymbolInfo(constructor.Initializer).Symbol ?? throw new InvalidOperationException();
      var thisConstructorSyntax = (ConstructorDeclarationSyntax) thisConstructorSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

      return VariableInitializedToNotNullInCtorChain(semanticModel, thisConstructorSyntax, variable);
    }

    private bool VariableInitializedToNotNull(SemanticModel semanticModel, ConstructorDeclarationSyntax constructor, VariableDeclaratorSyntax variable)
    {
      var assignments = constructor.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().ToArray();

      var variableSymbol = semanticModel.GetDeclaredSymbol(variable);
      var fieldAssignments = assignments.Where(a => SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(a.Left).Symbol, variableSymbol)).ToArray();

      if (fieldAssignments.Length == 0)
      {
        return false;
      }

      return fieldAssignments.All(a => !NullUtilities.CanBeNull(a.Right, semanticModel));
    }

    private bool IsValueType(SemanticModel semanticModel, FieldDeclarationSyntax declaration)
      => IsValueType(semanticModel, declaration.Declaration.Type);

    private bool IsValueType (SemanticModel semanticModel, TypeSyntax declaration)
    {
      if (declaration is ArrayTypeSyntax)
        return false;

      var typeSymbol = semanticModel.GetTypeInfo (declaration).Type as INamedTypeSymbol;

      return typeSymbol == null || typeSymbol.IsValueType;
    }

    private bool IsNullable(SemanticModel semanticModel, FieldDeclarationSyntax syntax)
      => (semanticModel.GetDeclaredSymbol(syntax.Declaration.Variables.First()) as IFieldSymbol)?.Type.NullableAnnotation == NullableAnnotation.Annotated;

    private bool IsInitializedToNull (SemanticModel semanticModel, VariableDeclaratorSyntax variableDeclarator)
    {
      if (variableDeclarator.Initializer != null)
      {
        return NullUtilities.CanBeNull (variableDeclarator.Initializer.Value, semanticModel);
      }

      return false;
    }

    private bool IsInitializedToNotNull (SemanticModel semanticModel, VariableDeclaratorSyntax variableDeclarator)
    {
      if (variableDeclarator.Initializer != null)
      {
        return !NullUtilities.CanBeNull (variableDeclarator.Initializer.Value, semanticModel);
      }

      return false;
    }
  }
}