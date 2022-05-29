// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
#if DEBUG
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Security;
#if NETFRAMEWORK
using System.Security.Permissions;
#endif
#endif //DEBUG
using CuteAnt.Diagnostics;
using CuteAnt.Runtime;
using CuteAnt.Pool;

namespace CuteAnt.Buffers
{
  internal abstract class InternalBufferManager
  {
    protected InternalBufferManager()
    {
    }

    internal abstract byte[] TakeBuffer(int bufferSize);
    internal abstract void ReturnBuffer(byte[] buffer);
    internal abstract void Clear();

    internal static InternalBufferManager Create(long maxBufferPoolSize, int maxBufferSize)
    {
      if (maxBufferPoolSize == 0)
      {
        return GCBufferManager.Value;
      }
      else
      {
        Fx.Assert(maxBufferPoolSize > 0 && maxBufferSize >= 0, "bad params, caller should verify");
        return new PooledBufferManager(maxBufferPoolSize, maxBufferSize);
      }
    }

    /// <summary>PooledBufferManager</summary>
    /// <remarks>http://kennyw.com/?p=229
    /// http://kennyw.com/?p=51
    /// http://blogs.msdn.com/b/sajay/archive/2011/12/09/efficient-buffer-management-reduced-loh-allocation.aspx
    /// http://obsessivelycurious.blogspot.ru/2008/04/wcf-memory-buffer-management.html
    /// I have recently joined the Connected Framework Team at Microsoft - at least if 7 months ago still qualifies for "recently". 
    /// The team is much less known by its name than by the product it created, i.e. Indigo, later charmingly renamed Windows Communication Foundation. 
    /// As part of my job as a Program Manager I get to work on WCF performance, and many folks on the team are quite particular about it. 
    /// Periodically, I monitor various related online forums and pick out questions related to performance. 
    /// About a month ago I spotted a couple of questions about memory management in WCF. 
    /// Since WCF provides a handful of different configuration parameters related to memory, people were curious what may be the impact of their various settings on performance. 
    /// Kenny Wolf has a few posts on the subject in his blog: here and here.
    /// 
    /// However, curious as I am, I wanted to dive a little deeper and see how things work under the hood. A few hours of spelunking through source code and stepping through in the debugger (which you can also do now that the .Net Framework source code is publicly available - see here for more) I realized that WCF uses a pretty sophisticated memory buffer management mechanism in order to save on garbage collection and improve performance. The rest of this post describes just how it works, so if you're interested, read on.
    /// 
    /// The crux of the buffer managment algorithm rests with the class aptly named BufferManager. BufferManager has one static method CreateBufferManager which takes two parameters: maxBufferPoolSize and maxBufferSize. The first one indicates how much total memory the manager can hold on to, while the other determines the size of the largest individual buffer the manager can store. In reality, BufferManager's CreateBufferManager method delegates to one of two internal classes depending on the value of maxBufferPoolSize. If maxBufferPoolSize = 0, then a GCBufferManager gets created, otherwise you get a PooledBufferManager. The former is trivial, and in fact doesn't really do any management whatsoever, but simply allocates a new buffer for any request, and lets the garbage collector take care of disposing it. The latter is much more interesting.
    /// 
    /// The PooledBufferManager's constructor takes the same two parameters as the CreateBufferManager method, i.e. maxBufferPoolSize, maxBufferSize. The constructor creates a list of buffer pools of type SynchronizedPool. Each of them collects buffers of a certain size (bufferSize). The size of the buffers in the smallest pool is 128 bytes. Each subsequent pool doubles the size of the buffers it holds until the cap of maxBufferSize is reached. Each pool is also assigned a quota of buffers it can contain at any time. As the pools are created each is given a quota of a single buffer until the total allotment exceeds maxBufferPoolSize. Any subsequent pools get a quota of zero. Each pool also contains some bookkeeping data, such as the number of buffers currently in the pool (count), the maximum number of buffers in the pool (limit), as well as the peak number of buffers in the pool (peak) and the number of misses (misses), which will be explained a bit later.
    /// 
    /// Buffer managers are created by a number of components of WCF, most notably by the OnOpening method of the TransportChannelListener. It's the listener that receives messages sent to a WCF service. TransportChannelListener creates a buffer manager with the maximum buffer size equal to the MaxReceivedMessageSize. That's the value you can set on your binding in config (maxReceivedMessageSize) to prevent denial of service attacks by limiting the maximum amount of memory the system will allocate for any incoming message (see Kenny's post for more details). TransportChannelListener also sets the maximum buffer pool size according to the value of MaxBufferPoolSize property of the transport binding element.
    /// 
    /// The really interesting fact about buffer management in WCF is that BufferManager doesn't actually allocate any buffers up front. Instead, each of the buffer pools is allotted a quota of buffers it can hold (limit). If someone requests a buffer - say, for an incoming message - (by calling TakeBuffer) the manager locates the first buffer pool that holds buffers large enough to satisfy the request. For example, if the requested buffer size is 560 bytes, the manager will try 128, 256, 512, and finally settle on 1024. Now, the manager will ask the pool for a buffer. If one is available in the pool, it is returned to the requestor, the pool’s buffer count is decremented and that's the end of the story. On the other hand, if none is available the manager will allocate a new buffer according to the pool's buffer size (1024 bytes in my example), but before that happens, some bookkeeping takes place. The manager checks if the peak number of buffers in the pool has reached its allotted limit. If so, then we have a miss, which means that the total number of allocated buffers of this size currently in the system has exceeded the limit. The manager will bump up the number of misses for this pool. It will also increment its own counter of total misses for all buffer pools (totalMisses). Once this number reaches a predefined threshold (maxMissesBeforeTuning = 8), the allotment of buffer limits per pool must be tuned up. The manager will call TuneQuotas, but this will be described later. For now the request has been satisfied by either returning a buffer from a pool or allocating a new buffer.
    /// 
    /// Having obtained a buffer the requestor can now hold on to it for as long as is needed, but when it's done it must call ReturnBuffer to give the buffer back to the BufferManager. What happens upon buffer return is pretty much the reverse of what you saw for taking a buffer. The manager finds the matching pool. If none is found, or if the matching pool has no more room left (count = limit), the buffer is simply abandoned and left for the GC to collect (this is to ensure that the total pooled memory never exceeds maxBufferPoolSize). Otherwise, the buffer is returned to the pool and the pool’s buffer count is incremented. If the count exceeds peak, the latter is updated accordingly.
    /// 
    /// Now that you know how buffers are obtained and returned it’s time to take a look at quota tuning. First, the BufferManager finds the most starved buffer pool, defined as the pool missing the most memory, which in turn is calculated as the product of the number of misses in the pool and the buffer size. If there is enough memory left under the maxBufferPoolSize to accommodate one more buffer in the most starved pool, the pool’s quota is simply incremented by one, and remaining memory is adjusted. Otherwise, the manager must find a different pool from which to steal a buffer. The best candidate is the most underutilized pool, i.e. the one with the maximum product of (limit – peak) * bufferSize. Once such a pool is found its quota is decremented by one, and the remaining memory is adjusted accordingly. If there is enough memory now to add a buffer to the most starved pool, its quota will be bumped up by one. Otherwise, the tuning ends, and another attempted will be made in the next tuning cycle. Finally, the misses count for all pools and the managers totalMisses count are reset.
    /// 
    /// If all this sounds a bit complicated, it's because the WCF team has gone to great length to ensure this platform performs extremely well, and efficient buffer management contributes substantially to this goal. I should add that my description omits another aspect of buffer managment, which adds to its efficiency. I will only mention it here very briefly. Whenever multiple threads have to access a common pool of resources, you must obviously make sure that they do so without corrupting the state of the resource pool (here buffer manager), which, in short, involves thread synchronization and locking. Synchronization is expensive, and could potentially lead to such contention over the buffer manager that it would erase any benefit from buffer pooling. To avoid this problem BufferManager uses a SynchronizedPool for each of its buffer pools, which in turn employs some clever tricks to allocate dedicated buffers to the most active threads, so that they can obtain and return them without locking.
    /// 
    /// In summary, WCF comes with a powerful and efficient buffer pooling subsystem. It does not pre-allocate any memory, but instead creates and reuses memory buffers only as messages come in. In addition, it dynamically adapts the division of the pre-configured memory pool between buffers of various sizes to best match the size(s) of the most common messages your service receives.
    /// </remarks>
    class PooledBufferManager : InternalBufferManager
    {
      private const Int32 maxSynchronizedBufferSize = 81920;
      private const int minBufferSize = 128;
      private const int maxMissesBeforeTuning = 8;
      private const int initialBufferCount = 1;
      private readonly object _tuningLock;

