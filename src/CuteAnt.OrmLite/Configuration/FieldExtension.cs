/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Text;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>字段扩展</summary>
	public static class FieldExtension
	{
		#region -- 时间复杂运算 --

		#region - 天 -

		/// <summary>当天范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression Today(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd 00:00:00}", DateTime.Now));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd 00:00:00}", DateTime.Now));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date;
			return field.Between(date, date);
		}

		/// <summary>昨天范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression Yesterday(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd 00:00:00}", DateTime.Now.AddDays(-1)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd 00:00:00}", DateTime.Now.AddDays(-1)));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date.AddDays(-1);
			return field.Between(date, date);
		}

		/// <summary>明天范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression Tomorrow(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd 00:00:00}", DateTime.Now.AddDays(1)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd 00:00:00}", DateTime.Now.AddDays(1)));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date.AddDays(1);
			return field.Between(date, date);
		}

		/// <summary>过去天数范围</summary>
		/// <param name="field">字段</param>
		/// <param name="days"></param>
		/// <returns></returns>
		public static Expression LastDays(this FieldItem field, Int32 days)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(-days)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(-1)));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date;
			return field.Between(date.AddDays(-1 * days), date);
		}

		/// <summary>未来天数范围</summary>
		/// <param name="field">字段</param>
		/// <param name="days"></param>
		/// <returns></returns>
		public static Expression NextDays(this FieldItem field, Int32 days)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(1)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(days)));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date;
			return field.Between(date, date.AddDays(days));
		}

		#endregion

		#region - 周 -

		/// <summary>本周范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression ThisWeek(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(Convert.ToDouble((0 - Convert.ToInt16(DateTime.Now.DayOfWeek))))));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(Convert.ToDouble((6 - Convert.ToInt16(DateTime.Now.DayOfWeek))))));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date;
			var day = (Int32)date.DayOfWeek;
			return field.Between(date.AddDays(-1 * day), date.AddDays(6 - day));
		}

		/// <summary>上周范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression LastWeek(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(Convert.ToDouble((0 - Convert.ToInt16(DateTime.Now.DayOfWeek))) - 7)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(Convert.ToDouble((6 - Convert.ToInt16(DateTime.Now.DayOfWeek))) - 7)));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date;
			var day = (Int32)date.DayOfWeek;
			return field.Between(date.AddDays(-1 * day - 7), date.AddDays(6 - day - 7));
		}

		/// <summary>下周范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression NextWeek(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(Convert.ToDouble((0 - Convert.ToInt16(DateTime.Now.DayOfWeek))) + 7)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Now.AddDays(Convert.ToDouble((6 - Convert.ToInt16(DateTime.Now.DayOfWeek))) + 7)));
			//return field.Between(fromDateStart, fromDateEnd);

			var date = DateTime.Now.Date;
			var day = (Int32)date.DayOfWeek;
			return field.Between(date.AddDays(-1 * day + 7), date.AddDays(6 - day + 7));
		}

		#endregion

		#region - 月 -

		/// <summary>本月范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression ThisMonth(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(1))).AddDays(-1);
			//return field.Between(fromDateStart, fromDateEnd);

			var now = DateTime.Now;
			var month = new DateTime(now.Year, now.Month, 1);
			return field.Between(month, month.AddMonths(1).AddDays(-1));
		}

		/// <summary>上月范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression LastMonth(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(-1)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now)).AddDays(-1);
			//return field.Between(fromDateStart, fromDateEnd);

			var now = DateTime.Now;
			var month = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
			return field.Between(month, month.AddMonths(1).AddDays(-1));
		}

		/// <summary>下月范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression NextMonth(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(1)));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(2))).AddDays(-1);
			//return field.Between(fromDateStart, fromDateEnd);

			var now = DateTime.Now;
			var month = new DateTime(now.Year, now.Month, 1).AddMonths(1);
			return field.Between(month, month.AddMonths(1).AddDays(-1));
		}

		#endregion

		#region - 季度 -

		/// <summary>本季度范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression ThisQuarter(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(0 - ((DateTime.Now.Month - 1) % 3))));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Parse(DateTime.Now.AddMonths(3 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01")))).AddDays(-1);
			//return field.Between(fromDateStart, fromDateEnd);

			var now = DateTime.Now;
			var month = new DateTime(now.Year, now.Month - (now.Month - 1) % 3, 1);
			return field.Between(month, month.AddMonths(3).AddDays(-1));
		}

		/// <summary>上季度范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression LastQuarter(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(-3 - ((DateTime.Now.Month - 1) % 3))));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Parse(DateTime.Now.AddMonths(0 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01")))).AddDays(-1);
			//return field.Between(fromDateStart, fromDateEnd);

			var now = DateTime.Now;
			var month = new DateTime(now.Year, now.Month - (now.Month - 1) % 3, 1).AddMonths(-3);
			return field.Between(month, month.AddMonths(3).AddDays(-1));
		}

		/// <summary>下季度范围</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static Expression NextQuarter(this FieldItem field)
		{
			//var fromDateStart = DateTime.Parse(String.Format("{0:yyyy-MM}-01 00:00:00", DateTime.Now.AddMonths(3 - ((DateTime.Now.Month - 1) % 3))));
			//var fromDateEnd = DateTime.Parse(String.Format("{0:yyyy-MM-dd} 00:00:00", DateTime.Parse(DateTime.Now.AddMonths(6 - ((DateTime.Now.Month - 1) % 3)).ToString("yyyy-MM-01")))).AddDays(-1);
			//return field.Between(fromDateStart, fromDateEnd);

			var now = DateTime.Now;
			var month = new DateTime(now.Year, now.Month - (now.Month - 1) % 3, 1).AddMonths(3);
			return field.Between(month, month.AddMonths(3).AddDays(-1));
		}

		#endregion

		#endregion

		#region -- 字符串复杂运算 --

		/// <summary>包含所有关键字</summary>
		/// <param name="field"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		public static Expression ContainsAll(this FieldItem field, String keys)
		{
			var exp = new WhereExpression();
			if (String.IsNullOrEmpty(keys)) { return exp; }

			var ks = keys.SplitDefaultSeparator();
			if (ks.IsNullOrEmpty()) { return exp; }

			for (int i = 0; i < ks.Length; i++)
			{
				if (!ks[i].IsNullOrWhiteSpace()) { exp &= field.Contains(ks[i].Trim()); }
			}

			return exp;
		}

		/// <summary>包含任意关键字</summary>
		/// <param name="field"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		public static Expression ContainsAny(this FieldItem field, String keys)
		{
			var exp = new WhereExpression();
			if (String.IsNullOrEmpty(keys)) { return exp; }

			var ks = keys.SplitDefaultSeparator();
			if (ks.IsNullOrEmpty()) { return exp; }

			for (int i = 0; i < ks.Length; i++)
			{
				if (!ks[i].IsNullOrWhiteSpace()) { exp |= field.Contains(ks[i].Trim()); }
			}

			return exp;
		}

		#endregion

		#region -- 排序 --

		/// <summary>升序</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		public static ConcatExpression Asc(this FieldItem field) { return field == null ? null : new ConcatExpression(field.QuotedColumnName); }

		/// <summary>降序</summary>
		/// <param name="field">字段</param>
		/// <remarks>感谢 树懒（303409914）发现这里的错误</remarks>
		/// <returns></returns>
		public static ConcatExpression Desc(this FieldItem field) { return field == null ? null : new ConcatExpression(field.QuotedColumnName + ExpressionConstants.SPDesc); }

		/// <summary>通过参数置顶升序降序</summary>
		/// <param name="field">字段</param>
		/// <param name="isdesc">是否降序</param>
		/// <returns></returns>
		public static ConcatExpression Sort(this FieldItem field, Boolean isdesc) { return isdesc ? Desc(field) : Asc(field); }

		#endregion

		#region -- 分组选择 --

		/// <summary>分组。有条件的分组请使用WhereExpression.GroupBy</summary>
		/// <returns></returns>
		public static ConcatExpression GroupBy(this FieldItem field)
		{
			return field == null ? null : new ConcatExpression("Group By {0}".FormatWith(field.QuotedColumnName));
		}

		/// <summary>聚合</summary>
		/// <param name="field">字段</param>
		/// <param name="action"></param>
		/// <param name="newName"></param>
		/// <returns></returns>
		public static ConcatExpression Aggregate(this FieldItem field, String action, String newName)
		{
			if (field == null) return null;

			var name = field.QuotedColumnName;
			if (newName.IsNullOrWhiteSpace())
			{
				newName = name;
			}
			else
			{
				newName = field.Factory.Quoter.QuoteColumnName(newName);
			}

			return new ConcatExpression("{2}({0}) as {1}".FormatWith(name, newName, action));
		}

		/// <summary>数量</summary>
		/// <param name="field">字段</param>
		/// <param name="newName">聚合后as的新名称，默认空，表示跟前面字段名一致</param>
		/// <returns></returns>
		public static ConcatExpression Count(this FieldItem field, String newName = null) { return Aggregate(field, "Count", newName); }

		/// <summary>求和</summary>
		/// <param name="field">字段</param>
		/// <param name="newName">聚合后as的新名称，默认空，表示跟前面字段名一致</param>
		/// <returns></returns>
		public static ConcatExpression Sum(this FieldItem field, String newName = null) { return Aggregate(field, "Sum", newName); }

		/// <summary>最小值</summary>
		/// <param name="field">字段</param>
		/// <param name="newName">聚合后as的新名称，默认空，表示跟前面字段名一致</param>
		/// <returns></returns>
		public static ConcatExpression Min(this FieldItem field, String newName = null) { return Aggregate(field, "Min", newName); }

		/// <summary>最大值</summary>
		/// <param name="field">字段</param>
		/// <param name="newName">聚合后as的新名称，默认空，表示跟前面字段名一致</param>
		/// <returns></returns>
		public static ConcatExpression Max(this FieldItem field, String newName = null) { return Aggregate(field, "Max", newName); }

		#endregion

		#region -- 位运算选择 --

		/// <summary>位与</summary>
		/// <returns></returns>
		public static ConcatExpression BitwiseAND(this FieldItem field, Object value)
		{
			var fi = value as FieldItem;
			if (fi != null)
			{
				return new ConcatExpression("({0} & {1}) as {0}".FormatWith(field.QuotedColumnName, fi.QuotedColumnName));
			}
			else
			{
				return new ConcatExpression("({0} & {1}) as {0}".FormatWith(field.QuotedColumnName, field.Factory.QuoteValue(field, value)));
			}
		}

		/// <summary>位或</summary>
		/// <returns></returns>
		public static ConcatExpression BitwiseOR(this FieldItem field, Object value)
		{
			var fi = value as FieldItem;
			if (fi != null)
			{
				return new ConcatExpression("({0} | {1}) as {0}".FormatWith(field.QuotedColumnName, fi.QuotedColumnName));
			}
			else
			{
				return new ConcatExpression("({0} | {1}) as {0}".FormatWith(field.QuotedColumnName, field.Factory.QuoteValue(field, value)));
			}
		}

		/// <summary>位异或</summary>
		/// <returns></returns>
		public static ConcatExpression BitwiseXOR(this FieldItem field, Object value)
		{
			var fi = value as FieldItem;
			if (fi != null)
			{
				return new ConcatExpression("({0} ^ {1}) as {0}".FormatWith(field.QuotedColumnName, fi.QuotedColumnName));
			}
			else
			{
				return new ConcatExpression("({0} ^ {1}) as {0}".FormatWith(field.QuotedColumnName, field.Factory.QuoteValue(field, value)));
			}
		}

		#endregion

		#region -- 日期时间转换选择 --

		/// <summary>日期时间转换</summary>
		/// <param name="field">字段</param>
		/// <param name="formatType">格式化方式</param>
		/// <returns></returns>
		public static ConcatExpression Convert(this FieldItem field, DateTimeFieldFormatType formatType)
		{
			ConcatExpression concat = null;
			switch (field.Factory.Dal.DbType)
			{
				#region SQL Server

				// http://www.cnblogs.com/Gavinzhao/archive/2009/11/10/1599690.html
				// http://www.cnblogs.com/linzheng/archive/2010/11/17/1880208.html
				case DatabaseType.SQLServer:
				case DatabaseType.SqlCe:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							concat = new ConcatExpression(@"CONVERT(varchar(100), {0}, 23) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Time:
							concat = new ConcatExpression(@"CONVERT(varchar(100), {0}, 24) as {0}".FormatWith(field.QuotedColumnName)); // 24 或 108
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							concat = new ConcatExpression(@"CONVERT(varchar(100), {0}, 14) as {0}".FormatWith(field.QuotedColumnName)); // 14 或 114
							break;
						case DateTimeFieldFormatType.DateTime:
							concat = new ConcatExpression(@"CONVERT(varchar(100), {0}, 20) as {0}".FormatWith(field.QuotedColumnName)); // 20 或 120
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							concat = new ConcatExpression(@"CONVERT(varchar(100), {0}, 21) as {0}".FormatWith(field.QuotedColumnName)); // 25 或 21 、121
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							concat = new ConcatExpression(@"CONVERT(varchar(16), {0}, 20) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							concat = new ConcatExpression(@"CONVERT(varchar(7), {0}, 23) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Year:
							concat = new ConcatExpression(@"DATENAME(Yy,{0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region SQLite

				// http://www.cnblogs.com/weixing/archive/2011/09/17/2179648.html
				case DatabaseType.SQLite:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							concat = new ConcatExpression("date({0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Time:
							concat = new ConcatExpression("time({0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							concat = new ConcatExpression(@"strftime('%f',{0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.DateTime:
							concat = new ConcatExpression("datetime({0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							concat = new ConcatExpression(@"strftime('%Y-%m-%d %H:%M:%f',{0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							concat = new ConcatExpression(@"strftime('%Y-%m-%d %H:%M',{0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							concat = new ConcatExpression(@"strftime('%Y-%m',{0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Year:
							concat = new ConcatExpression(@"strftime('%Y',{0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region MySql

				// http://www.cnblogs.com/andy_tigger/archive/2011/03/08/1977486.html
				case DatabaseType.MySql:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							concat = new ConcatExpression("DATE({0}) as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Time:
							concat = new ConcatExpression("DATE_FORMAT({0},'%T') as {0}".FormatWith(field.QuotedColumnName)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							concat = new ConcatExpression("DATE_FORMAT({0},'%T:%f') as {0}".FormatWith(field.QuotedColumnName)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.DateTime:
							concat = new ConcatExpression("DATE_FORMAT({0},'%Y-%m-%d %T') as {0}".FormatWith(field.QuotedColumnName)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							concat = new ConcatExpression("DATE_FORMAT({0},'%Y-%m-%d %T:%f') as {0}".FormatWith(field.QuotedColumnName)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							concat = new ConcatExpression("DATE_FORMAT({0},'%Y-%m-%d %H:%i') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							concat = new ConcatExpression("DATE_FORMAT({0},'%Y-%m') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Year:
							concat = new ConcatExpression("DATE_FORMAT({0},'%Y') as {0}".FormatWith(field.QuotedColumnName));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region Firebird

				// TODO
				case DatabaseType.Firebird:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							break;
						case DateTimeFieldFormatType.Time:
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							break;
						case DateTimeFieldFormatType.DateTime:
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							break;
						case DateTimeFieldFormatType.Year:
							break;
						default:
							break;
					}
					break;

				#endregion

				#region PostgreSQL

				// http://www.postgresql.org/docs/9.2/static/functions-formatting.html
				// http://www.postgresql.org/docs/9.2/static/functions-datetime.html
				// http://www.cnblogs.com/stephen-liu74/archive/2012/05/04/2294643.html
				case DatabaseType.PostgreSQL:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							concat = new ConcatExpression("date_trunc({0},'day') as {0}".FormatWith(field.QuotedColumnName)); // 2014-08-08 00:00:00
							break;
						case DateTimeFieldFormatType.Time:
							concat = new ConcatExpression("to_char({0},'HH24:MI:SS') as {0}".FormatWith(field.QuotedColumnName)); // 18:15:30
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							concat = new ConcatExpression("to_char({0},'HH24:MI:SS:US') as {0}".FormatWith(field.QuotedColumnName)); // 18:15:30:188000
							break;
						case DateTimeFieldFormatType.DateTime:
							concat = new ConcatExpression("date_trunc({0},'second') as {0}".FormatWith(field.QuotedColumnName)); // 2014-08-08 18:15:30
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							concat = new ConcatExpression("date_trunc({0},'microseconds') as {0}".FormatWith(field.QuotedColumnName)); // 2014-08-08 18:15:30:188000
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							concat = new ConcatExpression("date_trunc({0},'minute') as {0}".FormatWith(field.QuotedColumnName)); // 2014-08-08 18:15
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							concat = new ConcatExpression("date_trunc({0},'month') as {0}".FormatWith(field.QuotedColumnName)); // 2014-08-01 00:00:00
							break;
						case DateTimeFieldFormatType.Year:
							concat = new ConcatExpression("date_trunc({0},'year') as {0}".FormatWith(field.QuotedColumnName)); // 2014-01-01 00:00:00
							break;
						default:
							break;
					}
					break;

				#endregion

				#region Oracle

				// http://www.cnblogs.com/chuncn/archive/2009/04/29/1381282.html
				// http://blog.csdn.net/xxd851116/article/details/6250482
				// http://database.51cto.com/art/201010/231193.htm
				case DatabaseType.Oracle:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							concat = new ConcatExpression("to_date({0},'yyyy-mm-dd') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Time:
							concat = new ConcatExpression("to_char({0},'hh24:mi:ss') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							concat = new ConcatExpression("to_char({0},'hh24:mi:ss.ff9') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.DateTime:
							concat = new ConcatExpression("to_date({0},'yyyy-mm-dd hh24:mi:ss') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							concat = new ConcatExpression("to_date({0},'yyyy-mm-dd hh24:mi:ss.ff9') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							concat = new ConcatExpression("to_char({0},'yyyy-mm-dd hh24:mi') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							concat = new ConcatExpression("to_char({0},'yyyy-mm') as {0}".FormatWith(field.QuotedColumnName));
							break;
						case DateTimeFieldFormatType.Year:
							concat = new ConcatExpression("to_char({0},'yyyy') as {0}".FormatWith(field.QuotedColumnName));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region Other

				case DatabaseType.Access:
				case DatabaseType.Network:
				case DatabaseType.Distributed:
				case DatabaseType.Other:
				case DatabaseType.None:
				default:
					break;

				#endregion
			}

			if (concat == null) { concat = new ConcatExpression(field.QuotedColumnName); }
			return concat;
		}

		/// <summary>日期时间转换排序</summary>
		/// <param name="field">字段</param>
		/// <param name="formatType">格式化方式</param>
		/// <param name="isDesc">是否降序</param>
		/// <returns></returns>
		public static ConcatExpression Convert(this FieldItem field, DateTimeFieldFormatType formatType, Boolean isDesc)
		{
			ConcatExpression order = null;
			switch (field.Factory.Dal.DbType)
			{
				#region SQL Server

				// http://www.cnblogs.com/Gavinzhao/archive/2009/11/10/1599690.html
				// http://www.cnblogs.com/linzheng/archive/2010/11/17/1880208.html
				case DatabaseType.SQLServer:
				case DatabaseType.SqlCe:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							order = new ConcatExpression(@"CONVERT(varchar(100), {0}, 23){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Time:
							order = new ConcatExpression(@"CONVERT(varchar(100), {0}, 24){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 24 或 108
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							order = new ConcatExpression(@"CONVERT(varchar(100), {0}, 14){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 14 或 114
							break;
						case DateTimeFieldFormatType.DateTime:
							order = new ConcatExpression(@"CONVERT(varchar(100), {0}, 20){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 20 或 120
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							order = new ConcatExpression(@"CONVERT(varchar(100), {0}, 21){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 25 或 21 、121
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							order = new ConcatExpression(@"CONVERT(varchar(16), {0}, 20){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							order = new ConcatExpression(@"CONVERT(varchar(7), {0}, 23){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Year:
							order = new ConcatExpression(@"DATENAME(Yy,{0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region SQLite

				// http://www.cnblogs.com/weixing/archive/2011/09/17/2179648.html
				case DatabaseType.SQLite:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							order = new ConcatExpression("date({0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Time:
							order = new ConcatExpression("time({0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							order = new ConcatExpression(@"strftime('%f',{0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.DateTime:
							order = new ConcatExpression("datetime({0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							order = new ConcatExpression(@"strftime('%Y-%m-%d %H:%M:%f',{0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							order = new ConcatExpression(@"strftime('%Y-%m-%d %H:%M',{0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							order = new ConcatExpression(@"strftime('%Y-%m',{0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Year:
							order = new ConcatExpression(@"strftime('%Y',{0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region MySql

				// http://www.cnblogs.com/andy_tigger/archive/2011/03/08/1977486.html
				case DatabaseType.MySql:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							order = new ConcatExpression("DATE({0}){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Time:
							order = new ConcatExpression("DATE_FORMAT({0},'%T'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							order = new ConcatExpression("DATE_FORMAT({0},'%T:%f'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.DateTime:
							order = new ConcatExpression("DATE_FORMAT({0},'%Y-%m-%d %T'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							order = new ConcatExpression("DATE_FORMAT({0},'%Y-%m-%d %T:%f'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // %T = %H:%i:%S
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							order = new ConcatExpression("DATE_FORMAT({0},'%Y-%m-%d %H:%i'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							order = new ConcatExpression("DATE_FORMAT({0},'%Y-%m'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Year:
							order = new ConcatExpression("DATE_FORMAT({0},'%Y'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region Firebird

				// TODO
				case DatabaseType.Firebird:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							break;
						case DateTimeFieldFormatType.Time:
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							break;
						case DateTimeFieldFormatType.DateTime:
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							break;
						case DateTimeFieldFormatType.Year:
							break;
						default:
							break;
					}
					break;

				#endregion

				#region PostgreSQL

				// http://www.postgresql.org/docs/9.2/static/functions-formatting.html
				// http://www.postgresql.org/docs/9.2/static/functions-datetime.html
				// http://www.cnblogs.com/stephen-liu74/archive/2012/05/04/2294643.html
				case DatabaseType.PostgreSQL:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							order = new ConcatExpression("date_trunc({0},'day'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 2014-08-08 00:00:00
							break;
						case DateTimeFieldFormatType.Time:
							order = new ConcatExpression("to_char({0},'HH24:MI:SS'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 18:15:30
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							order = new ConcatExpression("to_char({0},'HH24:MI:SS:US'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 18:15:30:188000
							break;
						case DateTimeFieldFormatType.DateTime:
							order = new ConcatExpression("date_trunc({0},'second'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 2014-08-08 18:15:30
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							order = new ConcatExpression("date_trunc({0},'microseconds'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 2014-08-08 18:15:30:188000
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							order = new ConcatExpression("date_trunc({0},'minute'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 2014-08-08 18:15
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							order = new ConcatExpression("date_trunc({0},'month'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 2014-08-01 00:00:00
							break;
						case DateTimeFieldFormatType.Year:
							order = new ConcatExpression("date_trunc({0},'year'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty)); // 2014-01-01 00:00:00
							break;
						default:
							break;
					}
					break;

				#endregion

				#region Oracle

				// http://www.cnblogs.com/chuncn/archive/2009/04/29/1381282.html
				// http://blog.csdn.net/xxd851116/article/details/6250482
				// http://database.51cto.com/art/201010/231193.htm
				case DatabaseType.Oracle:
					switch (formatType)
					{
						case DateTimeFieldFormatType.Date:
							order = new ConcatExpression("to_date({0},'yyyy-mm-dd'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Time:
							order = new ConcatExpression("to_char({0},'hh24:mi:ss'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.TimeWithMicroSeconds:
							order = new ConcatExpression("to_char({0},'hh24:mi:ss.ff9'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.DateTime:
							order = new ConcatExpression("to_date({0},'yyyy-mm-dd hh24:mi:ss'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.DateTimeWithMicroSeconds:
							order = new ConcatExpression("to_date({0},'yyyy-mm-dd hh24:mi:ss.ff9'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.DateTimeWithoutSeconds:
							order = new ConcatExpression("to_char({0},'yyyy-mm-dd hh24:mi'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.YearAndMonth:
							order = new ConcatExpression("to_char({0},'yyyy-mm'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						case DateTimeFieldFormatType.Year:
							order = new ConcatExpression("to_char({0},'yyyy'){1}".FormatWith(field.QuotedColumnName, isDesc ? ExpressionConstants.SPDesc : String.Empty));
							break;
						default:
							break;
					}
					break;

				#endregion

				#region Other

				case DatabaseType.Access:
				case DatabaseType.Network:
				case DatabaseType.Distributed:
				case DatabaseType.Other:
				case DatabaseType.None:
				default:
					break;

				#endregion
			}

			if (order == null) { order = new ConcatExpression(field.QuotedColumnName); }
			return order;
		}

		#endregion
	}
}