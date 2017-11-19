using System;
using System.Reflection;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  partial class ReflectUtils
  {
    public static bool SupportsExpression { get; set; } = true;
    public static bool SupportsEmit { get; set; } = true;

    static class StaticMemberAccessors<T>
    {
      #region GetValueGetter for PropertyInfo

      private static readonly DictionaryCache<PropertyInfo, MemberGetter<T>> s_propertiesValueGetterCache =
          new DictionaryCache<PropertyInfo, MemberGetter<T>>();
      private static readonly Func<PropertyInfo, MemberGetter<T>> s_propertyInfoGetValueGetterFunc = GetValueGetterInternal;

      public static MemberGetter<T> GetValueGetter(PropertyInfo propertyInfo) =>
          s_propertiesValueGetterCache.GetItem(propertyInfo, s_propertyInfoGetValueGetterFunc);

      private static MemberGetter<T> GetValueGetterInternal(PropertyInfo propertyInfo)
      {
        //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }

        var method = propertyInfo.GetGetMethod(true);
        if (method == null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
        try
        {
          if (method.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? PropertyInvoker<T>.CreateEmitGetter(propertyInfo) : PropertyInvoker<T>.CreateDefaultGetter(propertyInfo);
          }
          else
          {
            return SupportsEmit ? PropertyInvoker<T>.CreateEmitGetter(propertyInfo) :
                   SupportsExpression
                      ? PropertyInvoker<T>.CreateExpressionGetter(propertyInfo)
                      : PropertyInvoker<T>.CreateDefaultGetter(propertyInfo);
          }
        }
        catch
        {
          return PropertyInvoker<T>.CreateDefaultGetter(propertyInfo);
        }
      }

      #endregion

      #region GetValueSetter for PropertyInfo

      private static readonly DictionaryCache<PropertyInfo, MemberSetter<T>> s_propertiesValueSetterCache =
          new DictionaryCache<PropertyInfo, MemberSetter<T>>();
      private static readonly Func<PropertyInfo, MemberSetter<T>> s_propertyInfoGetValueSetterFunc = GetValueSetterInternal;

      public static MemberSetter<T> GetValueSetter(PropertyInfo propertyInfo) =>
          s_propertiesValueSetterCache.GetItem(propertyInfo, s_propertyInfoGetValueSetterFunc);

      private static MemberSetter<T> GetValueSetterInternal(PropertyInfo propertyInfo)
      {
        //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }

        var method = propertyInfo.GetSetMethod(true);
        if (method == null) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
        try
        {
          if (method.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? PropertyInvoker<T>.CreateEmitSetter(propertyInfo) : PropertyInvoker<T>.CreateDefaultSetter(propertyInfo);
          }
          else
          {
            return SupportsEmit ? PropertyInvoker<T>.CreateEmitSetter(propertyInfo) :
                   SupportsExpression
                      ? PropertyInvoker<T>.CreateExpressionSetter(propertyInfo)
                      : PropertyInvoker<T>.CreateDefaultSetter(propertyInfo);
          }
        }
        catch
        {
          return PropertyInvoker<T>.CreateDefaultSetter(propertyInfo);
        }
      }

      #endregion

      #region GetValueGetter for FieldInfo

      private static readonly DictionaryCache<FieldInfo, MemberGetter<T>> s_fieldsValueGetterCache =
          new DictionaryCache<FieldInfo, MemberGetter<T>>();
      private static readonly Func<FieldInfo, MemberGetter<T>> s_fieldInfoGetValueGetterFunc = GetValueGetterInternal;

      public static MemberGetter<T> GetValueGetter(FieldInfo fieldInfo) =>
          s_fieldsValueGetterCache.GetItem(fieldInfo, s_fieldInfoGetValueGetterFunc);

      private static MemberGetter<T> GetValueGetterInternal(FieldInfo fieldInfo)
      {
        try
        {
          if (fieldInfo.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? FieldInvoker<T>.CreateEmitGetter(fieldInfo) : FieldInvoker<T>.CreateDefaultGetter(fieldInfo);
          }
          else
          {
            return SupportsEmit ? FieldInvoker<T>.CreateEmitGetter(fieldInfo) :
                   SupportsExpression
                      ? FieldInvoker<T>.CreateExpressionGetter(fieldInfo)
                      : FieldInvoker<T>.CreateDefaultGetter(fieldInfo);
          }
        }
        catch
        {
          return FieldInvoker<T>.CreateDefaultGetter(fieldInfo);
        }
      }

      #endregion

      #region GetValueSetter for FieldInfo

      private static readonly DictionaryCache<FieldInfo, MemberSetter<T>> s_fieldsValueSetterCache =
          new DictionaryCache<FieldInfo, MemberSetter<T>>();
      private static readonly Func<FieldInfo, MemberSetter<T>> s_fieldInfoGetValueSetterFunc = GetValueSetterInternal;

      public static MemberSetter<T> GetValueSetter(FieldInfo fieldInfo) =>
          s_fieldsValueSetterCache.GetItem(fieldInfo, s_fieldInfoGetValueSetterFunc);

      private static MemberSetter<T> GetValueSetterInternal(FieldInfo fieldInfo)
      {
        try
        {
          if (fieldInfo.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? FieldInvoker<T>.CreateEmitSetter(fieldInfo) : FieldInvoker<T>.CreateDefaultSetter(fieldInfo);
          }
          else
          {
            return SupportsEmit ? FieldInvoker<T>.CreateEmitSetter(fieldInfo) :
                   SupportsExpression
                      ? FieldInvoker<T>.CreateExpressionSetter(fieldInfo)
                      : FieldInvoker<T>.CreateDefaultSetter(fieldInfo);
          }
        }
        catch
        {
          return FieldInvoker<T>.CreateDefaultSetter(fieldInfo);
        }
      }

      #endregion
    }
  }
}