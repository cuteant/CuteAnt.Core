using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CuteAnt.OrmLite
{
	/// <summary>日期时间类型字段格式化方式</summary>
	public enum DateTimeFieldFormatType
	{
		/// <summary>格式化为日期字符串，格式：YYYY-MM-DD</summary>
		[Description("格式化为日期字符串，格式：YYYY-MM-DD")]
		Date,

		/// <summary>格式化为时间字符串，格式：HH:MM:SS</summary>
		[Description("格式化为时间字符串，格式：HH:MM:SS")]
		Time,

		/// <summary>格式化为时间字符串，格式：HH:MM:SS.SSS</summary>
		[Description("格式化为时间字符串，格式：HH:MM:SS.SSS")]
		TimeWithMicroSeconds,

		/// <summary>格式化为日期时间字符串，格式：YYYY-MM-DD HH:MM:SS</summary>
		[Description("格式化为日期时间字符串，格式：YYYY-MM-DD HH:MM:SS")]
		DateTime,

		/// <summary>格式化为日期时间字符串，格式：YYYY-MM-DD HH:MM:SS.SSS</summary>
		[Description("格式化为日期时间字符串，格式：YYYY-MM-DD HH:MM:SS.SSS")]
		DateTimeWithMicroSeconds,

		/// <summary>格式化为日期时间字符串，格式：YYYY-MM-DD HH:MM</summary>
		[Description("格式化为日期时间字符串，格式：YYYY-MM-DD HH:MM")]
		DateTimeWithoutSeconds,

		/// <summary>格式化为月份字符串，格式：YYYY-MM</summary>
		[Description("格式化为月份字符串，格式：YYYY-MM")]
		YearAndMonth,

		/// <summary>格式化为年份字符串，格式：YYYY</summary>
		[Description("格式化为年份字符串，格式：YYYY")]
		Year,
	}
}
