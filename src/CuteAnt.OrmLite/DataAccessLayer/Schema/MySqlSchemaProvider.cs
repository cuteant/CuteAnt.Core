using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.Log;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal partial class MySqlSchemaProvider : RemoteDbSchemaProvider
	{
		#region -- 架构检查 --

		/// <summary>已重载，数据库是否存在</summary>
		/// <returns></returns>
		public override Boolean DatabaseExist()
		{
			try
			{
				var session = DbInternal.CreateSession();
				var databaseName = session.DatabaseName;
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'".FormatWith(databaseName));
			}
			catch
			{
				try
				{
					return base.DatabaseExist();
				}
				catch { return true; }
			}
		}

		///// <summary>使用数据架构确定数据库是否存在，因为使用系统视图可能没有权限</summary>
		///// <param name="databaseName"></param>
		///// <param name="session"></param>
		///// <returns></returns>
		//internal override Boolean DatabaseExist(String databaseName, IDbSession session)
		//{
		//	var dt = GetSchema(_.Databases, new String[] { databaseName });
		//	return dt != null && dt.Rows != null && dt.Rows.Count > 0;
		//}

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists(@"select table_name from information_schema.tables where table_schema = SCHEMA() and table_name='{0}'".FormatWith(Helper.FormatSqlEscape(tableName)));
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists(@"select column_name from information_schema.columns
											where table_schema = SCHEMA() and table_name='{0}'
											and column_name='{1}'".FormatWith(
											Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(columnName)));
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists(@"select constraint_name from information_schema.table_constraints
											where table_schema = SCHEMA() and table_name='{0}'
											and constraint_name='{1}'".FormatWith(
											Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(constraintName)));
		}

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists(@"select index_name from information_schema.statistics
											where table_schema = SCHEMA() and table_name='{0}'
											and index_name='{1}'".FormatWith(
											Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(indexName)));
		}

		//public override Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue)
		//{
		//	string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
		//	return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = SCHEMA() AND TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}' AND COLUMN_DEFAULT LIKE '{2}'",
		//		 FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName), defaultValueAsString);
		//}

		#endregion

		#region -- 正向 --

		#region - 表架构 -

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		public override List<IDataTable> GetTables()
		{
			try
			{
				var session = DbInternal.CreateSession();
				var index = 1;
				List<IDataTable> tables = null;
				const String sql = "SELECT TABLE_NAME, TABLE_COMMENT FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=SCHEMA() AND TABLE_TYPE!='VIEW'";
				tables = session.ExecuteReader<List<IDataTable>>(rd =>
				{
					var list = new List<IDataTable>();
					while (rd.Read())
					{
						var dt = DAL.CreateTable();
						dt.ID = index;
						dt.TableName = rd.GetString(0);
						dt.Name = dt.TableName;
						if (!rd.IsDBNull(1))
						{
							dt.Description = rd.GetString(1);
						}
						dt.DbType = DbInternal.DbType;

						list.Add(dt);

						index++;
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
			var session = DbInternal.CreateSession();

			IDataTable table = null;
			var sql = "SELECT TABLE_NAME, TABLE_COMMENT FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=SCHEMA() AND TABLE_TYPE!='VIEW' AND TABLE_NAME='{0}'".FormatWith(tableName);

			table = session.ExecuteReader<IDataTable>(rd =>
			{
				if (rd.Read())
				{
					var dt = DAL.CreateTable();

					dt.TableName = rd.GetString(0);
					dt.Name = dt.TableName;
					if (!rd.IsDBNull(1))
					{
						dt.Description = rd.GetString(1);
					}
					dt.DbType = DbInternal.DbType;

					return dt;
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
			#region SQL 语句

			var sb = new StringBuilder(420);
			sb.Append("SELECT ");
			sb.Append("ORDINAL_POSITION,");
			sb.Append("COLUMN_NAME,");
			sb.Append("IS_NULLABLE,");
			sb.Append("DATA_TYPE,");
			sb.Append("CHARACTER_MAXIMUM_LENGTH,");
			sb.Append("CHARACTER_OCTET_LENGTH,");
			sb.Append("NUMERIC_PRECISION,");
			sb.Append("NUMERIC_SCALE,");
			sb.Append("DATETIME_PRECISION,");
			sb.Append("CHARACTER_SET_NAME,");
			sb.Append("COLLATION_NAME,");
			sb.Append("COLUMN_TYPE,");
			sb.Append("COLUMN_KEY,");
			sb.Append("EXTRA,");
			sb.Append("COLUMN_DEFAULT,");
			sb.Append("COLUMN_COMMENT ");
			sb.Append("FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=SCHEMA() AND ");
			sb.AppendFormat("TABLE_NAME='{0}' ", table.TableName);
			sb.Append("order by ORDINAL_POSITION");

			#endregion

			var session = DbInternal.CreateSession();
			var columns = session.ExecuteReader<List<IDataColumn>>(rd =>
			{

				var list = new List<IDataColumn>();
				while (rd.Read())
				{
					var field = table.CreateColumn();
					var properties = field.Properties;

					field.ID = rd.GetInt32(0); // ORDINAL_POSITION
					field.ColumnName = rd.GetString(1); // COLUMN_NAME
					field.Name = field.ColumnName;
					var isNullable = rd.GetString(2); // IS_NULLABLE
					if (isNullable.EqualIgnoreCase("YES"))
					{
						field.Nullable = true;
					}
					field.RawType = rd.GetString(3); // DATA_TYPE
					if (!rd.IsDBNull(4)) // CHARACTER_MAXIMUM_LENGTH
					{
						var length = rd.GetInt64(4);
						field.Length = length > Int32.MaxValue ? Int32.MaxValue : (Int32)length;
					}
					//if (!rd.IsDBNull(5)) // CHARACTER_OCTET_LENGTH
					//{
					//}
					if (!rd.IsDBNull(6)) // NUMERIC_PRECISION
					{
						field.Precision = rd.GetInt32(6);
					}
					if (!rd.IsDBNull(7)) // NUMERIC_SCALE
					{
						field.Scale = rd.GetInt32(7);
					}
					if (!rd.IsDBNull(8)) // DATETIME_PRECISION
					{
						field.Precision = rd.GetInt32(8);
					}
					if (!rd.IsDBNull(9)) // CHARACTER_SET_NAME
					{
						properties.Add("CHARACTER_SET_NAME", rd.GetString(9));
					}
					if (!rd.IsDBNull(10)) // COLLATION_NAME
					{
						properties.Add("COLLATION_NAME", rd.GetString(10));
					}
					if (!rd.IsDBNull(11)) // COLUMN_TYPE
					{
						properties.Add("COLUMN_TYPE", rd.GetString(11));
					}
					if (!rd.IsDBNull(12)) // COLUMN_KEY
					{
						field.PrimaryKey = rd.GetString(12).EqualIgnoreCase("PRI");
					}
					if (!rd.IsDBNull(13)) // EXTRA
					{
						field.Identity = rd.GetString(13).EqualIgnoreCase("auto_increment");
					}
					field.IsUnicode = Quoter.IsUnicode(field.RawType);

					if (!rd.IsDBNull(14)) // COLUMN_DEFAULT
					{
						field.Default = rd.GetString(9);
					}
					if (!rd.IsDBNull(15)) // COLUMN_COMMENT
					{
						field.Description = rd.GetString(15);
					}

					#region 数据类型识别

					switch (field.RawType.ToLowerInvariant())
					{
						case "char":
							if (field.Length == 36)
							{
								field.DbType = CommonDbType.Guid;
								field.DataType = typeof(Guid);
							}
							else if (field.Length == 32)
							{
								field.DbType = CommonDbType.Guid32Digits;
								field.DataType = typeof(Guid);
							}
							else
							{
								field.DbType = CommonDbType.StringFixedLength;
								field.DataType = typeof(String);
							}
							break;
						case "varchar":
							//if (field.Length > 21788)
							//{
							//	field.DbType = CommonDbType.AnsiString;
							//}
							//else
							//{
							field.DbType = CommonDbType.String;
							//}
							field.DataType = typeof(String);
							break;
						case "tinytext":
							field.DbType = CommonDbType.String; // 特殊处理
							field.DataType = typeof(String);
							break;
						case "text":
							field.DbType = CommonDbType.String; // 特殊处理
							field.DataType = typeof(String);
							break;
						case "mediumtext":
							field.DbType = CommonDbType.String; // 特殊处理
							field.DataType = typeof(String);
							break;
						case "longtext":
							field.DbType = CommonDbType.Text; // 特殊处理
							field.DataType = typeof(String);
							break;

						case "binary":
							if (field.Length == 16)
							{
								field.DbType = CommonDbType.CombGuid;
								field.DataType = typeof(CombGuid);
							}
							else
							{
								field.DbType = CommonDbType.BinaryFixedLength;
								field.DataType = typeof(Byte[]);
							}
							break;
						case "varbinary":
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;
						case "tinyblob":
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;
						case "blob":
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;
						case "mediumblob":
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;
						case "longblob":
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;

						case "date":
							field.DbType = CommonDbType.Date;
							field.DataType = typeof(DateTime);
							break;
						case "datetime":
							if (field.Precision > 3)
							{
								field.DbType = CommonDbType.DateTime2;
							}
							else
							{
								field.DbType = CommonDbType.DateTime;
							}
							field.DataType = typeof(DateTime);
							break;
						case "timestamp":
							field.DbType = CommonDbType.DateTimeOffset;
							field.DataType = typeof(DateTime);
							break;
						case "time":
							field.DbType = CommonDbType.Time;
							field.DataType = typeof(TimeSpan);
							break;

						case "dec":
						case "number":
						case "decimal":
							field.DbType = CommonDbType.Decimal;
							field.DataType = typeof(Decimal);
							break;

						case "tinyint":
							var fullRawType = field.Properties["COLUMN_TYPE"];
							if (!fullRawType.IsNullOrWhiteSpace() && fullRawType.ToLowerInvariant().Contains("unsigned"))
							{
								field.DbType = CommonDbType.SignedTinyInt;
							}
							else
							{
								field.DbType = CommonDbType.TinyInt;
							}
							field.DataType = typeof(Byte);
							break;
						case "smallint":
							field.DbType = CommonDbType.SmallInt;
							field.DataType = typeof(Int16);
							break;
						case "integer":
						case "int":
							field.DbType = CommonDbType.Integer;
							field.DataType = typeof(Int32);
							break;
						case "bigint":
							field.DbType = CommonDbType.BigInt;
							field.DataType = typeof(Int64);
							break;

						case "real": // 默认REAL视为DOUBLE PRECISION(非标准扩展)的同义词，除非SQL服务器模式包括REAL_AS_FLOAT选项。
						case "double precision":
						case "double":
							field.DbType = CommonDbType.Double;
							field.DataType = typeof(Double);
							break;
						case "float":
							if (field.Precision > 23)
							{
								field.DbType = CommonDbType.Float;
								field.DataType = typeof(Single);
							}
							else
							{
								field.DbType = CommonDbType.Double;
								field.DataType = typeof(Double);
							}
							break;

						//case "year":
						//case "bit":
						default:
							field.DbType = CommonDbType.Unknown;
							field.DataType = typeof(Object);
							break;
					}

					#endregion

					field.Fix();

					list.Add(field);
				}

				return list;
			}, sb.ToString());
			return columns;
		}

		#endregion

		#region - 索引架构 -

		/// <summary>获取索引</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected override List<IDataIndex> GetIndexes(IDataTable table)
		{
			var session = DbInternal.CreateSession();
			var databaseName = session.DatabaseName;
			var sql = "SHOW INDEX FROM `{0}`.`{1}`".FormatWith(databaseName, table.TableName);

			var allIndexs = session.ExecuteReader<List<IDataIndex>>(rd =>
			{
				var list = new List<IDataIndex>();
				var lastIndexName = String.Empty;
				IDataIndex lastIndex = null;
				var lastIndexColumns = new SortedList<Int32, String>();
				while (rd.Read())
				{
					var indexName = rd.GetString(2); // KEY_NAME
					if (!indexName.Equals(lastIndexName))
					{
						lastIndexName = indexName;
						if (lastIndex != null && lastIndexColumns.Count > 0)
						{
							lastIndex.Columns = lastIndexColumns.Values.ToArray();
							lastIndexColumns.Clear();
						}
						lastIndex = table.CreateIndex();
						list.Add(lastIndex);
						lastIndex.Name = indexName;
						if (indexName.EqualIgnoreCase("PRIMARY")) { lastIndex.PrimaryKey = true; }
						lastIndex.Unique = rd.GetInt64(1) == 0; // Non_unique
						lastIndexColumns.Add(rd.GetInt32(3), rd.GetString(4)); // Seq_in_index - Column_name
					}
					else
					{
						lastIndexColumns.Add(rd.GetInt32(3), rd.GetString(4)); // Seq_in_index - Column_name
					}
				}
				if (lastIndex != null && lastIndexColumns.Count > 0)
				{
					lastIndex.Columns = lastIndexColumns.Values.ToArray();
					lastIndexColumns.Clear();
				}
				return list;
			}, sql);

			return allIndexs;
		}

		#endregion

		#endregion

		#region -- 反向 --

		#region - Table -

		private void Fix(IDataTable table)
		{
			// MySQL InnoDB 使用了自增主键，不能在使用联合主键，因为InnoDB引擎规定必须包含只有该字段的索引
			// MyISAM 引擎不受这个限制
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
