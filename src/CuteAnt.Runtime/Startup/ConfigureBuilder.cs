// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Runtime.Startup
{
  public class ConfigureBuilder
  {
    public ConfigureBuilder(MethodInfo configure)
    {
      MethodInfo = configure;
    }

    public MethodInfo MethodInfo { get; }

    public Action<IServiceProvider> Build(object instance) => services => Invoke(instance, services);

    private void Invoke(object instance, IServiceProvider services)
    {
      var parameterInfos = MethodInfo.GetParameters();
      var parameters = new object[parameterInfos.Length];
      for (var index = 0; index < parameterInfos.Length; index++)
      {
        var parameterInfo = parameterInfos[index];
        if (parameterInfo.ParameterType == typeof(IServiceProvider))
        {
          parameters[index] = services;
        }
        else
        {
          try
          {
            parameters[index] = services.GetRequiredService(parameterInfo.ParameterType);
          }
          catch (Exception ex)
          {
            throw new Exception(string.Format(
                "Could not resolve a service of type '{0}' for the parameter '{1}' of method '{2}' on type '{3}'.",
                parameterInfo.ParameterType.FullName,
                parameterInfo.Name,
                MethodInfo.Name,
                MethodInfo.DeclaringType.FullName), ex);
          }
        }
      }
      MethodInfo.Invoke(instance, parameters);
    }
  }
}