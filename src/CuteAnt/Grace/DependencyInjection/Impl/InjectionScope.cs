﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Exceptions;
using Grace.DependencyInjection.Impl.KnownTypeStrategies;
using Grace.DependencyInjection.Impl.Wrappers;
using Grace.Diagnostics;
#if NET40
using System.Reflection;
#endif

namespace Grace.DependencyInjection.Impl
{
    /// <summary>Root injection scope that is inherited by the Dependency injection container</summary>
    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    [DebuggerTypeProxy(typeof(InjectionScopeDebuggerView))]
    public class InjectionScope : BaseExportLocatorScope, IInjectionScope
    {
        #region Fields

        /// <summary>Internal field storage</summary>
        protected InternalFieldStorageClass InternalFieldStorage = new InternalFieldStorageClass();

        /// <summary>string constant that is used to locate a lock for adding strategies to the container
        /// Note: Do not use this unless you are working on container internals</summary>
        public const string ActivationStrategyAddLockName = "ActivationStrategyAddLock";

        #endregion

        #region Constructors

        /// <summary>Constructor that takes configuration action</summary>
        /// <param name="configuration">configuration action</param>
        public InjectionScope(Action<InjectionScopeConfiguration> configuration)
          : this(CreateConfiguration(configuration), null, "RootScope") { }

        /// <summary>Constructor takes a configuration object</summary>
        /// <param name="configuration"></param>
        public InjectionScope(IInjectionScopeConfiguration configuration)
          : this(configuration, null, "RootScope") { }

        /// <summary>Configuration object constructor</summary>
        /// <param name="configuration"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        public InjectionScope(IInjectionScopeConfiguration configuration, IInjectionScope parent, string name)
          : base(parent, name, new ActivationStrategyDelegateCache())
        {
            configuration.SetInjectionScope(this);

            InternalFieldStorage.ScopeConfiguration = configuration;

            InternalFieldStorage.InjectionContextCreator = configuration.Implementation.Locate<IInjectionContextCreator>();

            InternalFieldStorage.CanLocateTypeService = configuration.Implementation.Locate<ICanLocateTypeService>();

            InternalFieldStorage.ActivationStrategyCompiler = configuration.Implementation.Locate<IActivationStrategyCompiler>();

            InternalFieldStorage.StrategyCollectionContainer =
                AddDisposable(configuration.Implementation.Locate<IActivationStrategyCollectionContainer<ICompiledExportStrategy>>());

            InternalFieldStorage.DecoratorCollectionContainer =
                AddDisposable(configuration.Implementation.Locate<IActivationStrategyCollectionContainer<ICompiledDecoratorStrategy>>());

            if (configuration.AutoRegisterUnknown && Parent == null)
            {
                InternalFieldStorage.MissingExportStrategyProviders =
                    InternalFieldStorage.MissingExportStrategyProviders.Add(
                        configuration.Implementation.Locate<IMissingExportStrategyProvider>());
            }

            if (configuration.SupportFuncType)
            {
                StrategyCollectionContainer.AddStrategy(new FuncTypeStrategy(this));
            }
        }

        #endregion

        #region Public members

        /// <summary>Scope configuration</summary>
        public IInjectionScopeConfiguration ScopeConfiguration => InternalFieldStorage.ScopeConfiguration;

        /// <summary>Compiler that produces Activation Strategy Delegates</summary>
        IActivationStrategyCompiler IInjectionScope.StrategyCompiler => InternalFieldStorage.ActivationStrategyCompiler;

        /// <summary>Can Locator type</summary>
        /// <param name="type">type to locate</param>
        /// <param name="consider"></param>
        /// <param name="key">key to use while locating</param>
        /// <returns></returns>
        public bool CanLocate(Type type, ActivationStrategyFilter consider = null, object key = null)
            => InternalFieldStorage.CanLocateTypeService.CanLocate(this, type, consider, key);

