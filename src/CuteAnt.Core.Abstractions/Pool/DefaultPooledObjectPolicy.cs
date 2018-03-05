// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CuteAnt.Pool
{
  public class DefaultPooledObjectPolicy<T> : PooledObjectPolicy<T>
    where T : class, new()
  {
    public override T Create() => new T();

    public override T PreGetting(T obj) => obj;

    // DefaultObjectPool<T> doesn't call 'Return' for the default policy.
    // So take care adding any logic to this method, as it might require changes elsewhere.
    public override bool Return(T obj) => obj != null;
  }
}