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
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlServer2012Generator : SqlServer2012Generator<SqlServer2008TypeMap> { }

	internal class SqlServer2012Generator<TTypeMap> : SqlServer2008Generator<TTypeMap, SqlServer2008Quoter>
		where TTypeMap : SqlServer2008TypeMap, new()
	{
		internal SqlServer2012Generator()
			: base() { }

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			var result = new StringBuilder("CREATE SEQUENCE ");
			result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(sequence.SchemaName), Quoter.QuoteSequenceName(sequence.Name));

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

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			var result = new StringBuilder("DROP SEQUENCE ");
			result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteSequenceName(sequenceName));

			return result.ToString();
		}
	}
}