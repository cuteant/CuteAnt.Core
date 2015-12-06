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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class FirebirdQuoter : QuoterBase
	{
		internal override String OpenQuote { get { return "\""; } }

		internal override String CloseQuote { get { return "\""; } }

		internal override String DefaultBlobValue { get { return "X''"; } }

		private static readonly DateTime _DateMin = new DateTime(100, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private static readonly DateTimeOffset _DateTimeOffsetMin = new DateTimeOffset(100, 1, 1, 0, 0, 0, TimeSpan.Zero);

		/// <summary>最小日期</summary>
		internal override DateTime DateMin { get { return _DateMin; } }

		/// <summary>最小时间</summary>
		public override DateTime DateTimeMin { get { return _DateMin; } }

		/// <summary>最小时间</summary>
		internal override DateTime DateTime2Min { get { return _DateMin; } }

		/// <summary>Min DateTimeOffset</summary>
		internal override DateTimeOffset DateTimeOffsetMin { get { return _DateTimeOffsetMin; } }

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		public override String FormatDateTime(DateTime value)
		{
			//return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
			//return "{1}{0:yyyy-MM-dd HH:mm:ss.fff}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.fff";

			var sb = new StringBuilder(25);
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
			//return "{1}{0:yyyy-MM-dd HH:mm:ss.ffff}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.ffff";

			var sb = new StringBuilder(26);
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
			// FireBird 不支持时区保存，使用 UTC 时间格式保存
			//return "{1}{0:yyyy-MM-dd HH:mm:ss.ffff}{1}".FormatWith(CultureInfo.InvariantCulture, value.UtcDateTime, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.ffff";

			var sb = new StringBuilder(26);
			sb.Append(ValueQuote);
			sb.Append(value.UtcDateTime.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatGuid(Guid value)
		{
			//return FormatByteArray(value.ToByteArray());
			var sb = new StringBuilder(38);
			sb.Append(ValueQuote);
			sb.Append(value.ToString());
			sb.Append(ValueQuote);
			return sb.ToString();
		}

		internal override String FormatGuid32Digits(Guid value)
		{
			//return FormatByteArray(value.ToByteArray());
			var sb = new StringBuilder(34);
			sb.Append(ValueQuote);
			sb.Append(value.ToString("N"));
			sb.Append(ValueQuote);
			return sb.ToString();
		}

		internal override String FormatCombGuid(CombGuid value)
		{
			if (value.IsNull) { return DefaultBlobValue; }

			var sb = new StringBuilder(35);
			sb.Append("X'");
			sb.Append(value.GetHexChars(CombGuidSequentialSegmentType.Comb));
			sb.Append("'");
			return sb.ToString();
		}

		internal override String FormatCombGuid32Digits(CombGuid value)
		{
			if (value.IsNull) { return DefaultBlobValue; }

			var sb = new StringBuilder(35);
			sb.Append("X'");
			sb.Append(value.GetHexChars(CombGuidSequentialSegmentType.Comb));
			sb.Append("'");
			return sb.ToString();
		}

		internal override String FormatByteArray(Byte[] value)
		{
			var hex = new StringBuilder((value.Length * 2) + 3);

			hex.Append("X'");
			var cs = HexToChars(value);
			hex.Append(cs);
			hex.Append("'");

			return hex.ToString();
		}
	}
}