      private int[] _bufferSizes;
      private BufferPool[] _bufferPools;
      private long _memoryLimit;
      private long _remainingMemory;
      private bool _areQuotasBeingTuned;
      private int _totalMisses;
#if DEBUG && !FEATURE_NETNATIVE
      private ConcurrentDictionary<int, string> _buffersPooled = new ConcurrentDictionary<int, string>();
#endif //DEBUG

      public PooledBufferManager(long maxMemoryToPool, int maxBufferSize)
      {
        _tuningLock = new object();
        _memoryLimit = maxMemoryToPool;
        _remainingMemory = maxMemoryToPool;
        List<BufferPool> bufferPoolList = new List<BufferPool>();

        #region ## 苦竹 修改 ##
        //for (int bufferSize = minBufferSize; ;)
        //{
        //  long bufferCountLong = _remainingMemory / bufferSize;

        //  int bufferCount = bufferCountLong > int.MaxValue ? int.MaxValue : (int)bufferCountLong;

        //  if (bufferCount > initialBufferCount)
        //  {
        //    bufferCount = initialBufferCount;
        //  }

        //  bufferPoolList.Add(BufferPool.CreatePool(bufferSize, bufferCount));

        //  _remainingMemory -= (long)bufferCount * bufferSize;

        //  if (bufferSize >= maxBufferSize)
        //  {
        //    break;
        //  }

        //  long newBufferSizeLong = (long)bufferSize * 2;

        //  if (newBufferSizeLong > (long)maxBufferSize)
        //  {
        //    bufferSize = maxBufferSize;
        //  }
        //  else
        //  {
        //    bufferSize = (int)newBufferSizeLong;
        //  }
        //}
        var flag = false;
        for (Int32 bufferSize = minBufferSize; ;)
        {
          var bufferCountLong = _remainingMemory / bufferSize;

          var bufferCount = bufferCountLong > Int32.MaxValue ? Int32.MaxValue : (Int32)bufferCountLong;

          if (bufferCount > initialBufferCount) { bufferCount = initialBufferCount; }

          bufferPoolList.Add(BufferPool.CreatePool(bufferSize, bufferCount));
          if (maxSynchronizedBufferSize == bufferSize) { flag = true; }

          _remainingMemory -= (Int64)bufferCount * bufferSize;

          if (bufferSize >= maxBufferSize) { break; }

          var newBufferSizeLong = (Int64)bufferSize * 2L;

          if (newBufferSizeLong > (Int64)maxBufferSize)
          {
            bufferSize = maxBufferSize;
          }
          else
          {
            if (!flag && newBufferSizeLong > maxSynchronizedBufferSize)
            {
              bufferPoolList.Add(BufferPool.CreatePool(maxSynchronizedBufferSize, initialBufferCount));
              _remainingMemory -= (Int64)initialBufferCount * maxSynchronizedBufferSize;
              flag = true;
            }

            bufferSize = (Int32)newBufferSizeLong;
          }
        }
        #endregion

        _bufferPools = bufferPoolList.ToArray();
        #region ## 苦竹 修改 ##
        //_bufferSizes = new int[_bufferPools.Length];
        //for (int i = 0; i < _bufferPools.Length; i++)
        //{
        //  _bufferSizes[i] = _bufferPools[i].BufferSize;
        //}
        _bufferSizes = bufferPoolList.Select(_ => _.BufferSize).ToArray();
        #endregion
      }

