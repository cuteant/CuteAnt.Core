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
using System.Collections.Generic;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class MySql55Generator : MySqlGenerator<MySql55TypeMap, MySql55Quoter> { }

	internal class MySql56Generator : MySqlGenerator<MySql56TypeMap, MySql56Quoter> { }

	internal class MySqlGenerator<TTypeMap, TQuoter> : GenericGenerator<MySqlColumn<TTypeMap, TQuoter>, TTypeMap, TQuoter, StandardDescriptionGenerator>
		where TTypeMap : MySql55TypeMap, new()
		where TQuoter : MySql55Quoter, new()
	{
		#region -- 构造 --

		internal MySqlGenerator()
		{
		}

		#endregion

		#region -- SQL语句定义 --

		internal override String CreateDatabaseSQLTemplate { get { return "CREATE DATABASE IF NOT EXISTS {0}"; } }

		internal override String DropDatabaseSQLTemplate { get { return "DROP DATABASE IF EXISTS {0}"; } }

		internal override String CreateTableSQLTemplate { get { return "CREATE TABLE {0} ({1}) ENGINE=INNODB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci"; } } //  AUTO_INCREMENT=1

		internal override String AlterColumnSQLTemplate { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }

		internal override String DropIndexSQLTemplate { get { return "DROP INDEX {0} ON {1}"; } }

		internal override String DeleteConstraintSQLTemplate { get { return "ALTER TABLE {0} DROP {1}{2}"; } }

		//internal override String DeleteConstraint { get { return "ALTER TABLE {0} DROP FOREIGN KEY {1}"; } }

		#endregion

		#region -- SQL语句生成 --

		#region - Column -

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return String.Format("ALTER TABLE {0} CHANGE {1} {2}", Quoter.QuoteTableName(tableName), Quoter.QuoteColumnName(oldName), Quoter.QuoteColumnName(newName));
		}

		internal override Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn)
		{
			// 先判断数据类型
			if (entityColumn.DbType == dbColumn.DbType)
			{
				if (entityColumn.DbType == CommonDbType.Decimal)
				{
					if (entityColumn.Precision > MySql55TypeMap.DecimalCapacity) { return false; } // 无效精度
					if (dbColumn.Precision >= entityColumn.Precision && dbColumn.Scale >= entityColumn.Scale) { return false; }
					return true;
				}
				else if (entityColumn.DbType == CommonDbType.Float)
				{
					if (entityColumn.Precision > MySql55TypeMap.FloatCapacity) { return false; } // 无效精度
					if (dbColumn.Precision >= entityColumn.Precision && dbColumn.Scale >= entityColumn.Scale) { return false; }
					return true;
				}
				else if (entityColumn.DbType == CommonDbType.Double)
				{
					if (entityColumn.Precision > MySql55TypeMap.DoubleCapacity) { return false; } // 无效精度
					if (dbColumn.Precision >= entityColumn.Precision && dbColumn.Scale >= entityColumn.Scale) { return false; }
					return true;
				}
				else if (entityColumn.DbType == CommonDbType.DateTime || entityColumn.DbType == CommonDbType.DateTime2 || entityColumn.DbType == CommonDbType.DateTimeOffset)
				{
					//if (entityColumn.Precision > 6) { return false; } // 无效精度
					//if (dbColumn.Precision >= entityColumn.Precision && dbColumn.Scale >= entityColumn.Scale) { return false; }
					//return true;
					// 不再比较精度，5.64以上版本才支持
					return false;
				}
				else if (entityColumn.DbType == CommonDbType.String || entityColumn.DbType == CommonDbType.StringFixedLength || entityColumn.DbType == CommonDbType.AnsiString || entityColumn.DbType == CommonDbType.AnsiStringFixedLength)
				{
					return dbColumn.Length >= entityColumn.Length ? false : true;
				}
				else // 其他数据类型
				{
					return false;
				}
			}

			switch (entityColumn.DbType)
			{
				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
					if (dbColumn.DataType == typeof(String))
					{
						return (dbColumn.Length >= entityColumn.Length) ? false : true;
					}
					return true;

				case CommonDbType.Xml:
				case CommonDbType.Json:
				case CommonDbType.Text:
					return (dbColumn.DataType == typeof(String)) ? false : true;

				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					if (dbColumn.DataType == typeof(Byte[]))
					{
						return (dbColumn.Length >= entityColumn.Length) ? false : true;
					}
					return true;

				case CommonDbType.Boolean:
					// 支持 tinyint(1,0) 或varchar类型("true", "y", "yes", "1"；"false", "n", "no", "0")
					return (dbColumn.DbType == CommonDbType.TinyInt || dbColumn.DbType == CommonDbType.SignedTinyInt || dbColumn.DbType == CommonDbType.String) ? false : true;

				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
					return (dbColumn.DataType == typeof(CombGuid) || dbColumn.DataType == typeof(Guid)) ? false : true;

				case CommonDbType.Guid:
				case CommonDbType.Guid32Digits:
					return (dbColumn.DataType == typeof(Guid)) ? false : true;

				case CommonDbType.DateTime2:
				case CommonDbType.DateTimeOffset:
					return (dbColumn.DataType == typeof(DateTime)) ? false : true;

				case CommonDbType.Time:
					return (dbColumn.DataType == typeof(Int64) || dbColumn.DataType == typeof(TimeSpan)) ? false : true;

				case CommonDbType.SignedTinyInt:
					return (dbColumn.DbType == CommonDbType.SmallInt || dbColumn.DbType == CommonDbType.Integer || dbColumn.DbType == CommonDbType.BigInt) ? false : true;

				case CommonDbType.Currency:
					return (dbColumn.DataType == typeof(Decimal)) ? false : true;

				case CommonDbType.Date:
				case CommonDbType.DateTime:

				case CommonDbType.Decimal:

				case CommonDbType.TinyInt:
				case CommonDbType.SmallInt:
				case CommonDbType.Integer:
				case CommonDbType.BigInt:
				case CommonDbType.Double:
				case CommonDbType.Float:
					return true;

				case CommonDbType.Unknown:
				default:
					return false;
			}
		}

		#endregion

		#region - Constraint -

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			return _CompatabilityMode.HandleCompatabilty("Altering of default constraints is not supporteed for MySql");
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			return _CompatabilityMode.HandleCompatabilty("Default constraints are not supported");
		}

		internal override String DeleteConstraintSQL(String schemaName, String tableName, String constraintName, Boolean isPrimaryKey)
		{
			if (isPrimaryKey)
			{
				return String.Format(DeleteConstraintSQLTemplate, Quoter.QuoteTableName(tableName), "PRIMARY KEY", "");
			}
			return String.Format(DeleteConstraintSQLTemplate, Quoter.QuoteTableName(tableName), "INDEX ", Quoter.Quote(constraintName));
		}

		#endregion

		#region - Sequence -

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences is not supporteed for MySql");
		}

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences is not supporteed for MySql");
		}

		#endregion

		#region - Data -

		internal override String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, List<StringBuilder> values)
		{
			var sb = new StringBuilder(1024);
			sb.AppendFormat("INSERT INTO {0} ({1}) {2}", Quoter.QuoteTableName(tableName), columns, Environment.NewLine);
			sb.Append("\t");
			sb.AppendLine("VALUES ");
			var count = values.Count;
			for (int i = 0; i < count; i++)
			{
				sb.Append("\t");
				sb.Append("(");
				sb.Append(values[i].ToString());
				sb.Append(")");
				if (i < count - 1)
				{
					sb.AppendLine(",");
				}
			}
			return sb.ToString();
		}

		#endregion

		#endregion
	}
}