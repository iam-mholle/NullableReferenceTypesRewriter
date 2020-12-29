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
  public class CastExpressionRewriterTest
  {
    [Test]
    public void DirectCast_NullableVariable ()
    {
      //language=C#
      const string expected = @"
public object DoStuff()
{
  string? obj = null;

  return (object?) obj;
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff()
{
  string? obj = null;

  return (object) obj;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().Single(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new CastExpressionRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void DirectCast_NullableReturnValue ()
    {
      //language=C#
      const string expected = @"
public object DoStuff()
{
  return (object?) GetNullableString();
}
";
      var (semantic, root) = CompiledSourceFileProvider.CompileInClass (
          "A",
          //language=C#
          @"
public object DoStuff()
{
  return (object) GetNullableString();
}
public string? GetNullableString()
{
  return null;
}
");
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var method = CreateMethodWrapper (syntax, semantic);
      var sut = new CastExpressionRewriter((b, c) => { });

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