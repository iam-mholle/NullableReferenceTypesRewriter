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
  public class DefaultParameterRewriterTest : RewriterTestBase<DefaultParameterRewriter>
  {
    [Test]
    public void DefaultParameter_InCtor_Nullable()
    {
      //language=C#
      const string expected = @"
public A(object? obj = null)
{
}
";
      //language=C#
      const string input = @"
public A(object obj = null)
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void DefaultParameter_Null_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(object? obj = null)
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(object obj = null)
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
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
      //language=C#
      const string input = @"
public void DoStuff(object? obj = null)
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
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
      //language=C#
      const string input = @"
public void DoStuff(int? value = null)
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void NonNullableValueTypeDefaultParameter_DefaultExpression_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(int value = default(int))
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(int value = default(int))
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void InterfaceDefaultParameter_Null_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(IReadOnlyCollection<string>? value = null)
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(IReadOnlyCollection<string> value = null)
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void InterfaceDefaultParameter_Default_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(IReadOnlyCollection<string>? value = default)
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(IReadOnlyCollection<string> value = default)
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void ReferenceTypeDefaultParameter_ExplicitDefault_Nullable()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string? value = default(string?))
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(string value = default(string))
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void StringDefaultParameter_StringWithDefaultText_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string value = ""default"")
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(string value = ""default"")
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void StringDefaultParameter_StringWithNullText_Unchanged()
    {
      //language=C#
      const string expected = @"
public void DoStuff(string value = ""null"")
{
}
";
      //language=C#
      const string input = @"
public void DoStuff(string value = ""null"")
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }
  }
}
