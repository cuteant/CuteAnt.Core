﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Runtime.Startup
{
  public class ConfigureContainerBuilder
  {
    public ConfigureContainerBuilder(MethodInfo configureContainerMethod)
    {
      MethodInfo = configureContainerMethod;
    }

    public MethodInfo MethodInfo { get; }

    public Action<object> Build(object instance) => container => Invoke(instance, container);

    public Type GetContainerType()
    {
      var parameters = MethodInfo.GetParameters();
      if (parameters.Length != 1)
      {
        // REVIEW: This might be a breaking change
        throw new InvalidOperationException($"The {MethodInfo.Name} method must take only one parameter.");
      }
      return parameters[0].ParameterType;
    }

    private void Invoke(object instance, object container)
    {
      if (MethodInfo == null)
      {
        return;
      }

      var arguments = new object[1] { container };

      MethodInfo.Invoke(instance, arguments);
    }
  }
}