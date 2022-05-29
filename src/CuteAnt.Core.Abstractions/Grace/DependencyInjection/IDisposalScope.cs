using System;

namespace Grace.DependencyInjection
{
    /// <summary>Represents a scope that holds disposable object</summary>
    public interface IDisposalScope : IDisposable
#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        , IAsyncDisposable
#endif
    {
        /// <summary>Add an object for disposal tracking.</summary>
        /// <param name="disposable">object to track for disposal.</param>
        T AddDisposable<T>(T disposable) where T : IDisposable;

        /// <summary>Add an object for disposal tracking.</summary>
        /// <param name="disposable">object to track for disposal.</param>
        /// <param name="cleanupDelegate">logic that will be run directly before the object is disposed.</param>
        T AddDisposable<T>(T disposable, Action<T> cleanupDelegate) where T : IDisposable;

#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        /// <summary>Add an object for disposal tracking.</summary>
        /// <param name="disposable">object to track for async disposal.</param>
        T AddAsyncDisposable<T>(T disposable) where T : IAsyncDisposable;

        /// <summary>Add an object for disposal tracking.</summary>
        /// <param name="disposable">object to track for async disposal.</param>
        /// <param name="cleanupDelegate">logic that will be run directly before the object is disposed.</param>
        T AddAsyncDisposable<T>(T disposable, Action<T> cleanupDelegate) where T : IAsyncDisposable;
#endif
    }
}