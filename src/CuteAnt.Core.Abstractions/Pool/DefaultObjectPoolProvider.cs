// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CuteAnt.Pool
{
    /// <summary>
    /// The default <see cref="ObjectPoolProvider"/>.
    /// </summary>
    public class DefaultObjectPoolProvider : ObjectPoolProvider
    {
        public static readonly DefaultObjectPoolProvider Default = new DefaultObjectPoolProvider();

        /// <summary>
        /// The maximum number of objects to retain in the pool.
        /// </summary>
        public int MaximumRetained { get; set; } = Environment.ProcessorCount * 2;

        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
        {
            return Create<T>(policy, MaximumRetained);
        }

        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained)
        {
            if (policy is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.policy); }

            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                return new DisposableObjectPool<T>(policy, maximumRetained);
            }

            return new DefaultObjectPool<T>(policy, maximumRetained);
        }
    }
}
