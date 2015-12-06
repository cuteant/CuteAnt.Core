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
	/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
	public interface IQuoter
	{
		/// <summary>最小时间</summary>
		DateTime DateTimeMin { get; }

		/// <summary>Returns a quoted String that has been correctly escaped</summary>
		/// <param name="name"></param>
		/// <returns></returns>
		String Quote(String name);

		/// <summary>Provides and unquoted, unescaped String</summary>
		/// <param name="quotedName"></param>
		/// <returns></returns>
		String UnQuote(String quotedName);

		/// <summary>Returns true is the value starts and ends with a close quote</summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Boolean IsQuoted(String name);

		/// <summary>是否Unicode编码。只是固定判断n开头的几个常见类型为Unicode编码，这种方法不是很严谨，可以考虑读取DataTypes架构</summary>
		/// <param name="rawType"></param>
		/// <returns></returns>
		Boolean IsUnicode(String rawType);

		/// <summary>Quotes a DataBase name</summary>
		/// <param name="dbName"></param>
		/// <returns></returns>
		String QuoteDataBaseName(String dbName);

		/// <summary>Quotes a Schema Name</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		String QuoteSchemaName(String schemaName);

		/// <summary>Quotes a Table name</summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		String QuoteTableName(String tableName);

		/// <summary>Quotes a column name</summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		String QuoteColumnName(String columnName);

		/// <summary>Quote an index name</summary>
		/// <param name="indexName"></param>
		/// <returns></returns>
		String QuoteIndexName(String indexName);

		/// <summary>Quotes a constraint name</summary>
		/// <param name="contraintName"></param>
		/// <returns></returns>
		String QuoteConstraintName(String contraintName);

		/// <summary>Quotes a Sequence name</summary>
		/// <param name="sequenceName"></param>
		/// <returns></returns>
		String QuoteSequenceName(String sequenceName);

		/// <summary>QuoteCommand</summary>
		/// <param name="command"></param>
		/// <returns></returns>
		String QuoteCommand(String command);

		/// <summary>转义数据为SQL数据</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		String QuoteValue(String value);

		/// <summary>转义数据为SQL数据</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		String QuoteValue(Object value);

		/// <summary>转义数据为SQL数据</summary>
		/// <param name="field">字段</param>
		/// <param name="value"></param>
		/// <returns></returns>
		String QuoteValue(IDataColumn field, Object value);

		/// <summary>转义时间为SQL字符串</summary>
		/// <param name="dateTime">时间值</param>
		/// <returns></returns>
		String FormatDateTime(DateTime dateTime);
	}
}
