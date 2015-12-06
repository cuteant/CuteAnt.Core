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
using System.Linq;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlServer2005Generator : SqlServer2005Generator<SqlServer2005TypeMap, SqlServer2000Quoter, SqlServer2005DescriptionGenerator> { }

	internal class SqlServer2005Generator<TTypeMap, TQuoter, TDescriptionGenerator> : SqlServer2000Generator<TTypeMap, TQuoter, TDescriptionGenerator>
		where TTypeMap : SqlServer2005TypeMap, new()
		where TQuoter : SqlServer2000Quoter, new()
		where TDescriptionGenerator : SqlServer2005DescriptionGenerator, new()
	{
		#region -- 构造 --

		internal SqlServer2005Generator()
			: base() { }

		#endregion

		#region -- SQL语句定义 --

		internal override String CreateTableSQLTemplate { get { return "{0} ({1})"; } }

		internal override String DropTableSQLTemplate { get { return "{0}"; } }

		internal override String AddColumnSQLTemplate { get { return "{0} ADD {1}"; } }

		internal override String AlterColumnSQLTemplate { get { return "{0} ALTER COLUMN {1}"; } }

		internal override String RenameColumnSQLTemplate { get { return "{0}.{1}', N'{2}', 'COLUMN'"; } }

		internal override String RenameTableSQLTemplate { get { return "{0}', N'{1}', 'OBJECT'"; } }

		internal override String CreateIndexSQLTemplate { get { return "CREATE {0}{1}INDEX {2} ON {3}.{4} ({5}{6}{7})"; } }

		internal override String DropIndexSQLTemplate { get { return "DROP INDEX {0} ON {1}.{2}"; } }

		internal override String InsertDataSQLTemplate { get { return "INSERT INTO {0}.{1} ({2}) VALUES ({3})"; } }

		internal override String UpdateDataSQLTemplate { get { return "UPDATE {0}.{1} SET {2}"; } }

		internal override String DeleteDataSQLTemplate { get { return "DELETE FROM {0}.{1}"; } }

		internal override String IdentityInsertSQLTemplate { get { return "SET IDENTITY_INSERT {0}.{1} {2}"; } }

		//internal override String CreateForeignKeyConstraint { get { return "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5} ({6}){7}{8}"; } }

		internal override String CreateConstraintSQLTemplate { get { return "{0} ADD CONSTRAINT {1} {2}{3} ({4})"; } }

		internal override String DeleteConstraintSQLTemplate { get { return "{0} DROP CONSTRAINT {1}"; } }

		#endregion

		#region -- SQL语句生成 --

		#region - Schema -

		internal override String CreateSchemaSQL(String schemaName)
		{
			return String.Format(CreateSchemaSQLTemplate, Quoter.QuoteSchemaName(schemaName));
		}

		internal override String DeleteSchemaSQL(String schemaName)
		{
			return String.Format(DropSchemaSQLTemplate, Quoter.QuoteSchemaName(schemaName));
		}

		internal override String AlterSchemaSQL(String srcSchemaName, String tableName, String destSchemaName)
		{
			return String.Format(AlterSchemaSQLTemplate, Quoter.QuoteSchemaName(destSchemaName), Quoter.QuoteSchemaName(srcSchemaName), Quoter.QuoteTableName(tableName));
		}

		#endregion

		#region - Table -

		internal override String CreateTableSQL(String schemaName, IDataTable table)
		{
			//var descriptionStatements = DescriptionGenerator.GenerateCreateTableDescriptionStatements(table);
			//var createTableStatement = String.Format("CREATE TABLE {0}.{1}", Quoter.QuoteSchemaName(table.Owner), base.CreateTableSQL(table));
			//var descriptionStatementsArray = descriptionStatements as String[] ?? descriptionStatements.ToArray();

			//if (!descriptionStatementsArray.Any()) { return createTableStatement; }

			//return ComposeStatements(createTableStatement, descriptionStatementsArray);
			var sb = new StringBuilder();
			sb.AppendFormat("CREATE TABLE {0}.{1}", Quoter.QuoteSchemaName(schemaName), base.CreateTableSQL(schemaName, table));
			var descriptionStatements = DescriptionGenerator.GenerateCreateTableDescriptionStatements(schemaName, table);
			if (descriptionStatements.Any())
			{
				sb.AppendLine(";");
				var count = descriptionStatements.Count;
				for (int i = 0; i < count; i++)
				{
					sb.Append(descriptionStatements[i]);
					if (i < count - 1)
					{
						sb.AppendLine("; ");
					}
				}
			}
			return sb.ToString();
		}

		internal override String AlterTableSQL(String schemaName, IDataTable table)
		{
			var descriptionStatement = DescriptionGenerator.GenerateAlterTableDescriptionStatement(schemaName, table);

			if (descriptionStatement.IsNullOrWhiteSpace())
			{
				return base.AlterTableSQL(schemaName, table);
			}

			return descriptionStatement;
		}

		internal override String DropTableSQL(String schemaName, String tableName)
		{
			return String.Format("DROP TABLE {0}.{1}", Quoter.QuoteSchemaName(schemaName), base.DropTableSQL(schemaName, tableName));
		}

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			return String.Format("EXECUTE sp_rename N'{0}.{1}", Quoter.QuoteSchemaName(schemaName), base.RenameTableSQL(schemaName, oldName, newName));
		}

		#endregion

		#region - Column -

		internal override String CreateColumnSQL(String schemaName, IDataColumn column)
		{
			//var alterTableStatement = String.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(column.Table.Owner), base.CreateColumnSQL(column));
			//var descriptionStatement = DescriptionGenerator.GenerateCreateColumnDescriptionStatement(column);

			//if (descriptionStatement.IsNullOrWhiteSpace()) { return alterTableStatement; }

			//return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
			var sb = new StringBuilder();
			sb.AppendFormat("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(schemaName), base.CreateColumnSQL(schemaName, column));

			var descriptionStatement = DescriptionGenerator.GenerateCreateColumnDescriptionStatement(schemaName, column);
			if (descriptionStatement.IsNullOrWhiteSpace()) { return sb.ToString(); }

			sb.AppendLine(";");
			sb.Append(descriptionStatement);

			return sb.ToString();
		}

		internal override String AlterColumnSQL(String schemaName, IDataColumn column)
		{
			//var alterTableStatement = String.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(column.Table.Owner), base.AlterColumnSQL(column));
			//var descriptionStatement = DescriptionGenerator.GenerateAlterColumnDescriptionStatement(column);

			//if (descriptionStatement.IsNullOrWhiteSpace()) { return alterTableStatement; }

			//return ComposeStatements(alterTableStatement, new[] { descriptionStatement });
			var sb = new StringBuilder();
			sb.AppendFormat("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(schemaName), base.AlterColumnSQL(schemaName, column));

			var descriptionStatement = DescriptionGenerator.GenerateAlterColumnDescriptionStatement(schemaName, column);
			if (descriptionStatement.IsNullOrWhiteSpace()) { return sb.ToString(); }

			sb.AppendLine(";");
			sb.Append(descriptionStatement);

			return sb.ToString();
		}

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return String.Format("EXECUTE sp_rename N'{0}.{1}", Quoter.QuoteSchemaName(schemaName), base.RenameColumnSQL(schemaName, tableName, oldName, newName));
		}

		internal override String DropColumnSQL(String schemaName, String tableName, String columnName)
		{
			// before we drop a column, we have to drop any default value constraints in SQL Server
			var builder = new StringBuilder();

			builder.AppendLine(DeleteDefaultConstraintSQL(schemaName, tableName, columnName));
			//builder.AppendLine();

			//builder.AppendLine("-- now we can finally drop column");
			builder.AppendFormat("ALTER TABLE {2}.{0} DROP COLUMN {1};",
													 Quoter.QuoteTableName(tableName),
													 Quoter.QuoteColumnName(columnName),
													 Quoter.QuoteSchemaName(schemaName));

			return builder.ToString();
		}

		#endregion

		#region - Index -

		internal virtual String GetIncludeString(IndexDefinition index)
		{
			return index.Includes.Count > 0 ? ") INCLUDE (" : String.Empty;
		}

		internal override String CreateIndexSQL(IndexDefinition index)
		{
			if (index.Columns.Count <= 0) { return String.Empty; }

			var indexColumns = String.Join(", ", index.Columns.Select(c => c.IsDescending ? Quoter.QuoteColumnName(c.Name) + " DESC" : Quoter.QuoteColumnName(c.Name)));

			var indexIncludes = String.Empty;
			if (index.Includes.Count > 0)
			{
				indexIncludes = String.Join(", ", index.Includes.Select(c => Quoter.QuoteColumnName(c)));
			}
			return String.Format(CreateIndexSQLTemplate
					, GetUniqueString(index)
					, GetClusterTypeString(index)
					, Quoter.QuoteIndexName(index.Name)
					, Quoter.QuoteSchemaName(index.SchemaName)
					, Quoter.QuoteTableName(index.TableName)
					, indexColumns
					, GetIncludeString(index)
					, indexIncludes);
		}

		internal override String DeleteIndexSQL(String schemaName, String tableName, String indexName)
		{
			return String.Format(DropIndexSQLTemplate, Quoter.QuoteIndexName(indexName), Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName));
		}

		#endregion

		#region - Data -

		#region Insert

		internal override String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, StringBuilder values)
		{
			return String.Format(InsertDataSQLTemplate, Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), columns, values);
		}

		#endregion

		#region Update

		internal override String UpdateDataSQL(String schemaName, String tableName, String setClause, String whereClause)
		{
			var sb = new StringBuilder();
			sb.AppendFormat(UpdateDataSQLTemplate, Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), setClause);
			if (!whereClause.IsNullOrWhiteSpace())
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause);
			}
			return sb.ToString();
		}

		#endregion

		#region Delete

		internal override String DeleteDataSQL(String schemaName, String tableName, String whereClause)
		{
			var sb = new StringBuilder();
			sb.AppendFormat(DeleteDataSQLTemplate, Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName));
			if (!whereClause.IsNullOrWhiteSpace())
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause);
			}
			return sb.ToString();
		}

		#endregion

		#endregion

		#region - Constraint -

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			// before we alter a default constraint on a column, we have to drop any default value constraints in SQL Server
			var builder = new StringBuilder();

			builder.AppendLine(DeleteDefaultConstraintSQL(schemaName, tableName, columnName));

			//builder.AppendLine();
			//builder.AppendLine("-- create alter table command to create new default constraint as string and run it");
			builder.AppendFormat("ALTER TABLE {3}.{0} WITH NOCHECK ADD CONSTRAINT {4} DEFAULT({2}) FOR {1};",
					Quoter.QuoteTableName(tableName),
					Quoter.QuoteColumnName(columnName),
					Column.FormatDefaultValue(defaultValue),
					Quoter.QuoteSchemaName(schemaName),
					Quoter.QuoteConstraintName(SqlServerColumn<TTypeMap, TQuoter>.GetDefaultConstraintName(tableName, columnName)));

			return builder.ToString();
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			//String sql =
			//		"DECLARE @default sysname, @sql nvarchar(max); " + Environment.NewLine + Environment.NewLine +
			//		"-- get name of default constraint" + Environment.NewLine +
			//		"SELECT @default = name" + Environment.NewLine +
			//		"FROM sys.default_constraints" + Environment.NewLine +
			//		"WHERE parent_object_id = object_id('{2}.{0}')" + Environment.NewLine +
			//		"AND type = 'D'" + Environment.NewLine +
			//		"AND parent_column_id = (" + Environment.NewLine +
			//		"SELECT column_id" + Environment.NewLine +
			//		"FROM sys.columns" + Environment.NewLine +
			//		"WHERE object_id = object_id('{2}.{0}')" + Environment.NewLine +
			//		"AND name = '{1}'" + Environment.NewLine +
			//		"); " + Environment.NewLine + Environment.NewLine +
			//		"-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
			//		"SET @sql = N'ALTER TABLE {2}.{0} DROP CONSTRAINT ' + @default;" + Environment.NewLine +
			//		"EXEC sp_executesql @sql;";
			//return String.Format(sql, Quoter.QuoteTableName(tableName), columnName, Quoter.QuoteSchemaName(schemaName));
			var quoteTableName = Quoter.QuoteTableName(tableName);
			var quoteSchemaName = Quoter.QuoteSchemaName(schemaName);
			var sb = new StringBuilder(600);
			sb.AppendLine("DECLARE @default sysname, @sql nvarchar(max); ");
			sb.AppendLine("SELECT @default = name ");
			sb.AppendLine("FROM sys.default_constraints ");
			sb.AppendFormat("WHERE parent_object_id = object_id('{1}.{0}') ", quoteTableName, quoteSchemaName);
			sb.AppendLine("AND type = 'D' ");
			sb.AppendLine("AND parent_column_id = (");
			sb.AppendLine("SELECT column_id ");
			sb.AppendLine("FROM sys.columns ");
			sb.AppendFormat("WHERE object_id = object_id('{1}.{0}') ", quoteTableName, quoteSchemaName);
			sb.AppendFormat("AND name = '{0}'", columnName);
			sb.AppendLine("); ");
			sb.AppendFormat("SET @sql = N'ALTER TABLE {1}.{0} DROP CONSTRAINT ' + @default; ", quoteTableName, quoteSchemaName);
			sb.AppendLine("EXEC sp_executesql @sql;");

			return sb.ToString();
		}

		internal override String CreateConstraintSQL(ConstraintDefinition constraint)
		{
			return String.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(constraint.SchemaName), base.CreateConstraintSQL(constraint));
		}

		internal override String DeleteConstraintSQL(String schemaName, String tableName, String constraintName, Boolean isPrimaryKey)
		{
			return String.Format("ALTER TABLE {0}.{1}", Quoter.QuoteSchemaName(schemaName), base.DeleteConstraintSQL(schemaName, tableName, constraintName, isPrimaryKey));
		}

		#endregion

		#endregion

		#region -- 辅助 --

		//private String ComposeStatements(String ddlStatement, IEnumerable<String> otherStatements)
		//{
		//	var otherStatementsArray = otherStatements.ToArray();

		//	var statementsBuilder = new StringBuilder();
		//	statementsBuilder.AppendLine(ddlStatement);
		//	statementsBuilder.AppendLine("GO");
		//	statementsBuilder.AppendLine(String.Join(";", otherStatementsArray));

		//	return statementsBuilder.ToString();
		//}

		#endregion
	}
}