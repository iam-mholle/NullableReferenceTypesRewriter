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
  public class CastExpressionRewriterTest : RewriterTestBase<CastExpressionRewriter>
  {
    [Test]
    public void DirectCast_NullableVariable()
      => SimpleRewriteAssertion(
          /* language=C# */ @"
public object DoStuff()
{
  string? obj = null;

  return (object?) obj;
}
",
          /* language=C# */ @"
public object DoStuff()
{
  string? obj = null;

  return (object) obj;
}
",
          WrapperType.Method);

    [Test]
    public void TwoDirectCasts_Stage1_NullableVariable()
      => SimpleRewriteAssertion(
          /* language=C# */ @"
public string DoStuff()
{
  string? obj = null;

  return (string) (object?) obj;
}
",
          /* language=C# */ @"
public string DoStuff()
{
  string? obj = null;

  return (string) (object) obj;
}
",
          WrapperType.Method,
          deferredRewritesPredicate: c => c.Count == 1);

    [Test]
    public void TwoDirectCasts_Stage2_NullableVariable()
      => SimpleRewriteAssertion(
          /* language=C# */ @"
public string DoStuff()
{
  string? obj = null;

  return (string?) (object?) obj;
}
",
          /* language=C# */ @"
public string DoStuff()
{
  string? obj = null;

  return (string) (object?) obj;
}
",
          WrapperType.Method);

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
      //language=C#
      const string input = @"
public object DoStuff()
{
  return (object) GetNullableString();
}
public string? GetNullableString()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void DirectCast_UnconstrainedGeneric_Unchanged ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>()
{
  return (T) new object();
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>()
{
  return (T) new object();
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void DirectCast_GenericWithClassConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>() where T : class
{
  return (T?) null;
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>() where T : class
{
  return (T) null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void DirectCast_GenericWithReferenceTypeConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>() where T : String
{
  return (T?) null;
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>() where T : String
{
  return (T) null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }
  }
}