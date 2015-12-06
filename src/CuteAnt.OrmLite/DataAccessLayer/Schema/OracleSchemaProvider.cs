using System;
using System.Data;
using CuteAnt.OrmLite.Common;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal partial class OracleSchemaProvider : RemoteDbSchemaProvider
	{
		#region -- 属性 --

		#endregion

		#region 架构检查

		/// <summary>查询指定的 Schema 是否存在</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public override Boolean SchemaExists(String schemaName)
		{
			if (schemaName.IsNullOrWhiteSpace()) { return false; }

			var session = DbInternal.CreateSession();
			return session.Exists("SELECT 1 FROM ALL_USERS WHERE USERNAME = '{0}'".FormatWith(schemaName.ToUpperInvariant()));
		}

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public override Boolean TableExists(String tableName)
		{
			if (tableName.IsNullOrWhiteSpace()) { return false; }

			var session = DbInternal.CreateSession();
			var owner = Owner;
			if (owner.IsNullOrWhiteSpace())
			{
				return session.Exists("SELECT 1 FROM USER_TABLES WHERE upper(TABLE_NAME) = '{0}'".FormatWith(Helper.FormatSqlEscape(tableName.ToUpperInvariant())));
			}
			else
			{
				return session.Exists("SELECT 1 FROM ALL_TABLES WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}'".FormatWith(owner.ToUpperInvariant(), Helper.FormatSqlEscape(tableName.ToUpperInvariant())));
			}
		}

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		public override Boolean ColumnExists(String tableName, String columnName)
		{
			if (tableName.IsNullOrWhiteSpace() || columnName.IsNullOrWhiteSpace()) { return false; }

			var session = DbInternal.CreateSession();
			var owner = Owner;
			if (owner.IsNullOrWhiteSpace())
			{
				return session.Exists("SELECT 1 FROM USER_TAB_COLUMNS WHERE upper(TABLE_NAME) = '{0}' AND upper(COLUMN_NAME) = '{1}'".FormatWith(
						Helper.FormatSqlEscape(tableName.ToUpperInvariant()), Helper.FormatSqlEscape(columnName.ToUpperInvariant())));
			}
			else
			{
				return session.Exists("SELECT 1 FROM ALL_TAB_COLUMNS WHERE upper(OWNER) = '{0}' AND upper(TABLE_NAME) = '{1}' AND upper(COLUMN_NAME) = '{2}'".FormatWith(
						owner.ToUpperInvariant(), Helper.FormatSqlEscape(tableName.ToUpperInvariant()), Helper.FormatSqlEscape(columnName.ToUpperInvariant())));
			}
		}

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		public override Boolean ConstraintExists(String tableName, String constraintName)
		{
			if (constraintName.IsNullOrWhiteSpace()) { return false; }

			// In Oracle DB constraint name is unique within the schema, so the table name is not used in the query

			var session = DbInternal.CreateSession();
			var owner = Owner;
			if (owner.IsNullOrWhiteSpace())
			{
				return session.Exists("SELECT 1 FROM USER_CONSTRAINTS WHERE upper(CONSTRAINT_NAME) = '{0}'".FormatWith(Helper.FormatSqlEscape(constraintName.ToUpperInvariant())));
			}
			else
			{
				return session.Exists("SELECT 1 FROM ALL_CONSTRAINTS WHERE upper(OWNER) = '{0}' AND upper(CONSTRAINT_NAME) = '{1}'".FormatWith(
						owner.ToUpperInvariant(), Helper.FormatSqlEscape(constraintName.ToUpperInvariant())));
			}
		}

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		public override Boolean IndexExists(String tableName, String indexName)
		{
			if (indexName.IsNullOrWhiteSpace()) { return false; }

			// In Oracle DB index name is unique within the schema, so the table name is not used in the query

			var session = DbInternal.CreateSession();
			var owner = Owner;
			if (owner.IsNullOrWhiteSpace())
			{
				return session.Exists("SELECT 1 FROM USER_INDEXES WHERE upper(INDEX_NAME) = '{0}'".FormatWith(Helper.FormatSqlEscape(indexName.ToUpperInvariant())));
			}
			else
			{
				return session.Exists("SELECT 1 FROM ALL_INDEXES WHERE upper(OWNER) = '{0}' AND upper(INDEX_NAME) = '{1}'".FormatWith(
					owner.ToUpperInvariant(), Helper.FormatSqlEscape(indexName.ToUpperInvariant())));
			}
		}

		/// <summary>序列</summary>
		private DataTable dtSequences;

		/// <summary>检查序列是否存在</summary>
		/// <param name="sequenceName">名称</param>
		/// <returns></returns>
		public override Boolean SequenceExists(String sequenceName)
		{
			if (dtSequences == null)
			{
				DataSet ds = null;
				if (Owner.IsNullOrWhiteSpace())
				{
					ds = DbInternal.CreateSession().Query("SELECT * FROM ALL_SEQUENCES");
				}
				else
				{
					ds = DbInternal.CreateSession().Query("SELECT * FROM ALL_SEQUENCES Where SEQUENCE_OWNER='" + Owner.ToUpperInvariant() + "'");
				}

				if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
				{
					dtSequences = ds.Tables[0];
				}
				else
				{
					dtSequences = new DataTable();
				}
			}
			if (dtSequences.Rows == null || dtSequences.Rows.Count < 1) { return false; }

			String where = null;
			if (Owner.IsNullOrWhiteSpace())
			{
				where = String.Format("SEQUENCE_NAME='{0}'", sequenceName);
			}
			else
			{
				where = String.Format("SEQUENCE_NAME='{0}' And SEQUENCE_OWNER='{1}'", sequenceName, Owner.ToUpperInvariant());
			}

			DataRow[] drs = dtSequences.Select(where);
			return drs != null && drs.Length > 0;

			//String sql = String.Format("SELECT Count(*) FROM ALL_SEQUENCES Where SEQUENCE_NAME='{0}' And SEQUENCE_OWNER='{1}'", name, Owner);
			//return Convert.ToInt32(Database.CreateSession().ExecuteScalar(sql)) > 0;
		}

		#endregion
	}
}
