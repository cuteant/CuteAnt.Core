﻿/* 本模块基于开源项目 FluentMigrator 的子模块 Runner.Generators 修改而成。修改：海洋饼干(cuteant@outlook.com)
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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlServerColumn<TTypeMap, TQuoter> : ColumnBase<TTypeMap, TQuoter>
		where TTypeMap : TypeMapBase, new()
		where TQuoter : SqlServer2000Quoter, new()
	{
		public SqlServerColumn()
		{
		}

		internal override String FormatDefaultValue(IDataColumn column)
		{
			//if (DefaultValueIsSqlFunction(column.Default))
			//{
			//	return "DEFAULT " + column.Default;
			//}

			//var defaultValue = base.FormatDefaultValue(column);

			//if (column.ModificationType == ColumnModificationType.Create && !defaultValue.IsNullOrWhiteSpace())
			//{
			//	return "CONSTRAINT " + Quoter.QuoteConstraintName(GetDefaultConstraintName(column.Table.TableName, column.Name)) + " " + defaultValue;
			//}

			return String.Empty;
		}

		private static Boolean DefaultValueIsSqlFunction(String defaultValue)
		{
			return defaultValue.EndsWith("()");
		}

		internal override String FormatIdentity(IDataColumn column)
		{
			return column.Identity ? "IDENTITY(1,1)" : String.Empty;
		}

		internal override String FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.NewGuid:
					return "NEWID()";

				case SystemMethods.NewSequentialId:
					return "NEWSEQUENTIALID()";

				case SystemMethods.CurrentDateTime:
					return "GETDATE()";

				case SystemMethods.CurrentUTCDateTime:
					return "GETUTCDATE()";

				case SystemMethods.CurrentUser:
					return "CURRENT_USER";
			}

			return null;
		}

		internal String FormatDefaultValue(Object defaultValue)
		{
			//if (DefaultValueIsSqlFunction(defaultValue)) { return defaultValue.ToString(); }

			//if (defaultValue is SystemMethods) { return FormatSystemMethods((SystemMethods)defaultValue); }

			//return Quoter.QuoteValue(defaultValue);
			return String.Empty;
		}

		internal override String AddPrimaryKeyConstraint(IDataTable table, IEnumerable<IDataColumn> primaryKeyColumns)
		{
			var keyColumns = String.Join(", ", primaryKeyColumns.Select(x => Quoter.QuoteColumnName(x.ColumnName)));
			var hasClustered = table.Indexes.Where(e => !e.PrimaryKey).Any(e => e.Clustered);
			//return String.Format("{0}PRIMARY KEY ({1})", GetPrimaryKeyConstraintName(primaryKeyColumns, tableName), keyColumns);
			return String.Format("PRIMARY KEY{1} ({0})", keyColumns, hasClustered ? " NONCLUSTERED" : " CLUSTERED");
		}

		internal static String GetDefaultConstraintName(String tableName, String columnName)
		{
			return String.Format("DF_{0}_{1}", tableName, columnName);
		}
	}
}