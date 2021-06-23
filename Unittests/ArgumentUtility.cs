// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
#nullable enable
// ReSharper disable once CheckNamespace
namespace Remotion.Utilities
{
  /// <summary>
  /// This utility class provides methods for checking arguments.
  /// </summary>
  /// <remarks>
  /// Some methods of this class return the value of the parameter. In some cases, this is useful because the value will be converted to another 
  /// type:
  /// <code><![CDATA[
  /// void foo (object o) 
  /// {
  ///   int i = ArgumentUtility.CheckNotNullAndType<int> ("o", o);
  /// }
  /// ]]></code>
  /// In some other cases, the input value is returned unmodified. This makes it easier to use the argument checks in calls to base class constructors
  /// or property setters:
  /// <code><![CDATA[
  /// class MyType : MyBaseType
  /// {
  ///   public MyType (string name) : base (ArgumentUtility.CheckNotNullOrEmpty ("name", name))
  ///   {
  ///   }
  /// 
  ///   public override Name
  ///   {
  ///     set { base.Name = ArgumentUtility.CheckNotNullOrEmpty ("value", value); }
  ///   }
  /// }
  /// ]]></code>
  /// </remarks>
  static partial class ArgumentUtility
  {
    [AssertionMethod]
#if !DEBUG
    [MethodImpl (MethodImplOptions.AggressiveInlining)]
#endif
    public static T CheckNotNull<T> (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL), NoEnumeration] T actualValue)
        where T : notnull
    {
      throw new NotImplementedException();
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckNotNull<T> (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] [NoEnumeration] T actualValue)
        where T : notnull
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
#if !DEBUG
    [MethodImpl (MethodImplOptions.AggressiveInlining)]
#endif
    public static string CheckNotNullOrEmpty (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] string actualValue)
    {
      throw new NotImplementedException();
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckNotNullOrEmpty (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] string actualValue)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static T CheckNotNullOrEmpty<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T collection)
        where T: ICollection
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotNullOrEmpty<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] ICollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotNullOrEmpty<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] IReadOnlyCollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckNotNullOrEmpty<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T collection)
        where T: ICollection
    {
      throw new NotImplementedException();
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckNotNullOrEmpty<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] ICollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckNotNullOrEmpty<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] IReadOnlyCollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static T CheckNotNullOrItemsNull<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T collection)
        where T: ICollection
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotNullOrItemsNull<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] ICollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotNullOrItemsNull<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] IReadOnlyCollection<T> collection)
    {
      throw new NotImplementedException();
    }

    private static void CheckNotNullOrItemsNullImplementation (string argumentName, IEnumerable enumerable)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static T CheckNotNullOrEmptyOrItemsNull<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T collection)
        where T: ICollection
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotNullOrEmptyOrItemsNull<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] ICollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotNullOrEmptyOrItemsNull<T> (
        [InvokerParameterName] string argumentName, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] IReadOnlyCollection<T> collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
#if !DEBUG
    [MethodImpl (MethodImplOptions.AggressiveInlining)]
