/* 本模块基于开源项目 FluentMigrator 的子模块 Runner.Generators 修改而成。修改：海洋饼干(cuteant@outlook.com)
 * 
 * h1. FluentMigrator
 * 
 * Fluent Migrator is a migration framework for .NET much like Ruby Migrations. Migrations are a structured way to alter your database schema and are an alternative to creating lots of sql scripts that have to be run manually by every developer involved. Migrations solve the problem of evolving a database schema for multiple databases (for example, the developer's local database, the test database and the production database). Database schema changes are described in classes written in C# that can be checked into version control.
 * 
 * h2. Project Info
 * 
 * *Documentation*: "http://wiki.github.com/schambers/fluentmigrator/":http://wiki.github.com/schambers/fluentmigrator/
 * *Discussions*: "fluentmigrator-google-group@googlegroups.com":http://groups.google.com/group/fluentmigrator-google-group
 * *Bug/Feature Tracking*: "http://github.com/schambers/fluentmigrator/issues":http://github.com/schambers/fluentmigrator/issues
 * *TeamCity sources*: "http://teamcity.codebetter.com/viewType.html?buildTypeId=bt82&tab=buildTypeStatusDiv":http://teamcity.codebetter.com/viewType.html?buildTypeId=bt82&tab=buildTypeStatusDiv
 ** Click the "Login as guest" link in the footer of the page.
 * 
 * h2. Build Status
 * 
 * The build is generously hosted and run on the "CodeBetter TeamCity":http://codebetter.com/codebetter-ci/ infrastructure.
 * Latest build status: !http://teamcity.codebetter.com/app/rest/builds/buildType:(id:bt82)/statusIcon!:http://teamcity.codebetter.com/viewType.html?buildTypeId=bt82&guest=1
 * 
 * Our Mono build is hosted on Travis CI.
 * Latest Mono build status: !https://secure.travis-ci.org/schambers/fluentmigrator.png!:http://travis-ci.org/schambers/fluentmigrator
 * 
 * h2. Powered by
 * 
 * <img src="http://www.jetbrains.com/img/logos/logo_resharper_small.gif" width="142" height="29" alt="ReSharper">
 * 
 * h2. Contributors
 * 
 * A "long list":https://github.com/schambers/fluentmigrator/wiki/ContributorList of everyone that has contributed to FluentMigrator. Thanks for all the Pull Requests!
 * 
 * h2. License
 * 
 * "Apache 2 License":https://github.com/schambers/fluentmigrator/blob/master/LICENSE.txt
 */

