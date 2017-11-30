using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  partial class ReflectUtils
  {
    #region ** enum ReflectMembersTokenType **

    private enum ReflectMembersTokenType
    {
      InstanceDeclaredAndPublicOnlyMembers,
      InstanceDeclaredOnlyMembers,

      InstancePublicOnlyMembers,
      InstanceMembers,

      TypeDeclaredAndPublicOnlyMembers,
      TypeDeclaredOnlyMembers,

      TypePublicOnlyMembers,
      TypeMembers,

      TypeFlattenHierarchyPublicOnlyMembers,
      TypeFlattenHierarchyMembers
    }

    #endregion

    #region * Fields Cache *

    private static readonly ConcurrentDictionary<Type, DictionaryCache<ReflectMembersTokenType, FieldInfo[]>> s_fieldsCache =
        new ConcurrentDictionary<Type, DictionaryCache<ReflectMembersTokenType, FieldInfo[]>>();

    #endregion

    #region * GetFieldsInternal *

    private static readonly Func<ReflectMembersTokenType, Type, FieldInfo[]> s_getTypeFieldsFunc = GetTypeFields;

    private static FieldInfo[] GetTypeFields(ReflectMembersTokenType reflectFieldsToken, Type type)
    {
      // Void*的基类就是null
      if (type == typeof(object) || type.BaseType == null) return EmptyArray<FieldInfo>.Instance;

      BindingFlags bindingFlags;
      switch (reflectFieldsToken)
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

      return type
#if !NET40
             .GetTypeInfo()
#endif
             .GetFields(bindingFlags);
    }

    [MethodImpl(InlineMethod.Value)]
    private static FieldInfo[] GetFieldsInternal(Type type, ReflectMembersTokenType reflectFieldsToken)
    {
      if (type == null) { return EmptyArray<FieldInfo>.Instance; }

      // 二级字典缓存
      var cache = s_fieldsCache.GetOrAdd(type, k => new DictionaryCache<ReflectMembersTokenType, FieldInfo[]>());
      return cache.GetItem(reflectFieldsToken, type, s_getTypeFieldsFunc);
    }

    #endregion

    /// <summary>GetInstanceDeclaredPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstanceDeclaredPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceDeclaredAndPublicOnlyMembers);

    /// <summary>GetInstanceDeclaredFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstanceDeclaredFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceDeclaredOnlyMembers);

    /// <summary>GetInstancePublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstancePublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstancePublicOnlyMembers);

    /// <summary>GetInstanceFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstanceFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceMembers);

    /// <summary>GetTypeDeclaredPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeDeclaredPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeDeclaredAndPublicOnlyMembers);

    /// <summary>GetTypeDeclaredFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeDeclaredFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeDeclaredOnlyMembers);

    /// <summary>GetTypePublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypePublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypePublicOnlyMembers);

    /// <summary>GetTypeFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeMembers);

    /// <summary>GetTypeFlattenHierarchyPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeFlattenHierarchyPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyPublicOnlyMembers);

    /// <summary>GetTypeFlattenHierarchyFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeFlattenHierarchyFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyMembers);

    #region - LookupTypeField -

    /// <summary>获取字段。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static FieldInfo LookupTypeField(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.ObjectType)
      {
        var fields = s_typeDeclaredFieldsCache.GetItem(type, s_getTypeDeclaredFieldsFunc);
        if (fields.TryGetValue(name, out FieldInfo field)) { return field; };

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    private static readonly DictionaryCache<Type, Dictionary<string, FieldInfo>> s_typeDeclaredFieldsCache =
        new DictionaryCache<Type, Dictionary<string, FieldInfo>>();
    private static readonly Func<Type, Dictionary<string, FieldInfo>> s_getTypeDeclaredFieldsFunc = GetTypeDeclaredFieldsInternal;
    private static Dictionary<string, FieldInfo> GetTypeDeclaredFieldsInternal(Type type)
    {
      //return GetTypeDeclaredFields(type).ToDictionary(_ => _.Name, StringComparer.Ordinal);
      var dic = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
      var fields = GetTypeDeclaredFields(type);
      foreach (var fi in fields)
      {
        dic[fi.Name] = fi;
      }
      return dic;
    }

    #endregion

    #region - LookupInstanceField -

    /// <summary>获取字段。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static FieldInfo LookupInstanceField(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeUtils._.Object)
      {
        var fields = s_instanceDeclaredFieldsCache.GetItem(type, s_getInstanceDeclaredFieldsFunc);
        if (fields.TryGetValue(name, out FieldInfo field)) { return field; };

        if (declaredOnly) { break; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
      }

      return null;
    }

    private static readonly DictionaryCache<Type, Dictionary<string, FieldInfo>> s_instanceDeclaredFieldsCache =
        new DictionaryCache<Type, Dictionary<string, FieldInfo>>();
    private static readonly Func<Type, Dictionary<string, FieldInfo>> s_getInstanceDeclaredFieldsFunc = GetInstanceDeclaredFieldsInternal;
    private static Dictionary<string, FieldInfo> GetInstanceDeclaredFieldsInternal(Type type)
    {
      //return GetTypeDeclaredFields(type).ToDictionary(_ => _.Name, StringComparer.Ordinal);
      var dic = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
      var fields = GetInstanceDeclaredFields(type);
      foreach (var fi in fields)
      {
        dic[fi.Name] = fi;
      }
      return dic;
    }

    #endregion
  }
}