using System;
using System.Reflection;
using System.Linq.Expressions;
#if DESKTOPCLR || NETSTANDARD
using System.Reflection.Emit;
#endif

namespace CuteAnt.Reflection
{
  public static class PropertyInvoker
  {
    #region @@ Fields @@

    private static readonly Type s_objectType = TypeX._.Object;

    #endregion

    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter CreateDefaultGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }
      var getMethodInfo = propertyInfo.GetMethodInfo();
      if (getMethodInfo == null) { return TypeAccessorHelper.EmptyMemberGetter; }

      return o => getMethodInfo.Invoke(o, EmptyArray<object>.Instance);
    }
    public static MemberSetter CreateDefaultSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }
      var propertySetMethod = propertyInfo.SetMethodInfo();
      if (propertySetMethod == null) return TypeAccessorHelper.EmptyMemberSetter;

      return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
    }

    #endregion

    #region -- CreateExpressionGetter --

    public static MemberGetter CreateExpressionGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }
      var getMethodInfo = propertyInfo.GetMethodInfo();
      if (getMethodInfo == null) { return TypeAccessorHelper.EmptyMemberGetter; }

      const string _oInstanceParameterName = "oInstanceParam";

      var oInstanceParam = Expression.Parameter(s_objectType, _oInstanceParameterName);
      var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.GetDeclaringType()); // ReflectedType() //propertyInfo.DeclaringType doesn't work on Proxy types

      var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
      var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, s_objectType);

      return Expression.Lambda<MemberGetter>(oExprCallPropertyGetFn, oInstanceParam).Compile();
    }

    #endregion

    #region -- CreateExpressionSetter --

    public static MemberSetter CreateExpressionSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }
      var propertySetMethod = propertyInfo.SetMethodInfo();
      if (propertySetMethod == null) return TypeAccessorHelper.EmptyMemberSetter;

      try
      {
        var instance = Expression.Parameter(s_objectType, TypeAccessorHelper.InstanceParameterName);
        var argument = Expression.Parameter(s_objectType, TypeAccessorHelper.ArgumentParameterName);

        var instanceParam = Expression.Convert(instance, propertyInfo.GetDeclaringType()); // ReflectedType()
        //var valueParam = Expression.Convert(argument, propertyInfo.PropertyType);
        var valueParam = TypeAccessorHelper.GetCastOrConvertExpression(argument, propertyInfo.PropertyType);

        var setterCall = Expression.Call(instanceParam, propertySetMethod, valueParam);

        return Expression.Lambda<MemberSetter>(setterCall, instance, argument).Compile();
      }
      catch //fallback for Android
      {
        return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
      }
    }

    #endregion

#if DESKTOPCLR || NETSTANDARD
    #region -- CreateEmitGetter --

    public static MemberGetter CreateEmitGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }
      var mi = propertyInfo.GetMethodInfo(true);
      if (null == mi) { return TypeAccessorHelper.EmptyMemberGetter; }
      var isStatic = mi.IsStatic;

      var getter = TypeAccessorHelper.CreateDynamicGetMethod(propertyInfo);

      var gen = getter.GetILGenerator();
      if (!isStatic)
      {
        gen.Emit(OpCodes.Ldarg_0);
        var declaringType = propertyInfo.GetDeclaringType();
        if (declaringType.IsValueType())
        {
          gen.Emit(OpCodes.Unbox, declaringType);
        }
        else
        {
          gen.Emit(OpCodes.Castclass, declaringType);
        }
      }

      //gen.Emit(mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi);
      gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, null);

      var propertyType = propertyInfo.PropertyType;
      if (propertyType.IsValueType())
      {
        gen.Emit(OpCodes.Box, propertyType);
      }

      gen.Emit(OpCodes.Ret);

      return (MemberGetter)getter.CreateDelegate(TypeAccessorHelper.MemberGetterType);
    }

    #endregion

    #region -- CreateEmitSetter --

    public static MemberSetter CreateEmitSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }
      var mi = propertyInfo.SetMethodInfo(true);
      if (mi == null) { return TypeAccessorHelper.EmptyMemberSetter; }
      var isStatic = mi.IsStatic;

      var setter = TypeAccessorHelper.CreateDynamicSetMethod(propertyInfo);

      var gen = setter.GetILGenerator();
      if (!isStatic)
      {
        gen.Emit(OpCodes.Ldarg_0);

        var declaringType = propertyInfo.GetDeclaringType();
        if (declaringType.IsValueType())
        {
          gen.Emit(OpCodes.Unbox, declaringType);
        }
        else
        {
          gen.Emit(OpCodes.Castclass, declaringType);
        }
      }

      gen.Emit(OpCodes.Ldarg_1);

      var propertyType = propertyInfo.PropertyType;
      if (propertyType.IsValueType())
      {
        gen.Emit(OpCodes.Unbox_Any, propertyType);
      }
      else
      {
        gen.Emit(OpCodes.Castclass, propertyType);
      }

      gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, (Type[])null);

      gen.Emit(OpCodes.Ret);

      return (MemberSetter)setter.CreateDelegate(TypeAccessorHelper.MemberSetterType);
    }

    #endregion