using System;
using System.Globalization;
using System.Text;
using CuteAnt.OrmLite.Common;
using CuteAnt.Log;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class QuoterBase : IQuoter
	{
		#region -- 属性 --

		/// <summary>最小日期</summary>
		internal virtual DateTime DateMin { get { return DateTime.MinValue; } }

		/// <summary>最大日期</summary>
		internal virtual DateTime DateMax { get { return DateTime.MaxValue.Date; } }

		/// <summary>最小时间</summary>
		public virtual DateTime DateTimeMin { get { return DateTime.MinValue; } }

		/// <summary>最大时间</summary>
		internal virtual DateTime DateTimeMax { get { return DateTime.MaxValue; } }

		/// <summary>最小时间</summary>
		internal virtual DateTime DateTime2Min { get { return DateTime.MinValue; } }

		/// <summary>最大时间</summary>
		internal virtual DateTime DateTime2Max { get { return DateTime.MaxValue; } }

		/// <summary>Min DateTimeOffset</summary>
		internal virtual DateTimeOffset DateTimeOffsetMin { get { return DateTimeOffset.MinValue; } }

		/// <summary>Max DateTimeOffset</summary>
		internal virtual DateTimeOffset DateTimeOffsetMax { get { return DateTimeOffset.MaxValue; } }

		internal virtual String ValueQuote { get { return "'"; } }

		internal virtual String EscapeValueQuote { get { return "''"; } } // ValueQuote + ValueQuote; } }

		/// <summary>Returns the opening quote identifier - " is the standard according to the specification</summary>
		internal virtual String OpenQuote { get { return "\""; } }

		/// <summary>Returns the closing quote identifier - " is the standard according to the specification</summary>
		internal virtual String CloseQuote { get { return "\""; } }

		internal virtual String OpenQuoteEscapeString { get { return OpenQuote.PadRight(2, OpenQuote.ToCharArray()[0]); } }

		internal virtual String CloseQuoteEscapeString { get { return CloseQuote.PadRight(2, CloseQuote.ToCharArray()[0]); } }

		internal virtual String DefaultBlobValue { get { return "0x0"; } }

		#endregion

		#region -- 值 --

		/// <summary>格式化数据为SQL数据</summary>
		/// <param name="value">数据值</param>
		/// <returns></returns>
		public String QuoteValue(String value)
		{
			if (value == null) { return FormatNull(); }

			return FormatString(value);
		}

		/// <summary>格式化数据为SQL数据</summary>
		/// <param name="value">数据值</param>
		/// <returns></returns>
		public String QuoteValue(Object value)
		{
			if (value == null || DBNull.Value.Equals(value)) { return FormatNull(); }

			var type = value.GetType();
			if (type == typeof(String))
			{
				return FormatString((String)value);
			}
			else if (type == typeof(Boolean))
			{
				return FormatBool((Boolean)value);
			}
			else if (type == typeof(DateTime))
			{
				var dt = (DateTime)value;
				if (dt < DateTimeMin || dt > DateTime.MaxValue) { return FormatNull(); }
				if ((dt == DateTime.MinValue || dt == DateTimeMin)) { return FormatNull(); }
				return FormatDateTime(dt);
			}
			else if (type == typeof(DateTimeOffset))
			{
				var dt = (DateTime)value;
				if (dt < DateTimeMin || dt > DateTime.MaxValue) { return FormatNull(); }
				if ((dt == DateTime.MinValue || dt == DateTimeMin)) { return FormatNull(); }
				return FormatDateTime(dt);
			}
			else if (type == typeof(DateTimeOffset))
			{
				var dt = (DateTimeOffset)value;
				if (dt <= DateTimeOffset.MinValue || dt > DateTimeOffset.MaxValue) { return FormatNull(); }
				return FormatDateTimeOffset(dt);
			}
			else if (type == typeof(Decimal))
			{
				return FormatDecimal((Decimal)value);
			}
			else if (type == typeof(Guid))
			{
				return FormatGuid((Guid)value);
			}
			else if (type == typeof(CombGuid))
			{
				return FormatCombGuid((CombGuid)value);
			}
			else if (type == typeof(Double))
			{
				return FormatDouble((Double)value);
			}
			else if (type == typeof(Single))
			{
				return FormatFloat((Single)value);
			}
			else if (type == typeof(Byte[]))
			{
				var bts = value as Byte[];
				if (bts == null || bts.Length < 1) { return FormatNull(); }
				return FormatByteArray(bts);
			}

			// 转为目标类型，比如枚举转为数字
			value = value.ChangeType(type);
			if (value == null) { return FormatNull(); }

			return value.ToString();
		}

		/// <summary>格式化数据为SQL数据</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public virtual String QuoteValue(IDataColumn field, Object value)
		{
			#region 旧代码
			//var isNullable = true;
			//Type type = null;

			//if (field != null)
			//{
			//	type = field.DataType;
			//	isNullable = field.Nullable;
			//}
			//else if (value != null)
			//{
			//	type = value.GetType();
			//}

			//var code = Type.GetTypeCode(type);
			//switch (code)
			//{
			//	case TypeCode.String:
			//		if (value == null) { return isNullable ? FormatNull() : EscapeValueQuote; }
			//		return FormatString("" + value, field.IsUnicode || IsUnicode(field.RawType));

			//	case TypeCode.Boolean:
			//		if (value == null) { return isNullable ? FormatNull() : String.Empty; }
			//		return FormatBool(value.ToBoolean());

			//	case TypeCode.DateTime:
			//		if (value == null) { return isNullable ? FormatNull() : EscapeValueQuote; }
			//		var dt = value.ToDateTime();
			//		if (dt < DateTimeMin || dt > DateTime.MaxValue) { return isNullable ? FormatNull() : EscapeValueQuote; }
			//		if ((dt == DateTime.MinValue || dt == DateTimeMin) && isNullable) { return FormatNull(); }
			//		return FormatDateTime(dt);

			//	case TypeCode.Decimal:
			//		if (value == null) { return isNullable ? FormatNull() : String.Empty; }
			//		return FormatDecimal(value.ToDecimal());

			//	case TypeCode.Double:
			//		if (value == null) { return isNullable ? FormatNull() : String.Empty; }
			//		return FormatDouble(value.ToDouble());

			//	case TypeCode.Single:
			//		if (value == null) { return isNullable ? FormatNull() : String.Empty; }
			//		return FormatFloat(value.ToSingle());

			//	default:
			//		if (type == typeof(Byte[]))
			//		{
			//			if (value == null) { return isNullable ? FormatNull() : "0x0"; }
			//			var bts = value as Byte[];
			//			if (bts == null || bts.Length < 1) { return isNullable ? FormatNull() : "0x0"; }
			//			return FormatByteArray(bts);
			//		}
			//		else if (value.GetType() == typeof(Guid))
			//		{
			//			if (value == null) { return isNullable ? FormatNull() : EscapeValueQuote; }
			//			return FormatGuid((Guid)value);
			//		}

			//		break;
			//}

			//if (value == null) { return isNullable ? FormatNull() : ""; }

			//// 转为目标类型，比如枚举转为数字
			//value = value.ChangeType(type);
			//if (value == null) { return isNullable ? FormatNull() : ""; }

			//return value.ToString();
			#endregion
			if (field == null) { return QuoteValue(value); }
			//if (value == null) { return FormatNull(); }

			/*
			* 不再考虑字段是否有设置默认值，那是 EntityPersistence 考虑的事
			* 
			* 转义数据原则：
			* 1、允许为空的字段，直接赋空值
			* 2、不允许为空的字段，如果数据值为空，需要智能识别并添加相应字段的默认数据（与 Helper.GetCommonDbTypeDefaultValue 方法返回值一致）
			*/
			switch (field.DbType)
			{
				#region 文本

				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : EscapeValueQuote; }
					return FormatString((String)value, false);

				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
				case CommonDbType.Text:
				case CommonDbType.Xml:
				case CommonDbType.Json:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : EscapeValueQuote; }
					return FormatString((String)value, true);

				#endregion

				#region Guid

				case CommonDbType.CombGuid:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatCombGuid(CombGuid.Empty); }
					return FormatCombGuid((CombGuid)value);
				case CommonDbType.CombGuid32Digits:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatCombGuid32Digits(CombGuid.Empty); }
					return FormatCombGuid32Digits((CombGuid)value);

				case CommonDbType.Guid:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatGuid(Guid.Empty); }
					return FormatGuid((Guid)value);
				case CommonDbType.Guid32Digits:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatGuid32Digits(Guid.Empty); }
					return FormatGuid32Digits((Guid)value);

				#endregion

				#region 布尔

				case CommonDbType.Boolean:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatBool(false); }
					return FormatBool((Boolean)value);

				#endregion

				#region 日期时间

				case CommonDbType.Date:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatDate(DateMin); }
					var date = Convert.ToDateTime(value);
					if (field.Nullable)
					{
						if (date <= DateMin || date > DateMax) { return FormatNull(); }
					}
					else
					{
						if (date < DateMin || date > DateMax) { return FormatDate(DateMin); }
					}
					return FormatDate(date);

				case CommonDbType.DateTime:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatDateTime(DateTimeMin); }
					var dt = Convert.ToDateTime(value);
					if (field.Nullable)
					{
						if (dt <= DateTimeMin || dt > DateTimeMax) { return FormatNull(); }
					}
					else
					{
						if (dt < DateTimeMin || dt > DateTimeMax) { return FormatDateTime(DateTimeMin); }
					}
					return FormatDateTime(dt);

				case CommonDbType.DateTime2:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatDateTime2(DateTime2Min); }
					var dt2 = Convert.ToDateTime(value);
					if (field.Nullable)
					{
						if (dt2 <= DateTime2Min || dt2 > DateTimeMax) { return FormatNull(); }
					}
					else
					{
						if (dt2 < DateTime2Min || dt2 > DateTimeMax) { return FormatDateTime2(DateTime2Min); }
					}
					return FormatDateTime2(dt2);

				case CommonDbType.DateTimeOffset:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatDateTimeOffset(DateTimeOffsetMin); }
					var dtOffset = (DateTimeOffset)value;
					if (field.Nullable)
					{
						if (dtOffset <= DateTimeOffsetMin || dtOffset > DateTimeOffsetMax) { return FormatNull(); }
					}
					else
					{
						if (dtOffset < DateTimeOffsetMin || dtOffset > DateTimeOffsetMax) { return FormatDateTimeOffset(DateTimeOffsetMin); }
					}
					return FormatDateTimeOffset(dtOffset);

				case CommonDbType.Time:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatTime(TimeSpan.Zero); } // 时间字段类型为长整形
					return FormatTime((TimeSpan)value);

				#endregion

				#region 精确数值 / 金额

				case CommonDbType.Currency:
				case CommonDbType.Decimal:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatDecimal(0M); }
					return FormatDecimal((Decimal)value);

				#endregion

				#region 浮点型

				case CommonDbType.Double:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatDouble(0D); }
					return FormatDouble((Double)value);

				case CommonDbType.Float:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : FormatFloat(0F); }
					return FormatFloat((Single)value);

				#endregion

				#region 二进制

				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : DefaultBlobValue; }
					var bts = value as Byte[];
					if (bts.Length < 1) { return field.Nullable ? FormatNull() : DefaultBlobValue; }
					return FormatByteArray(bts);

				#endregion

				#region 整形

				case CommonDbType.SignedTinyInt:
				case CommonDbType.TinyInt:
				case CommonDbType.SmallInt:
				case CommonDbType.Integer:
				case CommonDbType.BigInt:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : Helper.IntegerZero; }

					// 转为目标类型，比如枚举转为数字
					value = value.ChangeType(field.DataType);
					if (value == null) { return field.Nullable ? FormatNull() : Helper.IntegerZero; }
					return value.ToString();

				#endregion

				#region 其他

				default:
					if (value == null || DBNull.Value.Equals(value)) { return field.Nullable ? FormatNull() : String.Empty; }

					// 转为目标类型，比如枚举转为数字
					value = value.ChangeType(field.DataType);
					if (value == null) { return field.Nullable ? FormatNull() : String.Empty; }

					return value.ToString();

				#endregion
			}
		}

		internal virtual String FormatNull()
		{
			const String _NULL = "NULL";
			return _NULL;
		}

		internal virtual String FormatString(String value, Boolean isUnicode = false)
		{
			//return ValueQuote + value.Replace(ValueQuote, EscapeValueQuote) + ValueQuote;
			return "{1}{0}{1}".FormatWith(value.Replace(ValueQuote, EscapeValueQuote), ValueQuote);
		}

		internal virtual String FormatBool(Boolean value)
		{
			//return (value) ? 1.ToString() : 0.ToString();
			return value ? "1" : Helper.IntegerZero;
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">日期</param>
		/// <returns></returns>
		internal virtual String FormatDate(DateTime value)
		{
			//return "{1}{0:yyyy-MM-dd}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd";

			var sb = new StringBuilder(12);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		public virtual String FormatDateTime(DateTime value)
		{
			//return ValueQuote + (value).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) + ValueQuote;
			//return "{1}{0:yyyy-MM-dd HH:mm:ss}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss";

			var sb = new StringBuilder(21);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal virtual String FormatDateTime2(DateTime value)
		{
			//return "{1}{0:yyyy-MM-dd HH:mm:ss}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss";

			var sb = new StringBuilder(21);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal virtual String FormatDateTimeOffset(DateTimeOffset value)
		{
			//return "{1}{0:yyyy-MM-dd HH:mm:ss}{1}".FormatWith(CultureInfo.InvariantCulture, value.UtcDateTime, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss";

			var sb = new StringBuilder(21);
			sb.Append(ValueQuote);
			sb.Append(value.UtcDateTime.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal virtual String FormatTime(TimeSpan value)
		{
			return value.Ticks.ToString();
		}

		internal virtual String FormatByteArray(Byte[] value)
		{
			var hex = new StringBuilder((value.Length * 2) + 2);
			hex.Append("0x");

			//foreach (Byte b in value)
			//{
			//	hex.AppendFormat("{0:x2}", b);
			//}
			var cs = HexToChars(value);
			hex.Append(cs);

			return hex.ToString();
		}

		internal static Char[] HexToChars(Byte[] value)
		{
			var count = value.Length;
			var cs = new Char[count * 2];
			// 两个索引一起用，避免乘除带来的性能损耗
			for (int i = 0, j = 0; i < count; i++, j += 2)
			{
				var b = value[i];
				cs[j] = GetHexValue(b / 0x10);
				cs[j + 1] = GetHexValue(b % 0x10);
			}
			return cs;
		}

		private static Char GetHexValue(Int32 value)
		{
			if (value < 10) { return (Char)(value + 0x30); }
			//return (Char)(value - 10 + 0x41); // 大写字符
			return (Char)(value - 10 + 0x61); // 小写字符
		}

		private String FormatDecimal(Decimal value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private String FormatDouble(Double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private String FormatFloat(Single value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		internal virtual String FormatGuid(Guid value)
		{
			//return ValueQuote + value.ToString() + ValueQuote;
			return "{1}{0}{1}".FormatWith(value.ToString(), ValueQuote);
		}

		internal virtual String FormatGuid32Digits(Guid value)
		{
			//return ValueQuote + value.ToString() + ValueQuote;
			return "{1}{0}{1}".FormatWith(value.ToString("N"), ValueQuote);
		}

		internal virtual String FormatCombGuid(CombGuid value)
		{
			//return ValueQuote + value.ToString() + ValueQuote;
			if (value.IsNull) { return EscapeValueQuote; }
			return "{1}{0}{1}".FormatWith(value.ToString(), ValueQuote);
		}

		internal virtual String FormatCombGuid32Digits(CombGuid value)
		{
			//return ValueQuote + value.ToString() + ValueQuote;
			if (value.IsNull) { return EscapeValueQuote; }
			return "{1}{0}{1}".FormatWith(value.ToString(CombGuidFormatStringType.Comb32Digits), ValueQuote);
		}

		#endregion

		#region -- Command --

		public virtual String QuoteCommand(String command)
		{
			return command.Replace("\'", "\'\'");
		}

		#endregion

		#region -- 关键字 --

		/// <summary>Returns a quoted String that has been correctly escaped</summary>
		public virtual String Quote(String name)
		{
			// Exit early if not quoting is needed
			if (!ShouldQuote(name)) { return name; }

			var quotedName = name;
			if (!OpenQuoteEscapeString.IsNullOrWhiteSpace())
			{
				quotedName = name.Replace(OpenQuote, OpenQuoteEscapeString);
			}

			// If closing quote is the same as the opening quote then no need to escape again
			if (OpenQuote != CloseQuote)
			{
				if (!CloseQuoteEscapeString.IsNullOrWhiteSpace())
				{
					quotedName = quotedName.Replace(CloseQuote, CloseQuoteEscapeString);
				}
			}

			//return OpenQuote + quotedName + CloseQuote;
			return "{1}{0}{2}".FormatWith(quotedName, OpenQuote, CloseQuote);
		}

		/// <summary>Quotes a column name</summary>
		public virtual String QuoteColumnName(String columnName)
		{
			return IsQuoted(columnName) ? columnName : Quote(columnName);
		}

		/// <summary>Quotes a constraint name</summary>
		public virtual String QuoteConstraintName(String constraintName)
		{
			return IsQuoted(constraintName) ? constraintName : Quote(constraintName);
		}

		/// <summary>Quote an index name</summary>
		/// <param name="indexName"></param>
		/// <returns></returns>
		public virtual String QuoteIndexName(String indexName)
		{
			return IsQuoted(indexName) ? indexName : Quote(indexName);
		}

		/// <summary>Quotes a Table name</summary>
		public virtual String QuoteTableName(String tableName)
		{
			return IsQuoted(tableName) ? tableName : Quote(tableName);
		}

		/// <summary>Quotes a Schema Name</summary>
		public virtual String QuoteSchemaName(String schemaName)
		{
			return IsQuoted(schemaName) ? schemaName : Quote(schemaName);
		}

		/// <summary>Quotes a Sequence name</summary>
		public virtual String QuoteSequenceName(String sequenceName)
		{
			return IsQuoted(sequenceName) ? sequenceName : Quote(sequenceName);
		}

		/// <summary>Quotes a DataBase name</summary>
		public virtual String QuoteDataBaseName(String dbName)
		{
			return IsQuoted(dbName) ? dbName : Quote(dbName);
		}

		#endregion

		#region -- UnQuote --

		/// <summary>Provides and unquoted, unescaped String</summary>
		public virtual String UnQuote(String quoted)
		{
			String unquoted;

			if (IsQuoted(quoted))
			{
				unquoted = quoted.Substring(1, quoted.Length - 2);
			}
			else
			{
				unquoted = quoted;
			}

			unquoted = unquoted.Replace(OpenQuoteEscapeString, OpenQuote);

			if (OpenQuote != CloseQuote)
			{
				unquoted = unquoted.Replace(CloseQuoteEscapeString, CloseQuote);
			}

			return unquoted;
		}

		#endregion

		#region -- IsQuoted --

		/// <summary>Returns true is the value starts and ends with a close quote</summary>
		public virtual Boolean IsQuoted(String name)
		{
			if (name.IsNullOrEmpty()) { return false; }
			//This can return true incorrectly in some cases edge cases.
			//If a String say [myname]] is passed in this is not correctly quote for MSSQL but this function will
			//return true.
			return (name.StartsWith(OpenQuote) && name.EndsWith(CloseQuote));
		}

		#endregion

		#region -- IsUnicode --

		/// <summary>是否Unicode编码。只是固定判断n开头的几个常见类型为Unicode编码，这种方法不是很严谨，可以考虑读取DataTypes架构</summary>
		/// <param name="rawType"></param>
		/// <returns></returns>
		public virtual Boolean IsUnicode(String rawType)
		{
			if (rawType.IsNullOrWhiteSpace()) { return false; }
			rawType = rawType.ToLowerInvariant();
			if (rawType.StartsWith("nchar") || rawType.StartsWith("nvarchar") || rawType.StartsWith("ntext") || rawType.StartsWith("nclob"))
			{
				return true;
			}
			return false;
		}

		#endregion

		#region -- 辅助 --

		private Boolean ShouldQuote(String name)
		{
			return (!OpenQuote.IsNullOrWhiteSpace() || !CloseQuote.IsNullOrWhiteSpace()) && !name.IsNullOrWhiteSpace();
		}

		#endregion
	}
}