using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  public class Watchdog : AsynchAgent
  {
    private static readonly TimeSpan s_heartbeatPeriod = TimeSpan.FromMilliseconds(1000);
    private readonly TimeSpan m_healthCheckPeriod;
    private DateTime m_lastHeartbeat;
    private DateTime m_lastWatchdogCheck;
    private readonly List<IHealthCheckParticipant> m_participants;
    private readonly ILogger m_logger;
    private readonly CounterStatistic m_watchdogChecks;
    private CounterStatistic m_watchdogFailedChecks;

    public Watchdog(TimeSpan watchdogPeriod, List<IHealthCheckParticipant> watchables)
    {
      m_logger = TraceLogger.GetLogger<Watchdog>();
      m_healthCheckPeriod = watchdogPeriod;
      m_participants = watchables;
      m_watchdogChecks = CounterStatistic.FindOrCreate(StatisticNames.WATCHDOG_NUM_HEALTH_CHECKS);
    }

    public override void Start()
    {
      if (m_logger.IsInformationLevelEnabled()) { m_logger.LogInformation("Starting Silo Watchdog."); }
      var now = DateTime.UtcNow;
      m_lastHeartbeat = now;
      m_lastWatchdogCheck = now;
      base.Start();
    }

    #region Overrides of AsynchAgent

    protected override void Run()
    {
      while (!Cts.IsCancellationRequested)
      {
        try
        {
          WatchdogHeartbeatTick(null);
          Thread.Sleep(s_heartbeatPeriod);
        }
#if !NETSTANDARD
        catch (ThreadAbortException)
        {
          // Silo is probably shutting-down, so just ignore and exit
        }
#endif
        catch (Exception exc)
        {
          m_logger.LogInformation(ErrorCode.Watchdog_InternalError, exc, "Watchdog Internal Error.");
        }
      }
    }

    #endregion

    private void WatchdogHeartbeatTick(object state)
    {
      try
      {
        CheckYourOwnHealth(m_lastHeartbeat, m_logger);
      }
      finally
      {
        m_lastHeartbeat = DateTime.UtcNow;
      }

      var timeSinceLastWatchdogCheck = (DateTime.UtcNow - m_lastWatchdogCheck);
      if (timeSinceLastWatchdogCheck <= m_healthCheckPeriod) return;

      m_watchdogChecks.Increment();
      int numFailedChecks = 0;
      foreach (IHealthCheckParticipant participant in m_participants)
      {
        try
        {
          bool ok = participant.CheckHealth(m_lastWatchdogCheck);
          if (!ok)
            numFailedChecks++;
        }
        catch (Exception exc)
        {
          m_logger.LogWarning(ErrorCode.Watchdog_ParticipantThrownException, exc, "HealthCheckParticipant {0} has thrown an exception from its CheckHealth method.", participant.ToString());
        }
      }
      if (numFailedChecks > 0)
      {
        if (m_watchdogFailedChecks == null)
          m_watchdogFailedChecks = CounterStatistic.FindOrCreate(StatisticNames.WATCHDOG_NUM_FAILED_HEALTH_CHECKS);

        m_watchdogFailedChecks.Increment();
        m_logger.LogWarning(ErrorCode.Watchdog_HealthCheckFailure, "Watchdog had {0} Health Check Failure(s) out of {1} Health Check Participants.", numFailedChecks, m_participants.Count);
      }
      m_lastWatchdogCheck = DateTime.UtcNow;
    }

    private static void CheckYourOwnHealth(DateTime lastCheckt, ILogger logger)
    {
      var timeSinceLastTick = (DateTime.UtcNow - lastCheckt);
      if (timeSinceLastTick > s_heartbeatPeriod.Multiply(2))
      {
        var gc = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
        logger.LogWarning(ErrorCode.SiloHeartbeatTimerStalled,
            ".NET Runtime Platform stalled for {0} - possibly GC? We are now using total of {1}MB memory. gc={2}, {3}, {4}",
            timeSinceLastTick,
            GC.GetTotalMemory(false) / (1024 * 1024),
            gc[0],
            gc[1],
            gc[2]);
      }
    }
  }
}
