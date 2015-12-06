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
	internal class MySql56Quoter : MySql55Quoter
	{
		///// <summary>转义时间为SQL字符串</summary>
		///// <param name="value">时间值</param>
		///// <returns></returns>
		//public override String FormatDateTime(DateTime value)
		//{
		//	//return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
		//	/*
		//	 * ms not contained in format. MySql ignores ms part anyway
		//	 * 
		//	 * for more details see: http://dev.mysql.com/doc/refman/5.1/en/datetime.html
		//	 */
		//	return "{1}{0:yyyy-MM-dd HH:mm:ss}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
		//}

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="value">时间值</param>
		/// <returns></returns>
		public override String FormatDateTime(DateTime value)
		{
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
			// MySQL 以 TIMESTAMP 类型存储时，会自动以 UTC 格式保存，存储时对当前的时区进行转换，检索时再转换回当前的时区。
			//return "{1}{0:yyyy-MM-dd HH:mm:ss.ffffff}{1}".FormatWith(CultureInfo.InvariantCulture, value, ValueQuote);
			const String iso8601Format = "yyyy-MM-dd HH:mm:ss.ffffff";

			var sb = new StringBuilder(28);
			sb.Append(ValueQuote);
			sb.Append(value.ToString(iso8601Format, CultureInfo.InvariantCulture));
			sb.Append(ValueQuote);

			return sb.ToString();
		}
	}
}