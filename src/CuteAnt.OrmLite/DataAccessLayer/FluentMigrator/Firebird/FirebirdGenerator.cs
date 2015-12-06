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
using System.Linq;
using System.Text;
using CuteAnt.Security;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class FirebirdGenerator : GenericGenerator<FirebirdColumn, FirebirdTypeMap, FirebirdQuoter, StandardDescriptionGenerator>
	{
		private const Int32 _MaxNameLength = 31;

		#region -- 构造 --

		private FirebirdTruncator _Truncator;

		private FirebirdOptions _FBOptions;

		internal FirebirdOptions FBOptions
		{
			get { return _FBOptions; }
			set
			{
				_FBOptions = value;
				Column.FBOptions = value;
				_Truncator = new FirebirdTruncator(value.TruncateLongNames);
			}
		}

		internal FirebirdGenerator()
		{
		}

		#endregion

		#region -- SQL语句定义 --

		//It's kind of a hack to mess with system tables, but this is the cleanest and time-tested method to alter the nullable constraint.
		// It's even mentioned in the firebird official FAQ.
		// Currently the only drawback is that the integrity is not checked by the engine, you have to ensure it manually
		private String AlterColumnSetNullableSQLTemplate { get { return "UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = {0} WHERE RDB$RELATION_NAME = {1} AND RDB$FIELD_NAME = {2}"; } }

		private String AlterColumnSetTypeSQLTemplate { get { return "ALTER TABLE {0} ALTER COLUMN {1} TYPE {2}"; } }

		internal override String AddColumnSQLTemplate { get { return "ALTER TABLE {0} ADD {1}"; } }

		internal override String DropColumnSQLTemplate { get { return "ALTER TABLE {0} DROP {1}"; } }

		internal override String RenameColumnSQLTemplate { get { return "ALTER TABLE {0} ALTER COLUMN {1} TO {2}"; } }

		#endregion

		#region -- SQL语句生成 --

		#region - DataBase -

		internal override String CreateDatabaseSQL(String dbName, String dataPath)
		{
			return String.Empty;
		}

		#endregion

		#region - Table -

		//internal override String Generate(CreateTableExpression expression)
		//{
		//	_Truncator.Truncate(expression);
		//	return base.Generate(expression);
		//}

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			//_Truncator.Truncate(expression);
			return _CompatabilityMode.HandleCompatabilty("Rename table is not supported");
		}

		#endregion

		#region - Column -

		#region Alter column generators

		internal virtual String GenerateSetNull(IDataColumn column)
		{
			//_Truncator.Truncate(column);
			//return String.Format(AlterColumnSetNullableSQLTemplate,
			//		!column.IsNullable.HasValue || !column.IsNullable.Value ? "NULL" : "1",
			//		Quoter.QuoteValue(column.TableName),
			//		Quoter.QuoteValue(column.Name)
			//		);
			return String.Format(AlterColumnSetNullableSQLTemplate,
					!column.Nullable ? "NULL" : "1",
					Quoter.QuoteValue(column.Table.TableName),
					Quoter.QuoteValue(column.ColumnName)
					);
		}

		internal virtual String GenerateSetType(IDataColumn column)
		{
			//_Truncator.Truncate(column);
			return String.Format(AlterColumnSetTypeSQLTemplate,
					Quoter.QuoteTableName(column.Table.TableName),
					Quoter.QuoteColumnName(column.ColumnName),
					Column.GenerateForTypeAlter(column));
		}

		#endregion

		internal override String AlterColumnSQL(String schemaName, IDataColumn column)
		{
			//_Truncator.Truncate(expression);
			return _CompatabilityMode.HandleCompatabilty("Alter column is not supported as expected");
		}

		//internal override String Generate(CreateColumnExpression expression)
		//{
		//	_Truncator.Truncate(expression);
		//	return base.Generate(expression);
		//}

		//internal override String Generate(DeleteColumnExpression expression)
		//{
		//	_Truncator.Truncate(expression);
		//	return base.Generate(expression);
		//}

		//internal override String Generate(RenameColumnExpression expression)
		//{
		//	_Truncator.Truncate(expression);
		//	return base.Generate(expression);
		//}

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
			//Firebird doesn't have particular asc or desc order per column, only per the whole index
			// CREATE [UNIQUE] [ASC[ENDING] | [DESC[ENDING]] INDEX indexname
			//  ON tablename  { (<col> [, <col> ...]) | COMPUTED BY (expression) }
			//  <col>  ::=  a column not of type ARRAY, BLOB or COMPUTED BY
			//
			// Assuming the first column's direction for the index's direction.

			//_Truncator.Truncate(expression);

			if (index.Columns.Count <= 0) { return String.Empty; }
			var isDescending = index.Columns.First().IsDescending;
			var indexColumns = String.Join(", ", index.Columns.Select(c => Quoter.QuoteColumnName(c.Name)));

			return String.Format(CreateIndexSQLTemplate
					, GetUniqueString(index)
					, isDescending ? "DESC " : "ASC "
					, Quoter.QuoteIndexName(index.Name)
					, Quoter.QuoteTableName(index.TableName)
					, indexColumns);
		}

		#endregion

		#region - Constraint -

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			return String.Format("ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}",
					Quoter.QuoteTableName(tableName),
					Quoter.QuoteColumnName(columnName),
					Quoter.QuoteValue(defaultValue)
					);
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			return String.Format("ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
					Quoter.QuoteTableName(tableName),
					Quoter.QuoteColumnName(columnName)
					);
		}

		#endregion

		#region - Sequence -

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			//_Truncator.Truncate(expression);
			return String.Format("CREATE SEQUENCE {0}", Quoter.QuoteSequenceName(sequence.Name));
		}

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			//_Truncator.Truncate(expression);
			return String.Format("DROP SEQUENCE {0}", Quoter.QuoteSequenceName(sequenceName));
		}

		internal String GenerateAlterSequence(SequenceDefinition sequence)
		{
			//_Truncator.Truncate(sequence);
			if (sequence.StartWith != null)
			{
				return String.Format("ALTER SEQUENCE {0} RESTART WITH {1}", Quoter.QuoteSequenceName(sequence.Name), sequence.StartWith.ToString());
			}

			return String.Empty;
		}

		#endregion

		#endregion

		#region -- Helpers --

		private static bool ColumnTypesMatch(IDataColumn col1, IDataColumn col2)
		{
			var column = new FirebirdColumn();
			var colDef1 = column.GenerateForTypeAlter(col1);
			var colDef2 = column.GenerateForTypeAlter(col2);
			return colDef1 == colDef2;
		}

		private static bool DefaultValuesMatch(IDataColumn col1, IDataColumn col2)
		{
			//if (col1.DefaultValue is ColumnDefinition.UndefinedDefaultValue && col2.DefaultValue is ColumnDefinition.UndefinedDefaultValue) { return true; }
			//if (col1.DefaultValue is ColumnDefinition.UndefinedDefaultValue || col2.DefaultValue is ColumnDefinition.UndefinedDefaultValue) { return true; }
			//var column = new FirebirdColumn();
			//var col1Value = column.GenerateForDefaultAlter(col1);
			//var col2Value = column.GenerateForDefaultAlter(col2);
			//return col1Value != col2Value;
			return true;
		}

		private static String Pack(String name)
		{
			if (String.IsNullOrEmpty(name))
			{
				if (name.Length > _MaxNameLength)
				{
					//if (!enabled)
					//	throw new ArgumentException(String.Format("Name too long: {0}", name));

					//byte[] byteHash = MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(name));
					var hash = Convert.ToBase64String(name.MD5());
					var sb = new StringBuilder(hash.Length);
					var hLength = hash.Length;
					for (Int32 i = 0; i < hLength; i++)
					{
						var c = hash[i];
						if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) { sb.Append(c); }
					}
					hash = sb.ToString();
					return "fk_{0}".FormatWith(hash.Length <= 28 ? hash : hash.Substring(0, 28));
				}
			}
			return name;
		}

		#endregion
	}
}