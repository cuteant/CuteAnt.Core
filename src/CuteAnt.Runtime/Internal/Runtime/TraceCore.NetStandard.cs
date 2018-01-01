﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#if NETSTANDARD
using System;
using System.Reflection;
using System.Security;
using System.Runtime.CompilerServices;

namespace CuteAnt.Diagnostics
{
  internal partial class TraceCore
  {

    //static System.Resources.ResourceManager resourceManager;

    //static System.Globalization.CultureInfo resourceCulture;


    static object syncLock = new object();

    // Double-checked locking pattern requires volatile for read/write synchronization

    private TraceCore()
    {
    }

    #region ## 苦竹 修改 ##
    //[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is an auto-generated code, some ETW/TraceSource mixed code would use it.")]
    //static System.Resources.ResourceManager ResourceManager
    //{
    //  get
    //  {
    //    if (object.ReferenceEquals(resourceManager, null))
    //    {
    //      resourceManager = new System.Resources.ResourceManager("System.Runtime.TraceCore", typeof(TraceCore).GetTypeInfo().Assembly);
    //    }
    //    return resourceManager;
    //  }
    //}

    //[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This template is shared across all assemblies, some of which use this accessor.")]
    //internal static System.Globalization.CultureInfo Culture
    //{
    //  get
    //  {
    //    return resourceCulture;
    //  }
    //  set
    //  {
    //    resourceCulture = value;
    //  }
    //}
    internal class ResourceManager
    {
      internal const string ActionItemCallbackInvoked = "IO Thread scheduler callback invoked.";
      internal const string ActionItemScheduled = "IO Thread scheduler callback invoked.";
      internal const string AppDomainUnload = "AppDomain unloading. AppDomain.FriendlyName {0}, ProcessName {1}, ProcessId {2}.";
      internal const string BufferPoolAllocation = "Pool allocating {0} Bytes.";
      internal const string BufferPoolChangeQuota = "BufferPool of size {0}, changing quota by {1}.";
      internal const string EtwUnhandledException = "Unhandled exception. Exception details: {0}";
      internal const string HandledException = "Handling an exception.  Exception details: {0}";
      internal const string HandledExceptionError = "Handling an exception. Exception details: {0}";
      internal const string HandledExceptionVerbose = "Handling an exception  Exception details: {0}";
      internal const string HandledExceptionWarning = "Handling an exception. Exception details: {0}";
      internal const string ShipAssertExceptionMessage = "An unexpected failure occurred. Applications should not attempt to handle this error. For diagnostic purposes, this English message is associated with the failure: {0}.";
      internal const string ThrowingEtwException = "Throwing an exception. Source: {0}. Exception details: {1}";
      internal const string ThrowingEtwExceptionVerbose = "Throwing an exception. Source: {0}. Exception details: {1}";
      internal const string ThrowingException = "Throwing an exception. Source: {0}. Exception details: {1}";
      internal const string ThrowingExceptionVerbose = "Throwing an exception. Source: {0}. Exception details: {1}";
      internal const string TraceCodeEventLogCritical = "Wrote to the EventLog.";
      internal const string TraceCodeEventLogError = "Wrote to the EventLog.";
      internal const string TraceCodeEventLogInfo = "Wrote to the EventLog.";
      internal const string TraceCodeEventLogVerbose = "Wrote to the EventLog.";
      internal const string TraceCodeEventLogWarning = "Wrote to the EventLog.";
      internal const string UnhandledException = "Unhandled exception.  Exception details: {0}";

      internal const string TraceCodeEventLog = "Wrote to the EventLog.";
    }
    #endregion

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57394, Level=informational, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool HandledExceptionIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.HandledExceptionIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Handling an exception.  Exception details: {0}
    /// Event description ID=57394, Level=informational, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Handling an exception.  Exception details: {0}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void HandledException(EtwDiagnosticTrace trace, string param0, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.HandledException(param0, serializedException);
    }


