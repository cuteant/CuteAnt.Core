using System;
using System.Threading.Tasks;

namespace CuteAnt.Disposables
{
    /// <summary>
    /// A singleton disposable that does nothing when disposed.
    /// </summary>
    public sealed class NoopDisposable: IDisposable
#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        , IAsyncDisposable
#endif
    {
        private NoopDisposable()
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
        }

#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        /// <summary>
        /// Does nothing.
        /// </summary>
        public ValueTask DisposeAsync() => new ValueTask();
#endif

        /// <summary>
        /// Gets the instance of <see cref="NoopDisposable"/>.
        /// </summary>
        public static NoopDisposable Instance { get; } = new NoopDisposable();
    }
}
