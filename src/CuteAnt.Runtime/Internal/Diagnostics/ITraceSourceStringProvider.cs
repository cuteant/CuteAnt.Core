//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

#if NETFRAMEWORK
namespace CuteAnt.Diagnostics
{
    using System;

    interface ITraceSourceStringProvider
    {
        string GetSourceString();
    }
}
#endif