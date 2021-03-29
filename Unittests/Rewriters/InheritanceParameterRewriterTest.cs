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
  [TestFixture]
  public class InheritanceParameterRewriterTest : RewriterTestBase<InheritanceParameterRewriter>
  {
    [Test]
    public void AbstractOverride_OverriddenParameterNullable_Nullable()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(string? value)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeDerived : SomeBase
{
  public override void DoStuff(string value)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract void DoStuff(string? value);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void AbstractOverride_OverriddenParameterNonNullable_Unchanged()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(string value)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeDerived : SomeBase
{
  public override void DoStuff(string value)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract void DoStuff(string value);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void AbstractOverrideAlreadyNullable_OverriddenParameterNullable_Unchanged()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(string? value)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeDerived : SomeBase
{
  public override void DoStuff(string? value)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract void DoStuff(string? value);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void AbstractOverride_NullableValueType_Unchanged()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(int? value)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeDerived : SomeBase
{
  public override void DoStuff(int? value)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract void DoStuff(int? value);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void AbstractOverride_NullableFirstAndLast_AdaptedAccordingly()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(string? value1, string value2, string? value3)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeDerived : SomeBase
{
  public override void DoStuff(string value1, string value2, string value3)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract void DoStuff(string? value1, string value2, string? value3);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void AbstractOverride_NullableDefaultParameter_Nullable()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(string? value = null)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeDerived : SomeBase
{
  public override void DoStuff(string value = null)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract void DoStuff(string? value = null);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void InterfaceImplementation_NullableParameterValue_Nullable()
    {
      //language=C#
      const string expected = @"
  public override void DoStuff(string? value)
  {
    // Something
  }
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class Something : ISomething
{
  public override void DoStuff(string value)
  {
    // Something
  }
}
public class ISomething
{
  void DoStuff(string? value);
}
");
      Method method = null!;
      var baseSyntax = (MethodDeclarationSyntax) root.DescendantNodes().Last(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var baseMethod = CreateMethodWrapper(baseSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes().First(n => n.IsKind(SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => baseMethod, () => method, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, () => new[] { dependency });
      var sut = new InheritanceParameterRewriter((b, c) => { });

      var result = sut.Rewrite(method);

      Assert.That(result.ToString().Trim(), Is.EqualTo(expected.Trim()));
    }
  }
}
