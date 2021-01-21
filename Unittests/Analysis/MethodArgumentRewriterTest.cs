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
  public class MethodArgumentRewriterTest : RewriterTestBase
  {
    [Test]
    public void SingleParameter_NullArgument_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string? something)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something)
{
}

public void CallAboveMethod()
{
  DoStuff(null);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void SingleParameter_AlreadyNullable_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string? something)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string? something)
{
}

public void CallAboveMethod()
{
  DoStuff(null);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void SingleParameter_NotNull_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string something)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something)
{
}

public void CallAboveMethod()
{
  DoStuff(string.Empty);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleParameter_NotNull_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string something, string somethingElse)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something, string somethingElse)
{
}

public void CallAboveMethod()
{
  DoStuff(string.Empty, string.Empty);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleParameter_FirstNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string? something, string somethingElse)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something, string somethingElse)
{
}

public void CallAboveMethod()
{
  DoStuff(null, string.Empty);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleParameter_LastNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string something, string? somethingElse)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something, string somethingElse)
{
}

public void CallAboveMethod()
{
  DoStuff(string.Empty, null);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MultipleParameter_AllNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string? something, string? somethingElse)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something, string somethingElse)
{
}

public void CallAboveMethod()
{
  DoStuff(null, null);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }
  }
}