#endif
    public static string? CheckNotEmpty ([InvokerParameterName] string argumentName, string? actualValue)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static T CheckNotEmpty<T> ([InvokerParameterName] string argumentName, T collection)
        where T: ICollection?
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotEmpty<T> ([InvokerParameterName] string argumentName, ICollection<T>? collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static void CheckNotEmpty<T> ([InvokerParameterName] string argumentName, IReadOnlyCollection<T>? collection)
    {
      throw new NotImplementedException();
    }

    [AssertionMethod]
    public static Guid CheckNotEmpty ([InvokerParameterName] string argumentName, Guid actualValue)
    {
      throw new NotImplementedException();
    }

    public static object CheckNotNullAndType (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] [NoEnumeration] object actualValue,
        Type expectedType)
    {
      throw new NotImplementedException();
    }

    ///// <summary>Returns the value itself if it is not <see langword="null"/> and of the specified value type.</summary>
    ///// <typeparam name="TExpected"> The type that <paramref name="actualValue"/> must have. </typeparam>
    ///// <exception cref="ArgumentNullException"> <paramref name="actualValue"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentException"> <paramref name="actualValue"/> is an instance of another type (which is not a subclass of <typeparamref name="TExpected"/>).</exception>
    //public static TExpected CheckNotNullAndType<TExpected> (string argumentName, object actualValue)
    //  where TExpected: class
    //{
    //  if (actualValue == null)
    //    throw new ArgumentNullException (argumentName);
    //  return CheckType<TExpected> (argumentName, actualValue);
    //}

    /// <summary>Returns the value itself if it is not <see langword="null"/> and of the specified value type.</summary>
    /// <typeparam name="TExpected"> The type that <paramref name="actualValue"/> must have. </typeparam>
    /// <exception cref="ArgumentNullException">The <paramref name="actualValue"/> is a <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="actualValue"/> is an instance of another type.</exception>
    public static TExpected CheckNotNullAndType<TExpected> (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] [NoEnumeration] object actualValue)
        where TExpected : notnull
    {
      throw new NotImplementedException();
    }

    /// <summary>Checks of the <paramref name="actualValue"/> is of the <paramref name="expectedType"/>.</summary>
    /// <exception cref="ArgumentNullException">The <paramref name="actualValue"/> is a <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="actualValue"/> is an instance of another type.</exception>
    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckNotNullAndType (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] [NoEnumeration] object actualValue,
        Type expectedType)
    {
      CheckNotNullAndType (argumentName, actualValue, expectedType);
    }

    public static object? CheckType ([InvokerParameterName] string argumentName, [NoEnumeration] object? actualValue, Type expectedType)
    {
      throw new NotImplementedException();
    }

    /// <summary>Returns the value itself if it is of the specified type.</summary>
    /// <typeparam name="TExpected"> The type that <paramref name="actualValue"/> must have. </typeparam>
    /// <exception cref="ArgumentException"> 
    ///     <paramref name="actualValue"/> is an instance of another type (which is not a subtype of <typeparamref name="TExpected"/>).</exception>
    /// <exception cref="ArgumentNullException"> 
    ///     <paramref name="actualValue" /> is null and <typeparamref name="TExpected"/> cannot be null. </exception>
    /// <remarks>
    ///   For non-nullable value types, you should use either <see cref="CheckNotNullAndType{TExpected}"/> or pass the type 
    ///   <see cref="Nullable{T}" /> instead.
    /// </remarks>
    public static TExpected CheckType<TExpected> ([InvokerParameterName] string argumentName, [NoEnumeration] object? actualValue)
    {
      throw new NotImplementedException();
    }


    /// <summary>Checks whether <paramref name="actualType"/> is not <see langword="null"/> and can be assigned to <paramref name="expectedType"/>.</summary>
    /// <exception cref="ArgumentNullException">The <paramref name="actualType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="actualType"/> cannot be assigned to <paramref name="expectedType"/>.</exception>
    public static Type CheckNotNullAndTypeIsAssignableFrom (
        [InvokerParameterName] string argumentName,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] Type actualType,
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] Type expectedType)
    {
      throw new NotImplementedException();
    }

    /// <summary>Checks whether <paramref name="actualType"/> can be assigned to <paramref name="expectedType"/>.</summary>
    /// <exception cref="ArgumentException">The <paramref name="actualType"/> cannot be assigned to <paramref name="expectedType"/>.</exception>
    public static Type? CheckTypeIsAssignableFrom (
        [InvokerParameterName] string argumentName, 
        Type? actualType, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] Type expectedType)
    {
      throw new NotImplementedException();
    }

    /// <summary>Checks whether <paramref name="actualType"/> can be assigned to <paramref name="expectedType"/>.</summary>
    /// <exception cref="ArgumentException">The <paramref name="actualType"/> cannot be assigned to <paramref name="expectedType"/>.</exception>
    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugCheckTypeIsAssignableFrom (
        [InvokerParameterName] string argumentName, 
        Type? actualType, 
        [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] Type expectedType)
    {
      throw new NotImplementedException();
    }

    /// <summary>Checks whether all items in <paramref name="collection"/> are of type <paramref name="itemType"/> or a null reference.</summary>
    /// <exception cref="ArgumentException"> If at least one element is not of the specified type or a derived type. </exception>
    public static T CheckItemsType<T> ([InvokerParameterName] string argumentName, T collection, Type itemType)
        where T: ICollection?
    {
      throw new NotImplementedException();
    }

    /// <summary>Checks whether all items in <paramref name="collection"/> are of type <paramref name="itemType"/> and not null references.</summary>
    /// <exception cref="ArgumentException"> If at least one element is not of the specified type or a derived type. </exception>
    /// <exception cref="ArgumentNullException"> If at least one element is a null reference. </exception>
    public static T CheckItemsNotNullAndType<T> ([InvokerParameterName] string argumentName, T collection, Type itemType)
        where T: ICollection?
    {
      throw new NotImplementedException();
    }

    [MustUseReturnValue]
    public static ArgumentException CreateArgumentEmptyException ([InvokerParameterName] string argumentName)
    {
      throw new NotImplementedException();
    }

    [MustUseReturnValue]
    public static ArgumentException CreateArgumentTypeException ([InvokerParameterName] string argumentName, Type? actualType, Type expectedType)
    {
      throw new NotImplementedException();
    }

    [MustUseReturnValue]
    public static ArgumentException CreateArgumentItemTypeException (
        [InvokerParameterName] string argumentName,
        int index,
        Type expectedType,
        Type actualType)
    {
      throw new NotImplementedException();
    }

    [MustUseReturnValue]
    public static ArgumentNullException CreateArgumentItemNullException ([InvokerParameterName] string argumentName, int index)
    {
      throw new NotImplementedException();
    }
  }
}