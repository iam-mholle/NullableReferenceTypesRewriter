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
  public class InheritanceParameterRewriterTest
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

    private Method CreateMethodWrapper(
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
