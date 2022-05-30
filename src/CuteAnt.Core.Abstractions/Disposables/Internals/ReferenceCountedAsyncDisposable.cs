#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CuteAnt.Disposables.Internals
{
    /// <summary>
    /// An instance that represents a reference count.
    /// </summary>
    public sealed class ReferenceCountedAsyncDisposable<T> : SingleAsyncDisposable<IReferenceCounter>, IReferenceCountedAsyncDisposable<T>
        where T : class, IAsyncDisposable
    {
        /// <summary>
        /// Initializes a reference counted disposable that refers to the specified reference count. The specified reference count must have already been incremented for this instance.
        /// </summary>
        public ReferenceCountedAsyncDisposable(IReferenceCounter referenceCounter)
            : base(referenceCounter)
        {
            if (referenceCounter is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.referenceCounter); }

            // Ensure we can cast from the stored IDisposable to T.
            _ = ((IReferenceCountedAsyncDisposable<T>)this).Target;
        }

        /// <inheritdoc/>
        protected override ValueTask DisposeAsync(IReferenceCounter referenceCounter) => (referenceCounter.TryDecrementCount() as IAsyncDisposable)?.DisposeAsync() ?? new ValueTask();

        T IReferenceCountedAsyncDisposable<T>.Target => (T)ReferenceCounter.TryGetTarget();

        IReferenceCountedAsyncDisposable<T> IReferenceCountedAsyncDisposable<T>.AddReference()
        {
            var referenceCounter = ReferenceCounter;
            if (!referenceCounter.TryIncrementCount())
            {
                ThrowObjectDisposedException(); // cannot actually happen
            }
            return new ReferenceCountedAsyncDisposable<T>(referenceCounter);
        }

        IWeakReferenceCountedAsyncDisposable<T> IReferenceCountedAsyncDisposable<T>.AddWeakReference() => new WeakReferenceCountedAsyncDisposable<T>(ReferenceCounter);

        private IReferenceCounter ReferenceCounter
        {
            get
            {
                IReferenceCounter referenceCounter = null!;
                // Implementation note: this always "succeeds" in updating the context since it always returns the same instance.
                // So, we know that this will be called at most once. It may also be called zero times if this instance is disposed.
                if (!TryUpdateContext(x => referenceCounter = x))
                {
                    ThrowObjectDisposedException();
                }
                return referenceCounter;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowObjectDisposedException()
        {
            throw GetException();
            static ObjectDisposedException GetException()
            {
                return new ObjectDisposedException(nameof(ReferenceCountedAsyncDisposable<T>));
            }
        }
    }
}
#endif