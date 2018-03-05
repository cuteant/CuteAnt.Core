// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Pool
{
  public abstract class PooledObjectPolicy<T> : IPooledObjectPolicy<T>
  {
    public abstract T Create();

    public abstract T PreGetting(T obj);

    public abstract bool Return(T obj);
  }
}
