// Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableReferenceTypesRewriter.Analysis;
using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  public class MethodArgumentRewriterTest : RewriterTestBase<MethodArgumentRewriter>
  {
    [Test]
    public void SingleParameter_CtorNullArgument_Nullable()
    {
      //language=C#
      const string expected = @"
public A(string? something)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public A(string something)
{
}

public void CallAboveMethod()
{
  new A(null);
}
");
      Method method = null!;
      var callingSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var callingMethod = CreateMethodWrapper(callingSyntax, semantic);
      var syntax = (BaseMethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.ConstructorDeclaration));
      method = CreateMethodWrapper (syntax, semantic, () => new []{ new Dependency(() => callingMethod, () => method, DependencyType.Usage) });
      var sut = new MethodArgumentRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

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
    public void SingleParameter_ArgumentCheck_NotNullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string something)
{
  ArgumentUtility.CheckNotNull (""something"", something);
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(string something)
{
  ArgumentUtility.CheckNotNull (""something"", something);
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
