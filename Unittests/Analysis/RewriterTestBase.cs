using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;

namespace NullableReferenceTypesRewriter.UnitTests.Analysis
{
  public abstract class RewriterTestBase
  {
    protected Method CreateMethodWrapper (
        MethodDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null,
        Func<IReadOnlyCollection<Dependency>>? children = null)
    {
      return new Method(
          new SharedCompilation(semanticModel.Compilation),
          (IMethodSymbol) semanticModel.GetDeclaredSymbol(syntax)!,
          parents ?? Array.Empty<Dependency>,
          children ?? Array.Empty<Dependency>);
    }

    protected Property CreatePropertyWrapper (
        PropertyDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null,
        Func<IReadOnlyCollection<Dependency>>? children = null)
    {
      return new Property(
          new SharedCompilation(semanticModel.Compilation),
          (IPropertySymbol) semanticModel.GetDeclaredSymbol(syntax)!,
          parents ?? Array.Empty<Dependency>,
          children ?? Array.Empty<Dependency>);
    }

    protected Field CreateFieldWrapper (
        FieldDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null)
    {
      return new Field(
          new SharedCompilation(semanticModel.Compilation),
          (IFieldSymbol) semanticModel.GetDeclaredSymbol(syntax.Declaration.Variables.First())!,
          parents ?? Array.Empty<Dependency>);
    }
  }
}
