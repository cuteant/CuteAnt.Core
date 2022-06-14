// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CuteAnt.Pool
{
    /// <summary>
    /// An <see cref="ObjectPoolProvider"/> that produces instances of
    /// <see cref="LeakTrackingObjectPool{T}"/>.
    /// </summary>
    public class LeakTrackingObjectPoolProvider : ObjectPoolProvider
    {
        private readonly ObjectPoolProvider _inner;

        /// <summary>
        /// Initializes a new instance of <see cref="LeakTrackingObjectPoolProvider"/>.
        /// </summary>
        /// <param name="inner">The <see cref="ObjectPoolProvider"/> to wrap.</param>
        public LeakTrackingObjectPoolProvider(ObjectPoolProvider inner)
        {
            if (null == inner) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inner); }
            _inner = inner;
        }

        /// <inheritdoc/>
        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
        {
            var inner = _inner.Create<T>(policy);
            return new LeakTrackingObjectPool<T>(inner);
        }

        /// <inheritdoc/>
        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained)
        {
            var inner = _inner.Create<T>(policy, maximumRetained);
            return new LeakTrackingObjectPool<T>(inner);
        }
    }
}