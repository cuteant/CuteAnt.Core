﻿using System.Reflection;
using Grace.DependencyInjection.Attributes.Interfaces;

namespace Grace.DependencyInjection.Impl
{
    /// <summary>Process attributes on strategy</summary>
    public class ActivationStrategyAttributeProcessor : IActivationStrategyAttributeProcessor
    {
        /// <summary>Process attribute on strategy</summary>
        /// <param name="strategy">activation strategy</param>
        public void ProcessAttributeForConfigurableActivationStrategy(IConfigurableActivationStrategy strategy)
        {
            ProcessClassAttributes(strategy);

            ProcessFields(strategy);

            ProcessProperties(strategy);

            ProcessMethods(strategy);

            ProcessConstructors(strategy);
        }

        private void ProcessConstructors(IConfigurableActivationStrategy strategy)
        {
            foreach (var constructorInfo in strategy.ActivationType.GetTypeInfo().DeclaredConstructors)
            {
                if (constructorInfo.IsPublic)
                {
                    foreach (var customAttribute in constructorInfo.GetCustomAttributes())
                    {
                        if (customAttribute is IImportAttribute importAttribute)
                        {
                            var importInfo = importAttribute.ProvideImportInfo(strategy.ActivationType,
                                strategy.ActivationType.Name);

                            if (importInfo is not null)
                            {
                                strategy.SelectedConstructor = constructorInfo;

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void ProcessMethods(IConfigurableActivationStrategy strategy)
        {
            foreach (var methodInfo in strategy.ActivationType.GetRuntimeMethods())
            {
                if (!methodInfo.IsPublic || methodInfo.IsStatic) { continue; }

                foreach (var attribute in methodInfo.GetCustomAttributes())
                {
                    var importAttribute = attribute as IImportAttribute;

                    var importInfo = importAttribute?.ProvideImportInfo(strategy.ActivationType, methodInfo.Name);

                    if (importInfo != null)
                    {
                        strategy.MethodInjectionInfo(new MethodInjectionInfo { Method = methodInfo });
                    }
                }
            }
        }

        private void ProcessProperties(IConfigurableActivationStrategy strategy)
        {
            foreach (var propertyInfo in strategy.ActivationType.GetRuntimeProperties())
            {
                var setMethodInfo = propertyInfo.SetMethod;
                if (!propertyInfo.CanWrite ||
                    !setMethodInfo.IsPublic ||
                     setMethodInfo.IsStatic)
                {
                    continue;
                }

                foreach (var attribute in propertyInfo.GetCustomAttributes())
                {
                    var importAttr = attribute as IImportAttribute;

                    var importInfo = importAttr?.ProvideImportInfo(strategy.ActivationType, propertyInfo.Name);

                    if (importInfo != null)
                    {
                        var name = propertyInfo.Name;

                        strategy.MemberInjectionSelector(new PropertyFieldInjectionSelector(propertyInfo.PropertyType, m => m.Name == name, false) { IsRequired = importInfo.IsRequired, LocateKey = importInfo.ImportKey });
                    }
                }
            }
        }

        private void ProcessFields(IConfigurableActivationStrategy strategy)
        {
            foreach (var fieldInfo in strategy.ActivationType.GetRuntimeFields())
            {
                if (!fieldInfo.IsPublic || fieldInfo.IsStatic) { continue; }

                foreach (var attribute in fieldInfo.GetCustomAttributes())
                {
                    var importAttr = attribute as IImportAttribute;

                    var importInfo = importAttr?.ProvideImportInfo(strategy.ActivationType, fieldInfo.Name);

                    if (importInfo != null)
                    {
                        var name = fieldInfo.Name;

                        strategy.MemberInjectionSelector(new PropertyFieldInjectionSelector(fieldInfo.FieldType, info => info.Name == name, false) { IsRequired = importInfo.IsRequired, LocateKey = importInfo.ImportKey });
                    }
                }
            }
        }

        private void ProcessClassAttributes(IConfigurableActivationStrategy strategy)
        {
            foreach (var attribute in strategy.ActivationType.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case IExportAttribute exportAttribute:
                        var types = exportAttribute.ProvideExportTypes(strategy.ActivationType);
                        if (types != null)
                        {
                            foreach (var type in types)
                            {
                                strategy.AddExportAs(type);
                            }
                        }
                        break;

                    case IExportConditionAttribute conditionAttribute:
                        var condition = conditionAttribute.ProvideCondition(strategy.ActivationType);
                        if (condition != null)
                        {
                            strategy.AddCondition(condition);
                        }
                        break;

                    case IExportKeyedTypeAttribute keyedTypeAttribute:
                        var tuple = keyedTypeAttribute.ProvideKey(strategy.ActivationType);
                        if (tuple != null)
                        {
                            strategy.AddExportAsKeyed(tuple.Item1, tuple.Item2);
                        }
                        break;

                    case IExportPriorityAttribute priorityAttribute:
                        strategy.Priority = priorityAttribute.ProvidePriority(strategy.ActivationType);
                        break;
                }
            }
        }
    }
}