//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

#if DESKTOPCLR
namespace CuteAnt.Diagnostics
{
    using System;

    interface ITraceSourceStringProvider
    {
        string GetSourceString();
    }
}
#endif