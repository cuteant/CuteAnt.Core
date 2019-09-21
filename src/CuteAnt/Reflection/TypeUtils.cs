using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CuteAnt.Collections;
using CuteAnt.Text;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

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
            public static readonly Type Type = TypeConstants.TypeType;

            /// <summary>值类型</summary>
            public static readonly Type ValueType = TypeConstants.ValueType;

            /// <summary>枚举类型</summary>
            public static readonly Type Enum = TypeConstants.EnumType;

            /// <summary>对象类型</summary>
            public static readonly Type Object = TypeConstants.ObjectType;

            /// <summary>字符串类型</summary>
            public static readonly Type String = TypeConstants.StringType;

            /// <summary>Guid</summary>
            public static readonly Type Guid = TypeConstants.GuidType;

            /// <summary>CombGuid</summary>
            public static readonly Type CombGuid = TypeConstants.CombGuidType;

            /// <summary>Guid</summary>
            public static readonly Type ByteArray = TypeConstants.ByteArrayType;
        }

        #endregion

        #region @@ Fields @@

        private static readonly ILogger s_logger = TraceLogger.GetLogger(typeof(TypeUtils));

        private static readonly ConcurrentDictionary<Tuple<Type, TypeFormattingOptions>, string> ParseableNameCache = new ConcurrentDictionary<Tuple<Type, TypeFormattingOptions>, string>();

        private static readonly ConcurrentDictionary<Tuple<Type, bool>, List<Type>> ReferencedTypes = new ConcurrentDictionary<Tuple<Type, bool>, List<Type>>();

        private static readonly CachedReflectionOnlyTypeResolver ReflectionOnlyTypeResolver = new CachedReflectionOnlyTypeResolver();

        #endregion

        #region -- GetSimpleTypeName --

        public static string GetSimpleTypeName(Type type, Predicate<Type> fullName)
        {
            if (type.IsNestedPublic || type.IsNestedPrivate)
            {
                if (type.DeclaringType.IsGenericType)
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

        #endregion

        #region -- SerializeTypeName --

        private const char c_keyDelimiter = ':';

        [Obsolete("=> RuntimeTypeNameFormatter.Serialize")]
        public static string SerializeTypeName(Type type) => RuntimeTypeNameFormatter.Serialize(type);

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

            if (t.IsGenericType) return GetTemplatedName(GetSimpleTypeName(t, fullName), t, t.GetGenericArguments(), fullName);

            if (t.IsArray)
            {
                return GetTemplatedName(t.GetElementType(), fullName)
                       + "["
                       + new string(',', t.GetArrayRank() - 1)
                       + "]";
            }

            return GetSimpleTypeName(t, fullName);
        }

        public static string GetTemplatedName(string baseName, Type t, Type[] genericArguments, Predicate<Type> fullName)
        {
            if (!t.IsGenericType || (t.DeclaringType != null && t.DeclaringType.IsGenericType)) return baseName;
            string s = baseName;
            s += "<";
            s += GetGenericTypeArgs(genericArguments, fullName);
            s += ">";
            return s;
        }

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
                if (!genericParameter.IsGenericType)
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

        public static string GetParameterizedTemplateName(Type type, bool applyRecursively = false, Predicate<Type> fullName = null)
        {
            if (fullName == null)
                fullName = tt => true;

            return GetParameterizedTemplateName(type, fullName, applyRecursively);
        }

        public static string GetParameterizedTemplateName(Type type, Predicate<Type> fullName, bool applyRecursively = false)
        {
            if (type.IsGenericType)
            {
                return GetParameterizedTemplateName(GetSimpleTypeName(type, fullName), type, applyRecursively, fullName);
            }

            if (fullName != null && fullName(type) == true)
            {
                return type.FullName;
            }

            return type.Name;
        }

        public static string GetParameterizedTemplateName(string baseName, Type type, bool applyRecursively = false, Predicate<Type> fullName = null)
        {
            if (fullName == null)
                fullName = tt => false;

            if (!type.IsGenericType) return baseName;

            string s = baseName;
            s += "<";
            bool first = true;
            foreach (var genericParameter in type.GetGenericArguments())
            {
                if (!first)
                {
                    s += ",";
                }
                if (applyRecursively && genericParameter.IsGenericType)
                {
                    s += GetParameterizedTemplateName(genericParameter, applyRecursively);
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

        #endregion

        #region -- GetRawClassName --

        public static string GetRawClassName(string baseName, Type t)
        {
            return t.IsGenericType ? baseName + '`' + t.GetGenericArguments().Length : baseName;
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
            var candidatesWithPositions = charsWithPositions.Zip(charsWithPositions.Skip(1), (c1, c2) => new { Str = c1.Ch.ToString() + c2.Ch, c1.Pos });

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

        private static string GetFullName(Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }
            if (type.IsNested && !type.IsGenericParameter)
            {
                return type.Namespace + "." + type.DeclaringType.Name + "." + type.Name;
            }
            if (type.IsArray)
            {
                return GetFullName(type.GetElementType())
                       + "["
                       + new string(',', type.GetArrayRank() - 1)
                       + "]";
            }
            return type.FullName ?? (type.IsGenericParameter ? type.Name : type.Namespace + "." + type.Name);
        }

        #endregion

        #region -- IsConcreteTemplateType --

        public static bool IsConcreteTemplateType(Type t)
        {
            if (t.IsGenericType) return true;
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

                current = current.BaseType;
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

        public static IEnumerable<Type> GetTypes(Assembly assembly, Predicate<Type> whereFunc, ILogger logger = null)
        {
            return assembly.IsDynamic
                 ? Enumerable.Empty<Type>()
                 : GetDefinedTypes(assembly, logger)
                      .Where(type => whereFunc(type));
        }
        public static IEnumerable<Type> GetDefinedTypes(Assembly assembly, ILogger logger = null)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (Exception exception)
            {
                if (null == logger) { logger = s_logger; }
                if (logger.IsWarningLevelEnabled())
                {
                    var message = $"Exception loading types from assembly '{assembly.FullName}': {TraceLogger.PrintException(exception)}.";
                    logger.LogWarning(exception, message);
                }

                if (exception is ReflectionTypeLoadException typeLoadException)
                {
                    return typeLoadException.Types?.Where(type => type != null) ?? Enumerable.Empty<Type>();
                }

                return Enumerable.Empty<Type>();
            }
        }

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

            return type.GetCustomAttributes(attribType, true).Any();
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
                GetParseableName(
                    type,
                    builder,
                    new Queue<Type>(
                        type.IsGenericTypeDefinition
                            ? type.GetGenericArguments()
                            : type.GenericTypeArguments),
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
        /// <param name="getNameFunc">Delegate that returns name for a type.</param>
        private static void GetParseableName(Type type, StringBuilder builder, Queue<Type> typeArguments, TypeFormattingOptions options, Func<Type, string> getNameFunc)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType().GetParseableName(options);
                if (!string.IsNullOrWhiteSpace(elementType))
                {
                    builder.AppendFormat(
                        "{0}[{1}]",
                        elementType,
                        string.Concat(Enumerable.Range(0, type.GetArrayRank() - 1).Select(_ => ',')));
                }

                return;
            }

            if (type.IsGenericParameter)
            {
                if (options.IncludeGenericTypeParameters)
                {
                    builder.Append(type.GetUnadornedTypeName());
                }

                return;
            }

            if (type.DeclaringType != null)
            {
                // This is not the root type.
                GetParseableName(type.DeclaringType, builder, typeArguments, options, t => t.GetUnadornedTypeName());
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
                    Enumerable.Range(0, Math.Min(type.GetGenericArguments().Count(), typeArguments.Count))
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
            else if (type.IsGenericTypeDefinition)
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
            var methodCall = expression.Body as MethodCallExpression;
            if (null == methodCall) { ThrowArgumentException_Expr(); }
            return methodCall.Method;
        }

        /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
        /// <typeparam name="T">The containing type of the method.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
        public static MethodInfo Method<T>(Expression<Func<T>> expression)
        {
            var methodCall = expression.Body as MethodCallExpression;
            if (null == methodCall) { ThrowArgumentException_Expr(); }
            return methodCall.Method;
        }

        /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
        /// <typeparam name="T">The containing type of the method.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
        public static MethodInfo Method<T>(Expression<Action<T>> expression)
        {
            var methodCall = expression.Body as MethodCallExpression;
            if (null == methodCall) { ThrowArgumentException_Expr(); }
            return methodCall.Method;
        }

        /// <summary>Returns the <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The <see cref="MethodInfo"/> for the simple method call in the provided <paramref name="expression"/>.</returns>
        public static MethodInfo Method(Expression<Action> expression)
        {
            var methodCall = expression.Body as MethodCallExpression;
            if (null == methodCall) { ThrowArgumentException_Expr(); }
            return methodCall.Method;
        }

        #endregion

        #region -- CallMethod --

        private static readonly CachedReadConcurrentDictionary<MethodInfo, MethodMatcher> s_methodMatcherCache =
            new CachedReadConcurrentDictionary<MethodInfo, MethodMatcher>(DictionaryCacheConstants.SIZE_SMALL);

        /// <summary>CallMethod</summary>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object CallMethod(MethodInfo method, object target, params object[] parameters)
        {
            if (null == method) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method); }

            var matcher = s_methodMatcherCache.GetOrAdd(method, mi => new MethodMatcher(mi));

            matcher.Match(parameters, out var parameterValues, out var parameterValuesSet, out var paramInfos);
            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValuesSet[index] == false)
                {
                    if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                    {
                        ThrowInvalidOperationException(paramInfos[index].ParameterType, method);
                    }
                    else
                    {
                        parameterValues[index] = defaultValue;
                    }
                }
            }

            return matcher.Invocation.Invoke(target, parameterValues);
        }

        public static TReturn CallMethod<TTarget, TReturn>(MethodInfo method, TTarget target, params object[] parameters)
        {
            if (null == method) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method); }

            var matcher = MethodMatcher<TTarget, TReturn>.GetMethodMatcher(method);

            matcher.Match(parameters, out var parameterValues, out var parameterValuesSet, out var paramInfos);
            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValuesSet[index] == false)
                {
                    if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                    {
                        ThrowInvalidOperationException(paramInfos[index].ParameterType, method);
                    }
                    else
                    {
                        parameterValues[index] = defaultValue;
                    }
                }
            }

            return matcher.Invocation.Invoke(target, parameterValues);
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
            var property = expression.Body as MemberExpression;
            if (null == property) { ThrowArgumentException_Expr(); }
            return property.Member as PropertyInfo;
        }

        /// <summary>Returns the <see cref="PropertyInfo"/> for the simple member access in the provided <paramref name="expression"/>.</summary>
        /// <typeparam name="TResult">The return type of the property.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The <see cref="PropertyInfo"/> for the simple member access call in the provided <paramref name="expression"/>.</returns>
        public static PropertyInfo Property<TResult>(Expression<Func<TResult>> expression)
        {
            var property = expression.Body as MemberExpression;
            if (null == property) { ThrowArgumentException_Expr(); }
            return property.Member as PropertyInfo;
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
            switch (expression.Body)
            {
                case MethodCallExpression methodCall:
                    return methodCall.Method;
                case MemberExpression property:
                    return property.Member;
                default:
                    ThrowArgumentException_Expr(); return null;
            }
        }

        /// <summary>Returns the <see cref="MemberInfo"/> for the simple member access in the provided <paramref name="expression"/>.</summary>
        /// <typeparam name="TResult">The return type of the method.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The <see cref="MemberInfo"/> for the simple member access call in the provided <paramref name="expression"/>.</returns>
        public static MemberInfo Member<TResult>(Expression<Func<TResult>> expression)
        {
            switch (expression.Body)
            {
                case MethodCallExpression methodCall:
                    return methodCall.Method;
                case MemberExpression property:
                    return property.Member;
                default:
                    ThrowArgumentException_Expr(); return null;
            }
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
                if (CombGuid.TryParse(value, CombGuidSequentialSegmentType.Comb, out var comb)) { return comb; }
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
                if (conversionType.IsValueType) value = ActivatorUtils.FastCreateInstance(conversionType);
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

        [MethodImpl(InlineMethod.Value)]
        public static bool IsEnumFlags(this Type type) => type.IsEnum && type.FirstAttribute<FlagsAttribute>() != null;

        #endregion

        #region -- ResolveType / TryResolveType --

        private static readonly CachedReadConcurrentDictionary<string, Type> _resolveTypeCache =
            new CachedReadConcurrentDictionary<string, Type>(DictionaryCacheConstants.SIZE_MEDIUM, StringComparer.Ordinal)
            {
                { "null", (Type)null },
                { "dynamic", typeof(object) }
            };
        private static readonly List<Func<string, Type>> _resolvers = new List<Func<string, Type>>();
        private static readonly ReaderWriterLockSlim _resolverLock = new ReaderWriterLockSlim();
        private static readonly CachedReadConcurrentDictionary<string, Assembly> _assemblyCache =
            new CachedReadConcurrentDictionary<string, Assembly>(StringComparer.Ordinal);
        private static readonly CachedReadConcurrentDictionary<TypeNameKey, Type> _typeNameKeyCache =
            new CachedReadConcurrentDictionary<TypeNameKey, Type>(DictionaryCacheConstants.SIZE_MEDIUM, TypeNameKeyComparer.Default);

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

        internal static Type ResolveType(in TypeNameKey typeNameKey)
        {
            if (_typeNameKeyCache.TryGetValue(typeNameKey, out var type)) { return type; }

            return InternalResolveType(typeNameKey);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type InternalResolveType(in TypeNameKey typeNameKey)
        {
            var qualifiedTypeName = Combine(typeNameKey.TypeName, typeNameKey.AssemblyName);

            if (!TryResolveType(qualifiedTypeName, out var type))
            {
                ThrowTypeAccessException($"{typeNameKey.TypeName}[{typeNameKey.AssemblyName}]");
            }
            AddTypeToCache(typeNameKey, type);

            return type;
        }


        public static Type ResolveType(string qualifiedTypeName)
        {
            if (!TryResolveType(qualifiedTypeName, out var result))
            {
                ThrowTypeAccessException(qualifiedTypeName);
            }
            return result;
        }

        /// <summary>Gets <see cref="Type"/> by full name (with falling back to the first part only).</summary>
        /// <param name="qualifiedTypeName">The type name.</param>
        /// <param name="type"></param>
        /// <returns>The <see cref="Type"/> if valid.</returns>
        public static bool TryResolveType(string qualifiedTypeName, out Type type)
        {
            if (string.IsNullOrWhiteSpace(qualifiedTypeName))
            {
                type = null; return false;
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Type_Name_Must_Not_Null, ExceptionArgument.qualifiedTypeName);
            }

            if (_resolveTypeCache.TryGetValue(qualifiedTypeName, out type)) { return true; }

            type = InternalResolveType(qualifiedTypeName);
            return type != null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type InternalResolveType(string qualifiedTypeName)
        {
            Type type = null;
            string serializedTypeName = null;
            if (qualifiedTypeName.IndexOf(c_keyDelimiter) > 0)
            {
                serializedTypeName = qualifiedTypeName;
                qualifiedTypeName = qualifiedTypeName.Replace(":", ", ");

                if (_resolveTypeCache.TryGetValue(qualifiedTypeName, out type))
                {
                    if (serializedTypeName != null) { _resolveTypeCache[serializedTypeName] = type; }
                    return type;
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
                if (!TryResolveType(typeNameKey, out type)) { return null; }
            }

            AddTypeToCache(qualifiedTypeName, type);
            if (serializedTypeName != null) { AddTypeToCache(serializedTypeName, type); }
            return type;
        }

        [MethodImpl(InlineMethod.Value)]
        private static void AddTypeToCache(string typeName, Type type)
        {
            var entry = _resolveTypeCache.GetOrAdd(typeName, _ => type);
            if (!ReferenceEquals(entry, type)) { ThrowInvalidOperationException(); }
        }

        /// <inheritdoc />
        private static bool TryResolveType(in TypeNameKey typeNameKey, out Type type)
        {
            if (_typeNameKeyCache.TryGetValue(typeNameKey, out type)) { return true; }

            if (!TryPerformUncachedTypeResolution(typeNameKey, out type)) { return false; }

            AddTypeToCache(typeNameKey, type);
            return true;
        }

        [MethodImpl(InlineMethod.Value)]
        private static void AddTypeToCache(in TypeNameKey typeNameKey, Type type)
        {
            var entry = _typeNameKeyCache.GetOrAdd(typeNameKey, _ => type);
            if (!ReferenceEquals(entry, type)) { ThrowInvalidOperationException(); }
        }

        private static bool TryPerformUncachedTypeResolution(in TypeNameKey typeNameKey, out Type type)
        {
            string assemblyName = typeNameKey.AssemblyName;
            string typeName = typeNameKey.TypeName;

            var qualifiedTypeName = Combine(typeName, assemblyName);

            Assembly[] allAssemblies;
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

                if (assembly != null)
                {
                    type = assembly.GetType(qualifiedTypeName, false);
                    if (type != null) { return true; }
                }

                allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            else
            {
                allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (TryResolveFromAllAssemblies(qualifiedTypeName, out type, allAssemblies)) { return true; }
            }

            type = Type.GetType(qualifiedTypeName, throwOnError: false)
                ?? Type.GetType(qualifiedTypeName, ResolveAssembly, ResolveType, false);

            return type != null;

            Assembly ResolveAssembly(AssemblyName asmName)
            {
                var fullAssemblyName = asmName.FullName;
                if (_assemblyCache.TryGetValue(fullAssemblyName, out var result)) return result;

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = assembly.GetName();
                    _assemblyCache[name.FullName] = assembly;
                    _assemblyCache[name.Name] = assembly;
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
                //if (TryResolveFromAllAssemblies(name, out var result, allAssemblies)) { return result; }
                return asm?.GetType(name, throwOnError: false, ignoreCase: ignoreCase) ?? Type.GetType(name, throwOnError: false, ignoreCase: ignoreCase);
            }
        }

        private static string Combine(string typeName, string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName)) { return typeName; }

            var sb = StringBuilderCache.Acquire();
            sb.Append(typeName);
            sb.Append(", ");
            sb.Append(assemblyName);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static bool TryResolveFromAllAssemblies(string fullName, out Type type, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(fullName, false);
                if (type != null) { return true; }
            }

            type = null;
            return false;
        }

        #endregion

        #region == SplitFullyQualifiedTypeName ==

        internal static TypeNameKey SplitFullyQualifiedTypeName(string fullyQualifiedTypeName)
        {
            int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

            string typeName;
            string assemblyName;

            if (assemblyDelimiterIndex != null)
            {
                typeName = fullyQualifiedTypeName.Trim(0, assemblyDelimiterIndex.GetValueOrDefault());
                assemblyName = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
            }
            else
            {
                typeName = fullyQualifiedTypeName;
                assemblyName = null;
            }

            return new TypeNameKey(assemblyName, typeName);
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

        #region -- GetTypeIdentifier --

        private static readonly CachedReadConcurrentDictionary<Type, string> s_typeIdentifierCache =
            new CachedReadConcurrentDictionary<Type, string>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static int _typeIdentifier;
        public static string GetTypeIdentifier(this Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }
            return s_typeIdentifierCache.GetOrAdd(type, t => Interlocked.Increment(ref _typeIdentifier).ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region ** ThrowHelper **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentException_Expr()
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException("Expression type unsupported.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowTypeAccessException(string qualifiedTypeName)
        {
            throw GetTypeAccessException();

            TypeAccessException GetTypeAccessException()
            {
                return new TypeAccessException($"Unable to find a type named {qualifiedTypeName}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException()
        {
            throw GetInvalidOperationException();

            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("inconsistent type name association");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException(Type parameterType, MethodInfo method)
        {
            throw GetInvalidOperationException();

            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Unable to resolve service for type '{parameterType}' while attempting to activate '{method}'.");
            }
        }

        #endregion
    }
}
