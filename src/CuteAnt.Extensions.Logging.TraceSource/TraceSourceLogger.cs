﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using DiagnosticsTraceSource = System.Diagnostics.TraceSource;

namespace CuteAnt.Extensions.Logging.TraceSource
{
  public class TraceSourceLogger : ILogger
  {
    private readonly DiagnosticsTraceSource _traceSource;
    private readonly string _name;

    public string Name { get { return _name; } }

    public TraceSourceLogger(DiagnosticsTraceSource traceSource, string name)
    {
      _traceSource = traceSource;
      _name = name;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }
      var message = string.Empty;
      if (formatter != null)
      {
        message = formatter(state, exception);
      }
      else
      {
        if (state != null)
        {
          message += state;
        }
        if (exception != null)
        {
          message += Environment.NewLine + exception;
        }
      }
      if (!string.IsNullOrEmpty(message))
      {
        _traceSource.TraceEvent(GetEventType(logLevel), eventId.Id, message);
      }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      var traceEventType = GetEventType(logLevel);
      return _traceSource.Switch.ShouldTrace(traceEventType);
    }

    private static TraceEventType GetEventType(LogLevel logLevel)
    {
      switch (logLevel)
      {
        case LogLevel.Critical: return TraceEventType.Critical;
        case LogLevel.Error: return TraceEventType.Error;
        case LogLevel.Warning: return TraceEventType.Warning;
        case LogLevel.Information: return TraceEventType.Information;
        case LogLevel.Trace:
        default: return TraceEventType.Verbose;
      }
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      return new TraceSourceScope(state);
    }
  }
}