// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
//using System.ServiceModel.Channels;

namespace CuteAnt.Diagnostics
{
  internal class ExceptionHelper
  {
    internal static Exception AsError(Exception exception)
    {
      return exception;
    }

    internal static PlatformNotSupportedException PlatformNotSupported()
    {
      return new PlatformNotSupportedException();
    }

    internal static PlatformNotSupportedException PlatformNotSupported(string message)
    {
      return new PlatformNotSupportedException(message);
    }

    //public static Exception CreateMaxReceivedMessageSizeExceededException(long maxMessageSize)
    //{
    //  return MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(maxMessageSize);
    //}
  }
}
