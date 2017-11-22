using System;
using System.Reflection;
using System.Text;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
  /// <summary>Utility methods for formatting <see cref="Type"/> and {System.Reflection.TypeInfo} instances in a way which can be parsed by
  /// <see cref="Type.GetType(string)"/>.</summary>
  public static class RuntimeTypeNameFormatter
  {
    public static readonly Assembly SystemAssembly = typeof(int)
#if !NET40
        .GetTypeInfo()
#endif
        .Assembly;
    private static readonly char[] SimpleNameTerminators = { '`', '*', '[', '&' };

#if NET40
    private static readonly CachedReadConcurrentDictionary<Type, string> Cache = new CachedReadConcurrentDictionary<Type, string>();
#else
    private static readonly CachedReadConcurrentDictionary<TypeInfo, string> Cache = new CachedReadConcurrentDictionary<TypeInfo, string>();
#endif

#if NET40
    /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</returns>
    public static string Format(Type type)
    {
      if (type == null) throw new ArgumentNullException(nameof(type));

      if (!Cache.TryGetValue(type, out var result))
      {
        string FormatType(Type t)
        {
          var builder = StringBuilderCache.Acquire();
          Format(builder, t, isElementType: false);
          return StringBuilderCache.GetStringAndRelease(builder);
        }

        result = Cache.GetOrAdd(type, FormatType);
      }

      return result;
    }
#else
    /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</returns>
    public static string Format(Type type) => Format(type?.GetTypeInfo());

    /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</returns>
    public static string Format(TypeInfo type)
    {
      if (type == null) throw new ArgumentNullException(nameof(type));

      if (!Cache.TryGetValue(type, out var result))
      {
        string FormatType(Type t)
        {
          var builder = StringBuilderCache.Acquire();
          Format(builder, t.GetTypeInfo(), isElementType: false);
          return StringBuilderCache.GetStringAndRelease(builder);
        }

        result = Cache.GetOrAdd(type, FormatType);
      }

      return result;
    }
#endif

#if NET40
    private static void Format(StringBuilder builder, Type type, bool isElementType)
#else
    private static void Format(StringBuilder builder, TypeInfo type, bool isElementType)
#endif
    {
      // Arrays, pointers, and by-ref types are all element types and need to be formatted with their own adornments.
      if (type.HasElementType)
      {
        // Format the element type.
#if NET40
        Format(builder, type.GetElementType(), isElementType: true);
#else
        Format(builder, type.GetElementType().GetTypeInfo(), isElementType: true);
#endif

        // Format this type's adornments to the element type.
        AddArrayRank(builder, type);
        AddPointerSymbol(builder, type);
        AddByRefSymbol(builder, type);
      }
      else
      {
        AddNamespace(builder, type);
        AddClassName(builder, type);
        AddGenericParameters(builder, type);
      }

      // Types which are used as elements are not formatted with their assembly name, since that is added after the
      // element type's adornments.
      if (!isElementType) AddAssembly(builder, type);
    }

#if NET40
    private static void AddNamespace(StringBuilder builder, Type type)
#else
    private static void AddNamespace(StringBuilder builder, TypeInfo type)
#endif
    {
      if (string.IsNullOrWhiteSpace(type.Namespace)) { return; }
      builder.Append(type.Namespace);
      builder.Append('.');
    }

#if NET40
    private static void AddClassName(StringBuilder builder, Type type)
#else
    private static void AddClassName(StringBuilder builder, TypeInfo type)
#endif
    {
      // Format the declaring type.
      if (type.IsNested)
      {
#if NET40
        AddClassName(builder, type.DeclaringType);
#else
        AddClassName(builder, type.DeclaringType.GetTypeInfo());
#endif
        builder.Append('+');
      }

      // Format the simple type name.
      var index = type.Name.IndexOfAny(SimpleNameTerminators);
      builder.Append(index > 0 ? type.Name.Substring(0, index) : type.Name);

      // Format this type's generic arity.
      AddGenericArity(builder, type);
    }

#if NET40
    private static void AddGenericParameters(StringBuilder builder, Type type)
#else
    private static void AddGenericParameters(StringBuilder builder, TypeInfo type)
#endif
    {
      // Generic type definitions (eg, List<> without parameters) and non-generic types do not include any
      // parameters in their formatting.
#if NET40
      if (!type.IsConstructedGenericType()) { return; }
#else
      if (!type.AsType().IsConstructedGenericType) { return; }
#endif

      var args = type.GetGenericArguments();
      builder.Append('[');
      for (var i = 0; i < args.Length; i++)
      {
        builder.Append('[');
#if NET40
        Format(builder, args[i], isElementType: false);
#else
        Format(builder, args[i].GetTypeInfo(), isElementType: false);
#endif
        builder.Append(']');
        if (i + 1 < args.Length) builder.Append(',');
      }

      builder.Append(']');
    }

#if NET40
    private static void AddGenericArity(StringBuilder builder, Type type)
#else
    private static void AddGenericArity(StringBuilder builder, TypeInfo type)
#endif
    {
      if (!type.IsGenericType) { return; }

      // The arity is the number of generic parameters minus the number of generic parameters in the declaring types.
      var baseTypeParameterCount =
          type.IsNested ? type.DeclaringType
#if !NET40
          .GetTypeInfo()
#endif
          .GetGenericArguments().Length : 0;
      var arity = type.GetGenericArguments().Length - baseTypeParameterCount;

      // If all of the generic parameters are in the declaring types then this type has no parameters of its own.
      if (arity == 0) { return; }

      builder.Append('`');
      builder.Append(arity);
    }

#if NET40
    private static void AddPointerSymbol(StringBuilder builder, Type type)
#else
    private static void AddPointerSymbol(StringBuilder builder, TypeInfo type)
#endif
    {
      if (!type.IsPointer) { return; }
      builder.Append('*');
    }

#if NET40
    private static void AddByRefSymbol(StringBuilder builder, Type type)
#else
    private static void AddByRefSymbol(StringBuilder builder, TypeInfo type)
#endif
    {
      if (!type.IsByRef) { return; }
      builder.Append('&');
    }

#if NET40
    private static void AddArrayRank(StringBuilder builder, Type type)
#else
    private static void AddArrayRank(StringBuilder builder, TypeInfo type)
#endif
    {
      if (!type.IsArray) { return; }
      builder.Append('[');
      builder.Append(',', type.GetArrayRank() - 1);
      builder.Append(']');
    }

#if NET40
    private static void AddAssembly(StringBuilder builder, Type type)
#else
    private static void AddAssembly(StringBuilder builder, TypeInfo type)
#endif
    {
      // Do not include the assembly name for the system assembly.
      if (SystemAssembly.Equals(type.Assembly)) { return; }
      builder.Append(", ");
      builder.Append(type.Assembly.GetName().Name);
    }
  }
}
