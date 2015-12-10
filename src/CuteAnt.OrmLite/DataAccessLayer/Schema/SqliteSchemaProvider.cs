using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Exceptions;
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal partial class SqliteSchemaProvider : FileDbSchemaProvider
	{
		#region -- 属性 --

		#endregion

		#region -- 架构检查 --

		/// <summary>查询指定的 Schema 是否存在</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public override Boolean SchemaExists(String schemaName) { return true; }

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			var session = DbInternal.CreateSession();
			//return session.ExecuteScalar<Int32>("select count(*) from sqlite_master where name=\"{0}\" and type='table'".FormatWith(tableName)) > 0;
			return session.ExecuteScalar<Int32>("select count(*) from [main].[sqlite_master] where name='{0}' and type='table'".FormatWith(tableName)) > 0;
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			var session = DbInternal.CreateSession();
			var dataSet = session.Query("PRAGMA table_info([{0}])".FormatWith(tableName));
			return dataSet.Tables.Count > 0 && dataSet.Tables[0].Select(string.Format("Name='{0}'", columnName.Replace("'", "''"))).Length > 0;
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName) { return true; }

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			var session = DbInternal.CreateSession();
			return session.ExecuteScalar<Int32>("select count(*) from [main].[sqlite_master] where name='{0}' and tbl_name='{1}' and type='index'".FormatWith(indexName, tableName)) > 0;
		}

		/// <summary>根据序列名称检查序列是否存在</summary>
		/// <param name="sequenceName">序列名称</param>
		/// <returns></returns>
		public override Boolean SequenceExists(String sequenceName) { return true; }

		//public override Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue) { return false; }

		#endregion

		#region -- 正向 --

		#region - 表架构 -

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		public override List<IDataTable> GetTables()
		{
			try
			{
				// 特殊处理内存数据库
				if ((DbInternal as SQLite).IsMemoryDatabase) { return _MemoryTables; }

				var session = DbInternal.CreateSession();
				var sql = "SELECT [name], [rowid] FROM [main].[sqlite_master] WHERE [type]='table'";
				var tables = session.ExecuteReader<List<IDataTable>>(rd =>
				{
					var list = new List<IDataTable>();
					while (rd.Read())
					{
						var name = rd.GetString(0);
						// 系统表不做处理
						//if (String.Compare(name, 0, "SQLITE_", 0, 7, StringComparison.OrdinalIgnoreCase) == 0) { continue; }
						if (name.StartsWithIgnoreCase("SQLITE_")) { continue; }

						var dt = DAL.CreateTable();

						dt.ID = rd.GetInt32(1);
						dt.Name = name;
						dt.TableName = name;
						dt.DbType = DbInternal.DbType;

						list.Add(dt);
					}
					return list;
				}, sql);

				foreach (var item in tables)
				{
					FillDataTable(item);
				}

				// 表间关系处理
				ModelHelper.Connect(tables);

				return tables;
			}
			catch (Exception ex)
			{
				DAL.WriteLog(ex);
				throw new OrmLiteDbSchemaException(this, "取得所有表构架出错！", ex);
			}
		}

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		protected override List<IDataTable> OnGetTables(ICollection<String> names)
		{
			try
			{
				// 特殊处理内存数据库
				if ((DbInternal as SQLite).IsMemoryDatabase)
				{
					return _MemoryTables.Where(t => names.Contains(t.TableName)).ToList();
				}

				var list = new List<IDataTable>(names.Count);
				foreach (var item in names)
				{
					var table = GetTable(item);
					if (table != null) { list.Add(table); }
				}

				// 表间关系处理
				ModelHelper.Connect(list);

				return list;
			}
			catch (Exception ex)
			{
				DAL.WriteLog(ex);
				throw new OrmLiteDbSchemaException(this, "取得所有表构架出错！", ex);
			}
		}

		/// <summary>取得表构架</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override IDataTable GetTable(String tableName)
		{
			// 特殊处理内存数据库
			if ((DbInternal as SQLite).IsMemoryDatabase)
			{
				return _MemoryTables.Find(t => tableName.EqualIgnoreCase(t.TableName));
			}

			var session = DbInternal.CreateSession();
			var sql = "SELECT [name], [rowid] FROM [main].[sqlite_master] WHERE name='{0}' AND [type]='table'".FormatWith(tableName);
			var table = session.ExecuteReader<IDataTable>(rd =>
			{
				if (rd.Read())
				{
					var name = rd.GetString(0);
					//// 系统表不做处理
					//if (String.Compare(name, 0, "SQLITE_", 0, 7, StringComparison.OrdinalIgnoreCase) == 0) { return null; }
					//if (String.Compare(name, tableName, StringComparison.OrdinalIgnoreCase) == 0)
					//{
					var dt = DAL.CreateTable();

					dt.ID = rd.GetInt32(1);
					dt.Name = name;
					dt.TableName = name;
					dt.DbType = DbInternal.DbType;

					return dt;
					//	}
				}
				return null;
			}, sql);

			if (table == null) { return null; }

			FillDataTable(table);

			return table;
		}

		private void FillDataTable(IDataTable table)
		{
			var columns = GetFields(table);
			if (columns != null && columns.Count > 0)
			{
				table.Columns.AddRange(columns);
			}

			var indexes = GetIndexes(table);
			if (indexes != null && indexes.Count > 0)
			{
				table.Indexes.AddRange(indexes);
			}

			// 先修正一次关系数据
			table.Fix();
		}

		#endregion

		#region - 字段架构 -

		/// <summary>取得指定表的所有列构架</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected override List<IDataColumn> GetFields(IDataTable table)
		{
			var cmd = new SQLiteCommand("SELECT * FROM [main].[{0}]".FormatWith(table.TableName));
			var session = DbInternal.CreateSession();
			var columns = session.ExecuteReader<List<IDataColumn>>(reader =>
			{
				var rd = reader as SQLiteDataReader;
				if (rd == null) { return null; }

				List<IDataColumn> list = null;
				using (DataTable tblSchema = rd.GetSchemaTable(true, true))
				{
					list = new List<IDataColumn>(tblSchema.Rows.Count);
					foreach (DataRow schemaRow in tblSchema.Rows)
					{
						var field = table.CreateColumn();

						field.ID = Convert.ToInt32(schemaRow[SchemaTableColumn.ColumnOrdinal]);
						field.ColumnName = schemaRow[SchemaTableColumn.ColumnName].ToString();
						field.Name = field.ColumnName;
						field.Identity = Convert.ToBoolean(schemaRow[SchemaTableOptionalColumn.IsAutoIncrement]);
						field.PrimaryKey = Convert.ToBoolean(schemaRow[SchemaTableColumn.IsKey]);
						var rawType = schemaRow["DataTypeName"].ToString();

						#region 修正

						//switch (rawType.ToUpperInvariant())
						//{
						//	case "CHAR":

						//	case "VARCHAR":
						//	case "VARCHAR2":

						//	case "NCHAR":

						//	case "NVARCHAR":
						//	case "CLOB":
						//	case "LONGCHAR":
						//	case "LONGTEXT":
						//	case "LONGVARCHAR":
						//	case "MEMO":
						//	case "NOTE":
						//	case "STRING":

						//	case "TEXT":
						//	case "NTEXT":

						//	case "BIT":
						//	case "BOOL":
						//	case "BOOLEAN":
						//	case "LOGICAL":
						//	case "YESNO":
						//		rawType = "BIT";
						//		break;

						//	case "BIT":
						//	case "BIT":
						//	case "BIT":
						//	case "BIT":
						//	case "BIT":
						//	case "BIT":
						//	case "BIT":
						//	case "BIT":
						//	default:
						//		break;
						//}

						#endregion

						field.RawType = rawType;
						field.IsUnicode = Quoter.IsUnicode(rawType);
						var precision = schemaRow[SchemaTableColumn.NumericPrecision];
						if (precision != null && !DBNull.Value.Equals(precision))
						{
							field.Precision = Convert.ToInt32(precision);
						}
						var scale = schemaRow[SchemaTableColumn.NumericScale];
						if (scale != null && !DBNull.Value.Equals(scale))
						{
							field.Scale = Convert.ToInt32(scale);
						}
						field.Length = Convert.ToInt32(schemaRow[SchemaTableColumn.ColumnSize]);
						if (field.Length < 0) { field.Length = 0; }

						field.Nullable = Convert.ToBoolean(schemaRow[SchemaTableColumn.AllowDBNull]);
						field.Default = schemaRow[SchemaTableOptionalColumn.DefaultValue].ToString();

						field.DbType = Helper.ConvertDbType((DbType)schemaRow[SchemaTableColumn.ProviderType]);
						field.DataType = (Type)schemaRow[SchemaTableColumn.DataType];
						//// 如果数据库里面是integer或者autoincrement，识别类型是Int64，又是自增，则改为Int32，保持与大多数数据库的兼容
						//if (field.Identity && field.DataType == typeof(Int64) && field.RawType.EqualIgnoreCase("integer", "autoincrement"))
						//{
						//	field.DataType = typeof(Int32);
						//}
						// 检查是否已正确识别类型
						if (field.DataType == null)
						{
							DAL.Logger.LogWarning("无法识别{0}.{1}的类型{2}！", table.TableName, field.ColumnName, rawType);
						}

						field.Fix();

						list.Add(field);
					}
				}

				return list;
			}, cmd, CommandBehavior.SchemaOnly);
			return columns;
		}

		#endregion

		#region - 索引架构 -

		/// <summary>获取索引</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected override List<IDataIndex> GetIndexes(IDataTable table)
		{
			//var dtIndexs = GetSchema(_.Indexes, new String[] { null, table.TableName });
			//if (dtIndexs == null) { return null; }
			//var count = dtIndexs.Rows.Count;
			//if (count < 1) { return null; }

			//var indexs = new List<IDataIndex>(count);
			//foreach (DataRow indexRow in dtIndexs.Rows)
			//{
			//	var di = table.CreateIndex();
			//	di.Name = indexRow["INDEX_NAME"].ToString();
			//	di.PrimaryKey = Convert.ToBoolean(indexRow["PRIMARY_KEY"]);
			//	di.Unique = Convert.ToBoolean(indexRow["UNIQUE"]);
			//}

			var allIndexs = new List<IDataIndex>();

			List<Int32> primaryKeys = null;
			var maybeRowId = false;
			// First, look for any rowid indexes -- which sqlite defines are INTEGER PRIMARY KEY columns.
			// Such indexes are not listed in the indexes list but count as indexes just the same.
			try
			{
				var cmdTable = new SQLiteCommand("PRAGMA [main].table_info([{0}])".FormatWith(table.TableName));
				var sessionTable = DbInternal.CreateSession();
				primaryKeys = sessionTable.ExecuteReader<List<Int32>>(rdTable =>
				{
					var list = new List<Int32>();
					while (rdTable.Read())
					{
						if (rdTable.GetInt32(5) != 0)
						{
							list.Add(rdTable.GetInt32(0));

							// If the primary key is of type INTEGER, then its a rowid and we need to make a fake index entry for it.
							if (String.Compare(rdTable.GetString(2), "INTEGER", StringComparison.OrdinalIgnoreCase) == 0) { maybeRowId = true; }
						}
					}
					return list;
				}, cmdTable);
			}
			catch { }

			if (primaryKeys.Count == 1 && maybeRowId)
			{
				var di = table.CreateIndex();
				di.Name = "sqlite_master_PK_{0}".FormatWith(table.TableName);
				di.PrimaryKey = true;
				di.Unique = true;
				di.Columns = new String[] { table.Columns.Where(f => f.PrimaryKey).First().ColumnName };

				allIndexs.Add(di);

				primaryKeys.Clear();
			}

			var cmd = new SQLiteCommand("PRAGMA [main].index_list([{0}])".FormatWith(table.TableName));
			var session = DbInternal.CreateSession();
			var indexs = session.ExecuteReader<List<IDataIndex>>(rd =>
			{
				var list = new List<IDataIndex>();
				while (rd.Read())
				{
					var di = table.CreateIndex();
					di.Name = rd.GetString(1);
					di.PrimaryKey = false;
					di.Unique = rd.GetValue(2).ToBoolean();

					var names = new List<String>();
					using (SQLiteCommand cmdDetails = new SQLiteCommand("PRAGMA [main].index_info([{0}])".FormatWith(di.Name), session.Conn as SQLiteConnection))
					using (SQLiteDataReader rdDetails = cmdDetails.ExecuteReader())
					{
						int nMatches = 0;
						while (rdDetails.Read())
						{
							names.Add(rdDetails.GetString(2)); // name

							// Now for the really hard work.  Figure out which index is the primary key index.
							// The only way to figure it out is to check if the index was an autoindex and if we have a non-rowid
							// primary key, and all the columns in the given index match the primary key columns
							if (primaryKeys.Count > 0 && di.Name.StartsWithIgnoreCase("sqlite_autoindex_" + table.TableName))
							{
								if (primaryKeys.Contains(rdDetails.GetInt32(1)) == false) // cid
								{
									nMatches = 0;
									break;
								}
								nMatches++;
							}
						}
						if (primaryKeys.Count > 0 && di.Name.StartsWithIgnoreCase("sqlite_autoindex_" + table.TableName))
						{
							if (nMatches == primaryKeys.Count)
							{
								di.PrimaryKey = true;
								primaryKeys.Clear();
							}
						}
					}
					di.Columns = names.ToArray();

					list.Add(di);
				}
				return list;
			}, cmd);

			//foreach (var item in indexs)
			//{
			//	if (!session.Opened) { session.Open(); }
			//	var dtIndex = session.Conn.GetSchema(_.IndexColumns, new String[] { null, table.TableName, item.Name });
			//	session.AutoClose();
			//	DAL.Logger.LogWarning("{0} - {1} - {2}", table.TableName, item.Name, dtIndex.Rows.Count);
			//	var list = new List<String>();
			//	foreach (DataRow indexRow in dtIndex.Rows)
			//	{
			//		DAL.Logger.LogWarning("{0} - {1} - {2}", table.TableName, item.Name, indexRow["COLUMN_NAME"].ToString());
			//		list.Add(indexRow["COLUMN_NAME"].ToString());
			//	}
			//	item.Columns = list.ToArray();
			//}

			if (indexs != null) { allIndexs.AddRange(indexs); }

			return allIndexs;
		}

		#endregion

		#endregion

		#region -- 反向 --

		#region - Database -

		/// <summary>已重载，创建数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <param name="databasePath">数据库路径</param>
		public override void CreateDatabase(String databaseName, String databasePath)
		{
			if (!(DbInternal as SQLite).IsMemoryDatabase) { base.CreateDatabase(databaseName, databasePath); }
		}

		/// <summary>已重载，删除数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <returns></returns>
		public override void DropDatabase(String databaseName)
		{
			if (!(DbInternal as SQLite).IsMemoryDatabase) { base.DropDatabase(databaseName); }
		}

		#endregion

		#region - Table -

		private List<IDataTable> _MemoryTables = new List<IDataTable>();

		/// <summary>设置表模型，检查数据表是否匹配表模型，反向工程</summary>
		/// <param name="tables"></param>
		/// <param name="setting"></param>
		internal override void OnSetTables(IDataTable[] tables, NegativeSetting setting)
		{
			foreach (var item in tables)
			{
				Fix(item);
			}

			base.OnSetTables(tables, setting);
		}

		internal Boolean TryAddMemoryTable(IDataTable table)
		{
			if (table == null) { return false; }

			if (_MemoryTables.Any(t => t.TableName.EqualIgnoreCase(table.TableName))) { return false; }

			_MemoryTables.Add(table);

			return true;
		}

		private void Fix(IDataTable table)
		{
			foreach (var item in table.Columns)
			{
				// 自增字段必须是主键
				if (item.Identity && !item.PrimaryKey)
				{
					// 取消所有主键
					item.Table.Columns.ForEach(dc => dc.PrimaryKey = false);

					// 自增字段作为主键
					item.PrimaryKey = true;
					break;
				}
			}
		}

		#endregion

		#endregion
	}
}
