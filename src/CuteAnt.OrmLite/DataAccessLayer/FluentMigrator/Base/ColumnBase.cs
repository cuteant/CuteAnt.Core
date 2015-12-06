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
	/// <summary>数据列定义</summary>
	internal abstract class ColumnBase<TTypeMap, TQuoter>
		//where TColumn : ColumnBase<TColumn, TTypeMap, TQuoter>, new()
		where TTypeMap : TypeMapBase, new()
		where TQuoter : QuoterBase, new()
	{
		#region -- 属性 --

		private TTypeMap _TypeMap;

		internal TTypeMap TypeMap
		{
			get { return _TypeMap; }
			private set { _TypeMap = value; }
		}

		private TQuoter _Quoter;

		internal TQuoter Quoter
		{
			get { return _Quoter; }
			set { _Quoter = value; }
		}

		internal IList<Func<IDataColumn, String>> ClauseOrder { get; set; }

		#endregion

		#region -- 构造 --

		public ColumnBase()
		{
			TypeMap = new TTypeMap();
		}

		internal virtual void Initialize(TQuoter quoter)
		{
			Quoter = quoter;
			ClauseOrder = new List<Func<IDataColumn, String>> { FormatString, FormatType, FormatCollate, FormatNullable, FormatDefaultValue, FormatPrimaryKey, FormatIdentity };
			//ClauseOrder = new List<Func<IDataColumn, String>> { FormatString, FormatType, FormatCollation, FormatNullable, FormatDefaultValue, FormatPrimaryKey, FormatIdentity };
		}

		#endregion

		#region -- 定义 --

		//internal String GetTypeMap(CommonDbType value, Int32 size, Int32 precision, Int32 scale)
		//{
		//	return _TypeMap.GetTypeMap(value, size, precision, scale);
		//}

		internal virtual String FormatString(IDataColumn column)
		{
			return Quoter.QuoteColumnName(column.ColumnName);
		}

		internal virtual String FormatType(IDataColumn column)
		{
			return _TypeMap.GetTypeMap(column.DbType, column.Length, column.Precision, column.Scale);
		}

		internal virtual String FormatNullable(IDataColumn column)
		{
			return column.Nullable ? String.Empty : "NOT NULL";
		}

		internal virtual String FormatDefaultValue(IDataColumn column)
		{
			return String.Empty;
			//if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue) { return String.Empty; }

			//// see if this is for a system method
			//if (column.DefaultValue is SystemMethods)
			//{
			//	var method = FormatSystemMethods((SystemMethods)column.DefaultValue);
			//	if (method.IsNullOrWhiteSpace()) { return String.Empty; }

			//	return "DEFAULT " + method;
			//}

			//return "DEFAULT " + Quoter.QuoteValue(column.DefaultValue);
		}

		internal abstract String FormatIdentity(IDataColumn column);

		internal abstract String FormatSystemMethods(SystemMethods systemMethod);

		internal virtual String FormatPrimaryKey(IDataColumn column)
		{
			return String.Empty;
		}

		internal virtual String FormatCollate(IDataColumn column)
		{
			return String.Empty;
		}

		#endregion

		#region -- SQL 语句 --

		internal virtual String Generate(IDataColumn column)
		{
			var clauses = new List<String>();

			foreach (var action in ClauseOrder)
			{
				var clause = action(column);
				if (!clause.IsNullOrWhiteSpace()) { clauses.Add(clause); }
			}

			return String.Join(" ", clauses);
		}

		internal String Generate(IDataTable table)
		{
			var primaryKeyString = String.Empty;

			//if more than one column is a primary key or the primary key is given a name, then it needs to be added separately

			var columns = table.Columns;
			//CAUTION: this must execute before we set the values of primarykey to false; Beware of yield return
			var primaryKeyColumns = columns.Where(x => x.PrimaryKey);

			if (ShouldPrimaryKeysBeAddedSeparately(primaryKeyColumns))
			{
				primaryKeyString = AddPrimaryKeyConstraint(table, primaryKeyColumns);
				foreach (var column in columns)
				{
					column.PrimaryKey = false;
				}
			}

			var sb = new StringBuilder();
			for (Int32 i = 0; i < columns.Count; i++)
			{
				sb.AppendLine();
				sb.Append("\t");
				sb.Append(Generate(columns[i]));
				if (i < columns.Count - 1) { sb.Append(","); }
			}
			if (!primaryKeyString.IsNullOrWhiteSpace())
			{
				sb.Append(",");
				sb.AppendLine();
				sb.Append("\t");
				sb.Append(primaryKeyString);
			}
			return sb.ToString();
		}

		#endregion

		#region -- 主键 --

		internal virtual Boolean ShouldPrimaryKeysBeAddedSeparately(IEnumerable<IDataColumn> primaryKeyColumns)
		{
			//By default always try to add primary keys as a separate constraint if any exist
			return primaryKeyColumns.Any(x => x.PrimaryKey);
		}

		internal virtual String AddPrimaryKeyConstraint(IDataTable table, IEnumerable<IDataColumn> primaryKeyColumns)
		{
			var keyColumns = String.Join(", ", primaryKeyColumns.Select(x => Quoter.QuoteColumnName(x.ColumnName)));

			//return String.Format("{0}PRIMARY KEY ({1})", GetPrimaryKeyConstraintName(primaryKeyColumns, tableName), keyColumns);
			return String.Format("PRIMARY KEY ({0})", keyColumns);
		}

		/// <summary>获取主键约束名称，最多30个字符</summary>
		/// <returns></returns>
		internal String GetPrimaryKeyConstraintName(IEnumerable<IDataColumn> primaryKeyColumns, String tableName)
		{
			////var primaryKeyName = primaryKeyColumns.Select(x => x.PrimaryKeyName).FirstOrDefault();
			//var primaryKeyName = primaryKeyColumns.Select(x => x.ColumnName).FirstOrDefault();

			//if (primaryKeyName.IsNullOrWhiteSpace()) { return String.Empty; }

			//return String.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
			return "CONSTRAINT {0} ".FormatWith(Quoter.QuoteIndexName("PK__{0}__{1}".FormatWith(tableName.Cut(8), GuidHelper.GenerateId16().ToUpperInvariant())));
		}

		#endregion
	}
}