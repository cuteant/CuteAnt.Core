﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if NETSTANDARD
using System;
using System.Runtime.Serialization;

namespace CuteAnt.Runtime
{
  public class CommunicationException : Exception
  {
    public CommunicationException() { }
    public CommunicationException(string message) : base(message) { }
    public CommunicationException(string message, Exception innerException) : base(message, innerException) { }
    protected CommunicationException(SerializationInfo info, StreamingContext context) : base(info, context) { throw new PlatformNotSupportedException(); }
  }
}
#endif