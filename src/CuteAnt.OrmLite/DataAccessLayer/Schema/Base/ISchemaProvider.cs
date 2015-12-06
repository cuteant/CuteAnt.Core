using System;
using System.Collections.Generic;
using System.Data;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>数据库架构接口</summary>
	public partial interface ISchemaProvider
	{
		#region -- 属性 --

		/// <summary>数据库</summary>
		IDatabase Database { get; }

		/// <summary>拥有者</summary>
		String Owner { get; }

		/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
		IQuoter Quoter { get; }

		/// <summary>所有元数据集合</summary>
		ICollection<String> MetaDataCollections { get; }

		/// <summary>保留关键字</summary>
		ICollection<String> ReservedWords { get; }

		/// <summary>数据类型</summary>
		DataTable DataTypes { get; }

		#endregion

		#region -- 架构检查 --

		/// <summary>返回数据源的架构信息</summary>
		/// <param name="collectionName">指定要返回的架构的名称。</param>
		/// <param name="restrictionValues">为请求的架构指定一组限制值。</param>
		/// <returns></returns>
		DataTable GetSchema(String collectionName, String[] restrictionValues);

		/// <summary>数据库是否存在</summary>
		/// <returns></returns>
		Boolean DatabaseExist();

		/// <summary>查询指定的 Schema 是否存在</summary>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		Boolean SchemaExists(String schemaName);

		/// <summary>根据表名查询数据表是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		Boolean TableExists(String tableName);

		/// <summary>根据表名、数据列名称检查数据列是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnName">数据列名称</param>
		/// <returns></returns>
		Boolean ColumnExists(String tableName, String columnName);

		/// <summary>根据表名、约束名称检查约束是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="constraintName">约束名称</param>
		/// <returns></returns>
		Boolean ConstraintExists(String tableName, String constraintName);

		/// <summary>根据表名、索引名称检查索引是否存在</summary>
		/// <param name="tableName">表名</param>
		/// <param name="indexName">索引名称</param>
		/// <returns></returns>
		Boolean IndexExists(String tableName, String indexName);

		/// <summary>根据序列名称检查序列是否存在</summary>
		/// <param name="sequenceName">序列名称</param>
		/// <returns></returns>
		Boolean SequenceExists(String sequenceName);

		//public virtual Boolean DefaultValueExists(String tableName, String columnName, Object defaultValue) { return false; }

		#endregion

		#region -- 正向 --

		/// <summary>取得所有表构架</summary>
		/// <returns></returns>
		List<IDataTable> GetTables();

		/// <summary>取得表构架</summary>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		IDataTable GetTable(String tableName);

		#endregion

		#region -- 反向 --

		#region - DataBase -

		/// <summary>创建数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <param name="databasePath">数据库路径</param>
		void CreateDatabase(String databaseName, String databasePath);

		/// <summary>删除数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <returns></returns>
		void DropDatabase(String databaseName);

		#endregion

		#region - Schema -

		/// <summary>Create Schema</summary>
		/// <param name="schemaName"></param>
		void CreateSchema(String schemaName);

		/// <summary>Delete Schema</summary>
		/// <param name="schemaName"></param>
		void DeleteSchema(String schemaName);

		/// <summary>Alter Schema</summary>
		/// <param name="srcSchemaName"></param>
		/// <param name="tableName"></param>
		/// <param name="destSchemaName"></param>
		void AlterSchema(String srcSchemaName, String tableName, String destSchemaName);

		#endregion

		#region - Table -

		/// <summary>设置表模型，检查数据表是否匹配表模型，反向工程</summary>
		/// <param name="setting">设置</param>
		/// <param name="tables"></param>
		void SetTables(NegativeSetting setting, params IDataTable[] tables);

		/// <summary>创建表</summary>
		/// <param name="table"></param>
		void CreateTable(IDataTable table);

		/// <summary>删除表</summary>
		/// <param name="tableName"></param>
		void DropTable(String tableName);

		/// <summary>重命名表</summary>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		void RenameTable(String oldName, String newName);

		/// <summary>修改表注释</summary>
		/// <param name="table"></param>
		void AlterTable(IDataTable table);

		#endregion

		#region - Column -

		/// <summary>修改字段</summary>
		/// <param name="column"></param>
		void AlterColumn(IDataColumn column);

		/// <summary>创建字段</summary>
		/// <param name="column"></param>
		void CreateColumn(IDataColumn column);

		/// <summary>删除字段</summary>
		/// <param name="tableName"></param>
		/// <param name="columnName"></param>
		void DropColumn(String tableName, String columnName);

		/// <summary>重命名字段</summary>
		/// <param name="tableName"></param>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		void RenameColumn(String tableName, String oldName, String newName);

		#endregion

		#region - Index -

		/// <summary>创建索引</summary>
		/// <param name="dataIndex"></param>
		void CreateIndex(IDataIndex dataIndex);

		/// <summary>删除索引</summary>
		/// <param name="tableName"></param>
		/// <param name="indexName"></param>
		void DeleteIndex(String tableName, String indexName);

		#endregion

		#endregion
	}
}
