using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.ClassFields;
using NullableReferenceTypesRewriter.Utilities;

namespace NullableReferenceTypesRewriter.Analysis
{
  public class UninitializedFieldRewriter : RewriterBase
  {
    public UninitializedFieldRewriter (Action<RewriterBase, IReadOnlyCollection<IRewritable>> additionalRewrites)
        : base(additionalRewrites)
    {
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      var semanticModel = _currentField.SemanticModel;

      if (IsValueType(semanticModel, node))
        return node;

      if (IsNullable(semanticModel, node))
        return node;

      if (node.Declaration.Variables.Any(d => IsInitializedToNull(semanticModel, d)))
      {
        return node.WithDeclaration(node.Declaration.WithType(NullUtilities.ToNullable(node.Declaration.Type)));
      }

      // TODO: check if ctor initialized to nullable -> nullable
      // TODO: check if uninitialized in one ctor -> nullable

      return node;
    }

    private bool IsReadOnly (MemberDeclarationSyntax field)
      => field.Modifiers.FirstOrDefault (mod => mod.ToString() == "readonly").Kind() != SyntaxKind.None;

    private bool HasNoInitializer (VariableDeclaratorSyntax variableDeclarator)
      => variableDeclarator.Initializer == null;

    private bool IsValueType (SemanticModel semanticModel, FieldDeclarationSyntax declaration)
    {
      var typeSymbol = semanticModel.GetTypeInfo (declaration.Declaration.Type).Type as INamedTypeSymbol;
      return typeSymbol == null
             || typeSymbol.IsValueType;
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
  }
}