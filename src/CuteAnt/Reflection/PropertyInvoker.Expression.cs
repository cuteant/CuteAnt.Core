﻿using System.Linq.Expressions;
using System.Reflection;

namespace CuteAnt.Reflection
{
    public static partial class PropertyInvoker
    {
        #region -- CreateExpressionGetter --

        public static MemberGetter CreateExpressionGetter(PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod(true);
            if (getMethodInfo is null) { return TypeAccessorHelper.EmptyMemberGetter; }

            const string _oInstanceParameterName = "oInstanceParam";

            var oInstanceParam = Expression.Parameter(TypeConstants.ObjectType, _oInstanceParameterName);
            var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.ReflectedType); //propertyInfo.DeclaringType doesn't work on Proxy types

            var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
            var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, TypeConstants.ObjectType);

            return Expression.Lambda<MemberGetter>(oExprCallPropertyGetFn, oInstanceParam).Compile();
        }

        #endregion

        #region -- CreateExpressionSetter --

        public static MemberSetter CreateExpressionSetter(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod(true);
            if (propertySetMethod is null) return TypeAccessorHelper.EmptyMemberSetter;

            try
            {
                var declaringType = propertyInfo.ReflectedType;

                var instance = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.InstanceParameterName);
                var argument = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.ArgumentParameterName);

                var instanceParam = declaringType.IsValueType && !declaringType.IsNullableType()
                    ? Expression.Unbox(instance, declaringType)
                    : Expression.Convert(instance, declaringType);

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
    }

    public static partial class PropertyInvoker<T>
    {
        #region -- CreateExpressionGetter --

        public static MemberGetter<T> CreateExpressionGetter(PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod(true);
            if (getMethodInfo is null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }

            var thisType = TypeAccessorHelper<T>.ThisType;
            var propertyDeclaringType = propertyInfo.DeclaringType;

            var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
            var property = thisType != propertyDeclaringType
                ? Expression.Property(Expression.TypeAs(instance, propertyDeclaringType), propertyInfo)
                : Expression.Property(instance, propertyInfo);
            var convertProperty = Expression.TypeAs(property, TypeConstants.ObjectType);
            return Expression.Lambda<MemberGetter<T>>(convertProperty, instance).Compile();
        }

        #endregion

        #region -- CreateExpressionSetter --

        public static MemberSetter<T> CreateExpressionSetter(PropertyInfo propertyInfo)
        {
            //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
            var mi = propertyInfo.GetSetMethod(true);
            if (mi is null) return TypeAccessorHelper<T>.EmptyMemberSetter;

            try
            {
                var thisType = TypeAccessorHelper<T>.ThisType;
                var propertyDeclaringType = propertyInfo.DeclaringType;

                var instance = Expression.Parameter(thisType, TypeAccessorHelper.InstanceParameterName);
                var argument = Expression.Parameter(TypeConstants.ObjectType, TypeAccessorHelper.ArgumentParameterName);

                var instanceType = thisType != propertyDeclaringType
                    ? (Expression)Expression.TypeAs(instance, propertyDeclaringType)
                    : instance;

                var setterCall = Expression.Call(
                    instanceType,
                    mi,
                    TypeAccessorHelper.GetCastOrConvertExpression(argument, propertyInfo.PropertyType));  //Expression.Convert(argument, propertyInfo.PropertyType));

                return Expression.Lambda<MemberSetter<T>>(setterCall, instance, argument).Compile();
            }
            catch //fallback for Android
            {
                return (o, convertedValue) => mi.Invoke(o, new[] { convertedValue });
            }
        }

        #endregion
    }
}

