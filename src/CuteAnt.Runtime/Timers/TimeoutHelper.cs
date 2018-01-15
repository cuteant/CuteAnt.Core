// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// System.ServiceModel.Internals\System\Runtime\TimeoutHelper.cs

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;

namespace CuteAnt.Runtime
{
  public struct TimeoutHelper
  {
    public static readonly TimeSpan MaxWait = TimeSpan.FromMilliseconds(Int32.MaxValue);
    private static readonly CancellationToken s_precancelledToken = new CancellationToken(true);

    private bool _cancellationTokenInitialized;
    private bool _deadlineSet;

    private CancellationToken _cancellationToken;
    private DateTime _deadline;
    private TimeSpan _originalTimeout;

    public TimeoutHelper(TimeSpan timeout)
    {
      Contract.Assert(timeout >= TimeSpan.Zero, "timeout must be non-negative");

      _cancellationTokenInitialized = false;
      _originalTimeout = timeout;
      _deadline = DateTime.MaxValue;
      _deadlineSet = (timeout == TimeSpan.MaxValue);
    }

    public CancellationToken GetCancellationToken()
    {
      return GetCancellationTokenAsync().Result;
    }

    public async Task<CancellationToken> GetCancellationTokenAsync()
    {
      if (!_cancellationTokenInitialized)
      {
        var timeout = RemainingTime();
        if (timeout >= MaxWait || timeout == TimeoutShim.InfiniteTimeSpan)
        {
          _cancellationToken = CancellationToken.None;
        }
        else if (timeout > TimeSpan.Zero)
        {
          _cancellationToken = await TimeoutTokenSource.FromTimeoutAsync((int)timeout.TotalMilliseconds);
        }
        else
        {
          _cancellationToken = s_precancelledToken;
        }
        _cancellationTokenInitialized = true;
      }

      return _cancellationToken;
    }

    public TimeSpan OriginalTimeout
    {
      get { return _originalTimeout; }
    }

    public static bool IsTooLarge(in TimeSpan timeout)
    {
      return (timeout > TimeoutHelper.MaxWait) && (timeout != TimeSpan.MaxValue);
    }

    public static TimeSpan FromMilliseconds(int milliseconds)
    {
      if (milliseconds == Timeout.Infinite)
      {
        return TimeSpan.MaxValue;
      }
      else
      {
        return TimeSpan.FromMilliseconds(milliseconds);
      }
    }

    public static int ToMilliseconds(in TimeSpan timeout)
    {
      if (timeout == TimeSpan.MaxValue)
      {
        return Timeout.Infinite;
      }
      else
      {
        long ticks = Ticks.FromTimeSpan(timeout);
        if (ticks / TimeSpan.TicksPerMillisecond > int.MaxValue)
        {
          return int.MaxValue;
        }
        return Ticks.ToMilliseconds(ticks);
      }
    }

    public static TimeSpan Min(in TimeSpan val1, in TimeSpan val2)
    {
      if (val1 > val2)
      {
        return val2;
      }
      else
      {
        return val1;
      }
    }

    public static TimeSpan Add(in TimeSpan timeout1, in TimeSpan timeout2)
    {
      return Ticks.ToTimeSpan(Ticks.Add(Ticks.FromTimeSpan(timeout1), Ticks.FromTimeSpan(timeout2)));
    }

    public static DateTime Add(in DateTime time, in TimeSpan timeout)
    {
      if (timeout >= TimeSpan.Zero && DateTime.MaxValue - time <= timeout)
      {
        return DateTime.MaxValue;
      }
      if (timeout <= TimeSpan.Zero && DateTime.MinValue - time >= timeout)
      {
        return DateTime.MinValue;
      }
      return time + timeout;
    }

    public static DateTime Subtract(in DateTime time, in TimeSpan timeout)
    {
      return Add(time, TimeSpan.Zero - timeout);
    }

    public static TimeSpan Divide(in TimeSpan timeout, int factor)
    {
      if (timeout == TimeSpan.MaxValue)
      {
        return TimeSpan.MaxValue;
      }

      return Ticks.ToTimeSpan((Ticks.FromTimeSpan(timeout) / factor) + 1);
    }

    public TimeSpan RemainingTime()
    {
      if (!_deadlineSet)
      {
        this.SetDeadline();
        return _originalTimeout;
      }
      else if (_deadline == DateTime.MaxValue)
      {
        return TimeSpan.MaxValue;
      }
      else
      {
        TimeSpan remaining = _deadline - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
          return TimeSpan.Zero;
        }
        else
        {
          return remaining;
        }
      }
    }

    public TimeSpan ElapsedTime()
    {
      return _originalTimeout - this.RemainingTime();
    }

    private void SetDeadline()
    {
      Contract.Assert(!_deadlineSet, "TimeoutHelper deadline set twice.");
      _deadline = DateTime.UtcNow + _originalTimeout;
      _deadlineSet = true;
    }

    public static void ThrowIfNegativeArgument(in TimeSpan timeout)
    {
      ThrowIfNegativeArgument(timeout, "timeout");
    }

    public static void ThrowIfNegativeArgument(in TimeSpan timeout, string argumentName)
    {
      if (timeout < TimeSpan.Zero)
      {
        throw Fx.Exception.ArgumentOutOfRange(argumentName, timeout, InternalSR.TimeoutMustBeNonNegative(argumentName, timeout));
      }
    }

    public static void ThrowIfNonPositiveArgument(in TimeSpan timeout)
    {
      ThrowIfNonPositiveArgument(timeout, "timeout");
    }

