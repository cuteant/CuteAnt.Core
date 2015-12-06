using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CuteAnt.OrmLite.Common;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal partial class FirebirdSchemaProvider : FileDbSchemaProvider
	{
		#region -- 架构检查 --

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select rdb$relation_name from rdb$relations where (rdb$flags IS NOT NULL) and (rdb$relation_name = '{0}')".FormatWith(
					FormatToSafeName(tableName)));
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select rdb$field_name from rdb$relation_fields where (rdb$relation_name = '{0}') and (rdb$field_name = '{1}')".FormatWith(
					FormatToSafeName(tableName), FormatToSafeName(columnName)));
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select rdb$constraint_name from rdb$relation_constraints where (rdb$relation_name = '{0}') and (rdb$constraint_name = '{1}')".FormatWith(
					FormatToSafeName(tableName), FormatToSafeName(constraintName)));
		}

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select rdb$index_name from rdb$indices where (rdb$relation_name = '{0}') and (rdb$index_name = '{1}') and (rdb$unique_flag IS NULL) and (rdb$foreign_key IS NULL)".FormatWith(
					FormatToSafeName(tableName), FormatToSafeName(indexName)));
		}

		/// <summary>根据序列名称检查序列是否存在</summary>
		/// <param name="sequenceName">序列名称</param>
		/// <returns></returns>
		public override Boolean SequenceExists(String sequenceName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select rdb$generator_name from rdb$generators where rdb$generator_name = '{0}'".FormatWith(
					FormatToSafeName(sequenceName)));
		}

		public virtual Boolean TriggerExists(String tableName, String triggerName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select rdb$trigger_name from rdb$triggers where (rdb$relation_name = '{0}') and (rdb$trigger_name = '{1}')".FormatWith(
					FormatToSafeName(tableName), FormatToSafeName(triggerName)));
		}

		//public override Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue) { return false; }

		#endregion

		#region -- 正向 --

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		protected override List<IDataTable> OnGetTables(ICollection<String> names)
		{
			DataTable dt = GetSchema(_.Tables, new String[] { null, null, null, "TABLE" });

			// 默认列出所有字段
			DataRow[] rows = OnGetTables(names, dt.Rows);
			if (rows == null || rows.Length < 1) { return null; }

			return GetTables(rows);
		}

		protected override String GetFieldType(IDataColumn field)
		{
			if (field.DataType == typeof(Boolean)) return "smallint";
			return base.GetFieldType(field);
		}

		#endregion

		#region -- 反向 --

		/// <summary>已重载，创建数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <param name="databasePath">数据库路径</param>
		public override void CreateDatabase(String databaseName, String databasePath)
		{
			//base.CreateDatabase();

			if (FileName.IsNullOrWhiteSpace() || File.Exists(FileName)) { return; }

			//The miminum you must specify:
			//Hashtable parameters = new Hashtable();
			//parameters.Add("User", "SYSDBA");
			//parameters.Add("Password", "masterkey");
			//parameters.Add("Database", @"c:\database.fdb");
			//FbConnection.CreateDatabase(parameters);

			DAL.WriteDebugLog("创建数据库：{0}", FileName);

			var conn = DbInternal.Factory.CreateConnection();
			//var method = Reflect.GetMethodEx(conn.GetType(), "CreateDatabase", typeof(String));
			var method = conn.GetType().GetMethodEx("CreateDatabase", typeof(String));
			if (method == null) { return; }

			Reflect.Invoke(null, method, DbInternal.ConnectionString);
		}

		#endregion

		#region -- 辅助 --

		private string FormatToSafeName(string sqlName)
		{
			return Helper.FormatSqlEscape(Quoter.UnQuote(sqlName));
		}

		#endregion
	}
}
