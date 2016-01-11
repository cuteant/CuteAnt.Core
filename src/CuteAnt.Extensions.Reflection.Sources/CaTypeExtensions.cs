using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET40
using System.Reflection;
using System.Runtime.CompilerServices;
#endif

namespace System
{
  internal static class CaTypeExtensions
  {
    /// <summary>获取此类型通用类型参数的数组。</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Type[] GenericTypeArgumentsEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().GenericTypeArguments;
#else
      return type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericArguments() : Type.EmptyTypes;
#endif
    }

    /// <summary>获取当前类型的泛型参数的数组。</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Type[] GenericTypeParametersEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().GenericTypeParameters;
#else
      return type.IsGenericTypeDefinition ? type.GetGenericArguments() : Type.EmptyTypes;
#endif
    }

    /// <summary>获取当前类型实现的接口的集合。</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<Type> ImplementedInterfacesEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().ImplementedInterfaces;
#else
      return type.GetInterfaces();
#endif
    }

    /// <summary>确定 Type 的实例是否可以从指定 Type 的实例分配。</summary>
    /// <param name="type"></param>
    /// <param name="c"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Boolean IsAssignableFromEx(this Type type, Type c)
    {
#if !NET40
      return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
#else
      return type.IsAssignableFrom(c);
#endif
    }
  }
}
