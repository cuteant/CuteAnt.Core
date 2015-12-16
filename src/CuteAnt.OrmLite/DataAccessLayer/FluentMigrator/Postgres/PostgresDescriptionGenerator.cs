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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>almost copied from OracleDescriptionGenerator, modified for escaping table description</summary>
	internal class PostgresDescriptionGenerator : GenericDescriptionGenerator<PostgresDescriptionGenerator>
	{
		private IQuoter m_quoter;

		internal override void SetQuoter(IQuoter quoter)
		{
			m_quoter = quoter;
		}

		#region Constants

		private const String TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}'";
		private const String ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}'";

		#endregion

		private String GetFullTableName(String schemaName, String tableName)
		{
			return String.IsNullOrEmpty(schemaName)
				 ? m_quoter.QuoteTableName(tableName)
				 : String.Format("{0}.{1}", m_quoter.QuoteSchemaName(schemaName), m_quoter.QuoteTableName(tableName));
		}

		internal override String GenerateTableDescription(String schemaName, String tableName, String tableDescription)
		{
			if (String.IsNullOrEmpty(tableDescription)) { return String.Empty; }

			return String.Format(TableDescriptionTemplate, GetFullTableName(schemaName, tableName), tableDescription.Replace("'", "''"));
		}

		internal override String GenerateColumnDescription(String schemaName, String tableName, String columnName, String columnDescription)
		{
			if (String.IsNullOrEmpty(columnDescription)) { return String.Empty; }

			return String.Format(
					ColumnDescriptionTemplate,
					GetFullTableName(schemaName, tableName),
					m_quoter.QuoteColumnName(columnName),
					columnDescription.Replace("'", "''"));
		}
	}
}
