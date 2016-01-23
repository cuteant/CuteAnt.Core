﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;
using Serilog.Parsing;
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
using FrameworkLogger = CuteAnt.Extensions.Logging.ILogger;
#else
using Microsoft.Extensions.Logging;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;
#endif

namespace CuteAnt.Extensions.Logging.Serilog
{
  public class SerilogLogger : FrameworkLogger
  {
    readonly SerilogLoggerProvider _provider;
    readonly string _name;
    readonly global::Serilog.ILogger _logger;

    static readonly MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();

    public SerilogLogger(
        SerilogLoggerProvider provider,
        global::Serilog.ILogger logger = null,
        string name = null)
    {
      if (provider == null) throw new ArgumentNullException(nameof(provider));
      _provider = provider;
      _name = name;
      _logger = logger;

      // If a logger was passed, the provider has already added itself as an enricher
      _logger = _logger ?? global::Serilog.Log.Logger.ForContext(new[] { provider });

      if (_name != null)
      {
        _logger = _logger.ForContext(Constants.SourceContextPropertyName, name);
      }
    }

    public IDisposable BeginScopeImpl(object state)
    {
      return _provider.BeginScopeImpl(_name, state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return _logger.IsEnabled(ConvertLevel(logLevel));
    }

    public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
    {
      var level = ConvertLevel(logLevel);
      if (!_logger.IsEnabled(level))
      {
        return;
      }

      var logger = _logger;
      string messageTemplate = null;

      var structure = state as ILogValues;
      if (structure != null)
      {
        foreach (var property in structure.GetValues())
        {
          if (property.Key == SerilogLoggerProvider.OriginalFormatPropertyName && property.Value is string)
          {
            messageTemplate = (string)property.Value;
          }
          else if (property.Key.StartsWith("@"))
          {
            logger = logger.ForContext(property.Key.Substring(1), property.Value, destructureObjects: true);
          }
          else
          {
            logger = logger.ForContext(property.Key, property.Value);
          }
        }

        var stateType = state.GetType();
#if NET40
        // Imperfect, but at least eliminates `1 and + names
        if (messageTemplate == null && !stateType.IsGenericType && !stateType.IsNested)
        {
          messageTemplate = "{" + stateType.Name + ":l}";
          logger = logger.ForContext(stateType.Name, LogFormatter.Formatter(state, null));
        }
#else
        var stateTypeInfo = stateType.GetTypeInfo();
        // Imperfect, but at least eliminates `1 and + names
        if (messageTemplate == null && !stateTypeInfo.IsGenericType && !stateTypeInfo.IsNested)
        {
          messageTemplate = "{" + stateType.Name + ":l}";
          logger = logger.ForContext(stateType.Name, LogFormatter.Formatter(state, null));
        }
#endif
      }

      if (messageTemplate == null && state != null)
      {
        messageTemplate = LogFormatter.Formatter(state, null);
      }

      if (string.IsNullOrEmpty(messageTemplate))
      {
        return;
      }

      if (eventId != 0)
      {
        logger = logger.ForContext("EventId", eventId);
      }

      var parsedTemplate = _messageTemplateParser.Parse(messageTemplate);
      var evt = new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, Enumerable.Empty<LogEventProperty>());
      logger.Write(evt);
    }

    private LogEventLevel ConvertLevel(LogLevel logLevel)
    {
      switch (logLevel)
      {
        case LogLevel.Critical:
          return LogEventLevel.Fatal;
        case LogLevel.Error:
          return LogEventLevel.Error;
        case LogLevel.Warning:
          return LogEventLevel.Warning;
        case LogLevel.Information:
          return LogEventLevel.Information;
        case LogLevel.Verbose:
          return LogEventLevel.Debug;
        default:
          return LogEventLevel.Verbose;
      }
    }
  }
}