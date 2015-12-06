using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal sealed class CompareConstraints
	{
		#region -- Fields --

		private readonly SchemaProvider _SchemaProvider;
		private readonly IList<CompareResult> _Results;
		private readonly GeneratorBase _Generator;

		#endregion

		#region -- 构造 --

		internal CompareConstraints(SchemaProvider schemaProvider, GeneratorBase generator, IList<CompareResult> _results)
		{
			_SchemaProvider = schemaProvider;
			_Generator = generator;
			_Results = _results;
		}

		#endregion

		internal void Execute(IDataTable entityTable, IDataTable dbTable)
		{
			// 只做主键约束比较

			var entityPKs = entityTable.PrimaryKeys.Select(e => e.ColumnName).ToArray();
			String[] dbPKs = null;
			var pkIndex = dbTable.Indexes.Find(e => e.PrimaryKey);
			if (pkIndex != null) { dbPKs = pkIndex.Columns; }

			var entityHasNoPK = entityPKs == null || entityPKs.Length == 0;
			var dbHasNoPK = dbPKs == null || dbPKs.Length == 0;

			// 无主键
			if (entityHasNoPK && dbHasNoPK) { return; }

			ConstraintDefinition pkConstraint = null;
			if (!entityHasNoPK)
			{
				var hasClustered = entityTable.Indexes.Where(e => !e.PrimaryKey).Any(e => e.Clustered);
				pkConstraint = new ConstraintDefinition(ConstraintType.PrimaryKey);
				pkConstraint.SqlServerConstraintType = hasClustered ? SqlServerConstraintType.NonClustered : SqlServerConstraintType.Clustered;
				pkConstraint.SchemaName = _SchemaProvider.Owner;
				pkConstraint.ConstraintName = "PK__{0}__{1}".FormatWith(entityTable.TableName.Cut(8), GuidHelper.GenerateId16().ToUpperInvariant());
				pkConstraint.TableName = entityTable.TableName;
			}
			if (dbHasNoPK)
			{
				// 新增
				_Results.Add(new CompareResult(ResultType.Add, SchemaObjectType.Constraint,
								_Generator.CreateConstraintSQL(pkConstraint),
								"{0}.{1}".FormatWith(dbTable.TableName, pkIndex.Name)));
			}
			else if (entityHasNoPK)
			{
				// 删除
				_Results.Add(new CompareResult(ResultType.Delete, SchemaObjectType.Constraint,
								_Generator.DeleteConstraintSQL(_SchemaProvider.Owner, dbTable.TableName, pkIndex.Name, true),
								"{0}.{1}".FormatWith(dbTable.TableName, pkIndex.Name)));
			}
			else if (!entityPKs.SequenceEqual(dbPKs, StringComparer.OrdinalIgnoreCase))
			{
				// 修改
				_Results.Add(new CompareResult(ResultType.Delete, SchemaObjectType.Constraint,
								_Generator.DeleteConstraintSQL(_SchemaProvider.Owner, dbTable.TableName, pkIndex.Name, true),
								"{0}.{1}".FormatWith(dbTable.TableName, pkIndex.Name)));
				_Results.Add(new CompareResult(ResultType.Add, SchemaObjectType.Constraint,
								_Generator.CreateConstraintSQL(pkConstraint),
								"{0}.{1}".FormatWith(dbTable.TableName, pkIndex.Name)));
			}
		}
	}
}
