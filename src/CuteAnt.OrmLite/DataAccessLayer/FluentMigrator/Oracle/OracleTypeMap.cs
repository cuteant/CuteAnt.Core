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
	internal class OracleTypeMap : TypeMapBase
	{
		private const Int32 AnsiStringCapacity = 2000;
		private const Int32 AnsiTextCapacity = 4000;
		private const Int32 MaximumAnsiTextCapacity = Int32.MaxValue;
		private const Int32 UnicodeStringCapacity = 2000;
		private const Int32 UnicodeTextCapacity = 4000;
		private const Int32 MaximumUnicodeTextCapacity = Int32.MaxValue;
		private const Int32 BlobCapacity = Int32.MaxValue;
		private const Int32 DecimalCapacity = 38;
		private const Int32 XmlCapacity = 1073741823;

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
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR(255 CHAR)");
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR($size CHAR)", AnsiStringCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "VARCHAR2($size CHAR)", AnsiTextCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CLOB", MaximumAnsiTextCapacity);

			SetTypeMap(CommonDbType.AnsiString, "VARCHAR2(255 CHAR)");
			SetTypeMap(CommonDbType.AnsiString, "VARCHAR2($size CHAR)", AnsiTextCapacity);
			SetTypeMap(CommonDbType.AnsiString, "CLOB", MaximumAnsiTextCapacity);

			SetTypeMap(CommonDbType.StringFixedLength, "NCHAR(255)");
			SetTypeMap(CommonDbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(CommonDbType.StringFixedLength, "NVARCHAR2($size)", UnicodeTextCapacity);
			SetTypeMap(CommonDbType.StringFixedLength, "NCLOB", MaximumUnicodeTextCapacity);

			SetTypeMap(CommonDbType.String, "NVARCHAR2(255)");
			SetTypeMap(CommonDbType.String, "NVARCHAR2($size)", UnicodeTextCapacity);
			SetTypeMap(CommonDbType.String, "NCLOB", MaximumUnicodeTextCapacity);

			SetTypeMap(CommonDbType.BinaryFixedLength, "RAW(2000)");
			SetTypeMap(CommonDbType.BinaryFixedLength, "RAW($size)", AnsiStringCapacity);
			//SetTypeMap(CommonDbType.BinaryFixedLength, "RAW(MAX)", MaximumAnsiTextCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "BLOB", BlobCapacity);

			SetTypeMap(CommonDbType.Binary, "RAW(2000)");
			SetTypeMap(CommonDbType.Binary, "RAW($size)", AnsiStringCapacity);
			//SetTypeMap(CommonDbType.Binary, "RAW(MAX)", MaximumAnsiTextCapacity);
			SetTypeMap(CommonDbType.Binary, "BLOB", BlobCapacity);

			SetTypeMap(CommonDbType.Boolean, "NUMBER(1,0)");

			SetTypeMap(CommonDbType.CombGuid, "RAW(16)");
			SetTypeMap(CommonDbType.CombGuid32Digits, "RAW(16)");

			SetTypeMap(CommonDbType.Guid, "RAW(16)");
			SetTypeMap(CommonDbType.Guid32Digits, "RAW(16)");

			SetTypeMap(CommonDbType.Date, "DATE");
			
			SetTypeMap(CommonDbType.DateTime, "TIMESTAMP(3)");
			SetTypeMap(CommonDbType.DateTime2, "TIMESTAMP(7)");
			SetTypeMap(CommonDbType.DateTimeOffset, "TIMESTAMP(7) WITH TIME ZONE");
			// 时间类型，统一使用长整形
			SetTypeMap(CommonDbType.Time, "NUMBER(19,0)");

			SetTypeMap(CommonDbType.TinyInt, "NUMBER(3,0)");
			SetTypeMap(CommonDbType.SignedTinyInt, "NUMBER(3,0)");

			SetTypeMap(CommonDbType.Currency, "NUMBER(19,4)");

			SetTypeMap(CommonDbType.Decimal, "NUMBER(19,5)");
			SetTypeMap(CommonDbType.Decimal, "NUMBER($precision,$scale)", DecimalCapacity);

			SetTypeMap(CommonDbType.Double, "DOUBLE PRECISION"); //10g => BINARY_DOUBLE

			SetTypeMap(CommonDbType.SmallInt, "NUMBER(5,0)");

			SetTypeMap(CommonDbType.Integer, "NUMBER(10,0)");

			SetTypeMap(CommonDbType.BigInt, "NUMBER(19,0)");

			SetTypeMap(CommonDbType.Float, "FLOAT(24)"); // 10g => BINARY_FLOAT

			SetTypeMap(CommonDbType.Text, "NCLOB");
			SetTypeMap(CommonDbType.Json, "NCLOB");
			SetTypeMap(CommonDbType.Xml, "XMLTYPE");
		}
	}
}