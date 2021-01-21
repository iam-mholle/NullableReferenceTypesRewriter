using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  [TestFixture]
  public class UninitializedPropertyRewriterTest : RewriterTestBase<UninitializedPropertyRewriter>
  {
    [Test]
    public void InlineInitialized_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; } = null;
";
      //language=C#
      const string input = @"
public string Test { get; set; } = null;
";

      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void InlineInitialized_ToNonNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get; set; } = ""some string"";
";
      //language=C#
      const string input = @"
public string Test { get; set; } = ""some string"";
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_ValueType_Unchanged ()
    {
      //language=C#
      const string expected = @"
public int Test { get; set; }
";
      //language=C#
      const string input = @"
public int Test { get; set; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_NullableReferenceType_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      //language=C#
      const string input = @"
public string? Test { get; set; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_AssignedNullInCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      //language=C#
      const string input = @"
public string Test { get; set; }

public A()
{
  Test = null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_AssignedNonNullInCtor_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get; set; }
";
      //language=C#
      const string input = @"
public string Test { get; set; }

public A()
{
  Test = ""some string"";
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_AssignedNullInOneCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      //language=C#
      const string input = @"
public string Test { get; set; }

public A()
{
  Test = ""some string"";
}
public A(bool _)
{
  Test = null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_AssignedInCtorChainToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
public string? Test { get; set; }
";
      //language=C#
      const string input = @"
public string Test { get; set; }

public A() : this(true)
{
}
public A(bool _)
{
  Test = null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_AssignedInCtorChainToNonNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get; set; }
";
      //language=C#
      const string input = @"
public string Test { get; set; }

public A() : this(true)
{
}
public A(bool _)
{
  Test = ""some string"";
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Uninitialized_Array_Nullable ()
    {
      //language=C#
      const string expected = @"
public string[]? Test { get; set; }
";
      //language=C#
      const string input = @"
public string[] Test { get; set; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }

    [Test]
    public void Getter_NotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
public string Test { get { return _test; } }
";
      //language=C#
      const string input = @"
public string Test { get { return _test; } }
private string _test = ""some string"";
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Property);
    }
  }
}
