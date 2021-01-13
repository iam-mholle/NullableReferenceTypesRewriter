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
  public class LocalDeclarationRewriterTest
  {
    [Test]
    public void SingleDeclaration_InitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff()
{
  string a = null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new LocalDeclarationRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void SingleDeclaration_InitializedToNotNull_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string a = string.Empty;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff()
{
  string a = string.Empty;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new LocalDeclarationRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleDeclaration_AllInitializedToNotNull_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string a = string.Empty, b = string.Empty;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff()
{
  string a = string.Empty, b = string.Empty;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new LocalDeclarationRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleDeclaration_FirstInitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = null, b = string.Empty;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff()
{
  string a = null, b = string.Empty;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new LocalDeclarationRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleDeclaration_LastInitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = string.Empty, b = null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff()
{
  string a = string.Empty, b = null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new LocalDeclarationRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleDeclaration_AllInitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = null, b = null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff()
{
  string a = null, b = null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new LocalDeclarationRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    private Method CreateMethodWrapper (
        MethodDeclarationSyntax syntax,
        SemanticModel semanticModel,
        Func<IReadOnlyCollection<Dependency>>? parents = null,
        Func<IReadOnlyCollection<Dependency>>? children = null)
    {
      return new Method(
          new SharedCompilation(semanticModel.Compilation),
          semanticModel.GetDeclaredSymbol(syntax)!,
          parents ?? Array.Empty<Dependency>,
          children ?? Array.Empty<Dependency>);
    }
  }
}