    /// <summary>
    /// Gets trace definition like: An unexpected failure occurred. Applications should not attempt to handle this error. For diagnostic purposes, this English message is associated with the failure: {0}.
    /// Event description ID=57395, Level=error, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: An unexpected failure occurred. Applications should not attempt to handle this error. For diagnostic purposes, this English message is associated with the failure: {0}.</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ShipAssertExceptionMessage(EtwDiagnosticTrace trace, string param0)
    {
      WcfEventSource.Instance.ShipAssertExceptionMessage(param0);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57396, Level=warning, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool ThrowingExceptionIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.ThrowingExceptionIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Throwing an exception. Source: {0}. Exception details: {1}
    /// Event description ID=57396, Level=warning, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="param1">Parameter 1 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ThrowingException(EtwDiagnosticTrace trace, string param0, string param1, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.ThrowingException(param0, param1, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57397, Level=critical, Channel=Operational
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool UnhandledExceptionIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.UnhandledExceptionIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Unhandled exception.  Exception details: {0}
    /// Event description ID=57397, Level=critical, Channel=Operational
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Unhandled exception.  Exception details: {0}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void UnhandledException(EtwDiagnosticTrace trace, string param0, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.UnhandledException(param0, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57399, Level=critical, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool TraceCodeEventLogCriticalIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.TraceCodeEventLogCriticalIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Wrote to the EventLog.
    /// Event description ID=57399, Level=critical, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="traceRecord">Extended data (TraceRecord) for the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void TraceCodeEventLogCritical(EtwDiagnosticTrace trace, TraceRecord traceRecord)
    {
      WcfEventSource.Instance.TraceCodeEventLogCritical(traceRecord.EventId);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57400, Level=error, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool TraceCodeEventLogErrorIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.TraceCodeEventLogErrorIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Wrote to the EventLog.
    /// Event description ID=57400, Level=error, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="traceRecord">Extended data (TraceRecord) for the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void TraceCodeEventLogError(EtwDiagnosticTrace trace, TraceRecord traceRecord)
    {
      WcfEventSource.Instance.TraceCodeEventLogError(traceRecord.EventId);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57401, Level=informational, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool TraceCodeEventLogInfoIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.TraceCodeEventLogInfoIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Wrote to the EventLog.
    /// Event description ID=57401, Level=informational, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="traceRecord">Extended data (TraceRecord) for the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void TraceCodeEventLogInfo(EtwDiagnosticTrace trace, TraceRecord traceRecord)
    {
      WcfEventSource.Instance.TraceCodeEventLogInfo(traceRecord.EventId);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57402, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool TraceCodeEventLogVerboseIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.TraceCodeEventLogVerboseIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Wrote to the EventLog.
    /// Event description ID=57402, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="traceRecord">Extended data (TraceRecord) for the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void TraceCodeEventLogVerbose(EtwDiagnosticTrace trace, TraceRecord traceRecord)
    {
      WcfEventSource.Instance.TraceCodeEventLogVerbose(traceRecord.EventId);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57403, Level=warning, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool TraceCodeEventLogWarningIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.TraceCodeEventLogWarningIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Wrote to the EventLog.
    /// Event description ID=57403, Level=warning, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="traceRecord">Extended data (TraceRecord) for the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void TraceCodeEventLogWarning(EtwDiagnosticTrace trace, TraceRecord traceRecord)
    {
      WcfEventSource.Instance.TraceCodeEventLogWarning(traceRecord.EventId);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57404, Level=warning, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool HandledExceptionWarningIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.HandledExceptionWarningIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Handling an exception. Exception details: {0}
    /// Event description ID=57404, Level=warning, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Handling an exception. Exception details: {0}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void HandledExceptionWarning(EtwDiagnosticTrace trace, string param0, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.HandledExceptionWarning(param0, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=131, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool BufferPoolAllocationIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.BufferPoolAllocationIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Pool allocating {0} Bytes.
    /// Event description ID=131, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="Size">Parameter 0 for event: Pool allocating {0} Bytes.</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void BufferPoolAllocation(EtwDiagnosticTrace trace, int Size)
    {
      WcfEventSource.Instance.BufferPoolAllocation(Size);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=132, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool BufferPoolChangeQuotaIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.BufferPoolChangeQuotaIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: BufferPool of size {0}, changing quota by {1}.
    /// Event description ID=132, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="PoolSize">Parameter 0 for event: BufferPool of size {0}, changing quota by {1}.</param>
    /// <param name="Delta">Parameter 1 for event: BufferPool of size {0}, changing quota by {1}.</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void BufferPoolChangeQuota(EtwDiagnosticTrace trace, int PoolSize, int Delta)
    {
      WcfEventSource.Instance.BufferPoolChangeQuota(PoolSize, Delta);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=133, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool ActionItemScheduledIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.ActionItemScheduledIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: IO Thread scheduler callback invoked.
    /// Event description ID=133, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="eventTraceActivity">The event trace activity</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ActionItemScheduled(EtwDiagnosticTrace trace, CuteAnt.Diagnostics.EventTraceActivity eventTraceActivity)
    {
      WcfEventSource.Instance.ActionItemScheduled();
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=134, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool ActionItemCallbackInvokedIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.ActionItemCallbackInvokedIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: IO Thread scheduler callback invoked.
    /// Event description ID=134, Level=verbose, Channel=Debug
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="eventTraceActivity">The event trace activity</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ActionItemCallbackInvoked(EtwDiagnosticTrace trace, CuteAnt.Diagnostics.EventTraceActivity eventTraceActivity)
    {
      WcfEventSource.Instance.ActionItemCallbackInvoked();
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57405, Level=error, Channel=Operational
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool HandledExceptionErrorIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.HandledExceptionErrorIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Handling an exception. Exception details: {0}
    /// Event description ID=57405, Level=error, Channel=Operational
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Handling an exception. Exception details: {0}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void HandledExceptionError(EtwDiagnosticTrace trace, string param0, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.HandledExceptionError(param0, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57406, Level=verbose, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool HandledExceptionVerboseIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.HandledExceptionVerboseIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Handling an exception  Exception details: {0}
    /// Event description ID=57406, Level=verbose, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Handling an exception  Exception details: {0}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void HandledExceptionVerbose(EtwDiagnosticTrace trace, string param0, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.HandledExceptionVerbose(param0, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57408, Level=critical, Channel=Operational
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool EtwUnhandledExceptionIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.EtwUnhandledExceptionIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Unhandled exception. Exception details: {0}
    /// Event description ID=57408, Level=critical, Channel=Operational
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Unhandled exception. Exception details: {0}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void EtwUnhandledException(EtwDiagnosticTrace trace, string param0, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.EtwUnhandledException(param0, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57410, Level=warning, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool ThrowingEtwExceptionIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.ThrowingEtwExceptionIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Throwing an exception. Source: {0}. Exception details: {1}
    /// Event description ID=57410, Level=warning, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="param1">Parameter 1 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ThrowingEtwException(EtwDiagnosticTrace trace, string param0, string param1, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.ThrowingEtwException(param0, param1, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57409, Level=verbose, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool ThrowingEtwExceptionVerboseIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.ThrowingEtwExceptionVerboseIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Throwing an exception. Source: {0}. Exception details: {1}
    /// Event description ID=57409, Level=verbose, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="param1">Parameter 1 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ThrowingEtwExceptionVerbose(EtwDiagnosticTrace trace, string param0, string param1, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.ThrowingEtwExceptionVerbose(param0, param1, serializedException);
    }

    /// <summary>
    /// Check if trace definition is enabled
    /// Event description ID=57407, Level=verbose, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    [MethodImpl(InlineMethod.Value)]
    internal static bool ThrowingExceptionVerboseIsEnabled(EtwDiagnosticTrace trace)
    {
      return WcfEventSource.Instance.ThrowingExceptionVerboseIsEnabled();
    }

    /// <summary>
    /// Gets trace definition like: Throwing an exception. Source: {0}. Exception details: {1}
    /// Event description ID=57407, Level=verbose, Channel=Analytic
    /// </summary>
    /// <param name="trace">The trace provider</param>
    /// <param name="param0">Parameter 0 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="param1">Parameter 1 for event: Throwing an exception. Source: {0}. Exception details: {1}</param>
    /// <param name="exception">Exception associated with the event</param>
    [MethodImpl(InlineMethod.Value)]
    internal static void ThrowingExceptionVerbose(EtwDiagnosticTrace trace, string param0, string param1, System.Exception exception)
    {
      string serializedException = EtwDiagnosticTrace.ExceptionToTraceString(exception, int.MaxValue);
      WcfEventSource.Instance.ThrowingExceptionVerbose(param0, param1, serializedException);
    }
  }
}
#endif