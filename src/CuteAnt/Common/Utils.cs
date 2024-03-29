﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CuteAnt.Text;
using Microsoft.Extensions.Logging;

namespace CuteAnt
{
    /// <summary>The Utils class contains a variety of utility methods for use in application and grain code.</summary>
    public static class Utils
    {
        /// <summary>Returns a human-readable text string that describes an IEnumerable collection of objects.</summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="collection">The IEnumerable to describe.</param>
        /// <param name="toString">Converts the element to a string. If none specified, <see cref="object.ToString"/> will be used.</param>
        /// <param name="separator">The separator to use.</param>
        /// <param name="putInBrackets">Puts elements within brackets</param>
        /// <returns>A string assembled by wrapping the string descriptions of the individual
        /// elements with square brackets and separating them with commas.</returns>
        public static string EnumerableToString<T>(IEnumerable<T> collection, Func<T, string> toString = null,
                                                        string separator = ", ", bool putInBrackets = true)
        {
            if (collection == null)
            {
                if (putInBrackets) return "[]";
                else return "null";
            }
            var sb = StringBuilderCache.Acquire();
            if (putInBrackets) sb.Append("[");
            var enumerator = collection.GetEnumerator();
            bool firstDone = false;
            while (enumerator.MoveNext())
            {
                T value = enumerator.Current;
                string val;
                if (toString != null)
                    val = toString(value);
                else
                    val = value == null ? "null" : value.ToString();

                if (firstDone)
                {
                    sb.Append(separator);
                    sb.Append(val);
                }
                else
                {
                    sb.Append(val);
                    firstDone = true;
                }
            }
            if (putInBrackets) sb.Append("]");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        /// <summary>Returns a human-readable text string that describes a dictionary that maps objects to objects.</summary>
        /// <typeparam name="T1">The type of the dictionary keys.</typeparam>
        /// <typeparam name="T2">The type of the dictionary elements.</typeparam>
        /// <param name="dict">The dictionary to describe.</param>
        /// <param name="toString">Converts the element to a string. If none specified, <see cref="object.ToString"/> will be used.</param>
        /// <param name="separator">The separator to use. If none specified, the elements should appear separated by a new line.</param>
        /// <returns>A string assembled by wrapping the string descriptions of the individual
        /// pairs with square brackets and separating them with commas.
        /// Each key-value pair is represented as the string description of the key followed by
        /// the string description of the value,
        /// separated by " -> ", and enclosed in curly brackets.</returns>
        public static string DictionaryToString<T1, T2>(ICollection<KeyValuePair<T1, T2>> dict, Func<T2, string> toString = null, string separator = null)
        {
            if (dict == null || dict.Count == 0)
            {
                return "[]";
            }
            if (separator == null)
            {
                separator = Environment.NewLine;
            }
            var sb = StringBuilderCache.Acquire();
            sb.Append("[");
            var enumerator = dict.GetEnumerator();
            int index = 0;
            while (enumerator.MoveNext())
            {
                var pair = enumerator.Current;
                sb.Append("{");
                sb.Append(pair.Key);
                sb.Append(" -> ");

                string val;
                if (toString != null)
                    val = toString(pair.Value);
                else
                    val = pair.Value == null ? "null" : pair.Value.ToString();
                sb.Append(val);

                sb.Append("}");
                if (index++ < dict.Count - 1)
                    sb.Append(separator);
            }
            sb.Append("]");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static string TimeSpanToString(TimeSpan timeSpan)
        {
            //00:03:32.8289777
            return String.Format("{0}h:{1}m:{2}s.{3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        public static long TicksToMilliSeconds(long ticks)
        {
            return (long)TimeSpan.FromTicks(ticks).TotalMilliseconds;
        }

        public static float AverageTicksToMilliSeconds(float ticks)
        {
            return (float)TimeSpan.FromTicks((long)ticks).TotalMilliseconds;
        }

        ///// <summary>
        ///// Parse a Uri as an IPEndpoint.
        ///// </summary>
        ///// <param name="uri">The input Uri</param>
        ///// <returns></returns>
        //public static System.Net.IPEndPoint ToIPEndPoint(this Uri uri)
        //{
        //  switch (uri.Scheme)
        //  {
        //    case "gwy.tcp":
        //      return new System.Net.IPEndPoint(System.Net.IPAddress.Parse(uri.Host), uri.Port);
        //  }
        //  return null;
        //}

        ///// <summary>
        ///// Parse a Uri as a Silo address, including the IPEndpoint and generation identifier.
        ///// </summary>
        ///// <param name="uri">The input Uri</param>
        ///// <returns></returns>
        //public static SiloAddress ToSiloAddress(this Uri uri)
        //{
        //  switch (uri.Scheme)
        //  {
        //    case "gwy.tcp":
        //      return SiloAddress.New(uri.ToIPEndPoint(), uri.Segments.Length > 1 ? int.Parse(uri.Segments[1]) : 0);
        //  }
        //  return null;
        //}

        ///// <summary>
        ///// Represent an IP end point in the gateway URI format..
        ///// </summary>
        ///// <param name="ep">The input IP end point</param>
        ///// <returns></returns>
        //public static Uri ToGatewayUri(this System.Net.IPEndPoint ep)
        //{
        //  return new Uri(string.Format("gwy.tcp://{0}:{1}/0", ep.Address, ep.Port));
        //}

        ///// <summary>
        ///// Represent a silo address in the gateway URI format.
        ///// </summary>
        ///// <param name="address">The input silo address</param>
        ///// <returns></returns>
        //public static Uri ToGatewayUri(this SiloAddress address)
        //{
        //  return new Uri(string.Format("gwy.tcp://{0}:{1}/{2}", address.Endpoint.Address, address.Endpoint.Port, address.Generation));
        //}

        /// <summary>Calculates an integer hash value based on the consistent identity hash of a string.</summary>
        /// <param name="text">The string to hash.</param>
        /// <returns>An integer hash for the string.</returns>
        public static int CalculateIdHash(string text)
        {
#if NETSTANDARD2_0
            SHA256 sha = SHA256.Create(); // This is one implementation of the abstract class SHA1.
            int hash = 0;
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(text);
                byte[] result = sha.ComputeHash(data);
                for (int i = 0; i < result.Length; i += 4)
                {
                    int tmp = (result[i] << 24) | (result[i + 1] << 16) | (result[i + 2] << 8) | (result[i + 3]);
                    hash = hash ^ tmp;
                }
            }
            finally
            {
                sha.Dispose();
            }
            return hash;
#else
            var input = BitConverter.IsLittleEndian ? MemoryMarshal.AsBytes(text.AsSpan()) : Encoding.Unicode.GetBytes(text);

            Span<int> result = stackalloc int[256 / 8 / sizeof(int)];
            var sha = SHA256.Create();
            sha.TryComputeHash(input, MemoryMarshal.AsBytes(result), out _);
            sha.Dispose();

            var hash = 0;
            for (var i = 0; i < result.Length; i++) hash ^= result[i];
            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(hash) : hash;
#endif

        }

        /// <summary>Calculates a Guid hash value based on the consistent identity a string.</summary>
        /// <param name="text">The string to hash.</param>
        /// <returns>An integer hash for the string.</returns>
        public static Guid CalculateGuidHash(string text)
        {
#if (NETCOREAPP2_1 || NETSTANDARD2_1 || NETSTANDARD2_0)
            SHA256 sha = SHA256.Create(); // This is one implementation of the abstract class SHA1.
            byte[] hash = new byte[16];
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(text);
                byte[] result = sha.ComputeHash(data);
                for (int i = 0; i < result.Length; i++)
                {
                    byte tmp = (byte)(hash[i % 16] ^ result[i]);
                    hash[i % 16] = tmp;
                }
            }
            finally
            {
                sha.Dispose();
            }
            return new Guid(hash);
#else
            var input = BitConverter.IsLittleEndian ? MemoryMarshal.AsBytes(text.AsSpan()) : Encoding.Unicode.GetBytes(text);

            Span<byte> result = stackalloc byte[256 / 8];
            var sha = SHA256.Create();
            sha.TryComputeHash(input, result, out _);
            sha.Dispose();

            MemoryMarshal.AsRef<long>(result) ^= MemoryMarshal.Read<long>(result.Slice(16));
            MemoryMarshal.AsRef<long>(result.Slice(8)) ^= MemoryMarshal.Read<long>(result.Slice(24));
            return BitConverter.IsLittleEndian ? MemoryMarshal.Read<Guid>(result) : new Guid(result.Slice(0, 16));
#endif
        }

        public static bool TryFindException(Exception original, Type targetType, out Exception target)
        {
            if (original.GetType() == targetType)
            {
                target = original;
                return true;
            }
            else if (original is AggregateException)
            {
                var baseEx = original.GetBaseException();
                if (baseEx.GetType() == targetType)
                {
                    target = baseEx;
                    return true;
                }
                else
                {
                    var newEx = ((AggregateException)original).Flatten();
                    foreach (var exc in newEx.InnerExceptions)
                    {
                        if (exc.GetType() == targetType)
                        {
                            target = newEx;
                            return true;
                        }
                    }
                }
            }
            target = null;
            return false;
        }

        public static void SafeExecute(Action action, ILogger logger = null, string caller = null)
        {
            SafeExecute(action, logger, (object)caller);
        }

        // a function to safely execute an action without any exception being thrown.
        // callerGetter function is called only in faulty case (now string is generated in the success case).
        public static void SafeExecute(Action action, ILogger logger, Func<string> callerGetter) => SafeExecute(action, logger, (object)callerGetter);

        private static void SafeExecute(Action action, ILogger logger, object callerGetter)
        {
            try
            {
                action();
            }
            catch (Exception exc)
            {
                if (logger != null)
                {
                    try
                    {
                        string caller = null;
                        switch (callerGetter)
                        {
                            case string value:
                                caller = value;
                                break;
                            case Func<string> func:
                                try
                                {
                                    caller = func();
                                }
                                catch
                                {
                                }

                                break;
                        }

                        foreach (var e in exc.FlattenAggregate())
                        {
                            logger.LogWarning(
                                $"Ignoring {e.GetType().FullName} exception thrown from an action called by {caller ?? String.Empty}.", exc);
                        }
                    }
                    catch
                    {
                        // now really, really ignore.
                    }
                }
            }
        }

        /// <summary>Get the last characters of a string.</summary>
        /// <param name="s"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Tail(this string s, int count)
        {
            return s.Substring(Math.Max(0, s.Length - count));
        }

        public static TimeSpan Since(DateTime start)
        {
            return DateTime.UtcNow.Subtract(start);
        }

        public static List<Exception> FlattenAggregate(this Exception exc)
        {
            var result = new List<Exception>();
            if (exc is AggregateException)
                result.AddRange(exc.InnerException.FlattenAggregate());
            else
                result.Add(exc);
            return result;
        }

        public static AggregateException Flatten(this ReflectionTypeLoadException rtle)
        {
            // if ReflectionTypeLoadException is thrown, we need to provide the
            // LoaderExceptions property in order to make it meaningful.
            var all = new List<Exception> { rtle };
            all.AddRange(rtle.LoaderExceptions);
            throw new AggregateException("A ReflectionTypeLoadException has been thrown. The original exception and the contents of the LoaderExceptions property have been aggregated for your convenience.", all);
        }

        /// <summary>
        /// </summary>
        public static IEnumerable<List<T>> BatchIEnumerable<T>(this IEnumerable<T> sequence, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in sequence)
            {
                batch.Add(item);
                // when we've accumulated enough in the batch, send it out  
                if (batch.Count >= batchSize)
                {
                    yield return batch; // batch.ToArray();
                    batch = new List<T>(batchSize);
                }
            }
            if (batch.Count > 0)
            {
                yield return batch; //batch.ToArray();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetStackTrace(int skipFrames = 0)
        {
            skipFrames += 1; //skip this method from the stack trace
#if NETSTANDARD
            skipFrames += 2; //skip the 2 Environment.StackTrace related methods.
            var stackTrace = Environment.StackTrace;
            for (int i = 0; i < skipFrames; i++)
            {
                stackTrace = stackTrace.Substring(stackTrace.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
            }
            return stackTrace;
#else
            return new System.Diagnostics.StackTrace(skipFrames).ToString();
#endif
        }

        // Base32 encoding - in ascii sort order for easy text based sorting 
        private static readonly string _encode32Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV";

        public static unsafe string GenerateStringId(long id)
        {
            // The following routine is ~310% faster than calling long.ToString() on x64
            // and ~600% faster than calling long.ToString() on x86 in tight loops of 1 million+ iterations
            // See: https://github.com/aspnet/Hosting/pull/385

            // stackalloc to allocate array on stack rather than heap
            char* charBuffer = stackalloc char[13];

            charBuffer[0] = _encode32Chars[(int)(id >> 60) & 31];
            charBuffer[1] = _encode32Chars[(int)(id >> 55) & 31];
            charBuffer[2] = _encode32Chars[(int)(id >> 50) & 31];
            charBuffer[3] = _encode32Chars[(int)(id >> 45) & 31];
            charBuffer[4] = _encode32Chars[(int)(id >> 40) & 31];
            charBuffer[5] = _encode32Chars[(int)(id >> 35) & 31];
            charBuffer[6] = _encode32Chars[(int)(id >> 30) & 31];
            charBuffer[7] = _encode32Chars[(int)(id >> 25) & 31];
            charBuffer[8] = _encode32Chars[(int)(id >> 20) & 31];
            charBuffer[9] = _encode32Chars[(int)(id >> 15) & 31];
            charBuffer[10] = _encode32Chars[(int)(id >> 10) & 31];
            charBuffer[11] = _encode32Chars[(int)(id >> 5) & 31];
            charBuffer[12] = _encode32Chars[(int)id & 31];

            // string ctor overload that takes char*
            return new string(charBuffer, 0, 13);
        }
    }
}
