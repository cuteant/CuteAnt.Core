//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#if DESKTOPCLR
namespace CuteAnt
{
  // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/computer_name_format_str.asp
  public enum ComputerNameFormat
  {
    NetBIOS,
    DnsHostName,
    Dns,
    DnsFullyQualified,
    PhysicalNetBIOS,
    PhysicalDnsHostName,
    PhysicalDnsDomain,
    PhysicalDnsFullyQualified
  }
}
#endif