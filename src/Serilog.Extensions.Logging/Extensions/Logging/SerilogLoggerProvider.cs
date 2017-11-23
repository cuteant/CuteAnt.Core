﻿// Copyright (c) .NET Foundation. All rights reserved. Licensed under the Apache License, Version
// 2.0. See License.txt in the project root for license information.

using System;
#if ASYNCLOCAL
using System.Threading;
#else
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#endif
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using System.Collections.Generic;
using Serilog.Context;

namespace Serilog.Extensions.Logging
{
  /// <summary>An <see cref="ILoggerProvider"/> that pipes events through Serilog.</summary>
  [ProviderAlias("Serilog")]
  public class SerilogLoggerProvider : ILoggerProvider, ILogEventEnricher
  {
    internal const string OriginalFormatPropertyName = "{OriginalFormat}";
    internal const string ScopePropertyName = "Scope";

    // May be null; if it is, Log.Logger will be lazily used
    private readonly ILogger _logger;

    private readonly Action _dispose;

    /// <summary>Construct a <see cref="SerilogLoggerProvider"/>.</summary>
    /// <param name="logger">
    /// A Serilog logger to pipe events through; if null, the static <see cref="Log"/> class will be used.
    /// </param>
    /// <param name="dispose">
    /// If true, the provided logger or static log class will be disposed/closed when the provider is disposed.
    /// </param>
    public SerilogLoggerProvider(ILogger logger = null, bool dispose = false)
    {
      if (logger != null)
        _logger = logger.ForContext(new[] { this });

      if (dispose)
      {
        if (logger != null)
          _dispose = () => (logger as IDisposable)?.Dispose();
        else
          _dispose = Log.CloseAndFlush;
      }
    }

    /// <inheritdoc/>
    public FrameworkLogger CreateLogger(string name)
    {
      return new SerilogLogger(this, _logger, name);
    }

    /// <inheritdoc/>
    public IDisposable BeginScope<T>(T state)
    {
      if (CurrentScope != null)
        return new SerilogLoggerScope(this, state);

      // The outermost scope pushes and pops the Serilog `LogContext` - once this enricher is on the
      // stack, the `CurrentScope` property takes care of the rest of the `BeginScope()` stack.
      var popSerilogContext = LogContext.PushProperties(this);
      return new SerilogLoggerScope(this, state, popSerilogContext);
    }

    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      List<LogEventPropertyValue> scopeItems = null;
      for (var scope = CurrentScope; scope != null; scope = scope.Parent)
      {
        LogEventPropertyValue scopeItem;
        scope.EnrichAndCreateScopeItem(logEvent, propertyFactory, out scopeItem);

        if (scopeItem != null)
        {
          scopeItems = scopeItems ?? new List<LogEventPropertyValue>();
          scopeItems.Add(scopeItem);
        }
      }

      if (scopeItems != null)
      {
        scopeItems.Reverse();
        logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
      }
    }

#if ASYNCLOCAL
    private readonly AsyncLocal<SerilogLoggerScope> _value = new AsyncLocal<SerilogLoggerScope>();

    internal SerilogLoggerScope CurrentScope
    {
      get
      {
        return _value.Value;
      }
      set
      {
        _value.Value = value;
      }
    }

#else
    readonly string _currentScopeKey = nameof(SerilogLoggerScope) + "#" + Guid.NewGuid().ToString("n");

    internal SerilogLoggerScope CurrentScope
    {
      get
      {
        var objectHandle = CallContext.LogicalGetData(_currentScopeKey) as ObjectHandle;
        return objectHandle?.Unwrap() as SerilogLoggerScope;
      }
      set
      {
        CallContext.LogicalSetData(_currentScopeKey, new ObjectHandle(value));
      }
    }
#endif

    /// <inheritdoc/>
    public void Dispose()
    {
      _dispose?.Invoke();
    }
  }
}