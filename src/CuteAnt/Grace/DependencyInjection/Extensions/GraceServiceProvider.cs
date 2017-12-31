using System;
using Microsoft.Extensions.DependencyInjection;

namespace Grace.DependencyInjection.Extensions
{
  /// <summary>Service provider for Grace</summary>
  internal sealed class GraceServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
  {
    private readonly IExportLocatorScope _injectionScope;

    /// <summary>Default constructor</summary>
    /// <param name="injectionScope"></param>
    public GraceServiceProvider(IExportLocatorScope injectionScope) => _injectionScope = injectionScope;

    /// <summary>Gets service of type <paramref name="serviceType"/> from the <see
    /// cref="T:System.IServiceProvider"/> implementing this interface.</summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType"/>. Throws an exception if the <see
    /// cref="T:System.IServiceProvider"/> cannot create the object.</returns>
    public object GetRequiredService(Type serviceType) => _injectionScope.Locate(serviceType);

    /// <summary>Gets the service object of the specified type.</summary>
    /// <returns>A service object of type <paramref name="serviceType"/>.-or- null if there is no service
    /// object of type <paramref name="serviceType"/>.</returns>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <filterpriority>2</filterpriority>
    public object GetService(Type serviceType) => _injectionScope.LocateOrDefault(serviceType);

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => _injectionScope.Dispose();
  }
}
