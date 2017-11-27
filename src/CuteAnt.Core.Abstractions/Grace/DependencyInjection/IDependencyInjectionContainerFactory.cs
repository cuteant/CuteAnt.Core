
namespace Grace.DependencyInjection
{
  public interface IDependencyInjectionContainerFactory<TInjectionScopeConfiguration>
    where TInjectionScopeConfiguration : IInjectionScopeConfiguration
  {
    TInjectionScopeConfiguration CreateConfiguration();

    TInjectionScopeConfiguration ConfigureContainerBehavior(TInjectionScopeConfiguration configuration);

    IInjectionScope CreateContainer(TInjectionScopeConfiguration containerBuilder);
  }
}
