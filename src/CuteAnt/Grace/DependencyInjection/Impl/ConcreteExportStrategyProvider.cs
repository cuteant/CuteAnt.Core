﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CuteAnt;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Impl.CompiledStrategies;
using Grace.DependencyInjection.Impl.EnumerableStrategies;
using Grace.DependencyInjection.Impl.KnownTypeStrategies;
using Grace.DependencyInjection.Impl.Wrappers;

namespace Grace.DependencyInjection.Impl
{
    /// <summary>Provides export strategies for concrete types</summary>
    [DebuggerDisplay("ConcreteExportStrategyProvider")]
    public class ConcreteExportStrategyProvider : IMissingExportStrategyProvider, IConcreteExportStrategyProvider
    {
        private const string InvokeMethodName = "Invoke";
        private static readonly HashSet<string> s_immutableTypeNames = new HashSet<string>(
            new string[]
            {
                "System.Collections.Immutable.ImmutableList`1",
                "System.Collections.Immutable.ImmutableArray`1",
                "System.Collections.Immutable.ImmutableQueue`1",
                "System.Collections.Immutable.ImmutableStack`1",
                "System.Collections.Immutable.ImmutableSortedSet`1",
                "System.Collections.Immutable.ImmutableHashSet`1"
            }, StringComparer.Ordinal);

        private static readonly HashSet<Type> s_canLocateGenericTypes = new HashSet<Type>(
            new Type[]
            {
                typeof(ImmutableLinkedList<>), typeof(ImmutableArray<>),
                typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>), typeof(ReadOnlyCollection<>),
#if NET40
                typeof(ReadOnlyCollectionX<>),
#endif
                typeof(IList<>), typeof(ICollection<>), typeof(List<>),
                typeof(KeyedLocateDelegate<,>)
            });

        private static readonly HashSet<Type> s_listTypes = new HashSet<Type>(
            new Type[] { typeof(IList<>), typeof(ICollection<>), typeof(List<>) });

        private static readonly HashSet<Type> s_readOnlyListTypes = new HashSet<Type>(
            new Type[]
            {
                typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>),
#if NET40
                typeof(ReadOnlyCollectionX<>)
#else
                typeof(ReadOnlyCollection<>)
#endif
            });

        private ImmutableLinkedList<Func<Type, bool>> _filters = ImmutableLinkedList<Func<Type, bool>>.Empty;

        /// <summary>Add Filter type filter</summary>
        /// <param name="filter"></param>
        public virtual void AddFilter(Func<Type, bool> filter)
        {
            if (filter != null)
            {
                _filters = _filters.Add(filter);
            }
        }

        /// <summary>Can a given request be located using this provider</summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual bool CanLocate(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var requestedType = request.ActivationType;

#if NET40
            if (requestedType.IsConstructedGenericType())
#else
            if (requestedType.IsConstructedGenericType)
