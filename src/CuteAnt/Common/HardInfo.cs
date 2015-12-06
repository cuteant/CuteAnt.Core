﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using CuteAnt.Log;
using CuteAnt.Xml;
using Microsoft.VisualBasic.Devices;

namespace CuteAnt.Common
{
  /// <summary>硬件信息</summary>
  public class HardInfo
  {
    #region 获取信息

    /// <summary>内部获取</summary>
    public class _
    {
      private static String _BaseBoard;

      /// <summary>主板序列号</summary>
      public static String BaseBoard
      {
        get
        {
          if (_BaseBoard == null)
          {
            _BaseBoard = GetInfo("Win32_BaseBoard", "SerialNumber");
            if (_BaseBoard.IsNullOrWhiteSpace()) _BaseBoard = GetInfo("Win32_BaseBoard", "Product");
            _BaseBoard = GetInfo("Win32_BaseBoard", "Product") + ";" + _BaseBoard;
          }
          return _BaseBoard;
        }
      }

      private static String _Processors;

      /// <summary>处理器序列号</summary>
      public static String Processors
      {
        get
        {
          if (_Processors == null)
          {
            var name = GetInfo("Win32_Processor", "Name");
            var MaxClockSpeed = GetInfo("Win32_Processor", "MaxClockSpeed").ToDouble() / 1000;
            var ProcessorId = GetInfo("Win32_Processor", "ProcessorId");

            while (name.Contains("  ")) name = name.Replace("  ", " ");
            var speed = MaxClockSpeed.ToString("n2") + "GHz";
            if (name.Contains(speed))
              speed = null;
            else
              speed += " ";

            _Processors = name + " " + speed + ProcessorId;
          }
          return _Processors;
        }
      }

      private static Int64? _Memory;

      /// <summary>内存总量</summary>
      public static Int64 Memory
      {
        get
        {
          if (_Memory == null)
          {
            _Memory = (Int64)new ComputerInfo().TotalPhysicalMemory;

            //_Memory = Convert.ToInt64(GetInfo("Win32_LogicalMemoryConfiguration", "TotalPhysicalMemory"));
          }
          return _Memory.Value;
        }
      }

      private static String _Disk;

      /// <summary>磁盘名称</summary>
      public static String Disk
      {
        get
        {
          if (_Disk == null) _Disk = GetInfo("Win32_DiskDrive", "Model");
          return _Disk;

          //上面的方式取驱动器序列号会取得包括U盘和网络映射驱动器的序列号，实际只要当前所在盘就可以了
          //return Volume;
        }
      }

      private static String _DiskSerial = String.Empty;

      /// <summary>磁盘序列号</summary>
      public static String DiskSerial
      {
        get
        {
          if (_DiskSerial.IsNullOrWhiteSpace()) _DiskSerial = GetInfo("Win32_DiskDrive", "SerialNumber");
          return _DiskSerial;
        }
      }

      private static String _Volume;

      /// <summary>驱动器序列号</summary>
      public static String Volume
      {
        get
        {
          //if (_Volume.IsNullOrWhiteSpace()) _Volume = GetInfo("Win32_DiskDrive", "Model");
          //磁盘序列号不够明显，故使用驱动器序列号代替
          String id = AppDomain.CurrentDomain.BaseDirectory.Substring(0, 2);
          if (_Volume == null) _Volume = GetInfo("Win32_LogicalDisk Where DeviceID=\"" + id + "\"", "VolumeSerialNumber");
          return _Volume;
        }
      }

      private static String _Macs;

      /// <summary>网卡地址序列号</summary>
      public static String Macs
      {
        get
        {
          if (_Macs != null) return _Macs;

          //return GetInfo("Win32_NetworkAdapterConfiguration", "MacAddress");
          var cimobject = new ManagementClass("Win32_NetworkAdapterConfiguration");
          var moc = cimobject.GetInstances();
          var bbs = new List<String>();
          foreach (ManagementObject mo in moc)
          {
            if (mo != null &&
                mo.Properties != null &&
                mo.Properties["MacAddress"] != null &&
                mo.Properties["MacAddress"].Value != null &&
                mo.Properties["IPEnabled"] != null &&
                (bool)mo.Properties["IPEnabled"].Value)
            {
              //bbs.Add(mo.Properties["MacAddress"].Value.ToString());
              String s = mo.Properties["MacAddress"].Value.ToString();
              if (!bbs.Contains(s)) bbs.Add(s);
            }
          }
          bbs.Sort();
          var sb = new StringBuilder(bbs.Count * 15);
          foreach (var s in bbs)
          {
            if (sb.Length > 0) sb.Append(",");
            sb.Append(s);
          }
          _Macs = sb.ToString().Trim();
          return _Macs;
        }
      }

