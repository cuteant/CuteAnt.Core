#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
using System;

namespace CuteAnt.Disposables.Internals
{
    /// <summary>
    /// An instance that represents an uncounted weak reference.
    /// </summary>
    public sealed class WeakReferenceCountedAsyncDisposable<T> : IWeakReferenceCountedAsyncDisposable<T>
        where T : class, IAsyncDisposable
    {
        private readonly WeakReference<IReferenceCounter> _weakReference;

        /// <summary>
        /// Creates an instance that weakly references the specified reference counter. The specified reference counter should not be incremented.
        /// </summary>
        public WeakReferenceCountedAsyncDisposable(IReferenceCounter referenceCounter)
        {
            if (referenceCounter is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.referenceCounter); }

            _weakReference = new(referenceCounter);

            // Ensure we can cast from the stored disposable to T.
            _ = (T)referenceCounter.TryGetTarget();
        }

        IReferenceCountedAsyncDisposable<T> IWeakReferenceCountedAsyncDisposable<T>.TryAddReference()
        {
            if (!_weakReference.TryGetTarget(out var referenceCounter)) { return null; }
            if (!referenceCounter.TryIncrementCount()) { return null; }
            return new ReferenceCountedAsyncDisposable<T>(referenceCounter);
        }

        T IWeakReferenceCountedAsyncDisposable<T>.TryGetTarget()
        {
            if (!_weakReference.TryGetTarget(out var referenceCounter)) { return null; }
            return (T)referenceCounter.TryGetTarget();
        }
    }
}
#endif