using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
  public static class FieldInvoker
  {
    #region @@ Fields @@

    private static readonly Type s_objectType = TypeUtils._.Object;

    #endregion

    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter CreateDefaultGetter(FieldInfo fieldInfo) => fieldInfo.GetValue;
    public static MemberSetter CreateDefaultSetter(FieldInfo fieldInfo) => fieldInfo.SetValue;

    #endregion

    #region -- CreateExpressionGetter --

    public static MemberGetter CreateExpressionGetter(FieldInfo fieldInfo)
    {
      var fieldDeclaringType = fieldInfo.GetDeclaringType();

      var oInstanceParam = Expression.Parameter(s_objectType, TypeAccessorHelper.SourceParameterName);
      var instanceParam = TypeAccessorHelper.GetCastOrConvertExpression(oInstanceParam, fieldDeclaringType);

      var exprCallFieldGetFn = Expression.Field(instanceParam, fieldInfo);
      //var oExprCallFieldGetFn = this.GetCastOrConvertExpression(exprCallFieldGetFn, s_objectType);
      var oExprCallFieldGetFn = Expression.Convert(exprCallFieldGetFn, s_objectType);

      return Expression.Lambda<MemberGetter>(oExprCallFieldGetFn, oInstanceParam).Compile();
    }

    #endregion

    #region -- CreateExpressionSetter --

    public static MemberSetter CreateExpressionSetter(FieldInfo fieldInfo)
    {
      var fieldDeclaringType = fieldInfo.DeclaringType;

      var sourceParameter = Expression.Parameter(s_objectType, TypeAccessorHelper.SourceParameterName);
      var valueParameter = Expression.Parameter(s_objectType, TypeAccessorHelper.ValueParameterName);

      var sourceExpression = TypeAccessorHelper.GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

      var fieldExpression = Expression.Field(sourceExpression, fieldInfo);

      var valueExpression = TypeAccessorHelper.GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

      var genericSetFieldMethodInfo = s_setFieldMethod.MakeGenericMethod(fieldExpression.Type);

      var setFieldMethodCallExpression = Expression.Call(null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

      return Expression.Lambda<MemberSetter>(setFieldMethodCallExpression, sourceParameter, valueParameter).Compile();
    }

    #endregion

    #region -- CreateEmitGetter --

    public static MemberGetter CreateEmitGetter(FieldInfo fieldInfo)
    {
      var getter = TypeAccessorHelper.CreateDynamicGetMethod(fieldInfo);

      var gen = getter.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);

      var declaringType = fieldInfo.GetDeclaringType();
      if (declaringType.IsValueType)
      {
        gen.Emit(OpCodes.Unbox, declaringType);
      }
      else
      {
        gen.Emit(OpCodes.Castclass, declaringType);
      }

      gen.Emit(OpCodes.Ldfld, fieldInfo);

      var fieldType = fieldInfo.FieldType;
      if (fieldType.IsValueType)
      {
        gen.Emit(OpCodes.Box, fieldType);
      }

      gen.Emit(OpCodes.Ret);

      return (MemberGetter)getter.CreateDelegate(TypeAccessorHelper.MemberGetterType);
    }

    #endregion

    #region -- CreateEmitSetter --

    public static MemberSetter CreateEmitSetter(FieldInfo fieldInfo)
    {
      var setter = TypeAccessorHelper.CreateDynamicSetMethod(fieldInfo);

      var gen = setter.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);

      var declaringType = fieldInfo.GetDeclaringType();
      if (declaringType.IsValueType)
      {
        gen.Emit(OpCodes.Unbox, declaringType);
      }
      else
      {
        gen.Emit(OpCodes.Castclass, declaringType);
      }

      gen.Emit(OpCodes.Ldarg_1);

      var fieldType = fieldInfo.FieldType;
      var method = TypeAccessorHelper.GetFieldValueConvertMethod(fieldType);
      if (method != null)
      {
        gen.Call(method);
      }
      else
      {
        gen.Emit(fieldType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, fieldType);
      }

      gen.Emit(OpCodes.Stfld, fieldInfo);
      gen.Emit(OpCodes.Ret);

      return (MemberSetter)setter.CreateDelegate(TypeAccessorHelper.MemberSetterType);
    }

    #endregion

    #region ** Helper **

    private static readonly MethodInfo s_setFieldMethod = typeof(FieldInvoker).GetStaticMethod("SetField");

    internal static void SetField<TValue>(ref TValue field, TValue newValue)
    {
      field = newValue;
    }

    #endregion
  }
  public static class FieldInvoker<T>
  {
    #region @@ Fields @@

    private static readonly Type s_objectType = TypeUtils._.Object;

    #endregion

    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter<T> CreateDefaultGetter(FieldInfo fieldInfo) => obj => fieldInfo.GetValue(obj);
    public static MemberSetter<T> CreateDefaultSetter(FieldInfo fieldInfo) => (obj, v) => fieldInfo.SetValue(obj, v);

    #endregion

    #region -- CreateExpressionGetter --

    public static MemberGetter<T> CreateExpressionGetter(FieldInfo fieldInfo)
    {
      var fieldDeclaringType = fieldInfo.GetDeclaringType();
      var thisType = TypeAccessorHelper<T>.ThisType;
      var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
      var field = thisType != fieldDeclaringType
          ? Expression.Field(Expression.TypeAs(instance, fieldDeclaringType), fieldInfo)
          : Expression.Field(instance, fieldInfo);
      var convertField = Expression.TypeAs(field, s_objectType);
      return Expression.Lambda<MemberGetter<T>>(convertField, instance).Compile();
    }

    #endregion

    #region -- CreateExpressionSetter --

    public static MemberSetter<T> CreateExpressionSetter(FieldInfo fieldInfo)
    {
      var thisType = TypeAccessorHelper<T>.ThisType;
      var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
      var argument = Expression.Parameter(s_objectType, TypeAccessorHelper.ArgumentParameterName);

      var declaringType = fieldInfo.GetDeclaringType();
      var field = thisType != declaringType
          ? Expression.Field(Expression.TypeAs(instance, declaringType), fieldInfo)
          : Expression.Field(instance, fieldInfo);

      var setterCall = Expression.Assign(
          field,
          TypeAccessorHelper.GetCastOrConvertExpression(argument, fieldInfo.FieldType)); //Expression.Convert(argument, fieldInfo.FieldType));

      return Expression.Lambda<MemberSetter<T>>(setterCall, instance, argument).Compile();
    }

    #endregion

    #region -- CreateExpressionRefSetter --

    public static MemberRefSetter<T> CreateExpressionRefSetter(FieldInfo fieldInfo)
    {
      var thisType = TypeAccessorHelper<T>.ThisType;
      var instance = Expression.Parameter(thisType.MakeByRefType(), TypeAccessorHelper.InstanceParameterName);
      var argument = Expression.Parameter(s_objectType, TypeAccessorHelper.ArgumentParameterName);

      var field = thisType != fieldInfo.DeclaringType
          ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
          : Expression.Field(instance, fieldInfo);

      var setterCall = Expression.Assign(
          field,
          Expression.Convert(argument, fieldInfo.FieldType));

      return Expression.Lambda<MemberRefSetter<T>>(setterCall, instance, argument).Compile();
    }

    #endregion

    #region -- CreateEmitGetter --

    public static MemberGetter<T> CreateEmitGetter(FieldInfo fieldInfo)
    {
      var getter = TypeAccessorHelper<T>.CreateDynamicGetMethod(fieldInfo);

      var gen = getter.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);

      gen.Emit(OpCodes.Ldfld, fieldInfo);

      var fieldType = fieldInfo.FieldType;
      if (fieldType.IsValueType)
      {
        gen.Emit(OpCodes.Box, fieldType);
      }

      gen.Emit(OpCodes.Ret);

      return (MemberGetter<T>)getter.CreateDelegate(TypeAccessorHelper<T>.MemberGetterType);
    }

    #endregion

    #region -- CreateEmitSetter --

    public static MemberSetter<T> CreateEmitSetter(FieldInfo fieldInfo)
    {
      var setter = TypeAccessorHelper<T>.CreateDynamicSetMethod(fieldInfo);

      var gen = setter.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);

      var thisType = TypeAccessorHelper<T>.ThisType;
      var declaringType = fieldInfo.GetDeclaringType();
      if (declaringType.IsValueType)
      {
        if (thisType != declaringType) { gen.Emit(OpCodes.Unbox, declaringType); }
      }
      else
      {
        if (thisType != declaringType) { gen.Emit(OpCodes.Castclass, declaringType); }
      }

      gen.Emit(OpCodes.Ldarg_1);

      var fieldType = fieldInfo.FieldType;
      var method = TypeAccessorHelper.GetFieldValueConvertMethod(fieldType);
      if (method != null)
      {
        gen.Call(method);
      }
      else
      {
        gen.Emit(fieldType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, fieldType);
      }

      gen.Emit(OpCodes.Stfld, fieldInfo);
      gen.Emit(OpCodes.Ret);

      return (MemberSetter<T>)setter.CreateDelegate(TypeAccessorHelper<T>.MemberSetterType);
    }

    #endregion
  }
}