        /// <summary>Locate a specific type</summary>
        /// <param name="type">type to locate</param>
        /// <returns>located instance</returns>
        public object Locate(Type type)
        {
            return DelegateCache.ExecuteActivationStrategyDelegate(type, this);
        }

        /// <summary>Locate type or return default value</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object LocateOrDefault(Type type)
        {
            return DelegateCache.ExecuteActivationStrategyDelegateAllowNull(type, this);
        }

        /// <summary>Locate type or return default value</summary>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public object LocateOrDefault(Type type, object defaultValue)
        {
            return DelegateCache.ExecuteActivationStrategyDelegateAllowNull(type, this) ?? defaultValue;
        }

        /// <summary>Locate type</summary>
        /// <typeparam name="T">type to locate</typeparam>
        /// <returns>located instance</returns>
        public T Locate<T>() => (T)Locate(typeof(T));

        /// <summary>Locate or return default</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LocateOrDefault<T>() => (T)LocateOrDefault(typeof(T));

        /// <summary>Locate or return default</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T LocateOrDefault<T>(T defaultValue) => (T)LocateOrDefault(typeof(T), defaultValue);

        /// <summary>Locate specific type using extra data or key</summary>
        /// <param name="type">type to locate</param>
        /// <param name="extraData">extra data to be used during construction</param>
        /// <param name="consider">filter out exports you don't want to consider</param>
        /// <param name="withKey">key to use for locating type</param>
        /// <param name="isDynamic">skip cache and look through exports</param>
        /// <returns>located instance</returns>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public object Locate(Type type, object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
        {
            IInjectionContext context = extraData == null ?
                null : CreateInjectionContextFromExtraData(type, extraData);

            if (withKey == null && consider == null && !isDynamic)
            {
                return DelegateCache.ExecuteActivationStrategyDelegateWithContext(type, this, false, context);
            }

            return InternalLocate(this, this, type, consider, withKey, context, false, isDynamic);
        }

        /// <summary>Locate specific type using extra data or key</summary>
        /// <typeparam name="T">type to locate</typeparam>
        /// <param name="extraData">extra data</param>
        /// <param name="consider">filter out exports you don't want to consider</param>
        /// <param name="withKey">key to use during construction</param>
        /// <param name="isDynamic">skip cache and look at all strategies</param>
        /// <returns>located instance</returns>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public T Locate<T>(object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
            => (T)Locate(typeof(T), extraData, consider, withKey, isDynamic);

        /// <summary>Locate all instances of a type</summary>
        /// <param name="type">type to locate</param>
        /// <param name="extraData">extra data</param>
        /// <param name="consider">provide method to filter out exports</param>
        /// <param name="comparer">comparer to use for sorting</param>
        /// <returns>list of all type</returns>
        public List<object> LocateAll(Type type, object extraData = null, ActivationStrategyFilter consider = null, IComparer<object> comparer = null)
        {
            return ((IInjectionScope)this).InternalLocateAll(this, this, type, extraData, consider, comparer);
        }

        /// <summary>Locate all of a specific type</summary>
        /// <typeparam name="T">type to locate</typeparam>
        /// <param name="type">type to locate</param>
        /// <param name="extraData">extra data to use during construction</param>
        /// <param name="consider">provide method to filter out exports</param>
        /// <param name="comparer">comparer to use for sorting</param>
        /// <returns>list of all located</returns>
        public List<T> LocateAll<T>(Type type = null, object extraData = null, ActivationStrategyFilter consider = null, IComparer<T> comparer = null)
        {
            return ((IInjectionScope)this).InternalLocateAll(this, this, type ?? typeof(T), extraData, consider, comparer);
        }

