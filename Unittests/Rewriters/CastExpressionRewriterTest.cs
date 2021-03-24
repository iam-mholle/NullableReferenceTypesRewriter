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