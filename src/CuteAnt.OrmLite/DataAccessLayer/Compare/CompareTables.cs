using System;
using System.Collections.Generic;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal sealed class CompareTables
	{
		#region -- Fields --

		private readonly SchemaProvider _SchemaProvider;
		private readonly GeneratorBase _Generator;
		private readonly IDataTable[] _EntityTables;

		#endregion

		#region -- 构造 --

		internal CompareTables(SchemaProvider schemaProvider, GeneratorBase generator, IDataTable[] tables)
		{
			_SchemaProvider = schemaProvider;
			_Generator = generator;
			_EntityTables = tables;
		}

		#endregion

		internal List<List<CompareResult>> ExecuteResult()
		{
			var list = new List<List<CompareResult>>();

			foreach (var entityTable in _EntityTables)
			{
				var dbTable = _SchemaProvider.GetTable(entityTable.TableName);
				var remark = "{0}({1})".FormatWith(entityTable.TableName, entityTable.Description);

				var results = new List<CompareResult>();

				if (dbTable != null)
				{
					#region 修改表

					// 检查表注释
					if (!entityTable.Description.EqualIgnoreCase(dbTable.Description))
					{
						results.Add(new CompareResult(ResultType.Change, SchemaObjectType.Table,
								_Generator.AlterTableSQL(_SchemaProvider.Owner, entityTable),
								remark));
					}

					// 检查数据列
					var compareColumns = new CompareColumns(_SchemaProvider, _Generator, results);
					compareColumns.Execute(entityTable, dbTable);

					// 检查约束
					var compareConstraints = new CompareConstraints(_SchemaProvider, _Generator, results);
					compareConstraints.Execute(entityTable, dbTable);

					// 检查索引
					var compareIndexs = new CompareIndexes(_SchemaProvider, _Generator, results);
					compareIndexs.Execute(entityTable, dbTable);

					#endregion
				}
				else
				{
					#region 创建表

					// 没有字段的表不创建
					if (entityTable.Columns.Count > 0)
					{
						results.Add(new CompareResult(ResultType.Add, SchemaObjectType.Table,
								_Generator.CreateTableSQL(_SchemaProvider.Owner, entityTable),
								remark));

						// 没有索引，则不创建
						if (entityTable.Indexes.Count > 0)
						{
							results.Add(new CompareResult(ResultType.Add, SchemaObjectType.Index,
									_Generator.CreateIndexSQL(_SchemaProvider.Owner, entityTable),
									remark));
						}
					}

					// 加入内存表
					var sqliteDB = _SchemaProvider.DbInternal as SQLite;
					if (sqliteDB != null && sqliteDB.IsMemoryDatabase)
					{
						var sqliteSP = _SchemaProvider as SqliteSchemaProvider;
						if (sqliteSP != null) { sqliteSP.TryAddMemoryTable(entityTable); }
					}

					#endregion
				}

				list.Add(results);
			}

			return list;
		}
	}
}
