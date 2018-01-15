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
#if NET40
        var setMethodInfo = propertyInfo.SetMethod();
#else
        var setMethodInfo = propertyInfo.SetMethod;
#endif
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
#if NET40
      foreach (var attribute in strategy.ActivationType.GetCustomAttributes())
#else
      foreach (var attribute in strategy.ActivationType.GetTypeInfo().GetCustomAttributes())
#endif
      {
        var exportAttribute = attribute as IExportAttribute;

        var types = exportAttribute?.ProvideExportTypes(strategy.ActivationType);

        if (types != null)
        {
          foreach (var type in types)
          {
            strategy.AddExportAs(type);
          }
        }

        var conditionAttribute = attribute as IExportConditionAttribute;

        var condition = conditionAttribute?.ProvideCondition(strategy.ActivationType);

        if (condition != null)
        {
          strategy.AddCondition(condition);
        }

        var keyedTypeAttribute = attribute as IExportKeyedTypeAttribute;

        var tuple = keyedTypeAttribute?.ProvideKey(strategy.ActivationType);

        if (tuple != null)
        {
          strategy.AddExportAsKeyed(tuple.Item1, tuple.Item2);
        }
      }
    }
  }
}