﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CuteAnt;
using CuteAnt.Reflection;
using Microsoft.Extensions.Internal;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Class to create strongly typed instances</summary>
  public class StrongMetadataInstanceProvider : IStrongMetadataInstanceProvider
  {
    /// <summary></summary>
    /// <param name="metadataType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public virtual object GetMetadata(Type metadataType, IActivationStrategyMetadata metadata)
    {
      if (metadataType == typeof(IReadOnlyDictionary<object, object>) ||
          metadataType == typeof(IActivationStrategyMetadata))
      {
        return metadata;
      }

      if (metadataType.IsInterface)
      {
        throw new NotSupportedException("Interface metadata types not supported");
      }

      var constructorParameters = GetConstructorParameters(metadataType, metadata);

      // ## 苦竹 修改 ##
      //var instance = Activator.CreateInstance(metadataType, constructorParameters);
      var instance = ActivatorUtils.CreateInstance(metadataType, constructorParameters);

      BindPropertyValues(instance, metadata);

      return instance;
    }

    /// <summary></summary>
    /// <param name="metadataType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    protected object[] GetConstructorParameters(Type metadataType, IActivationStrategyMetadata metadata)
    {
      var constructors = metadataType.GetTypeInfo().DeclaredConstructors.ToArray();

      ConstructorInfo constructorInfo;

      if (constructors.Length == 1)
      {
        if (0u >= (uint)constructors[0].GetParameters().Length)
        {
          return EmptyArray<object>.Instance; // new object[0];
        }

        constructorInfo = constructors[0];
      }
      else
      {
        constructorInfo = constructors.OrderBy(c => c.GetParameters().Length).Last();
      }

      var parameters = constructorInfo.GetParameters();

      var constructorParameters = new object[parameters.Length];

      int index = 0;
      foreach (var parameter in parameters)
      {
        var parameterType = parameter.ParameterType;
        if (parameterType == typeof(IActivationStrategyMetadata) ||
            parameterType == typeof(IReadOnlyDictionary<object, object>))
        {
          constructorParameters[index] = metadata;
        }

        var uppercaseName = char.ToUpper(parameter.Name[0], CultureInfo.InvariantCulture).ToString();

        if (parameter.Name.Length > 1)
        {
          uppercaseName += parameter.Name.Substring(1);
        }

        if (metadata.TryGetValue(parameter.Name, out object parameterValue) ||
            metadata.TryGetValue(uppercaseName, out parameterValue))
        {
          if (parameterValue != null)
          {
            constructorParameters[index] =
                parameterType.IsAssignableFrom(parameterValue.GetType())
                    ? parameterValue
                    : ConvertValue(parameterType, parameterValue);
          }
          else
          {
            constructorParameters[index] = null;
          }
        }
        else if (ParameterDefaultValue.TryGetDefaultValue(parameter, out var defaultValue))
        {
          constructorParameters[index] = defaultValue;
        }
        else
        {
          constructorParameters[index] = null;
        }
        // ## 苦竹 修改 ##
        //else if (parameter.HasDefaultValue())
        //{
        //  constructorParameters[index] = parameter.DefaultValue;
        //}
        //else
        //{
        //  constructorParameters[index] = parameterTypeInfo.IsValueType
        //      ? ActivatorUtils.FastCreateInstance(parameterType)
        //      : null;
        //}
      }

      return constructorParameters;
    }

    /// <summary>Bind metadata values to instance</summary>
    /// <param name="instance"></param>
    /// <param name="metadata"></param>
    protected void BindPropertyValues(object instance, IActivationStrategyMetadata metadata)
    {
      foreach (var propertyInfo in instance.GetType().GetRuntimeProperties())
      {
#if NET40
        var setMethodInfo = propertyInfo.SetMethod();
#else
        var setMethodInfo = propertyInfo.SetMethod;
#endif
        if (!propertyInfo.CanWrite || setMethodInfo.IsStatic) { continue; }

        object setValue = null;

        setValue = metadata.ContainsKey(propertyInfo.Name)
                 ? metadata[propertyInfo.Name]
                 : propertyInfo.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        if (setValue != null)
        {
          if (!propertyInfo.PropertyType.IsAssignableFrom(setValue.GetType()))
          {
            setValue = ConvertValue(propertyInfo.PropertyType, setValue);
          }

          setMethodInfo.Invoke(instance, new[] { setValue });
        }
      }
    }

    protected virtual object ConvertValue(Type desiredType, object value) => Convert.ChangeType(value, desiredType);
  }
}