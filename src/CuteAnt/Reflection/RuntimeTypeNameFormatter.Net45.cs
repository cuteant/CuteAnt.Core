﻿#if !NET40
using System;
using System.Reflection;
using System.Text;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
  /// <summary>Utility methods for formatting <see cref="Type"/> and <see cref="TypeInfo"/> instances in a way which can be parsed by
  /// <see cref="Type.GetType(string)"/> or <see cref="TypeUtils.ResolveType(string)"/>.</summary>
  public static partial class RuntimeTypeNameFormatter
  {
    #region -- Format --

    private static readonly CachedReadConcurrentDictionary<TypeInfo, string> Cache =
        new CachedReadConcurrentDictionary<TypeInfo, string>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<TypeInfo, string> s_formatFunc = FormatInternal;

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

      return Cache.GetOrAdd(type, s_formatFunc);
    }

    private static string FormatInternal(TypeInfo type)
    {
      var builder = StringBuilderCache.Acquire();
      Format(builder, type.GetTypeInfo(), isElementType: false);
      return StringBuilderCache.GetStringAndRelease(builder);
    }

    #endregion

    #region -- Serialize --

    private static readonly CachedReadConcurrentDictionary<TypeInfo, string> _typeNameSerializerCache =
        new CachedReadConcurrentDictionary<TypeInfo, string>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<TypeInfo, string> _serializeTypeNameFunc = SerializeInternal;

    /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="TypeUtils.ResolveType(string)"/>.</summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="TypeUtils.ResolveType(string)"/>.</returns>
    public static string Serialize(Type type) => Serialize(type?.GetTypeInfo());

    /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="TypeUtils.ResolveType(string)"/>.</summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="TypeUtils.ResolveType(string)"/>.</returns>
    public static string Serialize(TypeInfo type)
    {
      if (null == type) { throw new ArgumentNullException(nameof(type)); }

      return _typeNameSerializerCache.GetOrAdd(type, _serializeTypeNameFunc);
    }

    private static string SerializeInternal(TypeInfo type)
    {
      var typeName = Format(type);
      return typeName.Replace(", ", ":");
    }

    #endregion

    #region ** Private Methods **

    private static void Format(StringBuilder builder, TypeInfo type, bool isElementType)
    {
      // Arrays, pointers, and by-ref types are all element types and need to be formatted with their own adornments.
      if (type.HasElementType)
      {
        // Format the element type.
        Format(builder, type.GetElementType().GetTypeInfo(), isElementType: true);

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

    private static void AddNamespace(StringBuilder builder, TypeInfo type)
    {
      if (string.IsNullOrWhiteSpace(type.Namespace)) { return; }
      builder.Append(type.Namespace);
      builder.Append('.');
    }

    private static void AddClassName(StringBuilder builder, TypeInfo type)
    {
      // Format the declaring type.
      if (type.IsNested)
      {
        AddClassName(builder, type.DeclaringType.GetTypeInfo());
        builder.Append('+');
      }

      // Format the simple type name.
      var index = type.Name.IndexOfAny(SimpleNameTerminators);
      builder.Append(index > 0 ? type.Name.Substring(0, index) : type.Name);

      // Format this type's generic arity.
      AddGenericArity(builder, type);
    }

    private static void AddGenericParameters(StringBuilder builder, TypeInfo type)
    {
      // Generic type definitions (eg, List<> without parameters) and non-generic types do not include any
      // parameters in their formatting.
      if (!type.AsType().IsConstructedGenericType) { return; }

      var args = type.GetGenericArguments();
      builder.Append('[');
      for (var i = 0; i < args.Length; i++)
      {
        builder.Append('[');
        Format(builder, args[i].GetTypeInfo(), isElementType: false);
        builder.Append(']');
        if (i + 1 < args.Length) builder.Append(',');
      }

      builder.Append(']');
    }

    private static void AddGenericArity(StringBuilder builder, TypeInfo type)
    {
      if (!type.IsGenericType) { return; }

      // The arity is the number of generic parameters minus the number of generic parameters in the declaring types.
      var baseTypeParameterCount =
          type.IsNested ? type.DeclaringType.GetTypeInfo().GetGenericArguments().Length : 0;
      var arity = type.GetGenericArguments().Length - baseTypeParameterCount;

      // If all of the generic parameters are in the declaring types then this type has no parameters of its own.
      if (arity == 0) { return; }

      builder.Append('`');
      builder.Append(arity);
    }

    private static void AddPointerSymbol(StringBuilder builder, TypeInfo type)
    {
      if (!type.IsPointer) { return; }
      builder.Append('*');
    }

    private static void AddByRefSymbol(StringBuilder builder, TypeInfo type)
    {
      if (!type.IsByRef) { return; }
      builder.Append('&');
    }

    private static void AddArrayRank(StringBuilder builder, TypeInfo type)
    {
      if (!type.IsArray) { return; }
      builder.Append('[');
      builder.Append(',', type.GetArrayRank() - 1);
      builder.Append(']');
    }

    private static void AddAssembly(StringBuilder builder, TypeInfo type)
    {
      // Do not include the assembly name for the system assembly.
      if (SystemAssembly.Equals(type.Assembly)) { return; }
      builder.Append(", ");
      builder.Append(type.Assembly.GetName().Name);
    }

    #endregion
  }
}
#endif
