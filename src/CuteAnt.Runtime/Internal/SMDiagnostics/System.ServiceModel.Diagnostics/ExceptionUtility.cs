﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#if NETFRAMEWORK
using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using CuteAnt.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Xml;
using CuteAnt.Runtime;

namespace CuteAnt.ServiceModel.Diagnostics
{
  public class ExceptionUtility
  {
    const string ExceptionStackAsStringKey = "CuteAnt.ServiceModel.Diagnostics.ExceptionUtility.ExceptionStackAsString";

    //// This field should be only used for debug build.
    //internal static ExceptionUtility mainInstance;

    LegacyDiagnosticTrace diagnosticTrace;
    ExceptionTrace exceptionTrace;
    string name;
    string eventSourceName;

    [ThreadStatic]
    static Guid activityId;

    [ThreadStatic]
    static bool useStaticActivityId;

    [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
    internal ExceptionUtility(string name, string eventSourceName, object exceptionTrace)
    {
      this.exceptionTrace = (ExceptionTrace)exceptionTrace;
      this.name = name;
      this.eventSourceName = eventSourceName;
    }

    [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
    internal ExceptionUtility(string name, string eventSourceName, object diagnosticTrace, object exceptionTrace)
    {
      this.diagnosticTrace = diagnosticTrace as LegacyDiagnosticTrace;
      this.exceptionTrace = (ExceptionTrace)exceptionTrace;
      this.name = name;
      this.eventSourceName = eventSourceName;
    }

    [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
    [MethodImpl(MethodImplOptions.NoInlining)]
#pragma warning disable 56500
    internal void TraceFailFast(string message)
    {
      CuteAnt.Diagnostics.EventLogger logger = null;
      try
      {
#pragma warning disable 618
        logger = new CuteAnt.Diagnostics.EventLogger(this.eventSourceName, this.diagnosticTrace);
#pragma warning restore 618
      }
      finally
      {
#pragma warning disable 618
        TraceFailFast(message, logger);
#pragma warning restore 618
      }
    }

    // Fail-- Event Log entry will be generated. 
    // To force a Watson on a dev machine, do the following:
    // 1. Set \HKLM\SOFTWARE\Microsoft\PCHealth\ErrorReporting ForceQueueMode = 0 
    // 2. In the command environment, set COMPLUS_DbgJitDebugLaunchSetting=0
    [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void TraceFailFast(string message, CuteAnt.Diagnostics.EventLogger logger)
    {
      try
      {
        if (logger != null)
        {
          string stackTrace = null;
          try
          {
            stackTrace = new StackTrace().ToString();
          }
          catch (Exception exception)
          {
            stackTrace = exception.Message;
          }
          finally
          {
            logger.LogEvent(TraceEventType.Critical,
                (ushort)EventLogCategory.FailFast,
                (uint)EventLogEventId.FailFast,
                message,
                stackTrace);
          }
        }
      }
      catch (Exception e)
      {
        if (logger != null)
        {
          logger.LogEvent(TraceEventType.Critical,
              (ushort)EventLogCategory.FailFast,
              (uint)EventLogEventId.FailFastException,
              e.ToString());
        }
        throw;
      }
    }
#pragma warning restore 56500

    [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
    internal void TraceFailFastException(Exception exception)
    {
      TraceFailFast(exception == null ? null : exception.ToString());
    }

    internal Exception ThrowHelper(Exception exception, TraceEventType eventType, TraceRecord extendedData)
    {
#pragma warning disable 618
      bool shouldTrace = (this.diagnosticTrace != null && this.diagnosticTrace.ShouldTrace(eventType));
#pragma warning restore 618
      if (shouldTrace)
      {
        using (ExceptionUtility.useStaticActivityId ? Activity.CreateActivity(ExceptionUtility.activityId) : null)
        {
          //this.diagnosticTrace.TraceEvent(eventType, DiagnosticsTraceCode.ThrowingException, LegacyDiagnosticTrace.GenerateMsdnTraceCode("CuteAnt.ServiceModel.Diagnostics", "ThrowingException"), TraceSR.GetString(TraceSR.ThrowingException), extendedData, exception, null);
        }

        IDictionary data = exception.Data;
        if (data != null && !data.IsReadOnly && !data.IsFixedSize)
        {
          object existingString = data[ExceptionStackAsStringKey];
          string stackString = existingString == null ? "" : existingString as string;
          if (stackString != null)
          {
            string stack = exception.StackTrace;
            if (!string.IsNullOrEmpty(stack))
            {
              stackString = string.Concat(stackString, stackString.Length == 0 ? "" : Environment.NewLine, "throw", Environment.NewLine, stack, Environment.NewLine, "catch", Environment.NewLine);
              data[ExceptionStackAsStringKey] = stackString;
            }
          }
        }
      }

      // Trace using ETW as well.
      exceptionTrace.TraceEtwException(exception, eventType);

      return exception;
    }

    public Exception ThrowHelper(Exception exception, TraceEventType eventType)
    {
      return this.ThrowHelper(exception, eventType, null);
    }

    public ArgumentException ThrowHelperArgument(string message)
    {
      return (ArgumentException)this.ThrowHelperError(new ArgumentException(message));
    }

    public ArgumentException ThrowHelperArgument(string paramName, string message)
    {
      return (ArgumentException)this.ThrowHelperError(new ArgumentException(message, paramName));
    }

    public ArgumentNullException ThrowHelperArgumentNull(string paramName)
    {
      return (ArgumentNullException)this.ThrowHelperError(new ArgumentNullException(paramName));
    }

    public ArgumentNullException ThrowHelperArgumentNull(string paramName, string message)
    {
      return (ArgumentNullException)this.ThrowHelperError(new ArgumentNullException(paramName, message));
    }

    public ArgumentException ThrowHelperArgumentNullOrEmptyString(string arg)
    {
      return (ArgumentException)this.ThrowHelperError(new ArgumentException($"'{arg}' cannot be empty."));
    }

    public Exception ThrowHelperFatal(string message, Exception innerException)
    {
      return this.ThrowHelperError(new FatalException(message, innerException));
    }

    public Exception ThrowHelperInternal(bool fatal)
    {
      return fatal ? Fx.AssertAndThrowFatal("Fatal InternalException should never be thrown.") : Fx.AssertAndThrow("InternalException should never be thrown.");
    }

    public Exception ThrowHelperInvalidOperation(string message)
    {
      return ThrowHelperError(new InvalidOperationException(message));
    }

    internal Exception ThrowHelperCallback(string message, Exception innerException)
    {
      return this.ThrowHelperCritical(new CallbackException(message, innerException));
    }

    public Exception ThrowHelperCallback(Exception innerException)
    {
      return this.ThrowHelperCallback("GenericCallbackException", innerException); //TraceSR.GetString(TraceSR.GenericCallbackException)
    }

    public Exception ThrowHelperCritical(Exception exception)
    {
      return this.ThrowHelper(exception, TraceEventType.Critical);
    }

    public Exception ThrowHelperError(Exception exception)
    {
      return this.ThrowHelper(exception, TraceEventType.Error);
    }

    public Exception ThrowHelperWarning(Exception exception)
    {
      return this.ThrowHelper(exception, TraceEventType.Warning);
    }

    public Exception ThrowHelperXml(XmlReader reader, string message)
    {
      return this.ThrowHelperXml(reader, message, null);
    }

    public Exception ThrowHelperXml(XmlReader reader, string message, Exception inner)
    {
      IXmlLineInfo lineInfo = reader as IXmlLineInfo;
      return this.ThrowHelperError(new XmlException(
          message,
          inner,
          (null != lineInfo) ? lineInfo.LineNumber : 0,
          (null != lineInfo) ? lineInfo.LinePosition : 0));
    }

    public void DiagnosticTraceHandledException(Exception exception, TraceEventType eventType)
    {
#pragma warning disable 618
      bool shouldTrace = (this.diagnosticTrace != null && this.diagnosticTrace.ShouldTrace(eventType));
#pragma warning restore 618
      if (shouldTrace)
      {
        using (ExceptionUtility.useStaticActivityId ? Activity.CreateActivity(ExceptionUtility.activityId) : null)
        {
          //this.diagnosticTrace.TraceEvent(eventType, DiagnosticsTraceCode.TraceHandledException, LegacyDiagnosticTrace.GenerateMsdnTraceCode("CuteAnt.ServiceModel.Diagnostics", "TraceHandledException"), TraceSR.GetString(TraceSR.TraceHandledException), null, exception, null);
        }
      }
    }

    // On a single thread, these functions will complete just fine
    // and don't need to worry about locking issues because the effected
    // variables are ThreadStatic.
    public static void UseActivityId(Guid activityId)
    {
      ExceptionUtility.activityId = activityId;
      ExceptionUtility.useStaticActivityId = true;
    }

    public static void ClearActivityId()
    {
      ExceptionUtility.useStaticActivityId = false;
      ExceptionUtility.activityId = Guid.Empty;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static bool IsInfrastructureException(Exception exception)
    {
      return exception != null && (exception is ThreadAbortException || exception is AppDomainUnloadedException);
    }
  }
}
#endif
