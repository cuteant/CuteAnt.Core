using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System.Reflection
{
  internal static class CaReflectionExtensions
  {
    #region -- Type --

#if NET40
    /// <summary>Wraps input type into <see cref="TypeInfo"/> structure.</summary>
    /// <param name="type">Input type.</param> <returns>Type info wrapper.</returns>
    public static TypeInfo GetTypeInfo(this Type type)
    {
      return new TypeInfo(type);
    }

    /// <summary>IsConstructedGenericType
	/// http://stackoverflow.com/questions/14476904/distinguish-between-generic-type-that-is-based-on-non-generic-value-type-and-oth
	/// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsConstructedGenericType(this Type t)
    {
      if (!t.IsGenericType || t.ContainsGenericParameters)
      {
        return false;
      }

      if (!t.GetGenericArguments().All(a => !a.IsGenericType || a.IsConstructedGenericType()))
      {
        return false;
      }

      return true;
    }
#endif

    /// <summary>IsNull.</summary>
    /// <param name="typeInfo">Input type.</param> <returns>Type info wrapper.</returns>
    public static bool IsNull(this TypeInfo typeInfo)
    {
#if NET40
      return null == typeInfo.AsType();
#else
      return null == typeInfo;
#endif
    }

    #endregion

    #region -- MemberInfo --

    /// <summary>获取包含该成员的自定义特性的集合。</summary>
    /// <param name="member"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<CustomAttributeData> CustomAttributesEx(this MemberInfo member)
    {
#if !NET40
      return member.CustomAttributes;
#else
      return member.GetCustomAttributesData(); ;
#endif
    }

    #endregion

    #region -- MethodInfo --

#if NET40
    /// <summary>创建指定类型的委托从此方法的</summary>
    /// <param name="method">MethodInfo</param>
    /// <param name="delegateType">创建委托的类型</param>
    /// <returns></returns>
    public static Delegate CreateDelegate(this MethodInfo method, Type delegateType)
    {
      return Delegate.CreateDelegate(delegateType, method);
    }

    /// <summary>使用从此方法的指定目标创建指定类型的委托</summary>
    /// <param name="method">MethodInfo</param>
    /// <param name="delegateType">创建委托的类型</param>
    /// <param name="target">委托面向的对象</param>
    /// <returns></returns>
    public static Delegate CreateDelegate(this MethodInfo method, Type delegateType, Object target)
    {
      return Delegate.CreateDelegate(delegateType, target, method);
    }
#endif

    #endregion

    #region -- CustomAttributeData --

    /// <summary>获取属性的类型。</summary>
    /// <param name="attrdata"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Type AttributeTypeEx(this CustomAttributeData attrdata)
    {
#if !NET40
      return attrdata.AttributeType;
#else
      return attrdata.Constructor.DeclaringType; ;
#endif
    }

    #endregion

    #region -- PropertyInfo --

#if NET40
    /// <summary>返回指定对象的属性值</summary>
    /// <param name="property"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Object GetValue(this PropertyInfo property, Object obj)
    {
      return property.GetValue(obj, null);
    }

    /// <summary>设置指定对象的属性值</summary>
    /// <param name="property"></param>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetValue(this PropertyInfo property, Object obj, Object value)
    {
      property.SetValue(obj, value, null);
    }
#endif

    /// <summary>获取此属性的 get 访问器。</summary>
    /// <param name="property"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static MethodInfo GetMethodEx(this PropertyInfo property)
    {
#if !NET40
      return property.GetMethod;
#else
      return property.GetGetMethod(true);
#endif
    }

    /// <summary>获取此属性的 set 访问器。</summary>
    /// <param name="property"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static MethodInfo SetMethodEx(this PropertyInfo property)
    {
#if !NET40
      return property.SetMethod;
#else
      return property.GetSetMethod(true); ;
#endif
    }

    #endregion

    #region -- ParameterInfo --

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool HasDefaultValueEx(this ParameterInfo pi)
    {
#if NET40
      const string _DBNullType = "System.DBNull";
      return pi.DefaultValue == null || pi.DefaultValue.GetType().FullName != _DBNullType;
#else
      return pi.HasDefaultValue;
#endif
    }

    #endregion
  }
}
