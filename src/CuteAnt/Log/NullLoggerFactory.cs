// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.Log
{
  public class NullLoggerFactory : ILoggerFactory
  {
    public static readonly NullLoggerFactory Instance = new NullLoggerFactory();

#if !DESKTOPCLR
    public LogLevel MinimumLevel { get; set; } = LogLevel.Verbose;
#endif

    public ILogger CreateLogger(string name)
    {
      return NullLogger.Instance;
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }
  }
}