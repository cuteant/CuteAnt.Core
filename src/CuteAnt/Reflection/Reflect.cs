﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using CuteAnt.Collections;
using CuteAnt.Text;
#if !NET40
using System.Runtime.CompilerServices;
#endif

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

  #region -- EmptyCtorDelegate --

  /// <summary>EmptyCtorDelegate</summary>
  /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
  /// <returns></returns>
  public delegate object EmptyCtorDelegate();

  #endregion

  /// <summary>反射工具类</summary>
  public static class Reflect
  {
    #region -- 属性 --

    /// <summary>当前反射提供者</summary>
    public static IReflect Provider { get; set; }

#if NETSTANDARD
    private static readonly Func<Type, object> GetUninitializedObjectDelegate;
#endif

    static Reflect()
    {
      Provider = new EmitReflect();

#if NETSTANDARD
      var formatterServices = typeof(string).GetTypeInfo().Assembly
          .GetType("System.Runtime.Serialization.FormatterServices");
      if (formatterServices != null)
      {
        var method = formatterServices.GetMethod("GetUninitializedObject");
        if (method != null)
            GetUninitializedObjectDelegate = (Func<Type, object>)method.CreateDelegate(typeof(Func<Type, object>));
      }
#endif
    }

    #endregion

    #region -- 反射获取 --

    #region - Type -

    /// <summary>根据名称获取类型。可搜索当前目录DLL，自动加载</summary>
    /// <param name="typeName">类型名</param>
    /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Type GetTypeEx(this String typeName, Boolean isLoadAssembly = true)
    {
      if (typeName.IsNullOrWhiteSpace()) { return null; }

      var type = Type.GetType(typeName);
      if (type != null) { return type; }

      return Provider.GetType(typeName, isLoadAssembly);
    }

    private static Dictionary<Type, object> s_defaultValueTypes = new Dictionary<Type, object>();
    /// <summary>GetDefaultValue</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetDefaultValue(this Type type)
    {
      if (type == null) { throw new ArgumentNullException(nameof(type)); }

      if (!type.IsValueType()) return null;

      object defaultValue;
      if (s_defaultValueTypes.TryGetValue(type, out defaultValue)) return defaultValue;

      defaultValue = Activator.CreateInstance(type);

      Dictionary<Type, object> snapshot, newCache;
      do
      {
        snapshot = s_defaultValueTypes;
        newCache = new Dictionary<Type, object>(s_defaultValueTypes);
        newCache[type] = defaultValue;
      } while (!ReferenceEquals(Interlocked.CompareExchange(ref s_defaultValueTypes, newCache, snapshot), snapshot));

      return defaultValue;
    }

    private static Dictionary<string, Type> s_genericTypeCache = new Dictionary<string, Type>();
    /// <summary>GetCachedGenericType</summary>
    /// <param name="type"></param>
    /// <param name="argTypes"></param>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <returns></returns>
    public static Type GetCachedGenericType(this Type type, params Type[] argTypes)
    {
      if (type == null) { throw new ArgumentNullException(nameof(type)); }

      if (!type.IsGenericTypeDefinition())
      {
        throw new ArgumentException($"{type.FullName} is not a Generic Type Definition", nameof(type));
      }

      if (argTypes == null) { argTypes = EmptyArray<Type>.Instance; }

      var sb = StringBuilderCache.Acquire().Append(type.FullName);

      foreach (var argType in argTypes)
      {
        sb.Append('|').Append(argType.FullName);
      }

      var key = StringBuilderCache.GetStringAndRelease(sb);

      Type genericType;
      if (s_genericTypeCache.TryGetValue(key, out genericType)) { return genericType; }

      genericType = type.MakeGenericType(argTypes);

      Dictionary<string, Type> snapshot, newCache;
      do
      {
        snapshot = s_genericTypeCache;
        newCache = new Dictionary<string, Type>(s_genericTypeCache);
        newCache[key] = genericType;
      } while (!ReferenceEquals(Interlocked.CompareExchange(ref s_genericTypeCache, newCache, snapshot), snapshot));

      return genericType;
    }

    #endregion

    #region * enum ReflectMembersTokenType *

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

    #region - GetEvent(s) -

    /// <summary>返回表示当前类型声明的指定公共事件的对象。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static EventInfo GetDeclaredEventEx(this Type type, String name)
    {
#if !NET40
      return type.GetTypeInfo().GetDeclaredEvent(name);
#else
      return type.GetEvent(name, BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    /// <summary>获取当前类型定义的操作的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<EventInfo> GetDeclaredEventsEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().DeclaredEvents;
#else
      return type.GetEvents(BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    #endregion

    #region - FieldInfo -

    /// <summary>返回表示当前类型声明的指定公共字段的对象</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    [Obsolete("=> GetFieldEx")]
    public static FieldInfo GetDeclaredFieldEx(this Type type, String name)
    {
#if !NET40
      return type.GetTypeInfo().GetDeclaredField(name);
#else
      return type.GetField(name, BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    /// <summary>获取当前类型定义的字段的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<FieldInfo> GetDeclaredFieldsEx(this Type type)
    {
      //#if !NET40
      //      return type.GetTypeInfo().DeclaredFields;
      //#else
      //      return type.GetFields(BindingFlagsHelper.MSDeclaredOnlyLookup);
      //#endif
      return GetFieldsInternal(type, ReflectMembersTokenType.TypeDeclaredOnlyMembers);
    }

    /// <summary>获取指定类型定义的属性的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetFieldsEx(this Type type) =>
        GetFieldsInternal(type, ReflectMembersTokenType.TypePublicOnlyMembers);

    /// <summary>获取指定类型定义的属性的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<FieldInfo> GetRuntimeFieldsEx(this Type type) =>
        GetFieldsInternal(type, ReflectMembersTokenType.TypeMembers);

    #region * Fields Cache *

    private static readonly DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, FieldInfo[]>> s_fieldsCache =
        new DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, FieldInfo[]>>();

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

    private static FieldInfo[] GetFieldsInternal(Type type, ReflectMembersTokenType reflectFieldsToken)
    {
      if (type == null) { return EmptyArray<FieldInfo>.Instance; }

      // 二级字典缓存
      var cache = s_fieldsCache.GetItem(type, k => new DictionaryCache<ReflectMembersTokenType, FieldInfo[]>());
      return cache.GetItem(reflectFieldsToken, type, s_getTypeFieldsFunc);
    }

    #endregion

    /// <summary>GetInstanceDeclaredPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetInstanceDeclaredPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceDeclaredAndPublicOnlyMembers);

    /// <summary>GetInstanceDeclaredFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetInstanceDeclaredFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceDeclaredOnlyMembers);

    /// <summary>GetInstancePublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetInstancePublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstancePublicOnlyMembers);

    /// <summary>GetInstanceFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetInstanceFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceMembers);

    /// <summary>GetTypeDeclaredPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetTypeDeclaredPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeDeclaredAndPublicOnlyMembers);

    /// <summary>GetTypeDeclaredFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetTypeDeclaredFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeDeclaredOnlyMembers);

    /// <summary>GetTypePublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetTypePublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypePublicOnlyMembers);

    /// <summary>GetTypeFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetTypeFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeMembers);

    /// <summary>GetTypeFlattenHierarchyPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetTypeFlattenHierarchyPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyPublicOnlyMembers);

    /// <summary>GetTypeFlattenHierarchyFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo[] GetTypeFlattenHierarchyFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyMembers);

    #region - GetTypeField -

    /// <summary>获取字段。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static FieldInfo GetFieldEx(this Type type, string name, bool declaredOnly = false) => GetTypeField(type, name, declaredOnly);

    /// <summary>获取字段。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static FieldInfo GetTypeField(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        FieldInfo field;
        var fields = s_typeDeclaredFieldsCache.GetItem(type, s_getTypeDeclaredFieldsFunc);
        if (fields.TryGetValue(name, out field)) { return field; };

        if (declaredOnly) { break; }

        type = type.BaseType();
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

    #region - GetInstanceField -

    /// <summary>获取字段。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static FieldInfo GetInstanceField(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        FieldInfo field;
        var fields = s_instanceDeclaredFieldsCache.GetItem(type, s_getInstanceDeclaredFieldsFunc);
        if (fields.TryGetValue(name, out field)) { return field; };

        if (declaredOnly) { break; }

        type = type.BaseType();
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

    #endregion

    #region - PropertyInfo -

    /// <summary>返回表示当前类型声明的公共属性的对象</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    [Obsolete("=> GetPropertyEx")]
    public static PropertyInfo GetDeclaredPropertyEx(this Type type, String name)
    {
#if !NET40
      return type.GetTypeInfo().GetDeclaredProperty(name);
#else
      return type.GetProperty(name, BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    /// <summary>获取指定类型定义的属性的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<PropertyInfo> GetDeclaredPropertiesEx(this Type type)
    {
      //#if !NET40
      //      return type.GetTypeInfo().DeclaredProperties;
      //#else
      //      return type.GetProperties(BindingFlagsHelper.MSDeclaredOnlyLookup);
      //#endif
      return GetPropertiesInternal(type, false, ReflectMembersTokenType.TypeDeclaredOnlyMembers);
    }

    /// <summary>获取指定类型定义的属性的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetPropertiesEx(this Type type) =>
        GetPropertiesInternal(type, false, ReflectMembersTokenType.TypePublicOnlyMembers);

    /// <summary>获取指定类型定义的属性的集合</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<PropertyInfo> GetRuntimePropertiesEx(this Type type) =>
        GetPropertiesInternal(type, false, ReflectMembersTokenType.TypeMembers);

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
      if (type.IsInterface())
      {
        var propertyInfos = new List<PropertyInfo>();

        var considered = new List<Type>();
        var queue = new Queue<Type>();
        considered.Add(type);
        queue.Enqueue(type);

        while (queue.Count > 0)
        {
          var subType = queue.Dequeue();
          foreach (var subInterface in subType.GetTypeInterfaces())
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

      if (type.IsInterface())
      {
        var propertyInfos = new List<PropertyInfo>();

        var considered = new List<Type>();
        var queue = new Queue<Type>();
        considered.Add(type);
        queue.Enqueue(type);

        while (queue.Count > 0)
        {
          var subType = queue.Dequeue();
          foreach (var subInterface in subType.GetTypeInterfaces())
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

    private static PropertyInfo[] GetPropertiesInternal(Type type, bool ignoreIndexedProperties, ReflectMembersTokenType reflectPropertiesToken)
    {
      if (type == null) { return EmptyArray<PropertyInfo>.Instance; }

      if (type.IsInterface()) { ignoreIndexedProperties = false; }
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
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetInstanceDeclaredPublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.InstanceDeclaredAndPublicOnlyMembers);

    /// <summary>GetInstanceDeclaredProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetInstanceDeclaredProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.InstanceDeclaredOnlyMembers);

    /// <summary>GetInstanceProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetInstanceProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.InstanceMembers);

    /// <summary>GetTypeDeclaredPublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetTypeDeclaredPublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeDeclaredAndPublicOnlyMembers);

    /// <summary>GetTypeDeclaredProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetTypeDeclaredProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeDeclaredOnlyMembers);

    /// <summary>GetTypePublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetTypePublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypePublicOnlyMembers);

    /// <summary>GetTypeProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetTypeProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeMembers);

    /// <summary>GetTypeFlattenHierarchyPublicProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetTypeFlattenHierarchyPublicProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeFlattenHierarchyPublicOnlyMembers);

    /// <summary>GetTypeFlattenHierarchyProperties</summary>
    /// <param name="type"></param>
    /// <param name="ignoreIndexedProperties"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo[] GetTypeFlattenHierarchyProperties(this Type type, bool ignoreIndexedProperties = false) =>
        GetPropertiesInternal(type, ignoreIndexedProperties, ReflectMembersTokenType.TypeFlattenHierarchyMembers);

    #region - GetTypeProperty -

    /// <summary>获取属性。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyEx(this Type type, string name, bool declaredOnly = false) => GetTypeProperty(type, name, declaredOnly);

    /// <summary>获取属性。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo GetTypeProperty(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        PropertyInfo property;
        var properties = s_typeDeclaredPropertiesCache.GetItem(type, s_getTypeDeclaredPropertiesFunc);
        if (properties.TryGetValue(name, out property)) { return property; };

        if (declaredOnly) { break; }

        type = type.BaseType();
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
    public static PropertyInfo GetTypeProperty(this Type type, string name, Type returnType, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (returnType == null) throw new ArgumentNullException(nameof(returnType));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var properties = GetTypeDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType);
        if (property != null) { return property; }

        if (declaredOnly) { break; }

        type = type.BaseType();
      }

      return null;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo GetTypeProperty(this Type type, string name, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var properties = GetTypeDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

        type = type.BaseType();
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
    public static PropertyInfo GetTypeProperty(this Type type, string name, Type returnType, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var properties = GetTypeDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

        type = type.BaseType();
      }

      return null;
    }

    #endregion

    #region - GetInstanceProperty -

    /// <summary>获取属性。</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo GetInstanceProperty(this Type type, string name, bool declaredOnly = false)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        PropertyInfo property;
        var properties = s_instanceDeclaredPropertiesCache.GetItem(type, s_getInstanceDeclaredPropertiesFunc);
        if (properties.TryGetValue(name, out property)) { return property; };

        if (declaredOnly) { break; }

        type = type.BaseType();
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
    public static PropertyInfo GetInstanceProperty(this Type type, string name, Type returnType, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (returnType == null) throw new ArgumentNullException(nameof(returnType));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var properties = GetInstanceDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType);
        if (property != null) { return property; }

        if (declaredOnly) { break; }

        type = type.BaseType();
      }

      return null;
    }

    /// <summary>获取属性。</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="parameterTypes"></param>
    /// <param name="declaredOnly"></param>
    /// <returns></returns>
    public static PropertyInfo GetInstanceProperty(this Type type, string name, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var properties = GetInstanceDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

        type = type.BaseType();
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
    public static PropertyInfo GetInstanceProperty(this Type type, string name, Type returnType, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var properties = GetInstanceDeclaredProperties(type);
        var property = properties.FirstOrDefault(_ => string.Equals(_.Name, name, StringComparison.Ordinal) &&
                                                      _.PropertyType == returnType &&
                                                      IsParameterMatch(_.GetIndexParameters(), parameterTypes));
        if (property != null) { return property; }

        if (declaredOnly) { break; }

        type = type.BaseType();
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
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }
      if (type == null) { throw new ArgumentNullException(nameof(type)); }

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

    #endregion

    #region - GetMemberEx -

    /// <summary>获取成员。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static MemberInfo GetMemberEx(this Type type, String name)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      var property = GetPropertyEx(type, name);
      if (property != null) { return property; }

      var field = GetFieldEx(type, name);
      if (field != null) { return field; }

      // 通过反射获取
      while (type != null && type != TypeX._.Object)
      {
        var fs = type.GetMember(name, BindingFlagsHelper.DefaultDeclaredOnlyLookup);
        if (fs != null && fs.Length > 0) { return fs[0]; }

        type = type.BaseType();
      }

      return null;
    }

    #endregion

    #region - MethodInfo -

    /// <summary>返回表示当前类型声明的指定公共方法的对象</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static MethodInfo GetDeclaredMethodEx(this Type type, String name)
    {
#if !NET40
      return type.GetTypeInfo().GetDeclaredMethod(name);
#else
      return type.GetMethod(name, BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    /// <summary>获取当前类型定义方法的集合</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<MethodInfo> GetDeclaredMethodsEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().DeclaredMethods;
#else
      return type.GetMethods(BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    /// <summary>返回包含在当前类型声明的所有公共方法与指定的名称的集合</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<MethodInfo> GetDeclaredMethodsEx(this Type type, String name)
    {
#if !NET40
      return type.GetTypeInfo().GetDeclaredMethods(name);
#else
      return type.GetMethods(BindingFlagsHelper.MSDeclaredOnlyLookup).Where(m => m.Name == name);
#endif
    }

    /// <summary>获取方法</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static MethodInfo GetMethodEx(this Type type, String name, params Type[] paramTypes)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      return Provider.GetMethod(type, name, paramTypes);
    }

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    public static MethodInfo[] GetMethodsEx(this Type type, String name, Int32 paramCount = -1)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      return Provider.GetMethods(type, name, paramCount);
    }

    #endregion

    #endregion

    #region -- 反射调用 --

    #region - CreateInstance -

    /// <summary>Creates a new instance from the default constructor of type</summary>
    public static object CreateInstance(this Type type)
    {
      if (type == null) { return null; }

      var ctorFn = GetConstructorMethod(type);
      return ctorFn();
    }

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T CreateInstance<T>(this Type type)
    {
      if (type == null) { return default(T); }

      var ctorFn = GetConstructorMethod(type);
      return (T)ctorFn();
    }

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static object CreateInstance(string typeName)
    {
      if (typeName == null) { return null; }

      var ctorFn = GetConstructorMethod(typeName);
      return ctorFn();
    }

    #endregion

    #region - GetConstructorMethod -

    private static Dictionary<Type, EmptyCtorDelegate> s_constructorMethods = new Dictionary<Type, EmptyCtorDelegate>();
    /// <summary>GetConstructorMethod</summary>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    public static EmptyCtorDelegate GetConstructorMethod(Type type)
    {
      EmptyCtorDelegate emptyCtorFn;
      if (s_constructorMethods.TryGetValue(type, out emptyCtorFn)) return emptyCtorFn;

      emptyCtorFn = GetConstructorMethodToCache(type);

      Dictionary<Type, EmptyCtorDelegate> snapshot, newCache;
      do
      {
        snapshot = s_constructorMethods;
        newCache = new Dictionary<Type, EmptyCtorDelegate>(s_constructorMethods);
        newCache[type] = emptyCtorFn;

      } while (!ReferenceEquals(
          Interlocked.CompareExchange(ref s_constructorMethods, newCache, snapshot), snapshot));

      return emptyCtorFn;
    }

    private static Dictionary<string, EmptyCtorDelegate> s_typeNamesMap = new Dictionary<string, EmptyCtorDelegate>();
    /// <summary>GetConstructorMethod</summary>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static EmptyCtorDelegate GetConstructorMethod(string typeName)
    {
      EmptyCtorDelegate emptyCtorFn;
      if (s_typeNamesMap.TryGetValue(typeName, out emptyCtorFn)) return emptyCtorFn;

      var type = typeName.GetTypeEx();
      if (type == null) return null;
      emptyCtorFn = GetConstructorMethodToCache(type);

      Dictionary<string, EmptyCtorDelegate> snapshot, newCache;
      do
      {
        snapshot = s_typeNamesMap;
        newCache = new Dictionary<string, EmptyCtorDelegate>(s_typeNamesMap);
        newCache[typeName] = emptyCtorFn;

      } while (!ReferenceEquals(
          Interlocked.CompareExchange(ref s_typeNamesMap, newCache, snapshot), snapshot));

      return emptyCtorFn;
    }

    #endregion

    #region - GetConstructorMethodToCache -

    /// <summary>GetConstructorMethodToCache</summary>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    public static EmptyCtorDelegate GetConstructorMethodToCache(Type type)
    {
      if (type == TypeX._.String)
      {
        return () => string.Empty;
      }
      else if (type.IsInterface())
      {
        if (type.HasGenericType())
        {
          var genericType = type.GetTypeWithGenericTypeDefinitionOfAny(typeof(IDictionary<,>));

          if (genericType != null)
          {
            var keyType = genericType.GenericTypeArguments()[0];
            var valueType = genericType.GenericTypeArguments()[1];
            return GetConstructorMethodToCache(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
          }

          genericType = type.GetTypeWithGenericTypeDefinitionOfAny(
              typeof(IEnumerable<>),
              typeof(ICollection<>),
              typeof(IList<>));

          if (genericType != null)
          {
            var elementType = genericType.GenericTypeArguments()[0];
            return GetConstructorMethodToCache(typeof(List<>).MakeGenericType(elementType));
          }
        }
      }
      else if (type.IsArray)
      {
        return () => Array.CreateInstance(type.GetElementType(), 0);
      }
      else if (type.IsGenericTypeDefinition())
      {
#if NETSTANDARD
        var genericArgs = type.GetTypeInfo().GenericTypeParameters;
#else
        var genericArgs = type.GetGenericArguments();
#endif
        var typeArgs = new Type[genericArgs.Length];
        for (var i = 0; i < genericArgs.Length; i++)
          typeArgs[i] = typeof(object);

        var realizedType = type.MakeGenericType(typeArgs);

        return realizedType.CreateInstance;
      }

      var emptyCtor = type.GetEmptyConstructor();
      if (emptyCtor != null)
      {
#if __IOS__ || XBOX || NETFX_CORE
        return () => Activator.CreateInstance(type);
#elif WP || PCL || NETSTANDARD
        System.Linq.Expressions.Expression conversion = Expression.Convert(
            System.Linq.Expressions.Expression.New(type), typeof(object));

        return System.Linq.Expressions.Expression.Lambda<EmptyCtorDelegate>(conversion).Compile();
#else

#if SL5
        var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes);
#else
        var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof(Reflect).Module, true);
#endif
        var ilgen = dm.GetILGenerator();
        ilgen.Emit(System.Reflection.Emit.OpCodes.Nop);
        ilgen.Emit(System.Reflection.Emit.OpCodes.Newobj, emptyCtor);
        ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);

        return (EmptyCtorDelegate)dm.CreateDelegate(typeof(EmptyCtorDelegate));
#endif
      }

#if (SL5 && !WP) || XBOX
      return () => Activator.CreateInstance(type);
#elif NETSTANDARD
      if (GetUninitializedObjectDelegate != null)
        return () => GetUninitializedObjectDelegate(type);

      return () => Activator.CreateInstance(type);
#elif WP || PCL
      return System.Linq.Expressions.Expression.Lambda<EmptyCtorDelegate>(
          System.Linq.Expressions.Expression.New(type)).Compile();
#else
      //Anonymous types don't have empty constructors
      //return () => FormatterServices.GetUninitializedObject(type);
      return () => TypeX.Create(type).CreateInstance();
#endif
    }

    #endregion

    #region - Invoke -

    /// <summary>反射调用指定对象的方法。target为类型时调用其静态方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="name">方法名</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Object Invoke(this Object target, String name, params Object[] parameters)
    {
      ValidationHelper.ArgumentNull(target, "target");
      ValidationHelper.ArgumentNullOrEmpty(name, "name");

      Object value = null;
      if (TryInvoke(target, name, out value, parameters)) { return value; }

      var type = GetTypeInternal(ref target);
      throw new HmExceptionBase("类{0}中找不到名为{1}的方法！", type, name);
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerHidden]
    public static Object Invoke(this Object target, MethodBase method, params Object[] parameters)
    {
      //ValidationHelper.ArgumentNull(target, "target");
      //ValidationHelper.ArgumentNull(method, "method");
      ValidationHelper.ArgumentNull(method, "method");
      if (!method.IsStatic)
      {
        ValidationHelper.ArgumentNull(target, "target");
      }

      return Provider.Invoke(target, method, parameters);
    }

    #endregion

    #region - TryInvoke -

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="name">方法名</param>
    /// <param name="value">数值</param>
    /// <param name="parameters">方法参数</param>
    /// <remarks>反射调用是否成功</remarks>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Boolean TryInvoke(this Object target, String name, out Object value, params Object[] parameters)
    {
      value = null;

      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetTypeInternal(ref target);

      // 参数类型数组
      var list = new List<Type>();
      foreach (var item in parameters)
      {
        Type t = null;
        if (item != null) { t = item.GetType(); }
        list.Add(t);
      }

      // 如果参数数组出现null，则无法精确匹配，可按参数个数进行匹配
      var method = GetMethodEx(type, name, list.ToArray());
      if (method == null) { return false; }

      value = Invoke(target, method, parameters);
      return true;
    }

    #endregion

    #region - InvokeWithParams -

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数字典</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object InvokeWithParams(this Object target, MethodBase method, IDictionary parameters)
    {
      //if (target == null) throw new ArgumentNullException("target");
      if (method == null) throw new ArgumentNullException("method");
      if (!method.IsStatic && target == null) throw new ArgumentNullException("target");

      return Provider.InvokeWithParams(target, method, parameters);
    }

    #endregion

    #region - GetValue -

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="throwOnError">出错时是否抛出异常</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static object GetMemberInfoValue(this object target, string name, bool throwOnError = true)
    {
      if (target == null) { throw new ArgumentNullException(nameof(target)); }
      ValidationHelper.ArgumentNullOrEmpty(name, nameof(name));

      object value = null;
      if (TryGetMemberInfoValue(target, name, out value)) { return value; }

      if (!throwOnError) { return null; }

      var type = GetTypeInternal(ref target);
      throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
    }

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns>是否成功获取数值</returns>
    public static bool TryGetMemberInfoValue(this object target, string name, out object value)
    {
      if (target == null) { throw new ArgumentNullException(nameof(target)); }

      value = null;

      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetTypeInternal(ref target);
      var pi = GetTypeProperty(type, name);
      if (pi != null)
      {
        return TryGetPropertyInfoValue(target, pi, out value);
      }

      var fi = GetTypeField(type, name);
      if (fi != null)
      {
        value = GetFieldInfoValue(target, fi);
        return true;
      }

      return false;
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <returns></returns>
    public static object GetMemberInfoValue(this object target, MemberInfo member)
    {
      var property = member as PropertyInfo;
      if (property != null) { return GetPropertyInfoValue(target, property); }
      var field = member as FieldInfo;
      if (field != null) { return GetFieldInfoValue(target, field); }

      throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetMemberInfoValue(this object target, MemberInfo member, out object value)
    {
      var property = member as PropertyInfo;
      if (property != null)
      {
        return TryGetPropertyInfoValue(target, property, out value);
      }
      var field = member as FieldInfo;
      if (field != null)
      {
        value = GetFieldInfoValue(target, field);
        return true;
      }

      value = null;
      return false;
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <returns></returns>
    public static object GetPropertyInfoValue(this object target, PropertyInfo property)
    {
      return GetValueGetter(property)?.Invoke(target);
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetPropertyInfoValue(this object target, PropertyInfo property, out object value)
    {
      var getter = GetValueGetter(property);

      if (getter != null)
      {
        value = getter(target);
        return true;
      }
      else
      {
        value = null;
        return false;
      }
    }

    /// <summary>获取目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <returns></returns>
    public static object GetFieldInfoValue(this object target, FieldInfo field)
    {
      return GetValueGetter(field).Invoke(target);
    }

    #endregion

    #region - SetValue -

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static void SetMemberInfoValue(this object target, string name, object value)
    {
      if (!TrySetMemberInfoValue(target, name, value))
      {
        var type = GetTypeInternal(ref target);
        throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
      }
    }

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static bool TrySetMemberInfoValue(this object target, string name, object value)
    {
      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetTypeInternal(ref target);
      var pi = GetTypeProperty(type, name);
      if (pi != null) { SetPropertyInfoValue(target, pi, value); return true; }

      var fi = GetTypeField(type, name);
      if (fi != null) { SetFieldInfoValue(target, fi, value); return true; }

      return false;
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static void SetMemberInfoValue(this object target, MemberInfo member, object value)
    {
      var property = member as PropertyInfo;
      if (property != null) { SetPropertyInfoValue(target, property, value); return; }
      var field = member as FieldInfo;
      if (field != null) { SetFieldInfoValue(target, field, value); return; }

      throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static bool TrySetMemberInfoValue(this object target, MemberInfo member, object value)
    {
      var property = member as PropertyInfo;
      if (property != null) { SetPropertyInfoValue(target, property, value); return true; }
      var field = member as FieldInfo;
      if (field != null) { SetFieldInfoValue(target, field, value); return true; }

      return false;
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static void SetPropertyInfoValue(this object target, PropertyInfo property, object value)
    {
      GetValueSetter(property)?.Invoke(target, value);
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static bool TrySetPropertyInfoValue(this object target, PropertyInfo property, object value)
    {
      var setter = GetValueSetter(property);
      if (setter != null)
      {
        setter(target, value);
        return true;
      }
      return false;
    }

    /// <summary>设置目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    public static void SetFieldInfoValue(this object target, FieldInfo field, object value)
    {
      GetValueSetter(field).Invoke(target, value);
    }

    #endregion

    #region - GetValueGetter for PropertyInfo -

    private static readonly DictionaryCache<PropertyInfo, Func<object, object>> s_propertiesValueGetterCache =
        new DictionaryCache<PropertyInfo, Func<object, object>>();
    private static readonly Func<PropertyInfo, Func<object, object>> s_propertyInfoGetValueGetterFunc = GetValueGetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return s_propertiesValueGetterCache.GetItem(propertyInfo, s_propertyInfoGetValueGetterFunc);
    }

    private static Func<object, object> GetValueGetterInternal(PropertyInfo propertyInfo)
    {
#if NETFX_CORE
      var getMethodInfo = propertyInfo.GetMethod;
      if (getMethodInfo == null) return null;
      return x => getMethodInfo.Invoke(x, TypeConstants.EmptyObjectArray);
#elif (SL5 && !WP) || __IOS__ || XBOX
      var getMethodInfo = propertyInfo.GetGetMethod();
      if (getMethodInfo == null) return null;
      return x => getMethodInfo.Invoke(x, TypeConstants.EmptyObjectArray);
#else
      if (!propertyInfo.CanRead) { return null; }

      var method = propertyInfo.GetMethodInfo();
      if (method == null) { return null; }
      if (method.IsStatic)
      {
        //定义一个没有名字的动态方法
        var setter = CreateDynamicGetMethod<object>(propertyInfo);
        var generator = setter.GetILGenerator();

        //if (!method.IsStatic) il.Ldarg(0).CastFromObject(method.DeclaringType);

        // 目标方法没有参数
        generator.Call(method)
            .BoxIfValueType(method.ReturnType)
            .Ret();

        return (Func<object, object>)setter.CreateDelegate(typeof(Func<object, object>));
      }
      else
      {
        var instance = Expression.Parameter(typeof(object), "i");
        var convertInstance = Expression.TypeAs(instance, propertyInfo.GetDeclaringType());
        var property = Expression.Property(convertInstance, propertyInfo);
        var convertProperty = Expression.TypeAs(property, typeof(object));
        return Expression.Lambda<Func<object, object>>(convertProperty, instance).Compile();
      }
#endif
    }

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return StaticMemberAccessors<T>.GetValueGetter(propertyInfo);
    }

    #endregion

    #region - GetValueSetter for PropertyInfo -

    private static readonly DictionaryCache<PropertyInfo, Action<object, object>> s_propertiesValueSetterCache =
        new DictionaryCache<PropertyInfo, Action<object, object>>();
    private static readonly Func<PropertyInfo, Action<object, object>> s_propertyInfoGetValueSetterFunc = GetValueSetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static Action<object, object> GetValueSetter(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return s_propertiesValueSetterCache.GetItem(propertyInfo, s_propertyInfoGetValueSetterFunc);
    }

    /// <summary>GetValueSetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    private static Action<object, object> GetValueSetterInternal(PropertyInfo propertyInfo)
    {
      if (!propertyInfo.CanWrite) { return null; }

      var method = propertyInfo.SetMethodInfo();
      if (method == null) { return null; }
      if (method.IsStatic)
      {
        //定义一个没有名字的动态方法
        var setter = CreateDynamicSetMethod<object>(propertyInfo);
        var il = setter.GetILGenerator();

        //if (!method.IsStatic) il.Ldarg(0).CastFromObject(method.DeclaringType);

        // 目标方法只有一个参数
        il.Ldarg(1)
            .CastFromObject(propertyInfo.PropertyType)
            .Call(method)
            .Ret();

        return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
      }
      else
      {
        var instance = Expression.Parameter(typeof(object), "i");
        var argument = Expression.Parameter(typeof(object), "a");

        var type = (Expression)Expression.TypeAs(instance, propertyInfo.GetDeclaringType());

        var setterCall = Expression.Call(
            type,
            propertyInfo.SetMethodInfo(),
            GetCastOrConvertExpression(argument, propertyInfo.PropertyType));

        return Expression.Lambda<Action<object, object>>(setterCall, instance, argument).Compile();
      }
    }

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return StaticMemberAccessors<T>.GetValueSetter(propertyInfo);
    }

    #endregion

    #region - GetValueGetter for FieldInfo -

    private static readonly DictionaryCache<FieldInfo, Func<object, object>> s_fieldsValueGetterCache =
        new DictionaryCache<FieldInfo, Func<object, object>>();
    private static readonly Func<FieldInfo, Func<object, object>> s_fieldInfoGetValueGetterFunc = GetValueGetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static Func<object, object> GetValueGetter(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return s_fieldsValueGetterCache.GetItem(fieldInfo, s_fieldInfoGetValueGetterFunc);
    }

    private static Func<object, object> GetValueGetterInternal(FieldInfo fieldInfo)
    {
#if (SL5 && !WP) || __IOS__ || XBOX
      return x => fieldInfo.GetValue(x);
#else
      if (fieldInfo.IsStatic)
      {
        //定义一个没有名字的动态方法
        var getter = CreateDynamicGetMethod<object>(fieldInfo);
        var il = getter.GetILGenerator();

        // 必须考虑对象是值类型的情况，需要拆箱
        // 其它地方看到的程序从来都没有人处理
        il.Ldarg(0)
            .CastFromObject(fieldInfo.DeclaringType)
            .Ldfld(fieldInfo)
            .BoxIfValueType(fieldInfo.FieldType)
            .Ret();

        return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
      }
      else
      {
        var instance = Expression.Parameter(typeof(object), "i");
        var field = Expression.Field(Expression.TypeAs(instance, fieldInfo.GetDeclaringType()), fieldInfo);
        var convertField = Expression.TypeAs(field, typeof(object));
        return Expression.Lambda<Func<object, object>>(convertField, instance).Compile();
      }
#endif
    }

    /// <summary>GetValueGetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static Func<T, object> GetValueGetter<T>(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return StaticMemberAccessors<T>.GetValueGetter(fieldInfo);
    }

    #endregion

    #region - GetValueSetter for FieldInfo -

    private static readonly DictionaryCache<FieldInfo, Action<object, object>> s_fieldsValueSetterCache =
        new DictionaryCache<FieldInfo, Action<object, object>>();
    private static readonly Func<FieldInfo, Action<object, object>> s_fieldInfoGetValueSetterFunc = GetValueSetterInternal;

    /// <summary>GetValueSetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static Action<object, object> GetValueSetter(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return s_fieldsValueSetterCache.GetItem(fieldInfo, s_fieldInfoGetValueSetterFunc);
    }

    private static Action<object, object> GetValueSetterInternal(FieldInfo fieldInfo)
    {
      if (fieldInfo.IsStatic)
      {
        //定义一个没有名字的动态方法
        var setter = CreateDynamicSetMethod<object>(fieldInfo);
        var il = setter.GetILGenerator();

        // 必须考虑对象是值类型的情况，需要拆箱
        // 其它地方看到的程序从来都没有人处理
        // 值类型是不支持这样子赋值的，暂时没有找到更好的替代方法
        il.Ldarg(0)
            .CastFromObject(fieldInfo.DeclaringType)
            .Ldarg(1);
        var method = GetFieldMethod(fieldInfo.FieldType);
        if (method != null)
        {
          il.Call(method);
        }
        else
        {
          il.CastFromObject(fieldInfo.FieldType);
        }
        il.Emit(OpCodes.Stfld, fieldInfo);
        il.Emit(OpCodes.Ret);
        return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
      }
      else
      {
        var instance = Expression.Parameter(typeof(object), "i");
        var argument = Expression.Parameter(typeof(object), "a");

        var field = Expression.Field(Expression.TypeAs(instance, fieldInfo.GetDeclaringType()), fieldInfo);

        var setterCall = Expression.Assign(
            field,
            GetCastOrConvertExpression(argument, fieldInfo.FieldType));

        return Expression.Lambda<Action<object, object>>(setterCall, instance, argument).Compile();

        //var fieldDeclaringType = fieldInfo.GetDeclaringType();

        //var sourceParameter = Expression.Parameter(typeof(object), "source");
        //var valueParameter = Expression.Parameter(typeof(object), "value");

        //var fieldExpression = Expression.Field(GetCastOrConvertExpression(sourceParameter, fieldDeclaringType), fieldInfo);

        //var valueExpression = GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

        //var genericSetFieldMethodInfo = s_setFieldMethod.MakeGenericMethod(fieldExpression.Type);

        //var setFieldMethodCallExpression = Expression.Call(
        //    null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

        //return Expression.Lambda<Action<object, object>>(setFieldMethodCallExpression, sourceParameter, valueParameter).Compile();
      }
    }
    private static MethodInfo GetFieldMethod(Type type)
    {
      String name = "To" + type.Name;
      return typeof(Convert).GetMethod(name, new Type[] { typeof(Object) });
    }

    /// <summary>GetValueSetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static Action<T, object> GetValueSetter<T>(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return StaticMemberAccessors<T>.GetValueSetter(fieldInfo);
    }

    #endregion

    #region * class StaticMemberAccessors<T> *

    static class StaticMemberAccessors<T>
    {
      #region GetValueGetter for PropertyInfo

      private static readonly DictionaryCache<PropertyInfo, Func<T, object>> s_propertiesValueGetterCache =
          new DictionaryCache<PropertyInfo, Func<T, object>>();
      private static readonly Func<PropertyInfo, Func<T, object>> s_propertyInfoGetValueGetterFunc = GetValueGetterInternal;

      public static Func<T, object> GetValueGetter(PropertyInfo propertyInfo) =>
          s_propertiesValueGetterCache.GetItem(propertyInfo, s_propertyInfoGetValueGetterFunc);

      private static Func<T, object> GetValueGetterInternal(PropertyInfo propertyInfo)
      {
#if NETFX_CORE
      var getMethodInfo = propertyInfo.GetMethod;
      if (getMethodInfo == null) return null;
      return x => getMethodInfo.Invoke(x, TypeConstants.EmptyObjectArray);
#elif (SL5 && !WP) || __IOS__ || XBOX
      var getMethodInfo = propertyInfo.GetGetMethod();
      if (getMethodInfo == null) return null;
      return x => getMethodInfo.Invoke(x, TypeConstants.EmptyObjectArray);
#else
        if (!propertyInfo.CanRead) { return null; }

        var method = propertyInfo.GetMethodInfo();
        if (method == null) { return null; }
        if (method.IsStatic)
        {
          //定义一个没有名字的动态方法
          var getter = CreateDynamicGetMethod<T>(propertyInfo);
          var il = getter.GetILGenerator();

          //if (!method.IsStatic) il.Ldarg(0).CastFromObject(method.DeclaringType);

          // 目标方法没有参数
          il.Call(method)
              .BoxIfValueType(method.ReturnType)
              .Ret();

          return (Func<T, object>)getter.CreateDelegate(typeof(Func<T, object>));
        }
        else
        {
          var instance = Expression.Parameter(typeof(T), "i");
          var propertyDeclaringType = propertyInfo.GetDeclaringType();
          var property = typeof(T) != propertyDeclaringType
              ? Expression.Property(Expression.TypeAs(instance, propertyDeclaringType), propertyInfo)
              : Expression.Property(instance, propertyInfo);
          var convertProperty = Expression.TypeAs(property, typeof(object));
          return Expression.Lambda<Func<T, object>>(convertProperty, instance).Compile();
        }
#endif
      }

      #endregion

      #region GetValueSetter for PropertyInfo

      private static readonly DictionaryCache<PropertyInfo, Action<T, object>> s_propertiesValueSetterCache =
          new DictionaryCache<PropertyInfo, Action<T, object>>();
      private static readonly Func<PropertyInfo, Action<T, object>> s_propertyInfoGetValueSetterFunc = GetValueSetterInternal;

      public static Action<T, object> GetValueSetter(PropertyInfo propertyInfo) =>
          s_propertiesValueSetterCache.GetItem(propertyInfo, s_propertyInfoGetValueSetterFunc);

      private static Action<T, object> GetValueSetterInternal(PropertyInfo propertyInfo)
      {
        if (!propertyInfo.CanWrite) { return null; }

        var method = propertyInfo.SetMethodInfo();
        if (method == null) { return null; }
        if (method.IsStatic)
        {
          //定义一个没有名字的动态方法
          var setter = CreateDynamicSetMethod<T>(propertyInfo);
          var il = setter.GetILGenerator();

          //if (!method.IsStatic) il.Ldarg(0).CastFromObject(method.DeclaringType);

          // 目标方法只有一个参数
          il.Ldarg(1)
              .CastFromObject(propertyInfo.PropertyType)
              .Call(method)
              .Ret();

          return (Action<T, object>)setter.CreateDelegate(typeof(Action<T, object>));
        }
        else
        {
          var instance = Expression.Parameter(typeof(T), "i");
          var argument = Expression.Parameter(typeof(object), "a");

          var propertyDeclaringType = propertyInfo.GetDeclaringType();
          var instanceType = typeof(T) != propertyDeclaringType
              ? (Expression)Expression.TypeAs(instance, propertyDeclaringType)
              : instance;

          var setterCall = Expression.Call(
              instanceType,
              propertyInfo.SetMethodInfo(),
              GetCastOrConvertExpression(argument, propertyInfo.PropertyType));

          return Expression.Lambda<Action<T, object>>(setterCall, instance, argument).Compile();
        }
      }

      #endregion

      #region GetValueGetter for FieldInfo

      private static readonly DictionaryCache<FieldInfo, Func<T, object>> s_fieldsValueGetterCache =
          new DictionaryCache<FieldInfo, Func<T, object>>();
      private static readonly Func<FieldInfo, Func<T, object>> s_fieldInfoGetValueGetterFunc = GetValueGetterInternal;

      public static Func<T, object> GetValueGetter(FieldInfo fieldInfo) =>
          s_fieldsValueGetterCache.GetItem(fieldInfo, s_fieldInfoGetValueGetterFunc);

      private static Func<T, object> GetValueGetterInternal(FieldInfo fieldInfo)
      {
#if (SL5 && !WP) || __IOS__ || XBOX
        return x => fieldInfo.GetValue(x);
#else
        if (fieldInfo.IsStatic)
        {
          //定义一个没有名字的动态方法
          var getter = CreateDynamicGetMethod<T>(fieldInfo);
          var il = getter.GetILGenerator();

          // 必须考虑对象是值类型的情况，需要拆箱
          // 其它地方看到的程序从来都没有人处理
          il.Ldarg(0)
              .CastFromObject(fieldInfo.DeclaringType)
              .Ldfld(fieldInfo)
              .BoxIfValueType(fieldInfo.FieldType)
              .Ret();

          return (Func<T, object>)getter.CreateDelegate(typeof(Func<T, object>));
        }
        else
        {
          var instance = Expression.Parameter(typeof(T), "i");
          var fieldDeclaringType = fieldInfo.GetDeclaringType();
          var field = typeof(T) != fieldDeclaringType
              ? Expression.Field(Expression.TypeAs(instance, fieldDeclaringType), fieldInfo)
              : Expression.Field(instance, fieldInfo);
          var convertField = Expression.TypeAs(field, typeof(object));
          return Expression.Lambda<Func<T, object>>(convertField, instance).Compile();
        }
#endif
      }

      #endregion

      #region GetValueSetter for FieldInfo

      private static readonly DictionaryCache<FieldInfo, Action<T, object>> s_fieldsValueSetterCache =
          new DictionaryCache<FieldInfo, Action<T, object>>();
      private static readonly Func<FieldInfo, Action<T, object>> s_fieldInfoGetValueSetterFunc = GetValueSetterInternal;

      public static Action<T, object> GetValueSetter(FieldInfo fieldInfo) =>
          s_fieldsValueSetterCache.GetItem(fieldInfo, s_fieldInfoGetValueSetterFunc);

      private static Action<T, object> GetValueSetterInternal(FieldInfo fieldInfo)
      {
        if (fieldInfo.IsStatic)
        {
          //定义一个没有名字的动态方法
          var setter = CreateDynamicSetMethod<T>(fieldInfo);
          var il = setter.GetILGenerator();

          // 必须考虑对象是值类型的情况，需要拆箱
          // 其它地方看到的程序从来都没有人处理
          // 值类型是不支持这样子赋值的，暂时没有找到更好的替代方法
          il.Ldarg(0)
              .CastFromObject(fieldInfo.DeclaringType)
              .Ldarg(1);
          var method = GetFieldMethod(fieldInfo.FieldType);
          if (method != null)
          {
            il.Call(method);
          }
          else
          {
            il.CastFromObject(fieldInfo.FieldType);
          }
          il.Emit(OpCodes.Stfld, fieldInfo);
          il.Emit(OpCodes.Ret);
          return (Action<T, object>)setter.CreateDelegate(typeof(Action<T, object>));
        }
        else
        {
          var instance = Expression.Parameter(typeof(T), "i");
          var argument = Expression.Parameter(typeof(object), "a");

          var fieldDeclaringType = fieldInfo.GetDeclaringType();
          var field = typeof(T) != fieldDeclaringType
              ? Expression.Field(Expression.TypeAs(instance, fieldDeclaringType), fieldInfo)
              : Expression.Field(instance, fieldInfo);

          var setterCall = Expression.Assign(
              field,
              GetCastOrConvertExpression(argument, fieldInfo.FieldType));

          return Expression.Lambda<Action<T, object>>(setterCall, instance, argument).Compile();
        }
      }

      #endregion
    }

    #endregion

    #region * CreateDynamicGetMethod *

    private static DynamicMethod CreateDynamicGetMethod<T>(MemberInfo memberInfo)
    {
      var args = new[] { typeof(object) };
      var name = $"_GET{memberInfo.Name}_";
      var returnType = typeof(T);

      return !memberInfo.DeclaringType.IsInterface
                 ? new DynamicMethod(name, returnType, args, memberInfo.DeclaringType, true)
                 : new DynamicMethod(name, returnType, args, memberInfo.Module, true);
    }

    #endregion

    #region * CreateDynamicSetMethod *

    private static DynamicMethod CreateDynamicSetMethod<T>(MemberInfo memberInfo)
    {
      var args = new[] { typeof(T), typeof(object) };
      var name = $"_SET{memberInfo.Name}_";
      var returnType = typeof(void);

      return !memberInfo.DeclaringType.IsInterface
                 ? new DynamicMethod(name, returnType, args, memberInfo.DeclaringType, true)
                 : new DynamicMethod(name, returnType, args, memberInfo.Module, true);
    }

    #endregion

    #region * GetCastOrConvertExpression *

    private static Expression GetCastOrConvertExpression(Expression expression, Type targetType)
    {
      Expression result;
      var expressionType = expression.Type;

      if (targetType.IsAssignableFrom(expressionType))
      {
        result = expression;
      }
      else
      {
        // Check if we can use the as operator for casting or if we must use the convert method
        if (targetType.IsValueType && !targetType.IsNullableType())
        {
          result = Expression.Convert(expression, targetType);
        }
        else
        {
          result = Expression.TypeAs(expression, targetType);
        }
      }

      return result;
    }

    #endregion

    #region = SetField =

    private static readonly MethodInfo s_setFieldMethod = typeof(Reflect).GetStaticMethod("SetField");

    internal static void SetField<TValue>(ref TValue field, TValue newValue)
    {
      field = newValue;
    }

    private static readonly MethodInfo s_getPropertyMethod = typeof(Reflect).GetStaticMethod("TypedGetPropertyFn");
    /// <summary>Func to get the Strongly-typed field</summary>
    internal static Func<TEntity, TValue> TypedGetPropertyFn<TEntity, TValue>(PropertyInfo pi)
    {
      var mi = pi.GetMethodInfo();
      return (Func<TEntity, TValue>)mi.CreateDelegate(typeof(Func<TEntity, TValue>));
    }

    private static readonly MethodInfo s_setPropertyMethod = typeof(Reflect).GetStaticMethod("TypedSetPropertyFn");
    /// <summary>Func to set the Strongly-typed field</summary>
    internal static Action<TEntity, TValue> TypedSetPropertyFn<TEntity, TValue>(PropertyInfo pi)
    {
      var mi = pi.SetMethodInfo();
      return (Action<TEntity, TValue>)mi.CreateDelegate(typeof(Action<TEntity, TValue>));
    }

    #endregion

    #endregion

    #region -- 类型辅助 --

    #region - MemberInfo -

    /// <summary>获取成员绑定的显示名，优先DisplayName，然后Description</summary>
    /// <param name="member"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static String GetDisplayName(this MemberInfo member, Boolean inherit = true)
    {
      var att = member.GetCustomAttributeX<DisplayNameAttribute>(inherit);
      if (att != null && !att.DisplayName.IsNullOrWhiteSpace()) { return att.DisplayName; }

      return null;
    }

    /// <summary>获取成员绑定的显示名，优先DisplayName，然后Description</summary>
    /// <param name="member"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static String GetDescription(this MemberInfo member, Boolean inherit = true)
    {
      var att2 = member.GetCustomAttributeX<DescriptionAttribute>(inherit);
      if (att2 != null && !att2.Description.IsNullOrWhiteSpace()) { return att2.Description; }

      return null;
    }

    #endregion

    #region - Type / TypeInfo -

    /// <summary>获取一个类型的元素类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Type GetElementTypeEx(this Type type)
    {
      return Provider.GetElementType(type);
    }

    #endregion

    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Object ChangeType(this Object value, Type conversionType)
    {
      return Provider.ChangeType(value, conversionType);
    }

    /// <summary>类型转换</summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="value">数值</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TResult ChangeType<TResult>(this Object value)
    {
      if (value is TResult) { return (TResult)value; }

      return (TResult)ChangeType(value, typeof(TResult));
    }

    /// <summary>获取类型的友好名称</summary>
    /// <param name="type">指定类型</param>
    /// <param name="isfull">是否全名，包含命名空间</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static String GetName(this Type type, Boolean isfull = false)
    {
      return Provider.GetName(type, isfull);
    }

    /// <summary>从参数数组中获取类型数组</summary>
    /// <param name="args"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Type[] GetTypeArray(this Object[] args)
    {
      if (args == null) { return Type.EmptyTypes; }

      var typeArray = new Type[args.Length];
      for (int i = 0; i < typeArray.Length; i++)
      {
        if (args[i] == null)
        {
          typeArray[i] = typeof(Object);
        }
        else
        {
          typeArray[i] = args[i].GetType();
        }
      }
      return typeArray;
    }

    /// <summary>获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身</summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public static Type GetMemberType(this MemberInfo member)
    {
      switch (member.MemberType)
      {
        case MemberTypes.Constructor:
          return (member as ConstructorInfo).DeclaringType;
        case MemberTypes.Field:
          return (member as FieldInfo).FieldType;
        case MemberTypes.Method:
          return (member as MethodInfo).ReturnType;
        case MemberTypes.Property:
          return (member as PropertyInfo).PropertyType;
        case MemberTypes.TypeInfo:
        case MemberTypes.NestedType:
          return member as Type;
        default:
          return null;
      }
    }

    /// <summary>EmptyTypes</summary>
#if NETSTANDARD
    public static readonly Type[] EmptyTypes = new Type[0];
#else
    public static readonly Type[] EmptyTypes = Type.EmptyTypes;
#endif

#if (NETSTANDARD1_1 || NETSTANDARD1_3)
    public static TypeCode GetTypeCode(this Type type)
    {
      if (type == null) return TypeCode.Empty;
      TypeCode result;
      if (s_typeCodeLookup.TryGetValue(type, out result)) return result;

      if (type.IsEnum())
      {
        type = Enum.GetUnderlyingType(type);
        if (s_typeCodeLookup.TryGetValue(type, out result)) return result;
      }
      return TypeCode.Object;
    }

    public static Type GetTypeFromTypeCode(this TypeCode typeCode)
    {
      Type result;

      if (s_typeFromTypeCodeLookup.TryGetValue(typeCode, out result))
        return result;

      return typeof(Object);
    }
    private static readonly Dictionary<Type, TypeCode> s_typeCodeLookup = new Dictionary<Type, TypeCode>
        {
            {typeof(bool), TypeCode.Boolean },
            {typeof(byte), TypeCode.Byte },
            {typeof(char), TypeCode.Char},
            {typeof(DateTime), TypeCode.DateTime},
            {typeof(decimal), TypeCode.Decimal},
            {typeof(double), TypeCode.Double },
            {typeof(short), TypeCode.Int16 },
            {typeof(int), TypeCode.Int32 },
            {typeof(long), TypeCode.Int64 },
            {typeof(object), TypeCode.Object},
            {typeof(sbyte), TypeCode.SByte },
            {typeof(float), TypeCode.Single },
            {typeof(string), TypeCode.String },
            {typeof(ushort), TypeCode.UInt16 },
            {typeof(uint), TypeCode.UInt32 },
            {typeof(ulong), TypeCode.UInt64 },
        };

    private static readonly Dictionary<TypeCode, Type> s_typeFromTypeCodeLookup = new Dictionary<TypeCode, Type>
        {
            {TypeCode.Boolean, typeof(bool) },
            {TypeCode.Byte , typeof(byte) },
            {TypeCode.Char, typeof(char) },
            {TypeCode.DateTime, typeof(DateTime) },
            {TypeCode.Decimal, typeof(decimal) },
            {TypeCode.Double , typeof(double) },
            {TypeCode.Int16, typeof(short) },
            {TypeCode.Int32, typeof(int) },
            {TypeCode.Int64, typeof(long) },
            {TypeCode.Object, typeof(object) },
            {TypeCode.SByte, typeof(sbyte) },
            {TypeCode.Single, typeof(float) },
            {TypeCode.String, typeof(string) },
            {TypeCode.UInt16, typeof(ushort) },
            {TypeCode.UInt32, typeof(uint) },
            {TypeCode.UInt64, typeof(ulong) },
        };

#else
    /// <summary>获取类型代码</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TypeCode GetTypeCode(this Type type) => Type.GetTypeCode(type);
#endif


    #endregion

    #region -- 插件 --

    /// <summary>是否子类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Boolean IsSubOf(this Type type, Type baseType)
    {
      return Provider.IsSubOf(type, baseType);
    }

    /// <summary>是否子类</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Boolean IsSubOf<T>(this Type type)
    {
      return Provider.IsSubOf(type, typeof(T));
    }

    /// <summary>在指定程序集中查找指定基类的子类</summary>
    /// <param name="asm">指定程序集</param>
    /// <param name="baseType">基类或接口</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<Type> GetSubclasses(this Assembly asm, Type baseType)
    {
      return Provider.GetSubclasses(asm, baseType);
    }

    /// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
    /// <param name="baseType">基类或接口</param>
    /// <param name="isLoadAssembly">是否加载为加载程序集</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<Type> GetAllSubclasses(this Type baseType, Boolean isLoadAssembly = false)
    {
      return Provider.GetAllSubclasses(baseType, isLoadAssembly);
    }

    #endregion

    #region -- 辅助方法 --

    /// <summary>获取类型，如果target是Type类型，则表示要反射的是静态成员</summary>
    /// <param name="target">目标对象</param>
    /// <returns></returns>
    static Type GetTypeInternal(ref Object target)
    {
      if (target == null) { throw new ArgumentNullException("target"); }

      var type = target as Type;
      if (type == null)
      {
        type = target.GetType();
      }
      else
      {
        target = null;
      }

      return type;
    }

    /// <summary>判断某个类型是否可空类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    static Boolean IsNullable(Type type)
    {
      //if (type.IsValueType) return false;

      if (type.IsGenericType && !type.IsGenericTypeDefinition &&
          Object.ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>))) { return true; }

      return false;
    }

    /// <summary>把一个方法转为泛型委托，便于快速反射调用</summary>
    /// <typeparam name="TFunc"></typeparam>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static TFunc As<TFunc>(this MethodInfo method, Object target = null)
    {
      if (target == null)
      {
#if !NET40
        return (TFunc)(Object)method.CreateDelegate(typeof(TFunc));
#else
        return (TFunc)(Object)Delegate.CreateDelegate(typeof(TFunc), method);
#endif
      }
      else
      {
#if !NET40
        return (TFunc)(Object)method.CreateDelegate(typeof(TFunc), target);
#else
        return (TFunc)(Object)Delegate.CreateDelegate(typeof(TFunc), target, method);
#endif
      }
    }

    #endregion
  }
}