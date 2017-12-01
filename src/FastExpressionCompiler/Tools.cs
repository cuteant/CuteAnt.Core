/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastExpressionCompiler
{
  // Helpers targeting the performance.
  // Extensions method names may be a bit funny (non standard), 
  // it is done to prevent conflicts with helpers with standard names
  internal static class Tools
  {
#if NET40
    public static bool IsNullable(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
#else
    public static bool IsNullable(this Type type) => type.GetTypeInfo().IsGenericType && type.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>);
#endif

#if NET40
    public static Type GetWrappedTypeFromNullable(this Type type) => type.GenericTypeArguments()[0];
#else
    public static Type GetWrappedTypeFromNullable(this Type type) => type.GetTypeInfo().GenericTypeArguments[0];
#endif

#if NET40
    public static ConstructorInfo GetConstructorByArgs(this Type type, params Type[] args) => type.GetTypeDeclaredConstructors().GetFirst(c => c.GetParameters().Project(p => p.ParameterType).SequenceEqual(args));
#else
    public static ConstructorInfo GetConstructorByArgs(this Type type, params Type[] args) => type.GetTypeInfo().DeclaredConstructors.GetFirst(c => c.GetParameters().Project(p => p.ParameterType).SequenceEqual(args));
#endif

    public static Expression ToExpression(this object exprObj) => exprObj == null ? null : exprObj as Expression ?? ((ExpressionInfo)exprObj).ToExpression();

    public static ExpressionType GetNodeType(this object exprObj) => (exprObj as Expression)?.NodeType ?? ((ExpressionInfo)exprObj).NodeType;

    public static Type GetResultType(this object exprObj) => (exprObj as Expression)?.Type ?? ((ExpressionInfo)exprObj).Type;

    private static class EmptyArray<T>
    {
#if NET_4_5_GREATER
      public static readonly T[] Value = Array.Empty<T>();
#else
      public static readonly T[] Value = new T[0];
#endif
    }

    public static T[] Empty<T>() => EmptyArray<T>.Value;

    public static T[] WithLast<T>(this T[] source, T value)
    {
      if (source == null || source.Length == 0) { return new[] { value }; }
      if (source.Length == 1) { return new[] { source[0], value }; }
      if (source.Length == 2) { return new[] { source[0], source[1], value }; }
      var sourceLength = source.Length;
      var result = new T[sourceLength + 1];
      Array.Copy(source, result, sourceLength);
      result[sourceLength] = value;
      return result;
    }

    public static Type[] GetParamExprTypes(IList<ParameterExpression> paramExprs)
    {
      if (paramExprs == null || paramExprs.Count == 0) { return Empty<Type>(); }

      if (paramExprs.Count == 1) { return new[] { paramExprs[0].GetResultType() }; }

      var paramTypes = new Type[paramExprs.Count];
      for (var i = 0; i < paramTypes.Length; i++)
      {
        paramTypes[i] = paramExprs[i].GetResultType();
      }
      return paramTypes;
    }

    public static Type[] GetParamExprTypes(IList<object> paramExprs)
    {
      if (paramExprs == null || paramExprs.Count == 0) { return Empty<Type>(); }

      if (paramExprs.Count == 1) { return new[] { paramExprs[0].GetResultType() }; }

      var paramTypes = new Type[paramExprs.Count];
      for (var i = 0; i < paramTypes.Length; i++)
      {
        paramTypes[i] = paramExprs[i].GetResultType();
      }
      return paramTypes;
    }

    public static Type GetFuncOrActionType(Type[] paramTypes, Type returnType)
    {
      if (returnType == typeof(void))
      {
        switch (paramTypes.Length)
        {
          case 0: return typeof(Action);
          case 1: return typeof(Action<>).MakeGenericType(paramTypes);
          case 2: return typeof(Action<,>).MakeGenericType(paramTypes);
          case 3: return typeof(Action<,,>).MakeGenericType(paramTypes);
          case 4: return typeof(Action<,,,>).MakeGenericType(paramTypes);
          case 5: return typeof(Action<,,,,>).MakeGenericType(paramTypes);
          case 6: return typeof(Action<,,,,,>).MakeGenericType(paramTypes);
          case 7: return typeof(Action<,,,,,,>).MakeGenericType(paramTypes);
          default:
            throw new NotSupportedException($"Action with so many ({paramTypes.Length}) parameters is not supported!");
        }
      }

      paramTypes = paramTypes.WithLast(returnType);
      switch (paramTypes.Length)
      {
        case 1: return typeof(Func<>).MakeGenericType(paramTypes);
        case 2: return typeof(Func<,>).MakeGenericType(paramTypes);
        case 3: return typeof(Func<,,>).MakeGenericType(paramTypes);
        case 4: return typeof(Func<,,,>).MakeGenericType(paramTypes);
        case 5: return typeof(Func<,,,,>).MakeGenericType(paramTypes);
        case 6: return typeof(Func<,,,,,>).MakeGenericType(paramTypes);
        case 7: return typeof(Func<,,,,,,>).MakeGenericType(paramTypes);
        case 8: return typeof(Func<,,,,,,,>).MakeGenericType(paramTypes);
        default:
          throw new NotSupportedException($"Func with so many ({paramTypes.Length}) parameters is not supported!");
      }
    }
    public static int GetFirstIndex<T>(this IList<T> source, object item)
    {
      if (source == null || source.Count == 0)
        return -1;
      var count = source.Count;
      if (count == 1)
        return ReferenceEquals(source[0], item) ? 0 : -1;
      for (var i = 0; i < count; ++i)
        if (ReferenceEquals(source[i], item))
          return i;
      return -1;
    }

    public static int GetFirstIndex<T>(this T[] source, Func<T, bool> predicate)
    {
      if (source == null || source.Length == 0) { return -1; }
      if (source.Length == 1) { return predicate(source[0]) ? 0 : -1; }
      for (var i = 0; i < source.Length; ++i)
      {
        if (predicate(source[i])) { return i; }
      }
      return -1;
    }

    public static T GetFirst<T>(this IEnumerable<T> source)
    {
      var arr = source as T[];
      return arr == null
          ? source.FirstOrDefault()
          : arr.Length != 0 ? arr[0] : default(T);
    }

    public static T GetFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
      var arr = source as T[];
      if (arr == null) { return source.FirstOrDefault(predicate); }

      var index = arr.GetFirstIndex(predicate);
      return index == -1 ? default(T) : arr[index];
    }

    public static R[] Project<T, R>(this T[] source, Func<T, R> project)
    {
      if (source == null || source.Length == 0) { return Empty<R>(); }

      if (source.Length == 1) { return new[] { project(source[0]) }; }

      var result = new R[source.Length];
      for (var i = 0; i < result.Length; ++i)
      {
        result[i] = project(source[i]);
      }
      return result;
    }
  }
}
