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
	internal class PostgresQuoter : QuoterBase
	{
		internal override String DefaultBlobValue { get { return "E''"; } }

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
			//return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
			//return "{1}{0:yyyy-MM-dd HH:mm:ss.ffffff}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.ffffff";

			var sb = new StringBuilder(28);
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
			//return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
			//return "{1}{0:yyyy-MM-dd HH:mm:ss.ffffff zzz}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.ffffff zzz";

			var sb = new StringBuilder(35);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatGuid(Guid value)
		{
			var sb = new StringBuilder(38);
			sb.Append(ValueQuote);
			sb.Append(value.ToString());
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatGuid32Digits(Guid value)
		{
			var sb = new StringBuilder(34);
			sb.Append(ValueQuote);
			sb.Append(value.ToString("N"));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatCombGuid(CombGuid value)
		{
			if (value.IsNull) { return EscapeValueQuote; }

			var sb = new StringBuilder(34);
			sb.Append(ValueQuote);
			sb.Append(value.GetChars(CombGuidFormatStringType.Comb32Digits));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatCombGuid32Digits(CombGuid value)
		{
			if (value.IsNull) { return EscapeValueQuote; }

			var sb = new StringBuilder(34);
			sb.Append(ValueQuote);
			sb.Append(value.GetChars(CombGuidFormatStringType.Comb32Digits));
			sb.Append(ValueQuote);

			return sb.ToString();
		}

		internal override String FormatByteArray(Byte[] byteArray)
		{
			var res = new StringBuilder((byteArray.Length * 5) + 3);

			res.Append("E'");
			foreach (byte b in byteArray)
			{
				if (b >= 0x20 && b < 0x7F && b != 0x27 && b != 0x5C)
				{
					res.Append((char)b);
				}
				else
				{
					res.Append("\\\\")
							.Append((char)('0' + (7 & (b >> 6))))
							.Append((char)('0' + (7 & (b >> 3))))
							.Append((char)('0' + (7 & b)));
				}
			}
			res.Append("'");

			return res.ToString();
		}

		//internal string ToArray<T>(T[] source)
		//{
		//	var values = new StringBuilder();
		//	foreach (var value in source)
		//	{
		//		if (values.Length > 0) values.Append(",");
		//		values.Append(base.GetQuotedValue(value, typeof(T)));
		//	}
		//	return "ARRAY[" + values + "]";
		//}

		public override String QuoteSchemaName(String schemaName)
		{
			//if (schemaName.IsNullOrWhiteSpace())
			//	schemaName = "public";
			//return base.QuoteSchemaName(schemaName);
			return schemaName.IsNullOrWhiteSpace() ? "public" : base.QuoteSchemaName(schemaName);
		}

		//internal String UnQuoteSchemaName(String quoted)
		//{
		//	//if (string.IsNullOrEmpty(quoted))
		//	//	return "public";
		//	//return UnQuote(quoted);
		//	return quoted.IsNullOrWhiteSpace() ? "public" : UnQuote(quoted);
		//}
	}
}