      internal override void Clear()
      {
#if DEBUG && !FEATURE_NETNATIVE
        _buffersPooled.Clear();
#endif //DEBUG

        for (int i = 0; i < _bufferPools.Length; i++)
        {
          BufferPool bufferPool = _bufferPools[i];
          bufferPool.Clear();
        }
      }

      [MethodImpl(InlineMethod.Value)]
      private void ChangeQuota(ref BufferPool bufferPool, int delta)
      {
        if (TraceCore.BufferPoolChangeQuotaIsEnabled(Fx.Trace))
        {
          TraceCore.BufferPoolChangeQuota(Fx.Trace, bufferPool.BufferSize, delta);
        }

        BufferPool oldBufferPool = bufferPool;
        int newLimit = oldBufferPool.Limit + delta;
        BufferPool newBufferPool = BufferPool.CreatePool(oldBufferPool.BufferSize, newLimit);
        for (int i = 0; i < newLimit; i++)
        {
          byte[] buffer = oldBufferPool.Take();
          if (buffer == null)
          {
            break;
          }
          newBufferPool.Return(buffer);
          newBufferPool.IncrementCount();
        }
        _remainingMemory -= oldBufferPool.BufferSize * delta;
        bufferPool = newBufferPool;
      }

      private void DecreaseQuota(ref BufferPool bufferPool)
      {
        ChangeQuota(ref bufferPool, -1);
      }

      private int FindMostExcessivePool()
      {
        long maxBytesInExcess = 0;
        int index = -1;

        for (int i = 0; i < _bufferPools.Length; i++)
        {
          BufferPool bufferPool = _bufferPools[i];

          if (bufferPool.Peak < bufferPool.Limit)
          {
            long bytesInExcess = (bufferPool.Limit - bufferPool.Peak) * (long)bufferPool.BufferSize;

            if (bytesInExcess > maxBytesInExcess)
            {
              index = i;
              maxBytesInExcess = bytesInExcess;
            }
          }
        }

        return index;
      }

