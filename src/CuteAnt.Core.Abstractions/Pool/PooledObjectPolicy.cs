// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Pool
{
    /// <summary>
    /// A base type for <see cref="IPooledObjectPolicy{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of object which is being pooled.</typeparam>
    public abstract class PooledObjectPolicy<T> : IPooledObjectPolicy<T>
    {
        /// <inheritdoc />
        public abstract T Create();

        /// <inheritdoc />
        public abstract T PreGetting(T obj);

        /// <inheritdoc />
        public abstract bool Return(T obj);
    }
}
