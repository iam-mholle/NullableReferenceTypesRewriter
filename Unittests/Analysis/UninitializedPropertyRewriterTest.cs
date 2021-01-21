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
  public class UninitializedPropertyRewriterTest
  {
    [Test]
    public void InlineInitialized_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; } = null;
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; } = null;
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InlineInitialized_ToNonNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get; set; } = ""some string"";
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; } = ""some string"";
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_ValueType_Unchanged ()
    {
      //language=C#
      const string expected = @"
public int Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public int Test { get; set; }
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_NullableReferenceType_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string? Test { get; set; }
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_AssignedNullInCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; }

public A()
{
  Test = null;
}
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_AssignedNonNullInCtor_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; }

public A()
{
  Test = ""some string"";
}
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_AssignedNullInOneCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; }

public A()
{
  Test = ""some string"";
}
public A(bool _)
{
  Test = null;
}
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_AssignedInCtorChainToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; }

public A() : this(true)
{
}
public A(bool _)
{
  Test = null;
}
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_AssignedInCtorChainToNonNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string Test { get; set; }

public A() : this(true)
{
}
public A(bool _)
{
  Test = ""some string"";
}
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void Uninitialized_Array_Nullable ()
    {
      //language=C#
      const string expected = @"
public string[]? Test { get; set; }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public string[] Test { get; set; }
");
      var syntax = (PropertyDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.PropertyDeclaration));
      var field = CreatePropertyWrapper (syntax, semantic);
      var sut = new UninitializedPropertyRewriter((b, c) => { });

      var result = sut.Rewrite (field);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    private Property CreatePropertyWrapper (
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
  }
}
