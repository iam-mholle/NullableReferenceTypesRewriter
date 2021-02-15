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

    [Test]
    public void AnnotatedProperty_PropertyTypeNullable ()
    {
      //language=C#
      const string expected = @"
[CanBeNull]
public string? Value { get; set; }
";
      //language=C#
      const string input = @"
[CanBeNull]
public string Value { get; set; }
";

      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void AnnotatedField_FieldTypeNullable ()
    {
      //language=C#
      const string expected = @"
[CanBeNull]
private string? _value;
";
      //language=C#
      const string input = @"
[CanBeNull]
private string _value;
";

      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }
  }
}
