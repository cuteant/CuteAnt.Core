﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grace.Data;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Attributes.Interfaces;
using Grace.DependencyInjection.Conditions;
using Grace.DependencyInjection.Impl.Expressions;
using Grace.DependencyInjection.Lifestyle;
using Grace.Utilities;

namespace Grace.DependencyInjection.Impl
{
    /// <summary>Configure a set of types for export</summary>
    public class ExportTypeSetConfiguration : IExportTypeSetConfiguration, IExportStrategyProvider
    {
        private readonly IActivationStrategyCreator _strategyCreator;
        private readonly IEnumerable<Type> _typesToExport;
        private readonly GenericFilterGroup<Type> _whereFilter;
        private readonly IInjectionScopeConfiguration _scopeConfiguration;

        private ImmutableLinkedList<Type> _basedOnTypes = ImmutableLinkedList<Type>.Empty;
        private ImmutableLinkedList<Type> _byInterface = ImmutableLinkedList<Type>.Empty;
        private ImmutableLinkedList<Func<Type, bool>> _byInterfaces = ImmutableLinkedList<Func<Type, bool>>.Empty;
        private ImmutableLinkedList<Func<Type, IEnumerable<Type>>> _byTypes = ImmutableLinkedList<Func<Type, IEnumerable<Type>>>.Empty;
        private ImmutableLinkedList<Func<Type, IEnumerable<Tuple<Type, object>>>> _byKeyedType = ImmutableLinkedList<Func<Type, IEnumerable<Tuple<Type, object>>>>.Empty;
        private ImmutableLinkedList<Func<Type, IEnumerable<ICompiledCondition>>> _conditions = ImmutableLinkedList<Func<Type, IEnumerable<ICompiledCondition>>>.Empty;
        private ImmutableLinkedList<Func<Type, bool>> _excludeFuncs = ImmutableLinkedList<Func<Type, bool>>.Empty;
        private ImmutableLinkedList<IActivationStrategyInspector> _inspectors = ImmutableLinkedList<IActivationStrategyInspector>.Empty;
        private ImmutableLinkedList<Func<Type, IEnumerable<string>>> _byName = ImmutableLinkedList<Func<Type, IEnumerable<string>>>.Empty;
        private Func<Type, ICompiledLifestyle> _lifestyleFunc;
        private bool _exportByAttributes;
        private bool _externallyOwned;
        private Func<Type, IConstructorExpressionCreator> _constructorSelectionMethod;

        /// <summary>Default constructor</summary>
        /// <param name="strategyCreator"></param>
        /// <param name="typesToExport"></param>
        /// <param name="scopeConfiguration"></param>
        public ExportTypeSetConfiguration(IActivationStrategyCreator strategyCreator, IEnumerable<Type> typesToExport, IInjectionScopeConfiguration scopeConfiguration)
        {
            _strategyCreator = strategyCreator;
            _typesToExport = typesToExport;
            _scopeConfiguration = scopeConfiguration;
            _whereFilter = new GenericFilterGroup<Type>(ShouldSkipType, ExcludeTypesFilter);
        }

        /// <summary>Add conditions for export</summary>
        /// <param name="conditionFunc"></param>
        /// <returns></returns>
        public IExportTypeSetConfiguration AndCondition(Func<Type, IEnumerable<ICompiledCondition>> conditionFunc)
        {
            if (null == conditionFunc) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.conditionFunc);

            _conditions = _conditions.Add(conditionFunc);

            return this;
        }

        /// <summary>Export all types based on speficied type by Type</summary>
        /// <param name="baseType">base type to export</param>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration BasedOn(Type baseType)
        {
            if (null == baseType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.baseType);

            _basedOnTypes = _basedOnTypes.Add(baseType);

            return this;
        }

        /// <summary>Export all types based on speficied type by Type</summary>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration BasedOn<T>()
        {
            _basedOnTypes = _basedOnTypes.Add(typeof(T));

            return this;
        }

        /// <summary>Export all objects that implements the specified interface</summary>
        /// <param name="interfaceType">interface type</param>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration ByInterface(Type interfaceType)
        {
            if (null == interfaceType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.interfaceType);

            _byInterface = _byInterface.Add(interfaceType);

            return this;
        }

        /// <summary>Export all objects that implements the specified interface</summary>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration ByInterface<T>()
        {
            _byInterface = _byInterface.Add(typeof(T));

            return this;
        }