        /// <summary>Try to locate a specific type</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">located value</param>
        /// <param name="extraData">extra data to be used during construction</param>
        /// <param name="consider">filter out exports you don't want</param>
        /// <param name="withKey">key to use while locating</param>
        /// <param name="isDynamic">skip cache and look at all exports</param>
        /// <returns></returns>
        public bool TryLocate<T>(out T value, object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
        {
            var context = CreateInjectionContextFromExtraData(typeof(T), extraData);

            var newValue = InternalLocate(this, this, typeof(T), consider, withKey, context, true, isDynamic);

            var returnValue = false;

            if (newValue != null)
            {
                returnValue = true;
                value = (T)newValue;
            }
            else
            {
                value = default(T);
            }

            return returnValue;
        }

        /// <summary>Try to locate an export by type</summary>
        /// <param name="type">locate type</param>
        /// <param name="value">out value</param>
        /// <param name="extraData">extra data to use during locate</param>
        /// <param name="consider">filter out exports you don't want</param>
        /// <param name="withKey">key to use during locate</param>
        /// <param name="isDynamic">skip cache and look at all exports</param>
        /// <returns>returns tue if export found</returns>
        public bool TryLocate(Type type, out object value, object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
        {
            var context = CreateInjectionContextFromExtraData(type, extraData);

            value = InternalLocate(this, this, type, consider, withKey, context, true, isDynamic);

            return value != null;
        }

        /// <summary>Locate by name</summary>
        /// <param name="name"></param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <returns></returns>
        public object LocateByName(string name, object extraData = null, ActivationStrategyFilter consider = null)
        {
            return ((IInjectionScope)this).LocateByNameFromChildScope(this,
                this, name, extraData, consider, false);
        }

        /// <summary>Locate all by specific name</summary>
        /// <param name="name"></param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <returns></returns>
        public List<object> LocateAllByName(string name, object extraData = null, ActivationStrategyFilter consider = null)
        {
            return ((IInjectionScope)this).InternalLocateAllByName(this,
                this,
                name,
                extraData,
                consider);
        }

        /// <summary>Try to locate by name</summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <returns></returns>
        public bool TryLocateByName(string name, out object value, object extraData = null, ActivationStrategyFilter consider = null)
        {
            value = ((IInjectionScope)this).LocateByNameFromChildScope(this,
                this, name, extraData, consider, true);

            return value != null;
        }

        /// <summary>Create as a new IExportLocate scope</summary>
        /// <param name="scopeName">scope name</param>
        /// <returns>new scope</returns>
        public virtual IExportLocatorScope BeginLifetimeScope(string scopeName = "")
        {
            return new LifetimeScope(this, this, scopeName, DelegateCache);
        }

        /// <summary>Create injection context</summary>
        /// <param name="extraData">extra data</param>
        /// <returns></returns>
        public IInjectionContext CreateContext(object extraData = null)
            => InternalFieldStorage.InjectionContextCreator.CreateContext(extraData);

        /// <summary>Configure the injection scope</summary>
        /// <param name="registrationBlock"></param>
        public void Configure(Action<IExportRegistrationBlock> registrationBlock)
        {
            lock (GetLockObject(ActivationStrategyAddLockName))
            {
                var provider = ScopeConfiguration.Implementation.Locate<IExportRegistrationBlockValueProvider>();

                registrationBlock(provider);

                foreach (var inspector in provider.GetInspectors())
                {
                    StrategyCollectionContainer.AddInspector(inspector);
                    WrapperCollectionContainer.AddInspector(inspector);
                    DecoratorCollectionContainer.AddInspector(inspector);
                }

                foreach (var missingExportStrategyProvider in provider.GetMissingExportStrategyProviders())
                {
                    InternalFieldStorage.MissingExportStrategyProviders = InternalFieldStorage.MissingExportStrategyProviders.Add(missingExportStrategyProvider);
                }

                foreach (var expressionProvider in provider.GetMissingDependencyExpressionProviders())
                {
                    InternalFieldStorage.MissingDependencyExpressionProviders =
                        InternalFieldStorage.MissingDependencyExpressionProviders.Add(expressionProvider);
                }

                foreach (var injectionValueProvider in provider.GetValueProviders())
                {
                    InternalFieldStorage.ValueProviders = InternalFieldStorage.ValueProviders.Add(injectionValueProvider);
                }

                foreach (var compiledWrapperStrategy in provider.GetWrapperStrategies())
                {
                    WrapperCollectionContainer.AddStrategy(compiledWrapperStrategy);
                }

                foreach (var decorator in provider.GetDecoratorStrategies())
                {
                    DecoratorCollectionContainer.AddStrategy(decorator);
                }

                foreach (var strategy in provider.GetExportStrategies())
                {
                    StrategyCollectionContainer.AddStrategy(strategy);

                    foreach (var secondaryStrategy in strategy.SecondaryStrategies())
                    {
                        StrategyCollectionContainer.AddStrategy(secondaryStrategy);
                    }
                }

                foreach (var selector in provider.GetMemberInjectionSelectors())
                {
                    InternalFieldStorage.MemberInjectionSelectors = InternalFieldStorage.MemberInjectionSelectors.Add(selector);
                }
            }
        }

        /// <summary>Configure with module</summary>
        /// <param name="module">configuration module</param>
        public void Configure(IConfigurationModule module)
        {
            if (null == module) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.module);

            Configure(module.Configure);
        }

        /// <summary>Strategies associated with this scope</summary>
        public IActivationStrategyCollectionContainer<ICompiledExportStrategy> StrategyCollectionContainer
            => InternalFieldStorage.StrategyCollectionContainer;

        /// <summary>Wrappers associated with this scope</summary>
        public IActivationStrategyCollectionContainer<ICompiledWrapperStrategy> WrapperCollectionContainer
            => InternalFieldStorage.Wrappers ?? GetWrappers();

        /// <summary>Decorators associated with this scope</summary>
        public IActivationStrategyCollectionContainer<ICompiledDecoratorStrategy> DecoratorCollectionContainer
            => InternalFieldStorage.DecoratorCollectionContainer;

        /// <summary>Member</summary>
        public IEnumerable<IMemberInjectionSelector> MemberInjectionSelectors => InternalFieldStorage.MemberInjectionSelectors;

        /// <summary>List of missing export strategy providers</summary>
        public IEnumerable<IMissingExportStrategyProvider> MissingExportStrategyProviders => InternalFieldStorage.MissingExportStrategyProviders;

        /// <summary>List of missing dependency expression providers</summary>
        public IEnumerable<IMissingDependencyExpressionProvider> MissingDependencyExpressionProviders => InternalFieldStorage.MissingDependencyExpressionProviders;

        /// <summary>List of value providers that can be used during construction of linq expression</summary>
        public IEnumerable<IInjectionValueProvider> InjectionValueProviders => InternalFieldStorage.ValueProviders;

        /// <summary>Locate an export from a child scope</summary>
        /// <param name="childScope">scope where the locate originated</param>
        /// <param name="disposalScope"></param>
        /// <param name="type">type to locate</param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <param name="key"></param>
        /// <param name="allowNull"></param>
        /// <param name="isDynamic"></param>
        /// <returns>configuration object</returns>
        object IInjectionScope.LocateFromChildScope(IExportLocatorScope childScope, IDisposalScope disposalScope, Type type, object extraData, ActivationStrategyFilter consider, object key, bool allowNull, bool isDynamic)
            => InternalLocate(childScope, disposalScope, type, consider, key, CreateInjectionContextFromExtraData(type, extraData), allowNull, isDynamic);

        /// <summary></summary>
        /// <param name="childScope"></param>
        /// <param name="disposalScope"></param>
        /// <param name="name"></param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <param name="allowNull"></param>
        /// <returns></returns>
        object IInjectionScope.LocateByNameFromChildScope(IExportLocatorScope childScope, IDisposalScope disposalScope,
          string name, object extraData, ActivationStrategyFilter consider, bool allowNull)
        {
            var collection = StrategyCollectionContainer.GetActivationStrategyCollectionByName(name);

            ICompiledExportStrategy strategy = null;

            if (collection != null)
            {
                if (consider != null)
                {
                    var context = new StaticInjectionContext(typeof(object));

                    strategy =
                        collection.GetStrategies()
                            .FirstOrDefault(
                                s => (!s.HasConditions || s.Conditions.All(con => con.MeetsCondition(s, context))) && consider(s));
                }
                else
                {
                    strategy = collection.GetPrimary();

                    if (strategy == null)
                    {
                        var context = new StaticInjectionContext(typeof(object));

                        strategy = collection.GetStrategies()
                            .FirstOrDefault(
                                s => !s.HasConditions || s.Conditions.All(con => con.MeetsCondition(s, context)));
                    }
                }
            }

            if (strategy != null)
            {
                var strategyDelegate =
                    strategy.GetActivationStrategyDelegate(this, InternalFieldStorage.ActivationStrategyCompiler, typeof(object));

                return strategyDelegate(childScope, disposalScope, CreateContext(extraData));
            }

            if (Parent != null)
            {
                return ((IInjectionScope)Parent).LocateByNameFromChildScope(childScope, disposalScope, name, extraData,
                    consider, allowNull);
            }

            if (!allowNull) { ThrowLocateException_Object(); }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowLocateException_Object()
        {
            throw GetLocateException();
            LocateException GetLocateException()
            {
                return new LocateException(new StaticInjectionContext(typeof(object)));
            }
        }

        /// <summary>Internal locate all method</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <param name="disposalScope"></param>
        /// <param name="type"></param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        List<T> IInjectionScope.InternalLocateAll<T>(IExportLocatorScope scope, IDisposalScope disposalScope, Type type, object extraData, ActivationStrategyFilter consider, IComparer<T> comparer)
        {
            var returnList = new List<T>();

            var context = CreateInjectionContextFromExtraData(typeof(T), extraData);

            var collection = StrategyCollectionContainer.GetActivationStrategyCollection(type);

            if (collection != null)
            {
                LocateEnumerablesFromStrategyCollection(collection, scope, disposalScope, type, context, consider, returnList);
            }

#if NET40
            if (type.IsConstructedGenericType())
#else
            if (type.IsConstructedGenericType)
#endif
            {
                var genericType = type.GetGenericTypeDefinition();

                collection = StrategyCollectionContainer.GetActivationStrategyCollection(genericType);

                if (collection != null)
                {
                    LocateEnumerablesFromStrategyCollection(collection, scope, disposalScope, type, context, consider, returnList);
                }
            }

            if (Parent is IInjectionScope injectionParent)
            {
                returnList.AddRange(injectionParent.InternalLocateAll<T>(scope, disposalScope, type, context, consider, null));
            }

            if (comparer != null) { returnList.Sort(comparer); }

            return returnList;
        }

        /// <summary></summary>
        /// <param name="scope"></param>
        /// <param name="disposalScope"></param>
        /// <param name="exportName"></param>
        /// <param name="extraData"></param>
        /// <param name="consider"></param>
        /// <returns></returns>
        List<object> IInjectionScope.InternalLocateAllByName(IExportLocatorScope scope, IDisposalScope disposalScope, string exportName, object extraData, ActivationStrategyFilter consider)
        {
            var context = CreateContext(extraData);

            var returnList = new List<object>();

            var collection = StrategyCollectionContainer.GetActivationStrategyCollectionByName(exportName);

            foreach (var strategy in collection.GetStrategies())
            {
                if (consider == null || consider(strategy))
                {
                    var activation = strategy.GetActivationStrategyDelegate(this,
                        InternalFieldStorage.ActivationStrategyCompiler, typeof(object));

                    returnList.Add(activation(scope, disposalScope, context.Clone()));
                }
            }

            if (Parent is IInjectionScope injectionScopeParent)
            {
                returnList.AddRange(injectionScopeParent.InternalLocateAllByName(scope, disposalScope, exportName, context, consider));
            }

            return returnList;
        }

        /// <summary>Creates a new child scope This is best used for long term usage, not per request scenario</summary>
        /// <param name="configure">configure scope</param>
        /// <param name="scopeName">scope name</param>
        /// <returns></returns>
        public IInjectionScope CreateChildScope(Action<IExportRegistrationBlock> configure = null, string scopeName = "")
        {
            var newScope = new InjectionScope(ScopeConfiguration.Clone(), this, scopeName);

            if (configure != null) { newScope.Configure(configure); }

            return newScope;
        }

        object IServiceProvider.GetService(Type type)
        {
            return DelegateCache.ExecuteActivationStrategyDelegateAllowNull(type, this);
        }

        /// <summary>TBD</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="createDelegate"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public T GetOrCreateScopedService<T>(int id, ActivationStrategyDelegate createDelegate, IInjectionContext context)
        {
            var initialStorage = InternalScopedStorage;

            if (ReferenceEquals(initialStorage, ScopedStorage.Empty))
            {
                return CreateAndSaveScopedService<T>(initialStorage, id, createDelegate, context);
            }

            if (initialStorage.Id == id)
            {
                return (T)initialStorage.ScopedService;
            }

            var storage = initialStorage.Next;

            while (!ReferenceEquals(storage, ScopedStorage.Empty))
            {
                if (storage.Id == id)
                {
                    return (T)storage.ScopedService;
                }

                storage = storage.Next;
            }

            return CreateAndSaveScopedService<T>(initialStorage, id, createDelegate, context);
        }

        private T CreateAndSaveScopedService<T>(ScopedStorage initialStorage, int id,
            ActivationStrategyDelegate createDelegate, IInjectionContext context)
        {
            var value = createDelegate(this, this, context);

            var newStorage = new ScopedStorage { Id = id, Next = initialStorage, ScopedService = value };

            if (Interlocked.CompareExchange(ref InternalScopedStorage, newStorage, initialStorage) == initialStorage)
            {
                return (T)value;
            }

            return HandleScopedStorageCollision<T>(initialStorage, id, newStorage, value);
        }

        private T HandleScopedStorageCollision<T>(ScopedStorage initialStorage, int id, ScopedStorage newStorage, object value)
        {
            SpinWait spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref InternalScopedStorage, newStorage, initialStorage) != initialStorage)
            {
                var current = InternalScopedStorage;

                while (!ReferenceEquals(current, ScopedStorage.Empty))
                {
                    if (current.Id == id)
                    {
                        return (T)current.ScopedService;
                    }

                    current = current.Next;
                }

                initialStorage = InternalScopedStorage;
                newStorage.Next = initialStorage;

                spinWait.SpinOnce();
            }

            return (T)value;
        }

