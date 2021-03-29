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
  public class InheritanceReturnRewriterTest : RewriterTestBase<InheritanceReturnRewriter>
  {
    [Test]
    public void AbstractOverride_OverriddenReturnNullable_Nullable()
    {
      //language=C#
      const string expected = @"
  public abstract string? DoStuff(string value);
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public abstract string DoStuff(string value);
}
public class SomeDerived : SomeBase
{
  public override string? DoStuff(string value)
  {
    // Something
  }
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new InheritanceReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void AbstractOverride_OverriddenParameterNonNullable_Unchanged()
    {
      //language=C#
      const string expected = @"
  public abstract string DoStuff(string value);
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public abstract string DoStuff(string value);
}
public class SomeDerived : SomeBase
{
  public override string DoStuff(string value)
  {
    // Something
  }
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new InheritanceReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void AbstractOverrideAlreadyNullable_OverriddenParameterNullable_Unchanged()
    {
      //language=C#
      const string expected = @"
  public abstract string? DoStuff(string value);
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public abstract string? DoStuff(string value);
}
public class SomeDerived : SomeBase
{
  public override string? DoStuff(string value)
  {
    // Something
  }
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new InheritanceReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void AbstractOverride_NullableValueType_Unchanged()
    {
      //language=C#
      const string expected = @"
  public abstract int? DoStuff(int? value);
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class SomeBase
{
  public abstract int? DoStuff(int? value);
}
public class SomeDerived : SomeBase
{
  public override int? DoStuff(int? value)
  {
    // Something
  }
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new InheritanceReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void InterfaceImplementation_NullableReturnValue_Nullable()
    {
      //language=C#
      const string expected = @"
  string? DoStuff(string value);
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInNameSpace(
          "A",
          //language=C#
          @"
public class ISomething
{
  string DoStuff(string value);
}
public class Something : ISomething
{
  public override string? DoStuff(string value)
  {
    // Something
  }
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new InheritanceReturnRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }
  }
}