        private static readonly Func<Type, bool> s_whereClause = type => true;
        /// <summary>Export all classes by interface or that match a set of interfaces</summary>
        /// <param name="whereClause">where clause to test if the interface should be used for exporting</param>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration ByInterfaces(Func<Type, bool> whereClause = null)
        {
            if (whereClause == null)
            {
                whereClause = s_whereClause;
            }

            _byInterfaces = _byInterfaces.Add(whereClause);

            return this;
        }

        private static readonly Func<Type, IEnumerable<string>> s_nameFunc = type => new[] { type.Name };
        /// <summary>Export by name</summary>
        /// <param name="nameFunc"></param>
        /// <returns></returns>
        public IExportTypeSetConfiguration ByName(Func<Type, IEnumerable<string>> nameFunc = null)
        {
            if (nameFunc == null)
            {
                nameFunc = s_nameFunc;
            }

            _byName = _byName.Add(nameFunc);

            return this;
        }

        /// <summary>Export the selected classes by type</summary>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration ByType() => ByTypes(t => new[] { t });

        /// <summary>Exports by a set of types</summary>
        /// <param name="typeDelegate"></param>
        /// <returns></returns>
        public IExportTypeSetConfiguration ByTypes(Func<Type, IEnumerable<Type>> typeDelegate)
        {
            if (null == typeDelegate) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeDelegate);

            _byTypes = _byTypes.Add(typeDelegate);

