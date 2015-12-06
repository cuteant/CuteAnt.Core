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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqliteColumn : ColumnBase<SqliteTypeMap, SqliteQuoter>
	{
		public SqliteColumn()
		{
		}

		internal override String FormatType(IDataColumn column)
		{
			if (column.Identity) { return "INTEGER"; }

			return base.FormatType(column);
		}

		internal override String FormatIdentity(IDataColumn column)
		{
			// 苦竹 屏蔽异常
			////SQLite only supports the concept of Identity in combination with a single primary key
			////see: http://www.sqlite.org/syntaxdiagrams.html#column-constraint syntax details
			//if (column.IsIdentity && !column.IsPrimaryKey && column.Type != DbType.Int32)
			//{
			//	throw new ArgumentException("SQLite only supports identity on single integer, primary key coulmns");
			//}
			return String.Empty;
		}

		internal override Boolean ShouldPrimaryKeysBeAddedSeparately(IEnumerable<IDataColumn> primaryKeyColumns)
		{
			// If there are no identity column then we can add as a separate constrint
			if (!primaryKeyColumns.Any(x => x.Identity) && primaryKeyColumns.Any(x => x.PrimaryKey)) { return true; }
			return false;
		}

		internal override String FormatPrimaryKey(IDataColumn column)
		{
			if (!column.PrimaryKey) { return String.Empty; }

			return column.Identity ? "PRIMARY KEY AUTOINCREMENT" : String.Empty;
		}

		internal override String FormatCollate(IDataColumn column)
		{
			// 给字符串字段加上忽略大小写，否则admin和Admin是查不出来的
			if (column.DataType == typeof(String)) { return "COLLATE NOCASE"; }
			return String.Empty;
		}

		internal override String FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.CurrentDateTime:
					return "CURRENT_TIMESTAMP";
			}

			return null;
		}
	}
}