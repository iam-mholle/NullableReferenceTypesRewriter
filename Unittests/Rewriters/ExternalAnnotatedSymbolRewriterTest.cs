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
  public class ExternalAnnotatedSymbolRewriterTest : RewriterTestBase<ExternalAnnotatedSymbolRewriter>
  {
    [Test]
    public void OverrideExternalMethod()
    {
      //language=C#
      const string expected = @"
  public override string? DoStuff(string? value, string? value2, string? value3)
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
  public override string DoStuff(string value, string? value2, string value3)
  {
    // Something
  }
}
public class SomeBase
{
  public abstract string? DoStuff(string? value, string value2, string? value3);
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new ExternalAnnotatedSymbolRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }

    [Test]
    public void ImplementExternalMethod()
    {
      //language=C#
      const string expected = @"
  public string? DoStuff(string? value, string? value2, string? value3)
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
  public string DoStuff(string value, string? value2, string value3)
  {
    // Something
  }
}
public interface SomeBase
{
  public string? DoStuff(string? value, string value2, string? value3);
}
");
      Method method = null!;
      var derivedSyntax = (MethodDeclarationSyntax) root.DescendantNodes ().Last(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var derivedMethod = CreateMethodWrapper(derivedSyntax, semantic);
      var syntax = (MethodDeclarationSyntax) root.DescendantNodes ().First(n => n.IsKind (SyntaxKind.MethodDeclaration));
      var dependency = new Dependency(() => method, () => derivedMethod, DependencyType.Inheritance);
      method = CreateMethodWrapper(syntax, semantic, null, () => new[] { dependency });
      var sut = new ExternalAnnotatedSymbolRewriter((b, c) => { });

      var result = sut.Rewrite (method);

      Assert.That (result.ToString().Trim(), Is.EqualTo (expected.Trim()));
    }
  }
}
