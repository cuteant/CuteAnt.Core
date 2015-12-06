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
	internal abstract class GeneratorBase<TColumn, TTypeMap, TQuoter, TDescriptionGenerator> : GeneratorBase
		//where TGenerator : GeneratorBase<TGenerator, TColumn, TTypeMap, TQuoter, TDescriptionGenerator>, new()
		where TColumn : ColumnBase<TTypeMap, TQuoter>, new()
		where TTypeMap : TypeMapBase, new()
		where TQuoter : QuoterBase, new()
		where TDescriptionGenerator : StandardDescriptionGenerator, new()
	{
		#region -- 属性 --

		private TColumn _Column;

		internal TColumn Column
		{
			get { return _Column; }
			private set { _Column = value; }
		}

		private TQuoter _Quoter;

		internal TQuoter Quoter
		{
			get { return _Quoter; }
			private set { _Quoter = value; }
		}

		internal override IQuoter QuoterInternal { get { return Quoter; } }

		private TDescriptionGenerator _DescriptionGenerator;

		internal TDescriptionGenerator DescriptionGenerator
		{
			get { return _DescriptionGenerator; }
			private set { _DescriptionGenerator = value; }
		}

		#endregion

		#region -- 构造 --

		internal GeneratorBase()
		{
			Quoter = new TQuoter();

			Column = new TColumn();
			Column.Initialize(Quoter);

			DescriptionGenerator = new TDescriptionGenerator();
			DescriptionGenerator.SetQuoter(Quoter);
		}

		#endregion
	}

	internal abstract class GeneratorBase
	{
		#region -- 属性 --

		internal abstract IQuoter QuoterInternal { get; }

		#endregion

		#region -- SQL语句生成 --

		#region - DataBase -

		internal abstract String CreateDatabaseSQL(String dbName, String dataPath);

		internal abstract String DropDatabaseSQL(String dbName);

		#endregion

		#region - Schema -

		internal abstract String CreateSchemaSQL(String schemaName);

		internal abstract String DeleteSchemaSQL(String schemaName);

		internal abstract String AlterSchemaSQL(String srcSchemaName, String tableName, String destSchemaName);

		#endregion

		#region - Table -

		internal abstract String CreateTableSQL(String schemaName, IDataTable table);

		internal abstract String DropTableSQL(String schemaName, String tableName);

		internal abstract String RenameTableSQL(String schemaName, String oldName, String newName);

		internal virtual String AlterTableSQL(String schemaName, IDataTable table)
		{
			// returns nothing because the individual AddColumn and AlterColumn calls
			//  create CreateColumnExpression and AlterColumnExpression respectively
			return String.Empty;
		}

		#endregion

		#region - Column -

		internal abstract String AlterColumnSQL(String schemaName, IDataColumn column);

		internal abstract String CreateColumnSQL(String schemaName, IDataColumn column);

		internal abstract String DropColumnSQL(String schemaName, String tableName, String columnName);

		internal abstract String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName);

		internal abstract Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn);

		#endregion

		#region - Index -

		internal String CreateIndexSQL(String schemaName, IDataTable table)
		{
			// 排除主键索引、计算所得的索引
			var indexs = IndexDefinition.Create(schemaName, table);
			if (indexs == null) { return String.Empty; }

			var sb = new StringBuilder();
			var count = indexs.Count;
			for (int i = 0; i < count; i++)
			{
				sb.Append(CreateIndexSQL(indexs[i]));
				if (i < count - 1) { sb.AppendLine(";"); }
			}
			return sb.ToString();
		}

		internal abstract String CreateIndexSQL(IndexDefinition index);

		internal String DeleteIndexSQL(String schemaName, IDataTable table)
		{
			var sb = new StringBuilder();
			var indexs = table.Indexes;
			var count = indexs.Count;
			for (int i = 0; i < count; i++)
			{
				var item = indexs[i];

				// 计算的索引不需要删除
				if (item.Computed) { continue; }
				// 主键的索引不能删
				if (item.PrimaryKey) { continue; }

				sb.Append(DeleteIndexSQL(schemaName, table.TableName, item.Name));
				if (i < count - 1) { sb.AppendLine(";"); }
			}
			return sb.ToString();
		}

		internal abstract String DeleteIndexSQL(String schemaName, String tableName, String indexName);

		#endregion

		#region - Sequence -

		internal abstract String CreateSequenceSQL(SequenceDefinition sequence);

		internal abstract String DeleteSequenceSQL(String schemaName, String sequenceName);

		#endregion

		#region - Constraint -

		internal abstract String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue);

		internal abstract String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName);

		internal abstract String CreateConstraintSQL(ConstraintDefinition constraint);

		internal abstract String DeleteConstraintSQL(String schemaName, String tableName, String constraintName, Boolean isPrimaryKey);

		#endregion

		#region - Data -

		internal abstract String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, StringBuilder values);

		internal abstract String InsertDataSQL(String schemaName, String tableName, StringBuilder columns, List<StringBuilder> values);

		internal abstract String DeleteDataSQL(String schemaName, String tableName, String whereClause);

		internal abstract String UpdateDataSQL(String schemaName, String tableName, String setClause, String whereClause);

		#endregion

		#endregion
	}
}