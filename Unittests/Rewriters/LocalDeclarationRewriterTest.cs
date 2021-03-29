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

using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  [TestFixture]
  public class LocalDeclarationRewriterTest : RewriterTestBase<LocalDeclarationRewriter>
  {
    [Test]
    public void SingleDeclaration_UninitializedValueType_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  int a;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  int a;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void SingleDeclaration_UninitializedReferenceType_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void SingleDeclaration_InitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = null;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a = null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void SingleDeclaration_InitializedToNotNull_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string a = string.Empty;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a = string.Empty;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MultipleDeclaration_AllInitializedToNotNull_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string a = string.Empty, b = string.Empty;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a = string.Empty, b = string.Empty;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MultipleDeclaration_FirstInitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = null, b = string.Empty;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a = null, b = string.Empty;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MultipleDeclaration_LastInitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = string.Empty, b = null;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a = string.Empty, b = null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MultipleDeclaration_AllInitializedToNull_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff()
{
  string? a = null, b = null;
}
";
      //language=C#
      const string input = @"
public void DoStuff()
{
  string a = null, b = null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }
  }
}
