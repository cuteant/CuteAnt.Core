//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;

namespace CuteAnt.Pool
{
  public class ObjectCacheSettings
  {
    private int m_cacheLimit;
    private TimeSpan m_idleTimeout;
    private TimeSpan m_leaseTimeout;
    private int m_purgeFrequency;

    private const int c_defaultCacheLimit = 64;
    private const int c_defaultPurgeFrequency = 32;
    private static TimeSpan s_defaultIdleTimeout = TimeSpan.FromMinutes(2);
    private static TimeSpan s_defaultLeaseTimeout = TimeSpan.FromMinutes(5);

    public ObjectCacheSettings()
    {
      CacheLimit = c_defaultCacheLimit;
      IdleTimeout = s_defaultIdleTimeout;
      LeaseTimeout = s_defaultLeaseTimeout;
      PurgeFrequency = c_defaultPurgeFrequency;
    }

    private ObjectCacheSettings(ObjectCacheSettings other)
    {
      CacheLimit = other.CacheLimit;
      IdleTimeout = other.IdleTimeout;
      LeaseTimeout = other.LeaseTimeout;
      PurgeFrequency = other.PurgeFrequency;
    }

    internal ObjectCacheSettings Clone()
    {
      return new ObjectCacheSettings(this);
    }

    public int CacheLimit
    {
      get { return m_cacheLimit; }
      set
      {
        Fx.Assert(value >= 0, "caller should validate cache limit is non-negative");
        m_cacheLimit = value;
      }
    }

    public TimeSpan IdleTimeout
    {
      get { return m_idleTimeout; }
      set
      {
        Fx.Assert(value >= TimeSpan.Zero, "caller should validate cache limit is non-negative");
        m_idleTimeout = value;
      }
    }

    public TimeSpan LeaseTimeout
    {
      get { return m_leaseTimeout; }
      set
      {
        Fx.Assert(value >= TimeSpan.Zero, "caller should validate cache limit is non-negative");
        m_leaseTimeout = value;
      }
    }

    public int PurgeFrequency
    {
      get { return m_purgeFrequency; }
      set
      {
        Fx.Assert(value >= 0, "caller should validate purge frequency is non-negative");
        m_purgeFrequency = value;
      }
    }
  }
}