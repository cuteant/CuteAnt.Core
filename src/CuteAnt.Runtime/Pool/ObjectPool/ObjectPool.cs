// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Pool
{
  public abstract class ObjectPool<T> where T : class
  {
    /// <summary></summary>
    /// <returns></returns>
    public abstract T Get();

    /// <summary></summary>
    /// <returns></returns>
    public abstract T Take();

    /// <summary></summary>
    /// <param name="obj"></param>
    public abstract void Return(T obj);

    /// <summary></summary>
    public abstract void Clear();
  }
}