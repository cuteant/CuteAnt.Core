//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace CuteAnt
{
  [Serializable]
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

    protected QuotaExceededException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}

