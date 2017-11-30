using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
  internal static class TypeAccessorHelper
  {
    #region @@ Fields @@

    internal const string c_field = "Field";
    internal const string c_property = "Property";
    private static readonly Type s_objectType = TypeConstants.ObjectType;
    internal static readonly Type s_voidType = TypeConstants.VoidType;
    private static readonly Type[] s_dynamicGetMethodArgs = { s_objectType };
    private static readonly Type[] s_dynamicSetMethodArgs = { s_objectType, s_objectType };
    private static readonly ConcurrentDictionary<Type, MethodInfo> s_fieldValueConvertCache = new ConcurrentDictionary<Type, MethodInfo>();

    internal const string InstanceParameterName = "i";
    internal const string ArgumentParameterName = "a";
    internal const string SourceParameterName = "source";
    internal const string ValueParameterName = "value";

    internal static readonly Type MemberGetterType = typeof(MemberGetter);
    internal static readonly Type MemberSetterType = typeof(MemberSetter);

    internal static readonly MemberGetter EmptyMemberGetter = obj => null;
    internal static readonly MemberSetter EmptyMemberSetter = (obj, v) => { };

    #endregion

    #region == GetCastOrConvertExpression ==

    internal static Expression GetCastOrConvertExpression(Expression expression, Type targetType)
    {
      Expression result;
      var expressionType = expression.Type;

      if (targetType.IsAssignableFrom(expressionType))
      {
        result = expression;
      }
      else
      {
        // Check if we can use the as operator for casting or if we must use the convert method
        if (targetType.IsValueType && !targetType.IsNullableType())
        {
          result = Expression.Convert(expression, targetType);
        }
        else
        {
          result = Expression.TypeAs(expression, targetType);
        }
      }

      return result;
    }

    #endregion

    #region == GetFieldMethod ==

    internal static MethodInfo GetFieldValueConvertMethod(Type type)
    {
      if (s_fieldValueConvertCache.TryGetValue(type, out MethodInfo mi)) { return mi; }

      var name = "To" + type.Name;
      mi = typeof(Convert).GetMethod(name, s_dynamicGetMethodArgs);
      s_fieldValueConvertCache.TryAdd(type, mi);
      return mi;
    }

    #endregion

    #region == CreateDynamicGetMethod ==


    internal static DynamicMethod CreateDynamicGetMethod(MemberInfo memberInfo)
    {
      var memberType = memberInfo is FieldInfo ? c_field : c_property;
      var name = $"_Get{memberType}_{memberInfo.Name}_";

      var declaringType = memberInfo.DeclaringType;
      return !declaringType.IsInterface
          ? new DynamicMethod(name, s_objectType, s_dynamicGetMethodArgs, declaringType, true)
          : new DynamicMethod(name, s_objectType, s_dynamicGetMethodArgs, memberInfo.Module, true);
    }

    #endregion

    #region == CreateDynamicSetMethod ==

    internal static DynamicMethod CreateDynamicSetMethod(MemberInfo memberInfo)
    {
      var memberType = memberInfo is FieldInfo ? c_field : c_property;
      var name = $"_Set{memberType}_{memberInfo.Name}_";

      var declaringType = memberInfo.DeclaringType;
      return !declaringType.IsInterface
          ? new DynamicMethod(name, s_voidType, s_dynamicSetMethodArgs, declaringType, true)
          : new DynamicMethod(name, s_voidType, s_dynamicSetMethodArgs, memberInfo.Module, true);
    }

    #endregion
  }

  internal static class TypeAccessorHelper<T>
  {
    #region @@ Fields @@

    internal static readonly Type ThisType = typeof(T);

    private const string c_field = TypeAccessorHelper.c_field;
    private const string c_property = TypeAccessorHelper.c_property;
    private static readonly Type s_objectType = TypeConstants.ObjectType;
    private static readonly Type s_voidType = TypeAccessorHelper.s_voidType;

    private static readonly Type[] s_dynamicGetMethodArgs = new Type[] { ThisType };
    private static readonly Type[] s_dynamicSetMethodArgs = new Type[] { ThisType, TypeConstants.ObjectType };

    internal static readonly Type MemberGetterType = typeof(MemberGetter<T>);
    internal static readonly Type MemberSetterType = typeof(MemberSetter<T>);

    internal static readonly MemberGetter<T> EmptyMemberGetter = obj => null;
    internal static readonly MemberSetter<T> EmptyMemberSetter = (obj, v) => { };

    #endregion

    #region == CreateDynamicGetMethod ==

    internal static DynamicMethod CreateDynamicGetMethod(MemberInfo memberInfo)
    {
      var memberType = memberInfo is FieldInfo ? c_field : c_property;
      var name = $"_Get{memberType}_{memberInfo.Name}_";

      var declaringType = memberInfo.DeclaringType;
      return !declaringType.IsInterface
          ? new DynamicMethod(name, s_objectType, s_dynamicGetMethodArgs, declaringType, true)
          : new DynamicMethod(name, s_objectType, s_dynamicGetMethodArgs, memberInfo.Module, true);
    }

    #endregion

    #region == CreateDynamicSetMethod ==

    internal static DynamicMethod CreateDynamicSetMethod(MemberInfo memberInfo)
    {
      var memberType = memberInfo is FieldInfo ? c_field : c_property;
      var name = $"_Set{memberType}_{memberInfo.Name}_";

      var declaringType = memberInfo.DeclaringType;
      return !declaringType.IsInterface
          ? new DynamicMethod(name, s_voidType, s_dynamicSetMethodArgs, declaringType, true)
          : new DynamicMethod(name, s_voidType, s_dynamicSetMethodArgs, memberInfo.Module, true);
    }

    #endregion
  }
}
