//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

#if DESKTOPCLR
namespace CuteAnt.Diagnostics
{
    using System;

    enum TraceEventOpcode
    {
        Info = 0,
        Start = 1,
        Stop = 2,
        Reply = 6,
        Resume = 7,
        Suspend = 8,
        Send = 9,
        Receive = 240
    }
}
#endif