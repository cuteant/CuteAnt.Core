// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace CuteAnt.Pool
{
  public class DefaultObjectPool<T> : ObjectPool<T> where T : class
  {
    private readonly T[] _items;
    private readonly IPooledObjectPolicy<T> _policy;

    public DefaultObjectPool(IPooledObjectPolicy<T> policy)
      : this(policy, Environment.ProcessorCount * 2)
    {
    }

    public DefaultObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
    {
      _policy = policy ?? throw new ArgumentNullException(nameof(policy));
      _items = new T[maximumRetained];
    }

    public override T Take()
    {
      T poolItem = null;

      for (var i = 0; i < _items.Length; i++)
      {
        var item = _items[i];
        if (item != null && Interlocked.CompareExchange(ref _items[i], null, item) == item)
        {
          poolItem = item;
          break;
        }
      }

      if (null == poolItem) { poolItem = _policy.Create(); }

      return poolItem;
    }

    public override T Get()
    {
      var poolItem = Take();

      return _policy.PreGetting(poolItem);
    }

    public override void Return(T obj)
    {
      if (!_policy.Return(obj)) { return; }

      for (var i = 0; i < _items.Length; i++)
      {
        if (Interlocked.CompareExchange(ref _items[i], obj, null) == null)
        {
          return;
        }
      }
    }

    public override void Clear()
    {
      for (var i = 0; i < _items.Length; i++)
      {
        Interlocked.Exchange(ref _items[i], null);
      }
    }
  }
}
