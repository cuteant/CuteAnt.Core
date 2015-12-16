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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqliteGenerator : GenericGenerator<SqliteColumn, SqliteTypeMap, SqliteQuoter, StandardDescriptionGenerator>
	{
		#region -- 构造 --

		internal SqliteGenerator()
		{
		}

		#endregion

		#region -- SQL语句定义 --

		internal override String RenameTableSQLTemplate { get { return "ALTER TABLE {0} RENAME TO {1}"; } }

		#endregion

		#region -- SQL语句生成 --

		#region - Column -

		internal override String AlterColumnSQL(String schemaName, IDataColumn column)
		{
			return _CompatabilityMode.HandleCompatabilty("Sqlite does not support alter column");
		}

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return _CompatabilityMode.HandleCompatabilty("Sqlite does not support renaming of columns");
		}

		internal override String DropColumnSQL(String schemaName, String tableName, String columnName)
		{
			return _CompatabilityMode.HandleCompatabilty("Sqlite does not support deleting of columns");
		}

		internal override Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn)
		{
			// 先判断数据类型
			if (entityColumn.DbType == dbColumn.DbType) { return false; }

			switch (entityColumn.DbType)
			{
				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
				case CommonDbType.Text:
				case CommonDbType.Xml:
				case CommonDbType.Json:
					if (dbColumn.DataType == typeof(String)) { return false; }
					break;

				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					if (dbColumn.DataType == typeof(Byte[])) { return false; }
					break;

				case CommonDbType.Boolean:
					if (dbColumn.DataType == typeof(Int64)) { return false; }
					break;

				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
					if (dbColumn.DataType == typeof(CombGuid)) { return false; }
					if (dbColumn.RawType.EqualIgnoreCase("UNIQUEIDENTIFIER", "GUID")) { return false; }
					break;

				case CommonDbType.Guid:
					if (dbColumn.DataType == typeof(Guid)) { return false; }
					if ("CHAR".EqualIgnoreCase(dbColumn.RawType) && dbColumn.Length == 36) { return false; }
					break;
				case CommonDbType.Guid32Digits:
					if (dbColumn.DataType == typeof(Guid)) { return false; }
					if ("CHAR".EqualIgnoreCase(dbColumn.RawType) && dbColumn.Length == 32) { return false; }
					break;

				case CommonDbType.Date:
				case CommonDbType.DateTime:
				case CommonDbType.DateTime2:
					if (dbColumn.DataType == typeof(DateTime)) { return false; }
					break;
				case CommonDbType.DateTimeOffset:
					if (dbColumn.DataType == typeof(DateTimeOffset) || dbColumn.DataType == typeof(DateTime)) { return false; }
					break;
				case CommonDbType.Time:
					if (dbColumn.DataType == typeof(Int64) || dbColumn.DataType == typeof(TimeSpan)) { return false; }
					if (dbColumn.RawType.EqualIgnoreCase("INTEGER")) { return false; }
					break;

				case CommonDbType.Currency:
				case CommonDbType.Decimal:
					if (dbColumn.DataType == typeof(Decimal)) { return false; }
					if (dbColumn.RawType.EqualIgnoreCase("NUMERIC", "NUMBER", "CURRENCY", "DECIMAL", "MONEY", "REAL")) { return false; }
					break;
				case CommonDbType.Double:
					if (dbColumn.DataType == typeof(Double)) { return false; }
					if (dbColumn.RawType.EqualIgnoreCase("REAL", "NUMERIC", "NUMBER", "CURRENCY", "DECIMAL", "DOUBLE", "FLOAT", "MONEY")) { return false; }
					break;
				case CommonDbType.Float:
					if (dbColumn.DataType == typeof(Single)) { return false; }
					if (dbColumn.RawType.EqualIgnoreCase("REAL", "NUMERIC", "NUMBER", "CURRENCY", "DECIMAL", "DOUBLE", "FLOAT", "MONEY", "SINGLE")) { return false; }
					break;

				case CommonDbType.TinyInt:
					if (dbColumn.DataType == typeof(Byte) ||
							dbColumn.DataType == typeof(Int16) || dbColumn.DataType == typeof(UInt16) ||
							dbColumn.DataType == typeof(Int32) || dbColumn.DataType == typeof(UInt32) ||
							dbColumn.DataType == typeof(Int64) || dbColumn.DataType == typeof(UInt64)) { return false; }
					break;
				case CommonDbType.SignedTinyInt:
					if (dbColumn.DataType == typeof(SByte) || dbColumn.DataType == typeof(Int16) || dbColumn.DataType == typeof(Int32) || dbColumn.DataType == typeof(Int64)) { return false; }
					break;
				case CommonDbType.SmallInt:
					if (dbColumn.DataType == typeof(Int16) || dbColumn.DataType == typeof(Int32) || dbColumn.DataType == typeof(Int64)) { return false; }
					break;
				case CommonDbType.Integer:
					if (dbColumn.DataType == typeof(Int32) || dbColumn.DataType == typeof(Int64)) { return false; }
					break;
				case CommonDbType.BigInt:
					if (dbColumn.DataType == typeof(Int64)) { return false; }
					break;

				case CommonDbType.Unknown:
				default:
					return false;
			}
			return true;
		}

		#endregion

		#region - Constraint -

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			return _CompatabilityMode.HandleCompatabilty("Sqlite does not support altering of default constraints");
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			return _CompatabilityMode.HandleCompatabilty("Default constraints are not supported");
		}

		#endregion

		#region - Sequence -

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences are not supported in Sqlite");
		}

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences are not supported in Sqlite");
		}

		#endregion

		#endregion
	}
}