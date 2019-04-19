using System;

namespace Grace.DependencyInjection.Extensions
{
    /// <summary>Service provider for Grace</summary>
    internal sealed class GraceServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IExportLocatorScope _injectionScope;

        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        public GraceServiceProvider(IExportLocatorScope injectionScope) => _injectionScope = injectionScope;

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
