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
using System.Data.SqlTypes;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlServer2000Quoter : QuoterBase
	{
		/// <summary>最小日期</summary>
		internal override DateTime DateMin { get { return SqlDateTime.MinValue.Value; } }

		/// <summary>最小时间</summary>
		public override DateTime DateTimeMin { get { return SqlDateTime.MinValue.Value; } }

		/// <summary>最小时间</summary>
		internal override DateTime DateTime2Min { get { return SqlDateTime.MinValue.Value; } }

		private static readonly DateTimeOffset _DateTimeOffsetMin = new DateTimeOffset(SqlDateTime.MinValue.Value, TimeSpan.Zero);

		/// <summary>Min DateTimeOffset</summary>
		internal override DateTimeOffset DateTimeOffsetMin { get { return _DateTimeOffsetMin; } }

		internal override String OpenQuote { get { return "["; } }

		internal override String CloseQuote { get { return "]"; } }

		internal override String CloseQuoteEscapeString { get { return "]]"; } }

		internal override String OpenQuoteEscapeString { get { return String.Empty; } }

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		public override String FormatDateTime(DateTime value)
		{
			//return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
			//return "#{0:yyyy-MM-dd HH:mm:ss}#".FormatWith(CultureInfo.InvariantCulture, value);
			//return "{ts" + "{1}{0:yyyy-MM-dd HH:mm:ss.fff}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote) + "}";
			const String iso8601Format = "yyyyMMdd HH:mm:ss.fff";

			var sb = new StringBuilder(23);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal override String FormatDateTime2(DateTime value)
		{
			const String iso8601Format = "yyyyMMdd HH:mm:ss.fff";

			var sb = new StringBuilder(23);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal override String FormatDateTimeOffset(DateTimeOffset value)
		{
			const String iso8601Format = "yyyyMMdd HH:mm:ss.fff";

			var sb = new StringBuilder(23);
			sb.Append(ValueQuote);
			sb.Append(value.UtcDateTime.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatString(String value, Boolean isUnicode = false)
		{
			//return ValueQuote + value.Replace(ValueQuote, EscapeValueQuote) + ValueQuote;
			if (isUnicode)
			{
				return "N{1}{0}{1}".FormatWith(value.Replace(ValueQuote, EscapeValueQuote), ValueQuote);
			}
			else
			{
				return "{1}{0}{1}".FormatWith(value.Replace(ValueQuote, EscapeValueQuote), ValueQuote);
			}
		}

		internal override String FormatGuid(Guid value)
		{
			//return "CAST({1}{0}{1} AS UNIQUEIDENTIFIER)".FormatWith(value.ToString(), ValueQuote);
			const String guidFormat = "CONVERT(UNIQUEIDENTIFIER, '";
			var sb = new StringBuilder(65);
			sb.Append(guidFormat);
			sb.Append(value.ToString());
			sb.Append("')");
			return sb.ToString();
		}

		internal override String FormatGuid32Digits(Guid value)
		{
			const String guidFormat = "CONVERT(UNIQUEIDENTIFIER, '";
			var sb = new StringBuilder(65);
			sb.Append(guidFormat);
			sb.Append(value.ToString());
			sb.Append("')");
			return sb.ToString();
		}

		internal override String FormatCombGuid(CombGuid value)
		{
			const String combFormat = "CONVERT(UNIQUEIDENTIFIER, 0x";
			const String combEmtpy = "CONVERT(UNIQUEIDENTIFIER, 0x0)";

			if (value.IsNullOrEmpty) { return combEmtpy; }

			var sb = new StringBuilder(61);
			sb.Append(combFormat);
			sb.Append(value.GetHexChars(CombGuidSequentialSegmentType.Guid));
			sb.Append(")");
			return sb.ToString();
		}

		internal override String FormatCombGuid32Digits(CombGuid value)
		{
			const String combFormat = "CONVERT(UNIQUEIDENTIFIER, 0x";
			const String combEmtpy = "CONVERT(UNIQUEIDENTIFIER, 0x0)";

			if (value.IsNullOrEmpty) { return combEmtpy; }

			var sb = new StringBuilder(61);
			sb.Append(combFormat);
			sb.Append(value.GetHexChars(CombGuidSequentialSegmentType.Guid));
			sb.Append(")");
			return sb.ToString();
		}

		public override String QuoteSchemaName(String schemaName)
		{
			return (schemaName.IsNullOrWhiteSpace()) ? "[dbo]" : Quote(schemaName);
		}
	}
}