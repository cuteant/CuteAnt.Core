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
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class OracleQuoterQuotedIdentifier : QuoterBase
	{
		internal override String DefaultBlobValue { get { return "NULL"; } }

		//internal override String FormatString(String value, Boolean isUnicode = false)
		//{
		//	//return ValueQuote + value.Replace(ValueQuote, EscapeValueQuote) + ValueQuote;
		//	if (isUnicode)
		//	{
		//		return "N{1}{0}{1}".FormatWith(value.Replace(ValueQuote, EscapeValueQuote), ValueQuote);
		//	}
		//	else
		//	{
		//		return "{1}{0}{1}".FormatWith(value.Replace(ValueQuote, EscapeValueQuote), ValueQuote);
		//	}
		//}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">日期</param>
		/// <returns></returns>
		internal override String FormatDate(DateTime value)
		{
			const String iso8601Format = "yyyy-MM-dd";
			return String.Format("to_timestamp({0}{1}{0}, {0}yyyy-mm-dd{0})", ValueQuote, value.ToString(iso8601Format));
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		public override String FormatDateTime(DateTime value)
		{
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.fff";
			return String.Format("to_timestamp({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss.ff3{0})", ValueQuote, value.ToString(iso8601Format)); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal override String FormatDateTime2(DateTime value)
		{
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.fffffff";
			return String.Format("to_timestamp({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss.ff7{0})", ValueQuote, value.ToString(iso8601Format)); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
		}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		internal override String FormatDateTimeOffset(DateTimeOffset value)
		{
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
			return String.Format("to_timestamp_tz({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss.ff7 TZH:TZM{0})", ValueQuote, value.ToString(iso8601Format)); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
		}

		internal override String FormatGuid(Guid value)
		{
			var sb = new StringBuilder(44);
			sb.Append(_HexToRawFunc);
			CombGuid comb = value;
			sb.Append(comb.GetHexChars(CombGuidSequentialSegmentType.Guid));
			sb.Append("')");

			return sb.ToString();
		}

		internal override String FormatGuid32Digits(Guid value)
		{
			var sb = new StringBuilder(44);
			sb.Append(_HexToRawFunc);
			CombGuid comb = value;
			sb.Append(comb.GetHexChars(CombGuidSequentialSegmentType.Guid));
			sb.Append("')");

			return sb.ToString();
		}

		internal override String FormatCombGuid(CombGuid value)
		{
			const String combEmpty = "hextoraw('')";

			if (value.IsNull) { return combEmpty; }

			var sb = new StringBuilder(44);
			sb.Append(_HexToRawFunc);
			sb.Append(value.GetHexChars(CombGuidSequentialSegmentType.Comb));
			sb.Append("')");

			return sb.ToString();
		}

		internal override String FormatCombGuid32Digits(CombGuid value)
		{
			const String combEmpty = "hextoraw('')";

			if (value.IsNull) { return combEmpty; }

			var sb = new StringBuilder(44);
			sb.Append(_HexToRawFunc);
			sb.Append(value.GetHexChars(CombGuidSequentialSegmentType.Comb));
			sb.Append("')");

			return sb.ToString();
		}

		private const String _HexToRawFunc = "hextoraw('";

		internal override String FormatByteArray(Byte[] value)
		{
			var hex = new StringBuilder((value.Length * 2) + 12);

			hex.Append(_HexToRawFunc);
			var cs = HexToChars(value);
			hex.Append(cs);
			hex.Append("')");

			return hex.ToString();
		}
	}
}