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
  public class NullReturnRewriterTest : RewriterTestBase<NullReturnRewriter>
  {
    [Test]
    public void MethodReturningNull_ReturnValueNullable ()
    {
      //language=C#
      const string expected = @"
public object? DoStuff()
{
  return null;
}
";
      //language=C#
      const string input = @"
public object DoStuff()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningNull_ReturnValueAlreadyNullable_SameInstance ()
    {
      //language=C#
      const string input = @"
public object? DoStuff()
{
  return null;
}
";
      SimpleUnchangedAssertion(input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningNull_NotNullAnnotated_SameInstance ()
    {
      //language=C#
      const string input = @"
[NotNull]
public object? DoStuff()
{
  return null;
}
";
      SimpleUnchangedAssertion(input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningMethodReturningNullable_ReturnValueNullable ()
    {
      //language=C#
      const string expected = @"
public object? A()
{
  return B();
}
";
      //language=C#
      const string input = @"
public object A()
{
  return B();
}
public object? B()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningMethodReturningNullable_WithIsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object A()
{
  var b = B();
  if (b is null)
    return new object();

  return b;
}
";
      //language=C#
      const string input = @"
public object A()
{
  var b = B();
  if (b is null)
    return new object();

  return b;
}
public object? B()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningMethodReturningNullable_WithEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object A()
{
  var b = B();
  if (b == null)
    return new object();

  return b;
}
";
      //language=C#
      const string input = @"
public object A()
{
  var b = B();
  if (b == null)
    return new object();

  return b;
}
public object? B()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningMethodReturningNullable_WithNotEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object A()
{
  var b = B();
  if (b != null)
    return b;

  return new object();
}
";
      //language=C#
      const string input = @"
public object A()
{
  var b = B();
  if (b != null)
    return b;

  return new object();
}
public object? B()
{
  return null;
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgument_ReturnValueNullable ()
    {
      //language=C#
      const string expected = @"
public object? DoStuff<T>(T obj)
{
  return obj;
}
";
      //language=C#
      const string input = @"
public object DoStuff<T>(T obj)
{
  return obj;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgument_WithIsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object DoStuff<T>(T obj)
{
  if (obj is null)
    return new object();

  return obj;
}
";
      //language=C#
      const string input = @"
public object DoStuff<T>(T obj)
{
  if (obj is null)
    return new object();

  return obj;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgument_WithEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object DoStuff<T>(T obj)
{
  if (obj == null)
    return new object();

  return obj;
}
";
      //language=C#
      const string input = @"
public object DoStuff<T>(T obj)
{
  if (obj == null)
    return new object();

  return obj;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgument_WithNotEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public object DoStuff<T>(T obj)
{
  if (obj != null)
    return obj;

  return new object();
}
";
      //language=C#
      const string input = @"
public object DoStuff<T>(T obj)
{
  if (obj != null)
    return obj;

  return new object();
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_WithNotEqualsNullCheck_ReturnValueUnchanged ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>(T obj)
{
  return null;
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>(T obj)
{
  return null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_WithClassConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
public T? DoStuff<T>(T obj) where T : class
{
  return null;
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>(T obj) where T : class
{
  return null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_WithReferenceTypeConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
public T? DoStuff<T>(T obj) where T : String
{
  return null;
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>(T obj) where T : String
{
  return null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericType_Unconstrained_Unchanged ()
    {
      //language=C#
      const string expected = @"
public T DoStuff<T>(T obj)
{
  return null;
}
";
      //language=C#
      const string input = @"
public T DoStuff<T>(T obj)
{
  return null;
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_Unconstrained_Unchanged ()
    {
      //language=C#
      const string expected = @"
  public T DoStuff(T obj)
  {
    return null;
  }
";
      //language=C#
      const string input = @"
public class A<T>
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method, CompileIn.Namespace);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_WithClassConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
  public T? DoStuff(T obj)
  {
    return null;
  }
";
      //language=C#
      const string input = @"
public class A<T> where T : class
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method, CompileIn.Namespace);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_WithStructConstraint_Unchanged ()
    {
      //language=C#
      const string expected = @"
  public T DoStuff(T obj)
  {
    return null;
  }
";
      //language=C#
      const string input = @"
public class A<T> where T : struct
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
";

      SimpleRewriteAssertion(expected, input, WrapperType.Method, CompileIn.Namespace);
    }

    [Test]
    public void MethodReturningGenericArgumentOfGenericClassType_WithReferenceTypeConstraint_Nullable ()
    {
      //language=C#
      const string expected = @"
  public T? DoStuff(T obj)
  {
    return null;
  }
";
      //language=C#
      const string input = @"
public class A<T> where T : String
{
  public T DoStuff(T obj)
  {
    return null;
  }
}
";
      
      SimpleRewriteAssertion(expected, input, WrapperType.Method, CompileIn.Namespace);
    }
  }
}
