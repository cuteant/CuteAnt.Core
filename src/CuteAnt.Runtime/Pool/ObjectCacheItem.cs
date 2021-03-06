﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\Collections\ObjectCacheItem.cs
//------------------------------------------------------------

namespace CuteAnt.Pool
{
  public abstract class ObjectCacheItem<T>
    where T : class
  {
    // only valid when you've called TryAddReference successfully
    public abstract T Value { get; }

    public abstract bool TryAddReference();

    public abstract void ReleaseReference();
  }
}