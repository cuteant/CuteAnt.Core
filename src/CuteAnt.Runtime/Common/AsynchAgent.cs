using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  public abstract class AsynchAgent : IDisposable
  {
    public enum FaultBehavior
    {
      CrashOnFault,   // Crash the process if the agent faults
      RestartOnFault, // Restart the agent if it faults
      IgnoreFault     // Allow the agent to stop if it faults, but take no other action (other than logging)
    }

#if NET40
    private Thread m_thread;
#endif
    private CancellationTokenSource _cts;
    protected CancellationTokenSource Cts => _cts;
    protected object Lockable;
    protected ILogger Log;
    private readonly string type;
    protected FaultBehavior OnFault;

    public ThreadState State { get; private set; }
    public string Name { get; protected set; }
#if NET40
    public int ManagedThreadId { get { return m_thread == null ? -1 : m_thread.ManagedThreadId; } }
#endif

    protected AsynchAgent()
      : this(null)
    {
    }

    protected AsynchAgent(string nameSuffix)
    {
      _cts = new CancellationTokenSource();
      var thisType = GetType();

      type = thisType.Namespace + "." + thisType.Name;
      //if (type.StartsWith("CuteAnt.Wings.", StringComparison.Ordinal))
      //{
      //  type = type.Substring(8);
      //}
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
      Log = TraceLogger.GetLogger(Name);//, TraceLogger.LoggerType.Runtime);
      AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

#if NET40
      m_thread = new Thread(AgentThreadProc) { IsBackground = true, Name = this.Name };
#endif
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
        if (Log.IsDebugLevelEnabled()) Log.LogDebug(exc, "Ignoring error during Stop: {0}");
      }
    }

    public virtual void Start()
    {
      lock (Lockable)
      {
        if (State == ThreadState.Running)
        {
          return;
        }

        if (State == ThreadState.Stopped)
        {
          _cts = new CancellationTokenSource();
#if NET40
          m_thread = new Thread(AgentThreadProc) { IsBackground = true, Name = this.Name };
#endif
        }

#if NET40
        m_thread.Start(this);
#else
        ExecutorService.RunTask(new AsynchAgentTask(() => AgentThreadProc(this), Name));
#endif
        State = ThreadState.Running;
      }
      if (Log.IsDebugLevelEnabled()) Log.LogDebug("Started asynch agent " + this.Name);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public virtual void Stop()
    {
      try
      {
        lock (Lockable)
        {
          if (State == ThreadState.Running)
          {
            State = ThreadState.StopRequested;
            _cts.Cancel();
            State = ThreadState.Stopped;
          }
        }
        AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
      }
      catch (Exception exc)
      {
        // ignore. Just make sure stop does not throw.
        if (Log.IsDebugLevelEnabled()) Log.LogDebug(exc, "Ignoring error during Stop: {0}");
      }
      if (Log.IsDebugLevelEnabled()) Log.LogDebug("Stopped agent");
    }

#if NET40
    public void Abort(object stateInfo)
    {
      if (m_thread != null)
        m_thread.Abort(stateInfo);
    }

    public void Join(in TimeSpan timeout)
    {
      try
      {
        var agentThread = m_thread;
        if (agentThread != null)
        {
          bool joined = agentThread.Join((int)timeout.TotalMilliseconds);
          if (Log.IsDebugLevelEnabled()) Log.LogDebug("{0} the agent thread {1} after {2} time.", joined ? "Joined" : "Did not join", Name, timeout);
        }
      }
      catch (Exception exc)
      {
        // ignore. Just make sure Join does not throw.
        if (Log.IsDebugLevelEnabled()) Log.LogDebug("Ignoring error during Join: {0}", exc);
      }
    }
#endif

    protected abstract void Run();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static void AgentThreadProc(Object obj)
    {
      var agent = obj as AsynchAgent;
      if (agent == null)
      {
        throw new InvalidOperationException("Agent thread started with incorrect parameter type");
      }

      var agentLog = agent.Log;
      try
      {
        LogStatus(agentLog, "Starting AsyncAgent {0} on managed thread {1}", agent.Name, Thread.CurrentThread.ManagedThreadId);
        agent.Run();
      }
      catch (Exception exc)
      {
        if (agent.State == ThreadState.Running) // If we're stopping, ignore exceptions
        {
          switch (agent.OnFault)
          {
            case FaultBehavior.CrashOnFault:
              //Console.WriteLine(
              //   "The {0} agent has thrown an unhandled exception, {1}. The process will be terminated.",
              //   agent.Name, exc);
              agentLog.LogError(exc, "AsynchAgent Run method has thrown an unhandled exception. The process will be terminated.");
              agentLog.LogCritical("Terminating process because of an unhandled exception caught in AsynchAgent.Run.");
              break;
            case FaultBehavior.IgnoreFault:
              agentLog.LogError(exc, "AsynchAgent Run method has thrown an unhandled exception. The agent will exit.");
              agent.State = ThreadState.Stopped;
              break;
            case FaultBehavior.RestartOnFault:
              agentLog.LogError(exc, "AsynchAgent Run method has thrown an unhandled exception. The agent will be restarted.");
              agent.State = ThreadState.Stopped;
              try
              {
                agent.Start();
              }
              catch (Exception ex)
              {
                agentLog.LogError(ex, "Unable to restart AsynchAgent");
                agent.State = ThreadState.Stopped;
              }
              break;
          }
        }
      }
      finally
      {
        if (agentLog.IsInformationLevelEnabled()) agentLog.LogInformation("Stopping AsyncAgent {0} that runs on managed thread {1}", agent.Name, Thread.CurrentThread.ManagedThreadId);
      }
    }

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) return;

      if (_cts != null)
      {
        _cts.Dispose();
        _cts = null;
      }
    }

    #endregion

    public override string ToString()
    {
      return Name;
    }

    public static bool IsStarting { get; set; }

    private static void LogStatus(ILogger log, string msg, params object[] args)
    {
      if (IsStarting)
      {
        // Reduce log noise during app startup
        if (log.IsDebugLevelEnabled()) log.LogDebug(msg, args);
      }
      else
      {
        // Changes in agent threads during all operations aside for initial creation are usually important diag events.
        if (log.IsDebugLevelEnabled()) log.LogInformation(msg, args);
      }
    }
  }
}
