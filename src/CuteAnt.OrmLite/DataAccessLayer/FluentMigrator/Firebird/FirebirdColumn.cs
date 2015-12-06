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
using System.Linq;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class FirebirdColumn : ColumnBase<FirebirdTypeMap, FirebirdQuoter>
	{
		private FirebirdOptions _FBOptions;

		internal FirebirdOptions FBOptions
		{
			get
			{
				if (_FBOptions == null) { _FBOptions = new FirebirdOptions(); }
				return _FBOptions;
			}
			set { _FBOptions = value; }
		}

		public FirebirdColumn()
		{
		}

		internal override void Initialize(FirebirdQuoter quoter)
		{
			Quoter = quoter;

			// In firebird DEFAULT clause precedes NULLABLE clause
			ClauseOrder = new List<Func<IDataColumn, String>> { FormatString, FormatType, FormatDefaultValue, FormatNullable, FormatPrimaryKey, FormatIdentity, FormatCollate };
		}

		internal override String FormatIdentity(IDataColumn column)
		{
			//Identity not supported
			return String.Empty;
		}

		internal override String FormatCollate(IDataColumn column)
		{
			// 给字符串字段加上忽略大小写，否则admin和Admin是查不出来的
			if (column.DataType == typeof(String)) { return "COLLATE UNICODE_CI_AI"; } // UNICODE_CI
			return String.Empty;
		}

		internal override String FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.NewGuid:
					return "gen_uuid()";

				case SystemMethods.CurrentDateTime:
					return "CURRENT_TIMESTAMP";
			}

			// 苦竹修改，去除异常，返回空值
			//throw new NotImplementedException();
			return null;
		}

		//internal override String GetPrimaryKeyConstraintName(IEnumerable<IDataColumn> primaryKeyColumns, String tableName)
		//{
		//	//var primaryKeyName = primaryKeyColumns.Select(x => x.PrimaryKeyName).FirstOrDefault();
		//	var primaryKeyName = primaryKeyColumns.Select(x => x.ColumnName).FirstOrDefault();

		//	if (primaryKeyName.IsNullOrWhiteSpace())
		//	{
		//		return String.Empty;
		//	}
		//	else if (primaryKeyName.Length > FirebirdOptions.MaxNameLength)
		//	{
		//		if (!FBOptions.TruncateLongNames)
		//		{
		//			throw new ArgumentException(String.Format("Name too long: {0}", primaryKeyName));
		//		}
		//		primaryKeyName = primaryKeyName.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, primaryKeyName.Length));
		//	}

		//	return String.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
		//}

		internal virtual String GenerateForTypeAlter(IDataColumn column)
		{
			return FormatType(column);
		}

		internal virtual String GenerateForDefaultAlter(IDataColumn column)
		{
			return FormatDefaultValue(column);
		}
	}
}