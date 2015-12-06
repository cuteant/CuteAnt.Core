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
	internal class SqlServer2000TypeMap : TypeMapBase
	{
		internal const Int32 AnsiStringCapacity = 8000;
		//internal const Int32 AnsiTextCapacity = Int32.MaxValue;
		internal const Int32 UnicodeStringCapacity = 4000;
		//internal const Int32 UnicodeTextCapacity = 1073741823;
		//internal const Int32 ImageCapacity = Int32.MaxValue;
		internal const Int32 DecimalCapacity = 38;

		internal override String GetTypeMap(CommonDbType type, Int32 size, Int32 precision, Int32 scale)
		{
			if (type == CommonDbType.Decimal)
			{
				// 精度范围为1～38，小数位范围是0～38，但不得超过M。
				var p = precision > DecimalCapacity ? DecimalCapacity : precision;
				var s = scale > p ? p : scale;
				return base.GetTypeMap(type, size, p, s);
			}
			return base.GetTypeMap(type, size, precision, scale);
		}

		internal override void SetupTypeMaps()
		{
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "TEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.AnsiString, "VARCHAR(255)");
			SetTypeMap(CommonDbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.AnsiString, "TEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.StringFixedLength, "NCHAR(255)");
			SetTypeMap(CommonDbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(CommonDbType.StringFixedLength, "NTEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.String, "NVARCHAR(255)");
			SetTypeMap(CommonDbType.String, "NVARCHAR($size)", UnicodeStringCapacity);
			// Officially this is 1073741823 but we will allow the int.MaxValue Convention
			SetTypeMap(CommonDbType.String, "NTEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.BinaryFixedLength, "BINARY(8000)");
			SetTypeMap(CommonDbType.BinaryFixedLength, "BINARY($size)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "IMAGE", Int32.MaxValue);

			SetTypeMap(CommonDbType.Binary, "VARBINARY(8000)");
			SetTypeMap(CommonDbType.Binary, "VARBINARY($size)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.Binary, "IMAGE", Int32.MaxValue);

			SetTypeMap(CommonDbType.Boolean, "BIT");

			SetTypeMap(CommonDbType.CombGuid, "UNIQUEIDENTIFIER");
			SetTypeMap(CommonDbType.CombGuid32Digits, "UNIQUEIDENTIFIER"); // CHAR(32)

			SetTypeMap(CommonDbType.Guid, "UNIQUEIDENTIFIER");
			SetTypeMap(CommonDbType.Guid32Digits, "UNIQUEIDENTIFIER"); // CHAR(32)

			SetTypeMap(CommonDbType.Date, "DATETIME");
			SetTypeMap(CommonDbType.DateTime, "DATETIME");
			SetTypeMap(CommonDbType.DateTime2, "DATETIME");
			SetTypeMap(CommonDbType.DateTimeOffset, "DATETIME");
			// 时间类型，统一使用长整形
			SetTypeMap(CommonDbType.Time, "BIGINT");

			SetTypeMap(CommonDbType.Currency, "MONEY");

			SetTypeMap(CommonDbType.Decimal, "DECIMAL(19,5)");
			SetTypeMap(CommonDbType.Decimal, "DECIMAL($precision,$scale)", DecimalCapacity);

			SetTypeMap(CommonDbType.TinyInt, "TINYINT");
			SetTypeMap(CommonDbType.SignedTinyInt, "SMALLINT");

			SetTypeMap(CommonDbType.SmallInt, "SMALLINT");

			SetTypeMap(CommonDbType.Integer, "INT");

			SetTypeMap(CommonDbType.BigInt, "BIGINT");

			SetTypeMap(CommonDbType.Float, "REAL");

			SetTypeMap(CommonDbType.Double, "FLOAT");

			SetTypeMap(CommonDbType.Text, "NTEXT");
			SetTypeMap(CommonDbType.Xml, "XML");
			SetTypeMap(CommonDbType.Json, "NTEXT");
		}
	}
}