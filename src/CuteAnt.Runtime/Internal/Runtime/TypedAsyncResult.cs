﻿//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

#if NETFRAMEWORK
using System;

namespace CuteAnt.Runtime
{
  public abstract class TypedAsyncResult<T> : AsyncResult
  {
    T m_data;

    public TypedAsyncResult(AsyncCallback callback, object state)
        : base(callback, state)
    {
    }

    public T Data
    {
      get { return m_data; }
    }

    protected void Complete(T data, bool completedSynchronously)
    {
      m_data = data;
      Complete(completedSynchronously);
    }

    public static T End(IAsyncResult result)
    {
      TypedAsyncResult<T> completedResult = AsyncResult.End<TypedAsyncResult<T>>(result);
      return completedResult.Data;
    }
  }
}
#endif