      private int FindMostStarvedPool()
      {
        long maxBytesMissed = 0;
        int index = -1;

        for (int i = 0; i < _bufferPools.Length; i++)
        {
          BufferPool bufferPool = _bufferPools[i];

          if (bufferPool.Peak == bufferPool.Limit)
          {
            long bytesMissed = bufferPool.Misses * (long)bufferPool.BufferSize;

            if (bytesMissed > maxBytesMissed)
            {
              index = i;
              maxBytesMissed = bytesMissed;
            }
          }
        }

        return index;
      }

      [MethodImpl(InlineMethod.Value)]
      private BufferPool FindPool(int desiredBufferSize)
      {
        for (int i = 0; i < _bufferSizes.Length; i++)
        {
          if (desiredBufferSize <= _bufferSizes[i])
          {
            return _bufferPools[i];
          }
        }

        return null;
      }

      private void IncreaseQuota(ref BufferPool bufferPool)
      {
        ChangeQuota(ref bufferPool, 1);
      }

      internal override void ReturnBuffer(byte[] buffer)
      {
        Fx.Assert(buffer != null, "caller must verify");

#if DEBUG
        int hash = buffer.GetHashCode();
        if (!_buffersPooled.TryAdd(hash, CaptureStackTrace()))
        {
          if (!_buffersPooled.TryGetValue(hash, out string originalStack))
          {
            originalStack = "NULL";
          }

          Fx.Assert(
              string.Format(
                  CultureInfo.InvariantCulture,
                  "Buffer '{0}' has already been returned to the bufferManager before. Previous CallStack: {1} Current CallStack: {2}",
                  hash,
                  originalStack,
                  CaptureStackTrace()));

        }
#endif //DEBUG

        BufferPool bufferPool = FindPool(buffer.Length);
        if (bufferPool != null)
        {
          if (buffer.Length != bufferPool.BufferSize)
          {
            ThrowHelper.ThrowArgumentException(ExceptionResource.BufferIsNotRightSizeForBufferManager, ExceptionArgument.buffer);
          }

          if (bufferPool.Return(buffer))
          {
            bufferPool.IncrementCount();
          }
        }
      }

