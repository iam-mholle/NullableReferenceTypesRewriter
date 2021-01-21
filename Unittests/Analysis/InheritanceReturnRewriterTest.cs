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
