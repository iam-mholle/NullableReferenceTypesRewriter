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
  public class NullReturnRewriterTest
  {
    [Test]
    public void MethodReturningNull_ReturnValueNullable ()
    {
      //language=C#
      const string expected = @"
public object? DoStuff()
{
  return null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningNull_ReturnValueAlreadyNullable_SameInstance ()
    {
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object? DoStuff()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result, Is.SameAs(syntax));
    }

    [Test]
    public void MethodReturningMethodReturningNullable_ReturnValueNullable ()
    {
      //language=C#
      const string expected = @"
public object? A()
{
  return B();
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object A()
{
  return B();
}
public object? B()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningMethodReturningNullable_WithIsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object A()
{
  var b = B();
  if (b is null)
    return new object();

  return b;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object A()
{
  var b = B();
  if (b is null)
    return new object();

  return b;
}
public object? B()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningMethodReturningNullable_WithEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object A()
{
  var b = B();
  if (b == null)
    return new object();

  return b;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object A()
{
  var b = B();
  if (b == null)
    return new object();

  return b;
}
public object? B()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningMethodReturningNullable_WithNotEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object A()
{
  var b = B();
  if (b != null)
    return b;

  return new object();
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object A()
{
  var b = B();
  if (b != null)
    return b;

  return new object();
}
public object? B()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgument_ReturnValueNullable ()
    {
      //language=C#
      const string expected = @"
public object? DoStuff<T>(T obj)
{
  return obj;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff<T>(T obj)
{
  return obj;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgument_WithIsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object DoStuff<T>(T obj)
{
  if (obj is null)
    return new object();

  return obj;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff<T>(T obj)
{
  if (obj is null)
    return new object();

  return obj;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgument_WithEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object DoStuff<T>(T obj)
{
  if (obj == null)
    return new object();

  return obj;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff<T>(T obj)
{
  if (obj == null)
    return new object();

  return obj;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgument_WithNotEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object DoStuff<T>(T obj)
{
  if (obj != null)
    return obj;

  return new object();
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff<T>(T obj)
{
  if (obj != null)
    return obj;

  return new object();
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_WithNotEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>(T obj)
{
  return null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public T DoStuff<T>(T obj)
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_WithClassConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
public T? DoStuff<T>(T obj) where T : class
{
  return null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public T DoStuff<T>(T obj) where T : class
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_WithReferenceTypeConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
public T? DoStuff<T>(T obj) where T : String
{
  return null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public T DoStuff<T>(T obj) where T : String
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_Unconstrained_Unchanged ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>(T obj)
{
  return null;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public T DoStuff<T>(T obj)
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_Unconstrained_Unchanged ()
    {
      //language=C#
      const string expected = @"
  public T DoStuff(T obj)
  {
    return null;
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class A<T>
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_WithClassConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
  public T? DoStuff(T obj)
  {
    return null;
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class A<T> where T : class
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_WithReferenceTypeConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
  public T? DoStuff(T obj)
  {
    return null;
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class A<T> where T : String
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new NullReturnRewriter((b, c) => { });

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
