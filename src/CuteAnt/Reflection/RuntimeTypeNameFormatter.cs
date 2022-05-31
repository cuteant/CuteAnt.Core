using System;
using System.Reflection;
using System.Text;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
    /// <summary>Utility methods for formatting <see cref="Type"/> instances in a way which can be parsed by
    /// <see cref="Type.GetType(string)"/>.</summary>
    public static partial class RuntimeTypeNameFormatter
    {
        /// <summary>SystemAssembly</summary>
        public static readonly Assembly SystemAssembly = typeof(int).Assembly;

        private static readonly char[] SimpleNameTerminators = { '`', '*', '[', '&' };

        #region -- Format --

        private static readonly CachedReadConcurrentDictionary<Type, string> Cache =
            new CachedReadConcurrentDictionary<Type, string>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static readonly Func<Type, string> s_formatFunc = FormatInternal;

        /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</summary>
        /// <param name="type">The type to format.</param>
        /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="Type.GetType(string)"/>.</returns>
        public static string Format(Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            return Cache.GetOrAdd(type, s_formatFunc);
        }

        private static string FormatInternal(Type type)
        {
            var builder = StringBuilderCache.Acquire();
            Format(builder, type, isElementType: false);
            return StringBuilderCache.GetStringAndRelease(builder);
        }

        #endregion

        #region -- Serialize --

        private static readonly CachedReadConcurrentDictionary<Type, string> _typeNameSerializerCache =
            new CachedReadConcurrentDictionary<Type, string>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static readonly Func<Type, string> _serializeTypeNameFunc = SerializeInternal;

        /// <summary>Returns a <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="TypeUtils.ResolveType(string)"/>.</summary>
        /// <param name="type">The type to format.</param>
        /// <returns>A <see cref="string"/> form of <paramref name="type"/> which can be parsed by <see cref="TypeUtils.ResolveType(string)"/>.</returns>
        public static string Serialize(Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            return _typeNameSerializerCache.GetOrAdd(type, _serializeTypeNameFunc);
        }

        private static string SerializeInternal(Type type)
        {
            var typeName = Format(type);
            return typeName.Replace(", ", ":");
        }

        #endregion

        #region ==& GetTypeDefinition &==

        private static readonly CachedReadConcurrentDictionary<Type, QualifiedType> s_asmTypeDefinitionCache =
            new CachedReadConcurrentDictionary<Type, QualifiedType>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static readonly Func<Type, QualifiedType> s_getTypeDefinitionFunc = GetTypeDefinitionInternal;

        internal static QualifiedType GetTypeDefinition(Type type)
            => s_asmTypeDefinitionCache.GetOrAdd(type, s_getTypeDefinitionFunc);

        private static QualifiedType GetTypeDefinitionInternal(Type type)
        {
            var builder = StringBuilderCache.Acquire();
            Format(builder, type, isElementType: true);
            var typeName = StringBuilderCache.GetStringAndRelease(builder);

            return new QualifiedType(GetAssemblyName(type), typeName);
        }

        #endregion

        #region --& GetAssemblyName &--

        /// <summary>GetAssemblyName</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetAssemblyName(Type type)
        {
            var asm = type.Assembly;

            // Do not include the assembly name for the system assembly.
            if (SystemAssembly.Equals(asm)) { return null; }
            return asm.GetName().Name;
        }

        #endregion

        #region ** Private Methods **

        private static void Format(StringBuilder builder, Type type, bool isElementType)
        {
            // Arrays, pointers, and by-ref types are all element types and need to be formatted with their own adornments.
            if (type.HasElementType)
            {
                // Format the element type.
                Format(builder, type.GetElementType(), isElementType: true);

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
            if (!isElementType)
            {
                AddAssembly(builder, type);
            }
        }

        private static void AddNamespace(StringBuilder builder, Type type)
        {
            if (string.IsNullOrWhiteSpace(type.Namespace)) { return; }
            _ = builder.Append(type.Namespace);
            _ = builder.Append('.');
        }

        private static void AddClassName(StringBuilder builder, Type type)
        {
            // Format the declaring type.
            if (type.IsNested)
            {
                AddClassName(builder, type.DeclaringType);
                _ = builder.Append('+');
            }

            // Format the simple type name.
            var index = type.Name.IndexOfAny(SimpleNameTerminators);
            _ = builder.Append(index > 0 ? type.Name.Substring(0, index) : type.Name);

            // Format this type's generic arity.
            AddGenericArity(builder, type);
        }

        private static void AddGenericParameters(StringBuilder builder, Type type)
        {
            // Generic type definitions (eg, List<> without parameters) and non-generic types do not include any
            // parameters in their formatting.
            if (!type.IsConstructedGenericType || type.ContainsGenericParameters) { return; }

            var args = type.GetGenericArguments();
            _ = builder.Append('[');
            for (var i = 0; i < args.Length; i++)
            {
                _ = builder.Append('[');
                Format(builder, args[i], isElementType: false);
                _ = builder.Append(']');
                if (i + 1 < args.Length) _ = builder.Append(',');
            }

            _ = builder.Append(']');
        }

        private static void AddGenericArity(StringBuilder builder, Type type)
        {
            if (!type.IsGenericType) { return; }

            // The arity is the number of generic parameters minus the number of generic parameters in the declaring types.
            var baseTypeParameterCount =
                type.IsNested ? type.DeclaringType.GetGenericArguments().Length : 0;
            var arity = type.GetGenericArguments().Length - baseTypeParameterCount;

            // If all of the generic parameters are in the declaring types then this type has no parameters of its own.
            if (0u >= (uint)arity) { return; }

            _ = builder.Append('`');
            _ = builder.Append(arity);
        }

        private static void AddPointerSymbol(StringBuilder builder, Type type)
        {
            if (!type.IsPointer) { return; }
            _ = builder.Append('*');
        }

        private static void AddByRefSymbol(StringBuilder builder, Type type)
        {
            if (!type.IsByRef) { return; }
            _ = builder.Append('&');
        }

        private static void AddArrayRank(StringBuilder builder, Type type)
        {
            if (!type.IsArray) { return; }
            _ = builder.Append('[');
            _ = builder.Append(',', type.GetArrayRank() - 1);
            _ = builder.Append(']');
        }

        private static void AddAssembly(StringBuilder builder, Type type)
        {
            // Do not include the assembly name for the system assembly.
            if (SystemAssembly.Equals(type.Assembly)) { return; }

            _ = builder.Append(", ");
            _ = builder.Append(type.Assembly.GetName().Name);
        }

        #endregion
    }
}
