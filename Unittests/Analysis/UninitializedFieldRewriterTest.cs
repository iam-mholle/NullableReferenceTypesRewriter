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
    public void InlineInitialized_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = null;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = null;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InlineInitialized_ToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test = string.Empty;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = string.Empty;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void AlreadyNullable_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string? test = string.Empty;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string? test = string.Empty;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InlineInitialized_ToReturnValueOfNullableMethod_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = GetString();
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = GetString();

private static string? GetString() => null;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void NotInitialized_NoCtor_Nullable ()
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

    [Test]
    public void NotInitialized_CtorWithoutAssignment_Nullable ()
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

public A(){}
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InitializedInCtor_ToNull_Nullable ()
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

public A(){ test = null; }
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InitializedInCtor_ToNullableReturnValue_Nullable ()
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

public A(){ test = GetString(); }

private static string? GetString() => null;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InitializedInCtor_ToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test;

public A(){ test = string.Empty; }
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleVariables_OneNullable_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = null, test2 = string.Empty;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = null, test2 = string.Empty;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleVariables_OneNullableLast_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = string.Empty, test2 = null;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = string.Empty, test2 = null;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleVariables_AllNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test = string.Empty, test2 = string.Empty;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = string.Empty, test2 = string.Empty;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleVariables_OneUninitialized_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = string.Empty, test2;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test = string.Empty, test2;
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InitializedInCtorChain_SecondCtorToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test;

public A() : this(true) { }
public A(bool _) { test = string.Empty; }
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InitializedInCtorChain_BothCtorToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
private string test;

public A() : this(true) { test = string.Empty; }
public A(bool _) { test = string.Empty; }
");
      var syntax = (FieldDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.FieldDeclaration));
      var field = CreateFieldWrapper (syntax, semantic);
      var sut = new UninitializedFieldRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InitializedInCtorChain_FirstCtorToNotNull_Nullable ()
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

public A() : this(true) { test = string.Empty; }
public A(bool _) { }
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
          (IFieldSymbol) semanticModel.GetDeclaredSymbol(syntax.Declaration.Variables.First())!,
          parents ?? Array.Empty<Dependency>);
    }
  }
}