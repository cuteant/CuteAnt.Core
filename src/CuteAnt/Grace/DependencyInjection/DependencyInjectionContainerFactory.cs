using System;
using Grace.Dynamic;

namespace Grace.DependencyInjection
{
  public class DefaultDependencyInjectionContainerFactory : DependencyInjectionContainerFactory<InjectionScopeConfiguration>
  {
    public DefaultDependencyInjectionContainerFactory(bool implementingSingleton = false) : base(implementingSingleton) { }
  }
  public class DynamicDependencyInjectionContainerFactory : DependencyInjectionContainerFactory<GraceDynamicMethod>
  {
    public DynamicDependencyInjectionContainerFactory(bool implementingSingleton = false) : base(implementingSingleton) { }
  }

  public abstract class DependencyInjectionContainerFactory<TInjectionScopeConfiguration> : IDependencyInjectionContainerFactory<TInjectionScopeConfiguration>
    where TInjectionScopeConfiguration : IInjectionScopeConfiguration, new()
  {
    private readonly bool _implementingSingleton;
    public DependencyInjectionContainerFactory(bool implementingSingleton = false) => _implementingSingleton = implementingSingleton;

    public virtual TInjectionScopeConfiguration CreateConfiguration() => new TInjectionScopeConfiguration();

    public virtual TInjectionScopeConfiguration ConfigureContainerBehavior(TInjectionScopeConfiguration configuration)
    {
      if (null == configuration) { throw new ArgumentNullException(nameof(configuration)); }
      configuration.Behaviors.AllowInjectionScopeLocation = true;
      return configuration;
    }

    public IInjectionScope CreateContainer(TInjectionScopeConfiguration configuration)
    {
      if (null == configuration) { throw new ArgumentNullException(nameof(configuration)); }

      if (!_implementingSingleton) { return new DependencyInjectionContainer(configuration); }

      DependencyInjectionContainer.ConfigureSingleton(configuration);
      return DependencyInjectionContainer.Singleton;
    }
  }
}
