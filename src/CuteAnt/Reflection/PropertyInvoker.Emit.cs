using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
  public static partial class PropertyInvoker
  {
    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter CreateDefaultGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }
      var getMethodInfo = propertyInfo.GetGetMethod(true);
      if (getMethodInfo == null) { return TypeAccessorHelper.EmptyMemberGetter; }

      return o => getMethodInfo.Invoke(o, EmptyArray<object>.Instance);
    }
    public static MemberSetter CreateDefaultSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }
      var propertySetMethod = propertyInfo.GetSetMethod(true);
      if (propertySetMethod == null) return TypeAccessorHelper.EmptyMemberSetter;

      return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
    }

    #endregion

    #region -- CreateEmitGetter --

    public static MemberGetter CreateEmitGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }
      var mi = propertyInfo.GetGetMethod(true);
      if (null == mi) { return TypeAccessorHelper.EmptyMemberGetter; }
      var isStatic = mi.IsStatic;

      var getter = TypeAccessorHelper.CreateDynamicGetMethod(propertyInfo);

      var gen = getter.GetILGenerator();
      if (!isStatic)
      {
        gen.Emit(OpCodes.Ldarg_0);
        var declaringType = propertyInfo.DeclaringType;
        gen.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType);
      }

      //gen.Emit(mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi);
      gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, null);

      var propertyType = propertyInfo.PropertyType;
      if (propertyType.IsValueType) { gen.Emit(OpCodes.Box, propertyType); }

      gen.Emit(OpCodes.Ret);

      return (MemberGetter)getter.CreateDelegate(TypeAccessorHelper.MemberGetterType);
    }

    #endregion

    #region -- CreateEmitSetter --

    public static MemberSetter CreateEmitSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }
      var mi = propertyInfo.GetSetMethod(true);
      if (mi == null) { return TypeAccessorHelper.EmptyMemberSetter; }
      var isStatic = mi.IsStatic;

      var setter = TypeAccessorHelper.CreateDynamicSetMethod(propertyInfo);

      var gen = setter.GetILGenerator();
      if (!isStatic)
      {
        gen.Emit(OpCodes.Ldarg_0);

        var declaringType = propertyInfo.DeclaringType;
        gen.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType);
      }

      gen.Emit(OpCodes.Ldarg_1);

      var propertyType = propertyInfo.PropertyType;
      gen.Emit(propertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyType);

      gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, null);

      gen.Emit(OpCodes.Ret);

      return (MemberSetter)setter.CreateDelegate(TypeAccessorHelper.MemberSetterType);
    }

    #endregion
  }

  public static partial class PropertyInvoker<T>
  {
    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter<T> CreateDefaultGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var getMethodInfo = propertyInfo.GetGetMethod(true);
      if (getMethodInfo == null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }

      return o => getMethodInfo.Invoke(o, EmptyArray<object>.Instance);
    }
    public static MemberSetter<T> CreateDefaultSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var propertySetMethod = propertyInfo.GetSetMethod(true);
      if (propertySetMethod == null) return TypeAccessorHelper<T>.EmptyMemberSetter;

      return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
    }

    #endregion

    #region -- CreateEmitGetter --

    public static MemberGetter<T> CreateEmitGetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var mi = propertyInfo.GetGetMethod(true);
      if (null == mi) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
      var isStatic = mi.IsStatic;

      var getter = TypeAccessorHelper<T>.CreateDynamicGetMethod(propertyInfo);

      var gen = getter.GetILGenerator();

      if (!isStatic)
      {
        var thisType = TypeAccessorHelper<T>.ThisType;
        var declaringType = propertyInfo.DeclaringType;
        if (thisType.IsValueType)
        {
          gen.Emit(OpCodes.Ldarga_S, 0);

          if (thisType != declaringType) { gen.Emit(OpCodes.Unbox, declaringType); }
        }
        else
        {
          gen.Emit(OpCodes.Ldarg_0);

          if (thisType != declaringType) { gen.Emit(OpCodes.Castclass, declaringType); }
        }
      }

      gen.Emit(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi);

      var propertyType = propertyInfo.PropertyType;
      if (propertyType.IsValueType)
      {
        gen.Emit(OpCodes.Box, propertyType);
      }

      gen.Emit(OpCodes.Isinst, TypeConstants.ObjectType);

      gen.Emit(OpCodes.Ret);

      return (MemberGetter<T>)getter.CreateDelegate(TypeAccessorHelper<T>.MemberGetterType);
    }

    #endregion

    #region -- CreateEmitSetter --

    public static MemberSetter<T> CreateEmitSetter(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var mi = propertyInfo.GetSetMethod(true);
      if (mi == null) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
      var isStatic = mi.IsStatic;

      var setter = TypeAccessorHelper<T>.CreateDynamicSetMethod(propertyInfo);

      var gen = setter.GetILGenerator();
      if (!isStatic)
      {
        var thisType = TypeAccessorHelper<T>.ThisType;
        gen.Emit(OpCodes.Ldarg_0);

        var declaringType = propertyInfo.DeclaringType;
        if (declaringType.IsValueType)
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
      gen.Emit(propertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyType);

      gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, null);

      gen.Emit(OpCodes.Ret);

      return (MemberSetter<T>)setter.CreateDelegate(TypeAccessorHelper<T>.MemberSetterType);
    }

    #endregion
  }
}

