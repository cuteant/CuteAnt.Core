using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

  #region -- MemberGetter / MemberSetter --

  /// <summary>GetMemberFunc</summary>
  /// <param name="instance"></param>
  /// <returns></returns>
  public delegate object MemberGetter(object instance);
  /// <summary>GetMemberFunc</summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="instance"></param>
  /// <returns></returns>
  public delegate object MemberGetter<T>(T instance);

  /// <summary>SetMemberAction</summary>
  /// <param name="instance"></param>
  /// <param name="value"></param>
  public delegate void MemberSetter(object instance, object value);
  /// <summary>SetMemberAction</summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="instance"></param>
  /// <param name="value"></param>
  public delegate void MemberSetter<T>(T instance, object value);

  /// <summary>SetMemberRefAction</summary>
  /// <param name="instance"></param>
  /// <param name="propertyValue"></param>
  public delegate void MemberRefSetter(ref object instance, object propertyValue);
  /// <summary>SetMemberRefAction</summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="instance"></param>
  /// <param name="value"></param>
  public delegate void MemberRefSetter<T>(ref T instance, object value);

  #endregion

  #region -- MethodCaller / CtorInvoker --

  public delegate TReturn MethodCaller<TTarget, TReturn>(TTarget target, object[] args);
  public delegate T CtorInvoker<T>(object[] parameters);

  #endregion

  /// <summary>反射工具类</summary>
  public static class Reflect
  {
    #region -- 属性 --

    public static bool SupportsExpression { get; set; } = true;

    public static bool SupportsEmit { get; set; } = true;

    #endregion

    #region -- 反射获取 --

    #region - Type -

    private static Dictionary<Type, object> s_defaultValueTypes = new Dictionary<Type, object>();
    /// <summary>GetDefaultValue</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetDefaultValue(this Type type)
    {
      if (type == null) { throw new ArgumentNullException(nameof(type)); }

      if (!type.IsValueType) return null;

      if (s_defaultValueTypes.TryGetValue(type, out object defaultValue)) return defaultValue;

      defaultValue = Activator.CreateInstance(type);

      Dictionary<Type, object> snapshot, newCache;
      do
      {
        snapshot = s_defaultValueTypes;
        newCache = new Dictionary<Type, object>(s_defaultValueTypes)
        {
          [type] = defaultValue
        };
      } while (!ReferenceEquals(Interlocked.CompareExchange(ref s_defaultValueTypes, newCache, snapshot), snapshot));

      return defaultValue;
    }

    private static Dictionary<string, Type> s_genericTypeCache = new Dictionary<string, Type>(StringComparer.Ordinal);
    /// <summary>GetCachedGenericType</summary>
    /// <param name="type"></param>
    /// <param name="argTypes"></param>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <returns></returns>
    public static Type GetCachedGenericType(this Type type, params Type[] argTypes)
    {
      if (type == null) { throw new ArgumentNullException(nameof(type)); }

      if (!type.IsGenericTypeDefinition)
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

      if (s_genericTypeCache.TryGetValue(key, out Type genericType)) { return genericType; }

      genericType = type.MakeGenericType(argTypes);

      Dictionary<string, Type> snapshot, newCache;
      do
      {
        snapshot = s_genericTypeCache;
        newCache = new Dictionary<string, Type>(s_genericTypeCache)
        {
          [key] = genericType
        };
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
      while (type != null && type != TypeConstants.Object)
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
      while (type != null && type != TypeConstants.Object)
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
    public static PropertyInfo GetTypeProperty(this Type type, string name, Type returnType, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (returnType == null) throw new ArgumentNullException(nameof(returnType));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.Object)
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
    public static PropertyInfo GetTypeProperty(this Type type, string name, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.Object)
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
    public static PropertyInfo GetTypeProperty(this Type type, string name, Type returnType, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.Object)
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
      while (type != null && type != TypeConstants.Object)
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
    public static PropertyInfo GetInstanceProperty(this Type type, string name, Type returnType, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (returnType == null) throw new ArgumentNullException(nameof(returnType));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.Object)
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
    public static PropertyInfo GetInstanceProperty(this Type type, string name, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.Object)
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
    public static PropertyInfo GetInstanceProperty(this Type type, string name, Type returnType, Type[] parameterTypes, bool declaredOnly = false)
    {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeConstants.Object)
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
      while (type != null && type != TypeConstants.Object)
      {
        var fs = type.GetMember(name, BindingFlagsHelper.DefaultDeclaredOnlyLookup);
        if (fs != null && fs.Length > 0) { return fs[0]; }

#if NET40
        type = type.BaseType;
#else
        type = type.GetTypeInfo().BaseType;
#endif
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
    public static MethodInfo GetMethodEx(this Type type, String name, params Type[] paramTypes)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      // 参数数组必须为空，或者所有参数类型都不为null，才能精确匹配
      if (paramTypes == null || paramTypes.Length == 0 || paramTypes.All(t => t != null))
      {
        MethodInfo method = null;
        while (true)
        {
          if (paramTypes == null || paramTypes.Length == 0)
          {
            method = type.GetMethod(name, BindingFlagsHelper.MSRuntimeLookup);
          }
          else
          {
            method = type.GetMethod(name, BindingFlagsHelper.MSRuntimeLookup, null, paramTypes, null);
          }

          if (method != null) return method;

#if NET40
          type = type.BaseType;
#else
          type = type.GetTypeInfo().BaseType;
#endif
          if (type == null || type == TypeConstants.Object) break;
        }
        if (method != null) return method;
      }

      // 任意参数类型为null，换一种匹配方式
      //if (paramTypes.Any(t => t == null))
      //{
      var ms = GetMethodsEx(type, name, paramTypes.Length);
      if (ms == null || ms.Length == 0) return null;

      // 对比参数
      foreach (var mi in ms)
      {
        var ps = mi.GetParameters();
        var flag = true;
        for (int i = 0; i < ps.Length; i++)
        {
          if (paramTypes[i] != null && !ps[i].ParameterType.IsAssignableFrom(paramTypes[i]))
          {
            flag = false;
            break;
          }
        }
        if (flag) return mi;
      }
      //}
      return null;
    }

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    public static MethodInfo[] GetMethodsEx(this Type type, String name, Int32 paramCount = -1)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      var ms = type.GetMethods(BindingFlagsHelper.MSRuntimeLookup);
      if (ms == null || ms.Length == 0) return ms;

      var list = new List<MethodInfo>();
      foreach (var item in ms)
      {
        if (string.Equals(item.Name, name, StringComparison.Ordinal))
        {
          if (paramCount >= 0 && item.GetParameters().Length == paramCount) list.Add(item);
        }
      }
      return list.ToArray();
    }

    #endregion

    #endregion

    #region -- 反射调用 --

    #region - DelegateForCtor / DelegateForCall -

    private static readonly ILEmitter emit = new ILEmitter();

    private const string kCtorInvokerName = "CI<>";
    private const string kMethodCallerName = "MC<>";

    private static readonly DictionaryCache<Type, DictionaryCache<int, Delegate>> s_ctorInvokerCache =
        new DictionaryCache<Type, DictionaryCache<int, Delegate>>(DictionaryCacheConstants.SIZE_SMALL);

    /// <summary>Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
    public static CtorInvoker<T> DelegateForCtor<T>(this Type type, params Type[] paramTypes)
    {
      int key = kCtorInvokerName.GetHashCode() ^ type.GetHashCode();
      for (int i = 0; i < paramTypes.Length; i++)
      {
        key ^= paramTypes[i].GetHashCode();
      }

      var cache = s_ctorInvokerCache.GetItem(type, k => new DictionaryCache<int, Delegate>());
      var result = cache.GetItem(key, k =>
      {
        var dynMethod = new DynamicMethod(kCtorInvokerName, typeof(T), new Type[] { typeof(object[]) });

        emit.il = dynMethod.GetILGenerator();
        GenCtor<T>(type, paramTypes);

        return dynMethod.CreateDelegate(typeof(CtorInvoker<T>));
      });
      return (CtorInvoker<T>)result;
    }

    /// <summary>Generates or gets a weakly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
    public static CtorInvoker<object> DelegateForCtor(this Type type, params Type[] ctorParamTypes)
        => DelegateForCtor<object>(type, ctorParamTypes);


    /// <summary>Generates a strongly-typed open-instance delegate to invoke the specified method</summary>
    public static MethodCaller<TTarget, TReturn> DelegateForCall<TTarget, TReturn>(this MethodInfo method)
    {
      int key = GetKey<TTarget, TReturn>(method, kMethodCallerName);

      return GenDelegateForMember<MethodCaller<TTarget, TReturn>, MethodInfo>(
          method, kMethodCallerName, GenMethodInvocation<TTarget>,
          typeof(TReturn), typeof(TTarget), typeof(object[]));
    }

    /// <summary>Generates a weakly-typed open-instance delegate to invoke the specified method.</summary>
    public static MethodCaller<object, object> DelegateForCall(this MethodInfo method)
        => DelegateForCall<object, object>(method);

    /// <summary>Executes the delegate on the specified target and arguments but only if it's not null.</summary>
    public static void SafeInvoke<TTarget, TValue>(this MethodCaller<TTarget, TValue> caller, TTarget target, params object[] args)
    {
      caller?.Invoke(target, args);
    }

    static int GetKey<T, R>(MemberInfo member, string dynMethodName)
    {
      return member.GetHashCode() ^ dynMethodName.GetHashCode() ^ typeof(T).GetHashCode() ^ typeof(R).GetHashCode();
    }

    static TDelegate GenDelegateForMember<TDelegate, TMember>(TMember member, string dynMethodName,
      Action<TMember> generator, Type returnType, params Type[] paramTypes)
      where TMember : MemberInfo where TDelegate : class
    {
      var dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);

      emit.il = dynMethod.GetILGenerator();
      generator(member);

      var result = dynMethod.CreateDelegate(typeof(TDelegate));
      return (TDelegate)(object)result;
    }

    static void GenCtor<T>(Type type, Type[] paramTypes)
    {
      // arg0: object[] arguments
      // goal: return new T(arguments)
      Type targetType = typeof(T) == typeof(object) ? type : typeof(T);

      if (targetType.IsValueType && paramTypes.Length == 0)
      {
        var tmp = emit.declocal(targetType);
        emit.ldloca(tmp)
            .initobj(targetType)
            .ldloc(0);
      }
      else
      {
        var ctor = targetType.GetConstructor(paramTypes);
        if (ctor == null)
          throw new Exception("Generating constructor for type: " + targetType +
              (paramTypes.Length == 0 ? "No empty constructor found!" :
              "No constructor found that matches the following parameter types: " +
              string.Join(",", paramTypes.Select(x => x.Name).ToArray())));

        // push parameters in order to then call ctor
        for (int i = 0, imax = paramTypes.Length; i < imax; i++)
        {
          emit.ldarg0()         // push args array
              .ldc_i4(i)          // push index
              .ldelem_ref()       // push array[index]
              .unbox_any(paramTypes[i]);  // cast
        }

        emit.newobj(ctor);
      }

      if (typeof(T) == typeof(object) && targetType.IsValueType)
        emit.box(targetType);

      emit.ret();
    }

    static void GenMethodInvocation<TTarget>(MethodInfo method)
    {
      var weaklyTyped = typeof(TTarget) == typeof(object);

      // push target if not static (instance-method. in that case first arg is always 'this')
      if (!method.IsStatic)
      {
        var targetType = weaklyTyped ? method.DeclaringType : typeof(TTarget);
        emit.declocal(targetType);
        emit.ldarg0();
        if (weaklyTyped)
          emit.unbox_any(targetType);
        emit.stloc0()
            .ifclass_ldloc_else_ldloca(0, targetType);
      }

      // push arguments in order to call method
      var prams = method.GetParameters();
      for (int i = 0, imax = prams.Length; i < imax; i++)
      {
        emit.ldarg1()   // push array
            .ldc_i4(i)    // push index
            .ldelem_ref();  // pop array, index and push array[index]

        var param = prams[i];
        var dataType = param.ParameterType;

        if (dataType.IsByRef)
          dataType = dataType.GetElementType();

        var tmp = emit.declocal(dataType);
        emit.unbox_any(dataType)
            .stloc(tmp)
            .ifbyref_ldloca_else_ldloc(tmp, param.ParameterType);
      }

      // perform the correct call (pushes the result)
      emit.callorvirt(method);

      // if method wasn't static that means we declared a temp local to load the target
      // that means our local variables index for the arguments start from 1
      int localVarStart = method.IsStatic ? 0 : 1;
      for (int i = 0; i < prams.Length; i++)
      {
        var paramType = prams[i].ParameterType;
        if (paramType.IsByRef)
        {
          var byRefType = paramType.GetElementType();
          emit.ldarg1()
              .ldc_i4(i)
              .ldloc(i + localVarStart);
          if (byRefType.IsValueType)
            emit.box(byRefType);
          emit.stelem_ref();
        }
      }

      if (method.ReturnType == typeof(void))
        emit.ldnull();
      else if (weaklyTyped)
        emit.ifvaluetype_box(method.ReturnType);

      emit.ret();
    }

    #endregion

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
      if (s_constructorMethods.TryGetValue(type, out EmptyCtorDelegate emptyCtorFn)) return emptyCtorFn;

      emptyCtorFn = GetConstructorMethodToCache(type);

      Dictionary<Type, EmptyCtorDelegate> snapshot, newCache;
      do
      {
        snapshot = s_constructorMethods;
        newCache = new Dictionary<Type, EmptyCtorDelegate>(s_constructorMethods)
        {
          [type] = emptyCtorFn
        };
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
      if (s_typeNamesMap.TryGetValue(typeName, out EmptyCtorDelegate emptyCtorFn)) return emptyCtorFn;

      if (!TypeUtils.TryResolveType(typeName, out var type)) { return null; }
      emptyCtorFn = GetConstructorMethodToCache(type);

      Dictionary<string, EmptyCtorDelegate> snapshot, newCache;
      do
      {
        snapshot = s_typeNamesMap;
        newCache = new Dictionary<string, EmptyCtorDelegate>(s_typeNamesMap)
        {
          [typeName] = emptyCtorFn
        };
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
      if (type == TypeUtils._.String)
      {
        return () => string.Empty;
      }
      else if (type.IsInterface)
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
      else if (type.IsGenericTypeDefinition)
      {
        var genericArgs = type.GetGenericArguments();
        var typeArgs = new Type[genericArgs.Length];
        for (var i = 0; i < genericArgs.Length; i++)
          typeArgs[i] = typeof(object);

        var realizedType = type.MakeGenericType(typeArgs);

        return realizedType.CreateInstance;
      }

      var emptyCtor = type.GetEmptyConstructor();
      if (emptyCtor != null)
      {
        var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof(Reflect).Module, true);
        var ilgen = dm.GetILGenerator();
        ilgen.Emit(System.Reflection.Emit.OpCodes.Nop);
        ilgen.Emit(System.Reflection.Emit.OpCodes.Newobj, emptyCtor);
        ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);

        return (EmptyCtorDelegate)dm.CreateDelegate(typeof(EmptyCtorDelegate));
      }

      //Anonymous types don't have empty constructors
      return () => FormatterServices.GetUninitializedObject(type);
      // return FormatterServices.GetSafeUninitializedObject(Type);
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

      if (TryGetMemberInfoValue(target, name, out object value)) { return value; }

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
      if (member is PropertyInfo property) { return GetPropertyInfoValue(target, property); }
      if (member is FieldInfo field) { return GetFieldInfoValue(target, field); }

      throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetMemberInfoValue(this object target, MemberInfo member, out object value)
    {
      if (member is PropertyInfo property)
      {
        return TryGetPropertyInfoValue(target, property, out value);
      }
      if (member is FieldInfo field)
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
      return GetValueGetter(property).Invoke(target);
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetPropertyInfoValue(this object target, PropertyInfo property, out object value)
    {
      var getter = GetValueGetter(property);

      if (null == getter || getter.IsEmpty())
      {
        value = null;
        return false;
      }
      else
      {
        value = getter(target);
        return true;
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
      if (member is PropertyInfo property) { SetPropertyInfoValue(target, property, value); return; }
      if (member is FieldInfo field) { SetFieldInfoValue(target, field, value); return; }

      throw new ArgumentOutOfRangeException(nameof(member));
    }

    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static bool TrySetMemberInfoValue(this object target, MemberInfo member, object value)
    {
      if (member is PropertyInfo property) { SetPropertyInfoValue(target, property, value); return true; }
      if (member is FieldInfo field) { SetFieldInfoValue(target, field, value); return true; }

      return false;
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static void SetPropertyInfoValue(this object target, PropertyInfo property, object value)
    {
      GetValueSetter(property).Invoke(target, value);
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public static bool TrySetPropertyInfoValue(this object target, PropertyInfo property, object value)
    {
      var setter = GetValueSetter(property);
      if (null == setter || setter.IsEmpty()) { return false; }
      setter(target, value);
      return true;
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

    #region -- MemberGetter.IsEmpty --

    public static bool IsEmpty(this MemberGetter getter) => object.ReferenceEquals(TypeAccessorHelper.EmptyMemberGetter, getter);
    public static bool IsEmpty<T>(this MemberGetter<T> getter) => object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberGetter, getter);
    public static bool IsNullOrEmpty(this MemberGetter getter) => null == getter || object.ReferenceEquals(TypeAccessorHelper.EmptyMemberGetter, getter);
    public static bool IsNullOrEmpty<T>(this MemberGetter<T> getter) => null == getter || object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberGetter, getter);

    #endregion

    #region -- MemberSetter.IsEmpty --

    public static bool IsEmpty(this MemberSetter setter) => object.ReferenceEquals(TypeAccessorHelper.EmptyMemberSetter, setter);
    public static bool IsEmpty<T>(this MemberSetter<T> setter) => object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberSetter, setter);
    public static bool IsNullOrEmpty(this MemberSetter setter) => null == setter || object.ReferenceEquals(TypeAccessorHelper.EmptyMemberSetter, setter);
    public static bool IsNullOrEmpty<T>(this MemberSetter<T> setter) => null == setter || object.ReferenceEquals(TypeAccessorHelper<T>.EmptyMemberSetter, setter);

    #endregion

    #region - GetValueGetter for PropertyInfo -

    private static readonly DictionaryCache<PropertyInfo, MemberGetter> s_propertiesValueGetterCache =
        new DictionaryCache<PropertyInfo, MemberGetter>();
    private static readonly Func<PropertyInfo, MemberGetter> s_propertyInfoGetValueGetterFunc = GetValueGetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberGetter GetValueGetter(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return s_propertiesValueGetterCache.GetItem(propertyInfo, s_propertyInfoGetValueGetterFunc);
    }

    private static MemberGetter GetValueGetterInternal(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanRead) { return TypeAccessorHelper.EmptyMemberGetter; }

      var method = propertyInfo.GetGetMethod(true);
      if (method == null) { return TypeAccessorHelper.EmptyMemberGetter; }
      try
      {
        if (method.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? PropertyInvoker.CreateEmitGetter(propertyInfo) : PropertyInvoker.CreateDefaultGetter(propertyInfo);
        }
        else
        {
          return SupportsEmit ? PropertyInvoker.CreateEmitGetter(propertyInfo) :
                 SupportsExpression
                    ? PropertyInvoker.CreateExpressionGetter(propertyInfo)
                    : PropertyInvoker.CreateDefaultGetter(propertyInfo);
        }
      }
      catch
      {
        return PropertyInvoker.CreateDefaultGetter(propertyInfo);
      }
    }

    #endregion

    #region - GetValueGetter<T> for PropertyInfo -

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberGetter<T> GetValueGetter<T>(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return StaticMemberAccessors<T>.GetValueGetter(propertyInfo);
    }

    #endregion

    #region - GetValueSetter for PropertyInfo -

    private static readonly DictionaryCache<PropertyInfo, MemberSetter> s_propertiesValueSetterCache =
        new DictionaryCache<PropertyInfo, MemberSetter>();
    private static readonly Func<PropertyInfo, MemberSetter> s_propertyInfoGetValueSetterFunc = GetValueSetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberSetter GetValueSetter(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return s_propertiesValueSetterCache.GetItem(propertyInfo, s_propertyInfoGetValueSetterFunc);
    }

    /// <summary>GetValueSetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    private static MemberSetter GetValueSetterInternal(PropertyInfo propertyInfo)
    {
      //if (!propertyInfo.CanWrite) { return TypeAccessorHelper.EmptyMemberSetter; }

      var method = propertyInfo.GetSetMethod(true);
      if (method == null) { return TypeAccessorHelper.EmptyMemberSetter; }
      try
      {
        if (method.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? PropertyInvoker.CreateEmitSetter(propertyInfo) : PropertyInvoker.CreateDefaultSetter(propertyInfo);
        }
        else
        {
          return SupportsEmit ? PropertyInvoker.CreateEmitSetter(propertyInfo) :
                 SupportsExpression
                    ? PropertyInvoker.CreateExpressionSetter(propertyInfo)
                    : PropertyInvoker.CreateDefaultSetter(propertyInfo);
        }
      }
      catch
      {
        return PropertyInvoker.CreateDefaultSetter(propertyInfo);
      }
    }

    #endregion

    #region - GetValueSetter<T> for PropertyInfo -

    /// <summary>GetValueGetter</summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static MemberSetter<T> GetValueSetter<T>(this PropertyInfo propertyInfo)
    {
      if (propertyInfo == null) { throw new ArgumentNullException(nameof(propertyInfo)); }

      return StaticMemberAccessors<T>.GetValueSetter(propertyInfo);
    }

    #endregion

    #region - GetValueGetter for FieldInfo -

    private static readonly DictionaryCache<FieldInfo, MemberGetter> s_fieldsValueGetterCache =
        new DictionaryCache<FieldInfo, MemberGetter>();
    private static readonly Func<FieldInfo, MemberGetter> s_fieldInfoGetValueGetterFunc = GetValueGetterInternal;

    /// <summary>GetValueGetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberGetter GetValueGetter(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return s_fieldsValueGetterCache.GetItem(fieldInfo, s_fieldInfoGetValueGetterFunc);
    }

    private static MemberGetter GetValueGetterInternal(FieldInfo fieldInfo)
    {
      try
      {
        if (fieldInfo.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? FieldInvoker.CreateEmitGetter(fieldInfo) : FieldInvoker.CreateDefaultGetter(fieldInfo);
        }
        else
        {
          return SupportsEmit ? FieldInvoker.CreateEmitGetter(fieldInfo) :
                 SupportsExpression
                    ? FieldInvoker.CreateExpressionGetter(fieldInfo)
                    : FieldInvoker.CreateDefaultGetter(fieldInfo);
        }
      }
      catch
      {
        return FieldInvoker.CreateDefaultGetter(fieldInfo);
      }
    }

    #endregion

    #region - GetValueGetter<T> for FieldInfo -

    /// <summary>GetValueGetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberGetter<T> GetValueGetter<T>(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return StaticMemberAccessors<T>.GetValueGetter(fieldInfo);
    }

    #endregion

    #region - GetValueSetter for FieldInfo -

    private static readonly DictionaryCache<FieldInfo, MemberSetter> s_fieldsValueSetterCache =
        new DictionaryCache<FieldInfo, MemberSetter>();
    private static readonly Func<FieldInfo, MemberSetter> s_fieldInfoGetValueSetterFunc = GetValueSetterInternal;

    /// <summary>GetValueSetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberSetter GetValueSetter(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return s_fieldsValueSetterCache.GetItem(fieldInfo, s_fieldInfoGetValueSetterFunc);
    }

    private static MemberSetter GetValueSetterInternal(FieldInfo fieldInfo)
    {
      try
      {
        if (fieldInfo.IsStatic)
        {
          return SupportsEmit || SupportsExpression ? FieldInvoker.CreateEmitSetter(fieldInfo) : FieldInvoker.CreateDefaultSetter(fieldInfo);
        }
        else
        {
          return SupportsEmit ? FieldInvoker.CreateEmitSetter(fieldInfo) :
                 SupportsExpression
                    ? FieldInvoker.CreateExpressionSetter(fieldInfo)
                    : FieldInvoker.CreateDefaultSetter(fieldInfo);
        }
      }
      catch
      {
        return FieldInvoker.CreateDefaultSetter(fieldInfo);
      }
    }

    #endregion

    #region - GetValueSetter<T> for FieldInfo -

    /// <summary>GetValueSetter</summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static MemberSetter<T> GetValueSetter<T>(this FieldInfo fieldInfo)
    {
      if (fieldInfo == null) { throw new ArgumentNullException(nameof(fieldInfo)); }

      return StaticMemberAccessors<T>.GetValueSetter(fieldInfo);
    }

    #endregion

    #region * class StaticMemberAccessors<T> *

    static class StaticMemberAccessors<T>
    {
      #region GetValueGetter for PropertyInfo

      private static readonly DictionaryCache<PropertyInfo, MemberGetter<T>> s_propertiesValueGetterCache =
          new DictionaryCache<PropertyInfo, MemberGetter<T>>();
      private static readonly Func<PropertyInfo, MemberGetter<T>> s_propertyInfoGetValueGetterFunc = GetValueGetterInternal;

      public static MemberGetter<T> GetValueGetter(PropertyInfo propertyInfo) =>
          s_propertiesValueGetterCache.GetItem(propertyInfo, s_propertyInfoGetValueGetterFunc);

      private static MemberGetter<T> GetValueGetterInternal(PropertyInfo propertyInfo)
      {
        //if (!propertyInfo.CanRead) { return TypeAccessorHelper<T>.EmptyMemberGetter; }

        var method = propertyInfo.GetGetMethod(true);
        if (method == null) { return TypeAccessorHelper<T>.EmptyMemberGetter; }
        try
        {
          if (method.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? PropertyInvoker<T>.CreateEmitGetter(propertyInfo) : PropertyInvoker<T>.CreateDefaultGetter(propertyInfo);
          }
          else
          {
            return SupportsEmit ? PropertyInvoker<T>.CreateEmitGetter(propertyInfo) :
                   SupportsExpression
                      ? PropertyInvoker<T>.CreateExpressionGetter(propertyInfo)
                      : PropertyInvoker<T>.CreateDefaultGetter(propertyInfo);
          }
        }
        catch
        {
          return PropertyInvoker<T>.CreateDefaultGetter(propertyInfo);
        }
      }

      #endregion

      #region GetValueSetter for PropertyInfo

      private static readonly DictionaryCache<PropertyInfo, MemberSetter<T>> s_propertiesValueSetterCache =
          new DictionaryCache<PropertyInfo, MemberSetter<T>>();
      private static readonly Func<PropertyInfo, MemberSetter<T>> s_propertyInfoGetValueSetterFunc = GetValueSetterInternal;

      public static MemberSetter<T> GetValueSetter(PropertyInfo propertyInfo) =>
          s_propertiesValueSetterCache.GetItem(propertyInfo, s_propertyInfoGetValueSetterFunc);

      private static MemberSetter<T> GetValueSetterInternal(PropertyInfo propertyInfo)
      {
        //if (!propertyInfo.CanWrite) { return TypeAccessorHelper<T>.EmptyMemberSetter; }

        var method = propertyInfo.GetSetMethod(true);
        if (method == null) { return TypeAccessorHelper<T>.EmptyMemberSetter; }
        try
        {
          if (method.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? PropertyInvoker<T>.CreateEmitSetter(propertyInfo) : PropertyInvoker<T>.CreateDefaultSetter(propertyInfo);
          }
          else
          {
            return SupportsEmit ? PropertyInvoker<T>.CreateEmitSetter(propertyInfo) :
                   SupportsExpression
                      ? PropertyInvoker<T>.CreateExpressionSetter(propertyInfo)
                      : PropertyInvoker<T>.CreateDefaultSetter(propertyInfo);
          }
        }
        catch
        {
          return PropertyInvoker<T>.CreateDefaultSetter(propertyInfo);
        }
      }

      #endregion

      #region GetValueGetter for FieldInfo

      private static readonly DictionaryCache<FieldInfo, MemberGetter<T>> s_fieldsValueGetterCache =
          new DictionaryCache<FieldInfo, MemberGetter<T>>();
      private static readonly Func<FieldInfo, MemberGetter<T>> s_fieldInfoGetValueGetterFunc = GetValueGetterInternal;

      public static MemberGetter<T> GetValueGetter(FieldInfo fieldInfo) =>
          s_fieldsValueGetterCache.GetItem(fieldInfo, s_fieldInfoGetValueGetterFunc);

      private static MemberGetter<T> GetValueGetterInternal(FieldInfo fieldInfo)
      {
        try
        {
          if (fieldInfo.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? FieldInvoker<T>.CreateEmitGetter(fieldInfo) : FieldInvoker<T>.CreateDefaultGetter(fieldInfo);
          }
          else
          {
            return SupportsEmit ? FieldInvoker<T>.CreateEmitGetter(fieldInfo) :
                   SupportsExpression
                      ? FieldInvoker<T>.CreateExpressionGetter(fieldInfo)
                      : FieldInvoker<T>.CreateDefaultGetter(fieldInfo);
          }
        }
        catch
        {
          return FieldInvoker<T>.CreateDefaultGetter(fieldInfo);
        }
      }

      #endregion

      #region GetValueSetter for FieldInfo

      private static readonly DictionaryCache<FieldInfo, MemberSetter<T>> s_fieldsValueSetterCache =
          new DictionaryCache<FieldInfo, MemberSetter<T>>();
      private static readonly Func<FieldInfo, MemberSetter<T>> s_fieldInfoGetValueSetterFunc = GetValueSetterInternal;

      public static MemberSetter<T> GetValueSetter(FieldInfo fieldInfo) =>
          s_fieldsValueSetterCache.GetItem(fieldInfo, s_fieldInfoGetValueSetterFunc);

      private static MemberSetter<T> GetValueSetterInternal(FieldInfo fieldInfo)
      {
        try
        {
          if (fieldInfo.IsStatic)
          {
            return SupportsEmit || SupportsExpression ? FieldInvoker<T>.CreateEmitSetter(fieldInfo) : FieldInvoker<T>.CreateDefaultSetter(fieldInfo);
          }
          else
          {
            return SupportsEmit ? FieldInvoker<T>.CreateEmitSetter(fieldInfo) :
                   SupportsExpression
                      ? FieldInvoker<T>.CreateExpressionSetter(fieldInfo)
                      : FieldInvoker<T>.CreateDefaultSetter(fieldInfo);
          }
        }
        catch
        {
          return FieldInvoker<T>.CreateDefaultSetter(fieldInfo);
        }
      }

      #endregion
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

    //#region - Type / TypeInfo -

    //    /// <summary>获取一个类型的元素类型</summary>
    //    /// <param name="type">类型</param>
    //    /// <returns></returns>
    //#if !NET40
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //#endif
    //    public static Type GetElementTypeEx(this Type type)
    //    {
    //      return Provider.GetElementType(type);
    //    }

    //    #endregion

    //    /// <summary>类型转换</summary>
    //    /// <param name="value">数值</param>
    //    /// <param name="conversionType"></param>
    //    /// <returns></returns>
    //#if !NET40
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //#endif
    //    public static Object ChangeType(this Object value, Type conversionType)
    //    {
    //      return Provider.ChangeType(value, conversionType);
    //    }

    //    /// <summary>类型转换</summary>
    //    /// <typeparam name="TResult"></typeparam>
    //    /// <param name="value">数值</param>
    //    /// <returns></returns>
    //#if !NET40
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //#endif
    //    public static TResult ChangeType<TResult>(this Object value)
    //    {
    //      if (value is TResult) { return (TResult)value; }

    //      return (TResult)ChangeType(value, typeof(TResult));
    //    }

    //    /// <summary>获取类型的友好名称</summary>
    //    /// <param name="type">指定类型</param>
    //    /// <param name="isfull">是否全名，包含命名空间</param>
    //    /// <returns></returns>
    //#if !NET40
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //#endif
    //    public static String GetName(this Type type, Boolean isfull = false)
    //    {
    //      return Provider.GetName(type, isfull);
    //    }

    //    /// <summary>从参数数组中获取类型数组</summary>
    //    /// <param name="args"></param>
    //    /// <returns></returns>
    //#if !NET40
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //#endif
    //    internal static Type[] GetTypeArray(this Object[] args)
    //    {
    //      if (args == null) { return Type.EmptyTypes; }

    //      var typeArray = new Type[args.Length];
    //      for (int i = 0; i < typeArray.Length; i++)
    //      {
    //        if (args[i] == null)
    //        {
    //          typeArray[i] = typeof(Object);
    //        }
    //        else
    //        {
    //          typeArray[i] = args[i].GetType();
    //        }
    //      }
    //      return typeArray;
    //    }

    ///// <summary>获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身</summary>
    ///// <param name="member"></param>
    ///// <returns></returns>
    //public static Type GetMemberType(this MemberInfo member)
    //{
    //  switch (member.MemberType)
    //  {
    //    case MemberTypes.Constructor:
    //      return (member as ConstructorInfo).DeclaringType;
    //    case MemberTypes.Field:
    //      return (member as FieldInfo).FieldType;
    //    case MemberTypes.Method:
    //      return (member as MethodInfo).ReturnType;
    //    case MemberTypes.Property:
    //      return (member as PropertyInfo).PropertyType;
    //    case MemberTypes.TypeInfo:
    //    case MemberTypes.NestedType:
    //      return member as Type;
    //    default:
    //      return null;
    //  }
    //}

    //public static readonly Type[] EmptyTypes = Type.EmptyTypes;

    /// <summary>获取类型代码</summary>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TypeCode GetTypeCode(this Type type) => Type.GetTypeCode(type);


    #endregion

    #region -- 插件 --

    /// <summary>是否子类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Boolean As(this Type type, Type baseType)
    {
      if (type == null) return false;

      // 如果基类是泛型定义
      if (baseType.IsGenericTypeDefinition && type.IsGenericType && !type.IsGenericTypeDefinition) type = type.GetGenericTypeDefinition();

      if (type == baseType) { return true; }

      if (baseType.IsAssignableFrom(type)) { return true; }

      // 接口
      if (baseType.IsInterface)
      {
        if (type.GetInterface(baseType.Name) != null) { return true; }
        if (type.GetInterfaces().Any(e => e.IsGenericType && baseType.IsGenericTypeDefinition ? e.GetGenericTypeDefinition() == baseType : e == baseType)) return true;
      }

      // 判断是否子类时，支持只反射加载的程序集
      if (type.Assembly.ReflectionOnly)
      {
        // 反射加载时，需要特殊处理接口
        //if (baseType.IsInterface && type.GetInterface(baseType.Name) != null) return true;
        while (type != null && type != TypeConstants.Object)
        {
          if (type.FullName == baseType.FullName && type.AssemblyQualifiedName == baseType.AssemblyQualifiedName)
          {
            return true;
          }
          type = type.BaseType;
        }
      }

      return false;
    }

    /// <summary>是否子类</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Boolean As<T>(this Type type) => type.As(typeof(T));

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
          ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>))) { return true; }

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

    #region ** class ILEmitter **

    private sealed class ILEmitter
    {
      public ILGenerator il;

      public ILEmitter ret() { il.Emit(OpCodes.Ret); return this; }
      public ILEmitter cast(Type type) { il.Emit(OpCodes.Castclass, type); return this; }
      public ILEmitter box(Type type) { il.Emit(OpCodes.Box, type); return this; }
      public ILEmitter unbox_any(Type type) { il.Emit(OpCodes.Unbox_Any, type); return this; }
      public ILEmitter unbox(Type type) { il.Emit(OpCodes.Unbox, type); return this; }
      public ILEmitter call(MethodInfo method) { il.Emit(OpCodes.Call, method); return this; }
      public ILEmitter callvirt(MethodInfo method) { il.Emit(OpCodes.Callvirt, method); return this; }
      public ILEmitter ldnull() { il.Emit(OpCodes.Ldnull); return this; }
      public ILEmitter bne_un(Label target) { il.Emit(OpCodes.Bne_Un, target); return this; }
      public ILEmitter beq(Label target) { il.Emit(OpCodes.Beq, target); return this; }
      public ILEmitter ldc_i4_0() { il.Emit(OpCodes.Ldc_I4_0); return this; }
      public ILEmitter ldc_i4_1() { il.Emit(OpCodes.Ldc_I4_1); return this; }
      public ILEmitter ldc_i4(int c) { il.Emit(OpCodes.Ldc_I4, c); return this; }
      public ILEmitter ldc_r4(float c) { il.Emit(OpCodes.Ldc_R4, c); return this; }
      public ILEmitter ldc_r8(double c) { il.Emit(OpCodes.Ldc_R8, c); return this; }
      public ILEmitter ldarg0() { il.Emit(OpCodes.Ldarg_0); return this; }
      public ILEmitter ldarg1() { il.Emit(OpCodes.Ldarg_1); return this; }
      public ILEmitter ldarg2() { il.Emit(OpCodes.Ldarg_2); return this; }
      public ILEmitter ldarga(int idx) { il.Emit(OpCodes.Ldarga, idx); return this; }
      public ILEmitter ldarga_s(int idx) { il.Emit(OpCodes.Ldarga_S, idx); return this; }
      public ILEmitter ldarg(int idx) { il.Emit(OpCodes.Ldarg, idx); return this; }
      public ILEmitter ldarg_s(int idx) { il.Emit(OpCodes.Ldarg_S, idx); return this; }
      public ILEmitter ldstr(string str) { il.Emit(OpCodes.Ldstr, str); return this; }
      public ILEmitter ifclass_ldind_ref(Type type) { if (!type.IsValueType) il.Emit(OpCodes.Ldind_Ref); return this; }
      public ILEmitter ldloc0() { il.Emit(OpCodes.Ldloc_0); return this; }
      public ILEmitter ldloc1() { il.Emit(OpCodes.Ldloc_1); return this; }
      public ILEmitter ldloc2() { il.Emit(OpCodes.Ldloc_2); return this; }
      public ILEmitter ldloca_s(int idx) { il.Emit(OpCodes.Ldloca_S, idx); return this; }
      public ILEmitter ldloca_s(LocalBuilder local) { il.Emit(OpCodes.Ldloca_S, local); return this; }
      public ILEmitter ldloc_s(int idx) { il.Emit(OpCodes.Ldloc_S, idx); return this; }
      public ILEmitter ldloc_s(LocalBuilder local) { il.Emit(OpCodes.Ldloc_S, local); return this; }
      public ILEmitter ldloca(int idx) { il.Emit(OpCodes.Ldloca, idx); return this; }
      public ILEmitter ldloca(LocalBuilder local) { il.Emit(OpCodes.Ldloca, local); return this; }
      public ILEmitter ldloc(int idx) { il.Emit(OpCodes.Ldloc, idx); return this; }
      public ILEmitter ldloc(LocalBuilder local) { il.Emit(OpCodes.Ldloc, local); return this; }
      public ILEmitter initobj(Type type) { il.Emit(OpCodes.Initobj, type); return this; }
      public ILEmitter newobj(ConstructorInfo ctor) { il.Emit(OpCodes.Newobj, ctor); return this; }
      public ILEmitter Throw() { il.Emit(OpCodes.Throw); return this; }
      public ILEmitter throw_new(Type type) { var exp = type.GetConstructor(Type.EmptyTypes); newobj(exp).Throw(); return this; }
      public ILEmitter stelem_ref() { il.Emit(OpCodes.Stelem_Ref); return this; }
      public ILEmitter ldelem_ref() { il.Emit(OpCodes.Ldelem_Ref); return this; }
      public ILEmitter ldlen() { il.Emit(OpCodes.Ldlen); return this; }
      public ILEmitter stloc(int idx) { il.Emit(OpCodes.Stloc, idx); return this; }
      public ILEmitter stloc_s(int idx) { il.Emit(OpCodes.Stloc_S, idx); return this; }
      public ILEmitter stloc(LocalBuilder local) { il.Emit(OpCodes.Stloc, local); return this; }
      public ILEmitter stloc_s(LocalBuilder local) { il.Emit(OpCodes.Stloc_S, local); return this; }
      public ILEmitter stloc0() { il.Emit(OpCodes.Stloc_0); return this; }
      public ILEmitter stloc1() { il.Emit(OpCodes.Stloc_1); return this; }
      public ILEmitter mark(Label label) { il.MarkLabel(label); return this; }
      public ILEmitter ldfld(FieldInfo field) { il.Emit(OpCodes.Ldfld, field); return this; }
      public ILEmitter ldsfld(FieldInfo field) { il.Emit(OpCodes.Ldsfld, field); return this; }
      public ILEmitter lodfld(FieldInfo field) { if (field.IsStatic) ldsfld(field); else ldfld(field); return this; }
      public ILEmitter ifvaluetype_box(Type type) { if (type.IsValueType) il.Emit(OpCodes.Box, type); return this; }
      public ILEmitter stfld(FieldInfo field) { il.Emit(OpCodes.Stfld, field); return this; }
      public ILEmitter stsfld(FieldInfo field) { il.Emit(OpCodes.Stsfld, field); return this; }
      public ILEmitter setfld(FieldInfo field) { if (field.IsStatic) stsfld(field); else stfld(field); return this; }
      public ILEmitter unboxorcast(Type type) { if (type.IsValueType) unbox(type); else cast(type); return this; }
      public ILEmitter callorvirt(MethodInfo method) { if (method.IsVirtual) il.Emit(OpCodes.Callvirt, method); else il.Emit(OpCodes.Call, method); return this; }
      public ILEmitter stind_ref() { il.Emit(OpCodes.Stind_Ref); return this; }
      public ILEmitter ldind_ref() { il.Emit(OpCodes.Ldind_Ref); return this; }
      public LocalBuilder declocal(Type type) { return il.DeclareLocal(type); }
      public Label deflabel() { return il.DefineLabel(); }
      public ILEmitter ifclass_ldarg_else_ldarga(int idx, Type type) { if (type.IsValueType) emit.ldarga(idx); else emit.ldarg(idx); return this; }
      public ILEmitter ifclass_ldloc_else_ldloca(int idx, Type type) { if (type.IsValueType) emit.ldloca(idx); else emit.ldloc(idx); return this; }
      public ILEmitter perform(Action<ILEmitter, MemberInfo> action, MemberInfo member) { action(this, member); return this; }
      public ILEmitter ifbyref_ldloca_else_ldloc(LocalBuilder local, Type type) { if (type.IsByRef) ldloca(local); else ldloc(local); return this; }
    }

    #endregion
  }
}