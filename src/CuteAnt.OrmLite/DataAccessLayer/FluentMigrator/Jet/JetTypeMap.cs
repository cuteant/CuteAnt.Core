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
	internal class JetTypeMap : TypeMapBase
	{
		private const Int32 AnsiStringCapacity = 255;
		private const Int32 AnsiTextCapacity = 1073741823;
		private const Int32 UnicodeStringCapacity = 255;
		private const Int32 UnicodeTextCapacity = 1073741823;
		private const Int32 ImageCapacity = Int32.MaxValue;
		private const Int32 DecimalCapacity = 19;

		internal override void SetupTypeMaps()
		{
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "TEXT", AnsiTextCapacity);

			SetTypeMap(CommonDbType.AnsiString, "VARCHAR(255)");
			SetTypeMap(CommonDbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.AnsiString, "TEXT", AnsiTextCapacity);

			// Unicode支持，节省空间统一使用变长字符类型
			SetTypeMap(CommonDbType.StringFixedLength, "VARCHAR(255)");
			SetTypeMap(CommonDbType.StringFixedLength, "VARCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(CommonDbType.StringFixedLength, "TEXT", UnicodeTextCapacity);

			SetTypeMap(CommonDbType.String, "VARCHAR(255)");
			SetTypeMap(CommonDbType.String, "VARCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(CommonDbType.String, "TEXT", UnicodeTextCapacity);

			SetTypeMap(CommonDbType.BinaryFixedLength, "VARBINARY(8000)");
			SetTypeMap(CommonDbType.BinaryFixedLength, "VARBINARY($size)", ImageCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "IMAGE", ImageCapacity);

			SetTypeMap(CommonDbType.Binary, "VARBINARY(8000)");
			SetTypeMap(CommonDbType.Binary, "VARBINARY($size)", ImageCapacity);
			SetTypeMap(CommonDbType.Binary, "IMAGE", ImageCapacity);

			SetTypeMap(CommonDbType.Boolean, "BIT");

			SetTypeMap(CommonDbType.CombGuid, "VARCHAR(36)");
			SetTypeMap(CommonDbType.CombGuid32Digits, "CHAR(32)");

			SetTypeMap(CommonDbType.Guid, "CHAR(36)");
			SetTypeMap(CommonDbType.Guid32Digits, "CHAR(32)");

			SetTypeMap(CommonDbType.Date, "DATETIME");
			SetTypeMap(CommonDbType.DateTime, "DATETIME");
			SetTypeMap(CommonDbType.DateTime2, "DATETIME");
			SetTypeMap(CommonDbType.DateTimeOffset, "DATETIME");
			// 时间类型，统一使用长整形
			SetTypeMap(CommonDbType.Time, "DECIMAL(19,0)");

			SetTypeMap(CommonDbType.Currency, "MONEY");

			SetTypeMap(CommonDbType.Decimal, "DECIMAL(19,5)");
			SetTypeMap(CommonDbType.Decimal, "DECIMAL($precision,$scale)", DecimalCapacity);

			SetTypeMap(CommonDbType.TinyInt, "TINYINT");
			SetTypeMap(CommonDbType.SignedTinyInt, "SMALLINT");

			SetTypeMap(CommonDbType.SmallInt, "SMALLINT");

			SetTypeMap(CommonDbType.Integer, "INTEGER");

			// Jet does not have a 64-bit integer type, thus mapping it to a decimal
			SetTypeMap(CommonDbType.BigInt, "DECIMAL(19,0)");

			SetTypeMap(CommonDbType.Float, "REAL");

			SetTypeMap(CommonDbType.Double, "FLOAT");

			SetTypeMap(CommonDbType.Text, "TEXT");
			SetTypeMap(CommonDbType.Xml, "TEXT");
			SetTypeMap(CommonDbType.Json, "TEXT");
		}
	}
}