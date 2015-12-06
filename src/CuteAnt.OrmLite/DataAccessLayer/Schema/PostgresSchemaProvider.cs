using System;
using CuteAnt.OrmLite.Common;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal partial class PostgresSchemaProvider : RemoteDbSchemaProvider
	{
		#region -- 属性 --

		#endregion

		#region -- 架构检查 --

		/// <summary>查询指定的 Schema 是否存在</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public override Boolean SchemaExists(String schemaName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select * from information_schema.schemata where schema_name = '{0}'".FormatWith(FormatToSafeSchemaName(schemaName)));
		}

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}'".FormatWith(
					FormatToSafeSchemaName(Owner), FormatToSafeName(tableName)));
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select * from information_schema.columns where table_schema = '{0}' and table_name = '{1}' and column_name = '{2}'".FormatWith(
					FormatToSafeSchemaName(Owner), FormatToSafeName(tableName), FormatToSafeName(columnName)));
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select * from information_schema.table_constraints where constraint_catalog = current_catalog and table_schema = '{0}' and table_name = '{1}' and constraint_name = '{2}'".FormatWith(
					FormatToSafeSchemaName(Owner), FormatToSafeName(tableName), FormatToSafeName(constraintName)));
		}

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select * from pg_catalog.pg_indexes where schemaname='{0}' and tablename = '{1}' and indexname = '{2}'".FormatWith(
					FormatToSafeSchemaName(Owner), FormatToSafeName(tableName), FormatToSafeName(indexName)));
		}

		/// <summary>根据序列名称检查序列是否存在</summary>
		/// <param name="sequenceName">序列名称</param>
		/// <returns></returns>
		public override Boolean SequenceExists(String sequenceName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("select * from information_schema.sequences where sequence_catalog = current_catalog and sequence_schema ='{0}' and sequence_name = '{1}'".FormatWith(
					FormatToSafeSchemaName(Owner), FormatToSafeName(sequenceName)));
		}

		//public override Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue)
		//{
		//		string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
		//		return Exists("select * from information_schema.columns where table_schema = '{0}' and table_name = '{1}' and column_name = '{2}' and column_default like '{3}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName), defaultValueAsString);
		//}

		#endregion

		#region -- 辅助函数 --

		private String FormatToSafeSchemaName(String schemaName)
		{
			return Helper.FormatSqlEscape(UnQuoteSchemaName(schemaName));
		}

		private String FormatToSafeName(String sqlName)
		{
			return Helper.FormatSqlEscape(Quoter.UnQuote(sqlName));
		}

		private String UnQuoteSchemaName(String quoted)
		{
			if (quoted.IsNullOrWhiteSpace()) { return "public"; }

			return Quoter.UnQuote(quoted);
		}

		#endregion
	}
}
