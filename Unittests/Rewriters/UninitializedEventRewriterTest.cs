using NullableReferenceTypesRewriter.Rewriters;
using NUnit.Framework;

namespace NullableReferenceTypesRewriter.UnitTests.Rewriters
{
  [TestFixture]
  public class UninitializedEventRewriterTest : RewriterTestBase<UninitializedEventRewriter>
  {
    [Test]
    public void InlineInitialized_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private event Action? test = null;
";
      //language=C#
      const string input = @"
private event Action test = null;
";

      SimpleRewriteAssertion(expected, input, WrapperType.EventField);
    }

    [Test]
    public void Uninitialized_Nullable ()
    {
      //language=C#
      const string expected = @"
private event Action? test;
";
      //language=C#
      const string input = @"
private event Action test;
";

      SimpleRewriteAssertion(expected, input, WrapperType.EventField);
    }

    [Test]
    public void InlineInitialized_OneNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private event Action? test1 = null, test2 = () => {};
";
      //language=C#
      const string input = @"
private event Action test1 = null, test2 = () => {};
";

      SimpleRewriteAssertion(expected, input, WrapperType.EventField);
    }

    [Test]
    public void InlineInitialized_ToNotNull_Unchanged ()
    {
      //language=C#
      const string input = @"
private event Action test = () => {};
";

      SimpleUnchangedAssertion(input, WrapperType.EventField);
    }

    [Test]
    public void InitializedInCtor_ToNotNull_Unchanged ()
    {
      //language=C#
      const string input = @"
private event Action test;

public A()
{
  test = () => {};
}
";

      SimpleUnchangedAssertion(input, WrapperType.EventField);
    }

    [Test]
    public void InitializedInCtor_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private event Action? test;
";
      //language=C#
      const string input = @"
private event Action test;

public A()
{
  test = null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.EventField);
    }

    [Test]
    public void InitializedInCtorChain_ToNotNull_Unchanged ()
    {
      //language=C#
      const string input = @"
private event Action test;

public A(bool _) : this()
{
}

public A()
{
  test = () => {};
}
";

      SimpleUnchangedAssertion(input, WrapperType.EventField);
    }

    [Test]
    public void UninitializedInOneCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
private event Action? test;
";
      //language=C#
      const string input = @"
private event Action test;

public A(bool _)
{
  test = () => {};
}

public A()
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.EventField);
    }

    [Test]
    public void UninitializedInOneCtorInCtorChain_Nullable ()
    {
      //language=C#
      const string expected = @"
private event Action? test;
";
      //language=C#
      const string input = @"
private event Action test;

public A(bool _) : this()
{
  test = () => {};
}

public A()
{
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.EventField);
    }
  }
}
