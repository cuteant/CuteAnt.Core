using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CuteAnt.Log;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal sealed class CompareColumns
	{
		#region -- Fields --

		private readonly SchemaProvider _SchemaProvider;
		private readonly IList<CompareResult> _Results;
		private readonly GeneratorBase _Generator;

		#endregion

		#region -- 构造 --

		internal CompareColumns(SchemaProvider schemaProvider, GeneratorBase generator, IList<CompareResult> _results)
		{
			_SchemaProvider = schemaProvider;
			_Generator = generator;
			_Results = _results;
		}

		#endregion

		internal void Execute(IDataTable entityTable, IDataTable dbTable)
		{
			var entityColumns = entityTable.Columns;
			var dbColumns = dbTable.Columns;

			#region 准备工作

			var entitydic = new Dictionary<String, IDataColumn>(StringComparer.OrdinalIgnoreCase);
			foreach (var item in entityColumns)
			{
				if (entitydic.ContainsKey(item.ColumnName))
				{
					DAL.Logger.Warn("《{0}》实体中存在重复列名，请检查《{1}》表《{2}》属性的ColumnName配置（目前配置为：{3}）。",
														entityTable.Name, entityTable.TableName, item.Name, item.ColumnName);
					continue;
				}
				entitydic.Add(item.ColumnName, item);
			}
			var dbdic = new Dictionary<String, IDataColumn>(StringComparer.OrdinalIgnoreCase);
			foreach (var item in dbColumns)
			{
				dbdic.Add(item.ColumnName, item);
			}

			#endregion

			#region 新增列

			foreach (var item in entityColumns)
			{
				if (!dbdic.ContainsKey(item.ColumnName))
				{
					_Results.Add(new CompareResult(ResultType.Add, SchemaObjectType.Column,
									_Generator.CreateColumnSQL(_SchemaProvider.Owner, item),
									"{0}({1})".FormatWith(item.ColumnName, item.Description)));
				}
			}

			#endregion

			#region 删除列

			for (int i = dbColumns.Count - 1; i >= 0; i--)
			{
				var item = dbColumns[i];
				if (!entitydic.ContainsKey(item.ColumnName))
				{
					if (_SchemaProvider.DbInternal.DbType == DatabaseType.SQLite || _SchemaProvider.DbInternal.DbType == DatabaseType.Firebird)
					{
						if (!_Results.Any(e => e.ResultType == ResultType.RebulidTable))
						{
							_Results.Add(new CompareResult(ResultType.RebulidTable, SchemaObjectType.Table,
											_SchemaProvider.ReBuildTable(entityTable, dbTable),
											"{0}，因无法执行：删除字段：{1}({2})".FormatWith(dbTable.TableName, item.ColumnName, item.Description)));
						}
						return;
					}
					else
					{
						CheckAndDropIndex(dbTable, item.ColumnName);
						_Results.Add(new CompareResult(ResultType.Delete, SchemaObjectType.Column,
										_Generator.DropColumnSQL(_SchemaProvider.Owner, entityTable.TableName, item.ColumnName),
										"{0}({1})".FormatWith(item.ColumnName, item.Description)));
					}
				}
			}

			#endregion

			#region 修改列

			// 开发时的实体数据库
			var entityDb = DbFactory.Create(entityTable.DbType);

			foreach (var entityColumn in entityTable.Columns)
			{
				IDataColumn dbColumn = null;
				if (!dbdic.TryGetValue(entityColumn.ColumnName, out dbColumn)) { continue; }

				// 标识列修改需要重建表
				if (entityColumn.Identity != dbColumn.Identity)
				{
					if (!_Results.Any(e => e.ResultType == ResultType.RebulidTable))
					{
						_Results.Add(new CompareResult(ResultType.RebulidTable, SchemaObjectType.Table,
										_SchemaProvider.ReBuildTable(entityTable, dbTable),
										"{0}，因无法执行：修改标识列：{1}({2})".FormatWith(dbTable.TableName, dbColumn.ColumnName, dbColumn.Description)));
						return;
					}
				}

				var isChanged = false;
				String remark = null;
				if (_Generator.IsColumnTypeChanged(entityColumn, dbColumn))
				{
					isChanged = true;
					remark = "字段{0}.{1}类型需要由{2}改变为{3}".FormatWith(entityTable.Name, entityColumn.Name, dbColumn.DataType, entityColumn.DataType);
				}
				else
				{
					// 主键在约束检查，在此忽略
					//if (entityColumn.PrimaryKey != dbColumn.PrimaryKey) { return true; }

					if (entityColumn.Nullable != dbColumn.Nullable && !entityColumn.Identity && !entityColumn.PrimaryKey)
					{
						isChanged = true;
						remark = "字段{0}.{1} 允许空 变更".FormatWith(entityTable.Name, entityColumn.Name);
					}
				}
				if (isChanged)
				{
					// SQLite无法修改字段
					if (_SchemaProvider.DbInternal.DbType == DatabaseType.SQLite)
					{
						if (!_Results.Any(e => e.ResultType == ResultType.RebulidTable))
						{
							_Results.Add(new CompareResult(ResultType.RebulidTable, SchemaObjectType.Table,
											_SchemaProvider.ReBuildTable(entityTable, dbTable),
											"{0}，因无法执行：修改数据列：{1}({2})".FormatWith(dbTable.TableName, dbColumn.ColumnName, dbColumn.Description)));
						}
						return;
					}
					else
					{
						CheckAndDropIndex(dbTable, entityColumn.ColumnName);
						_Results.Add(new CompareResult(ResultType.Change, SchemaObjectType.Column,
										_Generator.AlterColumnSQL(_SchemaProvider.Owner, entityColumn),
										remark));
					}
				}
			}

			#endregion
		}

		/// <summary>检查需要改变的数据列，目标数据表有索引使用它，先删除索引</summary>
		/// <param name="dbTable"></param>
		/// <param name="columnName"></param>
		private void CheckAndDropIndex(IDataTable dbTable, String columnName)
		{
			if (dbTable.Indexes.Count > 0)
			{
				for (Int32 i = dbTable.Indexes.Count - 1; i >= 0; i--)
				{
					var item = dbTable.Indexes[i];

					// 计算的索引不需要删除
					if (item.Computed) { continue; }

					// 主键的索引不能删
					if (item.PrimaryKey) { continue; }

					if (item.Columns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
					{
						_Results.Add(new CompareResult(ResultType.Delete, SchemaObjectType.Index,
										_Generator.DeleteIndexSQL(_SchemaProvider.Owner, dbTable.TableName, item.Name),
										"{0}.{1}".FormatWith(dbTable.TableName, item.Name)));

						dbTable.Indexes.RemoveAt(i);
					}
				}
			}
		}
	}
}
