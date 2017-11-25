#if NET40
using System;
using System.Reflection;
using System.Linq;

namespace FastExpressionCompiler
{
  internal static class ReflectionUtils
  {
    /// <summary>MSRuntimeLookup - from ReferenceSource\mscorlib\system\type.cs</summary>
    private const BindingFlags MSDefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    /// <summary>MSRuntimeLookup - from ReferenceSource\mscorlib\system\type.cs</summary>
    private const BindingFlags MSDeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    /// <summary>MSRuntimeLookup - from ReferenceSource\mscorlib\system\reflection\RuntimeReflectionExtensions.cs</summary>
    private const BindingFlags MSRuntimeLookup = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    public static Type[] GenericTypeArguments(this Type type)
    {
      return type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericArguments() : Type.EmptyTypes;
    }

    public static bool HasDefaultValue(this ParameterInfo pi)
    {
      const string _DBNullType = "System.DBNull";
      var defaultValue = pi.DefaultValue;
      if (null == defaultValue && pi.ParameterType.IsValueType)
      {
        defaultValue = Activator.CreateInstance(pi.ParameterType);
      }
      return null == defaultValue || !string.Equals(_DBNullType, defaultValue.GetType().FullName, StringComparison.Ordinal);
    }

    private static readonly FieldInfo[] _emptyFields = new FieldInfo[0];
    public static FieldInfo[] GetTypeDeclaredFields(this Type type)
    {
      return type.GetFields(MSDeclaredOnlyLookup) ?? _emptyFields;
    }

    public static MethodInfo GetDeclaredMethod(this Type type, string name)
    {
      return type.GetMethod(name, MSDeclaredOnlyLookup);
    }

    private static readonly MethodInfo[] _emptyMethods = new MethodInfo[0];
    public static MethodInfo[] GetTypeDeclaredMethods(this Type type)
    {
      return type.GetMethods(MSDeclaredOnlyLookup) ?? _emptyMethods;
    }

    private static readonly ConstructorInfo[] _emptyCtors = new ConstructorInfo[0];
    public static ConstructorInfo[] GetTypeDeclaredConstructors(this Type type)
    {
      return type.GetConstructors(MSDeclaredOnlyLookup) ?? _emptyCtors;
    }
  }
}
#endif