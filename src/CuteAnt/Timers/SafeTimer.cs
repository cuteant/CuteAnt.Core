using System;
using System.Threading;

namespace CuteAnt.Runtime
{
  /// <summary>SafeTimer - A wrapper class around .NET Timer objects, with some additional built-in safeguards against edge-case errors.
  /// 
  /// SafeTimer is a replacement for .NET Timer objects, and removes some of the more infrequently used method overloads for simplification.
  /// SafeTimer provides centralization of various "guard code" previously added in various places for handling edge-case fault conditions.
  /// 
  /// Log levels used: Recovered faults => Warning, Per-Timer operations => Verbose, Per-tick operations => Verbose3 </summary>
  internal sealed class SafeTimer : IDisposable
  {
    private readonly SafeTimerBase m_safeTimerBase;
    private readonly TimerCallback m_callbackFunc;

    public SafeTimer(TimerCallback callback, object state)
    {
      m_callbackFunc = callback;
      m_safeTimerBase = new SafeTimerBase(m_callbackFunc, state);
    }

    public SafeTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
    {
      m_callbackFunc = callback;
      m_safeTimerBase = new SafeTimerBase(m_callbackFunc, state, dueTime, period);
    }

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
      m_safeTimerBase.Start(dueTime, period);
    }

    #region IDisposable Members

    public void Dispose()
    {
      m_safeTimerBase.Dispose();
    }

    // May be called by finalizer thread with disposing=false. As per guidelines, in such a case do not touch other objects.
    // Dispose() may be called multiple times
    public void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_safeTimerBase.DisposeTimer();
      }
    }

    #endregion

    internal string GetFullName()
    {
      return String.Format("SafeTimer: {0}. ", m_callbackFunc != null ? m_callbackFunc.GetType().FullName : "");
    }

    public bool CheckTimerFreeze(DateTime lastCheckTime, Func<string> callerName)
    {
      return m_safeTimerBase.CheckTimerFreeze(lastCheckTime, callerName);
    }
  }
}
