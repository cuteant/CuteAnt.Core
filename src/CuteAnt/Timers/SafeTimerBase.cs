using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  /// <summary>SafeTimerBase - an internal base class for implementing sync and async timers.</summary>
  public class SafeTimerBase : IDisposable
  {
#if !NET40
    private const string asyncTimerName = "CuteAnt.Runtime.asynTask.SafeTimerBase";
#endif
    private const string syncTimerName = "CuteAnt.Runtime.sync.SafeTimerBase";

    // This needs to be first, as GrainId static initializers reference it. Otherwise, GrainId actually see a uninitialized (ie Zero) value for that "constant"!
    private static readonly TimeSpan INFINITE_TIMESPAN = TimeSpan.FromMilliseconds(-1);

    private Timer timer;
#if !NET40
    private Func<object, Task> asynTaskCallback;
#endif
    private TimerCallback syncCallbackFunc;
    private TimeSpan dueTime;
    private TimeSpan timerFrequency;
    private bool timerStarted;
    private DateTime previousTickTime;
    private int totalNumTicks;
    private ILogger logger;

#if !NET40
    internal SafeTimerBase(ILogger logger, Func<object, Task> asynTaskCallback, object state)
    {
      Init(logger, asynTaskCallback, null, state, INFINITE_TIMESPAN, INFINITE_TIMESPAN);
    }

    internal SafeTimerBase(ILogger logger, Func<object, Task> asynTaskCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      Init(logger, asynTaskCallback, null, state, dueTime, period);
      Start(dueTime, period);
    }
#endif

    internal SafeTimerBase(ILogger logger, TimerCallback syncCallback, object state)
    {
      Init(logger, null, syncCallback, state, INFINITE_TIMESPAN, INFINITE_TIMESPAN);
    }

    internal SafeTimerBase(ILogger logger, TimerCallback syncCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      Init(logger, null, syncCallback, state, dueTime, period);
      Start(dueTime, period);
    }

    public void Start(TimeSpan due, TimeSpan period)
    {
      if (timerStarted) throw new InvalidOperationException(String.Format("Calling start on timer {0} is not allowed, since it was already created in a started mode with specified due.", GetFullName()));
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException("period", period, "Cannot use TimeSpan.Zero for timer period");

      timerFrequency = period;
      dueTime = due;
      timerStarted = true;
      previousTickTime = DateTime.UtcNow;
      timer.Change(due, INFINITE_TIMESPAN);
    }

    private void Init(ILogger logger, Func<object, Task> asynCallback, TimerCallback synCallback, object state, TimeSpan due, TimeSpan period)
    {
      if (synCallback == null && asynCallback == null) throw new ArgumentNullException("synCallback", "Cannot use null for both sync and asyncTask timer callbacks.");
      int numNonNulls = (asynCallback != null ? 1 : 0) + (synCallback != null ? 1 : 0);
      if (numNonNulls > 1) throw new ArgumentNullException("synCallback", "Cannot define more than one timer callbacks. Pick one.");
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException("period", period, "Cannot use TimeSpan.Zero for timer period");

#if !NET40
      this.asynTaskCallback = asynCallback;
#endif
      syncCallbackFunc = synCallback;
      timerFrequency = period;
      this.dueTime = due;
      totalNumTicks = 0;
      this.logger = logger;
      if (logger.IsDebugLevelEnabled()) logger.LogDebug("Creating timer {0} with dueTime={1} period={2}", GetFullName(), due, period);

      timer = new Timer(HandleTimerCallback, state, INFINITE_TIMESPAN, INFINITE_TIMESPAN);
    }

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    // Maybe called by finalizer thread with disposing=false. As per guidelines, in such a case do not touch other objects.
    // Dispose() may be called multiple times
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        DisposeTimer();
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    internal void DisposeTimer()
    {
      if (timer != null)
      {
        try
        {
          var t = timer;
          timer = null;
          if (logger.IsDebugLevelEnabled()) logger.LogDebug("Disposing timer {0}", GetFullName());
          t.Dispose();

        }
        catch (Exception exc)
        {
          logger.LogWarning(exc, "Ignored error disposing timer {0}", GetFullName());
        }
      }
    }

    #endregion

    private string GetFullName()
    {
#if !NET40
      // the type information is really useless and just too long. 
      if (syncCallbackFunc != null) { return syncTimerName; }
      if (asynTaskCallback != null) { return asyncTimerName; }

      throw new InvalidOperationException("invalid SafeTimerBase state");
#else
      return syncTimerName;
#endif
    }

    public bool CheckTimerFreeze(DateTime lastCheckTime, Func<string> callerName)
    {
      return CheckTimerDelay(previousTickTime, totalNumTicks,
                  dueTime, timerFrequency, logger, () => String.Format("{0}.{1}", GetFullName(), callerName()), true);
    }

    public static bool CheckTimerDelay(DateTime previousTickTime, int totalNumTicks,
                    TimeSpan dueTime, TimeSpan timerFrequency, ILogger logger, Func<string> getName, bool freezeCheck)
    {
      TimeSpan timeSinceLastTick = DateTime.UtcNow - previousTickTime;
      TimeSpan exceptedTimeToNexTick = totalNumTicks == 0 ? dueTime : timerFrequency;
      TimeSpan exceptedTimeWithSlack;
      if (exceptedTimeToNexTick >= TimeSpan.FromSeconds(6))
      {
        exceptedTimeWithSlack = exceptedTimeToNexTick + TimeSpan.FromSeconds(3);
      }
      else
      {
        exceptedTimeWithSlack = exceptedTimeToNexTick.Multiply(1.5);
      }
      if (timeSinceLastTick <= exceptedTimeWithSlack) return true;

      // did not tick in the last period.
      var errMsg = String.Format("{0}{1} did not fire on time. Last fired at {2}, {3} since previous fire, should have fired after {4}.",
          freezeCheck ? "Watchdog Freeze Alert: " : "-", // 0
          getName == null ? "" : getName(),   // 1
          TraceLogger.PrintDate(previousTickTime), // 2
          timeSinceLastTick,                  // 3
          exceptedTimeToNexTick);             // 4

      if (freezeCheck)
      {
        logger.LogError(errMsg);
      }
      else
      {
        logger.LogError(errMsg);
      }
      return false;
    }

    /// <summary>
    /// Changes the start time and the interval between method invocations for a timer, using TimeSpan values to measure time intervals.
    /// </summary>
    /// <param name="newDueTime">A TimeSpan representing the amount of time to delay before invoking the callback method specified when the Timer was constructed. Specify negative one (-1) milliseconds to prevent the timer from restarting. Specify zero (0) to restart the timer immediately.</param>
    /// <param name="period">The time interval between invocations of the callback method specified when the Timer was constructed. Specify negative one (-1) milliseconds to disable periodic signaling.</param>
    /// <returns><c>true</c> if the timer was successfully updated; otherwise, <c>false</c>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private bool Change(TimeSpan newDueTime, TimeSpan period)
    {
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException("period", period, string.Format("Cannot use TimeSpan.Zero for timer {0} period", GetFullName()));

      if (timer == null) return false;

      timerFrequency = period;

      if (logger.IsDebugLevelEnabled()) logger.LogDebug("Changing timer {0} to dueTime={1} period={2}", GetFullName(), newDueTime, period);

      try
      {
        // Queue first new timer tick
        return timer.Change(newDueTime, INFINITE_TIMESPAN);
      }
      catch (Exception exc)
      {
        logger.LogWarning(exc, "Error changing timer period - timer {0} not changed", GetFullName());
        return false;
      }
    }

    private void HandleTimerCallback(object state)
    {
      if (timer == null) return;

#if !NET40
      if (asynTaskCallback != null)
      {
        HandleAsyncTaskTimerCallback(state);
      }
      else
      {
        HandleSyncTimerCallback(state);
      }
#else
        HandleSyncTimerCallback(state);
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void HandleSyncTimerCallback(object state)
    {
      try
      {
        var traceEnabled = logger.IsTraceLevelEnabled();
        if (traceEnabled) logger.LogTrace("About to make sync timer callback for timer {0}", GetFullName());
        syncCallbackFunc(state);
        if (traceEnabled) logger.LogTrace("Completed sync timer callback for timer {0}", GetFullName());
      }
      catch (Exception exc)
      {
        logger.LogWarning(exc, "Ignored exception {0} during sync timer callback {1}", exc.Message, GetFullName());
      }
      finally
      {
        previousTickTime = DateTime.UtcNow;
        // Queue next timer callback
        QueueNextTimerTick();
      }
    }

#if !NET40
    private async void HandleAsyncTaskTimerCallback(object state)
    {
      if (timer == null) return;

      // There is a subtle race/issue here w.r.t unobserved promises.
      // It may happen than the asyncCallbackFunc will resolve some promises on which the higher level application code is depends upon
      // and this promise's await or CW will fire before the below code (after await or Finally) even runs.
      // In the unit test case this may lead to the situation where unit test has finished, but p1 or p2 or p3 have not been observed yet.
      // To properly fix this we may use a mutex/monitor to delay execution of asyncCallbackFunc until all CWs and Finally in the code below were scheduled 
      // (not until CW lambda was run, but just until CW function itself executed). 
      // This however will relay on scheduler executing these in separate threads to prevent deadlock, so needs to be done carefully. 
      // In particular, need to make sure we execute asyncCallbackFunc in another thread (so use StartNew instead of ExecuteWithSafeTryCatch).

      try
      {
        var traceEnabled = logger.IsTraceLevelEnabled();
        if (traceEnabled) logger.LogTrace("About to make async task timer callback for timer {0}", GetFullName());
        await asynTaskCallback(state);
        if (traceEnabled) logger.LogTrace("Completed async task timer callback for timer {0}", GetFullName());
      }
      catch (Exception exc)
      {
        logger.LogWarning(exc, "Ignored exception {0} during async task timer callback {1}", exc.Message, GetFullName());
      }
      finally
      {
        previousTickTime = DateTime.UtcNow;
        // Queue next timer callback
        QueueNextTimerTick();
      }
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void QueueNextTimerTick()
    {
      try
      {
        if (timer == null) { return; }

        totalNumTicks++;

        var traceEnabled = logger.IsTraceLevelEnabled();
        if (traceEnabled) logger.LogTrace("About to QueueNextTimerTick for timer {0}", GetFullName());

        if (timerFrequency == INFINITE_TIMESPAN)
        {
          //timer.Change(Constants.INFINITE_TIMESPAN, Constants.INFINITE_TIMESPAN);
          DisposeTimer();

          if (traceEnabled) logger.LogTrace("Timer {0} is now stopped and disposed", GetFullName());
        }
        else
        {
          timer.Change(timerFrequency, INFINITE_TIMESPAN);

          if (traceEnabled) logger.LogTrace("Queued next tick for timer {0} in {1}", GetFullName(), timerFrequency);
        }
      }
      catch (ObjectDisposedException ode)
      {
        logger.LogWarning(ode, "Timer {0} already disposed - will not queue next timer tick", GetFullName());
      }
      catch (Exception exc)
      {
        logger.LogError(exc, "Error queueing next timer tick - WARNING: timer {0} is now stopped", GetFullName());
      }
    }
  }
}
