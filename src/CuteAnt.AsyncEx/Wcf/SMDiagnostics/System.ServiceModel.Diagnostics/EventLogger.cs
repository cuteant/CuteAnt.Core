//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace CuteAnt.ServiceModel.Diagnostics
{
  // 
  [Obsolete("This has been replaced by CuteAnt.Diagnostics.EventLogger")]
  class EventLogger
  {
    CuteAnt.Diagnostics.EventLogger innerEventLogger;

    EventLogger()
    {
    }

    [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.EventLog instead")]
    internal EventLogger(string eventLogSourceName, object diagnosticTrace)
    {
      this.innerEventLogger = new CuteAnt.Diagnostics.EventLogger(eventLogSourceName, (CuteAnt.Diagnostics.DiagnosticTraceBase)diagnosticTrace);
    }

    [CuteAnt.Fx.Tag.SecurityNote(Critical = "Calling SecurityCritical method/property")]
    [SecurityCritical]
    internal static EventLogger UnsafeCreateEventLogger(string eventLogSourceName, object diagnosticTrace)
    {
      EventLogger logger = new EventLogger();
      logger.innerEventLogger = CuteAnt.Diagnostics.EventLogger.UnsafeCreateEventLogger(eventLogSourceName, (CuteAnt.Diagnostics.DiagnosticTraceBase)diagnosticTrace);
      return logger;
    }

    internal void LogEvent(TraceEventType type, EventLogCategory category, EventLogEventId eventId, bool shouldTrace, params string[] values)
    {
      this.innerEventLogger.LogEvent(type, (ushort)category, (uint)eventId, shouldTrace, values);
    }

    [CuteAnt.Fx.Tag.SecurityNote(Critical = "Calling SecurityCritical method/property")]
    [SecurityCritical]
    internal void UnsafeLogEvent(TraceEventType type, EventLogCategory category, EventLogEventId eventId, bool shouldTrace, params string[] values)
    {
      this.innerEventLogger.UnsafeLogEvent(type, (ushort)category, (uint)eventId,
          shouldTrace, values);
    }

    internal void LogEvent(TraceEventType type, EventLogCategory category, EventLogEventId eventId, params string[] values)
    {
      this.innerEventLogger.LogEvent(type, (ushort)category, (uint)eventId, values);
    }

    internal static string NormalizeEventLogParameter(string param)
    {
      return CuteAnt.Diagnostics.EventLogger.NormalizeEventLogParameter(param);
    }
  }
}