      private static String _IPs;

      /// <summary>IP地址</summary>
      public static String IPs
      {
        get
        {
          if (_IPs != null) return _IPs;

          //return null;
          var cimobject = new ManagementClass("Win32_NetworkAdapterConfiguration");
          var moc = cimobject.GetInstances();
          var bbs = new List<String>();
          foreach (ManagementObject mo in moc)
          {
            if (mo != null &&
                mo.Properties != null &&
                mo.Properties["IPAddress"] != null &&
                mo.Properties["IPAddress"].Value != null &&
                mo.Properties["IPEnabled"] != null &&
                (bool)mo.Properties["IPEnabled"].Value)
            {
              String[] ss = (String[])mo.Properties["IPAddress"].Value;
              if (ss != null)
              {
                foreach (String s in ss)
                  if (!bbs.Contains(s)) bbs.Add(s);
              }

              //bbs.Add(mo.Properties["IPAddress"].Value.ToString());
            }
          }
          bbs.Sort();
          var sb = new StringBuilder(bbs.Count * 15);
          foreach (var s in bbs)
          {
            if (sb.Length > 0) sb.Append(",");
            sb.Append(s);
          }
          _IPs = sb.ToString().Trim();
          return _IPs;
        }
      }
    }

    #endregion

    #region 属性

    private String _MachineName;

    /// <summary>机器名</summary>
    public String MachineName
    {
      get { return _MachineName; }
      set { _MachineName = value; }
    }

    private String _BaseBoard;

    /// <summary>主板</summary>
    public String BaseBoard
    {
      get { return _BaseBoard; }
      set { _BaseBoard = value; }
    }

    private String _Processors;

    /// <summary>处理器</summary>
    public String Processors
    {
      get { return _Processors; }
      set { _Processors = value; }
    }

    private String _Disk;

    /// <summary>磁盘</summary>
    public String Disk
    {
      get { return _Disk; }
      set { _Disk = value; }
    }

    private String _DiskSerial;

    /// <summary>磁盘序列号</summary>
    public String DiskSerial
    {
      get { return _DiskSerial; }
      set { _DiskSerial = value; }
    }

    private String _Volume;

    /// <summary>驱动器序列号</summary>
    public String Volume
    {
      get { return _Volume; }
      set { _Volume = value; }
    }

    private String _Macs;

    /// <summary>网卡</summary>
    public String Macs
    {
      get { return _Macs; }
      set { _Macs = value; }
    }

    private String _IPs;

    /// <summary>IP地址</summary>
    public String IPs
    {
      get { return _IPs; }
      set { _IPs = value; }
    }

    private String _OSVersion;

    /// <summary>系统版本</summary>
    public String OSVersion
    {
      get { return _OSVersion; }
      set { _OSVersion = value; }
    }

    private long _Memory;

    /// <summary>内存</summary>
    public long Memory
    {
      get { return _Memory; }
      set { _Memory = value; }
    }

    private Int32 _ScreenWidth;

    /// <summary>屏幕宽</summary>
    public Int32 ScreenWidth
    {
      get { return _ScreenWidth; }
      set { _ScreenWidth = value; }
    }

    private Int32 _ScreenHeight;

    /// <summary>屏幕高</summary>
    public Int32 ScreenHeight
    {
      get { return _ScreenHeight; }
      set { _ScreenHeight = value; }
    }

    private Int64 _DiskSize;

    /// <summary>磁盘大小</summary>
    public Int64 DiskSize
    {
      get { return _DiskSize; }
      set { _DiskSize = value; }
    }

    #endregion

    #region 构造

    private HardInfo()
    {
    }

