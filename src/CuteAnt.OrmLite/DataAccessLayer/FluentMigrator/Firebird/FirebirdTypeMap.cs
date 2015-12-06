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
	internal class FirebirdTypeMap : TypeMapBase
	{
		private const Int32 DecimalCapacity = 18;
		private const Int32 FirebirdMaxVarcharSize = 32765;
		private const Int32 FirebirdMaxCharSize = 32767;
		// http://www.firebirdsql.org/en/firebird-technical-specifications/
		//private const Int32 FirebirdMaxTextSize = Int32.MaxValue;  // as close as Int32 can come to 32GB

		internal override String GetTypeMap(CommonDbType type, Int32 size, Int32 precision, Int32 scale)
		{
			if (type == CommonDbType.String || type == CommonDbType.StringFixedLength)
			{
				// UTF-8
				return base.GetTypeMap(type, size * 3, precision, scale);
			}
			else if (type == CommonDbType.Decimal)
			{
				var p = precision > DecimalCapacity ? DecimalCapacity : precision;
				var s = scale > p ? p : scale;
				return base.GetTypeMap(type, size, p, s);
			}
			return base.GetTypeMap(type, size, precision, scale);
		}

		internal override void SetupTypeMaps()
		{
			/*
			 * Values were taken from the Interbase 6 Data Definition Guide
			 * 
			 * INTEGER:长整型，取值范围：-2147483648至2147483647
			 * 
			 * FLOAT:单精度浮点型，取值范围：1.175*10[-38]至3.402*10[38]
			 * 
			 * DOUBLE PRECISION：双精度浮点型，取值范围：2.225*10[-308]至1.797*10[308]
			 * 
			 * DECIMAL：小数型，可指定有效位数最大为18位或小数点后18位。比如DECIMAL(5,2)，就是指有5位数字，不含小数点，形如123.45
			 * 
			 * NUMERIC：小数型，与DECIMAL类似，稍后讲它们的区别。
			 * 
			 * 注意，当数据含小数部分时，请尽量用DECIMAL，因为浮点型有精度问题，除非数值特别大，才用浮点型！
			 * 
			 * DECIMAL与NUMERIC的区别：
			 * 比如，DECIMAL(5,2)与NUMERIC(5,2)所分别定义的字段，DECIMAL(5,2)指的是至少有5位数字，还可以更多！而NUMERIC(5,2)指的是，就是5位，不多也不少。
			 * 
			 * */

			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR($size)", FirebirdMaxCharSize);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "BLOB SUB_TYPE TEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.AnsiString, "VARCHAR(255)");
			SetTypeMap(CommonDbType.AnsiString, "VARCHAR($size)", FirebirdMaxVarcharSize);
			SetTypeMap(CommonDbType.AnsiString, "BLOB SUB_TYPE TEXT", Int32.MaxValue);

			// Unicode支持，节省空间统一使用变长字符类型
			SetTypeMap(CommonDbType.StringFixedLength, "VARCHAR(255)");
			SetTypeMap(CommonDbType.StringFixedLength, "VARCHAR($size)", FirebirdMaxCharSize);
			SetTypeMap(CommonDbType.StringFixedLength, "BLOB SUB_TYPE TEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.String, "VARCHAR(255)");
			SetTypeMap(CommonDbType.String, "VARCHAR($size)", FirebirdMaxVarcharSize);
			SetTypeMap(CommonDbType.String, "BLOB SUB_TYPE TEXT", Int32.MaxValue);

			SetTypeMap(CommonDbType.BinaryFixedLength, "BLOB SUB_TYPE BINARY");
			SetTypeMap(CommonDbType.BinaryFixedLength, "BLOB SUB_TYPE BINARY", Int32.MaxValue);

			SetTypeMap(CommonDbType.Binary, "BLOB SUB_TYPE BINARY");
			SetTypeMap(CommonDbType.Binary, "BLOB SUB_TYPE BINARY", Int32.MaxValue);

			SetTypeMap(CommonDbType.Boolean, "BOOLEAN");

			SetTypeMap(CommonDbType.CombGuid, "CHAR(16) CHARACTER SET OCTETS");
			SetTypeMap(CommonDbType.CombGuid32Digits, "CHAR(16) CHARACTER SET OCTETS");

			SetTypeMap(CommonDbType.Guid, "VARCHAR(36)");
			SetTypeMap(CommonDbType.Guid32Digits, "VARCHAR(32)");

			SetTypeMap(CommonDbType.Date, "DATE");

			SetTypeMap(CommonDbType.DateTime, "TIMESTAMP");
			SetTypeMap(CommonDbType.DateTime2, "TIMESTAMP");
			SetTypeMap(CommonDbType.DateTimeOffset, "TIMESTAMP");
			// 时间类型，统一使用长整形
			SetTypeMap(CommonDbType.Time, "BIGINT");

			SetTypeMap(CommonDbType.Currency, "DECIMAL(18,4)");

			SetTypeMap(CommonDbType.Decimal, "DECIMAL(14,5)");
			SetTypeMap(CommonDbType.Decimal, "DECIMAL($precision,$scale)", DecimalCapacity);

			SetTypeMap(CommonDbType.TinyInt, "SMALLINT");
			SetTypeMap(CommonDbType.SignedTinyInt, "SMALLINT");

			SetTypeMap(CommonDbType.SmallInt, "SMALLINT");

			SetTypeMap(CommonDbType.Integer, "INTEGER");

			SetTypeMap(CommonDbType.BigInt, "BIGINT");

			SetTypeMap(CommonDbType.Float, "FLOAT");

			SetTypeMap(CommonDbType.Double, "DOUBLE PRECISION"); //64 bit double precision

			SetTypeMap(CommonDbType.Text, "BLOB SUB_TYPE TEXT");
			SetTypeMap(CommonDbType.Xml, "BLOB SUB_TYPE TEXT");
			SetTypeMap(CommonDbType.Json, "BLOB SUB_TYPE TEXT");
		}
	}
}