/*
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
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml.Serialization;
using CuteAnt.Collections;
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

  /// <summary>反射工具类</summary>
  public static class Reflect
  {
    #region -- 属性 --

    /// <summary>当前反射提供者</summary>
    public static IReflect Provider { get; set; }

    static Reflect()
    {
      Provider = new EmitReflect();
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

    #endregion

    #region * enum ReflectMembersTokenType *

    private enum ReflectMembersTokenType
    {
      InstanceDeclaredMembers,
      InstancePublicMembers,
      InstanceMembers,
      TypeDeclaredMembers,
      TypePublicMembers,
      TypeFlattenHierarchyPublicMembers,
      TypeMembers,
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
    [Obsolete("=> GetTypeDeclaredFields")]
    public static IEnumerable<FieldInfo> GetDeclaredFieldsEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().DeclaredFields;
#else
      return type.GetFields(BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    #region * Fields Cache *

    private static readonly DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, FieldInfo[]>> s_fieldsCache =
        new DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, FieldInfo[]>>();
    private static readonly DictionaryCache<Type, Dictionary<string, FieldInfo>> s_allFieldsCache =
        new DictionaryCache<Type, Dictionary<string, FieldInfo>>();

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
        case ReflectMembersTokenType.InstanceDeclaredMembers:
          bindingFlags = BindingFlagsHelper.InstanceDeclaredOnlyLookup;
          break;
        case ReflectMembersTokenType.InstancePublicMembers:
          bindingFlags = BindingFlagsHelper.InstancePublicOnlyLookup;
          break;
        case ReflectMembersTokenType.InstanceMembers:
          bindingFlags = BindingFlagsHelper.InstanceLookup;
          break;
        case ReflectMembersTokenType.TypeDeclaredMembers:
          bindingFlags = BindingFlagsHelper.DefaultDeclaredOnlyLookup;
          break;
        case ReflectMembersTokenType.TypePublicMembers:
          bindingFlags = BindingFlagsHelper.DefaultPublicOnlyLookup;
          break;
        case ReflectMembersTokenType.TypeFlattenHierarchyPublicMembers:
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

      return type.GetFields(bindingFlags);
    }

    private static FieldInfo[] GetFieldsInternal(Type type, ReflectMembersTokenType reflectFieldsToken)
    {
      if (type == null) { return EmptyArray<FieldInfo>.Instance; }

      // 二级字典缓存
      var cache = s_fieldsCache.GetItem(type, k => new DictionaryCache<ReflectMembersTokenType, FieldInfo[]>());
      return cache.GetItem(reflectFieldsToken, type, s_getTypeFieldsFunc);
    }

    #endregion

    /// <summary>GetInstanceDeclaredFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstanceDeclaredFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceDeclaredMembers);

    /// <summary>GetInstancePublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstancePublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstancePublicMembers);

    /// <summary>GetInstanceFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetInstanceFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.InstanceMembers);

    /// <summary>GetTypeDeclaredFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeDeclaredFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeDeclaredMembers);

    /// <summary>GetTypePublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypePublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypePublicMembers);

    /// <summary>GetTypeFlattenHierarchyPublicFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeFlattenHierarchyPublicFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyPublicMembers);

    /// <summary>GetTypeFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeMembers);

    /// <summary>GetTypeFlattenHierarchyFields</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetTypeFlattenHierarchyFields(this Type type) =>
      GetFieldsInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyMembers);

    /// <summary>获取字段。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo GetFieldEx(this Type type, String name)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        FieldInfo field;

        var fields = s_allFieldsCache.GetItem(type, s_getAllFieldsWithFunc);
        if (fields.TryGetValue(name, out field)) { return field; };

        type = type.BaseType();
      }

      return null;
    }

    private static readonly Func<Type, Dictionary<string, FieldInfo>> s_getAllFieldsWithFunc = GetAllFields;
    private static Dictionary<string, FieldInfo> GetAllFields(Type type) =>
      GetTypeFlattenHierarchyFields(type).ToDictionary(_ => _.Name, StringComparer.Ordinal);

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
    [Obsolete("=> GetTypeDeclaredProperties")]
    public static IEnumerable<PropertyInfo> GetDeclaredPropertiesEx(this Type type)
    {
#if !NET40
      return type.GetTypeInfo().DeclaredProperties;
#else
      return type.GetProperties(BindingFlagsHelper.MSDeclaredOnlyLookup);
#endif
    }

    #region * PropertiesCache *

    private static readonly DictionaryCache<Type, PropertyInfo[]> s_ignoreIndexedPublicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();
    private static readonly DictionaryCache<Type, PropertyInfo[]> s_ignoreNonSerializedPublicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();
    private static readonly DictionaryCache<Type, PropertyInfo[]> s_ignorePublicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();
    private static readonly DictionaryCache<Type, PropertyInfo[]> s_publicPropertiesCache =
        new DictionaryCache<Type, PropertyInfo[]>();

    private static readonly DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>> s_propertiesCache =
        new DictionaryCache<Type, DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>>();
    private static readonly DictionaryCache<Type, Dictionary<string, PropertyInfo>> s_allPropertiesCache =
        new DictionaryCache<Type, Dictionary<string, PropertyInfo>>();

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

          var typeProperties = subType.GetProperties(BindingFlagsHelper.InstancePublicOnlyLookup);

          var newPropertyInfos = typeProperties
              .Where(x => !propertyInfos.Contains(x));

          propertyInfos.InsertRange(0, newPropertyInfos);
        }

        return propertyInfos.ToArray();
      }

      // Void*的基类就是null
      if (type == typeof(object) || type.BaseType == null) return EmptyArray<PropertyInfo>.Instance;

      var list = new List<PropertyInfo>();
      var pis = type.GetProperties(BindingFlagsHelper.InstancePublicOnlyLookup);
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

    private static readonly Func<ReflectMembersTokenType, Type, PropertyInfo[]> s_getTypePropertiesFunc = GetTypeProperties;

    private static PropertyInfo[] GetTypeProperties(ReflectMembersTokenType reflectPropertiesToken, Type type)
    {
      BindingFlags bindingFlags;
      switch (reflectPropertiesToken)
      {
        case ReflectMembersTokenType.InstanceDeclaredMembers:
          bindingFlags = BindingFlagsHelper.InstanceDeclaredOnlyLookup;
          break;
        case ReflectMembersTokenType.InstancePublicMembers:
          bindingFlags = BindingFlagsHelper.InstancePublicOnlyLookup;
          break;
        case ReflectMembersTokenType.InstanceMembers:
          bindingFlags = BindingFlagsHelper.InstanceLookup;
          break;
        case ReflectMembersTokenType.TypeDeclaredMembers:
          bindingFlags = BindingFlagsHelper.DefaultDeclaredOnlyLookup;
          break;
        case ReflectMembersTokenType.TypePublicMembers:
          bindingFlags = BindingFlagsHelper.DefaultPublicOnlyLookup;
          break;
        case ReflectMembersTokenType.TypeFlattenHierarchyPublicMembers:
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

          var typeProperties = subType.GetProperties(bindingFlags);

          var newPropertyInfos = typeProperties
              .Where(x => !propertyInfos.Contains(x));

          propertyInfos.InsertRange(0, newPropertyInfos);
        }

        return propertyInfos.ToArray();
      }

      // Void*的基类就是null
      if (type == typeof(object) || type.BaseType == null) return EmptyArray<PropertyInfo>.Instance;

      return type.GetProperties(bindingFlags)
                 .Where(t => t.GetIndexParameters().Length == 0) // ignore indexed properties;
                 .ToArray();
    }

    private static PropertyInfo[] GetPropertiesInternal(Type type, ReflectMembersTokenType reflectPropertiesToken)
    {
      if (type == null) { return EmptyArray<PropertyInfo>.Instance; }

      // 二级字典缓存
      var cache = s_propertiesCache.GetItem(type, k => new DictionaryCache<ReflectMembersTokenType, PropertyInfo[]>());
      return cache.GetItem(reflectPropertiesToken, type, s_getTypePropertiesFunc);
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

    /// <summary>GetInstanceDeclaredProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetInstanceDeclaredProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.InstanceDeclaredMembers);

    /// <summary>GetInstanceProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetInstanceProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.InstanceMembers);

    /// <summary>GetTypeDeclaredProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeDeclaredProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.TypeDeclaredMembers);

    /// <summary>GetTypePublicProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypePublicProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.TypePublicMembers);

    /// <summary>GetTypeFlattenHierarchyPublicProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeFlattenHierarchyPublicProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyPublicMembers);

    /// <summary>GetTypeProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.TypeMembers);

    /// <summary>GetTypeFlattenHierarchyProperties</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetTypeFlattenHierarchyProperties(this Type type) =>
        GetPropertiesInternal(type, ReflectMembersTokenType.TypeFlattenHierarchyMembers);

    #region - GetPropertyEx -

    /// <summary>获取属性。搜索私有、静态、基类</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyEx(this Type type, string name)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        PropertyInfo property;

        var properties = s_allPropertiesCache.GetItem(type, s_getAllPropertiesFunc);
        if (properties.TryGetValue(name, out property)) { return property; };

        type = type.BaseType();
      }

      return null;
    }

    private static readonly Func<Type, Dictionary<string, PropertyInfo>> s_getAllPropertiesFunc = GetAllProperties;
    private static Dictionary<string, PropertyInfo> GetAllProperties(Type type) =>
      GetTypeFlattenHierarchyProperties(type).ToDictionary(_ => _.Name, StringComparer.Ordinal);

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
        var fs = type.GetMember(name, BindingFlagsHelper.DefaultLookup);
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

    /// <summary>反射创建指定类型的实例</summary>
    /// <param name="type">类型</param>
    /// <param name="parameters">参数数组</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerHidden]
    public static Object CreateInstance(this Type type, params Object[] parameters)
    {
      ValidationHelper.ArgumentNull(type, "type");

      return Provider.CreateInstance(type, parameters);
    }

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

      var type = GetType(ref target);
      throw new HmExceptionBase("类{0}中找不到名为{1}的方法！", type, name);
    }

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

      var type = GetType(ref target);

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

    #region - GetValue -

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="throwOnError">出错时是否抛出异常</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object GetValue(this Object target, String name, Boolean throwOnError = true)
    {
      ValidationHelper.ArgumentNull(target, "target");
      ValidationHelper.ArgumentNullOrEmpty(name, "name");

      Object value = null;
      if (TryGetValue(target, name, out value)) { return value; }

      if (!throwOnError) { return null; }

      var type = GetType(ref target);
      throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
    }

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns>是否成功获取数值</returns>
    public static Boolean TryGetValue(this Object target, String name, out Object value)
    {
      value = null;

      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetType(ref target);
      var pi = GetPropertyEx(type, name);
      if (pi != null)
      {
        value = target.GetValue(pi);
        return true;
      }

      var fi = GetFieldEx(type, name);
      if (fi != null)
      {
        value = target.GetValue(fi);
        return true;
      }

      return false;
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <returns></returns>
    public static Object GetValue(this Object target, PropertyInfo property)
    {
      return Provider.GetValue(target, property);
    }

    /// <summary>获取目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <returns></returns>
    public static Object GetValue(this Object target, FieldInfo field)
    {
      return Provider.GetValue(target, field);
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <returns></returns>
    public static Object GetValue(this Object target, MemberInfo member)
    {
      var property = member as PropertyInfo;
      if (property != null) { return target.GetValue(property); }
      var field = member as FieldInfo;
      if (field != null) { return target.GetValue(field); }

      throw new ArgumentOutOfRangeException("member");
    }

    #endregion

    #region - SetValue -

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    [Obsolete("=> TrySetValue")]
    public static Boolean SetValue(this Object target, String name, Object value) => TrySetValue(target, name, value);

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static Boolean TrySetValue(this Object target, String name, Object value)
    {
      if (name.IsNullOrWhiteSpace()) { return false; }

      var type = GetType(ref target);
      var pi = GetPropertyEx(type, name);
      if (pi != null) { target.SetValue(pi, value); return true; }

      var fi = GetFieldEx(type, name);
      if (fi != null) { target.SetValue(fi, value); return true; }

      //throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
      return false;
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static void SetValue(this Object target, PropertyInfo property, Object value)
    {
      Provider.SetValue(target, property, value);
    }

    /// <summary>设置目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    public static void SetValue(this Object target, FieldInfo field, Object value)
    {
      Provider.SetValue(target, field, value);
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static void SetValue(this Object target, MemberInfo member, Object value)
    {
      var property = member as PropertyInfo;
      if (property != null) { Provider.SetValue(target, property, value); return; }
      var field = member as FieldInfo;
      if (field != null) { Provider.SetValue(target, field, value); return; }

      throw new ArgumentOutOfRangeException("member");
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

    /// <summary>获取类型代码</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TypeCode GetTypeCode(this Type type)
    {
      return Type.GetTypeCode(type);
    }

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
    static Type GetType(ref Object target)
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