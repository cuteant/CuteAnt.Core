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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CuteAnt.Collections;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Reflection
{
  /// <summary>反射接口</summary>
  /// <remarks>该接口仅用于扩展，不建议外部使用</remarks>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  public interface IReflect
  {
    #region 反射获取

    /// <summary>根据名称获取类型</summary>
    /// <param name="typeName">类型名</param>
    /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
    /// <returns></returns>
    Type GetType(String typeName, Boolean isLoadAssembly);

    /// <summary>获取字段</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    FieldInfo GetField(Type type, String name, Boolean ignoreCase);

    /// <summary>获取方法</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    MethodInfo GetMethod(Type type, String name, params Type[] paramTypes);

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    MethodInfo[] GetMethods(Type type, String name, Int32 paramCount = -1);

    /// <summary>获取属性</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    PropertyInfo GetProperty(Type type, String name, Boolean ignoreCase);

    /// <summary>获取成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    MemberInfo GetMember(Type type, String name, Boolean ignoreCase);

    /// <summary>获取字段</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    IList<FieldInfo> GetFields(Type type, Boolean baseFirst = true);

    /// <summary>获取属性</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    IList<PropertyInfo> GetProperties(Type type, Boolean baseFirst = true);

    #endregion

    #region 反射调用

    /// <summary>反射创建指定类型的实例</summary>
    /// <param name="type">类型</param>
    /// <param name="parameters">参数数组</param>
    /// <returns></returns>
    Object CreateInstance(Type type, params Object[] parameters);

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    Object Invoke(Object target, MethodBase method, params Object[] parameters);

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数字典</param>
    /// <returns></returns>
    Object InvokeWithParams(Object target, MethodBase method, IDictionary parameters);

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <returns></returns>
    Object GetValue(Object target, PropertyInfo property);

    /// <summary>获取目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <returns></returns>
    Object GetValue(Object target, FieldInfo field);

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    void SetValue(Object target, PropertyInfo property, Object value);

    /// <summary>设置目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    void SetValue(Object target, FieldInfo field, Object value);

    #endregion

    #region 类型辅助

    /// <summary>获取一个类型的元素类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    Type GetElementType(Type type);

    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    Object ChangeType(Object value, Type conversionType);

    /// <summary>获取类型的友好名称</summary>
    /// <param name="type">指定类型</param>
    /// <param name="isfull">是否全名，包含命名空间</param>
    /// <returns></returns>
    String GetName(Type type, Boolean isfull);

    #endregion

    #region 插件

    /// <summary>是否子类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    Boolean IsSubOf(Type type, Type baseType);

    /// <summary>在指定程序集中查找指定基类或接口的所有子类实现</summary>
    /// <param name="asm">指定程序集</param>
    /// <param name="baseType">基类或接口，为空时返回所有类型</param>
    /// <returns></returns>
    IEnumerable<Type> GetSubclasses(Assembly asm, Type baseType);

    /// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
    /// <param name="baseType">基类或接口</param>
    /// <param name="isLoadAssembly">是否加载为加载程序集</param>
    /// <returns></returns>
    IEnumerable<Type> GetAllSubclasses(Type baseType, Boolean isLoadAssembly);

    #endregion
  }

  /// <summary>默认反射实现</summary>
  /// <remarks>该接口仅用于扩展，不建议外部使用</remarks>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  public class DefaultReflect : IReflect
  {
    #region 反射获取

    /// <summary>根据名称获取类型</summary>
    /// <param name="typeName">类型名</param>
    /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
    /// <returns></returns>
    public virtual Type GetType(String typeName, Boolean isLoadAssembly)
    {
      return Type.GetType(typeName);
    }

    private const BindingFlags DefaultLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;// | BindingFlags.FlattenHierarchy;
    private const BindingFlags DefaultLookupIC = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;

    /// <summary>获取方法</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    public virtual MethodInfo GetMethod(Type type, String name, params Type[] paramTypes)
    {
      if (paramTypes == null) { paramTypes = Type.EmptyTypes; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != typeof(Object))
      {
        var mi = type.GetMethod(name, DefaultLookup, null, paramTypes, null);
        if (mi != null) { return mi; }

        type = TypeX.Create(type).BaseType.Type;
      }
      return null;
    }

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    public virtual MethodInfo[] GetMethods(Type type, String name, Int32 paramCount = -1)
    {
      var ms = type.GetMethods(DefaultLookup);
      if (ms == null || ms.Length == 0) { return ms; }

      var list = new List<MethodInfo>();
      foreach (var item in ms)
      {
        if (item.Name == name)
        {
          if (paramCount >= 0 && item.GetParameters().Length == paramCount) { list.Add(item); }
        }
      }
      return list.ToArray();
    }

    /// <summary>获取属性</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public virtual PropertyInfo GetProperty(Type type, String name, Boolean ignoreCase)
    {
      // 父类私有属性的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
      while (type != null && type != typeof(Object))
      {
        var pi = GetSafeProperty(type, name, ignoreCase ? DefaultLookupIC : DefaultLookup);
        if (pi != null) { return pi; }

        type = TypeX.Create(type).BaseType.Type;
      }
      return null;
    }

    /// <summary>获取字段</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public virtual FieldInfo GetField(Type type, String name, Boolean ignoreCase)
    {
      // 父类私有字段的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
      while (type != null && type != typeof(Object))
      {
        var fi = GetSafeField(type, name, ignoreCase ? DefaultLookupIC : DefaultLookup);
        if (fi != null) { return fi; }

        type = TypeX.Create(type).BaseType.Type;
      }
      return null;
    }

    /// <summary>获取成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public virtual MemberInfo GetMember(Type type, String name, Boolean ignoreCase)
    {
      // 父类私有成员的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
      while (type != null && type != typeof(Object))
      {
        var fs = type.GetMember(name, ignoreCase ? DefaultLookupIC : DefaultLookup);
        if (fs != null && fs.Length > 0)
        {
          // 得到多个的时候，优先返回精确匹配
          if (ignoreCase && fs.Length > 1)
          {
            foreach (var fi in fs)
            {
              if (fi.Name == name) { return fi; }
            }
          }
          return fs[0];
        }

        type = TypeX.Create(type).BaseType.Type;
      }
      return null;
    }

    //private static MethodInfo GetSafeMethod(Type type, string methodName, BindingFlags flags, params Type[] paramTypes)
    //{
    //	try
    //	{
    //		return type.GetMethod(methodName, flags);
    //	}
    //	catch (AmbiguousMatchException)
    //	{
    //		return type.GetMethods(flags).First(mi => mi.Name == methodName);
    //	}
    //}

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FieldInfo GetSafeField(Type type, String propertyName, BindingFlags flags)
    {
      try
      {
        return type.GetField(propertyName, flags);
      }
      catch (AmbiguousMatchException)
      {
        return type.GetFields(flags).First(pi => pi.Name == propertyName);
      }
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PropertyInfo GetSafeProperty(Type type, String propertyName, BindingFlags flags)
    {
      try
      {
        return type.GetProperty(propertyName, flags);
      }
      catch (AmbiguousMatchException)
      {
        return type.GetProperties(flags).First(pi => pi.Name == propertyName);
      }
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static EventInfo GetSafeEvent(Type type, String eventName, BindingFlags flags)
    {
      try
      {
        return type.GetEvent(eventName, flags);
      }
      catch (AmbiguousMatchException)
      {
        return type.GetEvents(flags).First(ei => ei.Name == eventName);
      }
    }

    #endregion

    #region 反射获取 字段/属性

    private DictionaryCache<Type, IList<FieldInfo>> _cache1 = new DictionaryCache<Type, IList<FieldInfo>>();
    private DictionaryCache<Type, IList<FieldInfo>> _cache2 = new DictionaryCache<Type, IList<FieldInfo>>();
    /// <summary>获取字段</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    public virtual IList<FieldInfo> GetFields(Type type, Boolean baseFirst = true)
    {
      if (baseFirst)
        return _cache1.GetItem(type, key => GetFields2(key, true));
      else
        return _cache2.GetItem(type, key => GetFields2(key, false));
    }

    IList<FieldInfo> GetFields2(Type type, Boolean baseFirst)
    {
      var list = new List<FieldInfo>();

      // Void*的基类就是null
      if (type == typeof(Object) || type.BaseType == null) return list;

      if (baseFirst) list.AddRange(GetFields(type.BaseType));

      var fis = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      foreach (var fi in fis)
      {
        if (fi.GetCustomAttribute<NonSerializedAttribute>() != null) continue;

        list.Add(fi);
      }

      if (!baseFirst) list.AddRange(GetFields(type.BaseType));

      return list;
    }

    private DictionaryCache<Type, IList<PropertyInfo>> _cache3 = new DictionaryCache<Type, IList<PropertyInfo>>();
    private DictionaryCache<Type, IList<PropertyInfo>> _cache4 = new DictionaryCache<Type, IList<PropertyInfo>>();
    /// <summary>获取属性</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    public virtual IList<PropertyInfo> GetProperties(Type type, Boolean baseFirst = true)
    {
      if (baseFirst)
        return _cache3.GetItem(type, key => GetProperties2(key, true));
      else
        return _cache4.GetItem(type, key => GetProperties2(key, false));
    }

    IList<PropertyInfo> GetProperties2(Type type, Boolean baseFirst)
    {
      var list = new List<PropertyInfo>();

      // Void*的基类就是null
      if (type == typeof(Object) || type.BaseType == null) return list;

      // 本身type.GetProperties就可以得到父类属性，只是不能保证父类属性在子类属性之前
      if (baseFirst) list.AddRange(GetProperties(type.BaseType));

      // 父类子类可能因为继承而有重名的属性，此时以子类优先，否则反射父类属性会出错
      var set = new HashSet<String>(list.Select(e => e.Name));

      //var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (var pi in pis)
      {
        if (pi.GetIndexParameters().Length > 0) continue;
        if (pi.GetCustomAttributeX<XmlIgnoreAttribute>() != null) continue;
        if (pi.GetCustomAttributeX<IgnoreDataMemberAttribute>() != null) continue;

        if (!set.Contains(pi.Name))
        {
          list.Add(pi);
          set.Add(pi.Name);
        }
      }

      if (!baseFirst) list.AddRange(GetProperties(type.BaseType).Where(e => !set.Contains(e.Name)));

      return list;
    }

    #endregion

    #region 反射调用

    /// <summary>反射创建指定类型的实例</summary>
    /// <param name="type">类型</param>
    /// <param name="parameters">参数数组</param>
    /// <returns></returns>
    public virtual Object CreateInstance(Type type, params Object[] parameters)
    {
      if (parameters == null || parameters.Length == 0)
      {
        return Activator.CreateInstance(type, true);
      }
      else
      {
        return Activator.CreateInstance(type, parameters);
      }
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    public virtual Object Invoke(Object target, MethodBase method, params Object[] parameters)
    {
      return method.Invoke(target, parameters);
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数字典</param>
    /// <returns></returns>
    public virtual Object InvokeWithParams(Object target, MethodBase method, IDictionary parameters)
    {
      // 该方法没有参数，无视外部传入参数
      var pis = method.GetParameters();
      if (pis == null || pis.Length < 1) return Invoke(target, method, null);

      var ps = new Object[pis.Length];
      for (int i = 0; i < pis.Length; i++)
      {
        Object v = null;
        if (parameters != null && parameters.Contains(pis[i].Name)) v = parameters[pis[i].Name];
        ps[i] = v.ChangeType(pis[i].ParameterType);
      }

      return method.Invoke(target, ps);
    }

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <returns></returns>
    public virtual Object GetValue(Object target, PropertyInfo property)
    {
      return property.GetValue(target, null);
    }

    /// <summary>获取目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <returns></returns>
    public virtual Object GetValue(Object target, FieldInfo field)
    {
      return field.GetValue(target);
    }

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    public virtual void SetValue(Object target, PropertyInfo property, Object value)
    {
      property.SetValue(target, value, null);
    }

    /// <summary>设置目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    public virtual void SetValue(Object target, FieldInfo field, Object value)
    {
      field.SetValue(target, value);
    }

    #endregion

    #region 类型辅助

    /// <summary>获取一个类型的元素类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public virtual Type GetElementType(Type type)
    {
      return type.GetElementType();
    }

    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    public virtual Object ChangeType(Object value, Type conversionType)
    {
      return Convert.ChangeType(value, conversionType);
    }

    /// <summary>获取类型的友好名称</summary>
    /// <param name="type">指定类型</param>
    /// <param name="isfull">是否全名，包含命名空间</param>
    /// <returns></returns>
    public virtual String GetName(Type type, Boolean isfull)
    {
      return isfull ? type.FullName : type.Name;
    }

    #endregion

    #region 插件

    /// <summary>是否子类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public Boolean IsSubOf(Type type, Type baseType)
    {
      if (type == null) return false;
      if (type == baseType) return false;

      if (baseType.IsAssignableFrom(type)) return true;

      // 判断是否子类时，支持只反射加载的程序集
      if (type.Assembly.ReflectionOnly)
      {
        while (type != typeof(Object))
        {
          if (type.FullName == baseType.FullName &&
              type.AssemblyQualifiedName == baseType.AssemblyQualifiedName)
            return true;
          type = type.BaseType;
        }
      }

      return false;
    }

    /// <summary>在指定程序集中查找指定基类的子类</summary>
    /// <param name="asm">指定程序集</param>
    /// <param name="baseType">基类或接口，为空时返回所有类型</param>
    /// <returns></returns>
    public virtual IEnumerable<Type> GetSubclasses(Assembly asm, Type baseType)
    {
      ValidationHelper.ArgumentNull(asm, "asm");

      //foreach (var item in asm.GetTypes())
      //{
      //  if (baseType == null || baseType.IsAssignableFrom(item)) { yield return item; }
      //}
      return AssemblyX.Create(asm).FindPlugins(baseType);
    }

    /// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
    /// <param name="baseType">基类或接口</param>
    /// <param name="isLoadAssembly">是否加载为加载程序集</param>
    /// <returns></returns>
    public virtual IEnumerable<Type> GetAllSubclasses(Type baseType, Boolean isLoadAssembly)
    {
      // 不支持isLoadAssembly
      //foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
      //{
      //  foreach (var type in GetSubclasses(asm, baseType))
      //  {
      //    yield return type;
      //  }
      //}
      return AssemblyX.FindAllPlugins(baseType, isLoadAssembly);
    }

    #endregion

    #region 辅助方法

    /// <summary>获取类型，如果target是Type类型，则表示要反射的是静态成员</summary>
    /// <param name="target">目标对象</param>
    /// <returns></returns>
    protected virtual Type GetType(ref Object target)
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

    #endregion
  }
}