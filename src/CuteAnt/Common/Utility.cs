/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Globalization;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt
{
	/// <summary>工具类</summary>
	/// <remarks>
	/// 采用对象容器架构，允许外部重载工具类的各种实现
	/// </remarks>
	public static class Utility
	{
		//static Utility()
		//{
		//    _Convert = ObjectContainer.Current.AutoRegister<DefaultConvert, DefaultConvert>().Resolve<DefaultConvert>();
		//}

		#region 类型转换

		private static DefaultConvert _Convert = new DefaultConvert();

		/// <summary>类型转换提供者</summary>
		public static DefaultConvert Convert
		{
			get { return _Convert; }
			set { _Convert = value; }
		}

		///// <summary>转为整数</summary>
		///// <param name="value">待转换对象</param>
		///// <returns></returns>
		//public static Int32 ToInt32(this Object value) { return _Convert.ToInt32(value, 0); }

		/// <summary>转为32位整数</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Int32 ToInt(this Object value, Int32 defaultValue = 0)
		{
			return _Convert.ToInt(value, defaultValue);
		}

		/// <summary>转为16位整数</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Int32 ToInt16(this Object value, Int16 defaultValue = 0)
		{
			return _Convert.ToInt16(value, defaultValue);
		}

		/// <summary>转为64位整数</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Int64 ToInt64(this Object value, Int64 defaultValue = 0L)
		{
			return _Convert.ToInt64(value, defaultValue);
		}

		/// <summary>转为数值</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Decimal ToDecimal(this Object value, Decimal defaultValue = 0.0M)
		{
			return _Convert.ToDecimal(value, defaultValue);
		}

		/// <summary>转为单精度浮点数值</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Single ToSingle(this Object value, Single defaultValue = 0.0F)
		{
			return _Convert.ToSingle(value, defaultValue);
		}

		/// <summary>转为双精度浮点数值</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Double ToDouble(this Object value, Double defaultValue = 0.0D)
		{
			return _Convert.ToDouble(value, defaultValue);
		}

		///// <summary>转为布尔型</summary>
		///// <param name="value">待转换对象</param>
		///// <returns></returns>
		//public static Boolean ToBoolean(this Object value) { return _Convert.ToBoolean(value, false); }

		/// <summary>转为布尔型。支持大小写True/False、0和非零</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Boolean ToBoolean(this Object value, Boolean defaultValue = false)
		{
			return _Convert.ToBoolean(value, defaultValue);
		}

		/// <summary>转为时间日期</summary>
		/// <param name="value">待转换对象</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static DateTime ToDateTime(this Object value)
		{
			return _Convert.ToDateTime(value, DateTime.MinValue);
		}

		/// <summary>转为时间日期</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static DateTime ToDateTime(this Object value, DateTime defaultValue)
		{
			return _Convert.ToDateTime(value, defaultValue);
		}

		/// <summary>转为GUID</summary>
		/// <param name="value">待转换对象</param>
		/// <returns>转换失败，返回 Guid.Empty 对象</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Guid ToGuid(this Object value)
		{
			return _Convert.ToGuid(value, Guid.Empty);
		}

		/// <summary>转为GUID</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns>转换失败，返回 defaultValue 对象</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Guid ToGuid(this Object value, Guid defaultValue)
		{
			return _Convert.ToGuid(value, defaultValue);
		}

		/// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串</summary>
		/// <param name="value"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String ToFullString(this DateTime value)
		{
			return _Convert.ToFullString(value);
		}

		/// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串，支持指定最小时间的字符串</summary>
		/// <remarks>最常用的时间日期格式，可以无视各平台以及系统自定义的时间格式</remarks>
		/// <param name="value">待转换对象</param>
		/// <param name="emptyValue">字符串空值时（DateTime.MinValue）显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
		/// <returns></returns>
		public static String ToFullString(this DateTime value, String emptyValue = null) { return _Convert.ToFullString(value, emptyValue); }

		/// <summary>时间日期转为指定格式字符串</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="format">格式化字符串</param>
		/// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
		/// <returns></returns>
		public static String ToString(this DateTime value, String format, String emptyValue) { return _Convert.ToString(value, format, emptyValue); }

		#endregion
	}

	/// <summary>默认转换</summary>
	public class DefaultConvert
	{
		/// <summary>转为32位整数</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Int32 ToInt(Object value, Int32 defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (Int32.TryParse(str, out n)) { return n; }
				return defaultValue;
			}

			var buf = value as Byte[];
			if (buf != null)
			{
				if (buf == null || buf.Length < 1) { return defaultValue; }

				switch (buf.Length)
				{
					case 1:
						return buf[0];
					case 2:
						return BitConverter.ToInt16(buf, 0);
					case 3:
						return BitConverter.ToInt32(new Byte[] { buf[0], buf[1], buf[2], 0 }, 0);
					case 4:
						return BitConverter.ToInt32(buf, 0);
					default:
						break;
				}
			}

			//var tc = Type.GetTypeCode(value.GetType());
			//if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

			try
			{
				return Convert.ToInt32(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为16位整数</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Int16 ToInt16(Object value, Int16 defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (Int16.TryParse(str, out n)) { return n; }
				return defaultValue;
			}

			//var tc = Type.GetTypeCode(value.GetType());
			//if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

			try
			{
				return Convert.ToInt16(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为64位整数</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Int64 ToInt64(Object value, Int64 defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (Int64.TryParse(str, out n)) { return n; }
				return defaultValue;
			}

			//var tc = Type.GetTypeCode(value.GetType());
			//if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

			try
			{
				return Convert.ToInt64(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为数值</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Decimal ToDecimal(Object value, Decimal defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (Decimal.TryParse(str, out n)) { return n; }
				return defaultValue;
			}

			//var tc = Type.GetTypeCode(value.GetType());
			//if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

			try
			{
				return Convert.ToDecimal(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为单精度浮点数值</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Single ToSingle(Object value, Single defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (Single.TryParse(str, out n)) { return n; }
				return defaultValue;
			}

			//var tc = Type.GetTypeCode(value.GetType());
			//if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

			try
			{
				return Convert.ToSingle(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为双精度浮点数值</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Double ToDouble(Object value, Double defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (Double.TryParse(str, out n)) { return n; }
				return defaultValue;
			}
			var buf = value as Byte[];
			if (buf != null)
			{
				if (buf.Length < 1) { return defaultValue; }

				switch (buf.Length)
				{
					case 1:
						return buf[0];
					case 2:
						return BitConverter.ToInt16(buf, 0);
					case 3:
						return BitConverter.ToInt32(new Byte[] { buf[0], buf[1], buf[2], 0 }, 0);
					case 4:
						return BitConverter.ToInt32(buf, 0);
					default:
						// 凑够8字节
						if (buf.Length < 8)
						{
							var bts = new Byte[8];
							Buffer.BlockCopy(buf, 0, bts, 0, buf.Length);
							buf = bts;
						}
						return BitConverter.ToDouble(buf, 0);
				}
			}

			//var tc = Type.GetTypeCode(value.GetType());
			//if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

			try
			{
				return Convert.ToDouble(value);
			}
			catch { return defaultValue; }
		}

		//static readonly String[] trueStr = new String[] { "True", "Y", "Yes", "On" };
		//static readonly String[] falseStr = new String[] { "False", "N", "N", "Off" };

		/// <summary>转为布尔型。支持大小写True/False、0和非零</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Boolean ToBoolean(Object value, Boolean defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var b = defaultValue;
				if (Boolean.TryParse(str, out b)) { return b; }

				if (String.Equals(str, Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) { return true; }
				if (String.Equals(str, Boolean.FalseString, StringComparison.OrdinalIgnoreCase)) { return false; }

				// 特殊处理用数字0和1表示布尔型
				var n = 0;
				if (Int32.TryParse(str, out n)) { return n > 0; }

				return defaultValue;
			}

			try
			{
				return Convert.ToBoolean(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为时间日期</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual DateTime ToDateTime(Object value, DateTime defaultValue)
		{
			if (value == null) { return defaultValue; }

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				var n = defaultValue;
				if (DateTime.TryParse(str, out n)) { return n; }
				if (str.Contains("-") && DateTime.TryParseExact(str, "yyyy-M-d", null, DateTimeStyles.None, out n)) { return n; }
				if (str.Contains("/") && DateTime.TryParseExact(str, "yyyy/M/d", null, DateTimeStyles.None, out n)) { return n; }
				if (DateTime.TryParse(str, out n)) { return n; }
				return defaultValue;
			}

			try
			{
				return Convert.ToDateTime(value);
			}
			catch { return defaultValue; }
		}

		/// <summary>转为GUID</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="defaultValue">默认值。待转换对象无效时使用</param>
		/// <returns></returns>
		public virtual Guid ToGuid(Object value, Guid defaultValue)
		{
			if (value == null) { return defaultValue; }

			if (value.GetType() == typeof(Guid)) { return (Guid)value; }

			#region 字符串

			// 特殊处理字符串，也是最常见的
			var str = value as String;
			if (str != null)
			{
				if (str.Length == 0) { return defaultValue; }
				str = ToDBC(str).Trim();
				if (str.IsNullOrWhiteSpace()) { return defaultValue; }

				Guid guid = defaultValue;
				Boolean success = false;
#if NET_3_5_GREATER
				success = Guid.TryParse(str, out guid);
#else
				try
				{
					guid = new Guid(str);
					success = true;
				}
				catch { }
#endif
				return success ? guid : defaultValue;
			}

			#endregion

			#region 字节数组

			var bs = value as Byte[];
			if (bs != null)
			{
				return bs.Length == 16 ? new Guid(bs) : defaultValue;
			}

			#endregion

			return defaultValue;
		}

		/// <summary>全角为半角</summary>
		/// <remarks>全角半角的关系是相差0xFEE0</remarks>
		/// <param name="str"></param>
		/// <returns></returns>
		private String ToDBC(String str)
		{
			var ch = str.ToCharArray();
			for (int i = 0; i < ch.Length; i++)
			{
				// 全角空格
				if (ch[i] == 0x3000)
				{
					ch[i] = (char)0x20;
				}
				else if (ch[i] > 0xFF00 && ch[i] < 0xFF5F)
				{
					ch[i] = (char)(ch[i] - 0xFEE0);
				}
			}
			return new String(ch);
		}

		/// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
		/// <returns></returns>
		public virtual String ToFullString(DateTime value, String emptyValue = null)
		{
			if (emptyValue != null && value <= DateTime.MinValue) return emptyValue;

			return value.ToString("yyyy-MM-dd HH:mm:ss");
		}

		/// <summary>时间日期转为指定格式字符串</summary>
		/// <param name="value">待转换对象</param>
		/// <param name="format">格式化字符串</param>
		/// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
		/// <returns></returns>
		public virtual String ToString(DateTime value, String format, String emptyValue)
		{
			if (emptyValue != null && value <= DateTime.MinValue) return emptyValue;

			return value.ToString(format ?? "yyyy-MM-dd HH:mm:ss");
		}
	}
}