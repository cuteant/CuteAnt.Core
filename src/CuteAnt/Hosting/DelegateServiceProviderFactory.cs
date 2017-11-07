using System;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Hosting
{
  internal class DelegateServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
  {
    private readonly Func<IServiceCollection, IServiceProvider> _containerBuilder;

    public DelegateServiceProviderFactory(Func<IServiceCollection, IServiceProvider> containerBuilder)
    {
      _containerBuilder = containerBuilder;
    }

    /// <inheritdoc />
    public IServiceCollection CreateBuilder(IServiceCollection services) => services;

    /// <inheritdoc />
    public IServiceProvider CreateServiceProvider(IServiceCollection services) => _containerBuilder(services);
  }
}