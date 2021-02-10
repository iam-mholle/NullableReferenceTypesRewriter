using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  public class CanBeNullRewriterTest: RewriterTestBase<CanBeNullRewriter>
  {
    [Test]
    public void AnnotatedMethod_ReturnTypeNullable ()
    {
      //language=C#
      const string expected = @"
[CanBeNull]
public object? DoStuff()
{
  return null;
}
";
      //language=C#
      const string input = @"
[CanBeNull]
public object DoStuff()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void AnnotatedParameter_ParameterTypeNullable ()
    {
      //language=C#
      const string expected = @"
public object DoStuff([CanBeNull] string? input)
{
  return null;
}
";
      //language=C#
      const string input = @"
public object DoStuff([CanBeNull] string input)
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }
  }
}
