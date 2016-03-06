// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#if DESKTOPCLR
namespace CuteAnt.Extensions.Logging
#else
namespace Microsoft.Extensions.Logging
#endif
{
  public class NullLogger<T> : ILogger<T>
  {
    public static readonly NullLogger<T> Instance = new NullLogger<T>();

    public IDisposable BeginScopeImpl(object state)
    {
      return NullDisposable.Instance;
    }

#if DESKTOPCLR
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
    }
#else
    public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
    {
    }
#endif

    public bool IsEnabled(LogLevel logLevel)
    {
      return false;
    }

    private class NullDisposable : IDisposable
    {
      public static readonly NullDisposable Instance = new NullDisposable();

      public void Dispose()
      {
        // intentionally does nothing
      }
    }
  }
}
