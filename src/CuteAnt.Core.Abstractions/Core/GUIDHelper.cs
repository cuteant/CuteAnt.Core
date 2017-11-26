using System;
using System.Runtime.CompilerServices;

namespace CuteAnt
{
  /// <summary>常用工具类——GUID相关类</summary>
  public static class GuidHelper
  {
    #region -- 扩展 --

    /// <summary>返回此 GUID 实例值的字符串表示形式。
    /// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 8 个、4 个、4 个、4 个和 12 个位为一组合并而成。
    /// 例如，返回值可以是“382c74c3721d4f3480e557657b6cbc27”。
    /// </para></summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [MethodImpl(InlineMethod.Value)]
    public static String To32Digits(this Guid guid)
    {
      return guid.ToString("N");
    }

    ///// <summary>返回此 GUID 实例值的字符串表示形式。
    ///// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 12 个和 8 个、4 个、4 个、4 个位为一组并由连字符分隔开。
    ///// 例如，返回值可以是“57657b6cbc27-382c74c3-721d-4f34-80e5”。
    ///// </para></summary>
    ///// <param name="guid"></param>
    ///// <returns></returns>
    //public static String ToComb(this Guid guid)
    //{
    //	return ConvertGuidToComb(guid.ToByteArray(), true);
    //}

    ///// <summary>返回此 GUID 实例值的字符串表示形式。
    ///// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 12 个和 8 个、4 个、4 个、4 个位为一组合并而成。
    ///// 例如，返回值可以是“382c74c3721d4f3480e557657b6cbc27”。
    ///// </para></summary>
    ///// <param name="guid"></param>
    ///// <returns></returns>
    //public static String ToComb32Digits(this Guid guid)
    //{
    //	return ConvertGuidToComb(guid.ToByteArray(), false);
    //}

    #endregion

    #region -- GUID --

    /// <summary>生成默认GUID格式字符串，
    /// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 8 个、4 个、4 个、4 个和 12 个位为一组并由连字符分隔开。
    /// 例如，返回值可以是“382c74c3-721d-4f34-80e5-57657b6cbc27”。
    /// </para></summary>
    /// <returns>新生成的GUID的字符串</returns>
    [MethodImpl(InlineMethod.Value)]
    public static String GenerateGuid()
    {
      return Guid.NewGuid().ToString("D");
    }

    /// <summary>生成没有连字符分隔的GUID字符串，
    /// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 8 个、4 个、4 个、4 个和 12 个位为一组合并而成。
    /// 例如，返回值可以是“382c74c3721d4f3480e557657b6cbc27”。
    /// </para></summary>
    /// <returns>新生成的GUID的字符串</returns>
    [MethodImpl(InlineMethod.Value)]
    public static String GenerateGuid32()
    {
      return Guid.NewGuid().ToString("N");
    }

    #endregion

    #region -- COMB --

    //		private static readonly DateTime _CombBaseDate = new DateTime(1900, 1, 1);

    //		/// <summary>返回 GUID 用于数据库操作，特定的时间代码可以提高检索效率</summary>
    //		/// <remarks>From：NHibernate.Id.GuidCombGenerator</remarks>
    //		/// <returns>COMB (GUID 与时间混合型) 类型 GUID 数据</returns>
    //		public static Guid NewComb()
    //		{
    //			return new Guid(GenerateCombByteArray());
    //		}

    //		/// <summary>生成默认 COMB 类型 GUID格式字符串，
    //		/// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 12 个和 8 个、4 个、4 个、4 个位为一组并由连字符分隔开。
    //		/// 例如，返回值可以是“57657b6cbc27-382c74c3-721d-4f34-80e5”。
    //		/// </para></summary>
    //		/// <returns>新生成的COMB的字符串</returns>
    //		public static String GenerateComb()
    //		{
    //			return ConvertGuidToComb(GenerateCombByteArray(), true);
    //		}

    //		/// <summary>生成没有连字符分隔的 COMB 类型 GUID格式字符串，
    //		/// <para>其中 GUID 的值表示为一系列小写的十六进制位，这些十六进制位分别以 12 个和 8 个、4 个、4 个、4 个位为一组合并而成。
    //		/// 例如，返回值可以是“57657b6cbc27382c74c3721d4f3480e5”。
    //		/// </para></summary>
    //		/// <returns>新生成的COMB的字符串</returns>
    //		public static String GenerateComb32()
    //		{
    //			return ConvertGuidToComb(GenerateCombByteArray(), false);
    //		}

    //		internal static Byte[] GenerateCombByteArray()
    //		{
    //			var guidArray = Guid.NewGuid().ToByteArray();

    //			var now = DateTime.Now;

