using System;
using System.Diagnostics;
using System.Reflection;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  #region -- MemberGetter / MemberSetter --

  /// <summary>SetMemberAction</summary>
  /// <param name="instance"></param>
  /// <param name="value"></param>
  public delegate void MemberSetter(object instance, object value);
  /// <summary>SetMemberAction</summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="instance"></param>
  /// <param name="value"></param>
  public delegate void MemberSetter<T>(T instance, object value);

  /// <summary>SetMemberRefAction</summary>
  /// <param name="instance"></param>
  /// <param name="propertyValue"></param>
  public delegate void MemberRefSetter(ref object instance, object propertyValue);
  /// <summary>SetMemberRefAction</summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="instance"></param>
  /// <param name="value"></param>
  public delegate void MemberRefSetter<T>(ref T instance, object value);

  #endregion

  partial class ReflectUtils
  {
    #region -- MemberSetter.IsEmpty --

    public static bool IsEmpty(this MemberSetter setter) => object.ReferenceEquals(TypeAccessorHelper.EmptyMemberSetter, setter);
    public static bool IsEmpty<T>(this MemberSetter<T> setter) => object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberSetter, setter);
    public static bool IsNullOrEmpty(this MemberSetter setter) => null == setter || object.ReferenceEquals(TypeAccessorHelper.EmptyMemberSetter, setter);
    public static bool IsNullOrEmpty<T>(this MemberSetter<T> setter) => null == setter || object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberSetter, setter);

    #endregion

    #region -- GetValueSetter for PropertyInfo --

    private static readonly CachedReadConcurrentDictionary<PropertyInfo, MemberSetter> s_propertiesValueSetterCache =
        new CachedReadConcurrentDictionary<PropertyInfo, MemberSetter>();
    private static readonly Func<PropertyInfo, MemberSetter> s_propertyInfoGetValueSetterFunc = GetValueSetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberSetter GetValueSetter(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return s_propertiesValueSetterCache.GetOrAdd(propertyInfo, s_propertyInfoGetValueSetterFunc);
    }

    /// <summary>GetValueSetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    private static MemberSetter GetValueSetterInternal(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }

      var method = propertyInfo.GetSetMethod(true);
      if (method == null) { return TypeAccessorHelper.EmptyMemberSetter; }
      try
      {
        if (method.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? PropertyInvoker.CreateEmitSetter(propertyInfo) : PropertyInvoker.CreateDefaultSetter(propertyInfo);
        }
        else
        {
          return SupportsEmit ? PropertyInvoker.CreateEmitSetter(propertyInfo) :
                 SupportsExpression
                    ? PropertyInvoker.CreateExpressionSetter(propertyInfo)
                    : PropertyInvoker.CreateDefaultSetter(propertyInfo);
        }
      }
      catch
      {
        return PropertyInvoker.CreateDefaultSetter(propertyInfo);
      }
    }

    #endregion

    #region -- GetValueSetter<T> for PropertyInfo --

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberSetter<T> GetValueSetter<T>(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return StaticMemberAccessors<T>.GetValueSetter(propertyInfo);
    }

    #endregion

    #region -- GetValueSetter for FieldInfo --

    private static readonly CachedReadConcurrentDictionary<FieldInfo, MemberSetter> s_fieldsValueSetterCache =
        new CachedReadConcurrentDictionary<FieldInfo, MemberSetter>();
    private static readonly Func<FieldInfo, MemberSetter> s_fieldInfoGetValueSetterFunc = GetValueSetterInternal;

    /// <summary>GetValueSetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberSetter GetValueSetter(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return s_fieldsValueSetterCache.GetOrAdd(fieldInfo, s_fieldInfoGetValueSetterFunc);
    }

    private static MemberSetter GetValueSetterInternal(FieldInfo fieldInfo)
    {
      try
      {
        if (fieldInfo.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? FieldInvoker.CreateEmitSetter(fieldInfo) : FieldInvoker.CreateDefaultSetter(fieldInfo);
        }
        else
        {
          return SupportsEmit ? FieldInvoker.CreateEmitSetter(fieldInfo) :
                 SupportsExpression
                    ? FieldInvoker.CreateExpressionSetter(fieldInfo)
                    : FieldInvoker.CreateDefaultSetter(fieldInfo);
        }
      }
      catch
      {
        return FieldInvoker.CreateDefaultSetter(fieldInfo);
      }
    }

    #endregion

    #region -- GetValueSetter<T> for FieldInfo --

    /// <summary>GetValueSetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberSetter<T> GetValueSetter<T>(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return StaticMemberAccessors<T>.GetValueSetter(fieldInfo);
    }

    #endregion

    #region -- SetValue --

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static void SetMemberInfoValue(this object target, string name, object value)
    {
      if (!TrySetMemberInfoValue(target, name, value))
      {
        var type = GetTypeInternal(ref target);
        throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
      }
    }

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static bool TrySetMemberInfoValue(this object target, string name, object value)
    {
      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetTypeInternal(ref target);
      var pi = GetTypeProperty(type, name);
      if (pi != null) { SetPropertyInfoValue(target, pi, value); return true; }

      var fi = GetTypeField(type, name);
      if (fi != null) { SetFieldInfoValue(target, fi, value); return true; }

      return false;
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static void SetMemberInfoValue(this object target, MemberInfo member, object value)
    {
      if (member is PropertyInfo property) { SetPropertyInfoValue(target, property, value); return; }
      if (member is FieldInfo field) { SetFieldInfoValue(target, field, value); return; }

      throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static bool TrySetMemberInfoValue(this object target, MemberInfo member, object value)
    {
      if (member is PropertyInfo property) { SetPropertyInfoValue(target, property, value); return true; }
      if (member is FieldInfo field) { SetFieldInfoValue(target, field, value); return true; }

      return false;
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static void SetPropertyInfoValue(this object target, PropertyInfo property, object value)
    {
      GetValueSetter(property).Invoke(target, value);
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static bool TrySetPropertyInfoValue(this object target, PropertyInfo property, object value)
    {
      var setter = GetValueSetter(property);
      if (null == setter || setter.IsEmpty()) { return false; }
      setter(target, value);
      return true;
    }

    /// <summary>设置目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    public static void SetFieldInfoValue(this object target, FieldInfo field, object value)
    {
      GetValueSetter(field).Invoke(target, value);
    }

    #endregion
  }
}