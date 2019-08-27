using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.Reflection;

namespace System
{
    /// <summary>特性辅助类</summary>
    public static class AttributeX
    {
        #region -- MemberInfo --

        private static DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>> _miCache = new DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>>();
        private static DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>> _miCache2 = new DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>>();

        #region - HasAttribute -

        /// <summary>HasAttribute</summary>
        /// <param name="memberInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute(this MemberInfo memberInfo, Type attributeType, bool inherit = true)
        {
            if (null == memberInfo) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.memberInfo);
            if (null == attributeType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.attributeType);

            switch (memberInfo)
            {
                case Type typeInfo:
                    return HasAttribute(typeInfo, attributeType, inherit);
                case PropertyInfo propertyInfo:
                    return HasAttribute(propertyInfo, attributeType, inherit);
                case FieldInfo fieldInfo:
                    return HasAttribute(fieldInfo, attributeType, inherit);
                default:
                    return GetCustomAttributesX(memberInfo, null, inherit).Any(_ => _.GetType() == attributeType);
            }
        }

        /// <summary>HasAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
          where TAttribute : Attribute
            => HasAttribute(memberInfo, typeof(TAttribute), inherit);

        #endregion

        #region - HasAttributeNamed -

        /// <summary>HasAttributeNamed</summary>
        /// <param name="memberInfo"></param>
        /// <param name="name"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttributeNamed(this MemberInfo memberInfo, string name, bool inherit = true)
        {
            if (null == memberInfo) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.memberInfo);
            if (string.IsNullOrWhiteSpace(name)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            switch (memberInfo)
            {
                case Type typeInfo:
                    return HasAttributeNamed(typeInfo, name, inherit);
                case PropertyInfo propertyInfo:
                    return HasAttributeNamed(propertyInfo, name, inherit);
                case FieldInfo fieldInfo:
                    return HasAttributeNamed(fieldInfo, name, inherit);
                default:
                    var normalizedAttr = name.Replace("Attribute", "");
                    return GetCustomAttributesX(memberInfo, null, inherit).Any(_ =>
                           string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase));
            }
        }

        #endregion

        #region - FirstAttribute -

        /// <summary>FirstAttribute</summary>
        /// <param name="memberInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute FirstAttribute(this MemberInfo memberInfo, Type attributeType, bool inherit = true)
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return FirstAttribute(typeInfo, attributeType, inherit);
                case PropertyInfo propertyInfo:
                    return FirstAttribute(propertyInfo, attributeType, inherit);
                case FieldInfo fieldInfo:
                    return FirstAttribute(fieldInfo, attributeType, inherit);
                default:
                    return GetCustomAttributeX(memberInfo, attributeType, inherit);
            }
        }

        /// <summary>FirstAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute FirstAttribute<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return FirstAttribute<TAttribute>(typeInfo, inherit);
                case PropertyInfo propertyInfo:
                    return FirstAttribute<TAttribute>(propertyInfo, inherit);
                case FieldInfo fieldInfo:
                    return FirstAttribute<TAttribute>(fieldInfo, inherit);
                default:
                    return GetCustomAttributeX<TAttribute>(memberInfo, inherit);
            }
        }

        #endregion

        #region - GetCustomAttributesX -

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <param name="memberInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetCustomAttributesX(this MemberInfo memberInfo, Type attributeType = null, bool inherit = true)
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return GetCustomAttributesX(typeInfo, attributeType, inherit);
                case PropertyInfo propertyInfo:
                    return GetCustomAttributesX(propertyInfo, attributeType, inherit);
                case FieldInfo fieldInfo:
                    return GetCustomAttributesX(fieldInfo, attributeType, inherit);
                default:
                    return GetCustomAttributesInternal(memberInfo, attributeType, inherit);
            }
        }

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return GetCustomAttributesX<TAttribute>(typeInfo, inherit);
                case PropertyInfo propertyInfo:
                    return GetCustomAttributesX<TAttribute>(propertyInfo, inherit);
                case FieldInfo fieldInfo:
                    return GetCustomAttributesX<TAttribute>(fieldInfo, inherit);
                default:
                    var attrs = GetCustomAttributesInternal(memberInfo, typeof(TAttribute), inherit);
                    return attrs.Any() ? attrs.Cast<TAttribute>() : EmptyArray<TAttribute>.Instance;
            }
        }

        private static IEnumerable<Attribute> GetCustomAttributesInternal(MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            if (memberInfo == null) { return EmptyArray<Attribute>.Instance; }

            var micache = _miCache;
            if (!inherit) { micache = _miCache2; }

            if (attributeType == null) { attributeType = typeof(Attribute); }

            // 二级字典缓存
            var cache = micache.GetItem(memberInfo, m => new DictionaryCache<Type, Attribute[]>());
            return cache.GetItem(attributeType, memberInfo, inherit, (t, m, inh) =>
            {
                var attrs = Attribute.GetCustomAttributes(m, t, inh);
                return attrs ?? EmptyArray<Attribute>.Instance;
            });
        }

        #endregion

        #region - GetAllAttributes -

        /// <summary>GetAllAttributes</summary>
        /// <param name="memberInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetAllAttributes(this MemberInfo memberInfo, Type attributeType = null, bool inherit = true)
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return GetAllAttributes(typeInfo, attributeType, inherit);
                case PropertyInfo propertyInfo:
                    return GetAllAttributes(propertyInfo, attributeType, inherit);
                case FieldInfo fieldInfo:
                    return GetAllAttributes(fieldInfo, attributeType, inherit);
                default:
                    return GetCustomAttributesInternal(memberInfo, attributeType, inherit);
            }
        }

        /// <summary>GetAllAttributes</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAllAttributes<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return GetAllAttributes<TAttribute>(typeInfo, inherit);
                case PropertyInfo propertyInfo:
                    return GetAllAttributes<TAttribute>(propertyInfo, inherit);
                case FieldInfo fieldInfo:
                    return GetAllAttributes<TAttribute>(fieldInfo, inherit);
                default:
                    var attrs = GetCustomAttributesInternal(memberInfo, typeof(TAttribute), inherit);
                    return attrs.Any() ? attrs.Cast<TAttribute>() : EmptyArray<TAttribute>.Instance;
            }
        }

        #endregion

        #region - GetCustomAttributeX -

        /// <summary>获取自定义属性</summary>
        /// <param name="memberInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute GetCustomAttributeX(this MemberInfo memberInfo, Type attributeType = null, bool inherit = true)
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return GetCustomAttributeX(typeInfo, attributeType, inherit);
                case PropertyInfo propertyInfo:
                    return GetCustomAttributeX(propertyInfo, attributeType, inherit);
                case FieldInfo fieldInfo:
                    return GetCustomAttributeX(fieldInfo, attributeType, inherit);
                default:
                    var attrs = GetCustomAttributesX(memberInfo, attributeType, false);
                    if (attrs.Any()) return attrs.First();

                    if (inherit)
                    {
                        attrs = GetCustomAttributesX(memberInfo, attributeType, inherit);
                        return attrs.FirstOrDefault();
                    }

                    return default(Attribute);
            }
        }

        /// <summary>获取自定义属性</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetCustomAttributeX<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            switch (memberInfo)
            {
                case Type typeInfo:
                    return GetCustomAttributeX<TAttribute>(typeInfo, inherit);
                case PropertyInfo propertyInfo:
                    return GetCustomAttributeX<TAttribute>(propertyInfo, inherit);
                case FieldInfo fieldInfo:
                    return GetCustomAttributeX<TAttribute>(fieldInfo, inherit);
                default:
                    var attr = GetCustomAttributeX(memberInfo, typeof(TAttribute), inherit);
                    return attr as TAttribute;
            }
        }

        #endregion

        #endregion

        #region -- Type --

        private static DictionaryCache<Type, DictionaryCache<Type, Attribute[]>> s_typeCache = new DictionaryCache<Type, DictionaryCache<Type, Attribute[]>>();
        private static DictionaryCache<Type, DictionaryCache<Type, Attribute[]>> s_typeCache2 = new DictionaryCache<Type, DictionaryCache<Type, Attribute[]>>();
        private static DictionaryCache<Type, List<Attribute>> s_typeAttributesMap = new DictionaryCache<Type, List<Attribute>>();

        #region - Add / Replace / Clear -

        /// <summary>AddRuntimeAttributes</summary>
        /// <param name="type"></param>
        /// <param name="attrs"></param>
        /// <returns></returns>
        public static Type AddRuntimeAttributes(this Type type, params Attribute[] attrs)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            var runtimeAttributes = s_typeAttributesMap.GetItem(type, k => new List<Attribute>());
            lock (runtimeAttributes)
            {
                runtimeAttributes.AddRange(attrs);
            }
            return type;
        }

        /// <summary>ReplaceRuntimeAttribute</summary>
        /// <param name="type"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static Type ReplaceRuntimeAttribute(this Type type, Attribute attr)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            var runtimeAttributes = s_typeAttributesMap.GetItem(type, k => new List<Attribute>());

            lock (runtimeAttributes)
            {
                runtimeAttributes.RemoveAll(x => x.GetType() == attr.GetType());
                runtimeAttributes.Add(attr);
            }

            return type;
        }

        /// <summary>ClearRuntimeAttributes</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type ClearRuntimeAttributes(this Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            var runtimeAttributes = s_typeAttributesMap.GetItem(type, k => new List<Attribute>());

            lock (runtimeAttributes)
            {
                runtimeAttributes.Clear();
            }

            return type;
        }

        #endregion

        #region - HasAttribute -

        /// <summary>HasAttribute</summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute(this Type type, Type attributeType, bool inherit = true)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }
            if (null == attributeType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.attributeType);

            if (GetRuntimeAttributes(type).Any(_ => _.GetType() == attributeType)) { return true; }

            //if (type.Assembly.ReflectionOnly || attributeType.Assembly.ReflectionOnly)
            //{
            //  type = TypeUtils.ToReflectionOnlyType(type);
            //  attributeType = TypeUtils.ToReflectionOnlyType(attributeType);

            //  // we can't use Type.GetCustomAttributes here because we could potentially be working with a reflection-only type.
            //  return CustomAttributeData.GetCustomAttributes(type).Any(
            //          attrib => attributeType.IsAssignableFrom(attrib.AttributeTypeEx()));
            //}

            return GetCustomAttributesX(type, null, inherit).Any(_ => _.GetType() == attributeType);
        }

        /// <summary>HasAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute<TAttribute>(this Type type, bool inherit = true)
          where TAttribute : Attribute
          => HasAttribute(type, typeof(TAttribute), inherit);

        #endregion

        #region - HasAttributeNamed -

        /// <summary>HasAttributeNamed</summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttributeNamed(this Type type, string name, bool inherit = true)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }
            if (string.IsNullOrWhiteSpace(name)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            var normalizedAttr = name.Replace("Attribute", "");
            if (GetRuntimeAttributes(type).Any(_ =>
                string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return GetCustomAttributesX(type, null, inherit).Any(_ =>
                   string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region - FirstAttribute -

        /// <summary>FirstAttribute</summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute FirstAttribute(this Type type, Type attributeType, bool inherit = true)
        {
            var attrs = GetRuntimeAttributes(type, attributeType);
            if (attrs.Any()) { return attrs.First(); }

            return GetCustomAttributeX(type, attributeType, inherit);
        }

        /// <summary>FirstAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute FirstAttribute<TAttribute>(this Type type, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetRuntimeAttributes<TAttribute>(type);
            if (attrs.Any()) { return attrs.First(); }

            return GetCustomAttributeX<TAttribute>(type, inherit);
        }

        #endregion

        #region - GetRuntimeAttributes -

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetRuntimeAttributes(this Type type)
        {
            if (type == null) { return EmptyArray<Attribute>.Instance; }

            return s_typeAttributesMap.GetItem(type, k => new List<Attribute>());
        }

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetRuntimeAttributes<TAttribute>(this Type type)
          where TAttribute : Attribute
          => GetRuntimeAttributes(type).OfType<TAttribute>();

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="type"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetRuntimeAttributes(this Type type, Type attrType)
          => GetRuntimeAttributes(type).Where(_ => _.GetType().As(attrType));

        #endregion

        #region - GetCustomAttributesX -

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetCustomAttributesX(this Type type, Type attributeType = null, bool inherit = true)
        {
            if (type == null) { return EmptyArray<Attribute>.Instance; }

            var typeCache = s_typeCache;
            if (!inherit) { typeCache = s_typeCache2; }

            if (attributeType == null) { attributeType = typeof(Attribute); }

            // 二级字典缓存
            var cache = typeCache.GetItem(type, k => new DictionaryCache<Type, Attribute[]>());
            return cache.GetItem(attributeType, type, inherit, (at, t, inh) =>
            {
                var attrs = Attribute.GetCustomAttributes(t, at, inh);
                return attrs ?? EmptyArray<Attribute>.Instance;
            });
        }

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this Type type, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetCustomAttributesX(type, typeof(TAttribute), inherit);

            return attrs.Any() ? attrs.Cast<TAttribute>() : EmptyArray<TAttribute>.Instance;
        }

        #endregion

        #region - GetAllAttributes -

        /// <summary>GetAllAttributes</summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetAllAttributes(this Type type, Type attributeType = null, bool inherit = true)
        {
            var sysAttrs = GetCustomAttributesX(type, attributeType, inherit);
            var runtimeAttributes = (attributeType == null) ? GetRuntimeAttributes(type) : GetRuntimeAttributes(type, attributeType);
            return runtimeAttributes.Any() ? runtimeAttributes.Concat(sysAttrs) : sysAttrs;
        }

        /// <summary>GetAllAttributes</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAllAttributes<TAttribute>(this Type type, bool inherit = true)
          where TAttribute : Attribute
        {
            var sysAttrs = GetCustomAttributesX<TAttribute>(type, inherit);
            var runtimeAttributes = GetRuntimeAttributes<TAttribute>(type);
            return runtimeAttributes.Any() ? runtimeAttributes.Concat(sysAttrs) : sysAttrs;
        }

        #endregion

        #region - GetCustomAttributeX -

        /// <summary>获取自定义属性</summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute GetCustomAttributeX(this Type type, Type attributeType = null, bool inherit = true)
        {
            var attrs = GetCustomAttributesX(type, attributeType, false);
            if (attrs.Any()) return attrs.First();

            if (inherit)
            {
                attrs = GetCustomAttributesX(type, attributeType, inherit);
                return attrs.FirstOrDefault();
            }

            return default(Attribute);
        }

        /// <summary>获取自定义属性</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetCustomAttributeX<TAttribute>(this Type type, bool inherit = true)
          where TAttribute : Attribute
        {
            var attr = GetCustomAttributeX(type, typeof(TAttribute), inherit);
            return attr as TAttribute;
        }

        #endregion

        #endregion

        #region -- PropertyInfo --

        private static DictionaryCache<string, DictionaryCache<Type, Attribute[]>> s_piCache = new DictionaryCache<string, DictionaryCache<Type, Attribute[]>>(StringComparer.Ordinal);
        private static DictionaryCache<string, DictionaryCache<Type, Attribute[]>> s_piCache2 = new DictionaryCache<string, DictionaryCache<Type, Attribute[]>>(StringComparer.Ordinal);
        private static DictionaryCache<string, List<Attribute>> s_propertyAttributesMap = new DictionaryCache<string, List<Attribute>>(StringComparer.Ordinal);

        #region - Add / Replace / Clear -

        /// <summary>AddRuntimeAttributes</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attrs"></param>
        /// <returns></returns>
        public static PropertyInfo AddRuntimeAttributes(this PropertyInfo propertyInfo, params Attribute[] attrs)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }

            var runtimeAttributes = s_propertyAttributesMap.GetItem(UniqueKey(propertyInfo), k => new List<Attribute>());
            lock (runtimeAttributes)
            {
                runtimeAttributes.AddRange(attrs);
            }
            return propertyInfo;
        }

        /// <summary>ReplaceRuntimeAttribute</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static PropertyInfo ReplaceRuntimeAttribute(this PropertyInfo propertyInfo, Attribute attr)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }

            var runtimeAttributes = s_propertyAttributesMap.GetItem(UniqueKey(propertyInfo), k => new List<Attribute>());

            lock (runtimeAttributes)
            {
                runtimeAttributes.RemoveAll(x => x.GetType() == attr.GetType());
                runtimeAttributes.Add(attr);
            }

            return propertyInfo;
        }

        /// <summary>ClearRuntimeAttributes</summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static PropertyInfo ClearRuntimeAttributes(this PropertyInfo propertyInfo)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }

            var runtimeAttributes = s_propertyAttributesMap.GetItem(UniqueKey(propertyInfo), k => new List<Attribute>());

            lock (runtimeAttributes)
            {
                runtimeAttributes.Clear();
            }

            return propertyInfo;
        }

        #endregion

        #region - HasAttribute -

        /// <summary>HasAttribute</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute(this PropertyInfo propertyInfo, Type attributeType, bool inherit = true)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }
            if (null == attributeType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.attributeType);

            if (GetRuntimeAttributes(propertyInfo).Any(_ => _.GetType() == attributeType)) { return true; }

            return GetCustomAttributesX(propertyInfo, null, inherit).Any(_ => _.GetType() == attributeType);
        }

        /// <summary>HasAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute<TAttribute>(this PropertyInfo propertyInfo, bool inherit = true)
          where TAttribute : Attribute
          => HasAttribute(propertyInfo, typeof(TAttribute), inherit);

        #endregion

        #region - HasAttributeNamed -

        /// <summary>HasAttributeNamed</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="name"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttributeNamed(this PropertyInfo propertyInfo, string name, bool inherit = true)
        {
            if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }
            if (string.IsNullOrWhiteSpace(name)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            var normalizedAttr = name.Replace("Attribute", "");
            if (GetRuntimeAttributes(propertyInfo).Any(_ =>
                string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return GetCustomAttributesX(propertyInfo, null, inherit).Any(_ =>
                   string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region - FirstAttribute -

        /// <summary>FirstAttribute</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute FirstAttribute(this PropertyInfo propertyInfo, Type attributeType, bool inherit = true)
        {
            var attrs = GetRuntimeAttributes(propertyInfo, attributeType);
            if (attrs.Any()) { return attrs.First(); }

            return GetCustomAttributeX(propertyInfo, attributeType, inherit);
        }

        /// <summary>FirstAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute FirstAttribute<TAttribute>(this PropertyInfo propertyInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetRuntimeAttributes<TAttribute>(propertyInfo);
            if (attrs.Any()) { return attrs.First(); }

            return GetCustomAttributeX<TAttribute>(propertyInfo, inherit);
        }

        #endregion

        #region - GetRuntimeAttributes -

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetRuntimeAttributes(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) { return EmptyArray<Attribute>.Instance; }

            return s_propertyAttributesMap.GetItem(UniqueKey(propertyInfo), k => new List<Attribute>());
        }

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetRuntimeAttributes<TAttribute>(this PropertyInfo propertyInfo)
          where TAttribute : Attribute
          => GetRuntimeAttributes(propertyInfo).OfType<TAttribute>();

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetRuntimeAttributes(this PropertyInfo propertyInfo, Type attrType)
          => GetRuntimeAttributes(propertyInfo).Where(_ => _.GetType().As(attrType));

        #endregion

        #region - GetCustomAttributesX -

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetCustomAttributesX(this PropertyInfo propertyInfo, Type attributeType = null, bool inherit = true)
        {
            if (propertyInfo == null) { return EmptyArray<Attribute>.Instance; }

            var piCache = s_piCache;
            if (!inherit) { piCache = s_piCache2; }

            if (attributeType == null) { attributeType = typeof(Attribute); }

            var propertyKey = UniqueKey(propertyInfo);
            // 二级字典缓存
            var cache = piCache.GetItem(propertyKey, k => new DictionaryCache<Type, Attribute[]>());
            return cache.GetItem(attributeType, propertyInfo, inherit, (at, pi, inh) =>
            {
                var attrs = Attribute.GetCustomAttributes(pi, at, inh);
                return attrs ?? EmptyArray<Attribute>.Instance;
            });
        }

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this PropertyInfo propertyInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetCustomAttributesX(propertyInfo, typeof(TAttribute), inherit);

            return attrs.Any() ? attrs.Cast<TAttribute>() : EmptyArray<TAttribute>.Instance;
        }

        #endregion

        #region - GetAllAttributes -

        /// <summary>GetAllAttributes</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetAllAttributes(this PropertyInfo propertyInfo, Type attributeType = null, bool inherit = true)
        {
            var sysAttrs = GetCustomAttributesX(propertyInfo, attributeType, inherit);
            var runtimeAttributes = (attributeType == null) ? GetRuntimeAttributes(propertyInfo) : GetRuntimeAttributes(propertyInfo, attributeType);
            return runtimeAttributes.Any() ? runtimeAttributes.Concat(sysAttrs) : sysAttrs;
        }

        /// <summary>GetAllAttributes</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAllAttributes<TAttribute>(this PropertyInfo propertyInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var sysAttrs = GetCustomAttributesX<TAttribute>(propertyInfo, inherit);
            var runtimeAttributes = GetRuntimeAttributes<TAttribute>(propertyInfo);
            return runtimeAttributes.Any() ? runtimeAttributes.Concat(sysAttrs) : sysAttrs;
        }

        #endregion

        #region - GetCustomAttributeX -

        /// <summary>获取自定义属性</summary>
        /// <param name="propertyInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute GetCustomAttributeX(this PropertyInfo propertyInfo, Type attributeType, bool inherit = true)
        {
            var attrs = GetCustomAttributesX(propertyInfo, attributeType, false);
            if (attrs.Any()) return attrs.First();

            if (inherit)
            {
                attrs = GetCustomAttributesX(propertyInfo, attributeType, inherit);
                return attrs.FirstOrDefault();
            }

            return default(Attribute);
        }

        /// <summary>获取自定义属性</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetCustomAttributeX<TAttribute>(this PropertyInfo propertyInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attr = GetCustomAttributeX(propertyInfo, typeof(TAttribute), inherit);
            return attr as TAttribute;
        }

        #endregion

        #region * UniqueKey *

        private static readonly CachedReadConcurrentDictionary<PropertyInfo, string> s_propertyUniqueKeyCache =
            new CachedReadConcurrentDictionary<PropertyInfo, string>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static Func<PropertyInfo, string> s_propertyUniqueKeyFunc = UniqueKeyInternal;
        [MethodImpl(InlineMethod.Value)]
        private static string UniqueKey(PropertyInfo pi) => s_propertyUniqueKeyCache.GetOrAdd(pi, s_propertyUniqueKeyFunc);
        private static string UniqueKeyInternal(PropertyInfo pi)
        {
            if (null == pi.DeclaringType) { ThrowArgumentException_PI(pi.Name); }

            //return $"{pi.DeclaringType.Namespace}.{pi.DeclaringType.Name}.{pi.Name}";
            return $"{TypeUtils.GetTypeIdentifier(pi.DeclaringType)}.{pi.Name}";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentException_PI(string name)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"Property '{name}' has no DeclaringType", "pi");

            }
        }

        #endregion

        #endregion

        #region -- FieldInfo --

        private static DictionaryCache<string, DictionaryCache<Type, Attribute[]>> s_fiCache = new DictionaryCache<string, DictionaryCache<Type, Attribute[]>>(StringComparer.Ordinal);
        private static DictionaryCache<string, DictionaryCache<Type, Attribute[]>> s_fiCache2 = new DictionaryCache<string, DictionaryCache<Type, Attribute[]>>(StringComparer.Ordinal);
        private static DictionaryCache<string, List<Attribute>> s_fieldAttributesMap = new DictionaryCache<string, List<Attribute>>(StringComparer.Ordinal);

        #region - Add / Replace / Clear -

        /// <summary>AddRuntimeAttributes</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attrs"></param>
        /// <returns></returns>
        public static FieldInfo AddRuntimeAttributes(this FieldInfo fieldInfo, params Attribute[] attrs)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }

            var runtimeAttributes = s_fieldAttributesMap.GetItem(UniqueKey(fieldInfo), k => new List<Attribute>());
            lock (runtimeAttributes)
            {
                runtimeAttributes.AddRange(attrs);
            }
            return fieldInfo;
        }

        /// <summary>ReplaceRuntimeAttribute</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static FieldInfo ReplaceRuntimeAttribute(this FieldInfo fieldInfo, Attribute attr)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }

            var runtimeAttributes = s_fieldAttributesMap.GetItem(UniqueKey(fieldInfo), k => new List<Attribute>());

            lock (runtimeAttributes)
            {
                runtimeAttributes.RemoveAll(x => x.GetType() == attr.GetType());
                runtimeAttributes.Add(attr);
            }

            return fieldInfo;
        }

        /// <summary>ClearRuntimeAttributes</summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static FieldInfo ClearRuntimeAttributes(this FieldInfo fieldInfo)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }

            var runtimeAttributes = s_fieldAttributesMap.GetItem(UniqueKey(fieldInfo), k => new List<Attribute>());

            lock (runtimeAttributes)
            {
                runtimeAttributes.Clear();
            }

            return fieldInfo;
        }

        #endregion

        #region - HasAttribute -

        /// <summary>HasAttribute</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute(this FieldInfo fieldInfo, Type attributeType, bool inherit = true)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }
            if (null == attributeType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.attributeType);

            if (GetRuntimeAttributes(fieldInfo).Any(_ => _.GetType() == attributeType)) { return true; }

            return GetCustomAttributesX(fieldInfo, null, inherit).Any(_ => _.GetType() == attributeType);
        }

        /// <summary>HasAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="fieldInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttribute<TAttribute>(this FieldInfo fieldInfo, bool inherit = true)
          where TAttribute : Attribute
          => HasAttribute(fieldInfo, typeof(TAttribute), inherit);

        #endregion

        #region - HasAttributeNamed -

        /// <summary>HasAttributeNamed</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="name"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool HasAttributeNamed(this FieldInfo fieldInfo, string name, bool inherit = true)
        {
            if (null == fieldInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.fieldInfo); }
            if (string.IsNullOrWhiteSpace(name)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            var normalizedAttr = name.Replace("Attribute", "");
            if (GetRuntimeAttributes(fieldInfo).Any(_ =>
                string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return GetCustomAttributesX(fieldInfo, null, inherit).Any(_ =>
                   string.Equals(_.GetType().Name.Replace("Attribute", ""), normalizedAttr, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region - FirstAttribute -

        /// <summary>FirstAttribute</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute FirstAttribute(this FieldInfo fieldInfo, Type attributeType, bool inherit = true)
        {
            var attrs = GetRuntimeAttributes(fieldInfo, attributeType);
            if (attrs.Any()) { return attrs.First(); }

            return GetCustomAttributeX(fieldInfo, attributeType, inherit);
        }

        /// <summary>FirstAttribute</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="fieldInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute FirstAttribute<TAttribute>(this FieldInfo fieldInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetRuntimeAttributes<TAttribute>(fieldInfo);
            if (attrs.Any()) { return attrs.First(); }

            return GetCustomAttributeX<TAttribute>(fieldInfo, inherit);
        }

        #endregion

        #region - GetRuntimeAttributes -

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetRuntimeAttributes(this FieldInfo fieldInfo)
        {
            if (fieldInfo == null) { return EmptyArray<Attribute>.Instance; }

            return s_fieldAttributesMap.GetItem(UniqueKey(fieldInfo), k => new List<Attribute>());
        }

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetRuntimeAttributes<TAttribute>(this FieldInfo fieldInfo)
          where TAttribute : Attribute
          => GetRuntimeAttributes(fieldInfo).OfType<TAttribute>();

        /// <summary>GetRuntimeAttributes</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetRuntimeAttributes(this FieldInfo fieldInfo, Type attrType)
          => GetRuntimeAttributes(fieldInfo).Where(_ => _.GetType().As(attrType));

        #endregion

        #region - GetCustomAttributesX -

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetCustomAttributesX(this FieldInfo fieldInfo, Type attributeType = null, bool inherit = true)
        {
            if (fieldInfo == null) { return EmptyArray<Attribute>.Instance; }

            var fiCache = s_fiCache;
            if (!inherit) { fiCache = s_fiCache2; }

            if (attributeType == null) { attributeType = typeof(Attribute); }

            var fieldKey = UniqueKey(fieldInfo);
            // 二级字典缓存
            var cache = fiCache.GetItem(fieldKey, k => new DictionaryCache<Type, Attribute[]>());
            return cache.GetItem(attributeType, fieldInfo, inherit, (at, fi, inh) =>
            {
                var attrs = Attribute.GetCustomAttributes(fi, at, inh);
                return attrs ?? EmptyArray<Attribute>.Instance;
            });
        }

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="fieldInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this FieldInfo fieldInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetCustomAttributesX(fieldInfo, typeof(TAttribute), inherit);

            return attrs.Any() ? attrs.Cast<TAttribute>() : EmptyArray<TAttribute>.Instance;
        }

        #endregion

        #region - GetAllAttributes -

        /// <summary>GetAllAttributes</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetAllAttributes(this FieldInfo fieldInfo, Type attributeType = null, bool inherit = true)
        {
            var sysAttrs = GetCustomAttributesX(fieldInfo, attributeType, inherit);
            var runtimeAttributes = (attributeType == null) ? GetRuntimeAttributes(fieldInfo) : GetRuntimeAttributes(fieldInfo, attributeType);
            return runtimeAttributes.Any() ? runtimeAttributes.Concat(sysAttrs) : sysAttrs;
        }

        /// <summary>GetAllAttributes</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="fieldInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetAllAttributes<TAttribute>(this FieldInfo fieldInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var sysAttrs = GetCustomAttributesX<TAttribute>(fieldInfo, inherit);
            var runtimeAttributes = GetRuntimeAttributes<TAttribute>(fieldInfo);
            return runtimeAttributes.Any() ? runtimeAttributes.Concat(sysAttrs) : sysAttrs;
        }

        #endregion

        #region - GetCustomAttributeX -

        /// <summary>获取自定义属性</summary>
        /// <param name="fieldInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute GetCustomAttributeX(this FieldInfo fieldInfo, Type attributeType, bool inherit = true)
        {
            var attrs = GetCustomAttributesX(fieldInfo, attributeType, false);
            if (attrs.Any()) return attrs.First();

            if (inherit)
            {
                attrs = GetCustomAttributesX(fieldInfo, attributeType, inherit);
                return attrs.FirstOrDefault();
            }

            return default(Attribute);
        }

        /// <summary>获取自定义属性</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="fieldInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetCustomAttributeX<TAttribute>(this FieldInfo fieldInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attr = GetCustomAttributeX(fieldInfo, typeof(TAttribute), inherit);
            return attr as TAttribute;
        }

        #endregion

        #region * UniqueKey *

        private static readonly CachedReadConcurrentDictionary<FieldInfo, string> s_fieldUniqueKeyCache =
            new CachedReadConcurrentDictionary<FieldInfo, string>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static Func<FieldInfo, string> s_fieldUniqueKeyFunc = UniqueKeyInternal;
        [MethodImpl(InlineMethod.Value)]
        private static string UniqueKey(FieldInfo fi) => s_fieldUniqueKeyCache.GetOrAdd(fi, s_fieldUniqueKeyFunc);
        private static string UniqueKeyInternal(FieldInfo fi)
        {
            if (fi.DeclaringType == null) ThrowArgumentException_FI(fi.Name);

            //return $"{fi.DeclaringType.Namespace}.{fi.DeclaringType.Name}.{fi.Name}";
            return $"{TypeUtils.GetTypeIdentifier(fi.DeclaringType)}.{fi.Name}";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentException_FI(string name)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException($"Field '{name}' has no DeclaringType", "fi");

            }
        }

        #endregion

        #endregion

        #region -- ClearRuntimeAttributes --

        /// <summary>ClearRuntimeAttributes</summary>
        public static void ClearRuntimeAttributes()
        {
            s_typeAttributesMap.Clear();
            s_propertyAttributesMap.Clear();
            s_fieldAttributesMap.Clear();
        }

        #endregion

        #region -- ParameterInfo --

        private static DictionaryCache<ParameterInfo, DictionaryCache<Type, Attribute[]>> s_parameterCache = new DictionaryCache<ParameterInfo, DictionaryCache<Type, Attribute[]>>();
        private static DictionaryCache<ParameterInfo, DictionaryCache<Type, Attribute[]>> s_parameterCache2 = new DictionaryCache<ParameterInfo, DictionaryCache<Type, Attribute[]>>();

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <param name="parameterInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetCustomAttributesX(this ParameterInfo parameterInfo, Type attributeType = null, bool inherit = true)
        {
            if (parameterInfo == null) { return EmptyArray<Attribute>.Instance; }

            var parameterCache = s_parameterCache;
            if (!inherit) { parameterCache = s_parameterCache2; }

            if (attributeType == null) { attributeType = typeof(Attribute); }

            // 二级字典缓存
            var cache = parameterCache.GetItem(parameterInfo, k => new DictionaryCache<Type, Attribute[]>());
            return cache.GetItem(attributeType, parameterInfo, inherit, (at, pi, inh) =>
            {
                var attrs = Attribute.GetCustomAttributes(pi, at, inh);
                return attrs ?? EmptyArray<Attribute>.Instance;
            });
        }

        /// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="parameterInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this ParameterInfo parameterInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetCustomAttributesX(parameterInfo, typeof(TAttribute), inherit);

            return attrs.Any() ? attrs.Cast<TAttribute>() : EmptyArray<TAttribute>.Instance;
        }

        /// <summary>获取自定义属性</summary>
        /// <param name="parameterInfo"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute GetCustomAttributeX(this ParameterInfo parameterInfo, Type attributeType = null, bool inherit = true)
        {
            var attrs = GetCustomAttributesX(parameterInfo, attributeType, false);
            if (attrs.Any()) return attrs.First();

            if (inherit)
            {
                attrs = GetCustomAttributesX(parameterInfo, attributeType, inherit);
                return attrs.FirstOrDefault();
            }

            return default(Attribute);
        }

        /// <summary>获取自定义属性</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="parameterInfo"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetCustomAttributeX<TAttribute>(this ParameterInfo parameterInfo, bool inherit = true)
          where TAttribute : Attribute
        {
            var attr = GetCustomAttributeX(parameterInfo, typeof(TAttribute), inherit);
            return attr as TAttribute;
        }

        #endregion

        #region -- Assembly --

        private static DictionaryCache<string, Attribute[]> _asmCache = new DictionaryCache<string, Attribute[]>();
        private static DictionaryCache<string, Attribute[]> _asmCache2 = new DictionaryCache<string, Attribute[]>();

        /// <summary>获取自定义属性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <param name="assembly"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute[] GetCustomAttributesX(this Assembly assembly, Type attributeType, bool inherit = true)
        {
            if (assembly == null) { return EmptyArray<Attribute>.Instance; }
            if (attributeType == null) { return EmptyArray<Attribute>.Instance; }

            var key = $"{assembly.FullName}_{attributeType.FullName}";

            var asmCache = _asmCache;
            if (!inherit) { asmCache = _asmCache2; }

            return asmCache.GetItem(key, assembly, attributeType, inherit, (k, m, at, inh) =>
            {
                var atts = Attribute.GetCustomAttributes(m, at, inh);
                return atts ?? EmptyArray<Attribute>.Instance;
            });
        }

        /// <summary>获取自定义属性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute[] GetCustomAttributesX<TAttribute>(this Assembly assembly, bool inherit = true)
          where TAttribute : Attribute
        {
            var attrs = GetCustomAttributesX(assembly, typeof(TAttribute), inherit);

            return attrs.Any() ? attrs.Cast<TAttribute>().ToArray() : EmptyArray<TAttribute>.Instance;
        }

        /// <summary>获取自定义属性</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttribute GetCustomAttributeX<TAttribute>(this Assembly assembly, bool inherit = true)
          where TAttribute : Attribute
        {
            var avs = GetCustomAttributesX<TAttribute>(assembly, inherit);
            return avs.FirstOrDefault();
        }

        /// <summary>获取自定义属性的值。可用于ReflectionOnly加载的程序集</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult GetCustomAttributeValue<TAttribute, TResult>(this Assembly target)
          where TAttribute : Attribute
        {
            if (target == null) return default(TResult);

            var list = CustomAttributeData.GetCustomAttributes(target);
            if (list == null || list.Count < 1) return default(TResult);

            foreach (var item in list)
            {
                if (typeof(TAttribute) != item.Constructor.DeclaringType) continue;

                var args = item.ConstructorArguments;
                if (args != null && args.Count > 0) return (TResult)args[0].Value;
            }

            return default(TResult);
        }

        /// <summary>获取自定义属性的值。可用于ReflectionOnly加载的程序集</summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="target">目标对象</param>
        /// <param name="inherit">是否递归</param>
        /// <returns></returns>
        public static TResult GetCustomAttributeValue<TAttribute, TResult>(this MemberInfo target, bool inherit = true)
          where TAttribute : Attribute
        {
            if (target == null) return default(TResult);

            try
            {
                var list = CustomAttributeData.GetCustomAttributes(target);
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        if (!TypeUtils.Equal(typeof(TAttribute), item.Constructor.DeclaringType)) continue;

                        var args = item.ConstructorArguments;
                        if (args != null && args.Count > 0) return (TResult)args[0].Value;
                    }
                }
                if (inherit && target is Type)
                {
                    target = (target as Type).BaseType;
                    if (target != null && target != typeof(Object))
                        return GetCustomAttributeValue<TAttribute, TResult>(target, inherit);
                }
            }
            catch
            {
                // 出错以后，如果不是仅反射加载，可以考虑正面来一次
                if (!target.Module.Assembly.ReflectionOnly)
                {
                    var att = GetCustomAttributeX<TAttribute>(target, inherit);
                    if (att != null)
                    {
                        var pi = typeof(TAttribute).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(TResult));
                        if (pi != null) return (TResult)att.GetPropertyValue(pi);
                    }
                }
            }

            return default(TResult);
        }

        #endregion
    }
}