﻿using System;
using CuteAnt.Disposables.Internals;

namespace CuteAnt.Disposables
{
    /// <summary>
    /// Creation methods for reference counted disposables.
    /// </summary>
    public static class ReferenceCountedDisposable
    {
        /// <summary>
        /// Creates a new disposable that disposes <paramref name="disposable"/> when all reference counts have been disposed. This method uses attached (ephemeron) reference counters.
        /// </summary>
        /// <param name="disposable">The disposable to dispose when all references have been disposed. If this is <c>null</c>, then the returned instance does nothing when it is disposed.</param>
        public static IReferenceCountedDisposable<T> Create<T>(T disposable)
            where T : class, IDisposable =>
            TryCreate(disposable) ?? throw new ObjectDisposedException(nameof(T));

        /// <summary>
        /// Creates a new disposable that disposes <paramref name="disposable"/> when all reference counts have been disposed. This method uses attached (ephemeron) reference counters.
        /// </summary>
        /// <param name="disposable">The disposable to dispose when all references have been disposed. If this is <c>null</c>, then the returned instance does nothing when it is disposed.</param>
        public static IReferenceCountedDisposable<T> TryCreate<T>(T disposable)
            where T : class, IDisposable
        {
            // We can't attach reference counters to null, so we use a sort of null object pattern here.
            if (disposable is null)
            {
                return CreateWithNewReferenceCounter(disposable);
            }

            var referenceCounter = ReferenceCounterEphemerons.TryGetAndIncrementOrCreate(disposable);
            if (referenceCounter is null) { return null; }

            return new ReferenceCountedDisposable<T>(referenceCounter);
        }

        /// <summary>
        /// Creates a new disposable that disposes <paramref name="disposable"/> when all reference counts have been disposed. This method creates a new reference counter to keep track of the reference counts.
        /// </summary>
        /// <param name="disposable">The disposable to dispose when all references have been disposed. If this is <c>null</c>, then the returned instance does nothing when it is disposed.</param>
        public static IReferenceCountedDisposable<T> CreateWithNewReferenceCounter<T>(T disposable)
            where T : class, IDisposable
            => new ReferenceCountedDisposable<T>(new ReferenceCounter(disposable));
    }
}