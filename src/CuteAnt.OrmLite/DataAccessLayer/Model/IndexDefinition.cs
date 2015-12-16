using System;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class IndexDefinition
	{
		internal virtual String Name { get; set; }

		internal virtual String SchemaName { get; set; }

		internal virtual String TableName { get; set; }

		internal virtual Boolean IsUnique { get; set; }

		internal Boolean IsClustered { get; set; }

		internal virtual HashSet<IndexColumnDefinition> Columns { get; set; }

		internal virtual HashSet<String> Includes { get; set; }

		internal IndexDefinition()
		{
			Columns = new HashSet<IndexColumnDefinition>();
			Includes = new HashSet<String>();
		}

		internal static IndexDefinition Create(String schemaName, IDataIndex dataIndex)
		{
			var idx = new IndexDefinition();

			idx.Name = dataIndex.Name;
			idx.SchemaName = schemaName;
			idx.TableName = dataIndex.Table.TableName;
			idx.IsUnique = dataIndex.Unique;
			idx.IsClustered = dataIndex.Clustered;
			idx.Columns.UnionWith(dataIndex.Columns.Select(e => new IndexColumnDefinition(e)));

			return idx;
		}

		internal static IList<IndexDefinition> Create(String schemaName, IDataTable table)
		{
			var list = new List<IndexDefinition>(table.Indexes.Count);

			foreach (var item in table.Indexes)
			{
				if (item.PrimaryKey || item.Computed) { continue; }

				if (item.Columns == null || item.Columns.Length < 1) { continue; }

				//// 如果这个索引的唯一字段是主键，则无需建立索引
				//if (item.Columns.Length == 1 && table.GetColumn(item.Columns[0]).PrimaryKey) { continue; }
				// 如果索引全部就是主键，无需创建索引
				if (table.GetColumns(item.Columns).All(e => e.PrimaryKey)) { continue; }

				list.Add(Create(schemaName, item));
			}

			return list;
		}

		internal Boolean AddIndexColumn(String columnName, Boolean isDescending = false)
		{
			return Columns.Add(new IndexColumnDefinition(columnName, isDescending));
		}

		internal void AddIndexColumns(params String[] columnNames)
		{
			if (columnNames == null) { return; }
			Columns.UnionWith(columnNames.Select(e => new IndexColumnDefinition(e)));
		}

		internal void AddIncludeColumns(params String[] columnNames)
		{
			if (columnNames == null) { return; }
			Includes.UnionWith(columnNames);
		}
	}
}