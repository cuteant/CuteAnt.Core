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
using System.Security.Cryptography;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class FirebirdTruncator
	{
		private readonly Boolean enabled;
		private readonly Boolean packKeyNames;

		internal FirebirdTruncator(Boolean enabled)
			: this(enabled, true)
		{
		}

		internal FirebirdTruncator(Boolean enabled, Boolean packKeyNames)
		{
			this.enabled = enabled;
			this.packKeyNames = packKeyNames;
		}

		//internal void Truncate(CreateTableExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	TruncateColumns(expression.Columns);
		//}

		//internal void Truncate(AlterTableExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//}

		//internal void Truncate(DeleteTableExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//}

		//internal void Truncate(RenameTableExpression expression)
		//{
		//	expression.OldName = Truncate(expression.OldName);
		//	expression.NewName = Truncate(expression.NewName);
		//}

		//internal void Truncate(IDataColumn column)
		//{
		//	column.Name = Truncate(column.ColumnName);
		//	column.Table.TableName = Truncate(column.Table.TableName);
		//	if (column.PrimaryKey)
		//	{
		//		//column.PrimaryKeyName = packKeyNames ? Pack(column.PrimaryKeyName) : Truncate(column.PrimaryKeyName);
		//	}
		//}

		//internal void Truncate(CreateColumnExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	Truncate(expression.Column);
		//}

		//internal void Truncate(AlterColumnExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	Truncate(expression.Column);
		//}

		//internal void Truncate(DeleteColumnExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	expression.ColumnNames = TruncateNames(expression.ColumnNames);
		//}

		//internal void Truncate(RenameColumnExpression expression)
		//{
		//	expression.OldName = Truncate(expression.OldName);
		//	expression.NewName = Truncate(expression.NewName);
		//	expression.TableName = Truncate(expression.TableName);
		//}

		//internal void Truncate(IndexDefinition index)
		//{
		//	index.TableName = Truncate(index.TableName);
		//	index.Name = packKeyNames ? Pack(index.Name) : Truncate(index.Name);
		//	index.Columns.ToList().ForEach(x => x.Name = Truncate(x.Name));
		//}

		//internal void Truncate(CreateIndexExpression expression)
		//{
		//	Truncate(expression.Index);
		//}

		//internal void Truncate(DeleteIndexExpression expression)
		//{
		//	Truncate(expression.Index);
		//}

		//internal void Truncate(ConstraintDefinition constraint)
		//{
		//	constraint.TableName = Truncate(constraint.TableName);
		//	constraint.ConstraintName = packKeyNames ? Pack(constraint.ConstraintName) : Truncate(constraint.ConstraintName);
		//	constraint.Columns = TruncateNames(constraint.Columns);
		//}

		//internal void Truncate(CreateConstraintExpression expression)
		//{
		//	Truncate(expression.Constraint);
		//}

		//internal void Truncate(DeleteConstraintExpression expression)
		//{
		//	Truncate(expression.Constraint);
		//}

		//internal void Truncate(ForeignKeyDefinition foreignKey)
		//{
		//	foreignKey.Name = packKeyNames ? Pack(foreignKey.Name) : Truncate(foreignKey.Name);
		//	foreignKey.PrimaryTable = Truncate(foreignKey.PrimaryTable);
		//	foreignKey.PrimaryColumns = TruncateNames(foreignKey.PrimaryColumns);
		//	foreignKey.ForeignTable = Truncate(foreignKey.ForeignTable);
		//	foreignKey.ForeignColumns = TruncateNames(foreignKey.ForeignColumns);
		//}

		//internal void Truncate(CreateForeignKeyExpression expression)
		//{
		//	Truncate(expression.ForeignKey);
		//}

		//internal void Truncate(DeleteForeignKeyExpression expression)
		//{
		//	Truncate(expression.ForeignKey);
		//}

		//internal void Truncate(AlterDefaultConstraintExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	expression.ColumnName = Truncate(expression.ColumnName);
		//}

		//internal void Truncate(DeleteDefaultConstraintExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	expression.ColumnName = Truncate(expression.ColumnName);
		//}

		//internal void Truncate(SequenceDefinition sequence)
		//{
		//	sequence.Name = Truncate(sequence.Name);
		//}

		//internal void Truncate(CreateSequenceExpression expression)
		//{
		//	Truncate(expression.Sequence);
		//}

		//internal void Truncate(DeleteSequenceExpression expression)
		//{
		//	expression.SequenceName = Truncate(expression.SequenceName);
		//}

		//internal void Truncate(InsertDataExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	var insertions = new List<InsertionDataDefinition>();
		//	foreach (var insertion in expression.Rows)
		//	{
		//		var newInsertion = new InsertionDataDefinition();
		//		foreach (var data in insertion)
		//		{
		//			newInsertion.Add(new KeyValuePair<String, Object>(Truncate(data.Key), data.Value));
		//		}
		//		insertions.Add(newInsertion);
		//	}
		//	expression.Rows.Clear();
		//	expression.Rows.AddRange(insertions);
		//}

		//internal void Truncate(DeleteDataExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	var deletions = new List<DeletionDataDefinition>();
		//	foreach (var deletion in expression.Rows)
		//	{
		//		var newDeletion = new DeletionDataDefinition();
		//		foreach (var data in deletion)
		//		{
		//			newDeletion.Add(new KeyValuePair<String, Object>(Truncate(data.Key), data.Value));
		//		}
		//		deletions.Add(newDeletion);
		//	}
		//	expression.Rows.Clear();
		//	expression.Rows.AddRange(deletions);
		//}

		//internal void Truncate(UpdateDataExpression expression)
		//{
		//	expression.TableName = Truncate(expression.TableName);
		//	var newSet = new List<KeyValuePair<String, Object>>();
		//	foreach (var data in expression.Set)
		//	{
		//		newSet.Add(new KeyValuePair<String, Object>(Truncate(data.Key), data.Value));
		//	}
		//	expression.Set.Clear();
		//	expression.Set.AddRange(newSet);
		//	if (!expression.IsAllRows)
		//	{
		//		var newWhere = new List<KeyValuePair<String, Object>>();
		//		foreach (var data in expression.Where)
		//		{
		//			newWhere.Add(new KeyValuePair<String, Object>(Truncate(data.Key), data.Value));
		//		}
		//		expression.Where.Clear();
		//		expression.Where.AddRange(newWhere);
		//	}
		//}

		#region Helpers

		internal ICollection<String> TruncateNames(ICollection<String> names)
		{
			var ret = new List<String>();
			foreach (var item in names)
			{
				ret.Add(Truncate(item));
			}
			return ret;
		}

		internal void TruncateColumns(ICollection<IDataColumn> columns)
		{
			foreach (var colDef in columns)
			{
				//Truncate(colDef);
			}
		}

		internal String Truncate(String name)
		{
			if (!name.IsNullOrWhiteSpace())
			{
				if (name.Length > FirebirdOptions.MaxNameLength)
				{
					if (!enabled) { throw new ArgumentException(String.Format("Name too long: {0}", name)); }

					return name.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, name.Length));
				}
			}
			return name;
		}

		internal String Pack(String name)
		{
			if (!name.IsNullOrWhiteSpace())
			{
				if (name.Length > FirebirdOptions.MaxNameLength)
				{
					if (!enabled) { throw new ArgumentException(String.Format("Name too long: {0}", name)); }

					var byteHash = MD5.Create().ComputeHash(System.Text.Encoding.ASCII.GetBytes(name));
					var hash = Convert.ToBase64String(byteHash);
					var sb = new StringBuilder(hash.Length);
					var hLength = hash.Length;
					for (Int32 i = 0; i < hLength; i++)
					{
						var c = hash[i];
						if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
						{
							sb.Append(c);
						}
					}
					hash = sb.ToString();
					return String.Format("fk_{0}", hash.Substring(0, Math.Min(28, hash.Length)));
				}
			}
			return name;
		}

		#endregion
	}
}