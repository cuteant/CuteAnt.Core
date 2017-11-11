using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CuteAnt.Runtime.Providers;
using CuteAnt.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Runtime.Startup
{
  /// <summary>Configure dependency injection at startup</summary>
  public class ConfigureStartupBuilder
  {
    private static readonly Func<IServiceCollection, IServiceProvider> DefaultBuildServiceProvider = s =>
    {
      s.AddLogging();
      return s.BuildServiceProvider();
    };

    public static IServiceProvider ConfigureStartup(string startupTypeName, string environmentName = null, string serviceProviderFactoryTypeName = null)
    {
      if (string.IsNullOrWhiteSpace(startupTypeName))
      {
        return DefaultServiceProvider.Empty;
      }
      if (string.IsNullOrWhiteSpace(environmentName)) { environmentName = "Development"; }

      var startupType = TypeUtils.ResolveType(startupTypeName);

      if (startupType == null)
      {
        throw new InvalidOperationException($"Can not locate the type specified in the configuration file: '{startupTypeName}'.");
      }

      var servicesBuilder = FindConfigureServicesDelegate(startupType, environmentName);
      var containerBuilder = FindConfigureContainerDelegate(startupType, environmentName);

      object instance = null;
      if ((servicesBuilder != null && !servicesBuilder.MethodInfo.IsStatic))
      {
        instance = Activator.CreateInstance(startupType);
      }

      var serviceCollection = new ServiceCollection();
      var servicesCallback = servicesBuilder?.Build(instance) ?? DefaultBuildServiceProvider;
      var applicationServiceProvider = servicesCallback.Invoke(serviceCollection) as IServiceProvider;
      if (applicationServiceProvider == null)
      {
        if (containerBuilder != null && !string.IsNullOrWhiteSpace(serviceProviderFactoryTypeName))
        {
          var containerCallback = containerBuilder.Build(instance);
          // We have a ConfigureContainer method, get the IServiceProviderFactory<TContainerBuilder>
          //var serviceProviderFactoryType = typeof(IServiceProviderFactory<>).MakeGenericType(containerBuilder.GetContainerType());
          //var serviceProviderFactory = hostingServiceProvider.GetRequiredService(serviceProviderFactoryType);
          var serviceProviderFactoryType = TypeUtils.ResolveType(serviceProviderFactoryTypeName);
          var serviceProviderFactory = serviceProviderFactoryType.CreateInstance();
          // var builder = serviceProviderFactory.CreateBuilder(services);
          var builder = serviceProviderFactoryType.GetMethod(nameof(DefaultServiceProviderFactory.CreateBuilder)).Invoke(serviceProviderFactory, new object[] { serviceCollection });
          containerCallback.Invoke(builder);
          // applicationServiceProvider = serviceProviderFactory.CreateServiceProvider(builder);
          applicationServiceProvider = (IServiceProvider)serviceProviderFactoryType.GetMethod(nameof(DefaultServiceProviderFactory.CreateServiceProvider)).Invoke(serviceProviderFactory, new object[] { builder });
        }
        else
        {
          var factory = new DefaultServiceProviderFactory();
          applicationServiceProvider = factory.CreateServiceProvider(serviceCollection);
        }
      }

      var configureBuilder = FindConfigureDelegate(startupType, environmentName);
      if (configureBuilder != null)
      {
        if (!configureBuilder.MethodInfo.IsStatic)
        {
          if (instance == null) { instance = Activator.CreateInstance(startupType); }
        }
        else
        {
          instance = null;
        }
      }
      var configureCallback = configureBuilder?.Build(instance);
      configureCallback?.Invoke(applicationServiceProvider);

      return applicationServiceProvider;
    }

    private static ConfigureBuilder FindConfigureDelegate(Type startupType, string environmentName)
    {
      var configureMethod = FindMethod(startupType, "Configure{0}", environmentName, typeof(void), required: true);
      return configureMethod == null ? null : new ConfigureBuilder(configureMethod);
    }

    private static ConfigureContainerBuilder FindConfigureContainerDelegate(Type startupType, string environmentName)
    {
      var configureMethod = FindMethod(startupType, "Configure{0}Container", environmentName, typeof(void), required: false);
      return configureMethod == null ? null : new ConfigureContainerBuilder(configureMethod);
    }

    private static ConfigureServicesBuilder FindConfigureServicesDelegate(Type startupType, string environmentName)
    {
      var servicesMethod = FindMethod(startupType, "Configure{0}Services", environmentName, typeof(IServiceProvider), required: false)
          ?? FindMethod(startupType, "Configure{0}Services", environmentName, typeof(void), required: false);
      return servicesMethod == null ? null : new ConfigureServicesBuilder(servicesMethod);
    }

    private static MethodInfo FindMethod(Type startupType, string methodName, string environmentName, Type returnType = null, bool required = true)
    {
      var methodNameWithEnv = string.Format(CultureInfo.InvariantCulture, methodName, environmentName);
      var methodNameWithNoEnv = string.Format(CultureInfo.InvariantCulture, methodName, "");

      var methods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
      var selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithEnv)).ToList();
      if (selectedMethods.Count > 1)
      {
        throw new InvalidOperationException(string.Format("Having multiple overloads of method '{0}' is not supported.", methodNameWithEnv));
      }
      if (selectedMethods.Count == 0)
      {
        selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithNoEnv)).ToList();
        if (selectedMethods.Count > 1)
        {
          throw new InvalidOperationException(string.Format("Having multiple overloads of method '{0}' is not supported.", methodNameWithNoEnv));
        }
      }

      var methodInfo = selectedMethods.FirstOrDefault();
      if (methodInfo == null)
      {
        if (required)
        {
          throw new InvalidOperationException(string.Format("A public method named '{0}' or '{1}' could not be found in the '{2}' type.",
              methodNameWithEnv,
              methodNameWithNoEnv,
              startupType.FullName));

        }
        return null;
      }
      if (returnType != null && methodInfo.ReturnType != returnType)
      {
        if (required)
        {
          throw new InvalidOperationException(string.Format("The '{0}' method in the type '{1}' must have a return type of '{2}'.",
              methodInfo.Name,
              startupType.FullName,
              returnType.Name));
        }
        return null;
      }
      return methodInfo;
    }
  }
}
