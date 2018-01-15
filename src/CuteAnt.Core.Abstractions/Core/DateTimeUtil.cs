//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.IdentityModel\System\IdentityModel\DateTimeUtil.cs
//------------------------------------------------------------
using System;
using System.Runtime.CompilerServices;

namespace CuteAnt
{
  public static class DateTimeUtil
  {
    /// <summary>Add a DateTime and a TimeSpan.
    /// The maximum time is DateTime.MaxTime.  It is not an error if time + timespan > MaxTime.
    /// Just return MaxTime.</summary>
    /// <param name="time">Initial <see cref="DateTime"/> value.</param>
    /// <param name="timespan"><see cref="TimeSpan"/> to add.</param>
    /// <returns></returns>
    [MethodImpl(InlineMethod.Value)]
    public static DateTime Add(in DateTime time, in TimeSpan timespan)
    {
      if (timespan >= TimeSpan.Zero && DateTime.MaxValue - time <= timespan)
      {
        return GetMaxValue(time.Kind);
      }

      if (timespan <= TimeSpan.Zero && DateTime.MinValue - time >= timespan)
      {
        return GetMinValue(time.Kind);
      }

      return time + timespan;
    }

    /// <summary>Add a DateTime and a non-negative TimeSpan.
    /// The maximum time is DateTime.MaxTime.  It is not an error if time + timespan > MaxTime.
    /// Just return MaxTime. If TimeSpan is &lt; TimeSpan.Zero, throw exception.</summary>
    /// <param name="time"></param>
    /// <param name="timespan"></param>
    /// <returns></returns>
    [MethodImpl(InlineMethod.Value)]
    public static DateTime AddNonNegative(in DateTime time, in TimeSpan timespan)
    {
      if (timespan <= TimeSpan.Zero)
      {
        throw new InvalidOperationException("The value of this argument must be non-negative.");
      }
      return Add(time, timespan);
    }

    [MethodImpl(InlineMethod.Value)]
    public static DateTime GetMaxValue(DateTimeKind kind)
    {
      return new DateTime(DateTime.MaxValue.Ticks, kind);
    }

    [MethodImpl(InlineMethod.Value)]
    public static DateTime GetMinValue(DateTimeKind kind)
    {
      return new DateTime(DateTime.MinValue.Ticks, kind);
    }

    [MethodImpl(InlineMethod.Value)]
    public static DateTime? ToUniversalTime(in DateTime? value)
    {
      if (null == value || value.Value.Kind == DateTimeKind.Utc)
      {
        return value;
      }
      return ToUniversalTime(value.Value);
    }

    [MethodImpl(InlineMethod.Value)]
    public static DateTime ToUniversalTime(in DateTime value)
    {
      if (value.Kind == DateTimeKind.Utc)
      {
        return value;
      }

      return value.ToUniversalTime();
    }
  }
}
