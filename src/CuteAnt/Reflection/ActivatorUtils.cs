using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt.Collections;
using Grace.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace CuteAnt.Reflection
{
    public static class ActivatorUtils
    {
        #region ** ConstructorMatcher Cache **

        private static readonly CachedReadConcurrentDictionary<Type, ConstructorMatcher[]> s_typeConstructorMatcherCache =
            new CachedReadConcurrentDictionary<Type, ConstructorMatcher[]>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static readonly CachedReadConcurrentDictionary<Type, ConstructorMatcher[]> s_typeConstructorMatcherDICache =
            new CachedReadConcurrentDictionary<Type, ConstructorMatcher[]>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static ConstructorMatcher[] GetTypeDeclaredConstructors(Type instanceType, bool forDI = false)
        {
            var cache = forDI ? s_typeConstructorMatcherDICache : s_typeConstructorMatcherCache;
            if (cache.TryGetValue(instanceType, out var matchers)) { return matchers; }

            if (instanceType.IsAbstract)
            {
                matchers = new ConstructorMatcher[] { new ConstructorMatcher(instanceType, instanceType.MakeDelegateForCtor()) };
                s_typeConstructorMatcherCache.TryAdd(instanceType, matchers);
                s_typeConstructorMatcherDICache.TryAdd(instanceType, EmptyArray<ConstructorMatcher>.Instance);
                return forDI ? EmptyArray<ConstructorMatcher>.Instance : matchers;
            }

            ConstructorMatcher[] diMatchers = matchers = null;
            if (instanceType != TypeConstants.StringType && !instanceType.IsArray)
            {
                try
                {
                    diMatchers = instanceType.GetTypeInfo().DeclaredConstructors
                                         .Where(_ => !_.IsStatic && _.IsPublic)
                                         .Select(_ => new ConstructorMatcher(instanceType, _))
                                         .ToArray() ?? EmptyArray<ConstructorMatcher>.Instance;
                    matchers = diMatchers;
                    if (0u >= (uint)diMatchers.Length)
                    {
                        matchers = new ConstructorMatcher[] { new ConstructorMatcher(instanceType, instanceType.MakeDelegateForCtor()) };
                    }
                    else if (instanceType.GetEmptyConstructor() is null)
                    {
                        matchers = matchers.Concat(new ConstructorMatcher[] { new ConstructorMatcher(instanceType, instanceType.MakeDelegateForCtor()) }).ToArray();
                    }
                }
                catch { }
            }

            if (diMatchers is null) { diMatchers = EmptyArray<ConstructorMatcher>.Instance; }
            if (matchers is null)
            {
                matchers = new ConstructorMatcher[] { new ConstructorMatcher(instanceType, instanceType.MakeDelegateForCtor()) };
            }

            s_typeConstructorMatcherCache.TryAdd(instanceType, matchers);
            s_typeConstructorMatcherDICache.TryAdd(instanceType, diMatchers);
            return forDI ? diMatchers : matchers;
        }

        #endregion

        #region -- GetConstructorMatcher / TryGetConstructorMatcher --

        public static ConstructorMatcher GetConstructorMatcher(Type instanceType, params Type[] argumentTypes)
        {
            if (!TryGetConstructorMatcher(instanceType, argumentTypes, out var bestMatcher))
            {
                ThrowInvalidOperationException(instanceType);
            }
            return bestMatcher;
        }
        public static ConstructorMatcher<TInstance> GetConstructorMatcher<TInstance>(params Type[] argumentTypes)
        {
            if (!TryGetConstructorMatcher<TInstance>(argumentTypes, out var bestMatcher))
            {
                ThrowInvalidOperationException<TInstance>();
            }
            return bestMatcher;
        }

        public static bool TryGetConstructorMatcher(Type instanceType, Type[] argumentTypes, out ConstructorMatcher matcher)
        {
            if (instanceType is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.instanceType);
            if (argumentTypes is null) { argumentTypes = Type.EmptyTypes; }

            matcher = null;
            foreach (var item in GetTypeDeclaredConstructors(instanceType))
            {
                if (item.StrictMatch(argumentTypes)) { matcher = item; break; }
            }
            return matcher != null;
        }

        public static bool TryGetConstructorMatcher<TInstance>(Type[] argumentTypes, out ConstructorMatcher<TInstance> matcher)
        {
            if (argumentTypes is null) { argumentTypes = Type.EmptyTypes; }

            matcher = null;
            foreach (var item in ConstructorMatcher<TInstance>.ConstructorMatchers)
            {
                if (item.StrictMatch(argumentTypes)) { matcher = item; break; }
            }
            return matcher != null;
        }

        #endregion

        #region -- FastCreateInstance --

        private static readonly CachedReadConcurrentDictionary<Type, CtorInvoker<object>> s_typeEmptyConstructorCache =
            new CachedReadConcurrentDictionary<Type, CtorInvoker<object>>(DictionaryCacheConstants.SIZE_MEDIUM);
        private static readonly Func<Type, CtorInvoker<object>> s_makeDelegateForCtorFunc = GetConstructorMethodInternal;
        private static readonly object[] s_emptyObjects = EmptyArray<object>.Instance;

        private static CtorInvoker<object> GetConstructorMethodInternal(Type instanceType)
            => GetConstructorMatcher(instanceType).Invocation;

        /// <summary>Creates a new instance from the default constructor of type</summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object FastCreateInstance(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeName); }
            var instanceType = TypeUtils.ResolveType(typeName);

            return FastCreateInstance(instanceType);
        }

        /// <summary>Creates a new instance from the default constructor of type</summary>
        public static object FastCreateInstance(Type instanceType)
        {
            if (instanceType is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.instanceType);

            return s_typeEmptyConstructorCache.GetOrAdd(instanceType, s_makeDelegateForCtorFunc).Invoke(s_emptyObjects);
        }

        /// <summary>Creates a new instance from the default constructor of type</summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <returns></returns>
        public static TInstance FastCreateInstance<TInstance>()
            => ConstructorMatcher<TInstance>.DefaultInvocation.Invoke(s_emptyObjects);

        /// <summary>Creates a new instance from the default constructor of type</summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public static TInstance FastCreateInstance<TInstance>(Type implementationType) => (TInstance)FastCreateInstance(implementationType);

        #endregion

        #region -- CreateInstance --

        public static object CreateInstance(string typeName, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(typeName)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeName); }
            var instanceType = TypeUtils.ResolveType(typeName);

            return CreateInstance(instanceType, parameters);
        }

        public static object CreateInstance(Type instanceType, params object[] parameters)
        {
            if (instanceType is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.instanceType);

            // 需要考虑可选参数，不能在 parameters 为空的情况下直接转 FastCreateInstance 模式
            int bestLength = -1;
            ConstructorMatcher bestMatcher = null;
            object[] parameterValues = null;
            ParameterInfo[] paramInfos = null;
            foreach (var matcher in GetTypeDeclaredConstructors(instanceType))
            {
                var length = matcher.Match(parameters, out var pvs, out var pis);
                if (length == -1) { continue; }
                if (bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                    parameterValues = pvs;
                    paramInfos = pis;
                }
            }

            if (bestMatcher is null)
            {
                ThrowInvalidOperationException(instanceType);
            }

            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValues[index] is null)
                {
                    if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                    {
                        ThrowInvalidOperationException(paramInfos[index].ParameterType, instanceType);
                    }
                    else
                    {
                        parameterValues[index] = defaultValue;
                    }
                }
            }

            return bestMatcher.Invocation.Invoke(parameterValues);
        }

        public static TInstance CreateInstance<TInstance>(params object[] parameters)
        {
            // 需要考虑可选参数，不能在 parameters 为空的情况下直接转 FastCreateInstance 模式
            int bestLength = -1;
            ConstructorMatcher<TInstance> bestMatcher = null;
            object[] parameterValues = null;
            ParameterInfo[] paramInfos = null;
            foreach (var matcher in ConstructorMatcher<TInstance>.ConstructorMatchers)
            {
                var length = matcher.Match(parameters, out var pvs, out var pis);
                if (length == -1) { continue; }
                if (bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                    parameterValues = pvs;
                    paramInfos = pis;
                }
            }

            if (bestMatcher is null)
            {
                ThrowInvalidOperationException<TInstance>();
            }

            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValues[index] is null)
                {
                    if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                    {
                        ThrowInvalidOperationException<TInstance>(paramInfos[index].ParameterType);
                    }
                    else
                    {
                        parameterValues[index] = defaultValue;
                    }
                }
            }

            return bestMatcher.Invocation.Invoke(parameterValues);
        }

        public static TInstance CreateInstance<TInstance>(Type implementationType, params object[] parameters)
            => (TInstance)CreateInstance(implementationType, parameters);

        #endregion

        #region -- CreateInstance with MSDI --

        public static object CreateInstance(IServiceProvider serviceProvider, string typeName, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(typeName)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeName); }
            var instanceType = TypeUtils.ResolveType(typeName);

            return CreateInstance(serviceProvider, instanceType, parameters);
        }

        public static object CreateInstance(IServiceProvider serviceProvider, Type instanceType, params object[] parameters)
        {
            int bestLength = -1;
            var seenPreferred = false;

            ConstructorMatcher bestMatcher = null;
            object[] parameterValues = null;
            ParameterInfo[] paramInfos = null;
            foreach (var matcher in GetTypeDeclaredConstructors(instanceType, true))
            {
                var isPreferred = matcher.Value != null ? matcher.Value.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), false) : false;
                var length = matcher.Match(parameters, out var pvs, out var pis);

                if (isPreferred)
                {
                    if (seenPreferred)
                    {
                        ThrowMultipleCtorsMarkedWithAttributeException();
                    }

                    if (length == -1)
                    {
                        ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
                    }
                }

                if (isPreferred || bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                    parameterValues = pvs;
                    paramInfos = pis;
                }

                seenPreferred |= isPreferred;
            }

            if (bestMatcher is null)
            {
                ThrowInvalidOperationException(instanceType);
            }

            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValues[index] is null)
                {
                    var value = serviceProvider.GetService(paramInfos[index].ParameterType);
                    if (value is null)
                    {
                        if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                        {
                            ThrowInvalidOperationException(paramInfos[index].ParameterType, instanceType);
                        }
                        else
                        {
                            parameterValues[index] = defaultValue;
                        }
                    }
                    else
                    {
                        parameterValues[index] = value;
                    }
                }
            }

            return bestMatcher.Invocation.Invoke(parameterValues);
        }

        public static TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, params object[] parameters)
        {
            int bestLength = -1;
            var seenPreferred = false;

            ConstructorMatcher<TInstance> bestMatcher = null;
            object[] parameterValues = null;
            ParameterInfo[] paramInfos = null;
            foreach (var matcher in ConstructorMatcher<TInstance>.DIConstructorMatchers)
            {
                var isPreferred = matcher.Value != null ? matcher.Value.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), false) : false;
                var length = matcher.Match(parameters, out var pvs, out var pis);

                if (isPreferred)
                {
                    if (seenPreferred)
                    {
                        ThrowMultipleCtorsMarkedWithAttributeException();
                    }

                    if (length == -1)
                    {
                        ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
                    }
                }

                if (isPreferred || bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                    parameterValues = pvs;
                    paramInfos = pis;
                }

                seenPreferred |= isPreferred;
            }

            if (bestMatcher is null)
            {
                ThrowInvalidOperationException<TInstance>();
            }

            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValues[index] is null)
                {
                    var value = serviceProvider.GetService(paramInfos[index].ParameterType);
                    if (value is null)
                    {
                        if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                        {
                            ThrowInvalidOperationException<TInstance>(paramInfos[index].ParameterType);
                        }
                        else
                        {
                            parameterValues[index] = defaultValue;
                        }
                    }
                    else
                    {
                        parameterValues[index] = value;
                    }
                }
            }

            return bestMatcher.Invocation.Invoke(parameterValues);
        }

        public static TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType, params object[] parameters)
            => (TInstance)CreateInstance(serviceProvider, implementationType, parameters);

        #endregion

        #region -- CreateInstance with Grace --

        public static object CreateInstance(ILocatorService services, string typeName, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(typeName)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeName); }
            var instanceType = TypeUtils.ResolveType(typeName);

            return CreateInstance(services, instanceType, parameters);
        }

        public static object CreateInstance(ILocatorService services, Type instanceType, params object[] parameters)
        {
            int bestLength = -1;
            ConstructorMatcher bestMatcher = null;
            object[] parameterValues = null;
            ParameterInfo[] paramInfos = null;
            foreach (var matcher in GetTypeDeclaredConstructors(instanceType, true))
            {
                var length = matcher.Match(parameters, out var pvs, out var pis);
                if (length == -1) { continue; }
                if (bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                    parameterValues = pvs;
                    paramInfos = pis;
                }
            }

            if (bestMatcher is null)
            {
                ThrowInvalidOperationException(instanceType);
            }

            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValues[index] is null)
                {
                    var value = services.LocateOrDefault(paramInfos[index].ParameterType);
                    if (value is null)
                    {
                        if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                        {
                            ThrowInvalidOperationException(paramInfos[index].ParameterType, instanceType);
                        }
                        else
                        {
                            parameterValues[index] = defaultValue;
                        }
                    }
                    else
                    {
                        parameterValues[index] = value;
                    }
                }
            }

            return bestMatcher.Invocation.Invoke(parameterValues);
        }

        public static TInstance CreateInstance<TInstance>(ILocatorService services, params object[] parameters)
        {
            int bestLength = -1;
            ConstructorMatcher<TInstance> bestMatcher = null;
            object[] parameterValues = null;
            ParameterInfo[] paramInfos = null;
            foreach (var matcher in ConstructorMatcher<TInstance>.DIConstructorMatchers)
            {
                var length = matcher.Match(parameters, out var pvs, out var pis);
                if (length == -1) { continue; }
                if (bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                    parameterValues = pvs;
                    paramInfos = pis;
                }
            }

            if (bestMatcher is null)
            {
                ThrowInvalidOperationException<TInstance>();
            }

            for (var index = 0; index != paramInfos.Length; index++)
            {
                if (parameterValues[index] is null)
                {
                    var value = services.LocateOrDefault(paramInfos[index].ParameterType);
                    if (value is null)
                    {
                        if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
                        {
                            ThrowInvalidOperationException<TInstance>(paramInfos[index].ParameterType);
                        }
                        else
                        {
                            parameterValues[index] = defaultValue;
                        }
                    }
                    else
                    {
                        parameterValues[index] = value;
                    }
                }
            }

            return bestMatcher.Invocation.Invoke(parameterValues);
        }

        public static TInstance CreateInstance<TInstance>(ILocatorService services, Type implementationType, params object[] parameters)
            => (TInstance)CreateInstance(services, implementationType, parameters);

        #endregion

        #region -- GetServiceOrCreateInstance --

        public static T GetServiceOrCreateInstance<T>(IServiceProvider provider)
            => (T)GetServiceOrCreateInstance(provider, typeof(T));

        public static object GetServiceOrCreateInstance(IServiceProvider provider, Type type)
            => provider.GetService(type) ?? CreateInstance(provider, type);

        public static T GetServiceOrCreateInstance<T>(ILocatorService services)
            => (T)GetServiceOrCreateInstance(services, typeof(T));

        public static object GetServiceOrCreateInstance(ILocatorService services, Type type)
            => services.LocateOrDefault(type) ?? CreateInstance(services, type);

        #endregion

        #region ** ThrowHelper **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowMultipleCtorsMarkedWithAttributeException()
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Multiple constructors were marked with {nameof(ActivatorUtilitiesConstructorAttribute)}.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowMarkedCtorDoesNotTakeAllProvidedArguments()
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException($"Constructor marked with {nameof(ActivatorUtilitiesConstructorAttribute)} does not accept all given argument types.");
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException(Type instanceType)
        {
            throw GetInvalidOperationException(instanceType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException<T>()
        {
            throw GetInvalidOperationException(typeof(T));
        }

        private static InvalidOperationException GetInvalidOperationException(Type instanceType)
        {
            return new InvalidOperationException($"A suitable constructor for type '{instanceType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException(Type parameterType, Type instanceType)
        {
            throw GetInvalidOperationException(parameterType, instanceType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException<T>(Type parameterType)
        {
            throw GetInvalidOperationException(parameterType, typeof(T));
        }

        private static InvalidOperationException GetInvalidOperationException(Type parameterType, Type instanceType)
        {
            return new InvalidOperationException($"Unable to resolve service for type '{parameterType}' while attempting to activate '{instanceType}'.");
        }

        #endregion
    }
}
