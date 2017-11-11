using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  /// <summary>SafeTimerBase - an internal base class for implementing sync and async timers.</summary>
  public class SafeTimerBase : IDisposable
  {
    private const string c_asyncTimerName = "asynTask.SafeTimerBase";
    private const string c_syncTimerName = "sync.SafeTimerBase";

    private static readonly ILogger asyncLogger = TraceLogger.GetLogger("CuteAnt.Runtime." + c_asyncTimerName);//, LoggerType.Runtime);
    private static readonly ILogger syncLogger = TraceLogger.GetLogger("CuteAnt.Runtime." + c_syncTimerName);//, LoggerType.Runtime);

    private Timer m_timer;
    private Func<object, Task> m_asynTaskCallback;
    private TimerCallback m_syncCallbackFunc;
    private TimeSpan m_dueTime;
    private TimeSpan m_timerFrequency;
    private bool m_timerStarted;
    private DateTime m_previousTickTime;
    private int m_totalNumTicks;
    private ILogger m_logger;

    public SafeTimerBase(Func<object, Task> asynTaskCallback, object state)
    {
      Init(asynTaskCallback, null, state, TimeoutShim.InfiniteTimeSpan, TimeoutShim.InfiniteTimeSpan);
    }

    public SafeTimerBase(Func<object, Task> asynTaskCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      Init(asynTaskCallback, null, state, dueTime, period);
      Start(dueTime, period);
    }

    public SafeTimerBase(TimerCallback syncCallback, object state)
    {
      Init(null, syncCallback, state, TimeoutShim.InfiniteTimeSpan, TimeoutShim.InfiniteTimeSpan);
    }

    public SafeTimerBase(TimerCallback syncCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      Init(null, syncCallback, state, dueTime, period);
      Start(dueTime, period);
    }

    public void Start(TimeSpan due, TimeSpan period)
    {
      if (m_timerStarted) throw new InvalidOperationException(String.Format("Calling start on timer {0} is not allowed, since it was already created in a started mode with specified due.", GetFullName()));
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period), period, "Cannot use TimeSpan.Zero for timer period");

      m_timerFrequency = period;
      m_dueTime = due;
      m_timerStarted = true;
      m_previousTickTime = DateTime.UtcNow;
      m_timer.Change(due, TimeoutShim.InfiniteTimeSpan);
    }

    private void Init(Func<object, Task> asynCallback, TimerCallback synCallback, object state, TimeSpan due, TimeSpan period)
    {
      if (synCallback == null && asynCallback == null) throw new ArgumentNullException(nameof(synCallback), "Cannot use null for both sync and asyncTask timer callbacks.");
      int numNonNulls = (asynCallback != null ? 1 : 0) + (synCallback != null ? 1 : 0);
      if (numNonNulls > 1) throw new ArgumentNullException(nameof(synCallback), "Cannot define more than one timer callbacks. Pick one.");
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period), period, "Cannot use TimeSpan.Zero for timer period");

      this.m_asynTaskCallback = asynCallback;
      m_syncCallbackFunc = synCallback;
      m_timerFrequency = period;
      this.m_dueTime = due;
      m_totalNumTicks = 0;

      m_logger = m_syncCallbackFunc != null ? syncLogger : asyncLogger;
      if (m_logger.IsTraceLevelEnabled()) m_logger.LogTrace(ErrorCode.TimerChanging, "Creating timer {0} with dueTime={1} period={2}", GetFullName(), due, period);

      m_timer = new Timer(HandleTimerCallback, state, TimeoutShim.InfiniteTimeSpan, TimeoutShim.InfiniteTimeSpan);
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
      if (m_timer != null)
      {
        try
        {
          var t = m_timer;
          m_timer = null;
          if (m_logger.IsTraceLevelEnabled()) m_logger.LogTrace(ErrorCode.TimerDisposing, "Disposing timer {0}", GetFullName());
          t.Dispose();

        }
        catch (Exception exc)
        {
          m_logger.LogWarning(ErrorCode.TimerDisposeError, exc, "Ignored error disposing timer {0}", GetFullName());
        }
      }
    }

    #endregion

    private string GetFullName()
    {
      // the type information is really useless and just too long. 
      if (m_syncCallbackFunc != null)
        return c_syncTimerName;
      if (m_asynTaskCallback != null)
        return c_asyncTimerName;

      throw new InvalidOperationException("invalid SafeTimerBase state");
    }

    public bool CheckTimerFreeze(DateTime lastCheckTime, Func<string> callerName)
    {
      return CheckTimerDelay(m_previousTickTime, m_totalNumTicks,
                  m_dueTime, m_timerFrequency, m_logger, () => String.Format("{0}.{1}", GetFullName(), callerName()), ErrorCode.Timer_SafeTimerIsNotTicking, true);
    }

    public static bool CheckTimerDelay(DateTime previousTickTime, int totalNumTicks,
                    TimeSpan dueTime, TimeSpan timerFrequency, ILogger logger, Func<string> getName, int errorCode, bool freezeCheck)
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
          previousTickTime.Format(), // 2
          timeSinceLastTick,                  // 3
          exceptedTimeToNexTick);             // 4

      if (freezeCheck)
      {
        logger.LogError(errorCode, errMsg);
      }
      else
      {
        logger.LogWarning(errorCode, errMsg);
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
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period), period, string.Format("Cannot use TimeSpan.Zero for timer {0} period", GetFullName()));

      if (m_timer == null) return false;

      m_timerFrequency = period;

      if (m_logger.IsTraceLevelEnabled()) m_logger.LogTrace(ErrorCode.TimerChanging, "Changing timer {0} to dueTime={1} period={2}", GetFullName(), newDueTime, period);

      try
      {
        // Queue first new timer tick
        return m_timer.Change(newDueTime, TimeoutShim.InfiniteTimeSpan);
      }
      catch (Exception exc)
      {
        m_logger.LogWarning(ErrorCode.TimerChangeError, exc, "Error changing timer period - timer {0} not changed", GetFullName());
        return false;
      }
    }

    private void HandleTimerCallback(object state)
    {
      if (m_timer == null) return;

      if (m_asynTaskCallback != null)
      {
        HandleAsyncTaskTimerCallback(state);
      }
      else
      {
        HandleSyncTimerCallback(state);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void HandleSyncTimerCallback(object state)
    {
      try
      {
        var debugEnabled = m_logger.IsDebugLevelEnabled();
        if (debugEnabled) m_logger.LogDebug(ErrorCode.TimerBeforeCallback, "About to make sync timer callback for timer {0}", GetFullName());
        m_syncCallbackFunc(state);
        if (debugEnabled) m_logger.LogDebug(ErrorCode.TimerAfterCallback, "Completed sync timer callback for timer {0}", GetFullName());
      }
      catch (Exception exc)
      {
        m_logger.LogWarning(ErrorCode.TimerCallbackError, exc, "Ignored exception {0} during sync timer callback {1}", exc.Message, GetFullName());
      }
      finally
      {
        m_previousTickTime = DateTime.UtcNow;
        // Queue next timer callback
        QueueNextTimerTick();
      }
    }

    private async void HandleAsyncTaskTimerCallback(object state)
    {
      if (m_timer == null) return;

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
        var debugEnabled = m_logger.IsDebugLevelEnabled();
        if (debugEnabled) m_logger.LogDebug(ErrorCode.TimerBeforeCallback, "About to make async task timer callback for timer {0}", GetFullName());
        await m_asynTaskCallback(state);
        if (debugEnabled) m_logger.LogDebug(ErrorCode.TimerAfterCallback, "Completed async task timer callback for timer {0}", GetFullName());
      }
      catch (Exception exc)
      {
        m_logger.LogWarning(ErrorCode.TimerCallbackError, exc, "Ignored exception {0} during async task timer callback {1}", exc.Message, GetFullName());
      }
      finally
      {
        m_previousTickTime = DateTime.UtcNow;
        // Queue next timer callback
        QueueNextTimerTick();
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void QueueNextTimerTick()
    {
      try
      {
        if (m_timer == null) return;

        m_totalNumTicks++;

        var debugEnabled = m_logger.IsDebugLevelEnabled();
        if (debugEnabled) m_logger.LogDebug(ErrorCode.TimerChanging, "About to QueueNextTimerTick for timer {0}", GetFullName());

        if (m_timerFrequency == TimeoutShim.InfiniteTimeSpan)
        {
          //timer.Change(Constants.INFINITE_TIMESPAN, Constants.INFINITE_TIMESPAN);
          DisposeTimer();

          if (m_logger.IsTraceLevelEnabled()) m_logger.LogTrace(ErrorCode.TimerStopped, "Timer {0} is now stopped and disposed", GetFullName());
        }
        else
        {
          m_timer.Change(m_timerFrequency, TimeoutShim.InfiniteTimeSpan);

          if (debugEnabled) m_logger.LogDebug(ErrorCode.TimerNextTick, "Queued next tick for timer {0} in {1}", GetFullName(), m_timerFrequency);
        }
      }
      catch (ObjectDisposedException ode)
      {
        m_logger.LogWarning(ErrorCode.TimerDisposeError, ode, "Timer {0} already disposed - will not queue next timer tick", GetFullName());
      }
      catch (Exception exc)
      {
        m_logger.LogError(ErrorCode.TimerQueueTickError, exc, "Error queueing next timer tick - WARNING: timer {0} is now stopped", GetFullName());
      }
    }
  }
}
