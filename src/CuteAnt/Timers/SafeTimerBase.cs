using System;
using System.Threading;
#if NET40
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.Runtime
{
  /// <summary>SafeTimerBase - an internal base class for implementing sync and async timers.</summary>
  internal class SafeTimerBase : IDisposable
  {
    private const string c_syncTimerName = "sync.SafeTimerBase";

    private static readonly ILogger s_logger = TraceLogger.GetLogger("CuteAnt.Runtime." + c_syncTimerName);//, LoggerType.Runtime);

    private Timer m_timer;
    private TimerCallback m_syncCallbackFunc;
    private TimeSpan m_dueTime;
    private TimeSpan m_timerFrequency;
    private bool m_timerStarted;
    private DateTime m_previousTickTime;
    private int m_totalNumTicks;

    public SafeTimerBase(TimerCallback syncCallback, object state)
    {
      Init(syncCallback, state, TimeoutShim.InfiniteTimeSpan, TimeoutShim.InfiniteTimeSpan);
    }

    public SafeTimerBase(TimerCallback syncCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      Init(syncCallback, state, dueTime, period);
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

    private void Init(TimerCallback synCallback, object state, TimeSpan due, TimeSpan period)
    {
      if (synCallback == null) { throw new ArgumentNullException(nameof(synCallback)); }
      if (period == TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period), period, "Cannot use TimeSpan.Zero for timer period");

      m_syncCallbackFunc = synCallback;
      m_timerFrequency = period;
      this.m_dueTime = due;
      m_totalNumTicks = 0;

      if (s_logger.IsTraceLevelEnabled()) s_logger.LogTrace(ErrorCode.TimerChanging, "Creating timer {0} with dueTime={1} period={2}", GetFullName(), due, period);

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
          if (s_logger.IsTraceLevelEnabled()) s_logger.LogTrace(ErrorCode.TimerDisposing, "Disposing timer {0}", GetFullName());
          t.Dispose();

        }
        catch (Exception exc)
        {
          s_logger.LogWarning(ErrorCode.TimerDisposeError, exc, "Ignored error disposing timer {0}", GetFullName());
        }
      }
    }

    #endregion

    private string GetFullName() => c_syncTimerName;

    public bool CheckTimerFreeze(DateTime lastCheckTime, Func<string> callerName)
    {
      return CheckTimerDelay(m_previousTickTime, m_totalNumTicks,
                  m_dueTime, m_timerFrequency, s_logger, () => String.Format("{0}.{1}", GetFullName(), callerName()), ErrorCode.Timer_SafeTimerIsNotTicking, true);
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

      if (s_logger.IsTraceLevelEnabled()) s_logger.LogTrace(ErrorCode.TimerChanging, "Changing timer {0} to dueTime={1} period={2}", GetFullName(), newDueTime, period);

      try
      {
        // Queue first new timer tick
        return m_timer.Change(newDueTime, TimeoutShim.InfiniteTimeSpan);
      }
      catch (Exception exc)
      {
        s_logger.LogWarning(ErrorCode.TimerChangeError, exc, "Error changing timer period - timer {0} not changed", GetFullName());
        return false;
      }
    }

    private void HandleTimerCallback(object state)
    {
      if (m_timer == null) return;

      HandleSyncTimerCallback(state);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void HandleSyncTimerCallback(object state)
    {
      try
      {
        if (s_logger.IsDebugLevelEnabled()) s_logger.LogDebug(ErrorCode.TimerBeforeCallback, "About to make sync timer callback for timer {0}", GetFullName());
        m_syncCallbackFunc(state);
        if (s_logger.IsDebugLevelEnabled()) s_logger.LogDebug(ErrorCode.TimerAfterCallback, "Completed sync timer callback for timer {0}", GetFullName());
      }
      catch (Exception exc)
      {
        s_logger.LogWarning(ErrorCode.TimerCallbackError, exc, "Ignored exception {0} during sync timer callback {1}", exc.Message, GetFullName());
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

        if (s_logger.IsDebugLevelEnabled()) s_logger.LogDebug(ErrorCode.TimerChanging, "About to QueueNextTimerTick for timer {0}", GetFullName());

        if (m_timerFrequency == TimeoutShim.InfiniteTimeSpan)
        {
          //timer.Change(Constants.INFINITE_TIMESPAN, Constants.INFINITE_TIMESPAN);
          DisposeTimer();

          if (s_logger.IsTraceLevelEnabled()) s_logger.LogTrace(ErrorCode.TimerStopped, "Timer {0} is now stopped and disposed", GetFullName());
        }
        else
        {
          m_timer.Change(m_timerFrequency, TimeoutShim.InfiniteTimeSpan);

          if (s_logger.IsDebugLevelEnabled()) s_logger.LogDebug(ErrorCode.TimerNextTick, "Queued next tick for timer {0} in {1}", GetFullName(), m_timerFrequency);
        }
      }
      catch (ObjectDisposedException ode)
      {
        s_logger.LogWarning(ErrorCode.TimerDisposeError, ode, "Timer {0} already disposed - will not queue next timer tick", GetFullName());
      }
      catch (Exception exc)
      {
        s_logger.LogError(ErrorCode.TimerQueueTickError, exc, "Error queueing next timer tick - WARNING: timer {0} is now stopped", GetFullName());
      }
    }

    #region ** class ErrorCode **

    sealed class ErrorCode
    {
      internal const int Runtime = 100000;

      internal const int PerfCounterBase = Runtime + 700;
      internal const int PerfCounterTimerError = PerfCounterBase + 17;

      internal const int TimerBase = Runtime + 1400;
      internal const int TimerChangeError = PerfCounterTimerError; // Backward compatability

      internal const int TimerCallbackError = Runtime + 37; // Backward compatability
      internal const int TimerDisposeError = TimerBase + 1;
      internal const int TimerStopError = TimerBase + 2;
      internal const int TimerQueueTickError = TimerBase + 3;
      internal const int TimerChanging = TimerBase + 4;
      internal const int TimerBeforeCallback = TimerBase + 5;
      internal const int TimerAfterCallback = TimerBase + 6;
      internal const int TimerNextTick = TimerBase + 7;
      internal const int TimerDisposing = TimerBase + 8;
      internal const int TimerStopped = TimerBase + 9;
      internal const int Timer_TimerInsideGrainIsNotTicking = TimerBase + 10;
      internal const int Timer_TimerInsideGrainIsDelayed = TimerBase + 11;
      internal const int Timer_SafeTimerIsNotTicking = TimerBase + 12;
      internal const int Timer_GrainTimerCallbackError = TimerBase + 13;
      internal const int Timer_InvalidContext = TimerBase + 14;
    }

    #endregion
  }
}