    //			// Get the days and milliseconds which will be used to build the byte string 
    //			var days = new TimeSpan(now.Ticks - _CombBaseDate.Ticks);
    //			var msecs = now.TimeOfDay;

    //			// Convert to a byte array 
    //			// Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
    //			var daysArray = BitConverter.GetBytes(days.Days);
    //			//var msecsArray = BitConverter.GetBytes((Int64)(msecs.TotalMilliseconds / 3.333333));
    //			var msecsArray = BitConverter.GetBytes((Int64)msecs.TotalMilliseconds);

    //			// Reverse the bytes to match SQL Servers ordering 
    //			Array.Reverse(daysArray);
    //			Array.Reverse(msecsArray);

    //			// Copy the bytes into the guid 
    //			Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
    //			Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

    //			return guidArray;
    //		}

    //		internal static String ConvertGuidToComb(Byte[] guidArray, Boolean dash)
    //		{
    //			var offset = 0;
    //			var strLength = dash ? 36 : 32;
    //			var guidChars = new Char[strLength];

    //			#region MS GUID类内部代码

    //			//g[0] = (byte)(_a);
    //			//g[1] = (byte)(_a >> 8);
    //			//g[2] = (byte)(_a >> 16);
    //			//g[3] = (byte)(_a >> 24);
    //			//g[4] = (byte)(_b);
    //			//g[5] = (byte)(_b >> 8);
    //			//g[6] = (byte)(_c);
    //			//g[7] = (byte)(_c >> 8);
    //			//g[8] = _d;
    //			//g[9] = _e;
    //			//g[10] = _f;
    //			//g[11] = _g;
    //			//g[12] = _h;
    //			//g[13] = _i;
    //			//g[14] = _j;
    //			//g[15] = _k;
    //			//// [{|(]dddddddd[-]dddd[-]dddd[-]dddd[-]dddddddddddd[}|)]
    //			//offset = HexsToChars(guidChars, offset, _a >> 24, _a >> 16);
    //			//offset = HexsToChars(guidChars, offset, _a >> 8, _a);
    //			//if (dash) guidChars[offset++] = '-';
    //			//offset = HexsToChars(guidChars, offset, _b >> 8, _b);
    //			//if (dash) guidChars[offset++] = '-';
    //			//offset = HexsToChars(guidChars, offset, _c >> 8, _c);
    //			//if (dash) guidChars[offset++] = '-';
    //			//offset = HexsToChars(guidChars, offset, _d, _e);
    //			//if (dash) guidChars[offset++] = '-';
    //			//offset = HexsToChars(guidChars, offset, _f, _g);
    //			//offset = HexsToChars(guidChars, offset, _h, _i);
    //			//offset = HexsToChars(guidChars, offset, _j, _k);

    //			#endregion

    //			offset = HexsToChars(guidChars, offset, guidArray[10], guidArray[11]);
    //			offset = HexsToChars(guidChars, offset, guidArray[12], guidArray[13]);
    //			offset = HexsToChars(guidChars, offset, guidArray[14], guidArray[15]);
    //			if (dash) { guidChars[offset++] = '-'; }
    //			offset = HexsToChars(guidChars, offset, guidArray[3], guidArray[2]);
    //			offset = HexsToChars(guidChars, offset, guidArray[1], guidArray[0]);
    //			if (dash) { guidChars[offset++] = '-'; }
    //			offset = HexsToChars(guidChars, offset, guidArray[5], guidArray[4]);
    //			if (dash) { guidChars[offset++] = '-'; }
    //			offset = HexsToChars(guidChars, offset, guidArray[7], guidArray[6]);
    //			if (dash) { guidChars[offset++] = '-'; }
    //			offset = HexsToChars(guidChars, offset, guidArray[8], guidArray[9]);

    //			return new String(guidChars, 0, strLength);
    //		}

    //		private static Char HexToChar(Int32 a)
    //		{
    //			a = a & 0xf;
    //			return (Char)((a > 9) ? a - 10 + 0x61 : a + 0x30);
    //		}

    //		private static Int32 HexsToChars(Char[] guidChars, Int32 offset, Int32 a, Int32 b)
    //		{
    //			guidChars[offset++] = HexToChar(a >> 4);
    //			guidChars[offset++] = HexToChar(a);
    //			guidChars[offset++] = HexToChar(b >> 4);
    //			guidChars[offset++] = HexToChar(b);
    //			return offset;
    //		}

