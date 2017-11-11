using System;
using System.Security.Cryptography;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Runtime
{
  /// <summary>Thread-safe random number generator.
  /// Has same API as System.Random but is thread safe, similar to the implementation by 
  /// Steven Toub: http://blogs.msdn.com/b/pfxteam/archive/2014/10/20/9434171.aspx </summary>
  public class SafeRandom
  {
    private static readonly RandomNumberGenerator globalCryptoProvider = RandomNumberGenerator.Create();

    [ThreadStatic]
    private static Random random;

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static Random GetRandom()
    {
      if (random == null)
      {
        byte[] buffer = new byte[4];
        globalCryptoProvider.GetBytes(buffer);
        random = new Random(BitConverter.ToInt32(buffer, 0));
      }

      return random;
    }

    public int Next()
    {
      return GetRandom().Next();
    }

    public int Next(int maxValue)
    {
      return GetRandom().Next(maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
      return GetRandom().Next(minValue, maxValue);
    }

    public void NextBytes(byte[] buffer)
    {
      GetRandom().NextBytes(buffer);
    }

    public double NextDouble()
    {
      return GetRandom().NextDouble();
    }

    #region --& Extensions &--

    public TimeSpan NextTimeSpan(TimeSpan timeSpan)
    {
      if (timeSpan <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("timeSpan", timeSpan, "SafeRandom.NextTimeSpan timeSpan must be a positive number.");
      double ticksD = ((double)timeSpan.Ticks) * NextDouble();
      long ticks = checked((long)ticksD);
      return TimeSpan.FromTicks(ticks);
    }

    public TimeSpan NextTimeSpan(TimeSpan minValue, TimeSpan maxValue)
    {
      if (minValue <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("minValue", minValue, "SafeRandom.NextTimeSpan minValue must be a positive number.");
      if (minValue >= maxValue) throw new ArgumentOutOfRangeException("minValue", minValue, "SafeRandom.NextTimeSpan minValue must be greater than maxValue.");
      var span = maxValue - minValue;
      return minValue + NextTimeSpan(span);
    }

    #endregion
  }
}
