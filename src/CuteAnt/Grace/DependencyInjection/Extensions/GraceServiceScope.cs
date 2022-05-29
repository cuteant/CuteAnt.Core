using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Grace.DependencyInjection.Extensions
{
    /// <summary>Grace service scope</summary>
    internal sealed class GraceServiceScope : IServiceScope
#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        , IAsyncDisposable
#endif
    {
        private readonly IExportLocatorScope _injectionScope;

        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        public GraceServiceScope(IExportLocatorScope injectionScope) => _injectionScope = injectionScope;

        /// <summary>Service provider</summary>
        public IServiceProvider ServiceProvider => _injectionScope;

        // This code added to correctly implement the disposable pattern.
        public void Dispose() => _injectionScope.Dispose();

#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        // This code added to correctly and asynchronously implement the disposable pattern.
        public ValueTask DisposeAsync() => _injectionScope.DisposeAsync();
#endif
    }
}
