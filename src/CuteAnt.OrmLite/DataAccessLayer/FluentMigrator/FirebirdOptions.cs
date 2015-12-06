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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>Firebird Transaction Model</summary>
	public enum FirebirdTransactionModel
	{
		/// <summary>Automatically starts a new transaction when a virtual lock check fails</summary>
		AutoCommitOnCheckFail,

		/// <summary>Automaticaly commits every processed statement</summary>
		AutoCommit,

		/// <summary>Don't manage transactions</summary>
		None
	}

	/// <summary>Firebird Options</summary>
	public class FirebirdOptions
	{
		/// <summary>Maximum internal length of names in firebird is 31 characters</summary>
		public static readonly int MaxNameLength = 31;

		/// <summary>Firebird only supports constraint, table, column, etc. names up to 31 characters</summary>
		public bool TruncateLongNames { get; set; }

		/// <summary>Virtually lock tables and columns touched by DDL statements in a transaction</summary>
		public bool VirtualLock { get; set; }

		/// <summary></summary>
		public bool UndoEnabled { get; set; }

		/// <summary>
		/// Which transaction model to use if any to work around firebird's DDL restrictions
		/// </summary>
		public FirebirdTransactionModel TransactionModel { get; set; }

		/// <summary>默认构造函数</summary>
		public FirebirdOptions()
		{
			TransactionModel = FirebirdTransactionModel.None;
			TruncateLongNames = false;
			VirtualLock = false;
			UndoEnabled = false;
		}

		/// <summary></summary>
		/// <returns></returns>
		public static FirebirdOptions StandardBehaviour()
		{
			return new FirebirdOptions()
			{
				TransactionModel = FirebirdTransactionModel.None,
				TruncateLongNames = false,
				VirtualLock = false,
				UndoEnabled = false
			};
		}

		/// <summary></summary>
		/// <returns></returns>
		public static FirebirdOptions CommitOnCheckFailBehaviour()
		{
			return new FirebirdOptions()
			{
				TransactionModel = FirebirdTransactionModel.AutoCommitOnCheckFail,
				TruncateLongNames = true,
				VirtualLock = true,
				UndoEnabled = true
			};
		}

		/// <summary></summary>
		/// <returns></returns>
		public static FirebirdOptions AutoCommitBehaviour()
		{
			return new FirebirdOptions()
			{
				TransactionModel = FirebirdTransactionModel.AutoCommit,
				TruncateLongNames = true,
				VirtualLock = false,
				UndoEnabled = true
			};
		}

		/// <summary></summary>
		/// <returns></returns>
		public static FirebirdOptions AutoCommitWithoutUndoBehaviour()
		{
			return new FirebirdOptions()
			{
				TransactionModel = FirebirdTransactionModel.AutoCommit,
				TruncateLongNames = true,
				VirtualLock = true,
				UndoEnabled = false
			};
		}
	}
}