    //		/// <summary>将 COMB 类型 GUID 的字符串表示形式转换为等效的 Guid 结构</summary>
    //		/// <param name="comb">包含下面任一格式的 COMB 类型 GUID 的字符串（“d”表示忽略大小写的十六进制数字）： 
    //		/// <para>32 个连续的数字 dddddddddddddddddddddddddddddddd </para>
    //		/// <para>- 或 - </para>
    //		/// <para>12 和 8、4、4、4 位数字的分组，各组之间有连线符，dddddddddddd-dddddddd-dddd-dddd-dddd</para>
    //		/// </param>
    //		/// <param name="result">将包含已分析的值的结构。 如果此方法返回 true，result 包含有效的 Guid。 如果此方法返回 false，result 等于 Guid.Empty</param>
    //		/// <returns>如果分析操作成功，则为 true；否则为 false。</returns>
    //		public static Boolean TryParseComb(String comb, out Guid result)
    //		{
    //			ValidationHelper.ArgumentNullOrEmpty(comb, "comb");

    //			result = Guid.Empty;
    //			var dashpos = comb.IndexOf('-', 0);
    //			if (dashpos >= 0)
    //			{
    //				if (comb.Length != 36) { return false; }
    //				comb = "{1}-{0}".FormatWith(comb.Substring(0, dashpos), comb.Substring(dashpos + 1));
    //			}
    //			else
    //			{
    //				if (comb.Length != 32) { return false; }
    //				comb = "{1}{0}".FormatWith(comb.Substring(0, 12), comb.Substring(12));
    //			}
    //#if NET_3_5_GREATER
    //			return Guid.TryParse(comb, out result);
    //#else
    //			var isGuid = true;
    //			try
    //			{
    //				result = new Guid(comb);
    //			}
    //			catch { isGuid = false; }
    //			return isGuid;
    //#endif
    //		}

    #endregion

    #region -- 验证给定字符串是否是合法的Guid --

    /// <summary>验证给定字符串是否是合法的Guid</summary>
    /// <param name="value">要验证的字符串</param>
    /// <returns>true/false</returns>
    [MethodImpl(InlineMethod.Value)]
    public static Boolean IsGuid(String value)
    {
      return Guid.TryParse(value, out Guid guid);
    }

    #endregion

    #region -- 16位GUID --

    /// <summary>产生16位字符串：（例：49f949d735f5c79e）</summary>
    /// <returns>A String value...</returns>
    [MethodImpl(InlineMethod.Value)]
    public static String GenerateId16()
    {
      Int64 i = 1;
      foreach (Byte b in Guid.NewGuid().ToByteArray())
      {
        i *= ((Int32)b + 1);
      }
      return String.Format("{0:x16}", i - DateTime.Now.Ticks);
    }

    //产生Int64 类型：（例：4833055965497820814）
    [MethodImpl(InlineMethod.Value)]
    public static Int64 GenerateId()
    {
      Byte[] buffer = Guid.NewGuid().ToByteArray();
      return BitConverter.ToInt64(buffer, 0);
    }

    /// <summary>返加12位的UUID(GUID)</summary>
    /// <returns></returns>
    [MethodImpl(InlineMethod.Value)]
    public static String GenerateId12()
    {
      Byte[] buffer = Guid.NewGuid().ToByteArray();
      Int64 long_guid = BitConverter.ToInt64(buffer, 0);

      String _Value = Math.Abs(long_guid).ToString();

      Byte[] buf = new Byte[_Value.Length];
      Int32 p = 0;
      for (Int32 i = 0; i < _Value.Length; )
      {
        Byte ph = Convert.ToByte(_Value[i]);

        Int32 fix = 1;
        if ((i + 1) < _Value.Length)
        {
          Byte pl = Convert.ToByte(_Value[i + 1]);
          buf[p] = (Byte)((ph << 4) + pl);
          fix = 2;
        }
        else
        {
          buf[p] = (Byte)(ph);
        }

        if ((i + 3) < _Value.Length)
        {
          if (Convert.ToInt16(_Value.Substring(i, 3)) < 256)
          {
            buf[p] = Convert.ToByte(_Value.Substring(i, 3));
            fix = 3;
          }
        }
        p++;
        i = i + fix;
      }
      Byte[] buf2 = new Byte[p];
      for (Int32 i = 0; i < p; i++)
      {
        buf2[i] = buf[i];
      }
      String cRtn = Convert.ToBase64String(buf2);
      if (cRtn == null)
      {
        cRtn = "";
      }
      cRtn = cRtn.ToLower();
      cRtn = cRtn.Replace("/", "");
      cRtn = cRtn.Replace("+", "");
      cRtn = cRtn.Replace("=", "");
      if (cRtn.Length == 12)
      {
        return cRtn;
      }
      else
      {
        return GenerateId12();
      }
    }

    #endregion
  }
}