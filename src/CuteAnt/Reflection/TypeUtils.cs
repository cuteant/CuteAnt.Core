using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using CuteAnt.Collections;
using CuteAnt.Hosting;
using Microsoft.Extensions.Logging;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Reflection
{
  /// <summary>A collection of utility functions for dealing with Type information.</summary>
  public static class TypeUtils
  {
    #region -- 常用类型 --

    /// <summary>常用类型</summary>
    internal static class _
    {
      /// <summary>类型</summary>
      public static readonly Type Type = TypeConstants.Type;

      /// <summary>值类型</summary>
      public static readonly Type ValueType = TypeConstants.ValueType;

      /// <summary>枚举类型</summary>
      public static readonly Type Enum = TypeConstants.Enum;

      /// <summary>对象类型</summary>
      public static readonly Type Object = TypeConstants.Object;

      /// <summary>字符串类型</summary>
      public static readonly Type String = TypeConstants.String;

      /// <summary>Guid</summary>
      public static readonly Type Guid = TypeConstants.Guid;

      /// <summary>CombGuid</summary>
      public static readonly Type CombGuid = TypeConstants.CombGuid;

      /// <summary>Guid</summary>
      public static readonly Type ByteArray = TypeConstants.ByteArray;
    }

    #endregion

    #region @@ Fields @@

    private static readonly ConcurrentDictionary<Tuple<Type, TypeFormattingOptions>, string> ParseableNameCache = new ConcurrentDictionary<Tuple<Type, TypeFormattingOptions>, string>();

    private static readonly ConcurrentDictionary<Tuple<Type, bool>, List<Type>> ReferencedTypes = new ConcurrentDictionary<Tuple<Type, bool>, List<Type>>();

    private static readonly CachedReflectionOnlyTypeResolver ReflectionOnlyTypeResolver = new CachedReflectionOnlyTypeResolver();

    #endregion

    #region -- GetSimpleTypeName --

#if NET40
    public static string GetSimpleTypeName(Type type, Predicate<Type> fullName)
    {
      if (type.IsNestedPublic || type.IsNestedPrivate)
      {
        if (type.DeclaringType.GetTypeInfo().IsGenericType)
        {
          return GetTemplatedName(
              GetUntemplatedTypeName(type.DeclaringType.Name),
              type.DeclaringType,
              type.GetGenericArguments(),
              _ => true) + "." + GetUntemplatedTypeName(type.Name);
        }

        return GetTemplatedName(type.DeclaringType) + "." + GetUntemplatedTypeName(type.Name);
      }

      if (type.IsGenericType) return GetSimpleTypeName(fullName != null && fullName(type) ? GetFullName(type) : type.Name);

      return fullName != null && fullName(type) ? GetFullName(type) : type.Name;
    }
#else
    public static string GetSimpleTypeName(Type t, Predicate<Type> fullName)
    {
      return GetSimpleTypeName(t.GetTypeInfo(), fullName);
    }

    public static string GetSimpleTypeName(TypeInfo typeInfo, Predicate<Type> fullName)
    {
      if (typeInfo.IsNestedPublic || typeInfo.IsNestedPrivate)
      {
        if (typeInfo.DeclaringType.GetTypeInfo().IsGenericType)
        {
          return GetTemplatedName(
              GetUntemplatedTypeName(typeInfo.DeclaringType.Name),
              typeInfo.DeclaringType,
              typeInfo.GetGenericArguments(),
              _ => true) + "." + GetUntemplatedTypeName(typeInfo.Name);
        }

        return GetTemplatedName(typeInfo.DeclaringType) + "." + GetUntemplatedTypeName(typeInfo.Name);
      }

      var type = typeInfo.AsType();
      if (typeInfo.IsGenericType) return GetSimpleTypeName(fullName != null && fullName(type) ? GetFullName(type) : typeInfo.Name);

      return fullName != null && fullName(type) ? GetFullName(type) : typeInfo.Name;
    }
#endif

    public static string GetSimpleTypeName(string typeName)
    {
      int i = typeName.IndexOf('`');
      if (i > 0)
      {
        typeName = typeName.Substring(0, i);
      }
      i = typeName.IndexOf('[');
      if (i > 0)
      {
        typeName = typeName.Substring(0, i);
      }
      i = typeName.IndexOf('<');
      if (i > 0)
      {
        typeName = typeName.Substring(0, i);
      }
      return typeName;
    }

    // 这里不采用 Json.Net 的代码
    //private static readonly Func<Type, string> _getSimpleTypeNameFunc = GetSimpleTypeNameInternal;
    private static readonly Func<Type, string> _getSimpleTypeNameFunc = RuntimeTypeNameFormatter.Format;
    private static readonly DictionaryCache<Type, string> _simpleTypeNameCache = new DictionaryCache<Type, string>(DictionaryCacheConstants.SIZE_MEDIUM);

    public static string GetSimpleTypeName(Type type)
    {
      if (null == type) { throw new ArgumentNullException(nameof(type)); }
      return _simpleTypeNameCache.GetItem(type, _getSimpleTypeNameFunc);
    }

    //private static string GetSimpleTypeNameInternal(Type type)
    //{
    //  string fullyQualifiedTypeName = type.AssemblyQualifiedName;

    //  return RemoveAssemblyDetails(fullyQualifiedTypeName);
    //}

    //private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
    //{
    //  var builder = new StringBuilder();

    //  // loop through the type name and filter out qualified assembly details from nested type names
    //  bool writingAssemblyName = false;
    //  bool skippingAssemblyDetails = false;
    //  for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
    //  {
    //    char current = fullyQualifiedTypeName[i];
    //    switch (current)
    //    {
    //      case '[':
    //        writingAssemblyName = false;
    //        skippingAssemblyDetails = false;
    //        builder.Append(current);
    //        break;
    //      case ']':
    //        writingAssemblyName = false;
    //        skippingAssemblyDetails = false;
    //        builder.Append(current);
    //        break;
    //      case ',':
    //        if (!writingAssemblyName)
    //        {
    //          writingAssemblyName = true;
    //          builder.Append(current);
    //        }
    //        else
    //        {
    //          skippingAssemblyDetails = true;
    //        }
    //        break;
    //      default:
    //        if (!skippingAssemblyDetails)
    //        {
    //          builder.Append(current);
    //        }
    //        break;
    //    }
    //  }

    //  return builder.ToString();
    //}

    #endregion

    #region -- SerializeTypeName --

    private const char c_keyDelimiter = ':';
    private static readonly DictionaryCache<Type, string> _typeNameSerializerCache = new DictionaryCache<Type, string>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<Type, string> _serializeTypeNameFunc = SerializeTypeNameInternal;

    public static string SerializeTypeName(Type type)
    {
      if (null == type) { throw new ArgumentNullException(nameof(type)); }

      return _typeNameSerializerCache.GetItem(type, _serializeTypeNameFunc);
    }

    private static string SerializeTypeNameInternal(Type t)
    {
      var typeName = GetSimpleTypeName(t);
      return typeName.Replace(", ", ":");
    }

    #endregion

    #region -- GetUntemplatedTypeName --

    public static string GetUntemplatedTypeName(string typeName)
    {
      int i = typeName.IndexOf('`');
      if (i > 0)
      {
        typeName = typeName.Substring(0, i);
      }
      i = typeName.IndexOf('<');
      if (i > 0)
      {
        typeName = typeName.Substring(0, i);
      }
      return typeName;
    }

    #endregion

    #region -- GetTemplatedName --

    public static string GetTemplatedName(Type t, Predicate<Type> fullName = null)
    {
      if (fullName == null)
        fullName = _ => true; // default to full type names

#if NET40
      if (t.IsGenericType) return GetTemplatedName(GetSimpleTypeName(t, fullName), t, t.GetGenericArguments(), fullName);
#else
      var typeInfo = t.GetTypeInfo();
      if (typeInfo.IsGenericType) return GetTemplatedName(GetSimpleTypeName(typeInfo, fullName), t, typeInfo.GetGenericArguments(), fullName);
#endif

      if (t.IsArray)
      {
        return GetTemplatedName(t.GetElementType(), fullName)
               + "["
               + new string(',', t.GetArrayRank() - 1)
               + "]";
      }

#if NET40
      return GetSimpleTypeName(t, fullName);
#else
      return GetSimpleTypeName(typeInfo, fullName);
#endif
    }

    public static string GetTemplatedName(string baseName, Type t, Type[] genericArguments, Predicate<Type> fullName)
    {
#if NET40
      if (!t.IsGenericType || (t.DeclaringType != null && t.DeclaringType.IsGenericType)) return baseName;
#else
      var typeInfo = t.GetTypeInfo();
      if (!typeInfo.IsGenericType || (t.DeclaringType != null && t.DeclaringType.GetTypeInfo().IsGenericType)) return baseName;
#endif
      string s = baseName;
      s += "<";
      s += GetGenericTypeArgs(genericArguments, fullName);
      s += ">";
      return s;
    }

    #endregion

    #region -- GetTypeInfos --

#if !NET40
    public static IEnumerable<TypeInfo> GetTypeInfos(this IEnumerable<Type> types)
    {
      if (null == types) { return Enumerable.Empty<TypeInfo>(); }
      return types.Select(t => t.GetTypeInfo());
    }
#endif

    #endregion

    #region -- GetGenericTypeArgs --

    public static string GetGenericTypeArgs(IEnumerable<Type> args, Predicate<Type> fullName)
    {
      string s = string.Empty;

      bool first = true;
      foreach (var genericParameter in args)
      {
        if (!first)
        {
          s += ",";
        }
        if (!genericParameter.GetTypeInfo().IsGenericType)
        {
          s += GetSimpleTypeName(genericParameter, fullName);
        }
        else
        {
          s += GetTemplatedName(genericParameter, fullName);
        }
        first = false;
      }

      return s;
    }

    #endregion

    #region -- GetParameterizedTemplateName --

#if !NET40
    public static string GetParameterizedTemplateName(TypeInfo typeInfo, bool applyRecursively = false, Predicate<Type> fullName = null)
    {
      if (fullName == null)
        fullName = tt => true;

      return GetParameterizedTemplateName(typeInfo, fullName, applyRecursively);
    }

    public static string GetParameterizedTemplateName(TypeInfo typeInfo, Predicate<Type> fullName, bool applyRecursively = false)
    {
      if (typeInfo.IsGenericType)
      {
        return GetParameterizedTemplateName(GetSimpleTypeName(typeInfo, fullName), typeInfo, applyRecursively, fullName);
      }

      var t = typeInfo.AsType();
      if (fullName != null && fullName(t) == true)
      {
        return t.FullName;
      }

      return t.Name;
    }

    public static string GetParameterizedTemplateName(string baseName, TypeInfo typeInfo, bool applyRecursively = false, Predicate<Type> fullName = null)
    {
      if (fullName == null)
        fullName = tt => false;

      if (!typeInfo.IsGenericType) return baseName;

      string s = baseName;
      s += "<";
      bool first = true;
      foreach (var genericParameter in typeInfo.GetGenericArguments())
      {
        if (!first)
        {
          s += ",";
        }
        var genericParameterTypeInfo = genericParameter.GetTypeInfo();
        if (applyRecursively && genericParameterTypeInfo.IsGenericType)
        {
          s += GetParameterizedTemplateName(genericParameterTypeInfo, applyRecursively);
        }
        else
        {
          s += genericParameter.FullName == null || !fullName(genericParameter)
              ? genericParameter.Name
              : genericParameter.FullName;
        }
        first = false;
      }
      s += ">";
      return s;
    }
#endif

    #endregion

    #region -- GetRawClassName --

    public static string GetRawClassName(string baseName, Type t)
    {
#if NET40
      return t.IsGenericType ? baseName + '`' + t.GetGenericArguments().Length : baseName;
#else
      var typeInfo = t.GetTypeInfo();
      return typeInfo.IsGenericType ? baseName + '`' + typeInfo.GetGenericArguments().Length : baseName;
#endif
    }

    public static string GetRawClassName(string typeName)
    {
      int i = typeName.IndexOf('[');
      return i <= 0 ? typeName : typeName.Substring(0, i);
    }

    #endregion

    #region -- GenericTypeArgsFromClassName --

    public static Type[] GenericTypeArgsFromClassName(string className)
    {
      return GenericTypeArgsFromArgsString(GenericTypeArgsString(className));
    }

    #endregion

    #region -- GenericTypeArgsFromArgsString --

    public static Type[] GenericTypeArgsFromArgsString(string genericArgs)
    {
      if (string.IsNullOrEmpty(genericArgs)) return Type.EmptyTypes;

      var genericTypeDef = genericArgs.Replace("[]", "##"); // protect array arguments

      return InnerGenericTypeArgs(genericTypeDef);
    }

    #endregion

    #region ** InnerGenericTypeArgs **

    private static Type[] InnerGenericTypeArgs(string className)
    {
      var typeArgs = new List<Type>();
      var innerTypes = GetInnerTypes(className);

      foreach (var innerType in innerTypes)
      {
        if (innerType.StartsWith("[[")) // Resolve and load generic types recursively
        {
          InnerGenericTypeArgs(GenericTypeArgsString(innerType));
          string genericTypeArg = className.Trim('[', ']');
          typeArgs.Add(Type.GetType(genericTypeArg.Replace("##", "[]")));
        }

        else
        {
          string nonGenericTypeArg = innerType.Trim('[', ']');
          typeArgs.Add(Type.GetType(nonGenericTypeArg.Replace("##", "[]")));
        }
      }

      return typeArgs.ToArray();
    }

    #endregion

    #region ** GetInnerTypes **

    private static string[] GetInnerTypes(string input)
    {
      // Iterate over strings of length 2 positionwise.
      var charsWithPositions = input.Zip(Enumerable.Range(0, input.Length), (c, i) => new { Ch = c, Pos = i });
      var candidatesWithPositions = charsWithPositions.Zip(charsWithPositions.Skip(1), (c1, c2) => new { Str = c1.Ch.ToString() + c2.Ch, Pos = c1.Pos });

      var results = new List<string>();
      int startPos = -1;
      int endPos = -1;
      int endTokensNeeded = 0;
      string curStartToken = "";
      string curEndToken = "";
      var tokenPairs = new[] { new { Start = "[[", End = "]]" }, new { Start = "[", End = "]" } }; // Longer tokens need to come before shorter ones

      foreach (var candidate in candidatesWithPositions)
      {
        if (startPos == -1)
        {
          foreach (var token in tokenPairs)
          {
            if (candidate.Str.StartsWith(token.Start))
            {
              curStartToken = token.Start;
              curEndToken = token.End;
              startPos = candidate.Pos;
              break;
            }
          }
        }

        if (curStartToken != "" && candidate.Str.StartsWith(curStartToken))
          endTokensNeeded++;

        if (curEndToken != "" && candidate.Str.EndsWith(curEndToken))
        {
          endPos = candidate.Pos;
          endTokensNeeded--;
        }

        if (endTokensNeeded == 0 && startPos != -1)
        {
          results.Add(input.Substring(startPos, endPos - startPos + 2));
          startPos = -1;
          curStartToken = "";
        }
      }

      return results.ToArray();
    }

    #endregion

    #region -- GenericTypeArgsString --

    public static string GenericTypeArgsString(string className)
    {
      int startIndex = className.IndexOf('[');
      int endIndex = className.LastIndexOf(']');
      return className.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    #endregion

    #region ** GetFullName **

#if !NET40
    private static string GetFullName(TypeInfo typeInfo)
    {
      if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));
      return GetFullName(typeInfo.AsType());
    }
#endif

    private static string GetFullName(Type t)
    {
      if (t == null) throw new ArgumentNullException(nameof(t));
      if (t.IsNested && !t.IsGenericParameter)
      {
        return t.Namespace + "." + t.DeclaringType.Name + "." + t.Name;
      }
      if (t.IsArray)
      {
        return GetFullName(t.GetElementType())
               + "["
               + new string(',', t.GetArrayRank() - 1)
               + "]";
      }
      return t.FullName ?? (t.IsGenericParameter ? t.Name : t.Namespace + "." + t.Name);
    }

    #endregion

    #region -- IsConcreteTemplateType --

    public static bool IsConcreteTemplateType(Type t)
    {
#if NET40
      if (t.IsGenericType) return true;
#else
      if (t.GetTypeInfo().IsGenericType) return true;
#endif
      return t.IsArray && IsConcreteTemplateType(t.GetElementType());
    }

    #endregion

    #region -- IsGenericClass --

    public static bool IsGenericClass(string name)
    {
      return name.Contains("`") || name.Contains("[");
    }

    #endregion

    #region -- GetAllFields --

    /// <summary>Returns all fields of the specified type.</summary>
    /// <param name="type">The type.</param>
    /// <returns>All fields of the specified type.</returns>
    public static IEnumerable<FieldInfo> GetAllFields(this Type type)
    {
      const BindingFlags AllFields =
          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
      var current = type;
      while ((current != typeof(object)) && (current != null))
      {
        var fields = current.GetFields(AllFields);
        foreach (var field in fields)
        {
          yield return field;
        }

        current = current.GetTypeInfo().BaseType;
      }
    }

    #endregion

    #region -- IsNotSerialized --

    /// <summary>Returns <see langword="true"/> if <paramref name="field"/> is marked as
    /// <see cref="FieldAttributes.NotSerialized"/>, <see langword="false"/> otherwise.</summary>
    /// <param name="field">The field.</param>
    /// <returns><see langword="true"/> if <paramref name="field"/> is marked as
    /// <see cref="FieldAttributes.NotSerialized"/>, <see langword="false"/> otherwise.</returns>
    public static bool IsNotSerialized(this FieldInfo field)
        => (field.Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized;

    #endregion

    #region -- IsGeneratedType --

#if !NET40
    public static bool IsGeneratedType(Type type)
    {
      return TypeHasAttribute(type, typeof(GeneratedCodeAttribute));
    }
#endif

    #endregion

    #region -- IsInNamespace --

    /// <summary>Returns true if the provided <paramref name="type"/> is in any of the provided
    /// <paramref name="namespaces"/>, false otherwise.</summary>
    /// <param name="type">The type to check.</param>
    /// <param name="namespaces"></param>
    /// <returns>true if the provided <paramref name="type"/> is in any of the provided <paramref name="namespaces"/>, false otherwise.</returns>
    public static bool IsInNamespace(Type type, List<string> namespaces)
    {
      var typens = type.Namespace;
      if (typens == null)
      {
        return false;
      }

      foreach (var ns in namespaces)
      {
        if (ns.Length > typens.Length)
        {
          continue;
        }

        // If the candidate namespace is a prefix of the type's namespace, return true.
        if (typens.StartsWith(ns, StringComparison.Ordinal)
            && (typens.Length == ns.Length || typens[ns.Length] == '.'))
        {
          return true;
        }
      }

      return false;
    }

    #endregion

    #region -- CanUseReflectionOnly --

    private static readonly Lazy<bool> canUseReflectionOnly = new Lazy<bool>(() =>
    {
      try
      {
        ReflectionOnlyTypeResolver.TryResolveType(typeof(TypeUtils).AssemblyQualifiedName, out Type t);
        return true;
      }
      catch (PlatformNotSupportedException)
      {
        return false;
      }
      catch (Exception)
      {
        // if other exceptions not related to platform ocurr, assume that ReflectionOnly is supported
        return true;
      }
    });

    public static bool CanUseReflectionOnly => canUseReflectionOnly.Value;

    #endregion

    #region -- ResolveReflectionOnlyType / ToReflectionOnlyType --

    public static Type ResolveReflectionOnlyType(string assemblyQualifiedName)
    {
      return ReflectionOnlyTypeResolver.ResolveType(assemblyQualifiedName);
    }

    public static Type ToReflectionOnlyType(Type type)
    {
      if (CanUseReflectionOnly)
      {
        return type.Assembly.ReflectionOnly ? type : ResolveReflectionOnlyType(type.AssemblyQualifiedName);
      }
      else
      {
        return type;
      }
    }

    #endregion

    #region -- GetTypes / GetDefinedTypes --

#if NET40
    public static IEnumerable<Type> GetTypes(Assembly assembly, Predicate<Type> whereFunc, ILogger logger)
    {
      return assembly.IsDynamic ? Enumerable.Empty<Type>() : GetDefinedTypes(assembly, logger).Where(type => !type.GetTypeInfo().IsNestedPrivate && whereFunc(type));
    }

    public static IEnumerable<Type> GetDefinedTypes(Assembly assembly, ILogger logger)
    {
      try
      {
        return assembly.GetTypes();
      }
      catch (Exception exception)
      {
        if (exception is ReflectionTypeLoadException typeLoadException)
        {
          if (typeLoadException.LoaderExceptions != null)
          {
            //
            // If we've only BadImageFormatExceptions in LoaderExceptions, then it's ok to not to log, otherwise log
            // as a warning.
            //

            if (logger.IsWarningLevelEnabled())
            {
              if (typeLoadException.LoaderExceptions.Any(ex => !(ex is BadImageFormatException)))
              {
                var message = $"Exception loading types from assembly '{assembly.FullName}': {TraceLogger.PrintException(exception)}.";
                logger.LogWarning(exception, message);
              }
            }
          }

          return typeLoadException.Types?.Where(type => type != null) ?? Enumerable.Empty<Type>();
        }

        if (logger.IsWarningLevelEnabled())
        {
          var message = $"Exception loading types from assembly '{assembly.FullName}': {TraceLogger.PrintException(exception)}.";
          logger.LogWarning(exception, message);
        }

        return Enumerable.Empty<Type>();
      }
    }
#else
    public static IEnumerable<Type> GetTypes(Assembly assembly, Predicate<Type> whereFunc, ILogger logger)
    {
      return assembly.IsDynamic ? Enumerable.Empty<Type>() : GetDefinedTypes(assembly, logger).Select(t => t.AsType()).Where(type => !type.GetTypeInfo().IsNestedPrivate && whereFunc(type));
    }

    public static IEnumerable<TypeInfo> GetDefinedTypes(Assembly assembly, ILogger logger)
    {
      try
      {
        return assembly.DefinedTypes;
      }
      catch (Exception exception)
      {
        if (exception is ReflectionTypeLoadException typeLoadException)
        {
          if (typeLoadException.LoaderExceptions != null)
          {
            //
            // If we've only BadImageFormatExceptions in LoaderExceptions, then it's ok to not to log, otherwise log
            // as a warning.
            //

            if (logger.IsWarningLevelEnabled())
            {
              if (typeLoadException.LoaderExceptions.Any(ex => !(ex is BadImageFormatException)))
              {
                var message = $"Exception loading types from assembly '{assembly.FullName}': {TraceLogger.PrintException(exception)}.";
                logger.LogWarning(exception, message);
              }
            }
          }

          return typeLoadException.Types?.Where(type => type != null).Select(type => type.GetTypeInfo()) ??
                 Enumerable.Empty<TypeInfo>();
        }

        if (logger.IsWarningLevelEnabled())
        {
          var message = $"Exception loading types from assembly '{assembly.FullName}': {TraceLogger.PrintException(exception)}.";
          logger.LogWarning(exception, message);
        }

        return Enumerable.Empty<TypeInfo>();
      }
    }
#endif

    #endregion

    #region == TypeHasAttribute ==

#if !NET40
    internal static bool TypeHasAttribute(Type type, Type attribType)
    {
      if (type.Assembly.ReflectionOnly || attribType.Assembly.ReflectionOnly)
      {
        type = ToReflectionOnlyType(type);
        attribType = ToReflectionOnlyType(attribType);

        // we can't use Type.GetCustomAttributes here because we could potentially be working with a reflection-only type.
        return CustomAttributeData.GetCustomAttributes(type).Any(
                attrib => attribType.IsAssignableFrom(attrib.AttributeType));
      }

      return TypeHasAttribute(type.GetTypeInfo(), attribType);
    }

    internal static bool TypeHasAttribute(TypeInfo typeInfo, Type attribType)
    {
      return typeInfo.GetCustomAttributes(attribType, true).Any();
    }
#endif

    #endregion

    #region -- GetSuitableClassName --

    /// <summary>Returns a sanitized version of <paramref name="type"/>s name which is suitable for use as a class name.</summary>
    /// <param name="type">The grain type.</param>
    /// <returns>A sanitized version of <paramref name="type"/>s name which is suitable for use as a class name.</returns>
    public static string GetSuitableClassName(Type type)
    {
      return GetClassNameFromInterfaceName(type.GetUnadornedTypeName());
    }

    #endregion

    #region -- GetClassNameFromInterfaceName --

    /// <summary>Returns a class-like version of <paramref name="interfaceName"/>.</summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns>A class-like version of <paramref name="interfaceName"/>.</returns>
    public static string GetClassNameFromInterfaceName(string interfaceName)
    {
      string cleanName;
      if (interfaceName.StartsWith("i", StringComparison.OrdinalIgnoreCase))
      {
        cleanName = interfaceName.Substring(1);
      }
      else
      {
        cleanName = interfaceName;
      }

      return cleanName;
    }

    #endregion

    #region -- GetUnadornedTypeName --

    /// <summary>Returns the non-generic type name without any special characters.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The non-generic type name without any special characters.</returns>
    public static string GetUnadornedTypeName(this Type type)
    {
      var index = type.Name.IndexOf('`');

      // An ampersand can appear as a suffix to a by-ref type.
      return (index > 0 ? type.Name.Substring(0, index) : type.Name).TrimEnd('&');
    }

    #endregion

    #region -- GetUnadornedMethodName --

    /// <summary>Returns the non-generic method name without any special characters.</summary>
    /// <param name="method">The method.</param>
    /// <returns>The non-generic method name without any special characters.</returns>
    public static string GetUnadornedMethodName(this MethodInfo method)
    {
      var index = method.Name.IndexOf('`');

      return index > 0 ? method.Name.Substring(0, index) : method.Name;
    }

    #endregion

    #region -- GetParseableName --

#if !NET40
    /// <summary>Returns a string representation of <paramref name="type"/>.</summary>
    /// <param name="type">The type.</param>
    /// <param name="options">The type formatting options.</param>
    /// <param name="getNameFunc">The delegate used to get the unadorned, simple type name of <paramref name="type"/>.</param>
    /// <returns>A string representation of the <paramref name="type"/>.</returns>
    public static string GetParseableName(this Type type, TypeFormattingOptions options = null, Func<Type, string> getNameFunc = null)
    {
      options = options ?? TypeFormattingOptions.Default;

      // If a naming function has been specified, skip the cache.
      if (getNameFunc != null) return BuildParseableName();

      return ParseableNameCache.GetOrAdd(Tuple.Create(type, options), _ => BuildParseableName());

      string BuildParseableName()
      {
        var builder = new StringBuilder();
        var typeInfo = type.GetTypeInfo();
        GetParseableName(
            type,
            builder,
            new Queue<Type>(
                typeInfo.IsGenericTypeDefinition
                    ? typeInfo.GetGenericArguments()
                    : typeInfo.GenericTypeArguments),
            options,
            getNameFunc ?? (t => t.GetUnadornedTypeName() + options.NameSuffix));
        return builder.ToString();
      }
    }

    /// <summary>Returns a string representation of <paramref name="type"/>.</summary>
    /// <param name="type">The type.</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to append results to.</param>
    /// <param name="typeArguments">The type arguments of <paramref name="type"/>.</param>
    /// <param name="options">The type formatting options.</param>
    /// <param name="getNameFunc"> </param>
    private static void GetParseableName(Type type, StringBuilder builder, Queue<Type> typeArguments, TypeFormattingOptions options, Func<Type, string> getNameFunc)
    {
      var typeInfo = type.GetTypeInfo();
      if (typeInfo.IsArray)
      {
        var elementType = typeInfo.GetElementType().GetParseableName(options);
        if (!string.IsNullOrWhiteSpace(elementType))
        {
          builder.AppendFormat(
              "{0}[{1}]",
              elementType,
              string.Concat(Enumerable.Range(0, type.GetArrayRank() - 1).Select(_ => ',')));
        }

        return;
      }

      if (typeInfo.IsGenericParameter)
      {
        if (options.IncludeGenericTypeParameters)
        {
          builder.Append(type.GetUnadornedTypeName());
        }

        return;
      }

      if (typeInfo.DeclaringType != null)
      {
        // This is not the root type.
        GetParseableName(typeInfo.DeclaringType, builder, typeArguments, options, t => t.GetUnadornedTypeName());
        builder.Append(options.NestedTypeSeparator);
      }
      else if (!string.IsNullOrWhiteSpace(type.Namespace) && options.IncludeNamespace)
      {
        // This is the root type, so include the namespace.
        var namespaceName = type.Namespace;
        if (options.NestedTypeSeparator != '.')
        {
          namespaceName = namespaceName.Replace('.', options.NestedTypeSeparator);
        }

        if (options.IncludeGlobal)
        {
          builder.AppendFormat("global::");
        }

        builder.AppendFormat("{0}{1}", namespaceName, options.NestedTypeSeparator);
      }

      if (type.IsConstructedGenericType)
      {
        // Get the unadorned name, the generic parameters, and add them together.
        var unadornedTypeName = getNameFunc(type);
        builder.Append(EscapeIdentifier(unadornedTypeName));
        var generics =
            Enumerable.Range(0, Math.Min(typeInfo.GetGenericArguments().Count(), typeArguments.Count))
                .Select(_ => typeArguments.Dequeue())
                .ToList();
        if (generics.Count > 0 && options.IncludeTypeParameters)
        {
          var genericParameters = string.Join(
              ",",
              generics.Select(generic => GetParseableName(generic, options)));
          builder.AppendFormat("<{0}>", genericParameters);
        }
      }
      else if (typeInfo.IsGenericTypeDefinition)
      {
        // Get the unadorned name, the generic parameters, and add them together.
        var unadornedTypeName = getNameFunc(type);
        builder.Append(EscapeIdentifier(unadornedTypeName));
        var generics =
            Enumerable.Range(0, Math.Min(type.GetGenericArguments().Count(), typeArguments.Count))
                .Select(_ => typeArguments.Dequeue())
                .ToList();
        if (generics.Count > 0 && options.IncludeTypeParameters)
        {
          var genericParameters = string.Join(
              ",",
              generics.Select(_ => options.IncludeGenericTypeParameters ? _.ToString() : string.Empty));
          builder.AppendFormat("<{0}>", genericParameters);
        }
      }
      else
      {
        builder.Append(EscapeIdentifier(getNameFunc(type)));
      }
    }
#endif

    #endregion

    #region -- GetNamespaces --

    /// <summary>Returns the namespaces of the specified types.</summary>
    /// <param name="types">The types to include.</param>
    /// <returns>The namespaces of the specified types.</returns>
    public static IEnumerable<string> GetNamespaces(params Type[] types)
    {
      return types.Select(type => "global::" + type.Namespace).Distinct();
    }

    #endregion

    #region -- Method --

    /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="T">The containing type of the method.</typeparam>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
    public static MethodInfo Method<T, TResult>(Expression<Func<T, TResult>> expression)
    {
      if (expression.Body is MethodCallExpression methodCall)
      {
        return methodCall.Method;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="T">The containing type of the method.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
    public static MethodInfo Method<T>(Expression<Func<T>> expression)
    {
      if (expression.Body is MethodCallExpression methodCall)
      {
        return methodCall.Method;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="T">The containing type of the method.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
    public static MethodInfo Method<T>(Expression<Action<T>> expression)
    {
      if (expression.Body is MethodCallExpression methodCall)
      {
        return methodCall.Method;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
    public static MethodInfo Method(Expression<Action> expression)
    {
      if (expression.Body is MethodCallExpression methodCall)
      {
        return methodCall.Method;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    #endregion

    #region -- Property --

    /// <summary>Returns the <see cref="PropertyInfo"/> for the simple member access in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="T">The containing type of the property.</typeparam>
    /// <typeparam name="TResult">The return type of the property.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="PropertyInfo"/> for the simple member access call in the provided <paramref name="expression"/>.</returns>
    public static PropertyInfo Property<T, TResult>(Expression<Func<T, TResult>> expression)
    {
      if (expression.Body is MemberExpression property)
      {
        return property.Member as PropertyInfo;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    /// <summary>Returns the <see cref="PropertyInfo"/> for the simple member access in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="TResult">The return type of the property.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="PropertyInfo"/> for the simple member access call in the provided <paramref name="expression"/>.</returns>
    public static PropertyInfo Property<TResult>(Expression<Func<TResult>> expression)
    {
      if (expression.Body is MemberExpression property)
      {
        return property.Member as PropertyInfo;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    #endregion

    #region -- Member --

    /// <summary>Returns the <see cref="MemberInfo"/> for the simple member access in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="T">The containing type of the method.</typeparam>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="MemberInfo"/> for the simple member access call in the provided <paramref name="expression"/>.</returns>
    public static MemberInfo Member<T, TResult>(Expression<Func<T, TResult>> expression)
    {
      if (expression.Body is MethodCallExpression methodCall)
      {
        return methodCall.Method;
      }

      if (expression.Body is MemberExpression property)
      {
        return property.Member;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    /// <summary>Returns the <see cref="MemberInfo"/> for the simple member access in the provided <paramref name="expression"/>.</summary>
    /// <typeparam name="TResult">The return type of the method.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The <see cref="MemberInfo"/> for the simple member access call in the provided <paramref name="expression"/>.</returns>
    public static MemberInfo Member<TResult>(Expression<Func<TResult>> expression)
    {
      if (expression.Body is MethodCallExpression methodCall)
      {
        return methodCall.Method;
      }

      if (expression.Body is MemberExpression property)
      {
        return property.Member;
      }

      throw new ArgumentException("Expression type unsupported.");
    }

    #endregion

    #region -- GetNamespaceOrEmpty --

    /// <summary>Returns the namespace of the provided type, or <see cref="string.Empty"/> if the type has no namespace.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The namespace of the provided type, or <see cref="string.Empty"/> if the type has no namespace.</returns>
    public static string GetNamespaceOrEmpty(this Type type)
    {
      if (type == null || string.IsNullOrEmpty(type.Namespace))
      {
        return string.Empty;
      }

      return type.Namespace;
    }

    #endregion

    #region -- GetConstructorThatMatches --

    /// <summary>Get a public or non-public constructor that matches the constructor arguments signature</summary>
    /// <param name="type">The type to use.</param>
    /// <param name="constructorArguments">The constructor argument types to match for the signature.</param>
    /// <returns>A constructor that matches the signature or <see langword="null"/>.</returns>
    public static ConstructorInfo GetConstructorThatMatches(Type type, Type[] constructorArguments)
    {
      var constructorInfo = type.GetConstructor(
          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
          null,
          constructorArguments,
          null);
      return constructorInfo;
    }

    #endregion

    #region -- GetTypes --

    /// <summary>Returns the types referenced by the provided <paramref name="type"/>.</summary>
    /// <param name="type">The type.</param>
    /// <param name="includeMethods">Whether or not to include the types referenced in the methods of this type.</param>
    /// <returns>The types referenced by the provided <paramref name="type"/>.</returns>
    public static IList<Type> GetTypes(this Type type, bool includeMethods = false)
    {
      var key = Tuple.Create(type, includeMethods);
      if (!ReferencedTypes.TryGetValue(key, out List<Type> results))
      {
        results = GetTypes(type, includeMethods, null).ToList();
        ReferencedTypes.TryAdd(key, results);
      }

      return results;
    }

    /// <summary>Returns the types referenced by the provided <paramref name="type"/>.</summary>
    /// <param name="type">The type.</param>
    /// <param name="includeMethods">Whether or not to include the types referenced in the methods of this type.</param>
    /// <param name="exclude">Types to exclude</param>
    /// <returns>The types referenced by the provided <paramref name="type"/>.</returns>
    public static IEnumerable<Type> GetTypes(this Type type, bool includeMethods, HashSet<Type> exclude)
    {
      exclude = exclude ?? new HashSet<Type>();
      if (!exclude.Add(type))
      {
        yield break;
      }

      yield return type;

      if (type.IsArray)
      {
        foreach (var elementType in type.GetElementType().GetTypes(false, exclude: exclude))
        {
          yield return elementType;
        }
      }

#if NET40
      if (type.IsConstructedGenericType())
#else
      if (type.IsConstructedGenericType)
#endif
      {
        foreach (var genericTypeArgument in
            type.GetGenericArguments().SelectMany(_ => GetTypes(_, false, exclude: exclude)))
        {
          yield return genericTypeArgument;
        }
      }

      if (!includeMethods)
      {
        yield break;
      }

      foreach (var method in type.GetMethods())
      {
        foreach (var referencedType in GetTypes(method.ReturnType, false, exclude: exclude))
        {
          yield return referencedType;
        }

        foreach (var parameter in method.GetParameters())
        {
          foreach (var referencedType in GetTypes(parameter.ParameterType, false, exclude: exclude))
          {
            yield return referencedType;
          }
        }
      }
    }

    #endregion

    #region -- EscapeIdentifier --

    public static string EscapeIdentifier(string identifier)
    {
      if (IsCSharpKeyword(identifier)) return "@" + identifier;
      return identifier;
    }

    #endregion

    #region -- IsCSharpKeyword --

    public static bool IsCSharpKeyword(string identifier)
    {
      switch (identifier)
      {
        case "abstract":
        case "add":
        case "alias":
        case "as":
        case "ascending":
        case "async":
        case "await":
        case "base":
        case "bool":
        case "break":
        case "byte":
        case "case":
        case "catch":
        case "char":
        case "checked":
        case "class":
        case "const":
        case "continue":
        case "decimal":
        case "default":
        case "delegate":
        case "descending":
        case "do":
        case "double":
        case "dynamic":
        case "else":
        case "enum":
        case "event":
        case "explicit":
        case "extern":
        case "false":
        case "finally":
        case "fixed":
        case "float":
        case "for":
        case "foreach":
        case "from":
        case "get":
        case "global":
        case "goto":
        case "group":
        case "if":
        case "implicit":
        case "in":
        case "int":
        case "interface":
        case "internal":
        case "into":
        case "is":
        case "join":
        case "let":
        case "lock":
        case "long":
        case "nameof":
        case "namespace":
        case "new":
        case "null":
        case "object":
        case "operator":
        case "orderby":
        case "out":
        case "override":
        case "params":
        case "partial":
        case "private":
        case "protected":
        case "public":
        case "readonly":
        case "ref":
        case "remove":
        case "return":
        case "sbyte":
        case "sealed":
        case "select":
        case "set":
        case "short":
        case "sizeof":
        case "stackalloc":
        case "static":
        case "string":
        case "struct":
        case "switch":
        case "this":
        case "throw":
        case "true":
        case "try":
        case "typeof":
        case "uint":
        case "ulong":
        case "unchecked":
        case "unsafe":
        case "ushort":
        case "using":
        case "value":
        case "var":
        case "virtual":
        case "void":
        case "volatile":
        case "when":
        case "where":
        case "while":
        case "yield":
          return true;
        default:
          return false;
      }
    }

    #endregion

    #region -- Equal --

    /// <summary>判断两个类型是否相同，避免引用加载和执行上下文加载的相同类型显示不同</summary>
    /// <param name="type1"></param>
    /// <param name="type2"></param>
    /// <returns></returns>
    public static Boolean Equal(Type type1, Type type2)
    {
      if (type1 == type2) return true;

      return string.Equals(type1.FullName, type2.FullName, StringComparison.Ordinal) &&
             string.Equals(type1.AssemblyQualifiedName, type2.AssemblyQualifiedName, StringComparison.Ordinal);
    }

    #endregion

    #region -- ChangeType --

    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    public static Object ChangeType(Object value, Type conversionType)
    {
      Type vtype = null;
      if (value != null) { vtype = value.GetType(); }

      //if (vtype == conversionType || conversionType.IsAssignableFrom(vtype)) return value;
      if (vtype == conversionType) return value;

      // 处理可空类型
      if (!conversionType.IsValueType && conversionType.IsNullableType())
      {
        if (value == null) return null;

        conversionType = Nullable.GetUnderlyingType(conversionType);
      }

      if (conversionType.IsEnum)
      {
        if (vtype == _.String)
          return Enum.Parse(conversionType, (String)value, true);
        else
          return Enum.ToObject(conversionType, value);
      }

      // 字符串转为货币类型，处理一下
      if (vtype == _.String)
      {
        var str = (String)value;
        if (Type.GetTypeCode(conversionType) == TypeCode.Decimal)
        {
          value = str.TrimStart(new Char[] { '$', '￥' });
        }
        else if (typeof(Type).IsAssignableFrom(conversionType))
        {
          return ResolveType((String)value);
        }

        // 字符串转为简单整型，如果长度比较小，满足32位整型要求，则先转为32位再改变类型
        if (conversionType.IsIntegerType() && str.Length <= 10) return Convert.ChangeType(value.ToInt(), conversionType);
      }

      if (vtype == _.Guid)
      {
        return value.ToGuid();
      }
      else if (vtype == _.CombGuid)
      {
        CombGuid comb;
        if (CombGuid.TryParse(value, CombGuidSequentialSegmentType.Comb, out comb)) { return comb; }
        if (CombGuid.TryParse(value, CombGuidSequentialSegmentType.Guid, out comb)) { return comb; }
        return CombGuid.Null;
      }

      if (value != null)
      {
        // 尝试基础类型转换
        switch (Type.GetTypeCode(conversionType))
        {
          case TypeCode.Boolean:
            return value.ToBoolean();
          case TypeCode.DateTime:
            return value.ToDateTime();
          case TypeCode.Double:
            return value.ToDouble();
          case TypeCode.Int16:
            return value.ToInt16();
          case TypeCode.Int32:
            return value.ToInt();
          case TypeCode.UInt16:
            return (UInt16)value.ToInt();
          case TypeCode.UInt32:
            return (UInt32)value.ToInt64();
          default:
            break;
        }

        if (value is IConvertible)
        {
          // 上海石头 发现这里导致Json序列化问题
          // http://www.newlifex.com/showtopic-282.aspx
          if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
          {
            var nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
            conversionType = nullableConverter.UnderlyingType;
          }
          value = Convert.ChangeType(value, conversionType);
        }

        //else if (conversionType.IsInterface)
        //    value = DuckTyping.Implement(value, conversionType);
      }
      else
      {
        // 如果原始值是null，要转为值类型，则new一个空白的返回
        if (conversionType.IsValueType) value = conversionType.CreateInstance();
      }

      if (conversionType.IsAssignableFrom(vtype)) return value;
      return value;
    }

    /// <summary>类型转换</summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static TResult ChangeType<TResult>(Object value)
    {
      if (value is TResult) return (TResult)value;

      return (TResult)ChangeType(value, typeof(TResult));
    }

    #endregion

    #region -- IsNumericType --

    public static bool IsNumericType(this Type type)
    {
      if (type == null) return false;

      if (type.IsEnum) //TypeCode can be TypeCode.Int32
      {
        //return JsConfig.TreatEnumAsInteger || type.IsEnumFlags();
        return type.IsEnumFlags();
      }

      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Byte:
        case TypeCode.Decimal:
        case TypeCode.Double:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.SByte:
        case TypeCode.Single:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return true;

        case TypeCode.Object:
          if (type.IsNullableType())
          {
            return IsNumericType(Nullable.GetUnderlyingType(type));
          }
          if (type.IsEnum)
          {
            //return JsConfig.TreatEnumAsInteger || type.IsEnumFlags();
            return type.IsEnumFlags();
          }
          return false;
      }
      return false;
    }

    #endregion

    #region -- IsIntegerType --

    public static bool IsIntegerType(this Type type)
    {
      if (type == null) return false;

      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.SByte:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return true;

        case TypeCode.Object:
          if (type.IsNullableType())
          {
            return IsNumericType(Nullable.GetUnderlyingType(type));
          }
          return false;
      }
      return false;
    }

    #endregion

    #region -- IsRealNumberType --

    public static bool IsRealNumberType(this Type type)
    {
      if (type == null) return false;

      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Decimal:
        case TypeCode.Double:
        case TypeCode.Single:
          return true;

        case TypeCode.Object:
          if (type.IsNullableType())
          {
            return IsNumericType(Nullable.GetUnderlyingType(type));
          }
          return false;
      }
      return false;
    }

    #endregion

    #region -- IsEnumFlags --

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsEnumFlags(this Type type) => type.IsEnum && type.FirstAttribute<FlagsAttribute>() != null;

    #endregion

    #region -- ResolveType / TryResolveType --

    private static readonly DictionaryCache<string, Type> _resolveTypeCache =
        new DictionaryCache<string, Type>(DictionaryCacheConstants.SIZE_MEDIUM, StringComparer.Ordinal);
    private static readonly List<Func<string, Type>> _resolvers = new List<Func<string, Type>>();
    private static readonly ReaderWriterLockSlim _resolverLock = new ReaderWriterLockSlim();
    private static readonly CachedReadConcurrentDictionary<string, Assembly> _assemblyCache =
        new CachedReadConcurrentDictionary<string, Assembly>(StringComparer.Ordinal);
    private static readonly DictionaryCache<TypeNameKey, Type> _typeNameKeyCache =
        new DictionaryCache<TypeNameKey, Type>(DictionaryCacheConstants.SIZE_MEDIUM, TypeNameKeyComparer.Default);

    /// <summary>Registers a custom type resolver in case you really need to manipulate the way serialization works with types.
    /// The <paramref name="resolve"/> func is allowed to return null in case you cannot resolve the requested type.
    /// Any exception the <paramref name="resolve"/> func might throw will not bubble up.</summary>
    /// <param name="resolve">The resolver</param>
    public static void RegisterResolveType(Func<string, Type> resolve)
    {
      using (var token = _resolverLock.CreateToken())
      {
        _resolvers.Insert(0, resolve);
      }
    }

    public static Type ResolveType(string qualifiedTypeName)
    {
      if (TryResolveType(qualifiedTypeName, out var result)) { return result; }

      throw new TypeAccessException($"Unable to find a type named {qualifiedTypeName}");
    }

    /// <summary>Gets <see cref="Type"/> by full name (with falling back to the first part only).</summary>
    /// <param name="qualifiedTypeName">The type name.</param>
    /// <param name="type"></param>
    /// <returns>The <see cref="Type"/> if valid.</returns>
    public static bool TryResolveType(string qualifiedTypeName, out Type type)
    {
      if (string.IsNullOrWhiteSpace(qualifiedTypeName))
      {
        throw new ArgumentException("A type name must not be null nor consist of only whitespace.", nameof(qualifiedTypeName));
      }

      if (_resolveTypeCache.TryGetValue(qualifiedTypeName, out type)) { return true; }

      string serializedTypeName = null;
      if (qualifiedTypeName.IndexOf(c_keyDelimiter) > 0)
      {
        serializedTypeName = qualifiedTypeName;
        qualifiedTypeName = qualifiedTypeName.Replace(":", ", ");

        if (_resolveTypeCache.TryGetValue(qualifiedTypeName, out type))
        {
          if (serializedTypeName != null) { _resolveTypeCache[serializedTypeName] = type; }
          return true;
        }
      }

      using (var token = _resolverLock.CreateToken(true))
      {
        foreach (var resolver in _resolvers)
        {
          try
          {
            type = resolver(qualifiedTypeName);
            if (type != null) { break; }
          }
          catch { }
        }
      }

      if (null == type)
      {
        var typeNameKey = SplitFullyQualifiedTypeName(qualifiedTypeName);
        if (!TryResolveType(typeNameKey, out type)) { return false; }
      }

      AddTypeToCache(qualifiedTypeName, type);
      if (serializedTypeName != null) { AddTypeToCache(serializedTypeName, type); }
      return true;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static void AddTypeToCache(string typeName, Type type)
    {
      var entry = _resolveTypeCache.GetItem(typeName, _ => type);
      if (!ReferenceEquals(entry, type)) { throw new InvalidOperationException("inconsistent type name association"); }
    }

    internal static bool TryPerformUncachedTypeResolution(string fullName, out Type type)
    {
      if (string.IsNullOrWhiteSpace(fullName))
      {
        throw new ArgumentException("A type name must not be null nor consist of only whitespace.", nameof(fullName));
      }

      var typeNameKey = SplitFullyQualifiedTypeName(fullName);
      return TryPerformUncachedTypeResolution(typeNameKey, out type);
    }

    internal static Type ResolveType(TypeNameKey typeNameKey)
    {
      if (TryResolveType(typeNameKey, out var result)) { return result; }

      throw new TypeAccessException($"Unable to find a type named {typeNameKey.TypeName}[{typeNameKey.AssemblyName}]");
    }

    /// <inheritdoc />
    internal static bool TryResolveType(TypeNameKey typeNameKey, out Type type)
    {
      if (_typeNameKeyCache.TryGetValue(typeNameKey, out type)) { return true; }

      if (!TryPerformUncachedTypeResolution(typeNameKey, out type)) { return false; }

      AddTypeToCache(typeNameKey, type);
      return true;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static void AddTypeToCache(TypeNameKey typeNameKey, Type type)
    {
      var entry = _typeNameKeyCache.GetItem(typeNameKey, _ => type);
      if (!ReferenceEquals(entry, type)) { throw new InvalidOperationException("inconsistent type name association"); }
    }

    internal static bool TryPerformUncachedTypeResolution(TypeNameKey typeNameKey, out Type type)
    {
      string assemblyName = typeNameKey.AssemblyName;
      string typeName = typeNameKey.TypeName;

      if (assemblyName != null)
      {
        Assembly assembly = null;

        //assembly = Assembly.Load(assemblyName);
        try
        {
          // look, I don't like using obsolete methods as much as you do but this is the only way
          // Assembly.Load won't check the GAC for a partial name
#pragma warning disable 618, 612
          assembly = Assembly.LoadWithPartialName(assemblyName);
#pragma warning restore 618, 612
        }
        catch { }

        if (assembly == null)
        {
          // will find assemblies loaded with Assembly.LoadFile outside of the main directory
          Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
          foreach (Assembly asm in loadedAssemblies)
          {
            // check for both full name or partial name match
            if (string.Equals(asm.FullName, assemblyName, StringComparison.Ordinal) ||
                string.Equals(asm.GetName().Name, assemblyName, StringComparison.Ordinal))
            {
              assembly = asm;
              break;
            }
          }
        }
        if (assembly == null)
        {
          var dirEnumArgs = HostBuilderContext.Singleton.GetDictionaryEnumArgs();
          foreach (var asm in AssemblyLoader.LoadAssemblies(dirEnumArgs))
          {
            if (string.Equals(asm.FullName, assemblyName, StringComparison.Ordinal) ||
                string.Equals(asm.GetName().Name, assemblyName, StringComparison.Ordinal))
            {
              assembly = asm;
              break;
            }
          }
        }

        if (assembly == null)
        {
          type = null;
          return false;
        }

        type = assembly.GetType(typeName, false);
        if (type == null)
        {
          //// if generic type, try manually parsing the type arguments for the case of dynamically loaded assemblies
          //// example generic typeName format: System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
          //if (typeName.IndexOf('`') >= 0)
          //{
          //  try
          //  {
          //    type = GetGenericTypeFromTypeName(typeName, assembly);
          //  }
          //  catch //(Exception ex)
          //  {
          //    return false;
          //    //throw new SerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(typeName, assembly.FullName), ex);
          //  }
          //}

          // 这儿如果采用 Newtonsoft.Json 的代码，是无法通过 RuntimeTypeNameFormatterTests 测试的

          type = Type.GetType(typeName, throwOnError: false)
              ?? Type.GetType(typeName, ResolveAssembly, ResolveType, false);
        }
      }
      else
      {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
          type = assembly.GetType(typeName, false);
          if (type != null) { return true; }
        }

        type = Type.GetType(typeName, throwOnError: false)
            ?? Type.GetType(typeName, ResolveAssembly, ResolveType, false);
      }

      if (null == type) { return false; }
      if (type.Assembly.ReflectionOnly)
      {
        throw new InvalidOperationException($"Type resolution for {typeName}[{assemblyName}] yielded reflection-only type.");
      }
      return true;

      Assembly ResolveAssembly(AssemblyName asmName)
      {
        var fullAssemblyName = asmName.FullName;
        if (_assemblyCache.TryGetValue(fullAssemblyName, out var result)) return result;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
          var name = assembly.GetName();
          if (!_assemblyCache.ContainsKey(name.FullName))
          {
            _assemblyCache[name.FullName] = assembly;
            _assemblyCache[name.Name] = assembly;
          }
        }
        if (_assemblyCache.TryGetValue(fullAssemblyName, out result)) return result;

        var dirEnumArgs = HostBuilderContext.Singleton.GetDictionaryEnumArgs();
        foreach (var assembly in AssemblyLoader.LoadAssemblies(dirEnumArgs))
        {
          var name = assembly.GetName();
          if (!_assemblyCache.ContainsKey(name.FullName))
          {
            _assemblyCache[name.FullName] = assembly;
            _assemblyCache[name.Name] = assembly;
          }
        }
        if (_assemblyCache.TryGetValue(fullAssemblyName, out result)) return result;

        result = Assembly.Load(asmName);
        var resultName = result.GetName();
        _assemblyCache[resultName.Name] = result;
        _assemblyCache[resultName.FullName] = result;
        return result;
      }

      Type ResolveType(Assembly asm, string name, bool ignoreCase)
      {
        return asm?.GetType(name, throwOnError: false, ignoreCase: ignoreCase) ?? Type.GetType(name, throwOnError: false, ignoreCase: ignoreCase);
      }
    }

    private static Type GetGenericTypeFromTypeName(string typeName, Assembly assembly)
    {
      Type type = null;
      int openBracketIndex = typeName.IndexOf('[');
      if (openBracketIndex >= 0)
      {
        string genericTypeDefName = typeName.Substring(0, openBracketIndex);
        Type genericTypeDef = assembly.GetType(genericTypeDefName);
        if (genericTypeDef != null)
        {
          List<Type> genericTypeArguments = new List<Type>();
          int scope = 0;
          int typeArgStartIndex = 0;
          int endIndex = typeName.Length - 1;
          for (int i = openBracketIndex + 1; i < endIndex; ++i)
          {
            char current = typeName[i];
            switch (current)
            {
              case '[':
                if (scope == 0)
                {
                  typeArgStartIndex = i + 1;
                }
                ++scope;
                break;
              case ']':
                --scope;
                if (scope == 0)
                {
                  string typeArgAssemblyQualifiedName = typeName.Substring(typeArgStartIndex, i - typeArgStartIndex);

                  TypeNameKey typeNameKey = SplitFullyQualifiedTypeName(typeArgAssemblyQualifiedName);
                  genericTypeArguments.Add(ResolveType(typeNameKey));
                }
                break;
            }
          }

          type = genericTypeDef.GetCachedGenericType(genericTypeArguments.ToArray());
        }
      }

      return type;
    }

    #endregion

    #region == SplitFullyQualifiedTypeName ==

    internal static TypeNameKey SplitFullyQualifiedTypeName(string fullyQualifiedTypeName)
    {
      int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

      //string typeName;
      string assemblyName = null;

      //if (assemblyDelimiterIndex != null)
      //{
      //  typeName = fullyQualifiedTypeName.Trim(0, assemblyDelimiterIndex.GetValueOrDefault());
      //  assemblyName = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
      //}
      //else
      //{
      //  typeName = fullyQualifiedTypeName;
      //}

      //return new TypeNameKey(assemblyName, typeName);
      if (assemblyDelimiterIndex != null)
      {
        assemblyName = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
      }
      return new TypeNameKey(assemblyName, fullyQualifiedTypeName);
    }

    private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
    {
      // we need to get the first comma following all surrounded in brackets because of generic types
      // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      int scope = 0;
      for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
      {
        char current = fullyQualifiedTypeName[i];
        switch (current)
        {
          case '[':
            scope++;
            break;
          case ']':
            scope--;
            break;
          case ',':
            if (scope == 0)
            {
              return i;
            }
            break;
        }
      }

      return null;
    }

    #endregion
  }
}
