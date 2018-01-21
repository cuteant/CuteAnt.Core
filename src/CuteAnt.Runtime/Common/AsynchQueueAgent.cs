using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  public abstract class AsynchQueueAgent<T> : AsynchAgent, IDisposable
  {
    private BlockingCollection<T> _messageQueue;

    protected AsynchQueueAgent() : base(null) { }

    protected AsynchQueueAgent(string nameSuffix) : base(nameSuffix) { }

    public void EnqueueMessage(T message)
    {
      if (_messageQueue == null) { return; }
      _messageQueue.Add(message);
    }

    protected abstract void Process(T message);

    protected override void Run()
    {
      while (true)
      {
        if (Cts == null || Cts.IsCancellationRequested) { return; }
        T request;
        try
        {
          request = _messageQueue.Take();
        }
        catch (InvalidOperationException)
        {
          if (Log.IsInformationLevelEnabled()) Log.LogInformation("Stop request processed");
          break;
        }

        Process(request);
      }
    }

    public sealed override void Stop()
    {
      Stop(dropPendingMessages: true);
    }

    public virtual void Stop(bool dropPendingMessages)
    {
      if (_messageQueue != null)
      {
        _messageQueue.CompleteAdding();
        if (!dropPendingMessages)
        {
          var spinWait = new SpinWait();
          while (true)
          {
            spinWait.SpinOnce();
            if (this.IsCompleted) { break; }
          }
        }
      }
      base.Stop();
    }

    protected void DrainQueue(Action<T> action)
    {
      while (_messageQueue.TryTake(out T request))
      {
        action(request);
      }
    }

    public virtual int Count => _messageQueue != null ? _messageQueue.Count : 0;

    public virtual bool IsCompleted => _messageQueue != null ? _messageQueue.IsCompleted : true;

    #region IDisposable Members

    protected override void Dispose(bool disposing)
    {
      if (!disposing) return;

#if TRACK_DETAILED_STATS
      if (StatisticsCollector.CollectThreadTimeTrackingStats)
      {
        threadTracking.OnStopExecution();
      }
#endif
      // 注意：这里不调用 Stop 方法，留给派生类指派调用，决定是否丢弃未处理的消息
      base.Dispose(disposing);

      _messageQueue?.Dispose();
      _messageQueue = null;
    }

    #endregion
  }
}
