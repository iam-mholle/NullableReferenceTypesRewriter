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
