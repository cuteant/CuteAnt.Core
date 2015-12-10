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
	internal partial class SqlServerSchemaProvider : RemoteDbSchemaProvider
	{
		#region -- 属性 --

		internal SqlServerVersionType VersionType
		{
			get { return (DbInternal as SqlServer).VersionType; }
		}

		#endregion

		#region -- 架构检查 --

		/// <summary>已重载，数据库是否存在</summary>
		/// <returns></returns>
		public override Boolean DatabaseExist()
		{
			try
			{
				var session = DbInternal.CreateSession();
				var databaseName = session.DatabaseName;
				if (VersionType > SqlServerVersionType.SQLServer2000)
				{
					return session.Exists("SELECT * FROM [master].[sys].[databases] WHERE [name] = N'{0}'".FormatWith(databaseName));
				}
				else
				{
					return session.Exists("SELECT * FROM [master].[sysdatabases] WHERE [name] = N'{0}'".FormatWith(databaseName));
				}
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

		/// <summary>查询指定的 Schema 是否存在</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public override Boolean SchemaExists(String schemaName)
		{
			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				var session = DbInternal.CreateSession();
				return session.Exists("SELECT * FROM sys.schemas WHERE NAME = '{0}'".FormatWith(SafeSchemaName(schemaName)));
			}
			else
			{
				return true;
			}
		}

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			var session = DbInternal.CreateSession();
			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}'".FormatWith(
						SafeSchemaName(Owner), Helper.FormatSqlEscape(tableName)));
			}
			else
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'".FormatWith(Helper.FormatSqlEscape(tableName)));
			}
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			var session = DbInternal.CreateSession();
			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'".FormatWith(
						SafeSchemaName(Owner), Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(columnName)));
			}
			else
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'".FormatWith(
						Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(columnName)));
			}
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName)
		{
			var session = DbInternal.CreateSession();
			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND CONSTRAINT_NAME = '{2}'".FormatWith(
						SafeSchemaName(Owner), Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(constraintName)));
			}
			else
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'".FormatWith(
						Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(constraintName)));
			}
		}

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			var session = DbInternal.CreateSession();
			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				return session.Exists("SELECT * FROM sys.indexes WHERE name = '{0}' and object_id=OBJECT_ID('{1}.{2}')".FormatWith(
						Helper.FormatSqlEscape(indexName), SafeSchemaName(Owner), Helper.FormatSqlEscape(tableName)));
			}
			else
			{
				return session.Exists("SELECT NULL FROM sysindexes WHERE name = '{0}'".FormatWith(Helper.FormatSqlEscape(indexName)));
			}
		}

		/// <summary>根据序列名称检查序列是否存在</summary>
		/// <param name="sequenceName">序列名称</param>
		/// <returns></returns>
		public override Boolean SequenceExists(String sequenceName)
		{
			var session = DbInternal.CreateSession();
			if (VersionType >= SqlServerVersionType.SQLServer2012)
			{
				return session.Exists("SELECT * FROM INFORMATION_SCHEMA.SEQUENCES WHERE SEQUENCE_SCHEMA = '{0}' AND SEQUENCE_NAME = '{1}'".FormatWith(
						SafeSchemaName(Owner), Helper.FormatSqlEscape(sequenceName)));
			}
			else
			{
				return true;
			}
		}

		//public override Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue)
		//{
		//	string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
		//	return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}' AND COLUMN_DEFAULT LIKE '{3}'", SafeSchemaName(schemaName),
		//			FormatHelper.FormatSqlEscape(tableName),
		//			FormatHelper.FormatSqlEscape(columnName), defaultValueAsString);
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
				if (VersionType > SqlServerVersionType.SQLServer2000)
				{
					var sb = new StringBuilder(350);
					sb.Append("SELECT o.name as TableName, p.value as TableDescription FROM [sys].[objects] o ");
					sb.Append("INNER JOIN sys.schemas s ON s.schema_id = o.schema_id ");
					sb.Append("INNER JOIN sys.extended_properties p ON p.major_id = o.object_id AND p.minor_id = 0 AND p.name = 'MS_Description' ");
					sb.AppendFormat("WHERE s.name=N'{0}' AND o.type= 'U' ORDER BY s.name, o.name", SafeSchemaName(Owner));
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
					}, sb.ToString());
				}
				else
				{
					const String sql = @"SELECT name FROM sysobjects WHERE sysobjects.type= 'U' ORDER BY sysobjects.name";
					tables = session.ExecuteReader<List<IDataTable>>(rd =>
					{
						var list = new List<IDataTable>();
						while (rd.Read())
						{
							var dt = DAL.CreateTable();
							dt.ID = index;
							dt.TableName = rd.GetString(0);
							dt.Name = dt.TableName;
							dt.DbType = DbInternal.DbType;

							list.Add(dt);

							index++;
						}
						return list;
					}, sql);
				}

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
			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				var sb = new StringBuilder(340);
				sb.Append("SELECT o.name as TableName, p.value as TableDescription FROM [sys].[objects] o ");
				sb.Append("INNER JOIN sys.schemas s ON s.schema_id = o.schema_id ");
				sb.Append("INNER JOIN sys.extended_properties p ON p.major_id = o.object_id AND p.minor_id = 0 AND p.name = 'MS_Description' ");
				sb.AppendFormat("WHERE s.name=N'{0}' AND o.type= 'U' AND o.name=N'{1}'", SafeSchemaName(Owner), tableName);

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
						//dt.Owner = SafeSchemaName(Owner);

						return dt;
					}
					return null;
				}, sb.ToString());
			}
			else
			{
				var sb = new StringBuilder(120);
				sb.Append("SELECT name FROM sysobjects ");
				sb.AppendFormat("WHERE sysobjects.type= 'U' AND sysobjects.name=N'{0}'", tableName);
				table = session.ExecuteReader<IDataTable>(rd =>
				{
					if (rd.Read())
					{
						var dt = DAL.CreateTable();

						dt.TableName = rd.GetString(0);
						dt.Name = dt.TableName;
						dt.DbType = DbInternal.DbType;

						return dt;
					}
					return null;
				}, sb.ToString());
			}

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
			StringBuilder sb;

			#region SQL 语句

			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				sb = new StringBuilder(1150);
				sb.Append("SELECT ");
				//sb.Append("TableOwner=s.name,");
				//sb.Append("TableName=c.name,");
				sb.Append("OrdinalPosition=a.Column_id,");
				sb.Append("ColumnName=a.Name,");
				sb.Append("IsIdentity=a.is_identity,");
				//sb.Append("IDENT_SEED('[' + s.name + '].[' + c.Name + ']') AS IdentSeed, IDENT_INCR('[' + s.name + '].[' + c.Name + ']') AS IdentIncrement, ");
				sb.Append("IsPrimaryKey=case when exists(select 1 from sys.objects x join sys.indexes y on x.Type=N'PK' and x.Name=y.Name ");
				sb.Append("join sys.index_columns z on z.Object_id=a.Object_id and z.index_id=y.index_id and z.column_id=a.Column_id) ");
				sb.Append("then Convert(Bit,1) else Convert(Bit,0) end,");
				sb.Append("DataType=b.name,");
				sb.Append("ColumnSize=a.max_length,");
				sb.Append("ColumnLength=COLUMNPROPERTY(a.object_id,a.name,'PRECISION'),");
				sb.Append("ColumnScale=isnull(COLUMNPROPERTY(a.object_id,a.name,'Scale'),0),");
				sb.Append("AllowDBNull=case when a.is_nullable=1 then Convert(Bit,1)else Convert(Bit,0) end,");
				sb.Append("DefaultValue=isnull(d.text,''),");
				sb.Append("Description=isnull(e.[value],'')");
				sb.Append("FROM sys.columns a ");
				sb.Append("inner join sys.objects c on a.object_id=c.object_id and c.Type='U' ");
				sb.Append("inner join sys.schemas s ON s.schema_id = c.schema_id ");
				sb.Append("left join sys.types b on a.user_type_id=b.user_type_id ");
				sb.Append("left join syscomments d on a.default_object_id=d.ID ");
				sb.Append("left join sys.extended_properties e on e.major_id=c.object_id and e.minor_id=a.Column_id and e.class=1 ");
				sb.AppendFormat("where s.name=N'{0}' and c.name=N'{1}' ", SafeSchemaName(Owner), table.TableName);
				sb.Append("order by a.Object_id,a.column_id");
			}
			else
			{
				sb = new StringBuilder(950);
				sb.Append("SELECT ");
				//sb.Append("TableOwner='',");
				//sb.Append("TableName=d.name,");
				sb.Append("OrdinalPosition=a.colorder,");
				sb.Append("ColumnName=a.name,");
				sb.Append("IsIdentity=case when COLUMNPROPERTY(a.id,a.name,'IsIdentity')=1 then Convert(Bit,1) else Convert(Bit,0) end,");
				sb.Append("IsPrimaryKey=case when exists(SELECT 1 FROM sysobjects where xtype='PK' and name in (");
				sb.Append("SELECT name FROM sysindexes WHERE id = a.id AND indid in(");
				sb.Append("SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid");
				sb.Append("))) then Convert(Bit,1) else Convert(Bit,0) end,");
				sb.Append("DataType=b.name,");
				sb.Append("ColumnSize=a.length,");
				sb.Append("ColumnLength=COLUMNPROPERTY(a.id,a.name,'PRECISION'),");
				sb.Append("ColumnScale=isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0),");
				sb.Append("AllowDBNull=case when a.isnullable=1 then Convert(Bit,1)else Convert(Bit,0) end,");
				sb.Append("DefaultValue=isnull(e.text,''),");
				sb.Append("Description=''");
				sb.Append("FROM syscolumns a ");
				sb.Append("left join systypes b on a.xtype=b.xusertype ");
				sb.Append("inner join sysobjects d on a.id=d.id and d.xtype='U' ");
				sb.Append("left join syscomments e on a.cdefault=e.id ");
				sb.AppendFormat("where d.name=N'{0}'", table.TableName);
				sb.Append("order by a.id,a.colorder");
			}

			#endregion

			var session = DbInternal.CreateSession();
			var columns = session.ExecuteReader<List<IDataColumn>>(rd =>
			{

				var list = new List<IDataColumn>();
				while (rd.Read())
				{
					var field = table.CreateColumn();

					field.ID = rd.GetInt32(0);
					field.ColumnName = rd.GetString(1);
					field.Name = field.ColumnName;
					field.Identity = rd.GetBoolean(2);
					field.PrimaryKey = rd.GetBoolean(3);
					field.RawType = rd.GetString(4);
					field.IsUnicode = Quoter.IsUnicode(field.RawType);
					field.Precision = rd.GetInt32(6);
					field.Length = field.Precision;
					field.Scale = rd.GetInt32(7);

					field.Nullable = rd.GetBoolean(8);
					field.Default = rd.GetString(9);
					field.Description = rd.GetString(10);

					#region 数据类型识别

					switch (field.RawType.ToLowerInvariant())
					{
						case "char":
							field.DbType = CommonDbType.AnsiStringFixedLength;
							field.DataType = typeof(String);
							break;
						case "nchar":
							field.DbType = CommonDbType.StringFixedLength;
							field.DataType = typeof(String);
							break;
						case "varchar":
							if (field.Length < 0) { field.Length = Int32.MaxValue; }
							field.DbType = CommonDbType.AnsiString;
							field.DataType = typeof(String);
							break;
						case "nvarchar":
							if (field.Length < 0) { field.Length = 1073741823; }
							field.DbType = CommonDbType.String;
							field.DataType = typeof(String);
							break;
						case "text":
							field.DbType = CommonDbType.AnsiString; // 特殊处理
							field.DataType = typeof(String);
							break;
						case "ntext":
							field.DbType = CommonDbType.Text;
							field.DataType = typeof(String);
							break;

						case "xml":
							field.DbType = CommonDbType.Text;
							field.DataType = typeof(String);
							break;

						case "binary":
							field.DbType = CommonDbType.BinaryFixedLength;
							field.DataType = typeof(Byte[]);
							break;
						case "varbinary":
							if (field.Length < 0) { field.Length = Int32.MaxValue; }
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;
						case "image":
							field.DbType = CommonDbType.Binary;
							field.DataType = typeof(Byte[]);
							break;

						case "bit":
							field.DbType = CommonDbType.Boolean;
							field.DataType = typeof(Boolean);
							break;

						case "uniqueidentifier":
							field.DbType = CommonDbType.CombGuid;
							field.DataType = typeof(CombGuid);
							break;

						case "date":
							field.DbType = CommonDbType.Date;
							field.DataType = typeof(DateTime);
							break;
						case "datetime":
							field.DbType = CommonDbType.DateTime;
							field.DataType = typeof(DateTime);
							break;
						case "datetime2":
							field.DbType = CommonDbType.DateTime2;
							field.DataType = typeof(DateTime);
							break;
						case "datetimeoffset":
							field.DbType = CommonDbType.DateTimeOffset;
							field.DataType = typeof(DateTimeOffset);
							break;
						case "time":
							field.DbType = CommonDbType.Time;
							field.DataType = typeof(TimeSpan);
							break;
						//case "timestamp":

						case "money":
							field.DbType = CommonDbType.Currency;
							field.DataType = typeof(Decimal);
							break;
						case "number":
						case "decimal":
							field.DbType = CommonDbType.Decimal;
							field.DataType = typeof(Decimal);
							break;

						case "tinyint":
							field.DbType = CommonDbType.TinyInt;
							field.DataType = typeof(Byte);
							break;
						case "smallint":
							field.DbType = CommonDbType.SmallInt;
							field.DataType = typeof(Int16);
							break;
						case "int":
							field.DbType = CommonDbType.Integer;
							field.DataType = typeof(Int32);
							break;
						case "bigint":
							field.DbType = CommonDbType.BigInt;
							field.DataType = typeof(Int64);
							break;

						case "real":
							field.DbType = CommonDbType.Float;
							field.DataType = typeof(Single);
							break;
						case "float":
							field.DbType = CommonDbType.Double;
							field.DataType = typeof(Double);
							break;

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
			var sb = new StringBuilder(550);
			sb.Append("select ");
			sb.Append("s.name AS constraint_schema,");
			sb.Append("si.name AS constraint_name,");
			sb.Append("st.name AS table_schema,");
			sb.Append("t.name AS table_name,");
			sb.Append("si.name AS index_name,");
			sb.Append("si.type_desc AS type_desc,");
			sb.Append("si.is_unique AS is_unique,");
			sb.Append("si.is_primary_key AS is_primary_key ");
			sb.Append("from sys.indexes si ");
			sb.Append("inner join sys.objects o on o.object_id = si.object_id ");
			sb.Append("inner join sys.tables t on si.object_id = t.object_id ");
			sb.Append("inner join sys.schemas s on s.schema_id = o.schema_id ");
			sb.Append("inner join sys.schemas st on st.schema_id = t.schema_id ");
			sb.AppendFormat("where st.name=N'{0}' and t.name=N'{1}'", SafeSchemaName(Owner), table.TableName);

			var session = DbInternal.CreateSession();
			var allIndexs = session.ExecuteReader<List<IDataIndex>>(rd =>
			{
				var list = new List<IDataIndex>();
				while (rd.Read())
				{
					if (rd.IsDBNull(1)) { continue; }
					var name = rd.GetString(1);
					if (name.IsNullOrWhiteSpace()) { continue; }

					var di = table.CreateIndex();

					di.Name = name;
					di.Unique = rd.GetBoolean(6);
					di.PrimaryKey = rd.GetBoolean(7);
					di.Clustered = "CLUSTERED".EqualIgnoreCase(rd.GetString(5));

					list.Add(di);
				}
				return list;
			}, sb.ToString());

			foreach (var index in allIndexs)
			{
				var sql = new StringBuilder(750);
				sql.Append("SELECT ");
				//sql.Append("st.name AS table_schema,");
				//sql.Append("t.name AS table_name,");
				//sql.Append("I.Name AS IndexName, ");
				//sql.Append("IC.key_ordinal,");
				sql.Append("C.Name AS ColumnName, ");
				sql.Append("is_unique, ");
				sql.Append("is_primary_key, ");
				sql.Append("is_unique_constraint, ");
				sql.Append("IC.is_descending_key, ");
				sql.Append("IC.is_included_column ");
				sql.Append("FROM sys.indexes I ");
				sql.Append("INNER JOIN sys.objects OO ON OO.object_id = I.object_id ");
				sql.Append("INNER JOIN sys.tables t ON I.object_id = t.object_id ");
				sql.Append("INNER JOIN sys.schemas st ON st.schema_id = t.schema_id ");
				sql.Append("INNER JOIN sys.index_columns IC ON IC.index_id = I.index_id AND IC.object_id = I.object_id ");
				sql.Append("INNER JOIN sys.columns C ON C.column_id = IC.column_id AND IC.object_id = C.object_id ");
				sql.Append("WHERE I.type IN (1,2,3) ");
				sql.Append("AND objectproperty(I.object_id, 'IsMSShipped') <> 1 ");
				sql.AppendFormat("AND st.name=N'{0}' ", SafeSchemaName(Owner));
				sql.AppendFormat("AND t.name=N'{0}' ", table.TableName);
				sql.AppendFormat("AND I.Name=N'{0}' ", index.Name);
				//sql.Append("ORDER BY I.object_id, I.Name, IC.column_id");
				sql.Append("ORDER BY IC.index_column_id");

				var columns = session.ExecuteReader<List<String>>(rd =>
				{
					var names = new List<String>();
					while (rd.Read())
					{
						names.Add(rd.GetString(0));
					}
					return names;
				}, sql.ToString());
				index.Columns = columns.ToArray();
			}

			return allIndexs;
		}

		#endregion

		#endregion

		#region -- 反向 --

		#region - DropDatabase -

		internal override Boolean DropDatabase(String databaseName, IDbSession session)
		{
			// SQL语句片段，断开该数据库所有链接
			var sb = new StringBuilder();
			sb.AppendLine("use master");
			sb.AppendLine(";");
			sb.AppendLine("declare   @spid   varchar(20),@dbname   varchar(20)");
			sb.AppendLine("declare   #spid   cursor   for");
			sb.AppendFormat("select   spid=cast(spid   as   varchar(20))   from   master..sysprocesses   where   dbid=db_id('{0}')", databaseName);
			sb.AppendLine();
			sb.AppendLine("open   #spid");
			sb.AppendLine("fetch   next   from   #spid   into   @spid");
			sb.AppendLine("while   @@fetch_status=0");
			sb.AppendLine("begin");
			sb.AppendLine("exec('kill   '+@spid)");
			sb.AppendLine("fetch   next   from   #spid   into   @spid");
			sb.AppendLine("end");
			sb.AppendLine("close   #spid");
			sb.AppendLine("deallocate   #spid");

			var count = 0;
			if (session == null) { session = DbInternal.CreateSession(); }
			try { count = session.Execute(sb.ToString()); }
			catch { }
			return session.Execute(String.Format("Drop Database {0}", Quoter.QuoteDataBaseName(databaseName))) > 0;
		}

		#endregion

		#region - Table -

		internal override String SetIdentityInsertOn(IDataTable entityTable, IDataTable dbTable)
		{
			// 特殊处理带标识列的表，需要增加SET IDENTITY_INSERT
			if (!entityTable.Columns.Any(e => e.Identity)) { return String.Empty; }
			// 旧数据表无标识列
			if (!dbTable.Columns.Any(e => e.Identity)) { return String.Empty; }

			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				return "SET IDENTITY_INSERT {0}.{1} ON; ".FormatWith(Quoter.QuoteSchemaName(Owner), Quoter.QuoteTableName(entityTable.TableName));
			}
			else
			{
				return "SET IDENTITY_INSERT {0} ON; ".FormatWith(Quoter.QuoteTableName(entityTable.TableName));
			}
		}

		internal override String SetIdentityInsertOff(IDataTable entityTable, IDataTable dbTable)
		{
			// 特殊处理带标识列的表，需要增加SET IDENTITY_INSERT
			if (!entityTable.Columns.Any(e => e.Identity)) { return String.Empty; }
			// 旧数据表无标识列
			if (!dbTable.Columns.Any(e => e.Identity)) { return String.Empty; }

			if (VersionType > SqlServerVersionType.SQLServer2000)
			{
				return "SET IDENTITY_INSERT {0}.{1} OFF; ".FormatWith(Quoter.QuoteSchemaName(Owner), Quoter.QuoteTableName(entityTable.TableName));
			}
			else
			{
				return "SET IDENTITY_INSERT {0} OFF; ".FormatWith(Quoter.QuoteTableName(entityTable.TableName));
			}
		}

		#endregion

		#endregion

		#region -- 辅助 --

		private static string SafeSchemaName(String schemaName)
		{
			return schemaName.IsNullOrWhiteSpace() ? "dbo" : Helper.FormatSqlEscape(schemaName);
		}

		#endregion
	}
}
