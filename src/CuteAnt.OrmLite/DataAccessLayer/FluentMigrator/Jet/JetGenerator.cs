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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class JetGenerator : GenericGenerator<JetColumn, JetTypeMap, JetQuoter, StandardDescriptionGenerator>
	{
		internal JetGenerator()
		{
		}

		internal override String DropIndexSQLTemplate { get { return "DROP INDEX {0} ON {1}"; } }

		internal override String RenameTableSQL(String schemaName, String oldName, String newName)
		{
			return _CompatabilityMode.HandleCompatabilty("Renaming of tables is not supported for Jet");
		}

		internal override String RenameColumnSQL(String schemaName, String tableName, String oldName, String newName)
		{
			return _CompatabilityMode.HandleCompatabilty("Renaming of columns is not supported for Jet");
		}

		internal override String AlterDefaultConstraintSQL(String schemaName, String tableName, String columnName, Object defaultValue)
		{
			return _CompatabilityMode.HandleCompatabilty("Altering of default constraints is not supported for Jet");
		}

		internal override String CreateSequenceSQL(SequenceDefinition sequence)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences are not supported for Jet");
		}

		internal override String DeleteSequenceSQL(String schemaName, String sequenceName)
		{
			return _CompatabilityMode.HandleCompatabilty("Sequences are not supported for Jet");
		}

		internal override String DeleteDefaultConstraintSQL(String schemaName, String tableName, String columnName)
		{
			return _CompatabilityMode.HandleCompatabilty("Default constraints are not supported");
		}

		internal override Boolean IsColumnTypeChanged(IDataColumn entityColumn, IDataColumn dbColumn)
		{
			// 先判断数据类型
			if (entityColumn.DbType == dbColumn.DbType) { return false; }

			//// 类型不匹配，不一定就是有改变，还要查找类型对照表是否有匹配的，只要存在任意一个匹配，就说明是合法的
			//foreach (var item in FieldTypeMaps)
			//{
			//	//if (entityColumn.DataType == item.Key && dbColumn.DataType == item.Value) { return false; }
			//	// 把不常用的类型映射到常用类型，比如数据库SByte映射到实体类Byte，UInt32映射到Int32，而不需要重新修改数据库
			//	if (dbColumn.DataType == item.Key && entityColumn.DataType == item.Value) { return false; }
			//}
			switch (entityColumn.DbType)
			{
				case CommonDbType.AnsiString:
					break;
				case CommonDbType.AnsiStringFixedLength:
					break;
				case CommonDbType.String:
					break;
				case CommonDbType.StringFixedLength:
					break;

				case CommonDbType.Binary:
					break;
				case CommonDbType.BinaryFixedLength:
					break;

				case CommonDbType.Boolean:
					break;

				case CommonDbType.CombGuid:
					break;
				case CommonDbType.CombGuid32Digits:
					break;

				case CommonDbType.Guid:
					break;
				case CommonDbType.Guid32Digits:
					break;

				case CommonDbType.Date:
					break;
				case CommonDbType.DateTime:
					break;
				case CommonDbType.DateTime2:
					break;
				case CommonDbType.DateTimeOffset:
					break;
				case CommonDbType.Time:
					break;

				case CommonDbType.Currency:
					break;
				case CommonDbType.Decimal:
					break;

				case CommonDbType.TinyInt:
					break;
				case CommonDbType.SignedTinyInt:
					break;
				case CommonDbType.SmallInt:
					break;
				case CommonDbType.Integer:
					break;
				case CommonDbType.BigInt:
					break;
				case CommonDbType.Double:
					break;
				case CommonDbType.Float:
					break;
				case CommonDbType.Text:
					break;

				case CommonDbType.Xml:
					break;
				case CommonDbType.Json:
					break;
				case CommonDbType.Unknown:
				default:
					break;
			}
			return true;
		}
	}
}