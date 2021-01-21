using NullableReferenceTypesRewriter.Analysis;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Analysis
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