      internal override byte[] TakeBuffer(int bufferSize)
      {
        Fx.Assert(bufferSize >= 0, "caller must ensure a non-negative argument");

        BufferPool bufferPool = FindPool(bufferSize);
        byte[] returnValue;
        if (bufferPool != null)
        {
          byte[] buffer = bufferPool.Take();
          if (buffer != null)
          {
            bufferPool.DecrementCount();
            returnValue = buffer;
          }
          else
          {
            if (bufferPool.Peak == bufferPool.Limit)
            {
              bufferPool.Misses++;
              if (++_totalMisses >= maxMissesBeforeTuning)
              {
                TuneQuotas();
              }
            }

            if (TraceCore.BufferPoolAllocationIsEnabled(Fx.Trace))
            {
              TraceCore.BufferPoolAllocation(Fx.Trace, bufferPool.BufferSize);
            }

            returnValue = Fx.AllocateByteArray(bufferPool.BufferSize);
          }
        }
        else
        {
          if (TraceCore.BufferPoolAllocationIsEnabled(Fx.Trace))
          {
            TraceCore.BufferPoolAllocation(Fx.Trace, bufferSize);
          }

          returnValue = Fx.AllocateByteArray(bufferSize);
        }

#if DEBUG && !FEATURE_NETNATIVE
        _buffersPooled.TryRemove(returnValue.GetHashCode(), out string dummy);
#endif //DEBUG

        return returnValue;
      }

#if DEBUG
      [SecuritySafeCritical]
#if NETFRAMEWORK
      [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
#endif
      private static string CaptureStackTrace()
      {
        return new StackTrace(true).ToString();
      }
#endif //DEBUG

      private void TuneQuotas()
      {
        if (_areQuotasBeingTuned)
        {
          return;
        }

        bool lockHeld = false;
        try
        {
          Monitor.TryEnter(_tuningLock, ref lockHeld);

          // Don't bother if another thread already has the lock
          if (!lockHeld || _areQuotasBeingTuned)
          {
            return;
          }

          _areQuotasBeingTuned = true;
        }
        finally
        {
          if (lockHeld)
          {
            Monitor.Exit(_tuningLock);
          }
        }

        // find the "poorest" pool
        int starvedIndex = FindMostStarvedPool();
        if (starvedIndex >= 0)
        {
          BufferPool starvedBufferPool = _bufferPools[starvedIndex];

          if (_remainingMemory < starvedBufferPool.BufferSize)
          {
            // find the "richest" pool
            int excessiveIndex = FindMostExcessivePool();
            if (excessiveIndex >= 0)
            {
              // steal from the richest
              DecreaseQuota(ref _bufferPools[excessiveIndex]);
            }
          }

          if (_remainingMemory >= starvedBufferPool.BufferSize)
          {
            // give to the poorest
            IncreaseQuota(ref _bufferPools[starvedIndex]);
          }
        }

        // reset statistics
        for (int i = 0; i < _bufferPools.Length; i++)
        {
          BufferPool bufferPool = _bufferPools[i];
          bufferPool.Misses = 0;
        }

        _totalMisses = 0;
        _areQuotasBeingTuned = false;
      }

      abstract class BufferPool
      {
        private int _bufferSize;
        private int _count;
        private int _limit;
        private int _misses;
        private int _peak;

        public BufferPool(int bufferSize, int limit)
        {
          _bufferSize = bufferSize;
          _limit = limit;
        }

        public int BufferSize
        {
          [MethodImpl(InlineMethod.Value)]
          get { return _bufferSize; }
        }

        public int Limit
        {
          [MethodImpl(InlineMethod.Value)]
          get { return _limit; }
        }

        public int Misses
        {
          [MethodImpl(InlineMethod.Value)]
          get { return _misses; }
          set { _misses = value; }
        }

        public int Peak
        {
          [MethodImpl(InlineMethod.Value)]
          get { return _peak; }
        }

        public void Clear()
        {
          this.OnClear();
          _count = 0;
        }

        public void DecrementCount()
        {
          int newValue = _count - 1;
          if (newValue >= 0)
          {
            _count = newValue;
          }
        }

        public void IncrementCount()
        {
          int newValue = _count + 1;
          if (newValue <= _limit)
          {
            _count = newValue;
            if (newValue > _peak)
            {
              _peak = newValue;
            }
          }
        }

        internal abstract byte[] Take();
        internal abstract bool Return(byte[] buffer);
        internal abstract void OnClear();

        internal static BufferPool CreatePool(int bufferSize, int limit)
        {
          // To avoid many buffer drops during training of large objects which
          // get allocated on the LOH, we use the LargeBufferPool and for 
          // bufferSize < 85000, the SynchronizedPool. However if bufferSize < 85000
          // and (bufferSize + array-overhead) > 85000, this would still use 
          // the SynchronizedPool even though it is allocated on the LOH.
          if (bufferSize < 85000)
          {
            return new SynchronizedBufferPool(bufferSize, limit);
          }
          else
          {
            return new LargeBufferPool(bufferSize, limit);
          }
        }

        class SynchronizedBufferPool : BufferPool
        {
          private SynchronizedPool<byte[]> _innerPool;

          internal SynchronizedBufferPool(int bufferSize, int limit)
              : base(bufferSize, limit) => _innerPool = new SynchronizedPool<byte[]>(limit);

          internal override void OnClear() => _innerPool.Clear();

          internal override byte[] Take() => _innerPool.Take();

          internal override bool Return(byte[] buffer) => _innerPool.Return(buffer);
        }

        class LargeBufferPool : BufferPool
        {
          private Stack<byte[]> _items;

          internal LargeBufferPool(int bufferSize, int limit)
              : base(bufferSize, limit) => _items = new Stack<byte[]>(limit);

          private object ThisLock => _items;

          internal override void OnClear()
          {
            lock (ThisLock)
            {
              _items.Clear();
            }
          }

          internal override byte[] Take()
          {
            lock (ThisLock)
            {
              if (_items.Count > 0)
              {
                return _items.Pop();
              }
            }

            return null;
          }

          internal override bool Return(byte[] buffer)
          {
            lock (ThisLock)
            {
              if (_items.Count < this.Limit)
              {
                _items.Push(buffer);
                return true;
              }
            }

            return false;
          }
        }
      }
    }

    class GCBufferManager : InternalBufferManager
    {
      private static GCBufferManager s_value = new GCBufferManager();

      private GCBufferManager()
      {
      }

      internal static GCBufferManager Value => s_value;

      internal override void Clear()
      {
      }

      internal override byte[] TakeBuffer(int bufferSize) => Fx.AllocateByteArray(bufferSize);

      internal override void ReturnBuffer(byte[] buffer)
      {
        // do nothing, GC will reclaim this buffer
      }
    }
  }
}
