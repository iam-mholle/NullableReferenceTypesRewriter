using NullableReferenceTypesRewriter.Analysis;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Analysis
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
