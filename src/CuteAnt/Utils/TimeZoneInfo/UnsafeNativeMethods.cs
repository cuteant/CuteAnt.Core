#if NET_2_0
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CuteAnt.Utils
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		internal static extern Int32 GetDynamicTimeZoneInformation(out NativeMethods.DynamicTimeZoneInformation lpDynamicTimeZoneInformation);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		internal static extern Int32 GetTimeZoneInformation(out NativeMethods.TimeZoneInformation lpTimeZoneInformation);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		internal static extern Boolean GetFileMUIPath(Int32 flags, [MarshalAs(UnmanagedType.LPWStr)] String filePath, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder language, ref Int32 languageLength, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder fileMuiPath, ref Int32 fileMuiPathLength, ref Int64 enumerator);

		[SecurityCritical, DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern SafeLibraryHandle LoadLibraryEx(String libFilename, IntPtr reserved, Int32 flags);

		[SecurityCritical, DllImport("user32.dll", EntryPoint = "LoadStringW", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
		internal static extern Int32 LoadString(SafeLibraryHandle handle, Int32 id, StringBuilder buffer, Int32 bufferLength);
	}
}
#endif