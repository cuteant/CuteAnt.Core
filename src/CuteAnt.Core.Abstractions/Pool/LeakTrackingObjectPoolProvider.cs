// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CuteAnt.Pool
{
  public class LeakTrackingObjectPoolProvider : ObjectPoolProvider
  {
    private readonly ObjectPoolProvider _inner;

    public LeakTrackingObjectPoolProvider(ObjectPoolProvider inner)
    {
      _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
    {
      var inner = _inner.Create<T>(policy);
      return new LeakTrackingObjectPool<T>(inner);
    }

    public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained)
    {
      var inner = _inner.Create<T>(policy, maximumRetained);
      return new LeakTrackingObjectPool<T>(inner);
    }
  }
}