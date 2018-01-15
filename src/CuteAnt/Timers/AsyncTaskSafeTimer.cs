﻿#if !NET40
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  public class AsyncTaskSafeTimer : IDisposable
  {
    private readonly SafeTimerBase safeTimerBase;

    public AsyncTaskSafeTimer(ILogger logger, Func<object, Task> asynTaskCallback, object state)
    {
      safeTimerBase = new SafeTimerBase(logger, asynTaskCallback, state);
    }

    public AsyncTaskSafeTimer(ILogger logger, Func<object, Task> asynTaskCallback, object state, TimeSpan dueTime, TimeSpan period)
    {
      safeTimerBase = new SafeTimerBase(logger, asynTaskCallback, state, dueTime, period);
    }

    public void Start(in TimeSpan dueTime, in TimeSpan period)
    {
      safeTimerBase.Start(dueTime, period);
    }

    #region IDisposable Members

    public void Dispose()
    {
      safeTimerBase.Dispose();
    }

    // Maybe called by finalizer thread with disposing=false. As per guidelines, in such a case do not touch other objects.
    // Dispose() may be called multiple times
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "safeTimerBase")]
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        safeTimerBase.DisposeTimer();
      }
    }

    #endregion

    public bool CheckTimerFreeze(in DateTime lastCheckTime, Func<string> callerName)
    {
      return safeTimerBase.CheckTimerFreeze(lastCheckTime, callerName);
    }
  }
}
#endif