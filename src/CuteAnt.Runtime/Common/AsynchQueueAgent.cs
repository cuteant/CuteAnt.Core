using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  //// Message implements ITimeInterval to be able to measure different time intervals in the lifecycle of a message,
  //// such as time in queue...
  //public interface IOutgoingMessage : ITimeInterval
  //{
  //  bool IsSameDestination(IOutgoingMessage other);
  //}

  public abstract class AsynchQueueAgent<T> : AsynchAgent, IDisposable
  //where T : IOutgoingMessage
  {
    private BlockingCollection<T> _messageQueue;
    // ## 苦竹 屏蔽 ##
    //private QueueTrackingStatistic _queueTracking;
    private int _maxMessageBatchSize;

    private const int DEFAULT_MAX_MESSAGE_BATCH_SIZE = 10;

    protected virtual bool UseMessageBatching => false;

    protected AsynchQueueAgent()
      : this(null, DEFAULT_MAX_MESSAGE_BATCH_SIZE)
    {
    }

    protected AsynchQueueAgent(int maxMessageBatchSize)
      : this(null, maxMessageBatchSize)
    {
    }

    protected AsynchQueueAgent(string nameSuffix)
      : this(nameSuffix, DEFAULT_MAX_MESSAGE_BATCH_SIZE)
    {
    }

    protected AsynchQueueAgent(string nameSuffix, int maxMessageBatchSize)
      : base(nameSuffix)
    {
      _maxMessageBatchSize = maxMessageBatchSize;
      _messageQueue = new BlockingCollection<T>();
      // ## 苦竹 屏蔽 ##
      //if (StatisticsCollector.CollectQueueStats)
      //{
      //  _queueTracking = new QueueTrackingStatistic(base.Name);
      //}
    }

    public void EnqueueMessage(T message)
    {
      if (_messageQueue == null) { return; }

#if TRACK_DETAILED_STATS
      if (StatisticsCollector.CollectQueueStats)
      {
        _queueTracking.OnEnQueueRequest(1, _messageQueue.Count, message);
      }
#endif
      _messageQueue.Add(message);
    }

    protected abstract void Process(T message);
    protected abstract void ProcessBatch(List<T> messages);

    protected override void Run()
    {
#if TRACK_DETAILED_STATS
      if (StatisticsCollector.CollectThreadTimeTrackingStats)
      {
        threadTracking.OnStartExecution();
        _queueTracking.OnStartExecution();
      }
      try
      {
#endif
      if (UseMessageBatching)
      {
        RunBatching();
      }
      else
      {
        RunNonBatching();
      }
#if TRACK_DETAILED_STATS
      }
      finally
      {
        if (StatisticsCollector.CollectThreadTimeTrackingStats)
        {
          threadTracking.OnStopExecution();
          _queueTracking.OnStopExecution();
        }
      }
#endif
    }


    protected void RunNonBatching()
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
          if (Log.IsInformationLevelEnabled()) Log.LogInformation(ErrorCode.Runtime_Error_100312, "Stop request processed");
          break;
        }
#if TRACK_DETAILED_STATS
        if (StatisticsCollector.CollectQueueStats)
        {
          _queueTracking.OnDeQueueRequest(request);
        }
        if (StatisticsCollector.CollectThreadTimeTrackingStats)
        {
          threadTracking.OnStartProcessing();
        }
#endif
        Process(request);
#if TRACK_DETAILED_STATS
        if (StatisticsCollector.CollectThreadTimeTrackingStats)
        {
          threadTracking.OnStopProcessing();
          threadTracking.IncrementNumberOfProcessed();
        }
#endif
      }
    }

    protected void RunBatching()
    {
      int maxBatchingSize = _maxMessageBatchSize;

      while (true)
      {
        if (Cts == null || Cts.IsCancellationRequested) { return; }

        var mlist = new List<T>(maxBatchingSize);
        try
        {
          T firstRequest = _messageQueue.Take();
          mlist.Add(firstRequest);

          while (_messageQueue.Count != 0 && mlist.Count < maxBatchingSize)
          //&& _messageQueue.First().IsSameDestination(firstRequest))
          {
            mlist.Add(_messageQueue.Take());
          }
        }
        catch (InvalidOperationException)
        {
          Log.LogWarning(ErrorCode.Runtime_Error_100312, "Stop request processed");
          break;
        }

#if TRACK_DETAILED_STATS
        if (StatisticsCollector.CollectQueueStats)
        {
          foreach (var request in mlist)
          {
            queueTracking.OnDeQueueRequest(request);
          }
        }

        if (StatisticsCollector.CollectThreadTimeTrackingStats)
        {
          threadTracking.OnStartProcessing();
        }
#endif
        ProcessBatch(mlist);
#if TRACK_DETAILED_STATS
        if (StatisticsCollector.CollectThreadTimeTrackingStats)
        {
          threadTracking.OnStopProcessing();
          threadTracking.IncrementNumberOfProcessed(mlist.Count);
        }
#endif
      }
    }

    public sealed override void Stop()
    {
      Stop(dropPendingMessages: true);
    }

    public virtual void Stop(bool dropPendingMessages)
    {
#if TRACK_DETAILED_STATS
      if (StatisticsCollector.CollectThreadTimeTrackingStats)
      {
        threadTracking.OnStopExecution();
      }
#endif
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