            return this;
        }

        /// <summary>Export a type by a set of keyed types</summary>
        /// <param name="keyedDelegate">keyed types</param>
        /// <returns></returns>
        public IExportTypeSetConfiguration ByKeyedTypes(Func<Type, IEnumerable<Tuple<Type, object>>> keyedDelegate)
        {
            if (null == keyedDelegate) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keyedDelegate);

            _byKeyedType = _byKeyedType.Add(keyedDelegate);

            return this;
        }

        /// <summary>Exclude a type from being used</summary>
        /// <param name="exclude">exclude delegate</param>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration Exclude(Func<Type, bool> exclude)
        {
            if (null == exclude) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.exclude);

            _excludeFuncs = _excludeFuncs.Add(exclude);

            return this;
        }

        /// <summary>Export types using their attributes</summary>
        /// <returns></returns>
        public IExportTypeSetConfiguration ExportAttributedTypes()
        {
            _exportByAttributes = true;

            return this;
        }

        /// <summary>Set constructor selection method for individual exports</summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public IExportTypeSetConfiguration ImportConstructorSelection(Func<Type, IConstructorExpressionCreator> method)
        {
            if (null == method) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method);
            _constructorSelectionMethod = method;

            return this;
        }

        /// <summary>Lifestyle for set</summary>
        public ILifestylePicker<IExportTypeSetConfiguration> Lifestyle
            => new LifestylePicker<IExportTypeSetConfiguration>(this, lifestyle => UsingLifestyle(lifestyle));

        /// <summary>Set a particular life style</summary>
        /// <param name="lifestyle">lifestyle</param>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration UsingLifestyle(ICompiledLifestyle lifestyle) => UsingLifestyle(type => lifestyle?.Clone());

        /// <summary>Set a particular life style using a func</summary>
        /// <param name="lifestyleFunc">pick a lifestyle</param>
        /// <returns>configuration object</returns>
        public IExportTypeSetConfiguration UsingLifestyle(Func<Type, ICompiledLifestyle> lifestyleFunc)
        {
            if (null == lifestyleFunc) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.lifestyleFunc);
            _lifestyleFunc = lifestyleFunc;

            return this;
        }

        /// <summary>Export only types that match the filter provided</summary>
        /// <param name="typeFilter"></param>
        /// <returns></returns>
        public IExportTypeSetConfiguration Where(Func<Type, bool> typeFilter)
        {
            if (null == typeFilter) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeFilter);

            _whereFilter.Add(typeFilter);

            return this;
        }

        /// <summary>Add inspector for type set</summary>
        /// <param name="inspector"></param>
        /// <returns></returns>
        public IExportTypeSetConfiguration WithInspector(IActivationStrategyInspector inspector)
        {
            if (null == inspector) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inspector);

            _inspectors = _inspectors.Add(inspector);

            return this;
        }

        /// <summary>Mark all types as externally owned</summary>
        /// <returns></returns>
        public IExportTypeSetConfiguration ExternallyOwned()
        {
            _externallyOwned = true;

            return this;
        }

        /// <summary>Add condition to exports</summary>
        public IWhenConditionConfiguration<IExportTypeSetConfiguration> When
        {
            get
            {
                return new WhenConditionConfiguration<IExportTypeSetConfiguration>(
                    condition => _conditions = _conditions.Add(t => new[] { condition }), this);
            }
        }

        /// <summary>Get export strategies</summary>
        /// <returns>list of exports</returns>
        public IEnumerable<ICompiledExportStrategy> ProvideExportStrategies()
        {
            if (_basedOnTypes != ImmutableLinkedList<Type>.Empty)
            {
                _whereFilter.Add(BasedOnTypesFilter);
            }

            var types = _typesToExport.Where(_whereFilter).ToArray();

            return CreateExportStrategiesForTypes(types);
        }

        private bool BasedOnTypesFilter(Type type)
        {
            foreach (var basedOnType in _basedOnTypes)
            {
                if (ReflectionService.CheckTypeIsBasedOnAnotherType(type, basedOnType))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<ICompiledExportStrategy> CreateExportStrategiesForTypes(Type[] types)
        {
            foreach (var type in types)
            {
                if (!type.GetTypeInfo().DeclaredConstructors.Any(c => c.IsPublic && !c.IsStatic && !c.IsAbstract))
                {
                    continue;
                }

                var exportTypes = GetExportedTypes(type);
                var keyedExports = GetKeyedExportTypes(type);
                var names = GetExportNames(type);

                if (_exportByAttributes)
                {
                    foreach (var attribute in type.GetCustomAttributes())
                    {
                        if (attribute is IExportAttribute exportAttribute)
                        {
                            exportTypes = exportTypes.AddRange(exportAttribute.ProvideExportTypes(type));
                        }

                        var value = (attribute as IExportKeyedTypeAttribute)?.ProvideKey(type);

                        if (value is not null)
                        {
                            keyedExports = keyedExports.Add(value);
                        }

                        if (attribute is IExportStrategyProviderAttribute providerAttribute)
                        {
                            yield return providerAttribute.ProvideStrategy(type, _strategyCreator);
                        }
                    }
                }

                if (exportTypes != ImmutableLinkedList<Type>.Empty ||
                    keyedExports != ImmutableLinkedList<Tuple<Type, object>>.Empty ||
                    names != ImmutableLinkedList<string>.Empty)
                {
                    yield return CreateExportStrategyForType(type, exportTypes, keyedExports, names);
                }
            }
        }

        private ImmutableLinkedList<string> GetExportNames(Type type)
        {
            var returnList = ImmutableLinkedList<string>.Empty;

            foreach (var func in _byName)
            {
                foreach (var name in func(type))
                {
                    if (name != null) { returnList = returnList.Add(name); }
                }
            }

            return returnList;
        }

        private ICompiledExportStrategy CreateExportStrategyForType(Type type, ImmutableLinkedList<Type> exportTypes, ImmutableLinkedList<Tuple<Type, object>> keyedExports, ImmutableLinkedList<string> names)
        {
            var strategy = _strategyCreator.GetCompiledExportStrategy(type);

            foreach (var exportType in exportTypes)
            {
                strategy.AddExportAs(exportType);
            }

            foreach (var keyedExport in keyedExports)
            {
                strategy.AddExportAsKeyed(keyedExport.Item1, keyedExport.Item2);
            }

            foreach (var name in names)
            {
                strategy.AddExportAsName(name);
            }

            strategy.Lifestyle = _lifestyleFunc?.Invoke(type);

            _inspectors.Visit(i => i.Inspect(strategy), true);

            if (_exportByAttributes) { strategy.ProcessAttributeForStrategy(); }

            _conditions.Visit(func =>
            {
                foreach (var condition in func(type))
                {
                    strategy.AddCondition(condition);
                }
            }, true);

            strategy.ExternallyOwned = _externallyOwned;

            if (_exportByAttributes)
            {
                ProcessAttributes(type, strategy);
            }

            strategy.ConstructorSelectionMethod = _constructorSelectionMethod?.Invoke(type);

            return strategy;
        }

        private void ProcessAttributes(Type type, ICompiledExportStrategy strategy)
        {
            foreach (var customAttribute in type.GetCustomAttributes())
            {
                switch (customAttribute)
                {
                    case ILifestyleProviderAttribute lifestyleAttribute:
                        strategy.Lifestyle = lifestyleAttribute.ProvideLifestyle(type);
                        break;
                    case IExportConditionAttribute exportConditionAttribute:
                        var condition = exportConditionAttribute.ProvideCondition(type);
                        if (condition != null)
                        {
                            strategy.AddCondition(condition);
                        }
                        break;
                    case IExportMetadataAttribute exportMetadataAttribute:
                        var metadata = exportMetadataAttribute.ProvideMetadata(type);
                        if (metadata != null)
                        {
                            foreach (var keyValuePair in metadata)
                            {
                                strategy.SetMetadata(keyValuePair.Key, keyValuePair.Value);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (var property in type.GetRuntimeProperties())
            {
                foreach (var attribute in property.GetCustomAttributes())
                {
                    if (attribute is IImportAttribute importAttribute)
                    {
                        var injecitonInfo = importAttribute.ProvideImportInfo(property.PropertyType, property.Name);

                        if (injecitonInfo != null)
                        {
                            strategy.MemberInjectionSelector(new KnownMemberInjectionSelector(
                                    new MemberInjectionInfo
                                    {
                                        MemberInfo = property,
                                        IsRequired = injecitonInfo.IsRequired,
                                        LocateKey = injecitonInfo.ImportKey
                                    }));
                        }
                    }
                }
            }

            foreach (var method in type.GetRuntimeMethods())
            {
                foreach (var attribute in method.GetCustomAttributes())
                {
                    var importAttribute = attribute as IImportAttribute;

                    var injectionInfo = importAttribute?.ProvideImportInfo(null, method.Name);

                    if (injectionInfo != null)
                    {
                        strategy.MethodInjectionInfo(new MethodInjectionInfo { Method = method });
                    }
                }
            }
        }

        private ImmutableLinkedList<Tuple<Type, object>> GetKeyedExportTypes(Type type)
        {
            var returnList = ImmutableLinkedList<Tuple<Type, object>>.Empty;

            foreach (var exportFunc in _byKeyedType)
            {
                var types = exportFunc(type);

                if (types != null) { returnList = returnList.AddRange(types); }
            }

            return returnList;
        }

        private ImmutableLinkedList<Type> GetExportedTypes(Type type)
        {
            var returnList = GetExportedInterfaceList(type);

            foreach (var typeFunc in _byTypes)
            {
                var types = typeFunc(type);

                if (types != null) { returnList = returnList.AddRange(types); }
            }

            return returnList;
        }

        private ImmutableLinkedList<Type> GetExportedInterfaceList(Type type)
        {
            var returnList = ImmutableLinkedList<Type>.Empty;
            var isGeneric = type.IsGenericTypeDefinition;

            foreach (var testInterface in type.GetTypeInfo().ImplementedInterfaces)
            {
                var implementedInterface = testInterface;

                if (isGeneric)
                {
                    if (!implementedInterface.IsConstructedGenericType ||
                        implementedInterface.GenericTypeArguments.Length < type.GetTypeInfo().GenericTypeParameters.Length)
                    {
                        continue;
                    }

                    implementedInterface = implementedInterface.GetGenericTypeDefinition();
                }

                if (_scopeConfiguration.ExportByInterfaceFilter != null &&
                    _scopeConfiguration.ExportByInterfaceFilter(implementedInterface, type))
                {
                    continue;
                }

                bool found = false;

                foreach (var exportInterface in _byInterface)
                {
                    if (exportInterface.IsGenericTypeDefinition)
                    {
                        if (implementedInterface.IsGenericType &&
                            implementedInterface.GetGenericTypeDefinition() == exportInterface)
                        {
                            returnList = returnList.Add(type.IsGenericTypeDefinition ? exportInterface : implementedInterface);

                            found = true;
                        }
                    }
                    else if (exportInterface.IsAssignableFrom(implementedInterface))
                    {
                        returnList = returnList.Add(exportInterface);

                        found = true;
                    }
                }

                if (!found)
                {
                    foreach (var interfaceFunc in _byInterfaces)
                    {
                        if (interfaceFunc(implementedInterface))
                        {
                            returnList = returnList.Add(implementedInterface);
                        }
                    }
                }
            }

            return returnList;
        }

        private bool ExcludeTypesFilter(Type arg)
        {
            if (_excludeFuncs == ImmutableLinkedList<Func<Type, bool>>.Empty) { return true; }

            return !_excludeFuncs.Any(m => m(arg));
        }

        private static bool ShouldSkipType(Type exportedType)
        {
            var skipType = exportedType.IsInterface ||
                           exportedType.IsAbstract ||
                           typeof(MulticastDelegate).IsAssignableFrom(exportedType) ||
                           typeof(Exception).IsAssignableFrom(exportedType);

            return !skipType;
        }
    }
}