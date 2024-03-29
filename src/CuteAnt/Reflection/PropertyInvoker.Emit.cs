﻿using System.Reflection;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
    public static partial class PropertyInvoker
    {
        #region -- CreateDefaultGetter / CreateDefaultSetter --

        public static MemberGetter CreateDefaultGetter(PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod(true);
            if (getMethodInfo is null) { return TypeAccessorHelper.EmptyMemberGetter; }

            return o => getMethodInfo.Invoke(o, EmptyArray<object>.Instance);
        }
        public static MemberSetter CreateDefaultSetter(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod(true);
            if (propertySetMethod is null) return TypeAccessorHelper.EmptyMemberSetter;

            return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
        }

        #endregion

        #region -- CreateEmitGetter --

        public static MemberGetter CreateEmitGetter(PropertyInfo propertyInfo)
        {
            var mi = propertyInfo.GetGetMethod(true);
            if (mi is null) { return TypeAccessorHelper.EmptyMemberGetter; }
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
            var mi = propertyInfo.GetSetMethod(true);
            if (mi is null) { return TypeAccessorHelper.EmptyMemberSetter; }
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
            var method = TypeAccessorHelper.GetFieldValueConvertMethod(propertyType);
            // 字符串 null 赋值会被替换为 ""
            if (method != null && propertyType != TypeConstants.StringType)
            {
                gen.Emit(method.IsStatic || method.IsFinal ? OpCodes.Call : OpCodes.Callvirt, method);
            }
            else
            {
                gen.Emit(propertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyType);
            }

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
            var getMethodInfo = propertyInfo.GetGetMethod(true);
            if (getMethodInfo is null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }

            return o => getMethodInfo.Invoke(o, EmptyArray<object>.Instance);
        }
        public static MemberSetter<T> CreateDefaultSetter(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod(true);
            if (propertySetMethod is null) return TypeAccessorHelper<T>.EmptyMemberSetter;

            return (o, convertedValue) => propertySetMethod.Invoke(o, new[] { convertedValue });
        }

        #endregion

        #region -- CreateEmitGetter --

        public static MemberGetter<T> CreateEmitGetter(PropertyInfo propertyInfo)
        {
            var mi = propertyInfo.GetGetMethod(true);
            if (mi is null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
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
            var mi = propertyInfo.GetSetMethod(true);
            if (mi is null) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
            var isStatic = mi.IsStatic;

            var setter = TypeAccessorHelper<T>.CreateDynamicSetMethod(propertyInfo);

            var gen = setter.GetILGenerator();
            if (!isStatic)
            {
                var thisType = TypeAccessorHelper<T>.ThisType;
                gen.Emit(OpCodes.Ldarg_0);

                var declaringType = propertyInfo.DeclaringType;
                if (thisType != declaringType) { gen.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType); }
            }

            gen.Emit(OpCodes.Ldarg_1);

            var propertyType = propertyInfo.PropertyType;
            var method = TypeAccessorHelper.GetFieldValueConvertMethod(propertyType);
            // 字符串 null 赋值会被替换为 ""
            if (method != null && propertyType != TypeConstants.StringType)
            {
                gen.Emit(method.IsStatic || method.IsFinal ? OpCodes.Call : OpCodes.Callvirt, method);
            }
            else
            {
                gen.Emit(propertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, propertyType);
            }

            gen.EmitCall(isStatic || mi.IsFinal ? OpCodes.Call : OpCodes.Callvirt, mi, null);

            gen.Emit(OpCodes.Ret);

            return (MemberSetter<T>)setter.CreateDelegate(TypeAccessorHelper<T>.MemberSetterType);
        }

        #endregion
    }
}

