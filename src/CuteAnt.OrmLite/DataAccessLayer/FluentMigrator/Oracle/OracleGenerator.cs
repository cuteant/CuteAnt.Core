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
using CuteAnt.OrmLite.Common;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class OracleGenerator : OracleGenerator<OracleQuoter> { }

	internal class OracleGeneratorQuotedIdentifier : OracleGenerator<OracleQuoterQuotedIdentifier> { }

	internal class OracleGenerator<TQuoter> : GenericGenerator<OracleColumn<TQuoter>, OracleTypeMap, TQuoter, OracleDescriptionGenerator>
		where TQuoter : QuoterBase, new()
	{
		#region -- 构造 --

		internal OracleGenerator()
			: base() { }

		#endregion

		#region -- SQL语句定义 --

		internal override String RenameTableSQLTemplate
		{
			get { return "ALTER TABLE {0} RENAME TO {1}"; }
		}

		internal override String AddColumnSQLTemplate
		{
			get { return "ALTER TABLE {0} ADD {1}"; }
		}

		internal override String AlterColumnSQLTemplate
		{
			get { return "ALTER TABLE {0} MODIFY {1}"; }
		}

		//internal override String InsertDataSQLTemplate
		//{
		//	get { return "INTO {0} ({1}) VALUES ({2})"; }
		//}

		#endregion

		#region -- SQL语句生成 --

		#region - Table -

		private string innerGenerate(String schemaName, IDataTable table)
		{
			return String.Format(CreateTableSQLTemplate, ExpandTableName(schemaName, table.TableName), Column.Generate(table));
		}

		internal override String CreateTableSQL(String schemaName, IDataTable table)
		{
			var descriptionStatements = DescriptionGenerator.GenerateCreateTableDescriptionStatements(schemaName, table);
			var statements = descriptionStatements as string[] ?? descriptionStatements.ToArray();

			if (!statements.Any()) { return innerGenerate(schemaName, table); }

			var wrappedCreateTableStatement = WrapStatementInExecuteImmediateBlock(innerGenerate(schemaName, table));
			var createTableWithDescriptionsBuilder = new StringBuilder(wrappedCreateTableStatement);

			foreach (var descriptionStatement in statements)
			{
				if (!descriptionStatement.IsNullOrWhiteSpace())
				{
					var wrappedStatement = WrapStatementInExecuteImmediateBlock(descriptionStatement);
					createTableWithDescriptionsBuilder.Append(wrappedStatement);
				}
			}

			return WrapInBlock(createTableWithDescriptionsBuilder.ToString());
		}

		internal override String AlterTableSQL(String schemaName, IDataTable table)
		{
			var descriptionStatement = DescriptionGenerator.GenerateAlterTableDescriptionStatement(schemaName, table);

			if (descriptionStatement.IsNullOrWhiteSpace()) { return base.AlterTableSQL(schemaName, table); }

			return descriptionStatement;
		}

		internal override String DropTableSQL(String schemaName, String tableName)
		{
			return String.Format(DropTableSQLTemplate, ExpandTableName(schemaName, tableName));
		}

		#endregion

		#region - Column -

		internal override String CreateColumnSQL(String schemaName, IDataColumn column)
		{
			var descriptionStatement = DescriptionGenerator.GenerateCreateColumnDescriptionStatement(schemaName, column);

			if (descriptionStatement.IsNullOrWhiteSpace()) { return base.CreateColumnSQL(schemaName, column); }

			var wrappedCreateColumnStatement = WrapStatementInExecuteImmediateBlock(base.CreateColumnSQL(schemaName, column));

			var createColumnWithDescriptionBuilder = new StringBuilder(wrappedCreateColumnStatement);
			createColumnWithDescriptionBuilder.Append(WrapStatementInExecuteImmediateBlock(descriptionStatement));

			return WrapInBlock(createColumnWithDescriptionBuilder.ToString());
		}

		internal override String AlterColumnSQL(String schemaName, IDataColumn column)
		{
			var descriptionStatement = DescriptionGenerator.GenerateAlterColumnDescriptionStatement(schemaName, column);

			if (descriptionStatement.IsNullOrWhiteSpace()) { return base.AlterColumnSQL(schemaName, column); }

			var wrappedAlterColumnStatement = WrapStatementInExecuteImmediateBlock(base.AlterColumnSQL(schemaName, column));

			var alterColumnWithDescriptionBuilder = new StringBuilder(wrappedAlterColumnStatement);
			alterColumnWithDescriptionBuilder.Append(WrapStatementInExecuteImmediateBlock(descriptionStatement));

			return WrapInBlock(alterColumnWithDescriptionBuilder.ToString());
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

		#region - Constraint -

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			// 苦竹 修改 屏蔽异常
			//throw new NotImplementedException();
			return _CompatabilityMode.HandleCompatabilty("Default constraints are not supported");
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			return _CompatabilityMode.HandleCompatabilty("Default constraints are not supported");
		}

		#endregion

		#region - Sequence -

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			var result = new StringBuilder(string.Format("CREATE SEQUENCE "));
			if (sequence.SchemaName.IsNullOrWhiteSpace())
			{
				result.AppendFormat(Quoter.QuoteSequenceName(sequence.Name));
			}
			else
			{
				result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(sequence.SchemaName), Quoter.QuoteSequenceName(sequence.Name));
			}

			if (sequence.Increment.HasValue)
			{
				result.AppendFormat(" INCREMENT BY {0}", sequence.Increment);
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

		#endregion

		#region - Data -

		//		internal override String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, StringBuilder values, Boolean isUsingIdentityInsert)
		//		{
		//			var columnNames = new List<String>();
		//			var columnValues = new List<String>();
		//			var insertStrings = new List<String>();

		//			foreach (InsertionDataDefinition row in expression.Rows)
		//			{
		//				columnNames.Clear();
		//				columnValues.Clear();
		//				foreach (KeyValuePair<string, object> item in row)
		//				{
		//					columnNames.Add(Quoter.QuoteColumnName(item.Key));
		//					columnValues.Add(Quoter.QuoteValue(item.Value));
		//				}

		//				var columns = String.Join(", ", columnNames);
		//				var values = String.Join(", ", columnValues);
		//				insertStrings.Add(String.Format(InsertDataSQLTemplate, Quoter.QuoteTableName(expression.TableName), columns, values));
		//			}
		//			return "INSERT ALL " + String.Join(" ", insertStrings) + " SELECT 1 FROM DUAL";
		//		}

		#endregion

		#endregion

		#region -- 辅助 --

		private String ExpandTableName(String schemaName, String tableName)
		{
			return schemaName.IsNullOrWhiteSpace() ? Quoter.QuoteTableName(tableName) : String.Concat(Quoter.QuoteSchemaName(schemaName), ".", Quoter.QuoteTableName(tableName));
		}

		private String WrapStatementInExecuteImmediateBlock(string statement)
		{
			if (statement.IsNullOrWhiteSpace()) { return String.Empty; }

			return String.Format("EXECUTE IMMEDIATE '{0}';", Helper.FormatSqlEscape(statement));
		}

		private String WrapInBlock(String sql)
		{
			if (sql.IsNullOrWhiteSpace()) { return String.Empty; }

			return String.Format("BEGIN {0} END;", sql);
		}

		#endregion
	}
}