    public static void ThrowIfNonPositiveArgument(in TimeSpan timeout, string argumentName)
    {
      if (timeout <= TimeSpan.Zero)
      {
        throw Fx.Exception.ArgumentOutOfRange(argumentName, timeout, InternalSR.TimeoutMustBePositive(argumentName, timeout));
      }
    }

#if DESKTOPCLR
    [Fx.Tag.Blocking]
#endif
    public static bool WaitOne(WaitHandle waitHandle, in TimeSpan timeout)
    {
      ThrowIfNegativeArgument(timeout);
      if (timeout == TimeSpan.MaxValue)
      {
        waitHandle.WaitOne();
        return true;
      }
      else
      {
#if DESKTOPCLR
        return waitHandle.WaitOne(timeout, false);
#else
        //// http://msdn.microsoft.com/en-us/library/85bbbxt9(v=vs.110).aspx 
        //// with exitContext was used in Desktop which is not supported in Net Native or CoreClr
        return waitHandle.WaitOne(timeout);
#endif
      }
    }

    internal static TimeoutException CreateEnterTimedOutException(in TimeSpan timeout)
    {
      return new TimeoutException(string.Format("Cannot claim lock within the allotted timeout of { 0}. The time allotted to this operation may have been a portion of a longer timeout.", timeout));
    }
  }

  /// <summary>This class coalesces timeout tokens because cancelation tokens with timeouts are more expensive to expose.
  /// Disposing too many such tokens will cause thread contentions in high throughput scenario.
  ///
  /// Tokens with target cancelation time 15ms apart would resolve to the same instance.</summary>
  public static class TimeoutTokenSource
  {
    /// <summary>These are constants use to calculate timeout coalescing, for more description see method FromTimeoutAsync</summary>
    private const int CoalescingFactor = 15;
    private const int GranularityFactor = 2000;
    private const int SegmentationFactor = CoalescingFactor * GranularityFactor;

    private static readonly ConcurrentDictionary<long, Task<CancellationToken>> s_tokenCache =
        new ConcurrentDictionary<long, Task<CancellationToken>>();

    private static readonly Action<object> s_deregisterToken = (object state) =>
    {
      var args = (Tuple<long, CancellationTokenSource>)state;
      try
      {
        s_tokenCache.TryRemove(args.Item1, out var ignored);
      }
      finally
      {
        args.Item2.Dispose();
      }
    };

    public static CancellationToken FromTimeout(int millisecondsTimeout)
    {
      return FromTimeoutAsync(millisecondsTimeout).Result;
    }

    public static Task<CancellationToken> FromTimeoutAsync(int millisecondsTimeout)
    {
      // Note that CancellationTokenSource constructor requires input to be >= -1,
      // restricting millisecondsTimeout to be >= -1 would enforce that
      if (millisecondsTimeout < -1)
      {
        throw new ArgumentOutOfRangeException("Invalid millisecondsTimeout value " + millisecondsTimeout);
      }

      // To prevent s_tokenCache growing too large, we have to adjust the granularity of the our coalesce depending
      // on the value of millisecondsTimeout. The coalescing span scales proportionally with millisecondsTimeout which
      // would garentee constant s_tokenCache size in the case where similar millisecondsTimeout values are accepted.
      // If the method is given a wildly different millisecondsTimeout values all the time, the dictionary would still
      // only grow logarithmically with respect to the range of the input values

      uint currentTime = (uint)Environment.TickCount;
      long targetTime = millisecondsTimeout + currentTime;

      // Formula for our coalescing span:
      // Divide millisecondsTimeout by SegmentationFactor and take the highest bit and then multiply CoalescingFactor back
      var segmentValue = millisecondsTimeout / SegmentationFactor;
      var coalescingSpanMs = CoalescingFactor;
      while (segmentValue > 0)
      {
        segmentValue >>= 1;
        coalescingSpanMs <<= 1;
      }
      targetTime = ((targetTime + (coalescingSpanMs - 1)) / coalescingSpanMs) * coalescingSpanMs;

      if (!s_tokenCache.TryGetValue(targetTime, out Task<CancellationToken> tokenTask))
      {
#if NET_4_5_GREATER
        var tcs = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
#else
        var tcs = new TaskCompletionSource<CancellationToken>();
#endif

        // only a single thread may succeed adding its task into the cache
        if (s_tokenCache.TryAdd(targetTime, tcs.Task))
        {
          // Since this thread was successful reserving a spot in the cache, it would be the only thread
          // that construct the CancellationTokenSource
#if NET_4_0_GREATER
          var tokenSource = new CancellationTokenSource((int)(targetTime - currentTime));
          var token = tokenSource.Token;
#else
          var tokenSource = new CancellationTokenSource();
          tokenSource.CancelAfter((int)(targetTime - currentTime));
          var token = tokenSource.Token;
#endif
          // Clean up cache when Token is canceled
          token.Register(s_deregisterToken, Tuple.Create(targetTime, tokenSource));

          // set the result so other thread may observe the token, and return
          tcs.TrySetResult(token);
          tokenTask = tcs.Task;
        }
        else
        {
          // for threads that failed when calling TryAdd, there should be one already in the cache
          if (!s_tokenCache.TryGetValue(targetTime, out tokenTask))
          {
            // In unlikely scenario the token was already cancelled and timed out, we would not find it in cache.
            // In this case we would simply create a non-coalsed token
#if NET_4_0_GREATER
            tokenTask = TaskShim.FromResult(new CancellationTokenSource(millisecondsTimeout).Token);
#else
            var cts = new CancellationTokenSource();
            cts.CancelAfter(millisecondsTimeout);
            tokenTask = TaskShim.FromResult(cts.Token);
#endif
          }
        }
      }
      return tokenTask;
    }
  }
}
