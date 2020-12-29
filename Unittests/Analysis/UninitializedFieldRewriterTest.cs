using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Analysis
{
  [TestFixture]
  public class UninitializedFieldRewriterTest
  {
    [Test]
    public void Test ()
    {
      //language=C#
      const string expected = @"
private string? test;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    private Field CreateFieldWrapper (
        FieldDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null)
    {
      return new Field(
          new SharedCompilation(semanticModel.Compilation),
          (IFieldSymbol) semanticModel.GetDeclaredSymbol(syntax.Declaration.Variables.Single()),
          parents ?? Array.Empty<Dependency>);
    }
  }
}