        #endregion

        #region Non public members

        /// <summary>Creates a new configuration object</summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        protected static IInjectionScopeConfiguration CreateConfiguration(Action<InjectionScopeConfiguration> configuration)
        {
            var configurationObject = new InjectionScopeConfiguration();

            configuration?.Invoke(configurationObject);

            return configurationObject;
        }

        /// <summary>Create an injection context from extra data</summary>
        /// <param name="type"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        protected virtual IInjectionContext CreateInjectionContextFromExtraData(Type type, object extraData) => CreateContext(extraData);

        private object InternalLocate(IExportLocatorScope scope, IDisposalScope disposalScope, Type type, ActivationStrategyFilter consider, object key, IInjectionContext injectionContext, bool allowNull, bool isDynamic)
        {
            if (type == typeof(ILocatorService) || type == typeof(IExportLocatorScope))
            {
                return scope;
            }

            if (isDynamic)
            {
                if (type.IsArray)
                {
                    return DynamicArray(scope, disposalScope, type, consider, injectionContext);
                }

#if NET40
                if (type.IsConstructedGenericType() && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
#else
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
#endif
                {
                    return DynamicIEnumerable(scope, disposalScope, type, consider, injectionContext);
                }
            }

            var compiledDelegate = InternalFieldStorage.ActivationStrategyCompiler.FindDelegate(this, type, consider, key, injectionContext, InternalFieldStorage.MissingExportStrategyProviders != ImmutableLinkedList<IMissingExportStrategyProvider>.Empty);

            if (compiledDelegate != null)
            {
                if (key == null && consider == null)
                {
                    compiledDelegate = AddObjectFactory(type, compiledDelegate);
                }

                return compiledDelegate(scope, disposalScope ?? scope, injectionContext);
            }

            if (Parent != null)
            {
                var injectionScopeParent = (IInjectionScope)Parent;

                return injectionScopeParent.LocateFromChildScope(scope, disposalScope, type, injectionContext, consider, key, allowNull, isDynamic);
            }

            if (type == typeof(IInjectionScope) && ScopeConfiguration.Behaviors.AllowInjectionScopeLocation)
            {
                return scope;
            }

            var value = ScopeConfiguration.Implementation.Locate<IInjectionContextValueProvider>()
                .GetValueFromInjectionContext(scope, type, null, injectionContext, !allowNull);

            if (value != null)
            {
                return value;
            }

            if (!allowNull) { ThrowLocateException(type); }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowLocateException(Type type)
        {
            throw GetLocateException();
            LocateException GetLocateException()
            {
                throw new LocateException(new StaticInjectionContext(type));
            }
        }

        private object DynamicIEnumerable(IExportLocatorScope scope, IDisposalScope disposalScope, Type type, ActivationStrategyFilter consider, IInjectionContext injectionContext)
        {
            if (InternalFieldStorage.DynamicIEnumerableLocator == null)
            {
                Interlocked.CompareExchange(ref InternalFieldStorage.DynamicIEnumerableLocator,
                    ScopeConfiguration.Implementation.Locate<IDynamicIEnumerableLocator>(), null);
            }

            return InternalFieldStorage.DynamicIEnumerableLocator.Locate(this, scope, disposalScope, type, consider, injectionContext);
        }

        private object DynamicArray(IExportLocatorScope scope, IDisposalScope disposalScope, Type type, ActivationStrategyFilter consider, IInjectionContext injectionContext)
        {
            if (InternalFieldStorage.DynamicArrayLocator == null)
            {
                Interlocked.CompareExchange(ref InternalFieldStorage.DynamicArrayLocator,
                    ScopeConfiguration.Implementation.Locate<IDynamicArrayLocator>(), null);
            }

            return InternalFieldStorage.DynamicArrayLocator.Locate(this, scope, disposalScope, type, consider, injectionContext);
        }

        private ActivationStrategyDelegate AddObjectFactory(Type type, ActivationStrategyDelegate activationStrategyDelegate)
        {
            DelegateCache.AddDelegate(type, activationStrategyDelegate);

            return activationStrategyDelegate;
        }

        private IActivationStrategyCollectionContainer<ICompiledWrapperStrategy> GetWrappers()
        {
            if (InternalFieldStorage.Wrappers != null)
            {
                return InternalFieldStorage.Wrappers;
            }

            var wrapperCollectionProvider = ScopeConfiguration.Implementation.Locate<IDefaultWrapperCollectionProvider>();

            if (Interlocked.CompareExchange(ref InternalFieldStorage.Wrappers, wrapperCollectionProvider.ProvideCollection(this), null) == null)
            {
                AddDisposable(InternalFieldStorage.Wrappers);
            }

            return InternalFieldStorage.Wrappers;
        }

        private void LocateEnumerablesFromStrategyCollection<TStrategy, TValue>(IActivationStrategyCollection<TStrategy> collection, IExportLocatorScope scope, IDisposalScope disposalScope,
            Type type, IInjectionContext context, ActivationStrategyFilter filter, List<TValue> returnList) where TStrategy : IWrapperOrExportActivationStrategy
        {
            foreach (var strategy in collection.GetStrategies())
            {
                ProcessStrategyForCollection(scope, disposalScope, type, context, filter, returnList, strategy);
            }

            if (InternalFieldStorage.ScopeConfiguration.ReturnKeyedInEnumerable)
            {
                foreach (var keyValuePair in collection.GetKeyedStrategies())
                {
                    ProcessStrategyForCollection(scope, disposalScope, type, context, filter, returnList, keyValuePair.Value);
                }
            }
        }

        private void ProcessStrategyForCollection<TStrategy, TValue>(IExportLocatorScope scope, IDisposalScope disposalScope,
            Type type, IInjectionContext context, ActivationStrategyFilter filter, List<TValue> returnList, TStrategy strategy)
            where TStrategy : IWrapperOrExportActivationStrategy
        {
            if (strategy.HasConditions)
            {
                var pass = true;

                foreach (var condition in strategy.Conditions)
                {
                    if (!condition.MeetsCondition(strategy, new StaticInjectionContext(type)))
                    {
                        pass = false;
                        break;
                    }
                }

                if (!pass)
                {
                    return;
                }
            }

            if (filter != null && !filter(strategy))
            {
                return;
            }

            var activationDelegate =
                strategy.GetActivationStrategyDelegate(this, InternalFieldStorage.ActivationStrategyCompiler, type);

            if (activationDelegate != null)
            {
                returnList.Add(
                    (TValue)activationDelegate(scope, disposalScope, context?.Clone()));
            }
        }

        private string DebugDisplayString => "Exports: " + StrategyCollectionContainer.GetAllStrategies().Count();

        #endregion

        #region Internal storage class

        /// <summary>Class for storing fields for injection scope, Fields that are not on the fast path are put in
        /// this class to keep the injection scope as light as possible.</summary>
        protected class InternalFieldStorageClass
        {
            /// <summary>List of member injection selectors</summary>
            public ImmutableLinkedList<IMemberInjectionSelector> MemberInjectionSelectors = ImmutableLinkedList<IMemberInjectionSelector>.Empty;

            /// <summary>List of missing dependency expression providers.</summary>
            public ImmutableLinkedList<IMissingDependencyExpressionProvider> MissingDependencyExpressionProviders = ImmutableLinkedList<IMissingDependencyExpressionProvider>.Empty;

            /// <summary>Dynamic array locator</summary>
            public IDynamicArrayLocator DynamicArrayLocator;

            /// <summary>dynamic ienumerable locator</summary>
            public IDynamicIEnumerableLocator DynamicIEnumerableLocator;

            /// <summary>Wrappers for scope</summary>
            public IActivationStrategyCollectionContainer<ICompiledWrapperStrategy> Wrappers;

            /// <summary>Value providers</summary>
            public ImmutableLinkedList<IInjectionValueProvider> ValueProviders = ImmutableLinkedList<IInjectionValueProvider>.Empty;

            /// <summary>Missing export strategy providers</summary>
            public ImmutableLinkedList<IMissingExportStrategyProvider> MissingExportStrategyProviders =
                ImmutableLinkedList<IMissingExportStrategyProvider>.Empty;

            /// <summary>activation strategy compiler</summary>
            public IActivationStrategyCompiler ActivationStrategyCompiler;

            /// <summary>Strategy collection</summary>
            public IActivationStrategyCollectionContainer<ICompiledExportStrategy> StrategyCollectionContainer;

            /// <summary>Decorators</summary>
            public IActivationStrategyCollectionContainer<ICompiledDecoratorStrategy> DecoratorCollectionContainer;

            /// <summary>Scope configuration</summary>
            public IInjectionScopeConfiguration ScopeConfiguration;

            /// <summary>Creates injection context when needed</summary>
            public IInjectionContextCreator InjectionContextCreator;

            /// <summary>Implementation to tell if a type can be located</summary>
            public ICanLocateTypeService CanLocateTypeService;
        }

        #endregion
    }
}