using System;
using System.Threading.Tasks;

namespace CuteAnt.Runtime
{
  public class AsyncTaskSafeTimer : IDisposable
  {
    private readonly SafeTimerBase m_safeTimerBase;

    public AsyncTaskSafeTimer(Func<object, Task> asynTaskCallback, object state)
    {
      m_safeTimerBase = new SafeTimerBase(asynTaskCallback, state);
    }

    public AsyncTaskSafeTimer(Func<object, Task> asynTaskCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      m_safeTimerBase = new SafeTimerBase(asynTaskCallback, state, dueTime, period);
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

    // Maybe called by finalizer thread with disposing=false. As per guidelines, in such a case do not touch other objects.
    // Dispose() may be called multiple times
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "safeTimerBase")]
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_safeTimerBase.DisposeTimer();
      }
    }

    #endregion

    public bool CheckTimerFreeze(DateTime lastCheckTime, Func<string> callerName)
    {
      return m_safeTimerBase.CheckTimerFreeze(lastCheckTime, callerName);
    }
  }
}
