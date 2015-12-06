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
using System.Data;
using System.Linq;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal abstract class GenericGenerator<TColumn, TTypeMap, TQuoter, TDescriptionGenerator> : GeneratorBase<TColumn, TTypeMap, TQuoter, TDescriptionGenerator>
		//where TGenerator : GenericGenerator<TGenerator, TColumn, TTypeMap, TQuoter, TDescriptionGenerator>, new()
		where TColumn : ColumnBase<TTypeMap, TQuoter>, new()
		where TTypeMap : TypeMapBase, new()
		where TQuoter : QuoterBase, new()
		where TDescriptionGenerator : StandardDescriptionGenerator, new()
	{
		#region -- SQL语句定义 --

		#region - DataBase -

		internal virtual String CreateDatabaseSQLTemplate { get { return "CREATE DATABASE {0}"; } }

		internal virtual String DropDatabaseSQLTemplate { get { return "DROP DATABASE {0}"; } }

		#endregion

		#region - Schema -

		internal virtual String CreateSchemaSQLTemplate { get { return "CREATE SCHEMA {0}"; } }

		internal virtual String DropSchemaSQLTemplate { get { return "DROP SCHEMA {0}"; } }

		internal virtual String AlterSchemaSQLTemplate { get { return "ALTER SCHEMA {0} TRANSFER {1}.{2}"; } }

		#endregion

		#region - Table -

		internal virtual String CreateTableSQLTemplate { get { return "CREATE TABLE {0} ({1})"; } }

		internal virtual String DropTableSQLTemplate { get { return "DROP TABLE {0}"; } }

		internal virtual String RenameTableSQLTemplate { get { return "RENAME TABLE {0} TO {1}"; } }

		#endregion

		#region - Column -

		internal virtual String AlterColumnSQLTemplate { get { return "ALTER TABLE {0} ALTER COLUMN {1}"; } }

		internal virtual String AddColumnSQLTemplate { get { return "ALTER TABLE {0} ADD COLUMN {1}"; } }

		internal virtual String DropColumnSQLTemplate { get { return "ALTER TABLE {0} DROP COLUMN {1}"; } }

		internal virtual String RenameColumnSQLTemplate { get { return "ALTER TABLE {0} RENAME COLUMN {1} TO {2}"; } }

		#endregion

		#region - Index -

		internal virtual String CreateIndexSQLTemplate { get { return "CREATE {0}{1}INDEX {2} ON {3} ({4})"; } }

		internal virtual String DropIndexSQLTemplate { get { return "DROP INDEX {0}"; } }

		#endregion

		#region - Constraint -

		internal virtual String CreateConstraintSQLTemplate { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})"; } }

		internal virtual String DeleteConstraintSQLTemplate { get { return "ALTER TABLE {0} DROP CONSTRAINT {1}"; } }

		//internal virtual String CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}"; } }

		#endregion

		#region - Data -

		internal virtual String InsertDataSQLTemplate { get { return "INSERT INTO {0} ({1}) VALUES ({2})"; } }

		internal virtual String UpdateDataSQLTemplate { get { return "UPDATE {0} SET {1}"; } }

		internal virtual String DeleteDataSQLTemplate { get { return "DELETE FROM {0}"; } }

		#endregion

		#endregion

		#region -- 构造 --

		internal CompatabilityMode _CompatabilityMode;

		internal GenericGenerator()
			: base()
		{
			_CompatabilityMode = CompatabilityMode.LOOSE;
		}

		#endregion

		#region -- SQL语句生成 --

		#region - DataBase -

		internal override String CreateDatabaseSQL(String dbName, String dataPath)
		{
			return CreateDatabaseSQLTemplate.FormatWith(Quoter.QuoteDataBaseName(dbName));
		}

		internal override String DropDatabaseSQL(String dbName)
		{
			return DropDatabaseSQLTemplate.FormatWith(Quoter.QuoteDataBaseName(dbName));
		}

		#endregion

		#region - Schema -

		// All Schema method throw by default as only Sql server 2005 and up supports them.
		internal override String CreateSchemaSQL(String schemaName)
		{
			return _CompatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		internal override String DeleteSchemaSQL(String schemaName)
		{
			return _CompatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		internal override String AlterSchemaSQL(String srcSchemaName, String tableName, String destSchemaName)
		{
			return _CompatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		#endregion

		#region - Table -

		internal override String CreateTableSQL(String schemaName, IDataTable table)
		{
			if (table.Columns.Count == 0) { throw new ArgumentException("You must specifiy at least one column"); }

			var quotedTableName = Quoter.QuoteTableName(table.TableName);

			return String.Format(CreateTableSQLTemplate, quotedTableName, Column.Generate(table));
		}

		internal override String DropTableSQL(String schemaName, String tableName)
		{
			return String.Format(DropTableSQLTemplate, Quoter.QuoteTableName(tableName));
		}

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			return String.Format(RenameTableSQLTemplate, Quoter.QuoteTableName(oldName), Quoter.QuoteTableName(newName));
		}

		#endregion

		#region - Column -

		internal override String CreateColumnSQL(String schemaName, IDataColumn column)
		{
			return String.Format(AddColumnSQLTemplate, Quoter.QuoteTableName(column.Table.TableName), Column.Generate(column));
		}

		internal override String AlterColumnSQL(String schemaName, IDataColumn column)
		{
			return String.Format(AlterColumnSQLTemplate, Quoter.QuoteTableName(column.Table.TableName), Column.Generate(column));
		}

		internal override String DropColumnSQL(String schemaName, String tableName, String columnName)
		{
			return String.Format(DropColumnSQLTemplate, Quoter.QuoteTableName(tableName), Quoter.QuoteColumnName(columnName));
		}

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return String.Format(RenameColumnSQLTemplate,
					Quoter.QuoteTableName(tableName),
					Quoter.QuoteColumnName(oldName),
					Quoter.QuoteColumnName(newName));
		}

		#endregion

		#region - Index -

		internal override String CreateIndexSQL(IndexDefinition index)
		{
			if (index.Columns.Count <= 0) { return String.Empty; }

			var indexColumns = String.Join(", ", index.Columns.Select(c => c.IsDescending ? Quoter.QuoteColumnName(c.Name) + " DESC" : Quoter.QuoteColumnName(c.Name)));

			return String.Format(CreateIndexSQLTemplate,
													 GetUniqueString(index),
													 GetClusterTypeString(index),
													 Quoter.QuoteIndexName(index.Name),
													 Quoter.QuoteTableName(index.TableName),
													 indexColumns);
		}

		internal override String DeleteIndexSQL(String schemaName, String tableName, String indexName)
		{
			return String.Format(DropIndexSQLTemplate, Quoter.QuoteIndexName(indexName), Quoter.QuoteTableName(tableName));
		}

		#endregion

		#region - Constraint -

		internal override String CreateConstraintSQL(ConstraintDefinition constraint)
		{
			var constraintType = (constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

			var columns = String.Join(", ", constraint.Columns.Select(e => Quoter.QuoteColumnName(e)));

			return String.Format(CreateConstraintSQLTemplate, Quoter.QuoteTableName(constraint.TableName),
						 Quoter.QuoteConstraintName(constraint.ConstraintName),
						 constraintType,
						 columns);
		}

		internal override String DeleteConstraintSQL(String schemaName, String tableName, String constraintName, Boolean isPrimaryKey)
		{
			return String.Format(DeleteConstraintSQLTemplate, Quoter.QuoteTableName(tableName), Quoter.QuoteConstraintName(constraintName));
		}

		#endregion

		#region - Sequence -

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			var result = new StringBuilder("CREATE SEQUENCE ");
			if (sequence.SchemaName.IsNullOrWhiteSpace())
			{
				result.Append(Quoter.QuoteSequenceName(sequence.Name));
			}
			else
			{
				result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(sequence.SchemaName), Quoter.QuoteSequenceName(sequence.Name));
			}

			if (sequence.Increment.HasValue)
			{
				result.AppendFormat(" INCREMENT {0}", sequence.Increment);
			}

			if (sequence.MinValue.HasValue)
			{
				result.AppendFormat(" MINVALUE {0}", sequence.MinValue);
			}

			if (sequence.MaxValue.HasValue)
			{
				result.AppendFormat(" MAXVALUE {0}", sequence.MaxValue);
			}

			if (sequence.StartWith.HasValue)
			{
				result.AppendFormat(" START WITH {0}", sequence.StartWith);
			}

			if (sequence.Cache.HasValue)
			{
				result.AppendFormat(" CACHE {0}", sequence.Cache);
			}

			if (sequence.Cycle)
			{
				result.Append(" CYCLE");
			}

			return result.ToString();
		}

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			var result = new StringBuilder("DROP SEQUENCE ");
			if (schemaName.IsNullOrWhiteSpace())
			{
				result.Append(Quoter.QuoteSequenceName(sequenceName));
			}
			else
			{
				result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteSequenceName(sequenceName));
			}

			return result.ToString();
		}

		#endregion

		#region - Data -

		internal override String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, StringBuilder values)
		{
			return String.Format(InsertDataSQLTemplate, Quoter.QuoteTableName(tableName), columns, values);
		}

		internal override String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, List<StringBuilder> values)
		{
			var sb = new StringBuilder(10240); // 10 kb
			sb.AppendFormat("INSERT INTO {0} ({1}) {2}", Quoter.QuoteTableName(tableName), columns, Environment.NewLine);
			var count = values.Count;
			for (int i = 0; i < count; i++)
			{
				sb.Append("\t");
				sb.Append("SELECT ");
				sb.AppendLine(values[i].ToString());
				if (i < count - 1)
				{
					sb.Append("\t");
					sb.AppendLine("UNION ALL ");
				}
			}
			return sb.ToString();
		}

		internal override String UpdateDataSQL(String schemaName, String tableName, String setClause, String whereClause)
		{
			var sb = new StringBuilder();
			sb.AppendFormat(UpdateDataSQLTemplate, Quoter.QuoteTableName(tableName), setClause);
			if (!whereClause.IsNullOrWhiteSpace())
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause);
			}
			return sb.ToString();
		}

		internal override String DeleteDataSQL(String schemaName, String tableName, String whereClause)
		{
			var sb = new StringBuilder();
			sb.AppendFormat(DeleteDataSQLTemplate, Quoter.QuoteTableName(tableName));
			if (!whereClause.IsNullOrWhiteSpace())
			{
				sb.Append(" WHERE ");
				sb.Append(whereClause);
			}
			return sb.ToString();
		}

		#endregion

		#endregion

		#region -- 辅助 --

		internal virtual String GetUniqueString(IndexDefinition index)
		{
			return index.IsUnique ? "UNIQUE " : String.Empty;
		}

		internal virtual String GetClusterTypeString(IndexDefinition index)
		{
			return String.Empty;
		}

		internal String FormatCascade(String onWhat, Rule rule)
		{
			var action = "NO ACTION";
			switch (rule)
			{
				case Rule.None:
					return "";

				case Rule.Cascade:
					action = "CASCADE";
					break;

				case Rule.SetNull:
					action = "SET NULL";
					break;

				case Rule.SetDefault:
					action = "SET DEFAULT";
					break;
			}

			return String.Format(" ON {0} {1}", onWhat, action);
		}

		#endregion
	}
}