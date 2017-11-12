//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

#if DESKTOPCLR
using System;

namespace CuteAnt.Diagnostics
{
  public enum TraceEventLevel
  {
    LogAlways = 0,
    Critical = 1,
    Error = 2,
    Warning = 3,
    Informational = 4,
    Verbose = 5,
  }
}
#endif