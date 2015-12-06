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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal abstract class TypeMapBase
	{
		#region -- Fields --

		private readonly Dictionary<CommonDbType, SortedList<Int32, String>> _Templates = new Dictionary<CommonDbType, SortedList<Int32, String>>();

		private const String SizePlaceholder = "$size";

		private const String PrecisionPlaceholder = "$precision";

		private const String ScalePlaceholder = "$scale";

		#endregion

		#region -- 构造 --

		internal TypeMapBase()
		{
			SetupTypeMaps();
		}

		#endregion

		internal abstract void SetupTypeMaps();

		#region -- SetTypeMap Methods --

		internal void SetTypeMap(CommonDbType type, String template)
		{
			EnsureHasList(type);
			_Templates[type][0] = template;
		}

		internal void SetTypeMap(CommonDbType type, String template, Int32 maxSize)
		{
			EnsureHasList(type);
			_Templates[type][maxSize] = template;
		}

		#endregion

		#region -- method GetTypeMap --

		internal virtual String GetTypeMap(CommonDbType type, Int32 size, Int32 precision, Int32 scale)
		{
			if (!_Templates.ContainsKey(type)) { throw new NotSupportedException("Unsupported DbType '{0}'".FormatWith(type)); }

			#region 修正

			switch (type)
			{
				case CommonDbType.BigInt:
				case CommonDbType.Integer:
				case CommonDbType.SignedTinyInt:
				case CommonDbType.SmallInt:
				case CommonDbType.Currency:
				case CommonDbType.Double:
				case CommonDbType.Float:

				case CommonDbType.Boolean:

				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
				case CommonDbType.Guid:
				case CommonDbType.Guid32Digits:

				case CommonDbType.Date:
				case CommonDbType.DateTime:
				case CommonDbType.DateTime2:
				case CommonDbType.DateTimeOffset:
				case CommonDbType.Time:

				case CommonDbType.Text:
				case CommonDbType.TinyInt:
				case CommonDbType.Xml:
				case CommonDbType.Json:
					size = 0;
					break;
				default:
					break;
			}

			#endregion

			if (size == 0) { return ReplacePlaceholders(_Templates[type][0], size, precision, scale); }

			foreach (KeyValuePair<Int32, String> entry in _Templates[type])
			{
				var capacity = entry.Key;
				var template = entry.Value;

				if (size <= capacity) { return ReplacePlaceholders(template, size, precision, scale); }
			}
			throw new NotSupportedException("Unsupported DbType '{0}'".FormatWith(type));
		}

		#endregion

		#region -- method EnsureHasList --

		private void EnsureHasList(CommonDbType type)
		{
			if (!_Templates.ContainsKey(type))
			{
				_Templates.Add(type, new SortedList<Int32, String>());
			}
		}

		#endregion

		#region -- method ReplacePlaceholders --

		private String ReplacePlaceholders(String value, Int32 size, Int32 precision, Int32 scale)
		{
			return value.Replace(SizePlaceholder, size.ToString())
									.Replace(PrecisionPlaceholder, precision.ToString())
									.Replace(ScalePlaceholder, scale.ToString());
		}

		#endregion
	}
}