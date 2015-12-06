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
using CuteAnt.OrmLite.Exceptions;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	//internal class OracleColumn : OracleColumn<OracleColumn, OracleQuoter> { }
	//internal class OracleColumnQuotedIdentifier : OracleColumn<OracleColumnQuotedIdentifier, OracleQuoterQuotedIdentifier> { }

	internal class OracleColumn<TQuoter> : ColumnBase<OracleTypeMap, TQuoter>
		//where TColumn : ColumnBase<TColumn, OracleTypeMap, TQuoter>, new()
		where TQuoter : QuoterBase, new()
	{
		private const int OracleObjectNameMaxLength = 30;

		public OracleColumn()
		{
		}

		internal override void Initialize(TQuoter quoter)
		{
			Quoter = quoter;

			//int a = ClauseOrder.IndexOf(FormatDefaultValue);
			//int b = ClauseOrder.IndexOf(FormatNullable);

			//// Oracle requires DefaultValue before nullable
			//if (a > b)
			//{
			//	ClauseOrder[b] = FormatDefaultValue;
			//	ClauseOrder[a] = FormatNullable;
			//}
			// Oracle requires DefaultValue before nullable
			ClauseOrder = new List<Func<IDataColumn, String>> { FormatString, FormatType, FormatDefaultValue, FormatNullable, FormatPrimaryKey, FormatIdentity, FormatCollate };
		}

		internal override String FormatIdentity(IDataColumn column)
		{
			//if (column.Identity)
			//{
			//	throw new DatabaseOperationNotSupportedException("Oracle does not support identity columns. Please use a SEQUENCE instead");
			//}
			return String.Empty;
		}

		internal override String FormatNullable(IDataColumn column)
		{
			//Creates always return Not Null unless is nullable is true
			//if (column.ModificationType == ColumnModificationType.Create)
			//{
				if (column.Nullable)
				{
					return String.Empty;
				}
				else
				{
					return "NOT NULL";
				}
			//}

			//alter only returns "Not Null" if IsNullable is explicitly set
			//if (column.Nullable)
			//{
				//return column.Nullable ? "NULL" : "NOT NULL";
			//}
			//else
			//{
			//	return String.Empty;
			//}
		}

		internal override String FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.NewGuid:
					return "sys_guid()";
				case SystemMethods.CurrentDateTime:
					return "CURRENT_TIMESTAMP";
				case SystemMethods.CurrentUser:
					return "USER";
			}

			return null;
		}

		internal override String AddPrimaryKeyConstraint(IDataTable table, IEnumerable<IDataColumn> primaryKeyColumns)
		{
			var keyColumns = String.Join(", ", primaryKeyColumns.Select(x => Quoter.QuoteColumnName(x.ColumnName)));

			return String.Format("{0}PRIMARY KEY ({1})", GetPrimaryKeyConstraintName(primaryKeyColumns, table.TableName), keyColumns);
		}

		//internal override String GetPrimaryKeyConstraintName(IEnumerable<IDataColumn> primaryKeyColumns, String tableName)
		//{
		//	if (primaryKeyColumns == null) { throw new ArgumentNullException("primaryKeyColumns"); }
		//	if (tableName == null) { throw new ArgumentNullException("tableName"); }

		//	var primaryKeyName = primaryKeyColumns.First().ColumnName;

		//	if (primaryKeyName.IsNullOrWhiteSpace())
		//	{
		//		return String.Empty;
		//	}

		//	if (primaryKeyName.Length > OracleObjectNameMaxLength)
		//	{
		//		throw new DatabaseOperationNotSupportedException("Oracle does not support length of primary key name greater than {0} characters. Reduce length of primary key name. ({1})".FormatWith(OracleObjectNameMaxLength, primaryKeyName));
		//	}

		//	var result = String.Format("CONSTRAINT {0} ", Quoter.QuoteConstraintName(primaryKeyName));
		//	return result;
		//}
	}
}