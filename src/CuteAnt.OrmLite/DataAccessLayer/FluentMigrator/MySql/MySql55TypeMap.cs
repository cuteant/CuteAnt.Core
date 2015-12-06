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
	/// <summary>MySQL 5.64以下版本</summary>
	internal class MySql55TypeMap : TypeMapBase
	{
		private const Int32 AnsiTinyStringCapacity = 127;
		//private const Int32 StringCapacity = 255;
		private const Int32 TinyTextCapacity = 255;
		// utf-8字符集，每个字符占用1~3个字节，VARCHAR类型最大容量应为 65535 / 3 = 21845，但在Navicat Premium中设置最大为21788，超过就抛异常。
		private const Int32 Utf8TextCapacity = 21788;
		// VARBINARY类型，长度超过65366，SQL抛异常
		private const Int32 VarBinaryCapacity = 65366; // 65535
		private const Int32 BlobCapacity = 65366; // 65535
		// 65366
		private const Int32 MediumTextCapacity = 16777215;
		private const Int32 LongTextCapacity = Int32.MaxValue; // 实际可存储4294967295字节
		internal const Int32 DecimalCapacity = 65;
		internal const Int32 FloatCapacity = 23;
		internal const Int32 DoubleCapacity = 53;
		private const Int32 MaximumScaleCapacity = 30;

		internal override String GetTypeMap(CommonDbType type, Int32 size, Int32 precision, Int32 scale)
		{
			//if (type == CommonDbType.String || type == CommonDbType.StringFixedLength)
			//{
			//	// UTF-8
			//	return base.GetTypeMap(type, size * 3, precision, scale);
			//}
			if (type == CommonDbType.Decimal || type == CommonDbType.Float || type == CommonDbType.Double)
			{
				// 精度范围为1～65，小数位范围是0～30，但不得超过M。
				var p = precision > DecimalCapacity ? DecimalCapacity : precision;
				var s = scale > p ? p : scale;
				s = s > MaximumScaleCapacity ? MaximumScaleCapacity : s;
				return base.GetTypeMap(type, size, p, s);
			}
			else if (type == CommonDbType.Float)
			{
				// 精度范围为0～23，小数位范围是0～23，但不得超过M。
				var p = precision > FloatCapacity ? FloatCapacity : precision;
				var s = scale > p ? p : scale;
				s = s > FloatCapacity ? FloatCapacity : s;
				return base.GetTypeMap(type, size, p, s);
			}
			else if (type == CommonDbType.Double)
			{
				// 精度范围为1～53，小数位范围是0～53，但不得超过M。
				var p = precision > DoubleCapacity ? DoubleCapacity : precision;
				var s = scale > p ? p : scale;
				s = s > DoubleCapacity ? DoubleCapacity : s;
				return base.GetTypeMap(type, size, p, s);
			}
			return base.GetTypeMap(type, size, precision, scale);
		}

		internal override void SetupTypeMaps()
		{
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "CHAR($size)", TinyTextCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "VARCHAR($size)", Utf8TextCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(CommonDbType.AnsiStringFixedLength, "LONGTEXT", LongTextCapacity);

			SetTypeMap(CommonDbType.AnsiString, "VARCHAR(255)");
			SetTypeMap(CommonDbType.AnsiString, "VARCHAR($size)", Utf8TextCapacity);
			SetTypeMap(CommonDbType.AnsiString, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(CommonDbType.AnsiString, "LONGTEXT", LongTextCapacity);

			// Unicode支持，节省空间统一使用变长字符类型
			SetTypeMap(CommonDbType.StringFixedLength, "VARCHAR(255)");
			SetTypeMap(CommonDbType.StringFixedLength, "VARCHAR($size)", Utf8TextCapacity);
			SetTypeMap(CommonDbType.StringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(CommonDbType.StringFixedLength, "LONGTEXT", LongTextCapacity);

			SetTypeMap(CommonDbType.String, "VARCHAR(255)");
			SetTypeMap(CommonDbType.String, "VARCHAR($size)", Utf8TextCapacity);
			SetTypeMap(CommonDbType.String, "MEDIUMTEXT", MediumTextCapacity);
			SetTypeMap(CommonDbType.String, "LONGTEXT", LongTextCapacity);

			SetTypeMap(CommonDbType.BinaryFixedLength, "BINARY(255)");
			SetTypeMap(CommonDbType.BinaryFixedLength, "BINARY($size)", TinyTextCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "VARBINARY($size)", VarBinaryCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "BLOB", BlobCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "MEDIUMBLOB", MediumTextCapacity);
			SetTypeMap(CommonDbType.BinaryFixedLength, "LONGBLOB", LongTextCapacity);

			SetTypeMap(CommonDbType.Binary, "VARBINARY(255)");
			SetTypeMap(CommonDbType.Binary, "VARBINARY($size)", VarBinaryCapacity);
			SetTypeMap(CommonDbType.Binary, "BLOB", BlobCapacity);
			SetTypeMap(CommonDbType.Binary, "MEDIUMBLOB", MediumTextCapacity);
			SetTypeMap(CommonDbType.Binary, "LONGBLOB", LongTextCapacity);

			SetTypeMap(CommonDbType.Boolean, "TINYINT(1)");

			SetTypeMap(CommonDbType.CombGuid, "BINARY(16)");
			SetTypeMap(CommonDbType.CombGuid32Digits, "BINARY(16)");

			SetTypeMap(CommonDbType.Guid, "CHAR(36)");
			SetTypeMap(CommonDbType.Guid32Digits, "CHAR(32)");

			SetTypeMap(CommonDbType.Date, "DATE");

			SetTypeMap(CommonDbType.DateTime, "DATETIME");
			SetTypeMap(CommonDbType.DateTime2, "DATETIME");
			SetTypeMap(CommonDbType.DateTimeOffset, "TIMESTAMP");
			// 时间类型，统一使用长整形
			SetTypeMap(CommonDbType.Time, "BIGINT");

			SetTypeMap(CommonDbType.Currency, "DECIMAL(19,4)");

			// 精度范围为1～65，小数位范围是0～30，但不得超过M。
			SetTypeMap(CommonDbType.Decimal, "DECIMAL(19,5)");
			SetTypeMap(CommonDbType.Decimal, "DECIMAL($precision,$scale)", DecimalCapacity);

			SetTypeMap(CommonDbType.TinyInt, "TINYINT UNSIGNED");
			SetTypeMap(CommonDbType.SignedTinyInt, "TINYINT");

			SetTypeMap(CommonDbType.SmallInt, "SMALLINT");

			SetTypeMap(CommonDbType.Integer, "INTEGER");

			SetTypeMap(CommonDbType.BigInt, "BIGINT");

			SetTypeMap(CommonDbType.Float, "FLOAT");
			SetTypeMap(CommonDbType.Double, "FLOAT($precision,$scale)", FloatCapacity);

			SetTypeMap(CommonDbType.Double, "DOUBLE");
			SetTypeMap(CommonDbType.Double, "DOUBLE($precision,$scale)", DoubleCapacity);

			SetTypeMap(CommonDbType.Text, "LONGTEXT");
			SetTypeMap(CommonDbType.Xml, "LONGTEXT");
			SetTypeMap(CommonDbType.Json, "LONGTEXT");
		}
	}
}