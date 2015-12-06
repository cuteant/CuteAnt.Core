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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class PostgresColumn : ColumnBase<PostgresTypeMap, PostgresQuoter>
	{
		#region -- 构造 --

		public PostgresColumn()
		{
			AlterClauseOrder = new List<Func<IDataColumn, String>> { FormatAlterType, FormatAlterNullable };
		}

		#endregion

		internal String FormatAlterDefaultValue(String column, Object defaultValue)
		{
			//String formatDefaultValue = FormatDefaultValue(new ColumnDefinition { Name = column, DefaultValue = defaultValue });

			//return String.Format("SET {0}", formatDefaultValue);

			return String.Empty;
		}

		private String FormatAlterNullable(IDataColumn column)
		{
			//if (!column.IsNullable.HasValue) { return ""; }

			if (column.Nullable) { return "DROP NOT NULL"; }

			return "SET NOT NULL";
		}

		private String FormatAlterType(IDataColumn column)
		{
			return String.Format("TYPE {0}", GetColumnType(column));
		}

		internal IList<Func<IDataColumn, String>> AlterClauseOrder { get; set; }

		internal String GenerateAlterClauses(IDataColumn column)
		{
			var clauses = new List<String>();
			foreach (var action in AlterClauseOrder)
			{
				var columnClause = action(column);
				if (!columnClause.IsNullOrWhiteSpace())
				{
					clauses.Add(String.Format("ALTER {0} {1}", Quoter.QuoteColumnName(column.Name), columnClause));
				}
			}

			return String.Join(", ", clauses);
		}

		internal override String FormatIdentity(IDataColumn column)
		{
			return String.Empty;
		}

		//internal override String AddPrimaryKeyConstraint(String tableName, IEnumerable<IDataColumn> primaryKeyColumns)
		//{
		//	var pkName = GetPrimaryKeyConstraintName(primaryKeyColumns, tableName);

		//	var cols = String.Empty;
		//	var first = true;
		//	foreach (var col in primaryKeyColumns)
		//	{
		//		if (first)
		//		{
		//			first = false;
		//		}
		//		else
		//		{
		//			cols += ",";
		//		}
		//		cols += Quoter.QuoteColumnName(col.Name);
		//	}

		//	if (pkName.IsNullOrWhiteSpace())
		//	{
		//		return String.Format(", PRIMARY KEY ({0})", cols);
		//	}

		//	return String.Format(", {0}PRIMARY KEY ({1})", pkName, cols);
		//}

		internal override String FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.NewGuid:
					//need to run the script share/contrib/uuid-ossp.sql to install the uuid_generate4 function
					return "uuid_generate_v4()";

				case SystemMethods.NewSequentialId:
					return "uuid_generate_v1()";

				case SystemMethods.CurrentDateTime:
					return "now()";

				case SystemMethods.CurrentUTCDateTime:
					return "(now() at time zone 'UTC')";

				case SystemMethods.CurrentUser:
					return "current_user";
			}

			// 苦竹修改，去除异常，返回空值
			//throw new NotImplementedException(string.Format("System method {0} is not implemented.", systemMethod));
			return null;
		}

		internal override String FormatType(IDataColumn column)
		{
			if (column.Identity)
			{
				if (column.DbType == CommonDbType.BigInt) { return "bigserial"; }
				return "serial";
			}

			return base.FormatType(column);
		}

		internal String GetColumnType(IDataColumn column)
		{
			return FormatType(column);
		}
	}
}