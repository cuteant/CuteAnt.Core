using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CuteAnt
{
	/// <summary>常用工具类——验证类</summary>
	/// <remarks>此类已作废，不再使用！！！</remarks>
	[Obsolete("此类已作废，不再使用！！！")]
	public sealed class MatchHelper
	{
		/// <summary>字段串是否为Null或为""(空)</summary>
		/// <param name="str">校验的字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean StrIsNullOrEmpty(String str)
		{
			if (str == null || str.Trim() == String.Empty)
			{
				return true;
			}
			return false;
		}

		/// <summary>Determines whether the String is all white space. Empty String will return false.</summary>
		/// <param name="s">The String to test whether it is all white space.</param>
		/// <returns>
		/// 	<c>true</c> if the String is all white space; otherwise, <c>false</c>.
		/// </returns>
		public static Boolean IsWhiteSpace(String s)
		{
			if (s == null)
				throw new ArgumentNullException("s");
			if (s.Length == 0)
			{
				return false;
			}

			for (Int32 i = 0, len = s.Length; i < len; i++)
			{
				if (!Char.IsWhiteSpace(s[i]))
				{
					return false;
				}
			}
			return true;
		}

		#region -- 特殊格式 --

		#region - method IsEmail -

		/// <summary>检测是否符合email格式,需引用：using System.Text.RegularExpressions;</summary>
		/// <param name="strEmail">要判断的email字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsEmail(String strEmail)
		{
			if (StrIsNullOrEmpty(strEmail)) { return false; }
			return Regex.IsMatch(strEmail, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
		}

		#endregion

		#region - method IsURL -

		/// <summary>检测是否是正确的Url,需引用：using System.Text.RegularExpressions;</summary>
		/// <param name="strUrl">要验证的Url</param>
		/// <returns>判断结果</returns>
		public static Boolean IsURL(String strUrl)
		{
			if (StrIsNullOrEmpty(strUrl)) { return false; }
			return Regex.IsMatch(strUrl, @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|Int32|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
		}

		#endregion

		#region - method IsIP -

		/// <summary>是否为ip</summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static Boolean IsIP(String ip)
		{
			if (StrIsNullOrEmpty(ip)) { return false; }

			//^(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5])$
			return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
		}

		#endregion

		#region - method IsIPSect -

		public static Boolean IsIPSect(String ip)
		{
			if (StrIsNullOrEmpty(ip)) { return false; }
			return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){2}((2[0-4]\d|25[0-5]|[01]?\d\d?|\*)\.)(2[0-4]\d|25[0-5]|[01]?\d\d?|\*)$");
		}

		#endregion

		#region - method IsBase64String -

		/// <summary>判断是否为base64字符串</summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static Boolean IsBase64String(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }

			//A-Z, a-z, 0-9, +, /, =
			return Regex.IsMatch(str, @"[A-Za-z0-9\+\/\=]");
		}

		#endregion

		#region - method IsColorValue -

		/// <summary>检查颜色值是否为3/6位的合法颜色</summary>
		/// <param name="color">待检查的颜色</param>
		/// <returns></returns>
		public static Boolean IsColorValue(String color)
		{
			if (StrIsNullOrEmpty(color)) { return false; }
			color = color.Trim().Trim('#');
			if (color.Length != 3 && color.Length != 6)
			{
				return false;
			}

			//不包含0-9  a-f以外的字符
			if (!Regex.IsMatch(color, "[^0-9a-f]", RegexOptions.IgnoreCase))
			{
				return true;
			}
			return false;
		}

		#endregion

		#region - method IsChineseID -

		//是否合法的中国身份证号码
		public static Boolean IsChineseID(String cid)
		{
			if (cid.Length == 15)
			{
				cid = CidUpdate(cid);
			}
			if (cid.Length == 18)
			{
				String strResult = CheckCidInfo(cid);
				if (strResult == "非法格式" || strResult == "非法地区" || strResult == "非法生日" || strResult == "非法证号")
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		//中国身份证号码验证
		private static String CheckCidInfo(String cid)
		{
			String[] aCity = new String[] { null, null, null, null, null, null, null, null, null, null, null, "北京", "天津", "河北", "山西", "内蒙古", null, null, null, null, null, "辽宁", "吉林", "黑龙江", null, null, null, null, null, null, null, "上海", "江苏", "浙江", "安微", "福建", "江西", "山东", null, null, null, "河南", "湖北", "湖南", "广东", "广西", "海南", null, null, null, "重庆", "四川", "贵州", "云南", "西藏", null, null, null, null, null, null, "陕西", "甘肃", "青海", "宁夏", "新疆", null, null, null, null, null, "台湾", null, null, null, null, null, null, null, null, null, "香港", "澳门", null, null, null, null, null, null, null, null, "国外" };

			Double iSum = 0;
			String info = String.Empty;

			//Regex rg = new Regex(@"^\d{17}(\d|x)$");
			Regex rg = new Regex(@"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$");
			Match mc = rg.Match(cid);
			if (!mc.Success)
			{
				return "非法格式";
			}
			cid = cid.ToLower();
			cid = cid.Replace("x", "a");
			if (aCity[Int32.Parse(cid.Substring(0, 2))] == null)
			{
				return "非法地区";
			}

			try
			{
				DateTime.Parse(cid.Substring(6, 4) + " - " + cid.Substring(10, 2) + " - " + cid.Substring(12, 2));
			}
			catch
			{
				return "非法生日";
			}

			for (Int32 i = 17; i >= 0; i--)
			{
				iSum += (Math.Pow(2, i) % 11) * Int32.Parse(cid[17 - i].ToString(), NumberStyles.HexNumber);
			}
			if (iSum % 11 != 1)
			{
				return ("非法证号");
			}
			else
			{
				return (aCity[Int32.Parse(cid.Substring(0, 2))] + "," + cid.Substring(6, 4) + "-" + cid.Substring(10, 2) + "-" + cid.Substring(12, 2) + "," + (Int32.Parse(cid.Substring(16, 1)) % 2 == 1 ? "男" : "女"));
			}
		}

		//身份证号码15升级为18位
		private static String CidUpdate(String ShortCid)
		{
			Char[] strJiaoYan = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };
			Int32[] intQuan = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1 };
			String strTemp;
			Int32 intTemp = 0;
			strTemp = ShortCid.Substring(0, 6) + "19" + ShortCid.Substring(6);

			for (Int32 i = 0, len = strTemp.Length; i <= len - 1; i++)
			{
				intTemp += Int32.Parse(strTemp.Substring(i, 1)) * intQuan[i];
			}
			intTemp = intTemp % 11;
			return strTemp + strJiaoYan[intTemp];
		}

		#endregion

		#region - method IsChinaPhone -

		/// <summary>是否中国电话号码类型正确格式为："XXX-XXXXXXX"、"XXXX-XXXXXXXX"、"XXX-XXXXXXX"、"XXX-XXXXXXXX"、"XXXXXXX"和"XXXXXXXX"</summary>
		/// <param name="tel">要验证的电话号码字符串</param>
		/// <returns>Boolean</returns>
		public static Boolean IsChinaPhone(String tel)
		{
			if (StrIsNullOrEmpty(tel)) { return false; }
			Regex regex = new Regex(@"^(0\d{2})-(\d{8})$)|(^(0\d{3})-(\d{7})$)|(^(0\d{2})-(\d{8})-(\d+)$)|(^(0\d{3})-(\d{7})-(\d+)$");
			return regex.IsMatch(tel.Trim());
		}

		#endregion

		#region - method IsChinesePostalCode -

		/// <summary>是否中国邮政编码（6位数字 /d{6}）</summary>
		/// <param name="code">要检测的邮政编码字符串</param>
		/// <returns>Boolean</returns>
		public static Boolean IsChinesePostalCode(String code)
		{
			if (StrIsNullOrEmpty(code)) { return false; }
			return Regex.IsMatch(code, @"[1-9]\d{5}(?!\d)");
		}

		#endregion

		#region - method IsChineseMobile -

		/// <summary>是否中国移动电话号码（13开头的总11位数字 13/d{9}）</summary>
		/// <param name="phone">要检测的邮政编码字符串</param>
		/// <returns>Boolean</returns>
		public static Boolean IsChineseMobile(String phone)
		{
			if (StrIsNullOrEmpty(phone)) { return false; }

			//return Regex.IsMatch(phone, @"(86)*0*13\d{9}");
			return Regex.IsMatch(phone, @"^1[358]\d{9}$");
		}

		#endregion

		#region - method IsChineseWord -

		/// <summary>是否中文字符（[/u4e00-/u9fa5]）</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsChineseWord(String word)
		{
			if (StrIsNullOrEmpty(word)) { return false; }
			return Regex.IsMatch(word, @"^[\u4e00-\u9fa5]{0,}$");
		}

		#endregion

		#region - method IsWideWord -

		/// <summary>是否全角字符（[^/x00-/xff]）：包括汉字在内</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsWideWord(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return Regex.IsMatch(str, @"[^/x00-/xff]");
		}

		#endregion

		#region - method IsNarrowWord -

		/// <summary>是否半角字符（[^/x00-/xff]）：包括汉字在内</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsNarrowWord(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return Regex.IsMatch(str, @"[/x00-/xff]");
		}

		#endregion

		#region - method IsSafeSqlString -

		/// <summary>检测是否有Sql危险字符,,需引用：using System.Text.RegularExpressions;</summary>
		/// <param name="str">要判断字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsSafeSqlString(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			if (Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']") || Regex.IsMatch(str, @"select|insert|delete|from|count(|drop table|update|truncate|asc(|mid(|Char(|xp_cmdshell|exec master|netlocalgroup administrators|:|net user|""|or|and"))
			{
				return true;
			}
			else
			{
				return false;
			}

			//return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
		}

		#endregion

		#region - method IsSafeUserInfoString -

		/// <summary>检测是否有危险的可能用于链接的字符串</summary>
		/// <param name="str">要判断字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsSafeUserInfoString(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return !Regex.IsMatch(str, @"^\s*$|^c:\\con\\con$|[%,\*" + "\"" + @"\s\t\<\>\&]|游客|^Guest");
		}

		#endregion

		#region - method IsStartWithNumber -

		/// <summary>检测字符串是否以数字开头</summary>
		/// <param name="str">要判断字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsStartWithNumber(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return Char.IsDigit(str[0]);
		}

		#endregion

		#endregion

		#region -- 字符数字 --

		#region - method IsOnlyNumber -

		/// <summary>是否是数字（0到9的数字[/d]+）：不包括符号"."和"-"</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsOnlyNumber(String num)
		{
			if (StrIsNullOrEmpty(num)) { return false; }
			return Regex.IsMatch(num, @"^\d+$");
		}

		#endregion

		#region - method IsUpperCaseChar -

		/// <summary>只能输入由26个大写英文字母组成的字符串</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsUpperCaseChar(String num)
		{
			if (StrIsNullOrEmpty(num)) { return false; }
			return Regex.IsMatch(num, "^[A-Z]+$");
		}

		public static Boolean IsUpperCaseChar(Char s)
		{
			return IsUpperCaseChar(s.ToString());
		}

		#endregion

		#region - method IsLowerCaseChar -

		/// <summary>只能输入由26个小写英文字母组成的字符串</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsLowerCaseChar(String num)
		{
			if (StrIsNullOrEmpty(num)) { return false; }
			return Regex.IsMatch(num, "^[a-z]+$");
		}

		public static Boolean IsLowerCaseChar(Char s)
		{
			return IsLowerCaseChar(s.ToString());
		}

		#endregion

		#region - method IsCharAndNumber -

		/// <summary>字符串是否是字符和数字</summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static Boolean IsCharAndNumber(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return Regex.IsMatch(str, @"^[a-zA-Z0-9]+$");
		}

		#endregion

		#region - method IsUserPWD -

		/// <summary>验证用户密码，正确格式为：以字母开头，长度在6~18之间，只能包含字符、数字和下划线。</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsUserPWD(String word)
		{
			if (StrIsNullOrEmpty(word)) { return false; }
			return Regex.IsMatch(word, @"^[a-zA-Z]\w{5,17}$");
		}

		#endregion

		#region - method IsStringModel -

		/// <summary>是否只包含数字，英文和下划线（[/w]+）</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsStringModel_01(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return Regex.IsMatch(str, @"[/w]+");
		}

		/// <summary>是否大写首字母的英文字母（[A-Z][a-z]+）</summary>
		/// <returns>Boolean</returns>
		public static Boolean IsStringModel_02(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }
			return Regex.IsMatch(str, @"[A-Z][a-z]+");
		}

		#endregion

		#endregion

		#region -- 数值类型 --

		#region - method IsSByte -

		/// <summary>判断字符串是否为SByte类型（8 位的有符号整数）： -128 到 +127 之间的整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsSByte(String str)
		{
			SByte v;
			Boolean bReturn = IsSByte(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为SByte类型（8 位的有符号整数）： -128 到 +127 之间的整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的SByte类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsSByte(String str, out SByte result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = SByte.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsByte -

		/// <summary>判断字符串是否为Byte类型（8 位的无符号整数）： 0 和 255 之间的无符号整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsByte(String str)
		{
			Byte v;
			Boolean bReturn = IsByte(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Byte类型（8 位的无符号整数）： 0 和 255 之间的无符号整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Byte类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsByte(String str, out Byte result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Byte.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsShort -

		/// <summary>判断字符串是否为Int16类型（16 位的有符号整数）： -32768 到 +32767 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsShort(String str)
		{
			Int16 v;
			Boolean bReturn = IsShort(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Int16类型（16 位的有符号整数）： -32768 到 +32767 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Int16类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsShort(String str, out Int16 result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Int16.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsUShort -

		/// <summary>判断字符串是否为UInt16类型（16 位的无符号整数）： 0 到 65535 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsUShort(String str)
		{
			UInt16 v;
			Boolean bReturn = IsUShort(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为UInt16类型（16 位的无符号整数）： 0 到 65535 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的UShort类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsUShort(String str, out UInt16 result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = UInt16.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsInt -

		/// <summary>判断字符串是否为Int32类型（32 位的有符号整数）：-2,147,483,648 到 +2,147,483,647 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsInt(String str)
		{
			Int32 v;
			Boolean bReturn = IsInt(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Int32类型（32 位的有符号整数）：-2,147,483,648 到 +2,147,483,647 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Int32类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsInt(String str, out Int32 result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Int32.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsUInt -

		/// <summary>判断字符串是否为UInt32类型（32 位的无符号整数）：0 到 4,294,967,295 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsUInt(String str)
		{
			UInt32 v;
			Boolean bReturn = IsUInt(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为UInt32类型（32 位的无符号整数）：0 到 4,294,967,295 之间的有符号整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的UInt32类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsUInt(String str, out UInt32 result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = UInt32.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsLong -

		/// <summary>判断字符串是否为Int64类型（64 位的有符号整数）： -9,223,372,036,854,775,808 到 +9,223,372,036,854,775,807 之间的整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsLong(String str)
		{
			Int64 v;
			Boolean bReturn = IsLong(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Int64类型（64 位的有符号整数）： -9,223,372,036,854,775,808 到 +9,223,372,036,854,775,807 之间的整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Int64类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsLong(String str, out Int64 result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Int64.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsULong -

		/// <summary>判断字符串是否为UInt64类型（64 位的无符号整数）： 0 到 18,446,744,073,709,551,615 之间的整数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsULong(String str)
		{
			UInt64 v;
			Boolean bReturn = IsULong(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为UInt64类型（64 位的无符号整数）： 0 到 18,446,744,073,709,551,615 之间的整数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的UInt64类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsULong(String str, out UInt64 result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = UInt64.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsFloat -

		/// <summary>判断字符串是否为Single类型（单精度（32 位）浮点数字）： -3.402823e38 和 +3.402823e38 之间的单精度 32 位数字</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsFloat(String str)
		{
			Single v;
			Boolean bReturn = IsFloat(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Single类型（单精度（32 位）浮点数字）： -3.402823e38 和 +3.402823e38 之间的单精度 32 位数字</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Single类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsFloat(String str, out Single result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Single.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsDouble -

		/// <summary>判断字符串是否为Double类型（单精度（64 位）浮点数字）： -1.79769313486232e308 和 +1.79769313486232e308 之间的双精度 64 位数字</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsDouble(String str)
		{
			Double v;
			Boolean bReturn = IsDouble(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Double类型（单精度（64 位）浮点数字）： -1.79769313486232e308 和 +1.79769313486232e308 之间的双精度 64 位数字</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Double类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsDouble(String str, out Double result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Double.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsDecimal -

		/// <summary>判断字符串是否为Decimal类型（96 位十进制值）：从正 79,228,162,514,264,337,593,543,950,335 到负 79,228,162,514,264,337,593,543,950,335 之间的十进制数</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsDecimal(String str)
		{
			Decimal v;
			Boolean bReturn = IsDecimal(str, out v);
			return bReturn;
		}

		/// <summary>判断字符串是否为Decimal类型（96 位十进制值）：从正 79,228,162,514,264,337,593,543,950,335 到负 79,228,162,514,264,337,593,543,950,335 之间的十进制数</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的Decimal类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsDecimal(String str, out Decimal result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = 0;
				return false;
			}

			try
			{
				result = Decimal.Parse(str);
			}
			catch
			{
				result = 0;
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsBoolen -

		/// <summary>字符串能否转为Boolen类型</summary>
		/// <param name="str">字符串,一般为True或False</param>
		/// <returns>判断结果</returns>
		public static Boolean IsBoolen(String str)
		{
			if (StrIsNullOrEmpty(str)) { return false; }

			try
			{
				Boolean result = Boolean.Parse(str);
			}
			catch
			{
				return false;
			}
			return true;
		}

		#endregion

		#region - method IsChar -

		/// <summary>字符串能否转为Char类型（Unicode（16 位）字符）：该 16 位数字的值范围为从十六进制值 0x0000 到 0xFFFF</summary>
		/// <param name="str">字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsChar(String str)
		{
			Char v;
			Boolean bReturn = IsChar(str, out v);
			return bReturn;
		}

		/// <summary>字符串能否转为Char类型（Unicode（16 位）字符）：该 16 位数字的值范围为从十六进制值 0x0000 到 0xFFFF</summary>
		/// <param name="str">字符串</param>
		/// <param name="result">转换成功后的UInt32类型数值</param>
		/// <returns>判断结果</returns>
		public static Boolean IsChar(String str, out Char result)
		{
			if (StrIsNullOrEmpty(str))
			{
				result = (Char)0;
				return false;
			}

			try
			{
				result = Char.Parse(str);
			}
			catch
			{
				result = (Char)0;
				return false;
			}
			return true;
		}

		#endregion

		#endregion

		#region -- 日期类型 --

		#region - method IsTime -

		/// <summary>是否为时间格式</summary>
		/// <returns>是则返加true 不是则返回 false</returns>
		public static Boolean IsTime(String timeval)
		{
			return Regex.IsMatch(timeval, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
		}

		#endregion

		#region - method IsDate -

		/// <summary>是否为日期格式：2009-09-03</summary>
		/// <param name="DateStr">日期字符串</param>
		/// <returns></returns>
		public static Boolean IsDate(String DateStr)
		{
			Boolean flag = false;

			try
			{
				DateTime DaTi = Convert.ToDateTime(DateStr);
				if (DaTi.ToString("yyyy-MM-dd") == DateStr)
				{
					flag = true;
				}
			}
			catch (Exception) { }
			return flag;
		}

		#endregion

		#region - method IsDateTime -

		/// <summary>是否为日期加时间格式：2009-09-03 12:12:12</summary>
		/// <param name="DateTimeStr">日期加时间字符串</param>
		/// <returns></returns>
		public static Boolean IsDateTime(String DateTimeStr)
		{
			Boolean flag = false;

			try
			{
				DateTime DaTi = Convert.ToDateTime(DateTimeStr);
				if (DaTi.ToString("yyyy-MM-dd HH:mm:ss") == DateTimeStr) { flag = true; }
			}
			catch (Exception) { }
			return flag;
		}

		#endregion

		/// <summary>判断字符串是否是yy-mm-dd字符串</summary>
		/// <param name="str">待判断字符串</param>
		/// <returns>判断结果</returns>
		public static Boolean IsDateString(String str)
		{
			return Regex.IsMatch(str, @"(\d{4})-(\d{1,2})-(\d{1,2})");
		}

		#endregion
	}
}