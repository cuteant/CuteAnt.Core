//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;

namespace CuteAnt.ServiceModel.Diagnostics
{
  // FUTURE: This class is kept so that 4.0 Extended SKU runs fine on 4.5 Client. Will remove this in the future.
  // Order is important here. The order must match the order of strings in ..\EventLog\EventLog.mc
  [Obsolete("This has been replaced by CuteAnt.Diagnostics.EventLogCategory")]
  enum EventLogCategory : ushort
  {
    ServiceAuthorization = 1,  // reserved
    MessageAuthentication,     // reserved
    ObjectAccess,              // reserved
    Tracing,
    WebHost,
    FailFast,
    MessageLogging,
    PerformanceCounter,
    Wmi,
    ComPlus,
    StateMachine,
    Wsat,
    SharingService,
    ListenerAdapter
  }

}