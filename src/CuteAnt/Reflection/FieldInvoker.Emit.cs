using System.Reflection;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
  public static partial class FieldInvoker
  {
    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter CreateDefaultGetter(FieldInfo fieldInfo) => fieldInfo.GetValue;
    public static MemberSetter CreateDefaultSetter(FieldInfo fieldInfo) => fieldInfo.SetValue;

    #endregion

    #region -- CreateEmitGetter --

    public static MemberGetter CreateEmitGetter(FieldInfo fieldInfo)
    {
      var getter = TypeAccessorHelper.CreateDynamicGetMethod(fieldInfo);

      var gen = getter.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);

      var declaringType = fieldInfo.DeclaringType;
      gen.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType);

      gen.Emit(OpCodes.Ldfld, fieldInfo);

      var fieldType = fieldInfo.FieldType;
      if (fieldType.IsValueType) { gen.Emit(OpCodes.Box, fieldType); }

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

      var declaringType = fieldInfo.DeclaringType;
      gen.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType);

      gen.Emit(OpCodes.Ldarg_1);

      var fieldType = fieldInfo.FieldType;
      var method = TypeAccessorHelper.GetFieldValueConvertMethod(fieldType);
      if (method != null)
      {
        gen.Emit(method.IsStatic || method.IsFinal ? OpCodes.Call : OpCodes.Callvirt, method);
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
  }
  public static partial class FieldInvoker<T>
  {
    #region -- CreateDefaultGetter / CreateDefaultSetter --

    public static MemberGetter<T> CreateDefaultGetter(FieldInfo fieldInfo) => obj => fieldInfo.GetValue(obj);
    public static MemberSetter<T> CreateDefaultSetter(FieldInfo fieldInfo) => (obj, v) => fieldInfo.SetValue(obj, v);

    #endregion

    #region -- CreateEmitGetter --

    public static MemberGetter<T> CreateEmitGetter(FieldInfo fieldInfo)
    {
      var getter = TypeAccessorHelper<T>.CreateDynamicGetMethod(fieldInfo);

      var gen = getter.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);

      gen.Emit(OpCodes.Ldfld, fieldInfo);

      var fieldType = fieldInfo.FieldType;
      if (fieldType.IsValueType) { gen.Emit(OpCodes.Box, fieldType); }

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
      var declaringType = fieldInfo.DeclaringType;
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
        gen.Emit(method.IsStatic || method.IsFinal ? OpCodes.Call : OpCodes.Callvirt, method);
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
