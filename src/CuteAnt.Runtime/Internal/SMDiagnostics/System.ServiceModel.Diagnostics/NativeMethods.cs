﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#if NETFRAMEWORK
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using CuteAnt.Runtime;

namespace CuteAnt.ServiceModel.Diagnostics
{
  static class NativeMethods
  {
    const string ADVAPI32 = "advapi32.dll";

    [DllImport(ADVAPI32, CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    [ResourceExposure(ResourceScope.Machine)]
    [Fx.Tag.SecurityNote(Critical = "Returns security critical type SafeEventLogWriteHandle.")]
    [SecurityCritical]
    internal static extern SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName);
  }
}
#endif