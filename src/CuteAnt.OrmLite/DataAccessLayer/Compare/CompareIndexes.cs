using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal sealed class CompareIndexes
	{
		#region -- Fields --

		private readonly SchemaProvider _SchemaProvider;
		private readonly IList<CompareResult> _Results;
		private readonly GeneratorBase _Generator;

		#endregion

		#region -- 构造 --

		internal CompareIndexes(SchemaProvider schemaProvider, GeneratorBase generator, IList<CompareResult> _results)
		{
			_SchemaProvider = schemaProvider;
			_Generator = generator;
			_Results = _results;
		}

		#endregion

		internal void Execute(IDataTable entityTable, IDataTable dbTable)
		{
			#region 删除索引

			if (dbTable.Indexes.Count > 0)
			{
				for (Int32 i = dbTable.Indexes.Count - 1; i >= 0; i--)
				{
					var item = dbTable.Indexes[i];

					// 计算的索引不需要删除
					if (item.Computed) { continue; }

					// 主键的索引不能删
					if (item.PrimaryKey) { continue; }

					var di = ModelHelper.GetIndex(entityTable, item.Columns);
					if (di != null && di.Unique == item.Unique) { continue; }

					_Results.Add(new CompareResult(ResultType.Delete, SchemaObjectType.Index,
									_Generator.DeleteIndexSQL(_SchemaProvider.Owner, dbTable.TableName, item.Name),
									"{0}.{1}".FormatWith(dbTable.TableName, item.Name)));

					dbTable.Indexes.RemoveAt(i);
				}
			}

			#endregion

			#region 新增索引

			if (entityTable.Indexes.Count > 0)
			{
				foreach (var item in entityTable.Indexes)
				{
					if (item.PrimaryKey) { continue; }

					var di = ModelHelper.GetIndex(dbTable, item.Columns);

					// 计算出来的索引，也表示没有，需要创建
					if (di != null && di.Unique == item.Unique && !di.Computed) { continue; }
					//// 如果这个索引的唯一字段是主键，则无需建立索引
					//if (item.Columns.Length == 1 && entitytable.GetColumn(item.Columns[0]).PrimaryKey) { continue; }
					// 如果索引全部就是主键，无需创建索引
					if (entityTable.GetColumns(item.Columns).All(e => e.PrimaryKey)) { continue; }

					var index = IndexDefinition.Create(_SchemaProvider.Owner, item);
					_Results.Add(new CompareResult(ResultType.Add, SchemaObjectType.Index,
									_Generator.CreateIndexSQL(index),
									"{0}.{1}".FormatWith(dbTable.TableName, item.Name)));

					if (di == null)
					{
						dbTable.Indexes.Add(item.Clone(dbTable));
					}
					else
					{
						di.Computed = false;
					}
				}
			}

			#endregion
		}
	}
}
