using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Hosting
{
  /// <summary>Adapts an <see cref="IServiceProviderFactory{TContainerBuilder}"/> into an <see cref="IServiceProviderFactoryAdapter"/>.</summary>
  /// <typeparam name="TContainerBuilder">The container builder type.</typeparam>
  internal class ServiceProviderFactoryAdapter<TContainerBuilder> : IServiceProviderFactoryAdapter
  {
    private readonly IServiceProviderFactory<TContainerBuilder> _serviceProviderFactory;
    private readonly List<Action<HostBuilderContext, TContainerBuilder>> _configureContainerDelegates = new List<Action<HostBuilderContext, TContainerBuilder>>();

    public ServiceProviderFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
    {
      _serviceProviderFactory = serviceProviderFactory;
    }

    /// <inheritdoc />
    public IServiceProvider BuildServiceProvider(HostBuilderContext context, IServiceCollection services)
    {
      var builder = _serviceProviderFactory.CreateBuilder(services);

      foreach (var configureContainer in _configureContainerDelegates)
      {
        configureContainer(context, builder);
      }

      return _serviceProviderFactory.CreateServiceProvider(builder);
    }

    /// <inheritdoc />
    public void ConfigureContainer<TBuilder>(Action<HostBuilderContext, TBuilder> configureContainer)
    {
      if (configureContainer == null) throw new ArgumentNullException(nameof(configureContainer));
      var typedDelegate = configureContainer as Action<HostBuilderContext, TContainerBuilder>;
      if (typedDelegate == null)
      {
        var msg = $"Type of configuration delegate requires builder of type {typeof(TBuilder)} which does not match previously configured container builder type {typeof(TContainerBuilder)}.";
        throw new InvalidCastException(msg);
      }

      _configureContainerDelegates.Add(typedDelegate);
    }
  }
}