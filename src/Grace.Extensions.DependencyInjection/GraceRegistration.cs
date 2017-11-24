using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Grace.DependencyInjection.Extensions
{
  /// <summary>static class for MVC registration</summary>
  public static class GraceRegistration
  {
    /// <summary>Populate a container with service descriptors</summary>
    /// <param name="exportLocator">export locator</param>
    /// <param name="descriptors">descriptors</param>
    public static IServiceProvider Populate(this IInjectionScope exportLocator, IEnumerable<ServiceDescriptor> descriptors)
    {
      exportLocator.Configure(c =>
      {
        c.Export<GraceServiceProvider>().As<IServiceProvider>().ExternallyOwned();
        c.Export<GraceLifetimeScopeServiceScopeFactory>().As<IServiceScopeFactory>();
        Register(c, descriptors);
      });

      return exportLocator.Locate<IServiceProvider>();
    }

    private static void Register(IExportRegistrationBlock c, IEnumerable<ServiceDescriptor> descriptors)
    {
      foreach (var descriptor in descriptors)
      {
        if (descriptor.ImplementationType != null)
        {
          c.Export(descriptor.ImplementationType).As(descriptor.ServiceType).ConfigureLifetime(descriptor.Lifetime);
        }
        else if (descriptor.ImplementationFactory != null)
        {
          c.ExportFactory(descriptor.ImplementationFactory).As(descriptor.ServiceType).ConfigureLifetime(descriptor.Lifetime);
        }
        else
        {
          c.ExportInstance(descriptor.ImplementationInstance).As(descriptor.ServiceType).ConfigureLifetime(descriptor.Lifetime);
        }
      }
    }

    private static IFluentExportStrategyConfiguration ConfigureLifetime(this IFluentExportStrategyConfiguration configuration, ServiceLifetime lifetime)
    {
      switch (lifetime)
      {
        case ServiceLifetime.Scoped:
          return configuration.Lifestyle.SingletonPerScope();

        case ServiceLifetime.Singleton:
          return configuration.Lifestyle.Singleton();
      }

      return configuration;
    }

    private static IFluentExportInstanceConfiguration<T> ConfigureLifetime<T>(this IFluentExportInstanceConfiguration<T> configuration, ServiceLifetime lifecycleKind)
    {
      switch (lifecycleKind)
      {
        case ServiceLifetime.Scoped:
          return configuration.Lifestyle.SingletonPerScope();

        case ServiceLifetime.Singleton:
          return configuration.Lifestyle.Singleton();
      }

      return configuration;
    }
  }
}