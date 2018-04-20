#if !NET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
//// todo: dependency on runtime (due to logging)
using CuteAnt.Runtime;

namespace CuteAnt.Threading
{
  /// <summary>
  /// Essentially FixedThreadPool with work stealing
  /// </summary>
  public class ThreadPoolExecutor : IExecutor, IHealthCheckable
  {
    private readonly ThreadPoolWorkQueue workQueue;

    private readonly ThreadPoolExecutorOptions options;

    //private readonly ThreadPoolTrackingStatistic statistic;

    private readonly ExecutingWorkItemsTracker executingWorkTracker;

    private readonly ILogger log;

    public ThreadPoolExecutor(ThreadPoolExecutorOptions options)
    {
      this.options = options ?? throw new ArgumentNullException(nameof(options));

      workQueue = new ThreadPoolWorkQueue();

      //statistic = new ThreadPoolTrackingStatistic(options.Name, options.LoggerFactory);

      executingWorkTracker = new ExecutingWorkItemsTracker(this);

      log = TraceLogger.GetLogger<ThreadPoolExecutor>();

      options.CancellationTokenSource.Token.Register(Complete);

      for (var threadIndex = 0; threadIndex < options.DegreeOfParallelism; threadIndex++)
      {
        RunWorker(threadIndex);
      }
    }

    public void QueueWorkItem(WaitCallback callback, object state = null)
    {
      if (callback == null) throw new ArgumentNullException(nameof(callback));

      var workItem = new WorkItem(callback, state, options.WorkItemExecutionTimeTreshold, options.WorkItemStatusProvider);

      //statistic.OnEnQueueRequest(workItem);

      workQueue.Enqueue(workItem, forceGlobal: false);
    }

    public bool CheckHealth(DateTime lastCheckTime)
    {
      return !executingWorkTracker.HasFrozenWork();
    }

    public void Complete()
    {
      workQueue.CompleteAdding();
    }

    private void ProcessWorkItems(ExecutionContext context)
    {
      var threadLocals = workQueue.EnsureCurrentThreadHasQueue();
      //statistic.OnStartExecution();
      try
      {
        while (!ShouldStop())
        {
          while (workQueue.TryDequeue(threadLocals, out var workItem))
          {
            if (ShouldStop())
            {
              return;
            }

            context.ExecuteWithFilters(workItem);
          }

          workQueue.WaitForWork();
        }
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          return;
        }

        log.LogError(ex, SR.Executor_On_Exception, options.Name);
      }
      //finally
      //{
      //  //statistic.OnStopExecution();
      //}

      bool ShouldStop()
      {
        return context.CancellationTokenSource.IsCancellationRequested && !options.DrainAfterCancel;
      }
    }

    private void RunWorker(int index)
    {
      var actionFilters = new ActionFilter<ExecutionContext>[]
      {
        //new StatisticsTracker(statistic, options.DelayWarningThreshold, log),
        executingWorkTracker
      }.Union(options.ExecutionFilters);

      var exceptionFilters = new[] { new ThreadAbortHandler(log) }.Union(options.ExceptionFilters);

      var context = new ExecutionContext(
          actionFilters,
          exceptionFilters,
          options.CancellationTokenSource,
          index);

      new ThreadPoolThread(options.Name + index, options.CancellationTokenSource.Token)
          .QueueWorkItem(_ => ProcessWorkItems(context));
    }

    private sealed class ThreadAbortHandler : ExecutionExceptionFilter
    {
      private readonly ILogger log;

      public ThreadAbortHandler(ILogger log)
      {
        this.log = log;
      }

      public override bool ExceptionHandler(Exception ex, ExecutionContext context)
      {
        if (!(ex is ThreadAbortException))
        {
          return false;
        }

        if (log.IsDebugLevelEnabled()) log.LogDebug(ex, SR.On_Thread_Abort_Exit);
        Thread.ResetAbort();
        context.CancellationTokenSource.Cancel();
        return true;
      }
    }

    private sealed class ExecutingWorkItemsTracker : ExecutionActionFilter
    {
      private readonly WorkItem[] runningItems;

      private readonly ILogger log;

      public ExecutingWorkItemsTracker(ThreadPoolExecutor executor)
      {
        runningItems = new WorkItem[GetThreadSlot(executor.options.DegreeOfParallelism)];
        log = executor.log;
      }

      public override void OnActionExecuting(ExecutionContext context)
      {
        runningItems[GetThreadSlot(context.ThreadIndex)] = context.WorkItem;
      }

