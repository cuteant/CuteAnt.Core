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
	internal class PostgresGenerator : GenericGenerator<PostgresColumn, PostgresTypeMap, PostgresQuoter, PostgresDescriptionGenerator>
	{
		#region -- 构造 --

		internal PostgresGenerator()
		{
		}

		#endregion

		#region -- SQL语句定义 --

		internal override String AlterSchemaSQLTemplate { get { return "ALTER TABLE {0}.{1} SET SCHEMA {2}"; } }

		internal override String InsertDataSQLTemplate { get { return "INSERT INTO {0}.{1} ({2}) VALUES ({3})"; } }

		internal override String UpdateDataSQLTemplate { get { return "UPDATE {0}.{1} SET {2}"; } }

		internal override String DeleteDataSQLTemplate { get { return "DELETE FROM {0}.{1}"; } }

		internal override String CreateConstraintSQLTemplate { get { return "ALTER TABLE {0}.{1} ADD CONSTRAINT {2} {3} ({4})"; } }

		internal override String DeleteConstraintSQLTemplate { get { return "ALTER TABLE {0}.{1} DROP CONSTRAINT {2}"; } }

		internal override String DropIndexSQLTemplate { get { return "DROP INDEX {0}.{1}"; } }

		#endregion

		#region -- SQL语句生成 --

		#region - DataBase -

		internal override String CreateDatabaseSQL(String dbName, String dataPath)
		{
			/*
			 * CREATE DATABASE name
			 *     [ [ WITH ] [ OWNER [=] user_name ]
			 *            [ TEMPLATE [=] template ]
			 *            [ ENCODING [=] encoding ]
			 *            [ LC_COLLATE [=] lc_collate ]
			 *            [ LC_CTYPE [=] lc_ctype ]
			 *            [ TABLESPACE [=] tablespace ]
			 *            [ CONNECTION LIMIT [=] connlimit ] ]
			 * 
			 * CREATE DATABASE {0} WITH ENCODING 'UTF8' LC_COLLATE='ko_KR.euckr' LC_CTYPE='ko_KR.euckr' TEMPLATE=template0
			 */
			return "CREATE DATABASE {0} WITH ENCODING 'UTF8' TEMPLATE=template0".FormatWith(Quoter.QuoteDataBaseName(dbName));
		}

		#endregion

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
			return String.Format(AlterSchemaSQLTemplate, Quoter.QuoteSchemaName(srcSchemaName), Quoter.QuoteTableName(tableName), Quoter.QuoteSchemaName(destSchemaName));
		}

		#endregion

		#region - Table -

		internal override String CreateTableSQL(String schemaName, IDataTable table)
		{
			//var tableName = Quoter.QuoteTableName(table.TableName);
			//return String.Format("CREATE TABLE {0}.{1} ({2})", Quoter.QuoteSchemaName(schemaName), tableName, Column.Generate(table));
			var createStatement = new StringBuilder();
			var tableName = Quoter.QuoteTableName(table.TableName);
			createStatement.Append(string.Format("CREATE TABLE {0}.{1} ({2})", Quoter.QuoteSchemaName(schemaName), tableName, Column.Generate(table)));
			var descriptionStatement = DescriptionGenerator.GenerateCreateTableDescriptionStatements(schemaName, table);
			if (descriptionStatement != null && descriptionStatement.Any())
			{
				createStatement.Append(";");
				createStatement.Append(string.Join(";", descriptionStatement.ToArray()));
			}
			return createStatement.ToString();
		}

		internal override String DropTableSQL(String schemaName, String tableName)
		{
			return String.Format("DROP TABLE {0}.{1}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName));
		}

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			return String.Format("ALTER TABLE {0}.{1} RENAME TO {2}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(oldName), Quoter.QuoteTableName(newName));
		}

		internal override string AlterTableSQL(String schemaName, IDataTable table)
		{
			var alterStatement = new StringBuilder();
			var descriptionStatement = DescriptionGenerator.GenerateAlterTableDescriptionStatement(schemaName, table);
			alterStatement.Append(base.AlterTableSQL(schemaName, table));
			if (String.IsNullOrEmpty(descriptionStatement))
			{
				alterStatement.Append(descriptionStatement);
			}
			return alterStatement.ToString();
		}

		#endregion

		#region - Column -

		internal override String AlterColumnSQL(String schemaName, IDataColumn column)
		{
			var table = column.Table;
			var alterStatement = new StringBuilder();
			alterStatement.Append(String.Format("ALTER TABLE {0}.{1} {2}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(table.TableName), Column.GenerateAlterClauses(column)));
			var descriptionStatement = DescriptionGenerator.GenerateAlterColumnDescriptionStatement(schemaName, column);
			if (!String.IsNullOrEmpty(descriptionStatement))
			{
				alterStatement.Append(";");
				alterStatement.Append(descriptionStatement);
			}
			return alterStatement.ToString();
		}

		internal override String CreateColumnSQL(String schemaName, IDataColumn column)
		{
			var table = column.Table;
			var createStatement = new StringBuilder();
			createStatement.Append(String.Format("ALTER TABLE {0}.{1} ADD {2}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(table.TableName), Column.Generate(column)));
			var descriptionStatement = DescriptionGenerator.GenerateCreateColumnDescriptionStatement(schemaName, column);
			if (!String.IsNullOrEmpty(descriptionStatement))
			{
				createStatement.Append(";");
				createStatement.Append(descriptionStatement);
			}
			return createStatement.ToString();
		}

		internal override String DropColumnSQL(String schemaName, String tableName, String columnName)
		{
			return String.Format("ALTER TABLE {0}.{1} DROP COLUMN {2}",
					Quoter.QuoteSchemaName(schemaName),
					Quoter.QuoteTableName(tableName),
					Quoter.QuoteColumnName(columnName));
		}

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return String.Format("ALTER TABLE {0}.{1} RENAME COLUMN {2} TO {3}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), Quoter.QuoteColumnName(oldName), Quoter.QuoteColumnName(newName));
		}

		internal override Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn)
		{
			// 先判断数据类型
			if (entityColumn.DbType == dbColumn.DbType) { return false; }

			//// 类型不匹配，不一定就是有改变，还要查找类型对照表是否有匹配的，只要存在任意一个匹配，就说明是合法的
			//foreach (var item in FieldTypeMaps)
			//{
			//	//if (entityColumn.DataType == item.Key && dbColumn.DataType == item.Value) { return false; }
			//	// 把不常用的类型映射到常用类型，比如数据库SByte映射到实体类Byte，UInt32映射到Int32，而不需要重新修改数据库
			//	if (dbColumn.DataType == item.Key && entityColumn.DataType == item.Value) { return false; }
			//}
			switch (entityColumn.DbType)
			{
				case CommonDbType.AnsiString:
					break;
				case CommonDbType.AnsiStringFixedLength:
					break;
				case CommonDbType.String:
					break;
				case CommonDbType.StringFixedLength:
					break;

				case CommonDbType.Binary:
					break;
				case CommonDbType.BinaryFixedLength:
					break;

				case CommonDbType.Boolean:
					break;

				case CommonDbType.CombGuid:
					break;
				case CommonDbType.CombGuid32Digits:
					break;

				case CommonDbType.Guid:
					break;
				case CommonDbType.Guid32Digits:
					break;

				case CommonDbType.Date:
					break;
				case CommonDbType.DateTime:
					break;
				case CommonDbType.DateTime2:
					break;
				case CommonDbType.DateTimeOffset:
					break;
				case CommonDbType.Time:
					break;

				case CommonDbType.Currency:
					break;
				case CommonDbType.Decimal:
					break;

				case CommonDbType.TinyInt:
					break;
				case CommonDbType.SignedTinyInt:
					break;
				case CommonDbType.SmallInt:
					break;
				case CommonDbType.Integer:
					break;
				case CommonDbType.BigInt:
					break;
				case CommonDbType.Double:
					break;
				case CommonDbType.Float:
					break;
				case CommonDbType.Text:
					break;

				case CommonDbType.Xml:
					break;
				case CommonDbType.Json:
					break;
				case CommonDbType.Unknown:
				default:
					break;
			}
			return true;
		}

		#endregion

		#region - Index -

		internal override String CreateIndexSQL(IndexDefinition index)
		{
			if (index.Columns.Count <= 0) { return String.Empty; }

			var result = new StringBuilder("CREATE");
			if (index.IsUnique) { result.Append(" UNIQUE"); }

			result.AppendFormat(" INDEX {0} ON {1}.{2} (", Quoter.QuoteIndexName(index.Name), Quoter.QuoteSchemaName(index.SchemaName), Quoter.QuoteTableName(index.TableName));

			var first = true;
			foreach (var column in index.Columns)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					result.Append(",");
				}

				result.Append("\"" + column.Name + "\"");
				//result.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC");
				if (column.IsDescending) { result.Append(" DESC"); }
			}
			result.Append(")");

			return result.ToString();

			/*
			var idx = String.Format(result.ToString(), expression.Index.Name, Quoter.QuoteSchemaName(expression.Index.SchemaName), expression.Index.TableName);
			if (!expression.Index.IsClustered)
					return idx;

			 // Clustered indexes in Postgres do not cluster updates/inserts to the table after the initial cluster operation is applied.
			 // To keep the clustered index up to date run CLUSTER TableName periodically

			return String.Format("{0}; CLUSTER {1}\"{2}\" ON \"{3}\"", idx, Quoter.QuoteSchemaName(expression.Index.SchemaName), expression.Index.TableName, expression.Index.Name);
			 */
		}

		internal override String DeleteIndexSQL(String schemaName, String tableName, String indexName)
		{
			return String.Format(DropIndexSQLTemplate, Quoter.QuoteSchemaName(schemaName), Quoter.QuoteIndexName(indexName));
		}

		#endregion

		#region - Constraint -

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			return String.Format("ALTER TABLE {0}.{1} ALTER {2} DROP DEFAULT, ALTER {2} {3}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), Quoter.QuoteColumnName(columnName), Column.FormatAlterDefaultValue(columnName, defaultValue));
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			return String.Format("ALTER TABLE {0}.{1} ALTER {2} DROP DEFAULT", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), Quoter.Quote(columnName));
		}

		internal override String DeleteConstraintSQL(String schemaName, String tableName, String constraintName, Boolean isPrimaryKey)
		{
			return String.Format(DeleteConstraintSQLTemplate, Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), Quoter.Quote(constraintName));
		}

		internal override String CreateConstraintSQL(ConstraintDefinition constraint)
		{
			var constraintType = (constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

			var columns = String.Join(", ", constraint.Columns.Select(e => Quoter.QuoteColumnName(e)));

			return String.Format(CreateConstraintSQLTemplate, Quoter.QuoteSchemaName(constraint.SchemaName),
					Quoter.QuoteTableName(constraint.TableName),
					Quoter.QuoteConstraintName(constraint.ConstraintName),
					constraintType,
					columns);
		}

		#endregion

		#region - Data -

		internal override String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, StringBuilder values)
		{
			return String.Format(InsertDataSQLTemplate, Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName), columns, values);
		}

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

		#region -- 辅助 --

		internal String GetColumnList(IEnumerable<string> columns)
		{
			var result = "";
			foreach (var column in columns)
			{
				result += Quoter.QuoteColumnName(column) + ",";
			}
			return result.TrimEnd(',');
		}

		//internal String GetDataList(List<object> data)
		//{
		//	var result = "";
		//	foreach (var column in data)
		//	{
		//		result += Quoter.QuoteValue(column) + ",";
		//	}
		//	return result.TrimEnd(',');
		//}

		#endregion
	}
}