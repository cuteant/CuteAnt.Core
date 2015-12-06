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
using CuteAnt.OrmLite.Exceptions;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlServerCeGenerator : SqlServer2000Generator<SqlServerCeTypeMap, SqlServer2000Quoter, StandardDescriptionGenerator>
	{
		internal SqlServerCeGenerator()
		{
		}

		internal override String GetClusterTypeString(IndexDefinition index)
		{
			return String.Empty;
		}

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			return String.Format("sp_rename {0}, {1}", Quoter.QuoteValue(oldName), Quoter.QuoteValue(newName));
		}

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			//throw new DatabaseOperationNotSupportedException();
			return _CompatabilityMode.HandleCompatabilty("Rename column are not supported");
		}

		// All Schema method throw by default as only Sql server 2005 and up supports them.
		internal override String CreateSchemaSQL(String schemaName)
		{
			//throw new DatabaseOperationNotSupportedException();
			return _CompatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		internal override String DeleteSchemaSQL(String schemaName)
		{
			//throw new DatabaseOperationNotSupportedException();
			return _CompatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		internal override String AlterSchemaSQL(String srcSchemaName, String tableName, String destSchemaName)
		{
			//throw new DatabaseOperationNotSupportedException();
			return _CompatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		internal override String DropColumnSQL(String schemaName, String tableName, String columnName)
		{
			// Limited functionality in CE, for now will just drop the column.. no DECLARE support!
			return String.Format(@"ALTER TABLE {0} DROP COLUMN {1}", Quoter.QuoteTableName(tableName), Quoter.QuoteColumnName(columnName));
		}

		internal override String DeleteIndexSQL(String schemaName, String tableName, String indexName)
		{
			return String.Format("DROP INDEX {0}.{1}", Quoter.QuoteTableName(tableName), Quoter.QuoteIndexName(indexName));
		}

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			//throw new DatabaseOperationNotSupportedException();
			return _CompatabilityMode.HandleCompatabilty("Default constraint are not supported");
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			//throw new DatabaseOperationNotSupportedException();
			return _CompatabilityMode.HandleCompatabilty("Default constraint are not supported");
		}
	}
}