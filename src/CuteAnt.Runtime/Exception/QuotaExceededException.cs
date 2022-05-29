//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace CuteAnt
{
#if NETFRAMEWORK
  [Serializable]
#endif
  public class QuotaExceededException : SystemException
  {
    public QuotaExceededException()
      : base()
    {
    }

    public QuotaExceededException(string message)
      : base(message)
    {
    }

    public QuotaExceededException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

#if NETFRAMEWORK
    protected QuotaExceededException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
#endif
  }
}

