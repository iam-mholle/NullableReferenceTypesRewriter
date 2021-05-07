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
  public class UninitializedFieldRewriterTest : RewriterTestBase<UninitializedFieldRewriter>
  {
    [Test]
    public void InlineInitialized_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = null;
";
      //language=C#
      const string input = @"
private string test = null;
";

      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InlineInitialized_ToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test = string.Empty;
";
      //language=C#
      const string input = @"
private string test = string.Empty;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void AlreadyNullable_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string? test = string.Empty;
";
      //language=C#
      const string input = @"
private string? test = string.Empty;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InlineInitialized_ToReturnValueOfNullableMethod_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = GetString();
";
      //language=C#
      const string input = @"
private string test = GetString();

private static string? GetString() => null;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void NotInitialized_NoCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test;
";
      //language=C#
      const string input = @"
private string test;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void NotInitialized_CtorWithoutAssignment_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test;
";
      //language=C#
      const string input = @"
private string test;

public A(){}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInCtor_ToNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test;
";
      //language=C#
      const string input = @"
private string test;

public A(){ test = null; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInCtor_ToNullableReturnValue_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test;
";
      //language=C#
      const string input = @"
private string test;

public A(){ test = GetString(); }

private static string? GetString() => null;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInCtor_ToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test;
";
      //language=C#
      const string input = @"
private string test;

public A(){ test = string.Empty; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void MultipleVariables_OneNullable_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = null, test2 = string.Empty;
";
      //language=C#
      const string input = @"
private string test = null, test2 = string.Empty;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void MultipleVariables_OneNullableLast_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = string.Empty, test2 = null;
";
      //language=C#
      const string input = @"
private string test = string.Empty, test2 = null;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void MultipleVariables_AllNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test = string.Empty, test2 = string.Empty;
";
      //language=C#
      const string input = @"
private string test = string.Empty, test2 = string.Empty;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void MultipleVariables_OneUninitialized_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test = string.Empty, test2;
";
      //language=C#
      const string input = @"
private string test = string.Empty, test2;
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInCtorChain_SecondCtorToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test;
";
      //language=C#
      const string input = @"
private string test;

public A() : this(true) { }
public A(bool _) { test = string.Empty; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInCtorChain_BothCtorToNotNull_Unchanged ()
    {
      //language=C#
      const string expected = @"
private string test;
";
      //language=C#
      const string input = @"
private string test;

public A() : this(true) { test = string.Empty; }
public A(bool _) { test = string.Empty; }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInCtorChain_FirstCtorToNotNull_Nullable ()
    {
      //language=C#
      const string expected = @"
private string? test;
";
      //language=C#
      const string input = @"
private string test;

public A() : this(true) { test = string.Empty; }
public A(bool _) { }
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void Uninitialized_NotNullAttribute_Unchanged ()
    {
      //language=C#
      const string expected = @"
[NotNull]
private string test;
";
      //language=C#
      const string input = @"
[NotNull]
private string test;
";

      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void InitializedInStaticCtor_Unchanged ()
    {
      //language=C#
      const string expected = @"
private static string test;
";
      //language=C#
      const string input = @"
private static string test;

public static A() { test = string.Empty; }
";

      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }

    [Test]
    public void UninitializedInStaticCtor_Nullable ()
    {
      //language=C#
      const string expected = @"
private static string? test;
";
      //language=C#
      const string input = @"
private static string test;

public static A() { }
";

      SimpleRewriteAssertion(expected, input, WrapperType.Field);
    }
  }
}