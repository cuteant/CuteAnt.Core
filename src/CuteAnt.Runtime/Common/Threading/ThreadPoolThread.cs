#if !NET40
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using CuteAnt.Runtime;

namespace CuteAnt.Threading
{
  public class ThreadPoolThread
  {
    private readonly CancellationToken cancellationToken;

    private readonly ILogger log;

    public ThreadPoolThread(string name, CancellationToken cancellationToken)
    {
      this.Name = name;
      this.cancellationToken = cancellationToken;
      this.log = TraceLogger.GetLogger<ThreadPoolThread>();
    }

    public string Name { get; }

    public void QueueWorkItem(WaitCallback callback, object state = null)
    {
      if (callback == null) throw new ArgumentNullException(nameof(callback));

      new Thread(() =>
      {
        try
        {
          if (log.IsInformationLevelEnabled()) log.LogInformation(SR.Starting_Thread, Name, Thread.CurrentThread.ManagedThreadId);

          callback.Invoke(state);
        }
        catch (Exception exc)
        {
          HandleExecutionException(exc);
        }
        finally
        {
          if (log.IsInformationLevelEnabled()) log.LogInformation(SR.Stopping_Thread, Name, Thread.CurrentThread.ManagedThreadId);
        }
      })
      {
        IsBackground = true,
        Name = Name
      }.Start();
    }

    private void HandleExecutionException(Exception exc)
    {
      if (cancellationToken.IsCancellationRequested) return;

      log.LogError(exc, SR.Thread_On_Exception, Name);
    }

    private static class SR
    {
      public const string Starting_Thread = "Starting thread {0} on managed thread {1}";

      public const string Stopping_Thread = "Stopping Thread {0} on managed thread {1}";

      public const string Thread_On_Exception = "Executor thread {0} encountered unexpected exception:";
    }
  }
}
#endif
