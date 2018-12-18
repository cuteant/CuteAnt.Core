#if !NET40
using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CuteAnt.Threading;

namespace CuteAnt.Runtime
{
  public abstract class AsynchAgent : IHealthCheckable, IDisposable
  {
    public enum FaultBehavior
    {
      CrashOnFault,   // Crash the process if the agent faults
      RestartOnFault, // Restart the agent if it faults
      IgnoreFault     // Allow the agent to stop if it faults, but take no other action (other than logging)
    }

    private readonly ExecutorFaultHandler executorFaultHandler;

    protected ThreadPoolExecutor executor;
    protected CancellationTokenSource Cts;
    protected object Lockable;
    protected ILogger Log;
    protected readonly string type;
    protected FaultBehavior OnFault;
    protected bool disposed;

    public ThreadState State { get; protected set; }

    internal string Name { get; private set; }

    protected AsynchAgent() : this(null) { }

    protected AsynchAgent(string nameSuffix)
    {
      Cts = new CancellationTokenSource();
      var thisType = GetType();

      type = thisType.Namespace + "." + thisType.Name;
      if (!string.IsNullOrEmpty(nameSuffix))
      {
        Name = type + "/" + nameSuffix;
      }
      else
      {
        Name = type;
      }

      Lockable = new object();
      State = ThreadState.Unstarted;
      OnFault = FaultBehavior.IgnoreFault;

      this.Log = TraceLogger.GetLogger(Name);
      this.executorFaultHandler = new ExecutorFaultHandler(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void CurrentDomain_DomainUnload(object sender, EventArgs e)
    {
      try
      {
        if (State != ThreadState.Stopped)
        {
          Stop();
        }
      }
      catch (Exception exc)
      {
        // ignore. Just make sure DomainUnload handler does not throw.
        if (Log.IsDebugLevelEnabled()) Log.LogDebug(exc, "Ignoring error during Stop: ");
      }
    }

    public virtual void Start()
    {
      if(disposed) ThrowIfDisposed();
      lock (Lockable)
      {
        if (State == ThreadState.Running)
        {
          return;
        }

        if (State == ThreadState.Stopped)
        {
          Cts = new CancellationTokenSource();
        }

        AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        LogStatus(Log, "Starting AsyncAgent {0} on managed thread {1}", Name, Thread.CurrentThread.ManagedThreadId);
        EnsureExecutorInitialized();
        OnStart();
        State = ThreadState.Running;
      }

      if (Log.IsDebugLevelEnabled()) Log.LogDebug($"Started asynch agent {this.Name}");
    }

    public virtual void OnStart() { }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public virtual void Stop()
    {
      try
      {
        if (disposed) ThrowIfDisposed();
        lock (Lockable)
        {
          if (State == ThreadState.Running)
          {
            State = ThreadState.StopRequested;
            Cts.Cancel();
            executor = null;
            State = ThreadState.Stopped;
          }
        }

        AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
      }
      catch (Exception exc)
      {
        // ignore. Just make sure stop does not throw.
        if (Log.IsDebugLevelEnabled()) Log.LogDebug(exc, "Ignoring error during Stop: ");
      }
      if (Log.IsDebugLevelEnabled()) Log.LogDebug("Stopped agent");
    }

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || disposed) return;

      if (Cts != null)
      {
        Cts.Dispose();
        Cts = null;
      }

      disposed = true;
    }

    #endregion

    public override string ToString()
    {
      return Name;
    }

    public bool CheckHealth(DateTime lastCheckTime)
    {
      return executor.CheckHealth(lastCheckTime);
    }

    internal static bool IsStarting { get; set; }

    protected virtual ThreadPoolExecutorOptions.Builder ExecutorOptionsBuilder =>
        new ThreadPoolExecutorOptions.Builder(Name, GetType(), Cts).WithExceptionFilters(executorFaultHandler);

    private sealed class ExecutorFaultHandler : ExecutionExceptionFilter
    {
      private readonly AsynchAgent agent;

      public ExecutorFaultHandler(AsynchAgent agent)
      {
        this.agent = agent;
      }

      public override bool ExceptionHandler(Exception ex, Threading.ExecutionContext context)
      {
        if (!agent.HandleFault(ex, context))
        {
          context.CancellationTokenSource.Cancel();
        }
        return true;
      }
    }

    /// <summary>Handles fault</summary>
    /// <param name="ex"></param>
    /// <param name="context"></param>
    /// <returns>false agent has been stopped</returns>
    protected bool HandleFault(Exception ex, Threading.ExecutionContext context)
    {
      State = ThreadState.Stopped;
      if (ex is ThreadAbortException)
      {
        return false;
      }

      LogExecutorError(ex);

      if (OnFault == FaultBehavior.RestartOnFault && !Cts.IsCancellationRequested)
      {
        try
        {
          Start();
        }
        catch (Exception exc)
        {
          Log.LogError(exc, "Unable to restart AsynchAgent");
          State = ThreadState.Stopped;
        }
      }

      return State != ThreadState.Stopped;
    }

    private void EnsureExecutorInitialized()
    {
      if (executor == null)
      {
        executor = ExecutorService.GetExecutor(ExecutorOptionsBuilder.Options);
      }
    }

    private void LogExecutorError(Exception exc)
    {
      var logMessagePrefix = $"Asynch agent {Name} encountered unexpected exception";
      switch (OnFault)
      {
        case FaultBehavior.CrashOnFault:
          var logMessage = $"{logMessagePrefix} The process will be terminated.";
          Log.LogError(exc, logMessage);
          Log.LogCritical(logMessage);
          break;
        case FaultBehavior.IgnoreFault:
          Log.LogError(exc, $"{logMessagePrefix} The executor will exit.");
          break;
        case FaultBehavior.RestartOnFault:
          Log.LogError(exc, $"{logMessagePrefix} The Stage will be restarted.");
          break;
        default:
          throw new NotImplementedException();
      }
    }

    private static void LogStatus(ILogger log, string msg, params object[] args)
    {
      if (IsStarting)
      {
        // Reduce log noise during silo startup
        if (log.IsDebugLevelEnabled()) log.LogDebug(msg, args);
      }
      else
      {
        // Changes in agent threads during all operations aside for initial creation are usually important diag events.
        if (log.IsInformationLevelEnabled()) log.LogInformation(msg, args);
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowIfDisposed()
    {
      throw GetObjectDisposedException();
      ObjectDisposedException GetObjectDisposedException()
      {
        return new ObjectDisposedException("Cannot access disposed AsynchAgent");
      }
    }
  }
}
#endif
