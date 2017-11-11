using System;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Runtime.Providers
{
  /// <summary>Default service provider.
  /// This should be replaced with a minimal Dependency Injection system, once a stable version is available.</summary>
  public class DefaultServiceProvider : IServiceProvider
  {
    /// <summary>Empty service provider.</summary>
    public static readonly IServiceProvider Empty;
    /// <summary>Default service provider.</summary>
    public static readonly IServiceProvider Instance = new DefaultServiceProvider();

    static DefaultServiceProvider()
    {
      var services = new ServiceCollection();
      services.AddLogging();
      Empty = services.BuildServiceProvider();
    }

    public object GetService(Type serviceType)
    {
      return Activator.CreateInstance(serviceType);
    }
  }
}
