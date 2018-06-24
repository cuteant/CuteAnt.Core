using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  #region -- enum IgnorePropertiesTokenType --

  /// <summary>IgnorePropertiesTokenType</summary>
  [Flags]
  public enum IgnorePropertiesTokenType
  {
    /// <summary>None</summary>
    None = 0x00000000,

    /// <summary>Ignore indexed properties</summary>
    IgnoreIndexedProperties = 0x00000001,

    /// <summary>Ignore non serialized properties</summary>
    IgnoreNonSerializedProperties = 0x00000002
  }

  #endregion

  partial class ReflectUtils
  {
    #region * PropertiesCache *

    private static readonly DictionaryCache<Type, PropertyInfo[]> s_ignoreIndexedPublicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();
    private static readonly DictionaryCache<Type, PropertyInfo[]> s_ignoreNonSerializedPublicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();
    private static readonly DictionaryCache<Type, PropertyInfo[]> s_ignorePublicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();
    private static readonly DictionaryCache<Type, PropertyInfo[]> s_publicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();

    private static readonly DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>> s_ignorepropertiesCache =
        new DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>>();
    private static readonly DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>> s_propertiesCache =
        new DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>>();

    #endregion

    #region * GetInstancePublicProperties *

    private static readonly Func<Type, bool, bool, PropertyInfo[]> s_getInstancePublicPropertiesFunc = GetInstancePublicProperties;

    private static PropertyInfo[] GetInstancePublicProperties(Type type, bool ignoreIndexedProperties, bool ignoreNonSerializedProperties)
    {
      if (type.IsInterface)
      {
        var propertyInfos = new List<PropertyInfo>();

        var considered = new List<Type>();
        var queue = new Queue<Type>();
        considered.Add(type);
        queue.Enqueue(type);

        while (queue.Count > 0)
        {
          var subType = queue.Dequeue();
          foreach (var subInterface in subType.GetInterfaces())
          {
            if (considered.Contains(subInterface)) continue;

            considered.Add(subInterface);
            queue.Enqueue(subInterface);
          }

          var typeProperties = subType
#if !NET40
              .GetTypeInfo()
#endif
              .GetProperties(BindingFlagsHelper.InstancePublicOnlyLookup);

          var newPropertyInfos = typeProperties
              .Where(x => !propertyInfos.Contains(x));

          propertyInfos.InsertRange(0, newPropertyInfos);
        }

        return propertyInfos.ToArray();
      }

      // Void*的基类就是null
      if (type == typeof(object) || type.BaseType == null) return EmptyArray<PropertyInfo>.Instance;

      var list = new List<PropertyInfo>();
      var pis = type
#if !NET40
          .GetTypeInfo()
#endif
          .GetProperties(BindingFlagsHelper.InstancePublicOnlyLookup);
      foreach (var pi in pis)
      {
        if (ignoreIndexedProperties && pi.GetIndexParameters().Length > 0) { continue; }
        if (ignoreNonSerializedProperties)
        {
          if (pi.FirstAttribute<XmlIgnoreAttribute>() != null) { continue; }
          if (pi.FirstAttribute<IgnoreDataMemberAttribute>() != null) { continue; }
        }

        list.Add(pi);
      }
      return list.ToArray();
    }

    #endregion

    #region * GetPropertiesInternal *

    private static readonly Func<ReflectMembersTokenType, Type, bool, PropertyInfo[]> s_getTypePropertiesFunc = GetTypeProperties;

    private static PropertyInfo[] GetTypeProperties(ReflectMembersTokenType reflectPropertiesToken, Type type, bool ignoreIndexedProperties)
    {
      BindingFlags bindingFlags;
      switch (reflectPropertiesToken)
      {
        case ReflectMembersTokenType.InstanceDeclaredAndPublicOnlyMembers:
          bindingFlags = BindingFlagsHelper.InstanceDeclaredAndPublicOnlyLookup;
          break;
        case ReflectMembersTokenType.InstanceDeclaredOnlyMembers:
          bindingFlags = BindingFlagsHelper.InstanceDeclaredOnlyLookup;
          break;

        case ReflectMembersTokenType.InstancePublicOnlyMembers:
          bindingFlags = BindingFlagsHelper.InstancePublicOnlyLookup;
          break;
        case ReflectMembersTokenType.InstanceMembers:
          bindingFlags = BindingFlagsHelper.InstanceLookup;
          break;

        case ReflectMembersTokenType.TypeDeclaredAndPublicOnlyMembers:
          bindingFlags = BindingFlagsHelper.DefaultDeclaredAndPublicOnlyLookup;
          break;
        case ReflectMembersTokenType.TypeDeclaredOnlyMembers:
          bindingFlags = BindingFlagsHelper.DefaultDeclaredOnlyLookup;
          break;

        case ReflectMembersTokenType.TypePublicOnlyMembers:
          bindingFlags = BindingFlagsHelper.DefaultPublicOnlyLookup;
          break;

        case ReflectMembersTokenType.TypeFlattenHierarchyPublicOnlyMembers:
          bindingFlags = BindingFlagsHelper.DefaultPublicOnlyLookupAll;
          break;
        case ReflectMembersTokenType.TypeFlattenHierarchyMembers:
          bindingFlags = BindingFlagsHelper.DefaultLookupAll;
          break;

        case ReflectMembersTokenType.TypeMembers:
        default:
          bindingFlags = BindingFlagsHelper.DefaultLookup;
          break;
      }

      if (type.IsInterface)
      {
        var propertyInfos = new List<PropertyInfo>();

        var considered = new List<Type>();
        var queue = new Queue<Type>();
        considered.Add(type);
        queue.Enqueue(type);

        while (queue.Count > 0)
        {
          var subType = queue.Dequeue();
          foreach (var subInterface in subType.GetInterfaces())
          {
            if (considered.Contains(subInterface)) continue;

            considered.Add(subInterface);
            queue.Enqueue(subInterface);
          }

          var typeProperties = subType
#if !NET40
              .GetTypeInfo()
#endif
              .GetProperties(bindingFlags);
          var newPropertyInfos = typeProperties
              .Where(x => !propertyInfos.Contains(x));

          propertyInfos.InsertRange(0, newPropertyInfos);
        }

        return propertyInfos.ToArray();
      }

      // Void*的基类就是null
      if (type == typeof(object) || type.BaseType == null) return EmptyArray<PropertyInfo>.Instance;

      var pis = type
#if !NET40
                .GetTypeInfo()
#endif
                .GetProperties(bindingFlags);
      return ignoreIndexedProperties
            ? pis.Where(t => t.GetIndexParameters().Length == 0) // ignore indexed properties;
                 .ToArray()
            : pis;
    }

    [MethodImpl(InlineMethod.Value)]
    private static PropertyInfo[] GetPropertiesInternal(Type type, bool ignoreIndexedProperties, ReflectMembersTokenType reflectPropertiesToken)
    {
      if (type == null) { return EmptyArray<PropertyInfo>.Instance; }

      if (type.IsInterface) { ignoreIndexedProperties = false; }
      var propertiesCache = !ignoreIndexedProperties ? s_propertiesCache : s_ignorepropertiesCache;

      // 二级字典缓存
      var cache = propertiesCache.GetItem(type, k => new DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>());
      return cache.GetItem(reflectPropertiesToken, type, ignoreIndexedProperties, s_getTypePropertiesFunc);
    }

    #endregion

    #region - GetInstancePublicProperties -

    /// <summary>GetInstancePublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignorePropertiesToken"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetInstancePublicProperties(this Type type,
      IgnorePropertiesTokenType ignorePropertiesToken = IgnorePropertiesTokenType.None)
    {
      if (type == null) { return EmptyArray<PropertyInfo>.Instance; }

      var ignoreIndexedProperties = ignorePropertiesToken.HasFlag(IgnorePropertiesTokenType.IgnoreIndexedProperties);
      var ignoreNonSerializedProperties = ignorePropertiesToken.HasFlag(IgnorePropertiesTokenType.IgnoreNonSerializedProperties);

      DictionaryCache<Type, PropertyInfo[]> propertiesCache = null;
      switch (ignorePropertiesToken)
      {
        case IgnorePropertiesTokenType.IgnoreIndexedProperties:
          propertiesCache = s_ignoreIndexedPublicPropertiesCache;
          break;
        case IgnorePropertiesTokenType.IgnoreNonSerializedProperties:
          propertiesCache = s_ignoreNonSerializedPublicPropertiesCache;
          break;
        case IgnorePropertiesTokenType.IgnoreIndexedProperties | IgnorePropertiesTokenType.IgnoreNonSerializedProperties:
          propertiesCache = s_ignorePublicPropertiesCache;
          break;
        case IgnorePropertiesTokenType.None:
        default:
          propertiesCache = s_publicPropertiesCache;
          break;
      }

      return propertiesCache.GetItem(type, ignoreIndexedProperties, ignoreNonSerializedProperties, s_getInstancePublicPropertiesFunc);
    }

    #endregion

    /// <summary>GetInstanceDeclaredPublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetInstanceDeclaredPublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.InstanceDeclaredAndPublicOnlyMembers);

    /// <summary>GetInstanceDeclaredProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetInstanceDeclaredProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.InstanceDeclaredOnlyMembers);

    /// <summary>GetInstanceProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetInstanceProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.InstanceMembers);

    /// <summary>GetTypeDeclaredPublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeDeclaredPublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeDeclaredAndPublicOnlyMembers);

    /// <summary>GetTypeDeclaredProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeDeclaredProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeDeclaredOnlyMembers);

    /// <summary>GetTypePublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypePublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypePublicOnlyMembers);

    /// <summary>GetTypeProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeMembers);

    /// <summary>GetTypeFlattenHierarchyPublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeFlattenHierarchyPublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeFlattenHierarchyPublicOnlyMembers);

    /// <summary>GetTypeFlattenHierarchyProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeFlattenHierarchyProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeFlattenHierarchyMembers);

    #region - LookupTypeProperty -

    /// <summary>获取属性。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupTypeProperty(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = s_typeDeclaredPropertiesCache.GetItem(type, s_getTypeDeclaredPropertiesFunc);
        if (properties.TryGetValue(name, out PropertyInfo property)) { return property; };

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    private static readonly DictionaryCache<Type, Dictionary<string, PropertyInfo>> s_typeDeclaredPropertiesCache =
        new DictionaryCache<Type, Dictionary<string, PropertyInfo>>();
    private static readonly Func<Type, Dictionary<string, PropertyInfo>> s_getTypeDeclaredPropertiesFunc = GetTypeDeclaredPropertiesInternal;
    private static Dictionary<string, PropertyInfo> GetTypeDeclaredPropertiesInternal(Type type)
    {
      //return GetTypeDeclaredProperties(type).ToDictionary(_ => _.Name, StringComparer.Ordinal);
      var dic = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
      var properties = GetTypeDeclaredProperties(type);
      foreach (var pi in properties)
      {
        dic[pi.Name] = pi;
      }
      return dic;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="returnType"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupTypeProperty(this Type type, string name, Type returnType, bool declaredOnly = false)
    {
      if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
      if (null == returnType) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.returnType); }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = GetTypeDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType);
        if (property != null) { return property; }

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupTypeProperty(this Type type, string name, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
      if (null == parameterTypes) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.parameterTypes); }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = GetTypeDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="returnType"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupTypeProperty(this Type type, string name, Type returnType, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
      if (null == parameterTypes) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.parameterTypes); }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = GetTypeDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    #endregion

    #region - LookupInstanceProperty -

    /// <summary>获取属性。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupInstanceProperty(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = s_instanceDeclaredPropertiesCache.GetItem(type, s_getInstanceDeclaredPropertiesFunc);
        if (properties.TryGetValue(name, out PropertyInfo property)) { return property; };

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    private static readonly DictionaryCache<Type, Dictionary<string, PropertyInfo>> s_instanceDeclaredPropertiesCache =
        new DictionaryCache<Type, Dictionary<string, PropertyInfo>>();
    private static readonly Func<Type, Dictionary<string, PropertyInfo>> s_getInstanceDeclaredPropertiesFunc = GetInstanceDeclaredPropertiesInternal;
    private static Dictionary<string, PropertyInfo> GetInstanceDeclaredPropertiesInternal(Type type)
    {
      //return GetTypeDeclaredProperties(type).ToDictionary(_ => _.Name, StringComparer.Ordinal);
      var dic = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
      var properties = GetInstanceDeclaredProperties(type);
      foreach (var pi in properties)
      {
        dic[pi.Name] = pi;
      }
      return dic;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="returnType"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupInstanceProperty(this Type type, string name, Type returnType, bool declaredOnly = false)
    {
      if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
      if (null == returnType) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.returnType); }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = GetInstanceDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType);
        if (property != null) { return property; }

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupInstanceProperty(this Type type, string name, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
      if (null == parameterTypes) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.parameterTypes); }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = GetInstanceDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="returnType"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo LookupInstanceProperty(this Type type, string name, Type returnType, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
      if (null == parameterTypes) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.parameterTypes); }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var properties = GetInstanceDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    #endregion

    #region - AsDeclaredProperty -

    /// <summary>AsDeclaredProperty</summary>
    /// <param name="propertyInfo"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo AsDeclaredProperty(this PropertyInfo propertyInfo, Type type)
    {
      if (null == propertyInfo) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyInfo); }
      if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

      if (propertyInfo.DeclaringType == type) { return propertyInfo; }

      return GetTypeDeclaredProperties(propertyInfo.DeclaringType)
                .FirstOrDefault(_ => string.Equals(_.Name, propertyInfo.Name, StringComparison.Ordinal) &&
                                     _.PropertyType == propertyInfo.PropertyType &&
                                     IsParameterMatch(_.GetIndexParameters(), propertyInfo.GetIndexParameters())
                );
    }

    #endregion

    #region * IsParameterMatch *

    private static bool IsParameterMatch(ParameterInfo[] x, ParameterInfo[] y)
    {
      if (ReferenceEquals(x, y)) { return true; }
      if (x == null || y == null) { return false; }
      if (x.Length != y.Length) { return false; }
      for (int i = 0; i < x.Length; i++)
      {
        if (x[i].ParameterType != y[i].ParameterType) { return false; }
      }
      return true;
    }

    private static bool IsParameterMatch(ParameterInfo[] x, Type[] y)
    {
      if (x == null || y == null) { return false; }
      if (x.Length != y.Length) { return false; }
      for (int i = 0; i < x.Length; i++)
      {
        if (x[i].ParameterType != y[i]) { return false; }
      }
      return true;
    }

    #endregion
  }
}