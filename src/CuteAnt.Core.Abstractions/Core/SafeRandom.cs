using System;
using System.Runtime.CompilerServices;

namespace CuteAnt
{
    /// <summary>Thread-safe random number generator.
    /// Has same API as System.Random but is thread safe, similar to the implementation by Steven Toub: http://blogs.msdn.com/b/pfxteam/archive/2014/10/20/9434171.aspx
    /// </summary>
    public sealed class ThreadSafeRandom
    {
        [ThreadStatic] private static Random s_threadRandom;

        private static Random Instance => s_threadRandom ?? CreateInstance();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Random CreateInstance() => s_threadRandom = new Random();

        public int Next()
        {
            return Instance.Next();
        }

        public int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }

        public void NextBytes(byte[] buffer)
        {
            Instance.NextBytes(buffer);
        }

        public double NextDouble()
        {
            return Instance.NextDouble();
        }

        #region --& Extensions &--

        public TimeSpan NextTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan, "TimeSpan must be positive.");

#if NETSTANDARD2_0
            double ticksD = ((double)timeSpan.Ticks) * NextDouble();
            long ticks = checked((long)ticksD);
            return TimeSpan.FromTicks(ticks);
#else
            return timeSpan.Multiply(NextDouble());
#endif
        }

        public TimeSpan NextTimeSpan(TimeSpan minValue, TimeSpan maxValue)
        {
            if (minValue <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minValue), minValue, "MinValue must be positive.");
            if (minValue >= maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), minValue, "MinValue must be less than maxValue.");

            return minValue + NextTimeSpan(maxValue - minValue);
        }

#endregion
    }
}