    private void GetLocal()
    {
      MachineName = Environment.MachineName;
      BaseBoard = _.BaseBoard;
      Processors = _.Processors;
      Disk = _.Disk;
      DiskSerial = _.DiskSerial;
      Volume = _.Volume;
      Macs = _.Macs;
      IPs = _.IPs;
      OSVersion = Environment.OSVersion.ToString();
      Memory = _.Memory;
      ScreenWidth = GetInfo("Win32_DesktopMonitor", "ScreenWidth").ToInt();
      ScreenHeight = GetInfo("Win32_DesktopMonitor", "ScreenHeight").ToInt();

      var str = GetInfo("Win32_DiskDrive", "Size");
      Int64 n = 0;
      if (Int64.TryParse(str, out n)) DiskSize = n;
      if (DiskSize <= 0)
      {
        var drives = DriveInfo.GetDrives();
        if (drives != null && drives.Length > 0)
        {
          foreach (var item in drives)
          {
            if (item.DriveType == DriveType.CDRom ||
                item.DriveType == DriveType.Network ||
                item.DriveType == DriveType.NoRootDirectory) continue;

            DiskSize += item.TotalSize;
          }
        }
      }
    }

    private static HardInfo _Current;

    /// <summary>当前机器硬件信息</summary>
    public static HardInfo Current
    {
      get
      {
        if (_Current != null) return _Current;
        lock (typeof(HardInfo))
        {
          if (_Current != null) return _Current;

          try
          {
            _Current = new HardInfo();
            _Current.GetLocal();
          }
          catch (Exception ex)
          {
            HmTrace.WriteException(ex);
          }

          return _Current;
        }
      }
    }

    #endregion

    #region WMI辅助

    /// <summary>获取WMI信息</summary>
    /// <param name="path"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static String GetInfo(String path, String property)
    {
      var bbs = new List<String>();
      try
      {
        var wql = String.Format("Select {0} From {1}", property, path);
        var cimobject = new ManagementObjectSearcher(wql);
        var moc = cimobject.Get();
        foreach (ManagementObject mo in moc)
        {
          if (mo != null &&
              mo.Properties != null &&
              mo.Properties[property] != null &&
              mo.Properties[property].Value != null)
            bbs.Add(mo.Properties[property].Value.ToString());
        }
      }
      catch (Exception ex)
      {
        HmTrace.WriteDebug(ex, "获取{0} {1}硬件信息失败\r\n{2}", path, property);
        return null;
      }
      bbs.Sort();
      var sb = new StringBuilder(bbs.Count * 15);
      foreach (var s in bbs)
      {
        if (sb.Length > 0) sb.Append(",");
        sb.Append(s);
      }
      return sb.ToString().Trim();
    }

    #endregion

    #region 导入导出

    /// <summary></summary>
    /// <returns></returns>
    public ExtendData ToExtend()
    {
      ExtendData data = new ExtendData();
      data["MachineName"] = MachineName;
      data["BaseBoard"] = BaseBoard;
      data["Processors"] = Processors;
      data["Disk"] = Disk;
      data["DiskSerial"] = DiskSerial;
      data["Volume"] = Volume;
      data["Macs"] = Macs;
      data["IPs"] = IPs;
      data["OSVersion"] = OSVersion;
      data["Memory"] = Memory.ToString();
      data["ScreenWidth"] = ScreenWidth.ToString();
      data["ScreenHeight"] = ScreenHeight.ToString();
      data["DiskSize"] = DiskSize.ToString();

      return data;
    }

    /// <summary></summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static HardInfo FromExtend(ExtendData data)
    {
      HardInfo entity = new HardInfo();
      entity.MachineName = data["MachineName"];
      entity.BaseBoard = data["BaseBoard"];
      entity.Processors = data["Processors"];
      entity.Disk = data["Disk"];
      entity.DiskSerial = data["DiskSerial"];
      entity.Volume = data["Volume"];
      entity.Macs = data["Macs"];
      entity.IPs = data["IPs"];
      entity.OSVersion = data["OSVersion"];
      entity.Memory = data.GetItem<Int64>("Memory");
      entity.ScreenWidth = data.GetItem<Int32>("ScreenWidth");
      entity.ScreenHeight = data.GetItem<Int32>("ScreenHeight");
      entity.DiskSize = data.GetItem<Int64>("DiskSize");

      return entity;
    }

    /// <summary>导出XML</summary>
    /// <returns></returns>
    public virtual String ToXml()
    {
      return ToExtend().ToXml();
    }

    /// <summary>导入</summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static HardInfo FromXml(String xml)
    {
      if (!String.IsNullOrEmpty(xml)) xml = xml.Trim();

      return FromExtend(ExtendData.FromXml(xml));
    }

    #endregion
  }
}