#endif
  }

  public static class PropertyInvoker<T>
  {
    #region @@ Fields @@

    private static readonly Type s_objectType = TypeX._.Object;

    #endregion

    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter<T> CreateDefaultGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var getMethodInfo = propertyInfo.GetMethodInfo();
      if (getMethodInfo == null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }

      return o => getMethodInfo.Invoke(o, EmptyArray<object>.Instance);
    }
    public static MemberSetter<T> CreateDefaultSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var propertySetMethod = propertyInfo.SetMethodInfo();
      if (propertySetMethod == null) return TypeAccessorHelper<T>.EmptyMemberSetter;

      return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
    }

    #endregion

    #region -- CreateExpressionGetter --

    public static MemberGetter<T> CreateExpressionGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var thisType = TypeAccessorHelper<T>.ThisType;
      var propertyDeclaringType = propertyInfo.GetDeclaringType();

      var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
      var property = thisType != propertyDeclaringType
          ? Expression.Property(Expression.TypeAs(instance, propertyDeclaringType), propertyInfo)
          : Expression.Property(instance, propertyInfo);
      var convertProperty = Expression.TypeAs(property, s_objectType);
      return Expression.Lambda<MemberGetter<T>>(convertProperty, instance).Compile();
    }

    #endregion

    #region -- CreateExpressionSetter --

    public static MemberSetter<T> CreateExpressionSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var mi = propertyInfo.SetMethodInfo(true);
      if (mi == null) return TypeAccessorHelper<T>.EmptyMemberSetter;

      var thisType = TypeAccessorHelper<T>.ThisType;
      var propertyDeclaringType = propertyInfo.GetDeclaringType();

      var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
      var argument = Expression.Parameter(s_objectType, TypeAccessorHelper.ArgumentParameterName);

      var instanceType = thisType != propertyDeclaringType
          ? (Expression)Expression.TypeAs(instance, propertyDeclaringType)
          : instance;

      var setterCall = Expression.Call(
          instanceType,
          mi,
          TypeAccessorHelper.GetCastOrConvertExpression(argument, propertyInfo.PropertyType));  //Expression.Convert(argument, propertyInfo.PropertyType));

      return Expression.Lambda<MemberSetter<T>>(setterCall, instance, argument).Compile();
    }

    #endregion

#if DESKTOPCLR || NETSTANDARD
    #region -- CreateEmitGetter --

    public static MemberGetter<T> CreateEmitGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var mi = propertyInfo.GetMethodInfo(true);
      if (null == mi) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var isStatic = mi.IsStatic;

      var getter = TypeAccessorHelper<T>.CreateDynamicGetMethod(propertyInfo);

      var gen = getter.GetILGenerator();

      if (!isStatic)
      {
        var thisType = TypeAccessorHelper<T>.ThisType;
        var declaringType = propertyInfo.GetDeclaringType();
        if (thisType.IsValueType())
        {
          gen.Emit(OpCodes.Ldarga_S, 0);

          if (thisType != declaringType)
          {
            gen.Emit(OpCodes.Unbox, declaringType);
          }
        }
        else
        {
          gen.Emit(OpCodes.Ldarg_0);

          if (thisType != declaringType)
          {
            gen.Emit(OpCodes.Castclass, declaringType);
          }
        }
      }

      gen.Emit(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi);

      var propertyType = propertyInfo.PropertyType;
      if (propertyType.IsValueType())
      {
        gen.Emit(OpCodes.Box, propertyType);
      }

      gen.Emit(OpCodes.Isinst, s_objectType);

      gen.Emit(OpCodes.Ret);

      return (MemberGetter<T>)getter.CreateDelegate(TypeAccessorHelper<T>.MemberGetterType);
    }

    #endregion

    #region -- CreateEmitSetter --

    public static MemberSetter<T> CreateEmitSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var mi = propertyInfo.SetMethodInfo(true);
      if (mi == null) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var isStatic = mi.IsStatic;

      var setter = TypeAccessorHelper<T>.CreateDynamicSetMethod(propertyInfo);

      var gen = setter.GetILGenerator();
      if (!isStatic)
      {
        var thisType = TypeAccessorHelper<T>.ThisType;
        gen.Emit(OpCodes.Ldarg_0);

        var declaringType = propertyInfo.GetDeclaringType();
        if (declaringType.IsValueType())
        {
          if (thisType != declaringType) { gen.Emit(OpCodes.Unbox, declaringType); }
        }
        else
        {
          if (thisType != declaringType) { gen.Emit(OpCodes.Castclass, declaringType); }
        }
      }

      gen.Emit(OpCodes.Ldarg_1);

      var propertyType = propertyInfo.PropertyType;
      if (propertyType.IsValueType())
      {
        gen.Emit(OpCodes.Unbox_Any, propertyType);
      }
      else
      {
        gen.Emit(OpCodes.Castclass, propertyType);
      }

      gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, (Type[])null);

      gen.Emit(OpCodes.Ret);

      return (MemberSetter<T>)setter.CreateDelegate(TypeAccessorHelper<T>.MemberSetterType);
    }

    #endregion
#endif
  }
}