      public override void OnActionExecuted(ExecutionContext context)
      {
        runningItems[GetThreadSlot(context.ThreadIndex)] = null;
      }

      public bool HasFrozenWork()
      {
        var frozen = false;
        foreach (var workItem in runningItems)
        {
          if (workItem != null && workItem.IsFrozen())
          {
            frozen = true;
            log.LogError(SR.WorkItem_LongExecutionTime, workItem.GetWorkItemStatus(true));
          }
        }

        return frozen;
      }

      private static int GetThreadSlot(int threadIndex)
      {
        // false sharing prevention
        const int cacheLineSize = 64;
        const int padding = cacheLineSize;
        return threadIndex * padding;
      }
    }

    internal static class SR
    {
      public const string WorkItem_ExecutionTime = "WorkItem={0} Executing for {1} {2}";

      public const string WorkItem_LongExecutionTime = "Work item {0} has been executing for long time.";

      public const string Queue_Item_WaitTime = "Queue wait time of {0} for Item {1}";

      public const string On_Thread_Abort_Exit = "Received thread abort exception - exiting. {0}.";

      public const string Executor_On_Exception = "Executor {0} caught an exception.";
    }
  }

  public interface IExecutable
  {
    void Execute();
  }

  public class WorkItem : IExecutable
  {
    public static WorkItem NoOp = new WorkItem(s => { }, null, TimeSpan.MaxValue);

    private readonly WaitCallback callback;

    private readonly StatusProvider statusProvider;

    private readonly TimeSpan executionTimeTreshold;

    private readonly DateTime enqueueTime;

    private ITimeInterval executionTime;

    public WorkItem(
        WaitCallback callback,
        object state,
        TimeSpan executionTimeTreshold,
        StatusProvider statusProvider = null)
    {
      this.callback = callback;
      this.State = state;
      this.executionTimeTreshold = executionTimeTreshold;
      this.statusProvider = statusProvider ?? NoOpStatusProvider;
      this.enqueueTime = DateTime.UtcNow;
    }

    // Being tracked only when queue tracking statistic is enabled. Todo: remove implicit behavior?
    internal ITimeInterval ExecutionTime
    {
      get
      {
        EnsureExecutionTime();
        return executionTime;
      }
    }

    // for lightweight execution time tracking 
    public DateTime ExecutionStart { get; private set; }

    internal TimeSpan TimeSinceQueued => Utils.Since(enqueueTime);

    internal object State { get; }

    public void Execute()
    {
      ExecutionStart = DateTime.UtcNow;
      callback.Invoke(State);
    }

    public void EnsureExecutionTime()
    {
      if (executionTime == null)
      {
        executionTime = TimeIntervalFactory.CreateTimeInterval(true);
      }
    }

    internal string GetWorkItemStatus(bool detailed)
    {
      return string.Format(
          ThreadPoolExecutor.SR.WorkItem_ExecutionTime, State, Utils.Since(ExecutionStart),
          statusProvider?.Invoke(State, detailed));
    }

    internal bool IsFrozen()
    {
      return Utils.Since(ExecutionStart) > executionTimeTreshold;
    }

    public delegate string StatusProvider(object state, bool detailed);

    private static readonly StatusProvider NoOpStatusProvider = (s, d) => string.Empty;
  }

  public class ExecutionContext : IExecutable
  {
    private readonly FiltersApplicant<ExecutionContext> filtersApplicant;

    public ExecutionContext(
        IEnumerable<ActionFilter<ExecutionContext>> actionFilters,
        IEnumerable<ExceptionFilter<ExecutionContext>> exceptionFilters,
        CancellationTokenSource cts,
        int threadIndex)
    {
      filtersApplicant = new FiltersApplicant<ExecutionContext>(actionFilters, exceptionFilters);
      CancellationTokenSource = cts;
      ThreadIndex = threadIndex;
    }

    public CancellationTokenSource CancellationTokenSource { get; }

    public WorkItem WorkItem { get; private set; }

    internal int ThreadIndex { get; }

    public void ExecuteWithFilters(WorkItem workItem)
    {
      WorkItem = workItem;

      try
      {
        filtersApplicant.Apply(this);
      }
      finally
      {
        Reset();
      }
    }

    public void Execute()
    {
      WorkItem.Execute();
    }

    private void Reset()
    {
      WorkItem = null;
    }
  }
}
#endif
