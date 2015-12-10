using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.Reflection;
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal abstract partial class SchemaProvider : ISchemaProvider
	{
		#region -- 常量 --

		internal static class _
		{
			public static readonly String Tables = "Tables";
			public static readonly String Views = "Views";
			public static readonly String Indexes = "Indexes";
			public static readonly String IndexColumns = "IndexColumns";
			public static readonly String Databases = "Databases";
			public static readonly String Columns = "Columns";
			public static readonly String ID = "ID";
			public static readonly String OrdinalPosition = "ORDINAL_POSITION";
			public static readonly String ColumnPosition = "COLUMN_POSITION";
			public static readonly String TalbeName = "table_name";
			public static readonly String ColumnName = "COLUMN_NAME";
			public static readonly String IndexName = "INDEX_NAME";
			public static readonly String PrimaryKeys = "PrimaryKeys";
		}

		#endregion

		#region -- 属性 --

		/// <summary>数据库</summary>
		public virtual IDatabase Database
		{
			get { return DbInternal; }
		}

		private DbBase _DbInternal;

		/// <summary>数据库</summary>
		internal virtual DbBase DbInternal
		{
			get { return _DbInternal; }
			set { _DbInternal = value; }
		}

		private String ConnName
		{
			get { return DbInternal.ConnName; }
		}

		/// <summary>拥有者</summary>
		public String Owner { get { return DbInternal.Owner; } }

		internal GeneratorBase Generator
		{
			get { return DbInternal.Generator; }
		}

		/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
		public IQuoter Quoter
		{
			get { return DbInternal.Quoter; }
		}

		private ICollection<String> _MetaDataCollections;

		/// <summary>所有元数据集合</summary>
		public ICollection<String> MetaDataCollections
		{
			get
			{
				if (_MetaDataCollections == null)
				{
					var set = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
					var dt = GetSchema(DbMetaDataCollectionNames.MetaDataCollections, null);
					if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							var name = "" + dr[0];
							if (!name.IsNullOrWhiteSpace()) { set.Add(name); }
						}
					}
					_MetaDataCollections = set;
				}
				return _MetaDataCollections;
			}
		}

		private ICollection<String> _ReservedWords;

		/// <summary>保留关键字</summary>
		public virtual ICollection<String> ReservedWords
		{
			get
			{
				if (_ReservedWords == null)
				{
					var set = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
					if (MetaDataCollections.Contains(DbMetaDataCollectionNames.ReservedWords))
					{
						var dt = GetSchema(DbMetaDataCollectionNames.ReservedWords, null);
						if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
						{
							foreach (DataRow dr in dt.Rows)
							{
								var name = "" + dr[0];
								if (!name.IsNullOrWhiteSpace()) { set.Add(name); }
							}
						}
					}
					_ReservedWords = set;
				}
				return _ReservedWords;
			}
		}

		#endregion

		#region -- 架构检查 --

		/// <summary>返回数据源的架构信息</summary>
		/// <param name="collectionName">指定要返回的架构的名称。</param>
		/// <param name="restrictionValues">为请求的架构指定一组限制值。</param>
		/// <returns></returns>
		public DataTable GetSchema(String collectionName, String[] restrictionValues)
		{
			// 如果不是MetaDataCollections，并且MetaDataCollections中没有该集合，则返回空
			if (!collectionName.EqualIgnoreCase(DbMetaDataCollectionNames.MetaDataCollections))
			{
				if (!MetaDataCollections.Contains(collectionName)) { return null; }
			}
			return DbInternal.CreateSession().GetSchema(collectionName, restrictionValues);
		}

		/// <summary>数据库是否存在</summary>
		/// <returns></returns>
		public abstract Boolean DatabaseExist();

		/// <summary>查询指定的 Schema 是否存在</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public virtual Boolean SchemaExists(String schemaName) { return true; }

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public abstract Boolean TableExists(String tableName);

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public abstract Boolean ColumnExists(String tableName, String columnName);

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public abstract Boolean ConstraintExists(String tableName, String constraintName);

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public abstract Boolean IndexExists(String tableName, String indexName);

		/// <summary>根据序列名称检查序列是否存在</summary>
		/// <param name="sequenceName">序列名称</param>
		/// <returns></returns>
		public virtual Boolean SequenceExists(String sequenceName) { return true; }

		//public virtual Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue) { return false; }

		#endregion

		#region -- 正向 --

		#region - 表构架 -

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		public virtual List<IDataTable> GetTables()
		{
			try
			{
				return OnGetTables(null);
			}
			catch (DbException ex)
			{
				throw new OrmLiteDbSchemaException(this, "取得所有表构架出错！", ex);
			}
		}

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		protected virtual List<IDataTable> OnGetTables(ICollection<String> names)
		{
			DataTable dt = GetSchema(_.Tables, null);
			if (dt == null || dt.Rows == null || dt.Rows.Count < 1) { return null; }

			// 默认列出所有表
			DataRow[] rows = OnGetTables(names, dt.Rows);
			if (rows == null || rows.Length < 1) { return null; }

			return GetTables(rows);
		}

		/// <summary>取得表构架</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public virtual IDataTable GetTable(String tableName)
		{
			return null;
		}

		internal DataRow[] OnGetTables(ICollection<String> names, IEnumerable rows)
		{
			if (rows == null) { return null; }

			var list = new List<DataRow>();
			foreach (DataRow dr in rows)
			{
				if (names == null || names.Count < 1)
				{
					list.Add(dr);
				}
				else
				{
					var name = "";
					if (TryGetDataRowValue<String>(dr, _.TalbeName, out name) && names.Contains(name))
					{
						list.Add(dr);
					}
				}
			}
			if (list.Count < 1) { return null; }
			return list.ToArray();
		}

		/// <summary>根据数据行取得数据表</summary>
		/// <param name="rows">数据行</param>
		/// <returns></returns>
		internal List<IDataTable> GetTables(DataRow[] rows)
		{
			//if (_columns == null) _columns = GetSchema(_.Columns, null);
			//if (_indexes == null) _indexes = GetSchema(_.Indexes, null);
			//if (_indexColumns == null) _indexColumns = GetSchema(_.IndexColumns, null);
			if (_columns == null)
			{
				try { _columns = GetSchema(_.Columns, null); }
				catch (Exception ex) { DAL.WriteDebugLog(ex.ToString()); }
			}
			if (_indexes == null)
			{
				try { _indexes = GetSchema(_.Indexes, null); }
				catch (Exception ex) { DAL.WriteDebugLog(ex.ToString()); }
			}
			if (_indexColumns == null)
			{
				try { _indexColumns = GetSchema(_.IndexColumns, null); }
				catch (Exception ex) { DAL.WriteDebugLog(ex.ToString()); }
			}

			try
			{
				List<IDataTable> list = new List<IDataTable>();
				foreach (DataRow dr in rows)
				{
					#region 基本属性

					IDataTable table = DAL.CreateTable();
					table.TableName = GetDataRowValue<String>(dr, _.TalbeName);

					// 顺序、编号
					Int32 id = 0;
					if (TryGetDataRowValue<Int32>(dr, "TABLE_ID", out id))
					{
						table.ID = id;
					}
					else
					{
						table.ID = list.Count + 1;
					}

					// 描述
					table.Description = GetDataRowValue<String>(dr, "DESCRIPTION");

					//// 拥有者
					//table.Owner = GetDataRowValue<String>(dr, "OWNER");

					// 是否视图
					table.IsView = "View".EqualIgnoreCase(GetDataRowValue<String>(dr, "TABLE_TYPE"));

					table.DbType = DbInternal.DbType;

					#endregion

					#region 字段及修正

					// 字段的获取可能有异常，但不应该影响整体架构的获取
					try
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
					catch (Exception ex)
					{
						if (DAL.Debug)
						{
							DAL.WriteLog(ex.ToString());
						}
					}

					FixTable(table, dr);

					list.Add(table);

					#endregion
				}

				#region 表间关系处理

				//// 某字段名，为另一个表的（表名+单主键名）形式时，作为关联字段处理
				//foreach (var table in list)
				//{
				//    foreach (var rtable in list)
				//    {
				//        if (table != rtable) table.Connect(rtable);
				//    }
				//}
				ModelHelper.Connect(list);

				//// 因为可能修改了表间关系，再修正一次
				//foreach (var table in list)
				//{
				//    table.Fix();
				//}

				#endregion

				return list;

				// 不要把这些清空。因为，多线程同时操作的时候，前面的线程有可能把后面线程的数据给清空了
			}
			finally
			{
				_columns = null;
				_indexes = null;
				_indexColumns = null;
			}
		}

		/// <summary>修正表</summary>
		/// <param name="table"></param>
		/// <param name="dr"></param>
		protected virtual void FixTable(IDataTable table, DataRow dr)
		{
		}

		#endregion

		#region - 字段架构 -

		protected DataTable _columns;

		/// <summary>取得指定表的所有列构架</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected virtual List<IDataColumn> GetFields(IDataTable table)
		{
			//DataTable dt = GetSchema(_.Columns, new String[] { null, null, table.Name });
			DataTable dt = _columns;
			if (dt == null) { return null; }
			DataRow[] drs = null;
			String where = String.Format("{0}='{1}'", _.TalbeName, table.TableName);
			if (dt.Columns.Contains(_.OrdinalPosition))
			{
				drs = dt.Select(where, _.OrdinalPosition);
			}
			else if (dt.Columns.Contains(_.ID))
			{
				drs = dt.Select(where, _.ID);
			}
			else
			{
				drs = dt.Select(where);
			}
			return GetFields(table, drs);
		}

		/// <summary>获取指定表的字段</summary>
		/// <param name="table"></param>
		/// <param name="rows"></param>
		/// <returns></returns>
		protected virtual List<IDataColumn> GetFields(IDataTable table, DataRow[] rows)
		{
			var list = new List<IDataColumn>();

			// 开始序号
			Int32 startIndex = 0;

			foreach (var dr in rows)
			{
				var field = table.CreateColumn();

				// 序号
				Int32 n = 0;
				if (TryGetDataRowValue<Int32>(dr, _.OrdinalPosition, out n))
				{
					field.ID = n;
				}
				else if (TryGetDataRowValue<Int32>(dr, _.ID, out n))
				{
					field.ID = n;
				}

				// 如果从0开始，则所有需要同步增加；如果所有字段序号都是0，则按照先后顺序
				if (field.ID == 0)
				{
					startIndex++;

					//field.ID = startIndex;
				}
				if (startIndex > 0) { field.ID += startIndex; }

				// 名称
				field.ColumnName = GetDataRowValue<String>(dr, _.ColumnName);

				// 标识、主键
				Boolean b;
				if (TryGetDataRowValue<Boolean>(dr, "AUTOINCREMENT", out  b))
				{
					field.Identity = b;
				}
				if (TryGetDataRowValue<Boolean>(dr, "PRIMARY_KEY", out  b))
				{
					field.PrimaryKey = b;
				}

				// 原始数据类型
				String str;
				if (TryGetDataRowValue<String>(dr, "DATA_TYPE", out str))
				{
					field.RawType = str;
				}
				else if (TryGetDataRowValue<String>(dr, "DATATYPE", out str))
				{
					field.RawType = str;
				}
				else if (TryGetDataRowValue<String>(dr, "COLUMN_DATA_TYPE", out str))
				{
					field.RawType = str;
				}

				// 是否Unicode
				#region ## 苦竹 修改 ##
				//if (Database is DbBase)
				//{
				//	field.IsUnicode = (Database as DbBase).IsUnicode(field.RawType);
				//}
				field.IsUnicode = Quoter.IsUnicode(field.RawType);
				#endregion

				// 精度
				if (TryGetDataRowValue<Int32>(dr, "NUMERIC_PRECISION", out n))
				{
					field.Precision = n;
				}
				else if (TryGetDataRowValue<Int32>(dr, "DATETIME_PRECISION", out n))
				{
					field.Precision = n;
				}
				else if (TryGetDataRowValue<Int32>(dr, "PRECISION", out n))
				{
					field.Precision = n;
				}

				// 位数
				if (TryGetDataRowValue<Int32>(dr, "NUMERIC_SCALE", out n))
				{
					field.Scale = n;
				}
				else if (TryGetDataRowValue<Int32>(dr, "SCALE", out n))
				{
					field.Scale = n;
				}

				// 长度
				if (TryGetDataRowValue<Int32>(dr, "CHARACTER_MAXIMUM_LENGTH", out n))
				{
					field.Length = n;
				}
				else if (TryGetDataRowValue<Int32>(dr, "LENGTH", out n))
				{
					field.Length = n;
				}
				else if (TryGetDataRowValue<Int32>(dr, "COLUMN_SIZE", out n))
				{
					field.Length = n;
				}
				else
				{
					field.Length = field.Precision;
				}

				//// 字节数
				//if (TryGetDataRowValue<Int32>(dr, "CHARACTER_OCTET_LENGTH", out n))
				//{
				//	field.NumOfByte = n;
				//}
				//else
				//{
				//	field.NumOfByte = field.Length;
				//}

				// 允许空
				if (TryGetDataRowValue<Boolean>(dr, "IS_NULLABLE", out  b))
				{
					field.Nullable = b;
				}
				else if (TryGetDataRowValue<String>(dr, "IS_NULLABLE", out  str))
				{
					if (!str.IsNullOrWhiteSpace())
					{
						field.Nullable = "YES".EqualIgnoreCase(str);
					}
				}
				else if (TryGetDataRowValue<String>(dr, "NULLABLE", out  str))
				{
					if (!str.IsNullOrWhiteSpace())
					{
						field.Nullable = "Y".EqualIgnoreCase(str);
					}
				}

				// 默认值
				field.Default = GetDataRowValue<String>(dr, "COLUMN_DEFAULT");

				// 描述
				field.Description = GetDataRowValue<String>(dr, "DESCRIPTION");
				FixField(field, dr);

				// 检查是否已正确识别类型
				if (field.DataType == null)
				{
					DAL.Logger.LogWarning("无法识别{0}.{1}的类型{2}！", table.TableName, field.ColumnName, field.RawType);
				}

				field.Fix();

				list.Add(field);
			}
			return list;
		}

		/// <summary>修正指定字段</summary>
		/// <param name="field">字段</param>
		/// <param name="dr"></param>
		protected virtual void FixField(IDataColumn field, DataRow dr)
		{
			DataTable dt = DataTypes;
			if (dt == null) { return; }
			DataRow[] drs = FindDataType(field, field.RawType, null);
			if (drs == null || drs.Length < 1)
			{
				FixField(field, dr, null);
			}
			else
			{
				FixField(field, dr, drs[0]);
			}
		}

		/// <summary>修正指定字段</summary>
		/// <param name="field">字段</param>
		/// <param name="drColumn">字段元数据</param>
		/// <param name="drDataType">字段匹配的数据类型</param>
		protected virtual void FixField(IDataColumn field, DataRow drColumn, DataRow drDataType)
		{
			String typeName = field.RawType;

			// 修正数据类型 +++重点+++
			if (TryGetDataRowValue<String>(drDataType, "DataType", out typeName))
			{
				field.DataType = typeName.GetTypeEx();
			}

			// 修正长度为最大长度
			if (field.Length == 0)
			{
				Int32 n = 0;
				if (TryGetDataRowValue<Int32>(drDataType, "ColumnSize", out n))
				{
					field.Length = n;
					//if (field.NumOfByte == 0) field.NumOfByte = field.Length;
				}
				if (field.Length <= 0 && field.DataType == typeof(String))
				{
					field.Length = Int32.MaxValue / 2;
				}
			}

			// 处理格式参数
			if (!field.RawType.IsNullOrWhiteSpace() && !field.RawType.EndsWith(")"))
			{
				String param = GetFormatParam(field, drDataType);
				if (!param.IsNullOrWhiteSpace())
				{
					field.RawType += param;
				}
			}
		}

		#endregion

		#region - 索引架构 -

		protected DataTable _indexes;
		protected DataTable _indexColumns;

		/// <summary>获取索引</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected virtual List<IDataIndex> GetIndexes(IDataTable table)
		{
			if (_indexes == null) { return null; }
			DataRow[] drs = _indexes.Select(String.Format("{0}='{1}'", _.TalbeName, table.TableName));
			if (drs == null || drs.Length < 1) { return null; }
			List<IDataIndex> list = new List<IDataIndex>();

			foreach (DataRow dr in drs)
			{
				String name = null;
				if (!TryGetDataRowValue<String>(dr, _.IndexName, out name)) { continue; }
				IDataIndex di = table.CreateIndex();
				di.Name = name;
				if (TryGetDataRowValue<string>(dr, _.ColumnName, out name) && !name.IsNullOrWhiteSpace())
				{
					di.Columns = name.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				}
				else if (_indexColumns != null)
				{
					String orderby = null;

					// Oracle数据库用ColumnPosition，其它数据库用OrdinalPosition
					if (_indexColumns.Columns.Contains(_.OrdinalPosition))
					{
						orderby = _.OrdinalPosition;
					}
					else if (_indexColumns.Columns.Contains(_.ColumnPosition))
					{
						orderby = _.ColumnPosition;
					}
					DataRow[] dics = _indexColumns.Select(String.Format("{0}='{1}' And {2}='{3}'", _.TalbeName, table.TableName, _.IndexName, di.Name), orderby);
					if (dics != null && dics.Length > 0)
					{
						List<String> ns = new List<String>();

						foreach (DataRow item in dics)
						{
							String dcname = null;
							if (TryGetDataRowValue<String>(item, _.ColumnName, out dcname) &&
									!dcname.IsNullOrWhiteSpace() && !ns.Contains(dcname))
							{
								ns.Add(dcname);
							}
						}
						if (ns.Count < 1)
						{
							DAL.WriteDebugLog("表{0}的索引{1}无法取得字段列表！", table, di.Name);
						}
						di.Columns = ns.ToArray();
					}
				}
				Boolean b = false;
				if (TryGetDataRowValue<Boolean>(dr, "UNIQUE", out b))
				{
					di.Unique = b;
				}
				if (TryGetDataRowValue<Boolean>(dr, "PRIMARY", out b))
				{
					di.PrimaryKey = b;
				}
				else if (TryGetDataRowValue<Boolean>(dr, "PRIMARY_KEY", out b))
				{
					di.PrimaryKey = b;
				}
				FixIndex(di, dr);
				list.Add(di);
			}
			return list != null && list.Count > 0 ? list : null;
		}

		/// <summary>修正索引</summary>
		/// <param name="index"></param>
		/// <param name="dr"></param>
		protected virtual void FixIndex(IDataIndex index, DataRow dr)
		{
		}

		#endregion

		#region - 数据类型  -

		private DataTable _DataTypes;

		/// <summary>数据类型</summary>
		public DataTable DataTypes
		{
			get { return _DataTypes ?? (_DataTypes = GetSchema(DbMetaDataCollectionNames.DataTypes, null)); }
			protected internal set { _DataTypes = value; }
		}

		protected List<KeyValuePair<Type, Type>> _FieldTypeMaps;

		/// <summary>字段类型映射</summary>
		protected virtual List<KeyValuePair<Type, Type>> FieldTypeMaps
		{
			get
			{
				if (_FieldTypeMaps == null)
				{
					// 把不常用的类型映射到常用类型，比如数据库SByte映射到实体类Byte，UInt32映射到Int32，而不需要重新修改数据库
					var list = new List<KeyValuePair<Type, Type>>();
					list.Add(new KeyValuePair<Type, Type>(typeof(SByte), typeof(Byte)));
					//list.Add(new KeyValuePair<Type, Type>(typeof(SByte), typeof(Int16)));
					// 因为等价，字节需要能够互相映射
					list.Add(new KeyValuePair<Type, Type>(typeof(Byte), typeof(SByte)));

					list.Add(new KeyValuePair<Type, Type>(typeof(UInt16), typeof(Int16)));
					list.Add(new KeyValuePair<Type, Type>(typeof(Int16), typeof(UInt16)));
					//list.Add(new KeyValuePair<Type, Type>(typeof(UInt16), typeof(Int32)));
					//list.Add(new KeyValuePair<Type, Type>(typeof(Int16), typeof(Int32)));

					list.Add(new KeyValuePair<Type, Type>(typeof(UInt32), typeof(Int32)));
					list.Add(new KeyValuePair<Type, Type>(typeof(Int32), typeof(UInt32)));
					//// 因为自增的原因，某些字段需要被映射到Int32里面来
					//list.Add(new KeyValuePair<Type, Type>(typeof(SByte), typeof(Int32)));

					list.Add(new KeyValuePair<Type, Type>(typeof(UInt64), typeof(Int64)));
					list.Add(new KeyValuePair<Type, Type>(typeof(Int64), typeof(UInt64)));
					//list.Add(new KeyValuePair<Type, Type>(typeof(UInt64), typeof(Int32)));
					//list.Add(new KeyValuePair<Type, Type>(typeof(Int64), typeof(Int32)));

					list.Add(new KeyValuePair<Type, Type>(typeof(Guid), typeof(CombGuid)));
					list.Add(new KeyValuePair<Type, Type>(typeof(CombGuid), typeof(Guid)));

					//// 根据常用行，从不常用到常用排序，然后配对进入映射表
					//var types = new Type[] { typeof(SByte), typeof(Byte), typeof(UInt16), typeof(Int16), typeof(UInt64), typeof(Int64), typeof(UInt32), typeof(Int32) };

					//for (int i = 0; i < types.Length; i++)
					//{
					//    for (int j = i + 1; j < types.Length; j++)
					//    {
					//        list.Add(new KeyValuePair<Type, Type>(types[i], types[j]));
					//    }
					//}
					//// 因为自增的原因，某些字段需要被映射到Int64里面来
					//list.Add(new KeyValuePair<Type, Type>(typeof(UInt32), typeof(Int64)));
					//list.Add(new KeyValuePair<Type, Type>(typeof(Int32), typeof(Int64)));
					list.Add(new KeyValuePair<Type, Type>(typeof(Guid), typeof(String)));

					_FieldTypeMaps = list;
				}
				return _FieldTypeMaps;
			}
		}

		/// <summary>查找指定字段指定类型的数据类型</summary>
		/// <param name="field">字段</param>
		/// <param name="typeName"></param>
		/// <param name="isLong"></param>
		/// <returns></returns>
		protected virtual DataRow[] FindDataType(IDataColumn field, String typeName, Boolean? isLong)
		{
			DataRow[] drs = null;

			try
			{
				drs = OnFindDataType(field, typeName, isLong);
			}
			catch { }
			if (drs != null && drs.Length > 0) { return drs; }

			// 把Guid映射到varchar(32)去
			if (typeName == typeof(Guid).FullName || typeName.EqualIgnoreCase("Guid"))
			{
				typeName = "varchar(32)";

				try
				{
					drs = OnFindDataType(field, typeName, isLong);
				}
				catch { }
				if (drs != null && drs.Length > 0) { return drs; }
			}

			// 如果该类型无法识别，则去尝试使用最接近的高阶类型
			foreach (var item in FieldTypeMaps)
			{
				if (item.Key.FullName == typeName)
				{
					try
					{
						drs = OnFindDataType(field, item.Value.FullName, isLong);
					}
					catch { }
					if (drs != null && drs.Length > 0) { return drs; }
				}
			}
			return null;
		}

		private DataRow[] OnFindDataType(IDataColumn field, String typeName, Boolean? isLong)
		{
			ValidationHelper.ArgumentNullOrEmpty(typeName, "typeName");

			// 去掉类型中，长度等限制条件
			if (typeName.Contains("("))
			{
				typeName = typeName.Substring(0, typeName.IndexOf("("));
			}
			var dt = DataTypes;
			if (dt == null) { return null; }
			DataRow[] drs = null;
			var sb = new StringBuilder();

			// 匹配TypeName，TypeName具有唯一性
			sb.AppendFormat("TypeName='{0}'", typeName);

			drs = dt.Select(sb.ToString());
			if (drs != null && drs.Length > 0)
			{
				// 找到太多，试试过滤自增等
				if (drs.Length > 1 && field.Identity && dt.Columns.Contains("IsAutoIncrementable"))
				{
					var dr = drs.FirstOrDefault(e => (Boolean)e["IsAutoIncrementable"]);
					if (dr != null) { return new DataRow[] { dr }; }
				}

				return drs;
			}

			// 匹配DataType，重复的可能性很大
			sb = new StringBuilder();
			sb.AppendFormat("DataType='{0}'", typeName);

			drs = dt.Select(sb.ToString());
			if (drs != null && drs.Length > 0)
			{
				if (drs.Length == 1) return drs;
				// 找到太多，试试过滤自增等
				if (drs.Length > 1 && field.Identity && dt.Columns.Contains("IsAutoIncrementable"))
				{
					var drs1 = drs.Where(e => (Boolean)e["IsAutoIncrementable"]).ToArray();
					if (drs1 != null)
					{
						if (drs1.Length == 1) { return drs1; }
						drs = drs1;
					}
				}

				sb.AppendFormat(" And ColumnSize>={0}", field.Length);
				//if (field.DataType == typeof(String) && field.Length > Database.LongTextLength) sb.AppendFormat(" And IsLong=1");
				// 如果字段的长度为0，则也算是大文本
				if (field.DataType == typeof(String) && (field.Length > DbInternal.LongTextLength || field.Length <= 0))
				{
					sb.AppendFormat(" And IsLong=1");
				}

				var drs2 = dt.Select(sb.ToString(), "IsBestMatch Desc, ColumnSize Asc, IsFixedLength Asc, IsLong Asc");
				if (drs2 == null || drs2.Length < 1) { return drs; }
				if (drs2.Length == 1) { return drs2; }

				return drs2;
			}
			return null;
		}

		///// <summary>检查是否自增，如果自增，则附加过滤条件</summary>
		///// <param name="dt"></param>
		///// <param name="field"></param>
		///// <param name="where"></param>
		///// <returns></returns>
		//protected virtual Boolean CheckAutoIncrementable(DataTable dt, IDataColumn field,StringBuilder where)
		//{
		//    if (field.Identity && dt.Columns.Contains("IsAutoIncrementable")) sb.Append(" And IsAutoIncrementable=1");
		//}

		/// <summary>取字段类型</summary>
		/// <param name="field">字段</param>
		/// <returns></returns>
		protected virtual String GetFieldType(IDataColumn field)
		{
			/*
			 * 首先尝试原始数据类型，因为即使是不同的数据库，相近的类型也可能采用相同的名称；
			 * 然后才使用.Net类型名去匹配；
			 * 两种方法都要注意处理类型参数，比如长度、精度、小数位数等
			 */
			String typeName = field.RawType;
			DataRow[] drs = null;
			if (!typeName.IsNullOrWhiteSpace())
			{
				if (typeName.Contains("("))
				{
					typeName = typeName.Substring(0, typeName.IndexOf("("));
				}
				drs = FindDataType(field, typeName, null);
				if (drs != null && drs.Length > 0)
				{
					if (TryGetDataRowValue<String>(drs[0], "TypeName", out typeName))
					{
						// 处理格式参数
						String param = GetFormatParam(field, drs[0]);
						if (!param.IsNullOrWhiteSpace() && param != "()")
						{
							typeName += param;
						}
						return typeName;
					}
				}
			}
			typeName = field.DataType.FullName;
			drs = FindDataType(field, typeName, null);
			if (drs != null && drs.Length > 0)
			{
				if (TryGetDataRowValue<String>(drs[0], "TypeName", out typeName))
				{
					// 处理格式参数
					String param = GetFormatParam(field, drs[0]);
					if (!param.IsNullOrWhiteSpace() && param != "()")
					{
						typeName += param;
					}
					return typeName;
				}
			}
			return null;
		}

		/// <summary>取得格式化的类型参数</summary>
		/// <param name="field">字段</param>
		/// <param name="dr"></param>
		/// <returns></returns>
		protected virtual String GetFormatParam(IDataColumn field, DataRow dr)
		{
			// 为了最大程度保证兼容性，所有数据库的Decimal和DateTime类型不指定精度，均采用数据库默认值
			if (field.DataType == typeof(Decimal)) { return null; }
			if (field.DataType == typeof(DateTime)) { return null; }

			String ps = null;
			if (!TryGetDataRowValue<String>(dr, "CreateParameters", out ps) || ps.IsNullOrWhiteSpace()) { return null; }
			var sb = new StringBuilder();
			sb.Append("(");
			var pms = ps.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < pms.Length; i++)
			{
				if (sb.Length > 1) { sb.Append(","); }
				sb.Append(GetFormatParamItem(field, dr, pms[i]));
			}
			sb.Append(")");
			return sb.ToString();
		}

		/// <summary>获取格式化参数项</summary>
		/// <param name="field">字段</param>
		/// <param name="dr"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual String GetFormatParamItem(IDataColumn field, DataRow dr, String item)
		{
			if (item.Contains("length") || item.Contains("size"))
			{
				return field.Length.ToString();
			}
			if (item.Contains("precision"))
			{
				return field.Precision.ToString();
			}
			if (item.Contains("scale") || item.Contains("bits"))
			{
				// 如果没有设置位数，则使用最大位数
				Int32 d = field.Scale;

				//if (d < 0)
				//{
				//    if (!TryGetDataRowValue<Int32>(dr, "MaximumScale", out d)) d = field.Scale;
				//}
				return d.ToString();
			}
			return Helper.IntegerZero;
		}

		#endregion

		#endregion

		#region -- 反向 --

		#region - DataBase -

		#region - Check Database -

		private Int32 _CheckedDatabase = 0;

		/// <summary>检查数据库是否存在，如果不存在则建立数据库</summary>
		/// <param name="setting"></param>
		private void CheckDatabase(NegativeSetting setting)
		{
			if (Interlocked.CompareExchange(ref _CheckedDatabase, 1, 0) != 0) { return; }

			//数据库检查
			var dbExist = DatabaseExist();
			if (!dbExist)
			{
				if (!setting.CheckOnly)
				{
					DAL.Logger.LogInformation("创建数据库：{0}", ConnName);
					CreateDatabase(null, null);
				}
				else
				{
					var sql = Generator.CreateDatabaseSQL(null, null);
					if (sql.IsNullOrWhiteSpace())
					{
						DAL.Logger.LogWarning("请为连接{0}创建数据库！", ConnName);
					}
					else
					{
						DAL.Logger.LogWarning("请为连接{0}创建数据库，使用以下语句：{1}", ConnName, Environment.NewLine + sql);
					}
				}
			}
		}

		#endregion

		/// <summary>创建数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <param name="databasePath">数据库路径</param>
		public abstract void CreateDatabase(String databaseName, String databasePath);

		/// <summary>删除数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <returns></returns>
		public abstract void DropDatabase(String databaseName);

		#endregion

		#region - Schema -

		public virtual void CreateSchema(String schemaName)
		{
			Execute(Generator.CreateSchemaSQL(schemaName));
		}

		public virtual void DeleteSchema(String schemaName)
		{
			Execute(Generator.DeleteSchemaSQL(schemaName));
		}

		public virtual void AlterSchema(String srcSchemaName, String tableName, String destSchemaName)
		{
			Execute(Generator.AlterSchemaSQL(srcSchemaName, tableName, destSchemaName));
		}

		#endregion

		#region - Table -

		/// <summary>设置表模型，检查数据表是否匹配表模型，反向工程</summary>
		/// <param name="setting">设置</param>
		/// <param name="tables"></param>
		public void SetTables(NegativeSetting setting, params IDataTable[] tables)
		{
			OnSetTables(tables, setting);
		}

		/// <summary>设置表模型，检查数据表是否匹配表模型，反向工程</summary>
		/// <param name="tables"></param>
		/// <param name="setting"></param>
		internal virtual void OnSetTables(IDataTable[] tables, NegativeSetting setting)
		{
			CheckDatabase(setting);

			var comparison = new CompareTables(this, Generator, tables);
			var resultsList = comparison.ExecuteResult();

			if (setting.CheckOnly)
			{
				DAL.Logger.LogInformation("只检查不对数据库进行操作, 请手工执行 SQL 语句：" + Environment.NewLine);
				foreach (var results in resultsList)
				{
					var rebulid = results.Find(e => e.ResultType == ResultType.RebulidTable);
					// 如果存在重建表操作
					if (rebulid != null)
					{
						DAL.Logger.LogInformation(rebulid.ToString());
					}
					else
					{
						foreach (var item in results)
						{
							if (!item.Script.IsNullOrWhiteSpace())
							{
								DAL.Logger.LogInformation(item.ToString());
							}
						}
					}
				}
			}
			else
			{
				foreach (var results in resultsList)
				{
					var rebulid = results.Find(e => e.ResultType == ResultType.RebulidTable);
					// 如果存在重建表操作
					if (rebulid != null)
					{
						DAL.Logger.LogInformation("--{0}{1}：{2}", rebulid.ResultType.GetDescription(), rebulid.SchemaObjectType.GetDescription(), rebulid.Remark);
						try
						{
							Execute(rebulid.Script);
						}
						catch (Exception ex)
						{
							DAL.WriteLog(ex, "{0}{1}：{2} 失败！", rebulid.ResultType.GetDescription(), rebulid.SchemaObjectType.GetDescription(), rebulid.Remark);
						}
					}
					else
					{
						foreach (var item in results)
						{
							if (setting.NoDelete && item.ResultType == ResultType.Delete && item.SchemaObjectType == SchemaObjectType.Column)
							{
								//不许删除列，显示日志
								DAL.Logger.LogInformation("数据表中发现有多余字段，请手工执行以下语句删除：" + Environment.NewLine + item.Script);
							}
							else
							{
								if (!item.Script.IsNullOrWhiteSpace())
								{
									DAL.Logger.LogInformation("--{0}{1}：{2}", item.ResultType.GetDescription(), item.SchemaObjectType.GetDescription(), item.Remark);
									try
									{
										Execute(item.Script);
									}
									catch (Exception ex)
									{
										DAL.WriteLog(ex, "{0}{1}：{2} 失败！", item.ResultType.GetDescription(), item.SchemaObjectType.GetDescription(), item.Remark);
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>创建表</summary>
		/// <param name="table"></param>
		public virtual void CreateTable(IDataTable table)
		{
			Execute(Generator.CreateTableSQL(Owner, table));
			Execute(Generator.CreateIndexSQL(Owner, table));
		}

		/// <summary>删除表</summary>
		/// <param name="tableName"></param>
		public virtual void DropTable(String tableName)
		{
			Execute(Generator.DropTableSQL(Owner, tableName));
		}

		/// <summary>重命名表</summary>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		public virtual void RenameTable(String oldName, String newName)
		{
			Execute(Generator.RenameTableSQL(Owner, oldName, newName));
		}

		/// <summary>修改表注释</summary>
		/// <param name="table"></param>
		public virtual void AlterTable(IDataTable table)
		{
			Execute(Generator.AlterTableSQL(Owner, table));
		}

		internal String ReBuildTable(IDataTable entityTable, IDataTable dbTable)
		{
			// 通过重建表的方式修改字段
			String tableName = dbTable.TableName;
			String tempTableName = "Temp_" + tableName + "_" + new Random((Int32)DateTime.Now.Ticks).Next(1000, 10000);

			// 每个分号后面故意加上空格，是为了让DbMetaData执行SQL时，不要按照分号加换行来拆分这个SQL语句
			var sb = new StringBuilder();
			sb.AppendLine("BEGIN TRANSACTION; ");
			// 清空索引
			sb.Append(Generator.DeleteIndexSQL(Owner, dbTable));
			sb.AppendLine("; ");
			sb.Append(Generator.RenameTableSQL(Owner, tableName, tempTableName));
			sb.AppendLine("; ");
			sb.Append(Generator.CreateTableSQL(Owner, entityTable));
			sb.AppendLine("; ");

			var prefix = SetIdentityInsertOn(entityTable, dbTable);
			if (!prefix.IsNullOrWhiteSpace()) { sb.AppendLine(prefix); }

			// 如果指定了新列和旧列，则构建两个集合
			if (entityTable.Columns != null && entityTable.Columns.Count > 0 && dbTable.Columns != null && dbTable.Columns.Count > 0)
			{
				var sbName = new StringBuilder();
				var sbValue = new StringBuilder();

				foreach (var item in entityTable.Columns)
				{
					String name = item.ColumnName;
					var field = dbTable.GetColumn(item.ColumnName);
					if (field == null)
					{
						// 如果新增列为标识列，不做处理
						if (!item.Identity)
						{
							// 如果新增了不允许空的列，则处理一下默认值
							if (!item.Nullable)
							{
								if (sbName.Length > 0) { sbName.Append(", "); }
								if (sbValue.Length > 0) { sbValue.Append(", "); }
								sbName.Append(Quoter.QuoteColumnName(name));
								sbValue.Append(Quoter.QuoteValue(item, Helper.GetCommonDbTypeDefaultValue(item.DbType)));
							}
						}
					}
					else
					{
						if (sbName.Length > 0) { sbName.Append(", "); }
						if (sbValue.Length > 0) { sbValue.Append(", "); }
						sbName.Append(Quoter.QuoteColumnName(name));

						//sbValue.Append(FormatName(name));
						// 处理字符串不允许空，ntext不支持+""
						if (item.DataType == typeof(String) && !item.Nullable && item.Length > 0 && item.Length < 500)
						{
							sbValue.Append(DbInternal.StringConcat(Quoter.QuoteColumnName(name), "\'\'"));
						}
						else
						{
							sbValue.Append(Quoter.QuoteColumnName(name));
						}
					}
				}
				sb.AppendFormat("Insert Into {0}({2}) Select {3} From {1}", tableName, tempTableName, sbName.ToString(), sbValue.ToString());
			}
			else
			{
				sb.AppendFormat("Insert Into {0} Select * From {1}", tableName, tempTableName);
			}
			sb.AppendLine("; ");

			var suffix = SetIdentityInsertOff(entityTable, dbTable);
			if (!suffix.IsNullOrWhiteSpace()) { sb.AppendLine(suffix); }

			// 重建索引
			sb.Append(Generator.CreateIndexSQL(Owner, entityTable));
			sb.AppendLine("; ");

			sb.AppendFormat("Drop Table {0}", tempTableName);
			sb.AppendLine("; ");

			sb.Append("COMMIT;");

			sb.Replace(";" + Environment.NewLine, "; " + Environment.NewLine);

			return sb.ToString();
		}

		internal virtual String SetIdentityInsertOn(IDataTable entityTable, IDataTable dbTable)
		{
			return String.Empty;
		}

		internal virtual String SetIdentityInsertOff(IDataTable entityTable, IDataTable dbTable)
		{
			return String.Empty;
		}

		#endregion

		#region - Column -

		/// <summary>修改字段</summary>
		/// <param name="column"></param>
		public virtual void AlterColumn(IDataColumn column)
		{
			Execute(Generator.AlterColumnSQL(Owner, column));
		}

		/// <summary>创建字段</summary>
		/// <param name="column"></param>
		public virtual void CreateColumn(IDataColumn column)
		{
			Execute(Generator.CreateColumnSQL(Owner, column));
		}

		/// <summary>删除字段</summary>
		/// <param name="tableName"></param>
		/// <param name="columnName"></param>
		public virtual void DropColumn(String tableName, String columnName)
		{
			Execute(Generator.DropColumnSQL(Owner, tableName, columnName));
		}

		/// <summary>重命名字段</summary>
		/// <param name="tableName"></param>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		public virtual void RenameColumn(String tableName, String oldName, String newName)
		{
			Execute(Generator.RenameColumnSQL(Owner, tableName, oldName, newName));
		}

		#endregion

		#region - Index -

		/// <summary>创建索引</summary>
		/// <param name="dataIndex"></param>
		public virtual void CreateIndex(IDataIndex dataIndex)
		{
			var index = IndexDefinition.Create(Owner, dataIndex);
			Execute(Generator.CreateIndexSQL(index));
		}

		/// <summary>删除索引</summary>
		/// <param name="tableName"></param>
		/// <param name="indexName"></param>
		public virtual void DeleteIndex(String tableName, String indexName)
		{
			Execute(Generator.DeleteIndexSQL(Owner, tableName, indexName));
		}

		#endregion

		#region - Sequence -

		public virtual void CreateSequence(SequenceDefinition sequence) { }

		public virtual void DeleteSequence(String schemaName, String sequenceName) { }

		#endregion

		#region - Constraint -

		public virtual void AlterDefaultConstraint(String schemaName, String tableName, String columnName, Object defaultValue)
		{
		}

		public virtual void DeleteDefaultConstraint(String schemaName, String tableName, String columnName)
		{
		}

		public virtual void CreateConstraint(ConstraintDefinition constraint)
		{
			Execute(Generator.CreateConstraintSQL(constraint));
		}

		public virtual void DeleteConstraint(String tableName, String constraintName, Boolean isPrimaryKey)
		{
			Execute(Generator.DeleteConstraintSQL(Owner, tableName, constraintName, isPrimaryKey));
		}

		#endregion

		/// <summary>执行SQL语句，返回受影响的行数</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="session">数据库会话</param>
		/// <returns></returns>
		internal Int32 Execute(String sql, IDbSession session = null)
		{
			if (sql.IsNullOrWhiteSpace()) { return 0; }

			if (session == null) { session = DbInternal.CreateSession(); }

			// 分隔符是分号加换行，如果不想被拆开执行（比如有事务），可以在分号和换行之间加一个空格
			var ss = sql.Split(";" + Environment.NewLine);
			if (ss == null || ss.Length < 1) { return session.Execute(sql); }

			foreach (var item in ss)
			{
				session.Execute(item);
			}
			return 0;
		}

		#endregion

		#region -- 辅助函数 --

		/// <summary>尝试从指定数据行中读取指定名称列的数据</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dr"></param>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		internal static Boolean TryGetDataRowValue<T>(DataRow dr, String name, out T value)
		{
			value = default(T);
			if (dr == null || !dr.Table.Columns.Contains(name) || dr.IsNull(name))
			{
				return false;
			}
			Object obj = dr[name];

			// 特殊处理布尔类型
			if (Type.GetTypeCode(typeof(T)) == TypeCode.Boolean && obj != null)
			{
				if (obj is Boolean)
				{
					value = (T)obj;
					return true;
				}
				if ("YES".EqualIgnoreCase(obj.ToString()))
				{
					value = (T)(Object)true;
					return true;
				}
				if ("NO".EqualIgnoreCase(obj.ToString()))
				{
					value = (T)(Object)false;
					return true;
				}
			}

			try
			{
				if (obj is T)
				{
					value = (T)obj;
				}
				else
				{
					value = (T)Convert.ChangeType(obj, typeof(T));
				}
			}
			catch { return false; }
			return true;
		}

		/// <summary>获取指定数据行指定字段的值，不存在时返回空</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dr"></param>
		/// <param name="name">名称</param>
		/// <returns></returns>
		internal static T GetDataRowValue<T>(DataRow dr, String name)
		{
			T value = default(T);
			if (TryGetDataRowValue<T>(dr, name, out value)) { return value; }
			return default(T);
		}

		/// <summary>检查并获取当前数据库的默认值。如果数据库类型一致，则直接返回false，因为没有修改</summary>
		/// <param name="dc"></param>
		/// <param name="oriDefault"></param>
		/// <returns></returns>
		protected virtual Boolean CheckAndGetDefault(IDataColumn dc, ref String oriDefault)
		{
			// 如果数据库类型等于原始类型，则直接通过
			if (dc.Table.DbType == DbInternal.DbType) { return false; }

			// 原始数据库类型
			var db = DbFactory.Create(dc.Table.DbType);
			if (db == null) { return false; }

			var tc = Type.GetTypeCode(dc.DataType);

			// 特殊处理时间
			if (tc == TypeCode.DateTime)
			{
				if (oriDefault.IsNullOrWhiteSpace() || oriDefault.EqualIgnoreCase(db.DateTimeNow))
				{
					oriDefault = DbInternal.DateTimeNow;
					return true;
				}
				else
				{
					//// 出现了不支持的时间默认值
					//if (DAL.Debug) DAL.WriteLog("出现了{0}不支持的时间默认值：{1}.{2}={3}", Database.DbType, dc.Table.Name, dc.Name, oriDefault);

					//oriDefault = null;
					//return true;

					return false;
				}
			}

			// 特殊处理Guid
			else if (tc == TypeCode.String || dc.DataType == typeof(Guid))
			{
				// 如果字段类型是Guid，不需要设置默认值，则也说明是Guid字段
				if (oriDefault.IsNullOrWhiteSpace() || oriDefault.EqualIgnoreCase(db.NewGuid) ||
					 db.NewGuid.IsNullOrWhiteSpace() && dc.DataType == typeof(Guid))
				{
					oriDefault = DbInternal.NewGuid;
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
