using Microsoft.Extensions.DependencyInjection;

namespace Grace.DependencyInjection.Extensions
{
    /// <summary>Service scope factory that uses grace</summary>
    internal sealed class GraceLifetimeScopeServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IExportLocatorScope _injectionScope;

        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        public GraceLifetimeScopeServiceScopeFactory(IExportLocatorScope injectionScope) => _injectionScope = injectionScope;

        /// <summary>Create an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceScope"/> which
        /// contains an <see cref="T:System.IServiceProvider"/> used to resolve dependencies from a
        /// newly created scope.</summary>
        /// <returns>An <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceScope"/> controlling the
        /// lifetime of the scope. Once this is disposed, any scoped services that have been resolved
        /// from the <see cref="P:Microsoft.Extensions.DependencyInjection.IServiceScope.ServiceProvider"/> 
        /// will also be disposed.</returns>
        public IServiceScope CreateScope() => new GraceServiceScope(_injectionScope.BeginLifetimeScope());
    }
}
