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
