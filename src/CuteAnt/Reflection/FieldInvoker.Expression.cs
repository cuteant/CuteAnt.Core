using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CuteAnt.Reflection
{
  public static partial class FieldInvoker
  {
    #region -- CreateExpressionGetter --

    public static MemberGetter CreateExpressionGetter(FieldInfo fieldInfo)
    {
      var fieldDeclaringType = fieldInfo.DeclaringType;

      var oInstanceParam = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.SourceParameterName);
      var instanceParam = TypeAccessorHelper.GetCastOrConvertExpression(oInstanceParam, fieldDeclaringType);

      var exprCallFieldGetFn = Expression.Field(instanceParam, fieldInfo);
      //var oExprCallFieldGetFn = this.GetCastOrConvertExpression(exprCallFieldGetFn, TypeConstants.ObjectType);
      var oExprCallFieldGetFn = Expression.Convert(exprCallFieldGetFn, TypeConstants.ObjectType);

      return Expression.Lambda<MemberGetter>(oExprCallFieldGetFn, oInstanceParam).Compile();
    }

    #endregion

    #region -- CreateExpressionSetter --

    public static MemberSetter CreateExpressionSetter(FieldInfo fieldInfo)
    {
      var fieldDeclaringType = fieldInfo.DeclaringType;

      var sourceParameter = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.SourceParameterName);
      var valueParameter = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.ValueParameterName);

      var sourceExpression = TypeAccessorHelper.GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

      var fieldExpression = Expression.Field(sourceExpression, fieldInfo);

      var valueExpression = TypeAccessorHelper.GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

      var genericSetFieldMethodInfo = s_setFieldMethod.MakeGenericMethod(fieldExpression.Type);

      var setFieldMethodCallExpression = Expression.Call(null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

      return Expression.Lambda<MemberSetter>(setFieldMethodCallExpression, sourceParameter, valueParameter).Compile();
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
  public static partial class FieldInvoker<T>
  {
    #region -- CreateExpressionGetter --

    public static MemberGetter<T> CreateExpressionGetter(FieldInfo fieldInfo)
    {
      var fieldDeclaringType = fieldInfo.DeclaringType;
      var thisType = TypeAccessorHelper<T>.ThisType;
      var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
      var field = thisType != fieldDeclaringType
          ? Expression.Field(Expression.TypeAs(instance, fieldDeclaringType), fieldInfo)
          : Expression.Field(instance, fieldInfo);
      var convertField = Expression.TypeAs(field, TypeConstants.ObjectType);
      return Expression.Lambda<MemberGetter<T>>(convertField, instance).Compile();
    }

    #endregion

    #region -- CreateExpressionSetter --

    public static MemberSetter<T> CreateExpressionSetter(FieldInfo fieldInfo)
    {
      var thisType = TypeAccessorHelper<T>.ThisType;
      var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
      var argument = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.ArgumentParameterName);

      var declaringType = fieldInfo.DeclaringType;
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
      var argument = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.ArgumentParameterName);

      var field = thisType != fieldInfo.DeclaringType
          ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
          : Expression.Field(instance, fieldInfo);

      var setterCall = Expression.Assign(
          field,
          Expression.Convert(argument, fieldInfo.FieldType));

      return Expression.Lambda<MemberRefSetter<T>>(setterCall, instance, argument).Compile();
    }

    #endregion
  }
}
