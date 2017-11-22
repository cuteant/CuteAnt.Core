// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CuteAnt.Pool
{
  public class DefaultPooledObjectPolicy<T> : IPooledObjectPolicy<T>
    where T : class, new()
  {
    public T Create() => new T();

    public T PreGetting(T obj) => obj;

    public bool Return(T obj) => obj != null;
  }
}