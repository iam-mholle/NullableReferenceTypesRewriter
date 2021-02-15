using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests
{
  [TestFixture]
  public class SymbolExtensionsTest
  {
    [Test]
    public void StaticMethod()
    {
      var (semantic, syntax) = CompiledSourceFileProvider.CompileMethod(
          @"public static void Test(string input)
{
  Console.WriteLine(input);
}");
      var symbol = ModelExtensions.GetDeclaredSymbol(semantic, syntax)!;

      var result = symbol.ToDisplayStringWithStaticModifier();

      Assert.That(symbol, Is.Not.Null);
      Assert.That(result, Is.EqualTo("static TestNameSpace.TestClass.Test(string)"));
    }

    [Test]
    public void NonStaticMethod()
    {
      var (semantic, syntax) = CompiledSourceFileProvider.CompileMethod(
          @"public void Test(string input)
{
  Console.WriteLine(input);
}");
      var symbol = ModelExtensions.GetDeclaredSymbol(semantic, syntax)!;

      var result = symbol.ToDisplayStringWithStaticModifier();

      Assert.That(symbol, Is.Not.Null);
      Assert.That(result, Is.EqualTo("TestNameSpace.TestClass.Test(string)"));
    }

    [Test]
    public void StaticProperty()
    {
      var (semantic, syntax) = CompiledSourceFileProvider.CompileInClass(
          "A",
          "public static string Test => string.Empty");
      var symbol = semantic.GetDeclaredSymbol(syntax.DescendantNodes().First(n => n.IsKind(SyntaxKind.PropertyDeclaration)))!;

      var result = symbol.ToDisplayStringWithStaticModifier();

      Assert.That(result, Is.EqualTo("static TestNameSpace.A.Test"));
    }

    [Test]
    public void NonStaticProperty()
    {
      var (semantic, syntax) = CompiledSourceFileProvider.CompileInClass(
          "A",
          "public string Test => string.Empty");
      var symbol = semantic.GetDeclaredSymbol(syntax.DescendantNodes().First(n => n.IsKind(SyntaxKind.PropertyDeclaration)))!;

      var result = symbol.ToDisplayStringWithStaticModifier();

      Assert.That(result, Is.EqualTo("TestNameSpace.A.Test"));
    }
  }
}
