// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if !NETFRAMEWORK
using System;
using System.Runtime.Serialization;

namespace CuteAnt.Runtime
{
  public class EndpointNotFoundException : CommunicationException
  {
    public EndpointNotFoundException() { }
    public EndpointNotFoundException(string message) : base(message) { }
    public EndpointNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    protected EndpointNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { throw new PlatformNotSupportedException(); }
  }
}
#endif