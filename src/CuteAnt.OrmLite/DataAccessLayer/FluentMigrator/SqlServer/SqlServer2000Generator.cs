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
using System.IO;
using System.Linq;
using System.Text;
using CuteAnt.IO;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlServer2000Generator : SqlServer2000Generator<SqlServer2000TypeMap, SqlServer2000Quoter, StandardDescriptionGenerator> { }

	internal class SqlServer2000Generator<TTypeMap, TQuoter, TDescriptionGenerator> : GenericGenerator<SqlServerColumn<TTypeMap, TQuoter>, TTypeMap, TQuoter, TDescriptionGenerator>
		where TTypeMap : TypeMapBase, new()
		where TQuoter : SqlServer2000Quoter, new()
		where TDescriptionGenerator : StandardDescriptionGenerator, new()
	{
		#region -- 构造 --

		internal SqlServer2000Generator()
			: base() { }

		#endregion

		#region -- SQL语句定义 --

		internal override String RenameTableSQLTemplate { get { return "EXECUTE sp_rename N'{0}', N'{1}', 'OBJECT'"; } }

		internal override String RenameColumnSQLTemplate { get { return "EXECUTE sp_rename N'{0}.{1}', N'{2}', 'COLUMN'"; } }

		internal override String DropIndexSQLTemplate { get { return "DROP INDEX {1}.{0}"; } }

		internal override String AddColumnSQLTemplate { get { return "ALTER TABLE {0} ADD {1}"; } }

		internal virtual String IdentityInsertSQLTemplate { get { return "SET IDENTITY_INSERT {0} {1}"; } }

		internal override String CreateConstraintSQLTemplate { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2}{3} ({4})"; } }

		#endregion

		#region -- SQL语句生成 --

		#region - DataBase -

		internal override String CreateDatabaseSQL(String dbName, String dataPath)
		{
			if (dataPath.IsNullOrWhiteSpace())
			{
				return String.Format("CREATE DATABASE {0}", Quoter.QuoteDataBaseName(dbName));
			}
			var file = PathHelper.PathCombineFix(dataPath, dbName + ".mdf");

			if (!Path.IsPathRooted(file))
			{
				file = PathHelper.ApplicationStartupPathCombine(file);
			}
			file = new FileInfo(file).FullName;

			var logfile = String.Empty;

			logfile = Path.ChangeExtension(file, ".ldf");
			logfile = new FileInfo(logfile).FullName;

			var dir = Path.GetDirectoryName(file);
			if (!dir.IsNullOrWhiteSpace() && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			var sb = new StringBuilder();
			sb.AppendFormat("CREATE DATABASE {0} ON  PRIMARY", Quoter.QuoteDataBaseName(dbName));
			sb.AppendLine();
			sb.AppendFormat(@"( NAME = N'{0}', FILENAME = N'{1}', SIZE = 10 , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)", dbName, file);
			sb.AppendLine();
			sb.Append("LOG ON ");
			sb.AppendLine();
			sb.AppendFormat(@"( NAME = N'{0}_Log', FILENAME = N'{1}', SIZE = 10 , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)", dbName, logfile);
			sb.AppendLine();
			return sb.ToString();
		}

		internal override String DropDatabaseSQL(String dbName)
		{
			var sb = new StringBuilder();
			sb.AppendLine("use master");
			sb.AppendLine("; ");
			sb.AppendLine("declare   @spid   varchar(20),@dbname   varchar(20)");
			sb.AppendLine("declare   #spid   cursor   for");
			sb.AppendFormat("select   spid=cast(spid   as   varchar(20))   from   master..sysprocesses   where   dbid=db_id('{0}')", dbName);
			sb.AppendLine();
			sb.AppendLine("open   #spid");
			sb.AppendLine("fetch   next   from   #spid   into   @spid");
			sb.AppendLine("while   @@fetch_status=0");
			sb.AppendLine("begin");
			sb.AppendLine("exec('kill   '+@spid)");
			sb.AppendLine("fetch   next   from   #spid   into   @spid");
			sb.AppendLine("end");
			sb.AppendLine("close   #spid");
			sb.AppendLine("deallocate   #spid");
			sb.AppendLine("; ");
			sb.AppendFormat("Drop Database {0}", Quoter.QuoteDataBaseName(dbName));
			return sb.ToString();
		}

		#endregion

		#region - Table -

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			return String.Format(RenameTableSQLTemplate, Quoter.QuoteTableName(Quoter.QuoteCommand(oldName)), Quoter.QuoteCommand(newName));
		}

		#endregion

		#region - Column -

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return String.Format(RenameColumnSQLTemplate, Quoter.QuoteTableName(tableName), Quoter.QuoteColumnName(Quoter.QuoteCommand(oldName)), Quoter.QuoteCommand(newName));
		}

		internal override String DropColumnSQL(String schemaName, String tableName, String columnName)
		{
			// before we drop a column, we have to drop any default value constraints in SQL Server
			var builder = new StringBuilder();

			builder.AppendLine(DeleteDefaultConstraintSQL(schemaName, tableName, columnName));

			//builder.AppendLine("-- now we can finally drop column");
			builder.AppendFormat("ALTER TABLE {0} DROP COLUMN {1};",
													 Quoter.QuoteTableName(tableName),
													 Quoter.QuoteColumnName(columnName));

			return builder.ToString();
		}

		internal override Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn)
		{
			// 先判断数据类型
			if (entityColumn.DbType == dbColumn.DbType)
			{
				if (entityColumn.DbType == CommonDbType.Decimal)
				{
					if (entityColumn.Precision > 38) { return false; } // 无效精度
					if (dbColumn.Precision >= entityColumn.Precision && dbColumn.Scale >= entityColumn.Scale) { return false; }
					return true;
				}
				else if (entityColumn.DbType == CommonDbType.String || entityColumn.DbType == CommonDbType.StringFixedLength)
				{
					// 大于1073741823属于无效长度
					return (dbColumn.Length >= entityColumn.Length || entityColumn.Length > 1073741823) ? false : true;
				}
				else if (entityColumn.DbType == CommonDbType.AnsiString || entityColumn.DbType == CommonDbType.AnsiStringFixedLength)
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
					if (dbColumn.DbType == CommonDbType.String || dbColumn.DbType == CommonDbType.Text)
					{
						// 非Unicode类型字符允许保存在Unicode类型字段中
						if (dbColumn.Length >= entityColumn.Length) { return false; }
						// 1073741823个字符实际存储容量为 1 GB，忽略非Unicode类型字符大于1073741823的情况
						if (entityColumn.Length > 1073741823) { return false; }
					}
					return true;
				case CommonDbType.AnsiStringFixedLength:
					if (dbColumn.DbType == CommonDbType.AnsiString || dbColumn.DbType == CommonDbType.String || dbColumn.DbType == CommonDbType.Text)
					{
						// 非Unicode类型字符允许保存在Unicode类型字段中
						if (dbColumn.Length >= entityColumn.Length) { return false; }
						// 1073741823个字符实际存储容量为 1 GB，忽略非Unicode类型字符大于1073741823的情况
						if (entityColumn.Length > 1073741823) { return false; }
					}
					return true;
				case CommonDbType.String:
					return (dbColumn.DbType == CommonDbType.Text) ? false : true;
				case CommonDbType.StringFixedLength:
					if (dbColumn.DbType == CommonDbType.String || dbColumn.DbType == CommonDbType.Text)
					{
						return (dbColumn.Length >= entityColumn.Length || entityColumn.Length > 1073741823) ? false : true;
					}
					return true;

				case CommonDbType.Xml:
				case CommonDbType.Json:
				case CommonDbType.Text:
					return (dbColumn.DataType == typeof(String) && dbColumn.Length >= 1073741823) ? false : true;

				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					if (dbColumn.DataType == typeof(Byte[]))
					{
						return (dbColumn.Length >= entityColumn.Length || entityColumn.Length > 1073741823) ? false : true;
					}
					return true;

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

				case CommonDbType.Boolean:

				case CommonDbType.Date:
				case CommonDbType.DateTime:

				case CommonDbType.Currency:
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

		#region - Index -

		//Not need for the nonclusted keyword as it is the default mode
		internal override String GetClusterTypeString(IndexDefinition index)
		{
			return index.IsClustered ? "CLUSTERED " : String.Empty;
		}

		#endregion

		#region - Constraint -

		internal override String CreateConstraintSQL(ConstraintDefinition constraint)
		{
			var constraintType = constraint.IsPrimaryKeyConstraint ? "PRIMARY KEY" : "UNIQUE";

			var constraintClustering = String.Empty;
			if (constraint.SqlServerConstraintType.HasValue)
			{
				constraintClustering = constraint.SqlServerConstraintType.Value == SqlServerConstraintType.Clustered ? " CLUSTERED" : " NONCLUSTERED";
			}

			var columns = String.Join(", ", constraint.Columns.Select(x => Quoter.QuoteColumnName(x)));

			return String.Format(CreateConstraintSQLTemplate, Quoter.QuoteTableName(constraint.TableName),
					Quoter.Quote(constraint.ConstraintName),
					constraintType,
					constraintClustering,
					columns);
		}

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			// before we alter a default constraint on a column, we have to drop any default value constraints in SQL Server
			var builder = new StringBuilder();

			builder.AppendLine(DeleteDefaultConstraintSQL(schemaName, tableName, columnName));
			//builder.AppendLine();

			//builder.AppendLine("-- create alter table command to create new default constraint as string and run it");
			builder.AppendFormat("ALTER TABLE {0} WITH NOCHECK ADD CONSTRAINT {3} DEFAULT({2}) FOR {1};",
					Quoter.QuoteTableName(tableName),
					Quoter.QuoteColumnName(columnName),
					Quoter.QuoteValue(defaultValue),
					Quoter.QuoteConstraintName(SqlServerColumn<TTypeMap, TQuoter>.GetDefaultConstraintName(tableName, columnName)));

			return builder.ToString();
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			//String sql =
			//		"DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
			//		"-- get name of default constraint" + Environment.NewLine +
			//		"SELECT @default = name" + Environment.NewLine +
			//		"FROM sys.default_constraints" + Environment.NewLine +
			//		"WHERE parent_object_id = object_id('{0}')" + Environment.NewLine +
			//		"AND type = 'D'" + Environment.NewLine +
			//		"AND parent_column_id = (" + Environment.NewLine +
			//		"SELECT column_id" + Environment.NewLine +
			//		"FROM sys.columns" + Environment.NewLine +
			//		"WHERE object_id = object_id('{0}')" + Environment.NewLine +
			//		"AND name = '{1}'" + Environment.NewLine +
			//		");" + Environment.NewLine + Environment.NewLine +
			//		"-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
			//		"SET @sql = N'ALTER TABLE {0} DROP CONSTRAINT ' + @default;" + Environment.NewLine +
			//		"EXEC sp_executesql @sql;";
			//return String.Format(sql, Quoter.QuoteTableName(tableName), columnName);
			var quotedTableName = Quoter.QuoteTableName(tableName);
			var sb = new StringBuilder(600); // 700
			sb.AppendLine("DECLARE @default sysname, @sql nvarchar(4000); ");
			sb.AppendLine();
			//sb.AppendLine("-- get name of default constraint");
			sb.AppendLine("SELECT @default = name ");
			sb.AppendLine("FROM sys.default_constraints ");
			sb.AppendFormat("WHERE parent_object_id = object_id('{0}') ", quotedTableName);
			sb.AppendLine();
			sb.AppendLine("AND type = 'D' ");
			sb.AppendLine("AND parent_column_id = (");
			sb.AppendLine("SELECT column_id ");
			sb.AppendLine("FROM sys.columns ");
			sb.AppendFormat("WHERE object_id = object_id('{0}') ", quotedTableName);
			sb.AppendLine();
			sb.AppendFormat("AND name = '{0}'", columnName);
			sb.AppendLine();
			sb.AppendLine("); ");
			sb.AppendLine();
			//sb.AppendLine("-- create alter table command to drop constraint as string and run it");
			sb.AppendFormat("SET @sql = N'ALTER TABLE {0} DROP CONSTRAINT ' + @default; ", quotedTableName);
			sb.AppendLine();
			sb.Append("EXEC sp_executesql @sql;");
			return sb.ToString();
		}

		#endregion

		#region - Sequence -

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences are not supported in SqlServer2000");
		}

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences are not supported in SqlServer2000");
		}

		#endregion

		#endregion
	}
}