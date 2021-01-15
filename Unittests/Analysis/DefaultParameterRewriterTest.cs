﻿using System;
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
  public class DefaultParameterRewriterTest
  {
    [Test]
    public void DefaultParameter_Null_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(object? obj = null)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(object obj = null)
{
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new DefaultParameterRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void NullableDefaultParameter_Null_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(object? obj = null)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(object? obj = null)
{
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new DefaultParameterRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void NullableValueTypeDefaultParameter_Null_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(int? value = null)
{
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public void DoStuff(int? value = null)
{
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new DefaultParameterRewriter((b, c) => { });

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
