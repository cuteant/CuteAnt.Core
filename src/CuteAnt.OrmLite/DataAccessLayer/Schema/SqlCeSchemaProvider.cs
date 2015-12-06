using System;
using System.Collections.Generic;
using System.Text;
using CuteAnt.OrmLite.Common;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal partial class SqlCeSchemaProvider : FileDbSchemaProvider
	{
		#region -- 架构检查 --

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'".FormatWith(Helper.FormatSqlEscape(tableName)));
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'".FormatWith(
					Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(columnName)));
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'".FormatWith(
					Helper.FormatSqlEscape(tableName), Helper.FormatSqlEscape(constraintName)));
		}

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			var session = DbInternal.CreateSession();
			return session.Exists("SELECT NULL FROM INFORMATION_SCHEMA.INDEXES WHERE INDEX_NAME = '{0}'".FormatWith(Helper.FormatSqlEscape(indexName)));
		}

		#endregion
	}
}
