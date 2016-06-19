// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using CuteAnt.Extensions.Caching.Memory;

namespace CuteAnt.Extensions.Caching.Distributed
{
  public class MemoryDistributedCache : IDistributedCache
  {
#if NET40
    private static readonly Task CompletedTask = TaskEx.FromResult<object>(null);
#else
    private static readonly Task CompletedTask = Task.FromResult<object>(null);
#endif

    private readonly IMemoryCache _memCache;

    public MemoryDistributedCache(IMemoryCache memoryCache)
    {
      if (memoryCache == null)
      {
        throw new ArgumentNullException(nameof(memoryCache));
      }

      _memCache = memoryCache;
    }

    public byte[] Get(string key)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      return (byte[])_memCache.Get(key);
    }

    public Task<byte[]> GetAsync(string key)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

#if NET40
      return TaskEx.FromResult(Get(key));
#else
      return Task.FromResult(Get(key));
#endif
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      if (value == null)
      {
        throw new ArgumentNullException(nameof(value));
      }

      var memoryCacheEntryOptions = new MemoryCacheEntryOptions();
      memoryCacheEntryOptions.AbsoluteExpiration = options.AbsoluteExpiration;
      memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
      memoryCacheEntryOptions.SlidingExpiration = options.SlidingExpiration;

      _memCache.Set(key, value, memoryCacheEntryOptions);
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      if (value == null)
      {
        throw new ArgumentNullException(nameof(value));
      }

      Set(key, value, options);
      return CompletedTask;
    }

    public void Refresh(string key)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      object value;
      _memCache.TryGetValue(key, out value);
    }

    public Task RefreshAsync(string key)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      Refresh(key);
      return CompletedTask;
    }

    public void Remove(string key)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      _memCache.Remove(key);
    }

    public Task RemoveAsync(string key)
    {
      if (key == null)
      {
        throw new ArgumentNullException(nameof(key));
      }

      Remove(key);
      return CompletedTask;
    }
  }
}