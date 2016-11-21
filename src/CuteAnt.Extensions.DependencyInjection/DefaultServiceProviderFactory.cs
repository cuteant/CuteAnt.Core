using System;

namespace CuteAnt.Extensions.DependencyInjection
{
    public class DefaultServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
