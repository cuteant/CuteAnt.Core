/************************************************************************************************************************
 * 作者		:	Loney
 * 邮箱		:	X-0.0-X@Live.cn
 * 创建日期	:	2012年1月1日
 * 日志	:
 *		*****************************************************************************************************************
 *		修改者	:	Loney
 *		邮箱		:	X-0.0-X@Live.cn
 *		修改日期	:	2011年10月31日 12:59
 *		目的 :
 *			 将判断 Windows 操作系统版本的代码重构成我期望的 Api.
 ***********************************************************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace CuteAnt
{
	/// <summary>操作系统类型.</summary>
	public enum OSVersionType
	{
		/// <summary>未知</summary>
		UnKnown,

		/// <summary>Microsoft Windows 3.1</summary>
		Microsoft_Windows_3_1,

		/// <summary>Microsoft Windows Me.</summary>
		Microsoft_Windows_Me,

		/// <summary>Microsoft Windows 98 Second Edition.</summary>
		Microsoft_Windows_98_SecondEdition,

		/// <summary>Microsoft Windows 98.</summary>
		Microsoft_Windows_98,

		/// <summary>Microsoft Windows 95 OS R2.</summary>
		Microsoft_Windows_95_OS_R2,

		/// <summary>Microsoft Windows 95.</summary>
		Microsoft_Windows_95,

		/// <summary>Microsoft Windows NT 3.51.</summary>
		Microsoft_Windows_NT_3_5_1,

		/// <summary>Microsoft Windows NT 4.0.</summary>
		Microsoft_Windows_NT_4_0,

		/// <summary>Microsoft Windows NT 4.0 Server.</summary>
		Microsoft_Windows_NT_4_0_Server,

		/// <summary>Microsoft Windows 2000.</summary>
		Microsoft_Windows_2000,

		/// <summary>Microsoft Windows XP.</summary>
		Microsoft_Windows_XP,

		/// <summary>Microsoft Windows Server 2003.</summary>
		Microsoft_Windows_Server_2003,

		/// <summary>Microsoft Windows Vista.</summary>
		Microsoft_Windows_Vista,

		/// <summary>Microsoft Windows 7.</summary>
		Microsoft_Windows_7,

		/// <summary>Microsoft Windows Server 2008.</summary>
		Microsoft_Windows_Server_2008,

		/// <summary>Microsoft Windows Server 2008 R2.</summary>
		Microsoft_Windows_Server_2008_R2,

		/// <summary>Microsoft Windows CE.</summary>
		Microsoft_Windows_CE,

		/// <summary>Unix.</summary>
		Unix,

		/// <summary>MacOSX,基于 Unix 的增强版.</summary>
		MacOSX,

		/// <summary>XBox 360</summary>
		XBox_360
	}

	/// <summary>提供有关当前环境和平台的信息以及操作它们的方法.</summary>
	public sealed class EnvironmentHelper
	{
		#region 常量

		/// <summary>回车换行符</summary>
		static readonly public String CarriageReturnLineFeed = "\r\n";

		/// <summary>空字符</summary>
		static readonly public String Empty = "";

		/// <summary>回车符</summary>
		static readonly public Char CarriageReturn = '\r';

		/// <summary>换行符</summary>
		static readonly public Char LineFeed = '\n';

		/// <summary>制表符</summary>
		static readonly public Char Tab = '\t';

		#endregion

		#region 操作系统

		/***************************************************************************************************************
		 * 作者		:	Johnny J
		 * Blog		:	http://www.codeproject.com/Members/Johnny-J
		 * 创建日期	:	2011年10月31日
		 * 日志 :
		 *		********************************************************************************************************
		 *		修改者	:	Loney
		 *		邮箱		:	X-0.0-X@Live.cn
		 *		修改日期	:	2011年10月31日 12:59
		 *		目的 :
		 *			 重构成我期望的 Api.
		 *			 实现完整的 System.PlatformID 检测.
		 *		*****************************************************************************************************************
		 *		修改者	:	Loney
		 *		邮箱		:	X-0.0-X@Live.cn
		 *		修改日期	:	2011年11月4日 13:52
		 *		目的 :
		 *			 新增 IsGreaterThanOrEqualToVista 属性.
		 *		*****************************************************************************************************************
		 *		修改者	:	Loney
		 *		邮箱		:	X-0.0-X@Live.cn
		 *		修改日期	:	2012年1月7日 22:01
		 *		目的 :
		 *			 重构 Api 效果如下 :
		 *				操作系统名称 : OSName				Microsoft Windows Server 2003
		 *				操作系统版本 : OSServicePack		Service Pack 2
		 *				操作系统补丁 : OSEdition			Standard
		 *				操作系统版本 : OSVersion			5.2.3790.131072
		 *				操作系统版本 : OSVersionString	Microsoft Windows Server 2003 5.2.3790 Service Pack 2
		 **************************************************************************************************************/

		#region 嵌套类型

		private struct OSVERSIONINFOEX
		{
			public Int32 dwOSVersionInfoSize;
			public Int32 dwMajorVersion;
			public Int32 dwMinorVersion;
			public Int32 dwBuildNumber;
			public Int32 dwPlatformId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public String szCSDVersion;

			public Int16 wServicePackMajor;
			public Int16 wServicePackMinor;
			public Int16 wSuiteMask;
			public Byte wProductType;
			public Byte wReserved;
		}

		private struct SYSTEM_INFO
		{
			public UInt32 dwPageSize;
			public IntPtr lpMinimumApplicationAddress;
			public IntPtr lpMaximumApplicationAddress;
			public IntPtr dwActiveProcessorMask;
			public UInt32 dwNumberOfProcessors;
			public UInt32 dwProcessorType;
			public UInt32 dwAllocationGranularity;
			public UInt16 dwProcessorLevel;
			public UInt16 dwProcessorRevision;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct _PROCESSOR_INFO_UNION
		{
			[FieldOffset(0)]
			internal UInt32 dwOemId;

			[FieldOffset(0)]
			internal UInt16 wProcessorArchitecture;

			[FieldOffset(2)]
			internal UInt16 wReserved;
		}

		#endregion

		#region 私有字段

		#region 产品

		private const Int32 PRODUCT_UNDEFINED = 0;
		private const Int32 PRODUCT_ULTIMATE = 1;
		private const Int32 PRODUCT_HOME_BASIC = 2;
		private const Int32 PRODUCT_HOME_PREMIUM = 3;
		private const Int32 PRODUCT_ENTERPRISE = 4;
		private const Int32 PRODUCT_HOME_BASIC_N = 5;
		private const Int32 PRODUCT_BUSINESS = 6;
		private const Int32 PRODUCT_STANDARD_SERVER = 7;
		private const Int32 PRODUCT_DATACENTER_SERVER = 8;
		private const Int32 PRODUCT_SMALLBUSINESS_SERVER = 9;
		private const Int32 PRODUCT_ENTERPRISE_SERVER = 10;
		private const Int32 PRODUCT_STARTER = 11;
		private const Int32 PRODUCT_DATACENTER_SERVER_CORE = 12;
		private const Int32 PRODUCT_STANDARD_SERVER_CORE = 13;
		private const Int32 PRODUCT_ENTERPRISE_SERVER_CORE = 14;
		private const Int32 PRODUCT_ENTERPRISE_SERVER_IA64 = 15;
		private const Int32 PRODUCT_BUSINESS_N = 16;
		private const Int32 PRODUCT_WEB_SERVER = 17;
		private const Int32 PRODUCT_CLUSTER_SERVER = 18;
		private const Int32 PRODUCT_HOME_SERVER = 19;
		private const Int32 PRODUCT_STORAGE_EXPRESS_SERVER = 20;
		private const Int32 PRODUCT_STORAGE_STANDARD_SERVER = 21;
		private const Int32 PRODUCT_STORAGE_WORKGROUP_SERVER = 22;
		private const Int32 PRODUCT_STORAGE_ENTERPRISE_SERVER = 23;
		private const Int32 PRODUCT_SERVER_FOR_SMALLBUSINESS = 24;
		private const Int32 PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 25;
		private const Int32 PRODUCT_HOME_PREMIUM_N = 26;
		private const Int32 PRODUCT_ENTERPRISE_N = 27;
		private const Int32 PRODUCT_ULTIMATE_N = 28;
		private const Int32 PRODUCT_WEB_SERVER_CORE = 29;
		private const Int32 PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 30;
		private const Int32 PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 31;
		private const Int32 PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 32;
		private const Int32 PRODUCT_SERVER_FOUNDATION = 33;
		private const Int32 PRODUCT_HOME_PREMIUM_SERVER = 34;
		private const Int32 PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 35;
		private const Int32 PRODUCT_STANDARD_SERVER_V = 36;
		private const Int32 PRODUCT_DATACENTER_SERVER_V = 37;
		private const Int32 PRODUCT_ENTERPRISE_SERVER_V = 38;
		private const Int32 PRODUCT_DATACENTER_SERVER_CORE_V = 39;
		private const Int32 PRODUCT_STANDARD_SERVER_CORE_V = 40;
		private const Int32 PRODUCT_ENTERPRISE_SERVER_CORE_V = 41;
		private const Int32 PRODUCT_HYPERV = 42;
		private const Int32 PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 43;
		private const Int32 PRODUCT_STORAGE_STANDARD_SERVER_CORE = 44;
		private const Int32 PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 45;
		private const Int32 PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 46;
		private const Int32 PRODUCT_STARTER_N = 47;
		private const Int32 PRODUCT_PROFESSIONAL = 48;
		private const Int32 PRODUCT_PROFESSIONAL_N = 49;
		private const Int32 PRODUCT_SB_SOLUTION_SERVER = 50;
		private const Int32 PRODUCT_SERVER_FOR_SB_SOLUTIONS = 51;
		private const Int32 PRODUCT_STANDARD_SERVER_SOLUTIONS = 52;
		private const Int32 PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 53;
		private const Int32 PRODUCT_SB_SOLUTION_SERVER_EM = 54;
		private const Int32 PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 55;
		private const Int32 PRODUCT_SOLUTION_EMBEDDEDSERVER = 56;
		private const Int32 PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE = 57;
		private const Int32 PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 59;
		private const Int32 PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 60;
		private const Int32 PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 61;
		private const Int32 PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 62;
		private const Int32 PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 63;
		private const Int32 PRODUCT_CLUSTER_SERVER_V = 64;
		private const Int32 PRODUCT_EMBEDDED = 65;
		private const Int32 PRODUCT_STARTER_E = 66;
		private const Int32 PRODUCT_HOME_BASIC_E = 67;
		private const Int32 PRODUCT_HOME_PREMIUM_E = 68;
		private const Int32 PRODUCT_PROFESSIONAL_E = 69;
		private const Int32 PRODUCT_ENTERPRISE_E = 70;
		private const Int32 PRODUCT_ULTIMATE_E = 71;

		#endregion

		#region 版本

		private const Int32 VER_NT_WORKSTATION = 1;
		private const Int32 VER_NT_DOMAIN_CONTROLLER = 2;
		private const Int32 VER_NT_SERVER = 3;
		private const Int32 VER_SUITE_SMALLBUSINESS = 1;
		private const Int32 VER_SUITE_ENTERPRISE = 2;
		private const Int32 VER_SUITE_TERMINAL = 16;
		private const Int32 VER_SUITE_DATACENTER = 128;
		private const Int32 VER_SUITE_SINGLEUSERTS = 256;
		private const Int32 VER_SUITE_PERSONAL = 512;
		private const Int32 VER_SUITE_BLADE = 1024;

		#endregion

		static private OSVersionType _InternalOSVersionType;
		static private OperatingSystem _OSVersion;

		#endregion

		static EnvironmentHelper()
		{
			_OSVersion = Environment.OSVersion;
			OSVersion = _OSVersion.Version;
			OSEdition = GetOSEdition();

			// OSName
			// OSVersion
			GetOSVersion();
			OSServicePack = GetOSServicePack();

			// 操作系统名称 [操作系统的主版本号、次版本号、内部版本号和修订版本号] 操作系统补丁包
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(OSName);
			sb.Append(" " + OSVersion.ToString(3));
			sb.Append(" " + OSServicePack);
			OSVersionString = sb.ToString();
			IsGreaterThanOrEqualToVista = _InternalOSVersionType >= OSVersionType.Microsoft_Windows_Vista;
		}

		#region 公有属性

		/// <summary>当前系统的发行版本.类似 "Standard".</summary>
		public static String OSEdition { get; private set; }

		/// <summary>获取当前计算机操作系统的版本名称.类似 "Microsoft Windows Server 2003".</summary>
		public static String OSName { get; private set; }

		/// <summary>获取标识操作系统的 <see cref="OSVersion"/> 对象.</summary>
		public static Version OSVersion { get; private set; }

		/// <summary>获取平台标识符、版本和当前安装在操作系统上的 Service Pack 的连接字符串表示形式.</summary>
		public static String OSVersionString { get; private set; }

		/// <summary>获取当前操作系统补丁包的详细信息.类似 "Service Pack 2".</summary>
		public static String OSServicePack { get; private set; }

		/// <summary>获取当前的系统是否是 Windows Vista (包含)以上.</summary>
		public static Boolean IsGreaterThanOrEqualToVista { get; private set; }

		#endregion

		#region 辅助方法

		// ToDo : 需要更新 Windows 操作系统的版本; 目前更新到 Windows 7

		#region P/Invoke

		[DllImport("Kernel32.dll")]
		private static extern Boolean GetProductInfo(Int32 osMajorVersion, Int32 osMinorVersion, Int32 spMajorVersion, Int32 spMinorVersion, out Int32 edition);

		[DllImport("kernel32.dll")]
		private static extern Boolean GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

		[DllImport("user32")]
		private static extern Int32 GetSystemMetrics(Int32 nIndex);

		[DllImport("kernel32.dll")]
		private static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

		[DllImport("kernel32.dll")]
		private static extern void GetNativeSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

		#endregion

		private static String GetOSEdition()
		{
			String result = "UnKnown";
			OSVERSIONINFOEX osVersionInfo = default(OSVERSIONINFOEX);
			osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
			if (GetVersionEx(ref osVersionInfo))
			{
				Int32 majorVersion = _OSVersion.Version.Major;
				Int32 minorVersion = _OSVersion.Version.Minor;
				Byte productType = osVersionInfo.wProductType;
				Int16 suiteMask = osVersionInfo.wSuiteMask;
				if (majorVersion == 4)
				{
					if (productType == 1)
					{
						result = "Workstation";
					}
					else
					{
						if (productType == 3)
						{
							if ((suiteMask & 2) != 0)
							{
								result = "Enterprise Server";
							}
							else
							{
								result = "Standard Server";
							}
						}
					}
				}
				else
				{
					if (majorVersion == 5)
					{
						if (productType == 1)
						{
							if ((suiteMask & 512) != 0)
							{
								result = "Home";
							}
							else
							{
								if (GetSystemMetrics(86) == 0)
								{
									result = "Professional";
								}
								else
								{
									result = "Tablet Edition";
								}
							}
						}
						else
						{
							if (productType == 3)
							{
								if (minorVersion == 0)
								{
									if ((suiteMask & 128) != 0)
									{
										result = "Datacenter Server";
									}
									else
									{
										if ((suiteMask & 2) != 0)
										{
											result = "Advanced Server";
										}
										else
										{
											result = "Server";
										}
									}
								}
								else
								{
									if ((suiteMask & 128) != 0)
									{
										result = "Datacenter";
									}
									else
									{
										if ((suiteMask & 2) != 0)
										{
											result = "Enterprise";
										}
										else
										{
											if ((suiteMask & 1024) != 0)
											{
												result = "Web Edition";
											}
											else
											{
												result = "Standard";
											}
										}
									}
								}
							}
						}
					}
					else
					{
						if (majorVersion == 6)
						{
							Int32 ed;
							if (GetProductInfo(
								majorVersion,
								minorVersion,
								(Int32)osVersionInfo.wServicePackMajor,
								(Int32)osVersionInfo.wServicePackMinor,
								out ed
							))
							{
								switch (ed)
								{
									case 0: result = "Unknown product"; break;

									case 1: result = "Ultimate"; break;

									case 2: result = "Home Basic"; break;

									case 3: result = "Home Premium"; break;

									case 4: result = "Enterprise"; break;

									case 5: result = "Home Basic N"; break;

									case 6: result = "Business"; break;

									case 7: result = "Standard Server"; break;

									case 8: result = "Datacenter Server"; break;

									case 9: result = "Microsoft Windows Small Business Server"; break;

									case 10: result = "Enterprise Server"; break;

									case 11: result = "Starter"; break;

									case 12: result = "Datacenter Server (core installation)"; break;

									case 13: result = "Standard Server (core installation)"; break;

									case 14: result = "Enterprise Server (core installation)"; break;

									case 15: result = "Enterprise Server for Itanium-based Systems"; break;

									case 16: result = "Business N"; break;

									case 17: result = "Web Server"; break;

									case 18: result = "HPC Edition"; break;

									case 20: result = "Express Storage Server"; break;

									case 21: result = "Standard Storage Server"; break;

									case 22: result = "Workgroup Storage Server"; break;

									case 23: result = "Enterprise Storage Server"; break;

									case 24: result = "Microsoft Windows Essential Server Solutions"; break;

									case 25: result = "Microsoft Windows Small Business Server Premium"; break;

									case 26: result = "Home Premium N"; break;

									case 27: result = "Enterprise N"; break;

									case 28: result = "Ultimate N"; break;

									case 29: result = "Web Server (core installation)"; break;

									case 30: result = "Microsoft Windows Essential Business Management Server"; break;

									case 31: result = "Microsoft Windows Essential Business Security Server"; break;

									case 32: result = "Microsoft Windows Essential Business Messaging Server"; break;

									case 33: result = "Server Foundation"; break;

									case 34: result = "Home Premium Server"; break;

									case 35: result = "Microsoft Windows Essential Server Solutions without Hyper-V"; break;

									case 36: result = "Standard Server without Hyper-V"; break;

									case 37: result = "Datacenter Server without Hyper-V"; break;

									case 38: result = "Enterprise Server without Hyper-V"; break;

									case 39: result = "Datacenter Server without Hyper-V (core installation)";
										break;

									case 40: result = "Standard Server without Hyper-V (core installation)";
										break;

									case 41: result = "Enterprise Server without Hyper-V (core installation)";
										break;

									case 42: result = "Microsoft Hyper-V Server"; break;

									case 43: result = "Express Storage Server (core installation)"; break;

									case 44: result = "Standard Storage Server (core installation)"; break;

									case 45: result = "Workgroup Storage Server (core installation)"; break;

									case 46: result = "Enterprise Storage Server (core installation)"; break;

									case 47: result = "Starter N"; break;

									case 48: result = "Professional"; break;

									case 49: result = "Professional N"; break;

									case 50: result = "SB Solution Server"; break;

									case 51: result = "Server for SB Solutions"; break;

									case 52: result = "Standard Server Solutions"; break;

									case 53: result = "Standard Server Solutions (core installation)"; break;

									case 54: result = "SB Solution Server EM"; break;

									case 55: result = "Server for SB Solutions EM"; break;

									case 56: result = "Solution Embedded Server"; break;

									case 57: result = "Solution Embedded Server (core installation)"; break;

									case 59: result = "Essential Business Server MGMT"; break;

									case 60: result = "Essential Business Server ADDL"; break;

									case 61: result = "Essential Business Server MGMTSVC"; break;

									case 62: result = "Essential Business Server ADDLSVC"; break;

									case 63: result = "Microsoft Windows Small Business Server Premium (core installation)";
										break;

									case 64: result = "HPC Edition without Hyper-V"; break;

									case 65: result = "Embedded"; break;

									case 66: result = "Starter E"; break;

									case 67: result = "Home Basic E"; break;

									case 68: result = "Home Premium E"; break;

									case 69: result = "Professional E"; break;

									case 70: result = "Enterprise E"; break;

									case 71: result = "Ultimate E"; break;
								}
							}
						}
					}
				}
			}
			return result;
		}

		private static void GetOSVersion()
		{
			OSName = "UnKnown";
			_InternalOSVersionType = OSVersionType.UnKnown;
			OSVERSIONINFOEX osVersionInfo = default(OSVERSIONINFOEX);
			osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
			if (GetVersionEx(ref osVersionInfo))
			{
				Int32 majorVersion = _OSVersion.Version.Major;
				Int32 minorVersion = _OSVersion.Version.Minor;

				switch (_OSVersion.Platform)
				{
					case PlatformID.Win32S:
						OSName = "Microsoft Windows 3.1";
						_InternalOSVersionType = OSVersionType.Microsoft_Windows_3_1;
						break;

					case PlatformID.Win32Windows:
						{
							if (majorVersion == 4)
							{
								String csdVersion = osVersionInfo.szCSDVersion;
								Int32 num = minorVersion;
								if (num != 0)
								{
									if (num != 10)
									{
										if (num == 90)
										{
											OSName = "Microsoft Windows Me";
											_InternalOSVersionType = OSVersionType.Microsoft_Windows_Me;
										}
									}
									else
									{
										if (csdVersion == "A")
										{
											OSName = "Microsoft Windows 98 Second Edition";
											_InternalOSVersionType = OSVersionType.Microsoft_Windows_98_SecondEdition;
										}
										else
										{
											OSName = "Microsoft Windows 98";
											_InternalOSVersionType = OSVersionType.Microsoft_Windows_98;
										}
									}
								}
								else
								{
									if (csdVersion == "B" || csdVersion == "C")
									{
										OSName = "Microsoft Windows 95 OS R2";
										_InternalOSVersionType = OSVersionType.Microsoft_Windows_95_OS_R2;
									}
									else
									{
										OSName = "Microsoft Windows 95";
										_InternalOSVersionType = OSVersionType.Microsoft_Windows_95;
									}
								}
							}
							break;
						}

					case PlatformID.Win32NT:
						{
							Byte productType = osVersionInfo.wProductType;

							switch (majorVersion)
							{
								case 3: OSName = "Microsoft Windows NT 3.51";
									_InternalOSVersionType = OSVersionType.Microsoft_Windows_NT_3_5_1;
									break;

								case 4:
									{
										switch (productType)
										{
											case 1: OSName = "Microsoft Windows NT 4.0";
												_InternalOSVersionType = OSVersionType.Microsoft_Windows_NT_4_0;
												break;

											case 3: OSName = "Microsoft Windows NT 4.0 Server";
												_InternalOSVersionType = OSVersionType.Microsoft_Windows_NT_4_0_Server;
												break;
										}
										break;
									}

								case 5:

									switch (minorVersion)
									{
										case 0:
											OSName = "Microsoft Windows 2000";
											_InternalOSVersionType = OSVersionType.Microsoft_Windows_2000;
											break;

										case 1:
											OSName = "Microsoft Windows XP";
											_InternalOSVersionType = OSVersionType.Microsoft_Windows_XP;
											break;

										case 2:
											OSName = "Microsoft Windows Server 2003";
											_InternalOSVersionType = OSVersionType.Microsoft_Windows_Server_2003;
											break;
									}
									break;

								case 6:
									{
										switch (minorVersion)
										{
											case 0:

												switch (productType)
												{
													case 1:
														OSName = "Microsoft Windows Vista";
														_InternalOSVersionType = OSVersionType.Microsoft_Windows_Vista;
														break;

													case 3:
														OSName = "Microsoft Windows Server 2008";
														_InternalOSVersionType = OSVersionType.Microsoft_Windows_Server_2008;
														break;
												}
												break;

											case 1:

												switch (productType)
												{
													case 1:
														OSName = "Microsoft Windows 7";
														_InternalOSVersionType = OSVersionType.Microsoft_Windows_7;
														break;

													case 3:
														OSName = "Microsoft Windows Server 2008 R2";
														_InternalOSVersionType = OSVersionType.Microsoft_Windows_Server_2008_R2;
														break;
												}
												break;
										}
										break;
									}
							}
							break;
						}

					case PlatformID.WinCE:
						OSName = "Microsoft Windows CE";
						_InternalOSVersionType = OSVersionType.Microsoft_Windows_CE;
						break;

					case PlatformID.Unix:
						OSName = "Unix";
						_InternalOSVersionType = OSVersionType.Unix;
						break;

					case PlatformID.MacOSX:
						OSName = "MacOSX";
						_InternalOSVersionType = OSVersionType.MacOSX;
						break;

					case PlatformID.Xbox:
						OSName = "XBox 360";
						_InternalOSVersionType = OSVersionType.XBox_360;
						break;
				}
			}
		}

		private static String GetOSServicePack()
		{
			String result = "UnKnown";
			OSVERSIONINFOEX osVersionInfo = default(OSVERSIONINFOEX);
			osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
			if (GetVersionEx(ref osVersionInfo))
			{
				result = osVersionInfo.szCSDVersion;
			}
			return result;
		}

		#endregion

		#endregion
	}
}