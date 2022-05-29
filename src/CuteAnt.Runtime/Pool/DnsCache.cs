//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
#if !NET40
using System.Threading.Tasks;
#endif
#if NETFRAMEWORK
using System.Diagnostics;
using System.ServiceModel;
using CuteAnt.Runtime.Interop;
#else
using CuteAnt.Runtime;
#endif

namespace CuteAnt.Pool
{
  public static class DnsCache
  {
    private const int c_mruWatermark = 64;
    private static MruCache<string, DnsCacheEntry> s_resolveCache = new MruCache<string, DnsCacheEntry>(c_mruWatermark);
    private static readonly TimeSpan s_cacheTimeout = TimeSpan.FromSeconds(2);

    // Double-checked locking pattern requires volatile for read/write synchronization
    private static volatile string s_machineName;

    private static object ThisLock => s_resolveCache;

    public static string MachineName
    {
      get
      {
        if (s_machineName == null)
        {
          lock (ThisLock)
          {
            if (s_machineName == null)
            {
              try
              {
#if NET40
                s_machineName = Dns.GetHostEntry(String.Empty).HostName;
#else
                s_machineName = Dns.GetHostEntryAsync(String.Empty).GetAwaiter().GetResult().HostName;
#endif
              }
              catch (SocketException exception)
              {
#if NETFRAMEWORK
                DiagnosticUtility.ExceptionUtility.DiagnosticTraceHandledException(exception, TraceEventType.Information);
                // we fall back to the NetBios machine if Dns fails
                s_machineName = UnsafeNativeMethods.GetComputerName(ComputerNameFormat.PhysicalNetBIOS);
#else
                throw exception;
#endif
              }
            }
          }
        }

        return s_machineName;
      }
    }

#if NET40
    public static IPHostEntry Resolve(Uri uri) => Resolve(uri.DnsSafeHost);

    public static IPHostEntry Resolve(string hostName)
    {
      IPHostEntry hostEntry = null;
      DateTime now = DateTime.UtcNow;

      lock (ThisLock)
      {
        if (s_resolveCache.TryGetValue(hostName, out DnsCacheEntry cacheEntry))
        {
          if (now.Subtract(cacheEntry.TimeStamp) > s_cacheTimeout)
          {
            s_resolveCache.Remove(hostName);
            cacheEntry = null;
          }
          else
          {
            if (cacheEntry.HostEntry == null)
            {
              throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                  new EndpointNotFoundException(string.Format(InternalSR.DnsResolveFailed, hostName)));
            }
            hostEntry = cacheEntry.HostEntry;
          }
        }
      }

      if (hostEntry == null)
      {
        SocketException dnsException = null;
        try
        {
          hostEntry = Dns.GetHostEntry(hostName);
        }
        catch (SocketException e)
        {
          dnsException = e;
        }

        lock (ThisLock)
        {
          // MruCache doesn't have a this[] operator, so we first remove (just in case it exists already)
          s_resolveCache.Remove(hostName);
          s_resolveCache.Add(hostName, new DnsCacheEntry(hostEntry, now));
        }

        if (dnsException != null)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
              new EndpointNotFoundException(string.Format(InternalSR.DnsResolveFailed, hostName), dnsException));
        }
      }

      return hostEntry;
    }

    internal class DnsCacheEntry
    {
      private IPHostEntry _hostEntry;
      private DateTime _timeStamp;

      public DnsCacheEntry(IPHostEntry hostEntry, DateTime timeStamp)
      {
        _hostEntry = hostEntry;
        _timeStamp = timeStamp;
      }

      public IPHostEntry HostEntry => _hostEntry;

      public DateTime TimeStamp => _timeStamp;
    }
#else
    public static Task<IPAddress[]> ResolveAsync(Uri uri) => ResolveAsync(uri.DnsSafeHost);

    public static async Task<IPAddress[]> ResolveAsync(string hostName)
    {
      IPAddress[] hostAddresses = null;
      DateTime now = DateTime.UtcNow;

      lock (ThisLock)
      {
        DnsCacheEntry cacheEntry;
        if (s_resolveCache.TryGetValue(hostName, out cacheEntry))
        {
          if (now.Subtract(cacheEntry.TimeStamp) > s_cacheTimeout)
          {
            s_resolveCache.Remove(hostName);
            cacheEntry = null;
          }
          else
          {
            if (cacheEntry.AddressList == null)
            {
              throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                  new EndpointNotFoundException(string.Format(InternalSR.DnsResolveFailed, hostName)));
            }
            hostAddresses = cacheEntry.AddressList;
          }
        }
      }

      if (hostAddresses == null)
      {
        SocketException dnsException = null;
        try
        {
          hostAddresses = await LookupHostName(hostName);
        }
        catch (SocketException e)
        {
          dnsException = e;
        }

        lock (ThisLock)
        {
          // MruCache doesn't have a this[] operator, so we first remove (just in case it exists already)
          s_resolveCache.Remove(hostName);
          s_resolveCache.Add(hostName, new DnsCacheEntry(hostAddresses, now));
        }

        if (dnsException != null)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
              new EndpointNotFoundException(string.Format(InternalSR.DnsResolveFailed, hostName), dnsException));
        }
      }

      return hostAddresses;
    }

    internal static async Task<IPAddress[]> LookupHostName(string hostName)
    {
      return (await Dns.GetHostEntryAsync(hostName)).AddressList;
    }

    internal class DnsCacheEntry
    {
      private DateTime _timeStamp;
      private IPAddress[] _addressList;

      public DnsCacheEntry(IPAddress[] addressList, DateTime timeStamp)
      {
        _timeStamp = timeStamp;
        _addressList = addressList;
      }

      public IPAddress[] AddressList => _addressList;

      public DateTime TimeStamp => _timeStamp;
    }
#endif
  }
}