#endif
            {
                var genericType = requestedType.GetGenericTypeDefinition();

                // ## 苦竹 修改 ## if (genericType == typeof(ImmutableLinkedList<>)) { return true; }

                // if (genericType == typeof(ImmutableArray<>)) { return true; }

                //        if (genericType == typeof(IReadOnlyCollection<>) ||
                //            genericType == typeof(IReadOnlyList<>) ||
                //#if NET40
                //            genericType == typeof(ReadOnlyCollectionX<>) ||
                //#endif
                //            genericType == typeof(ReadOnlyCollection<>))
                //        {
                //          return true;
                //        }

                // if (genericType == typeof(IList<>) || genericType == typeof(ICollection<>) || genericType
                // == typeof(List<>)) { return true; }

                // if (genericType == typeof(KeyedLocateDelegate<,>))
                if (s_canLocateGenericTypes.Contains(genericType))
                {
                    return true;
                }

                // ## 苦竹 修改 ##
                //if (genericType.FullName == "System.Collections.Immutable.ImmutableList`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableArray`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableQueue`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableStack`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableSortedSet`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableHashSet`1")
                if (s_immutableTypeNames.Contains(genericType.FullName))
                {
                    return true;
                }
            }

            if (requestedType.IsInterface || requestedType.IsAbstract)
            {
                return false;
            }

            if (typeof(MulticastDelegate).IsAssignableFrom(requestedType))
            {
                var method = requestedType.GetDeclaredMethod(InvokeMethodName);

                if (method.ReturnType != TypeConstants.VoidType &&
                    scope.CanLocate(method.ReturnType))
                {
                    return method.GetParameters().Length <= 5;
                }

                return false;
            }

            return ShouldCreateConcreteStrategy(request);
        }

        /// <summary>Provide exports for a missing type</summary>
        /// <param name="scope">scope to provide value</param>
        /// <param name="request">request</param>
        /// <returns>set of activation strategies</returns>
        public virtual IEnumerable<IActivationStrategy> ProvideExports(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var requestedType = request.ActivationType;

#if NET40
            if (requestedType.IsConstructedGenericType())
#else
            if (requestedType.IsConstructedGenericType)
#endif
            {
                var genericType = requestedType.GetGenericTypeDefinition();

                if (genericType == typeof(ImmutableLinkedList<>))
                {
                    yield return new ImmutableLinkListStrategy(scope);
                    yield break;
                }

                if (genericType == typeof(ImmutableArray<>))
                {
                    yield return new ImmutableArrayStrategy(scope);
                    yield break;
                }

                // ## 苦竹 修改 ##
#if NET40
                //if (genericType == typeof(IReadOnlyCollection<>) ||
                //    genericType == typeof(IReadOnlyList<>) ||
                //    genericType == typeof(ReadOnlyCollectionX<>))
                if (s_readOnlyListTypes.Contains(genericType))
                {
                    yield return new ReadOnlyCollectionXStrategy(scope);
                    yield break;
                }
                if (genericType == typeof(ReadOnlyCollection<>))
                {
                    yield return new ReadOnlyCollectionStrategy(scope);
                    yield break;
                }
#else
                //if (genericType == typeof(IReadOnlyCollection<>) ||
                //    genericType == typeof(IReadOnlyList<>) ||
                //    genericType == typeof(ReadOnlyCollection<>))
                if (s_readOnlyListTypes.Contains(genericType))
                {
                    yield return new ReadOnlyCollectionStrategy(scope);
                    yield break;
                }
#endif

                // ## 苦竹 修改 ##
                //if (genericType == typeof(IList<>) ||
                //    genericType == typeof(ICollection<>) ||
                //    genericType == typeof(List<>))
                if (s_listTypes.Contains(genericType))
                {
                    yield return new ListEnumerableStrategy(scope);
                    yield break;
                }

                if (genericType == typeof(KeyedLocateDelegate<,>))
                {
                    yield return new KeyedLocateDelegateStrategy(scope);
                    yield break;
                }

                // ## 苦竹 修改 ##
                //if (genericType.FullName == "System.Collections.Immutable.ImmutableList`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableArray`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableQueue`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableStack`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableSortedSet`1" ||
                //    genericType.FullName == "System.Collections.Immutable.ImmutableHashSet`1")
                if (s_immutableTypeNames.Contains(genericType.FullName))
                {
                    yield return new ImmutableCollectionStrategy(genericType, scope);
                    yield break;
                }
            }

            if (requestedType.IsInterface || requestedType.IsAbstract)
            {
                yield break;
            }

            if (typeof(MulticastDelegate).IsAssignableFrom(requestedType))
            {
                var method = requestedType.GetDeclaredMethod(InvokeMethodName);

                if (method.ReturnType != TypeConstants.VoidType && scope.CanLocate(method.ReturnType))
                {
                    var parameterCount = method.GetParameters().Length;

                    switch (parameterCount)
                    {
                        case 0:
                            yield return new DelegateNoArgWrapperStrategy(requestedType, scope);
                            break;

                        case 1:
                            yield return new DelegateOneArgWrapperStrategy(requestedType, scope);
                            break;

                        case 2:
                            yield return new DelegateTwoArgWrapperStrategy(requestedType, scope);
                            break;

                        case 3:
                            yield return new DelegateThreeArgWrapperStrategy(requestedType, scope);
                            break;

                        case 4:
                            yield return new DelegateFourArgWrapperStrategy(requestedType, scope);
                            break;

                        case 5:
                            yield return new DelegateFiveArgWrapperStrategy(requestedType, scope);
                            break;
                    }
                }
            }
            else if (ShouldCreateConcreteStrategy(request))
            {
                var strategy =
                    new CompiledExportStrategy(requestedType, scope, request.Services.LifestyleExpressionBuilder).ProcessAttributeForStrategy();

                strategy.Lifestyle = scope.ScopeConfiguration.AutoRegistrationLifestylePicker?.Invoke(requestedType);

                yield return strategy;
            }
        }

        private static readonly HashSet<Type> s_specialTypes = new HashSet<Type>(
           new Type[]
           {
                typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(DateTimeOffset),
                typeof(Guid), typeof(CombGuid)
           });

        /// <summary>Should a type be exported</summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual bool ShouldCreateConcreteStrategy(IActivationExpressionRequest request)
        {
            var type = request.ActivationType;

            // ## 苦竹 修改 ##
            //if (type == typeof(string) || type.IsPrimitive)
            if (type.IsPrimitive || s_specialTypes.Contains(type))
            {
                return false;
            }

            const string _systemNamespace = "System";
            if (type.IsValueType &&
                (string.Equals(_systemNamespace, type.Namespace) ||
                (type.Namespace?.StartsWith(_systemNamespace, StringComparison.Ordinal) ?? false)))
            {
                return false;
            }

            if (type
#if NET40
                .IsConstructedGenericType()
#else
                .IsConstructedGenericType
#endif
                && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return false;
            }

            return type.GetTypeInfo().DeclaredConstructors.Any(c => c.IsPublic && !c.IsStatic) &&
                   _filters.All(func => !func(type));
        }
    }
}