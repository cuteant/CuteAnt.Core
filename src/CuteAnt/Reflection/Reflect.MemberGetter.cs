using System;
using System.Diagnostics;
using System.Reflection;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  #region -- MemberGetter --

  /// <summary>GetMemberFunc</summary>
  /// <param name="instance"></param>
  /// <returns></returns>
  public delegate object MemberGetter(object instance);
  /// <summary>GetMemberFunc</summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="instance"></param>
  /// <returns></returns>
  public delegate object MemberGetter<T>(T instance);

  #endregion

  partial class ReflectUtils
  {
    #region -- MemberGetter.IsEmpty --

    public static bool IsEmpty(this MemberGetter getter) => object.ReferenceEquals(TypeAccessorHelper.EmptyMemberGetter, getter);
    public static bool IsEmpty<T>(this MemberGetter<T> getter) => object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberGetter, getter);
    public static bool IsNullOrEmpty(this MemberGetter getter) => null == getter || object.ReferenceEquals(TypeAccessorHelper.EmptyMemberGetter, getter);
    public static bool IsNullOrEmpty<T>(this MemberGetter<T> getter) => null == getter || object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberGetter, getter);

    #endregion

    #region -- GetValueGetter for PropertyInfo --

    private static readonly DictionaryCache<PropertyInfo, MemberGetter> s_propertiesValueGetterCache =
        new DictionaryCache<PropertyInfo, MemberGetter>();
    private static readonly Func<PropertyInfo, MemberGetter> s_propertyInfoGetValueGetterFunc = GetValueGetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberGetter GetValueGetter(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return s_propertiesValueGetterCache.GetItem(propertyInfo, s_propertyInfoGetValueGetterFunc);
    }

    private static MemberGetter GetValueGetterInternal(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }

      var method = propertyInfo.GetGetMethod(true);
      if (method == null) { return TypeAccessorHelper.EmptyMemberGetter; }
      try
      {
        if (method.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? PropertyInvoker.CreateEmitGetter(propertyInfo) : PropertyInvoker.CreateDefaultGetter(propertyInfo);
        }
        else
        {
          return SupportsEmit ? PropertyInvoker.CreateEmitGetter(propertyInfo) :
                 SupportsExpression
                    ? PropertyInvoker.CreateExpressionGetter(propertyInfo)
                    : PropertyInvoker.CreateDefaultGetter(propertyInfo);
        }
      }
      catch
      {
        return PropertyInvoker.CreateDefaultGetter(propertyInfo);
      }
    }

    #endregion

    #region -- GetValueGetter<T> for PropertyInfo --

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberGetter<T> GetValueGetter<T>(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return StaticMemberAccessors<T>.GetValueGetter(propertyInfo);
    }

    #endregion

    #region -- GetValueGetter for FieldInfo --

    private static readonly DictionaryCache<FieldInfo, MemberGetter> s_fieldsValueGetterCache =
        new DictionaryCache<FieldInfo, MemberGetter>();
    private static readonly Func<FieldInfo, MemberGetter> s_fieldInfoGetValueGetterFunc = GetValueGetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberGetter GetValueGetter(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return s_fieldsValueGetterCache.GetItem(fieldInfo, s_fieldInfoGetValueGetterFunc);
    }

    private static MemberGetter GetValueGetterInternal(FieldInfo fieldInfo)
    {
      try
      {
        if (fieldInfo.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? FieldInvoker.CreateEmitGetter(fieldInfo) : FieldInvoker.CreateDefaultGetter(fieldInfo);
        }
        else
        {
          return SupportsEmit ? FieldInvoker.CreateEmitGetter(fieldInfo) :
                 SupportsExpression
                    ? FieldInvoker.CreateExpressionGetter(fieldInfo)
                    : FieldInvoker.CreateDefaultGetter(fieldInfo);
        }
      }
      catch
      {
        return FieldInvoker.CreateDefaultGetter(fieldInfo);
      }
    }

    #endregion

    #region -- GetValueGetter<T> for FieldInfo --

    /// <summary>GetValueGetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberGetter<T> GetValueGetter<T>(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return StaticMemberAccessors<T>.GetValueGetter(fieldInfo);
    }

    #endregion

    #region -- GetValue --

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="throwOnError">出错时是否抛出异常</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static object GetMemberInfoValue(this object target, string name, bool throwOnError = true)
    {
      if (target == null) { throw new ArgumentNullException(nameof(target)); }
      ValidationHelper.ArgumentNullOrEmpty(name, nameof(name));

      if (TryGetMemberInfoValue(target, name, out object value)) { return value; }

      if (!throwOnError) { return null; }

      var type = GetTypeInternal(ref target);
      throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
    }

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns>是否成功获取数值</returns>
    public static bool TryGetMemberInfoValue(this object target, string name, out object value)
    {
      if (target == null) { throw new ArgumentNullException(nameof(target)); }

      value = null;

      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetTypeInternal(ref target);
      var pi = GetTypeProperty(type, name);
      if (pi != null)
      {
        return TryGetPropertyInfoValue(target, pi, out value);
      }

      var fi = GetTypeField(type, name);
      if (fi != null)
      {
        value = GetFieldInfoValue(target, fi);
        return true;
      }

      return false;
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <returns></returns>
    public static object GetMemberInfoValue(this object target, MemberInfo member)
    {
      if (member is PropertyInfo property) { return GetPropertyInfoValue(target, property); }
      if (member is FieldInfo field) { return GetFieldInfoValue(target, field); }

      throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetMemberInfoValue(this object target, MemberInfo member, out object value)
    {
      if (member is PropertyInfo property)
      {
        return TryGetPropertyInfoValue(target, property, out value);
      }
      if (member is FieldInfo field)
      {
        value = GetFieldInfoValue(target, field);
        return true;
      }

      value = null;
      return false;
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <returns></returns>
    public static object GetPropertyInfoValue(this object target, PropertyInfo property)
    {
      return GetValueGetter(property).Invoke(target);
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetPropertyInfoValue(this object target, PropertyInfo property, out object value)
    {
      var getter = GetValueGetter(property);

      if (null == getter || getter.IsEmpty())
      {
        value = null;
        return false;
      }
      else
      {
        value = getter(target);
        return true;
      }
    }

    /// <summary>获取目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <returns></returns>
    public static object GetFieldInfoValue(this object target, FieldInfo field)
    {
      return GetValueGetter(field).Invoke(target);
    }

    #endregion
  }
}