using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
  /// <summary>反射工具类</summary>
  public static partial class ReflectUtils
  {
    #region -- 反射获取 --

    #region - Type -

    private static Dictionary<Type, object> s_defaultValueTypes = new Dictionary<Type, object>();
    /// <summary>GetDefaultValue</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetDefaultValue(this Type type)
    {
      if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

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

    private static CachedReadConcurrentDictionary<string, Type> s_genericTypeCache =
        new CachedReadConcurrentDictionary<string, Type>(DictionaryCacheConstants.SIZE_MEDIUM, StringComparer.Ordinal);
    private static Func<string, Type, Type[], Type> s_makeGenericTypeFunc = MakeGenericTypeInternal;

    /// <summary>GetCachedGenericType</summary>
    /// <param name="type"></param>
    /// <param name="argTypes"></param>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <returns></returns>
    public static Type GetCachedGenericType(this Type type, params Type[] argTypes)
    {
      if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

      if (!type.IsGenericTypeDefinition)
      {
        ThrowArgumentException_Make(type);
      }

      if (argTypes == null) { argTypes = Type.EmptyTypes; }

      var sb = StringBuilderCache.Acquire().Append(TypeUtils.GetTypeIdentifier(type));
      foreach (var argType in argTypes)
      {
        sb.Append('|').Append(TypeUtils.GetTypeIdentifier(argType));
      }
      var typeKey = StringBuilderCache.GetStringAndRelease(sb);

      return s_genericTypeCache.GetOrAdd(typeKey, s_makeGenericTypeFunc, type, argTypes);
    }

    private static Type MakeGenericTypeInternal(string typeKey, Type type, Type[] argTypes) => type.MakeGenericType(argTypes);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowArgumentException_Make(Type type)
    {
      throw GetArgumentException();
      ArgumentException GetArgumentException()
      {
        return new ArgumentException($"{type.FullName} is not a Generic Type Definition", nameof(type));

      }
    }

    #endregion

    #region - LookupMember -

    /// <summary>获取成员。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static MemberInfo LookupMember(this Type type, String name)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      var property = type.LookupTypeProperty(name);
      if (property != null) { return property; }

      var field = type.LookupTypeField(name);
      if (field != null) { return field; }

      // 通过反射获取
      while (type != null && type != TypeConstants.ObjectType)
      {
        var fs = type.GetMember(name, BindingFlagsHelper.DefaultDeclaredOnlyLookup);
        if (fs != null && fs.Length > 0) { return fs[0]; }

#if NET40
        type = type.BaseType;
#else
        type = type.BaseType;
#endif
      }

      return null;
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

    #endregion

    #region -- 插件 --

    /// <summary>是否子类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
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
        while (type != null && type != TypeConstants.ObjectType)
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
    [MethodImpl(InlineMethod.Value)]
    public static Boolean As<T>(this Type type) => type.As(typeof(T));

    #endregion

    #region -- 辅助方法 --

    /// <summary>获取类型，如果target是Type类型，则表示要反射的是静态成员</summary>
    /// <param name="target">目标对象</param>
    /// <returns></returns>
    static Type GetTypeInternal(ref Object target)
    {
      if (null == target) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.target);

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