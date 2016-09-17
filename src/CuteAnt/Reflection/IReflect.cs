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

    #endregion

    #region 反射调用

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

    /// <summary>获取方法</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    public virtual MethodInfo GetMethod(Type type, String name, params Type[] paramTypes)
    {
      if (paramTypes == null) { paramTypes = Type.EmptyTypes; }

      // 父类属性的获取需要递归，有些类型的父类为空，比如接口
      while (type != null && type != TypeX._.Object)
      {
        var mi = type.GetMethod(name, BindingFlagsHelper.DefaultDeclaredOnlyLookup, null, paramTypes, null);
        if (mi != null) { return mi; }

        type = type.BaseType();
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
      var ms = type.GetMethods(BindingFlagsHelper.DefaultLookup);
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

    #endregion

    #region 反射调用

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