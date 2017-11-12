// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security;
#if DESKTOPCLR
using CuteAnt.Runtime.Interop;
#endif

namespace CuteAnt.Diagnostics
{
  internal static class AssertHelper
  {
    [SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.InvariantAssertRule, Justification = "Assert implementation")]
    internal static void FireAssert(string message)
    {
      try
      {
        InternalFireAssert(ref message);
      }
      finally
      {
        Debug.Assert(false, message);
      }
    }

    [SuppressMessage(FxCop.Category.Globalization, FxCop.Rule.DoNotPassLiteralsAsLocalizedParameters, Justification = "Debug Only")]
    [Fx.Tag.SecurityNote(Critical = "Calls into various critical methods",
        Safe = "Exists only on debug versions")]
    [SecuritySafeCritical]
    private static void InternalFireAssert(ref string message)
    {
      try
      {
#if DESKTOPCLR
        string debugMessage = "Assert fired! --> " + message + "\r\n";
        if (Debugger.IsAttached)
        {
          Debugger.Log(0, Debugger.DefaultCategory, debugMessage);
          Debugger.Break();
        }
        if (UnsafeNativeMethods.IsDebuggerPresent())
        {
          UnsafeNativeMethods.OutputDebugString(debugMessage);
          UnsafeNativeMethods.DebugBreak();
        }
#endif

        if (Fx.AssertsFailFast)
        {
          try
          {
            Fx.Exception.TraceFailFast(message);
          }
          finally
          {
            Environment.FailFast(message);
          }
        }
      }
      catch (Exception exception)
      {
        if (Fx.IsFatal(exception))
        {
          throw;
        }

        string newMessage = "Exception during FireAssert!";
        try
        {
          newMessage = string.Concat(newMessage, " [", exception.GetType().Name, ": ", exception.Message, "] --> ", message);
        }
        finally
        {
          message = newMessage;
        }
        throw;
      }
    }
  }
}

