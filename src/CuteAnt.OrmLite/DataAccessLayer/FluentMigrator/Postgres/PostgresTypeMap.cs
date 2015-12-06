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
using System.Data;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class PostgresTypeMap : TypeMapBase
	{
		private const Int32 DecimalCapacity = 1000;
		private const Int32 PostgresMaxVarcharSize = 1073741823;

		internal override String GetTypeMap(CommonDbType type, Int32 size, Int32 precision, Int32 scale)
		{
			if (type == CommonDbType.Decimal)
			{
				// 精度范围为1～65，小数位范围是0～30，但不得超过M。
				var p = precision > DecimalCapacity ? DecimalCapacity : precision;
				var s = scale > p ? p : scale;
				return base.GetTypeMap(type, size, p, s);
			}
			return base.GetTypeMap(type, size, precision, scale);
		}

		internal override void SetupTypeMaps()
		{
			// PostgreSQL定长字符串性能最差，统一使用变长字符串
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "character varying(255)");
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "character varying($size)", PostgresMaxVarcharSize);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "text", Int32.MaxValue); // character varying

			SetTypeMap(CommonDbType.AnsiString, "character varying(255)");
			SetTypeMap(CommonDbType.AnsiString, "character varying($size)", PostgresMaxVarcharSize);
			SetTypeMap(CommonDbType.AnsiString, "text", Int32.MaxValue); // character varying

			SetTypeMap(CommonDbType.StringFixedLength, "character varying(255)");
			SetTypeMap(CommonDbType.StringFixedLength, "character varying($size)", PostgresMaxVarcharSize);
			SetTypeMap(CommonDbType.StringFixedLength, "text", Int32.MaxValue); // character varying

			SetTypeMap(CommonDbType.String, "character varying(255)");
			SetTypeMap(CommonDbType.String, "character varying($size)", PostgresMaxVarcharSize);
			SetTypeMap(CommonDbType.String, "text", Int32.MaxValue); // character varying

			SetTypeMap(CommonDbType.BinaryFixedLength, "bytea");
			SetTypeMap(CommonDbType.BinaryFixedLength, "bytea", Int32.MaxValue);

			SetTypeMap(CommonDbType.Binary, "bytea");
			SetTypeMap(CommonDbType.Binary, "bytea", Int32.MaxValue);

			SetTypeMap(CommonDbType.Boolean, "boolean");

			SetTypeMap(CommonDbType.CombGuid, "uuid");
			SetTypeMap(CommonDbType.CombGuid32Digits, "uuid");

			SetTypeMap(CommonDbType.Guid, "character varying(36)");
			SetTypeMap(CommonDbType.Guid32Digits, "character varying(32)");

			SetTypeMap(CommonDbType.Date, "date");
			SetTypeMap(CommonDbType.DateTime, "timestamp(3)");
			SetTypeMap(CommonDbType.DateTime2, "timestamp(6)");
			SetTypeMap(CommonDbType.DateTimeOffset, "timestamp(6) with time zone");
			// 时间类型，统一使用长整形
			SetTypeMap(CommonDbType.Time, "bigint");

			SetTypeMap(CommonDbType.TinyInt, "smallint"); //no built-in support for single byte unsigned integers
			SetTypeMap(CommonDbType.SignedTinyInt, "smallint");

			SetTypeMap(CommonDbType.Currency, "money");

			SetTypeMap(CommonDbType.Decimal, "decimal(19,5)");
			SetTypeMap(CommonDbType.Decimal, "decimal($precision,$scale)", DecimalCapacity);

			SetTypeMap(CommonDbType.Double, "double precision"); // float8

			SetTypeMap(CommonDbType.SmallInt, "smallint");
			SetTypeMap(CommonDbType.Integer, "integer");
			SetTypeMap(CommonDbType.BigInt, "bigint");

			SetTypeMap(CommonDbType.Float, "real"); // float4

			SetTypeMap(CommonDbType.Text, "text");
			SetTypeMap(CommonDbType.Xml, "xml");
			SetTypeMap(CommonDbType.Json, "json");
		}
	}
}