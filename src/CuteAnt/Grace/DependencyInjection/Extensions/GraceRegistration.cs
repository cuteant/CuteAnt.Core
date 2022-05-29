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
            if (exportLocator is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.exportLocator); }

            exportLocator.Configure(c =>
            {
#if NET6_0_OR_GREATER
                c.Export<ServiceProviderIsServiceImpl>().As<IServiceProviderIsService>();
#endif

                c.ExcludeTypeFromAutoRegistration(nameof(Microsoft) + ".*");
                c.Export<GraceServiceProvider>().As<IServiceProvider>().ExternallyOwned();
                c.Export<GraceLifetimeScopeServiceScopeFactory>().As<IServiceScopeFactory>().Lifestyle.Singleton();
                Register(c, descriptors);
            });

            return exportLocator/*.Locate<IServiceProvider>()*/;
        }

        private static void Register(IExportRegistrationBlock c, IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType is not null)
                {
                    c.Export(descriptor.ImplementationType).As(descriptor.ServiceType).ConfigureLifetime(descriptor.Lifetime);
                }
                else if (descriptor.ImplementationFactory is not null)
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

                default:
                    return configuration;
            }
        }

        private static IFluentExportInstanceConfiguration<T> ConfigureLifetime<T>(this IFluentExportInstanceConfiguration<T> configuration, ServiceLifetime lifecycleKind)
        {
            switch (lifecycleKind)
            {
                case ServiceLifetime.Scoped:
                    return configuration.Lifestyle.SingletonPerScope();

                case ServiceLifetime.Singleton:
                    return configuration.Lifestyle.Singleton();

                default:
                    return configuration;
            }
        }
    }

#if NET6_0_OR_GREATER
    sealed class ServiceProviderIsServiceImpl : IServiceProviderIsService
    {
        private readonly IExportLocatorScope _exportLocatorScope;

        public ServiceProviderIsServiceImpl(IExportLocatorScope exportLocatorScope)
        {
            _exportLocatorScope = exportLocatorScope;
        }

        public bool IsService(Type serviceType)
        {
            if (serviceType.IsGenericTypeDefinition)
            {
                return false;
            }

            return _exportLocatorScope.CanLocate(serviceType);
        }
    }
#endif
}