/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;
using CuteAnt.OrmLite.Cache;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>数据实体操作接口</summary>
	public interface IEntityOperate
	{
		#region -- 主要属性 --

		/// <summary>实体类型</summary>
		Type EntityType { get; }

		/// <summary>实体会话</summary>
		IEntitySession Session { get; }

		#endregion

		#region -- 属性 --

		/// <summary>默认实体</summary>
		IEntity Default { get; set; }

		/// <summary>数据表元数据</summary>
		TableItem Table { get; }

		// ## 苦竹 修改 ##
		///// <summary>所有数据属性</summary>
		//FieldItem[] AllFields { get; }

		///// <summary>所有绑定到数据表的属性</summary>
		//FieldItem[] Fields { get; }

		///// <summary>字段名列表</summary>
		//IList<String> FieldNames { get; }
		/// <summary>所有数据属性</summary>
		IList<FieldItem> AllFields { get; }

		/// <summary>所有绑定到数据表的属性</summary>
		IList<FieldItem> Fields { get; }

		/// <summary>所有绑定到数据表的SQL语句转义字段名称</summary>
		IEnumerable<String> QuotedColumnNames { get; }

		/// <summary>字段名集合，不区分大小写的哈希表存储，外部不要修改元素数据</summary>
		ISet<String> FieldNames { get; }

		/// <summary>唯一键，返回第一个标识列或者唯一的主键</summary>
		FieldItem Unique { get; }

		/// <summary>主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
		FieldItem Master { get; }

		/// <summary>连接名</summary>
		String ConnName { get; set; }

		/// <summary>表名</summary>
		String TableName { get; set; }

		/// <summary>已格式化的表名，带有中括号等</summary>
		String FormatedTableName { get; }

		/// <summary>实体缓存</summary>
		IEntityCache Cache { get; }

		/// <summary>单对象实体缓存</summary>
		ISingleEntityCache SingleCache { get; }

		/// <summary>总记录数</summary>
		Int64 Count { get; }

		#endregion

		#region -- 创建实体、填充数据 --

		/// <summary>创建一个实体对象</summary>
		/// <param name="forEdit">是否为了编辑而创建，如果是，可以再次做一些相关的初始化工作</param>
		/// <returns></returns>
		IEntity Create(Boolean forEdit = false);

		/// <summary>加载记录集</summary>
		/// <param name="ds">记录集</param>
		/// <returns>实体数组</returns>
		[Obsolete("请使用LoadDataToList")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IEntityList LoadData(DataSet ds);

		/// <summary>加载实体记录集合</summary>
		/// <param name="ds">记录集</param>
		/// <returns>实体集合</returns>
		IEntityList LoadDataToList(DataSet ds);

		/// <summary>加载实体记录哈希集合</summary>
		/// <param name="ds">记录集</param>
		/// <returns>实体哈希集合</returns>
		IEntitySet LoadDataToSet(DataSet ds);

		#endregion

		#region -- 批量操作 --

		/// <summary>根据条件删除实体记录，此操作跨越缓存，使用事务保护</summary>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="batchSize">每次删除记录数</param>
		void DeleteAll(String whereClause, Int32 batchSize);

		/// <summary>根据条件删除实体记录，使用读写锁令牌，缩小事务范围，删除时不再确保数据一致性，慎用！！！
		/// <para>如果删除操作不带业务，可直接使用静态方法 Delete(String whereClause)</para>
		/// </summary>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="batchSize">每次删除记录数</param>
		void DeleteAllWithLockToken(String whereClause, Int32 batchSize);

		/// <summary>批量处理实体记录，此操作跨越缓存</summary>
		/// <param name="action">处理实体记录集方法</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <param name="batchSize">每次处理记录数</param>
		/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
		void ProcessAll(Action<IEntityList> action, Boolean useTransition, Int32 batchSize, Int32 maxCount);

		/// <summary>批量处理实体记录，此操作跨越缓存</summary>
		/// <param name="action">处理实体记录集方法</param>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <param name="batchSize">每次处理记录数</param>
		/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
		void ProcessAll(Action<IEntityList> action, String whereClause, Boolean useTransition, Int32 batchSize, Int32 maxCount);

		/// <summary>批量处理实体记录，此操作跨越缓存，使用事务保护</summary>
		/// <param name="action">实体记录操作方法</param>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="selects">查询列</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <param name="batchSize">每次处理记录数</param>
		/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
		void ProcessAll(Action<IEntityList> action, String whereClause, String orderClause, String selects, Boolean useTransition, Int32 batchSize, Int32 maxCount);

		/// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
		/// <param name="action">处理实体记录集方法</param>
		/// <param name="actionLockType">操作方法锁令牌方式</param>
		/// <param name="batchSize">每次处理记录数</param>
		/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
		void ProcessAllWithLockToken(Action<IEntityList> action, ActionLockTokenType actionLockType, Int32 batchSize, Int32 maxCount);

		/// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
		/// <param name="action">处理实体记录集方法</param>
		/// <param name="actionLockType">操作方法锁令牌方式</param>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="batchSize">每次处理记录数</param>
		/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
		void ProcessAllWithLockToken(Action<IEntityList> action, ActionLockTokenType actionLockType, String whereClause, Int32 batchSize, Int32 maxCount);

		/// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
		/// <param name="action">处理实体记录集方法</param>
		/// <param name="actionLockType">操作方法锁令牌方式</param>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="selects">查询列</param>
		/// <param name="batchSize">每次处理记录数</param>
		/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
		void ProcessAllWithLockToken(Action<IEntityList> action, ActionLockTokenType actionLockType, String whereClause, String orderClause, String selects, Int32 batchSize, Int32 maxCount);

		/// <summary>实体数据迁移，调用此方法前请确定进行了数据分片配置。</summary>
		/// <param name="entities">实体数据列表</param>
		/// <param name="keepIdentity">是否允许向自增列插入数据</param>
		/// <param name="batchSize">单条SQL语句插入数据数</param>
		void TransformAll(IEntityList entities, Boolean keepIdentity, Int32 batchSize);

		/// <summary>实体数据迁移，调用此方法前请确定进行了数据分片配置。</summary>
		/// <param name="dt">实体数据表</param>
		/// <param name="keepIdentity">是否允许向自增列插入数据</param>
		/// <remarks>SQL Server 2008或2008以上版本使用表值参数（Table-valued parameters）进行批量插入会更快，但需要为每个表单独建立TVP。</remarks>
		void TransformAll(DataTable dt, Boolean keepIdentity);

		#endregion

		#region -- 查找单个实体 --

		/// <summary>根据属性以及对应的值，查找单个实体</summary>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		IEntity Find(String name, Object value);

		// ## 苦竹 添加 2012.12.12 PM 19:45 ##
		/// <summary>根据属性列表以及对应的值列表，查找单个实体</summary>
		/// <param name="names">属性名称集合</param>
		/// <param name="values">属性值集合</param>
		/// <returns></returns>
		IEntity Find(String[] names, Object[] values);

		/// <summary>根据条件查找单个实体</summary>
		/// <param name="whereClause"></param>
		/// <returns></returns>
		IEntity Find(String whereClause);

		/// <summary>根据主键查找单个实体</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IEntity FindByKey(Object key);

		/// <summary>根据主键查询一个实体对象用于表单编辑</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IEntity FindByKeyForEdit(Object key);

		#endregion

		#region -- 静态查询 --

		#region - IEntityList -

		/// <summary>获取所有实体对象。获取大量数据时会非常慢，慎用</summary>
		/// <returns>实体数组</returns>
		IEntityList FindAll();

		/// <summary>查询并返回实体对象集合。
		/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
		/// </summary>
		/// <param name="whereClause">条件字句，不带Where</param>
		/// <param name="orderClause">排序字句，不带Order By</param>
		/// <param name="selects">查询列，默认null表示所有字段</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		IEntityList FindAll(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
		/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
		/// <returns></returns>
		IEntityList FindAll(PageParameter param);

		/// <summary>根据属性列表以及对应的值列表查询数据。没有数据时返回空集合而不是null</summary>
		/// <param name="names">属性列表</param>
		/// <param name="values">值列表</param>
		/// <returns>实体数组</returns>
		IEntityList FindAll(String[] names, Object[] values);

		/// <summary>根据属性以及对应的值查询数据。没有数据时返回空集合而不是null</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <returns>实体数组</returns>
		IEntityList FindAll(String name, Object value);

		/// <summary>根据属性以及对应的值查询数据，带排序。没有数据时返回空集合而不是null</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		IEntityList FindAllByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows);

		// ## 苦竹 添加 2012.12.12 PM 19:40 ##
		/// <summary>查询SQL并返回实体对象数组。
		/// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
		/// </summary>
		/// <param name="sql">查询语句</param>
		/// <returns>实体数组</returns>
		IEntityList FindAll(String sql);

		#endregion

		#region - IEntityList WithLockToken -

		/// <summary>获取所有实体对象，执行SQL查询时使用读锁令牌</summary>
		/// <returns>实体数组</returns>
		IEntityList FindAllWithLockToken();

		/// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌
		/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
		/// </summary>
		/// <param name="whereClause">条件字句，不带Where</param>
		/// <param name="orderClause">排序字句，不带Order By</param>
		/// <param name="selects">查询列，默认null表示所有字段</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		IEntityList FindAllWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
		/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
		/// <returns></returns>
		IEntityList FindAllWithLockToken(PageParameter param);

		#endregion

		#region - IEntitySet -

		/// <summary>获取所有实体对象哈希集合。获取大量数据时会非常慢，慎用</summary>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSet();

		/// <summary>查询并返回实体对象哈希集合。
		/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
		/// </summary>
		/// <param name="whereClause">条件字句，不带Where</param>
		/// <param name="orderClause">排序字句，不带Order By</param>
		/// <param name="selects">查询列，默认null表示所有字段</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
		/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
		/// <returns></returns>
		IEntitySet FindAllSet(PageParameter param);

		/// <summary>根据属性列表以及对应的值列表查询数据。没有数据时返回空集合而不是null</summary>
		/// <param name="names">属性列表</param>
		/// <param name="values">值列表</param>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSet(String[] names, Object[] values);

		/// <summary>根据属性以及对应的值查询数据。没有数据时返回空集合而不是null</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSet(String name, Object value);

		/// <summary>根据属性以及对应的值查询数据，带排序。没有数据时返回空集合而不是null</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSetByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>查询SQL并返回实体对象哈希集合。
		/// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
		/// </summary>
		/// <param name="sql">查询语句</param>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSet(String sql);

		#endregion

		#region - IEntitySet WithLockToken -

		/// <summary>获取所有实体对象哈希集合，执行SQL查询时使用读锁令牌</summary>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSetWithLockToken();

		/// <summary>查询并返回实体对象哈希集合，执行SQL查询时使用读锁令牌
		/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
		/// </summary>
		/// <param name="whereClause">条件字句，不带Where</param>
		/// <param name="orderClause">排序字句，不带Order By</param>
		/// <param name="selects">查询列，默认null表示所有字段</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		IEntitySet FindAllSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
		/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
		/// <returns></returns>
		IEntitySet FindAllSetWithLockToken(PageParameter param);

		#endregion

		#region - DataSet -

		// ## 苦竹 添加 2012.12.13 PM 16:40 ##
		/// <summary>获取所有记录集。获取大量数据时会非常慢，慎用</summary>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSet();

		// ## 苦竹 添加 2012.12.13 PM 16:40 ##
		/// <summary>查询并返回实体对象集合。
		/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
		/// </summary>
		/// <param name="whereClause">条件字句，不带Where</param>
		/// <param name="orderClause">排序字句，不带Order By</param>
		/// <param name="selects">查询列，默认null表示所有字段</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>同时查询满足条件的记录集和记录总数。</summary>
		/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
		/// <returns></returns>
		DataSet FindAllDataSet(PageParameter param);

		// ## 苦竹 添加 2012.12.13 PM 16:40 ##
		/// <summary>根据属性列表以及对应的值列表查询数据。</summary>
		/// <param name="names">属性列表</param>
		/// <param name="values">值列表</param>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSet(String[] names, Object[] values);

		// ## 苦竹 添加 2012.12.13 PM 16:40 ##
		/// <summary>根据属性以及对应的值查询数据。</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSet(String name, Object value);

		// ## 苦竹 添加 2012.12.13 PM 16:40 ##
		/// <summary>根据属性以及对应的值查询数据，带排序。</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体数组</returns>
		DataSet FindAllByNameDataSet(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows);

		// ## 苦竹 添加 2012.12.13 PM 16:40 ##
		/// <summary>查询SQL并返回实体对象数组。
		/// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
		/// </summary>
		/// <param name="sql">查询语句</param>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSet(String sql);

		#endregion

		#region - DataSet WithLockToken -

		/// <summary>获取所有记录集，执行SQL查询时使用读锁令牌。获取大量数据时会非常慢，慎用！</summary>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSetWithLockToken();

		/// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌。</summary>
		/// <param name="whereClause">条件字句，不带Where</param>
		/// <param name="orderClause">排序字句，不带Order By</param>
		/// <param name="selects">查询列，默认null表示所有字段</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>DataSet对象</returns>
		DataSet FindAllDataSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>同时查询满足条件的记录集和记录总数。</summary>
		/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
		/// <returns></returns>
		DataSet FindAllDataSetWithLockToken(PageParameter param);

		#endregion

		#endregion

		#region -- 获取查询SQL --

		// ## 苦竹 添加 2012.12.12 PM 20:20 ##
		/// <summary>获取查询SQL。主要用于构造子查询</summary>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="selects">查询列</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体集</returns>
		SelectBuilder FindSQL(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		// ## 苦竹 添加 2012.12.12 PM 20:20 ##
		/// <summary>获取查询唯一键的SQL。比如Select ID From Table</summary>
		/// <param name="whereClause"></param>
		/// <returns></returns>
		SelectBuilder FindSQLWithKey(String whereClause = null);

		#endregion

		#region -- 高级查询 --

		// ## 苦竹 添加 2012.12.12 PM 20:25 ##
		/// <summary>查询满足条件的记录集，分页、排序</summary>
		/// <param name="key">关键字</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>实体集</returns>
		IEntityList Search(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows);

		// ## 苦竹 添加 2012.12.12 PM 20:25 ##
		/// <summary>查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一</summary>
		/// <param name="key">关键字</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>记录数</returns>
		Int64 SearchCount(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>根据空格分割的关键字集合构建查询条件</summary>
		/// <param name="keys">空格分割的关键字集合</param>
		/// <param name="fields">要查询的字段，默认为空表示查询所有字符串字段</param>
		/// <param name="func">处理每一个查询关键字的回调函数</param>
		/// <returns></returns>
		WhereExpression SearchWhereByKeys(String keys, IEnumerable<FieldItem> fields, Func<String, IEnumerable<FieldItem>, WhereExpression> func);

		/// <summary>构建关键字查询条件</summary>
		/// <param name="key">关键字</param>
		/// <param name="fields">要查询的字段，默认为空表示查询所有字符串字段</param>
		/// <returns></returns>
		WhereExpression SearchWhereByKey(String key, IEnumerable<FieldItem> fields);

		#endregion

		#region -- 缓存查询 --

		/// <summary>根据属性以及对应的值，在缓存中查找单个实体</summary>
		/// <param name="name">属性名称</param>
		/// <param name="value">属性值</param>
		/// <returns></returns>
		IEntity FindWithCache(String name, Object value);

		/// <summary>查找所有缓存</summary>
		/// <returns></returns>
		IEntityList FindAllWithCache();

		/// <summary>根据属性以及对应的值，在缓存中获取所有实体对象</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <returns>实体数组</returns>
		IEntityList FindAllWithCache(String name, Object value);

		#endregion

		#region -- 取总记录数 --

		/// <summary>返回总记录数</summary>
		/// <returns></returns>
		Int64 FindCount();

		/// <summary>返回总记录数</summary>
		/// <param name="whereClause">条件，不带Where</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="selects">查询列</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>总行数</returns>
		Int64 FindCount(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>根据属性列表以及对应的值列表，返回总记录数</summary>
		/// <param name="names">属性列表</param>
		/// <param name="values">值列表</param>
		/// <returns>总行数</returns>
		Int64 FindCount(String[] names, Object[] values);

		/// <summary>根据属性以及对应的值，返回总记录数</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <returns>总行数</returns>
		Int64 FindCount(String name, Object value);

		/// <summary>根据属性以及对应的值，返回总记录数</summary>
		/// <param name="name">属性</param>
		/// <param name="value">值</param>
		/// <param name="orderClause">排序，不带Order By</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>总行数</returns>
		Int64 FindCountByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>获取总记录数，执行SQL查询时使用读锁令牌</summary>
		/// <param name="whereClause">条件，不带Where</param>
		/// <returns>返回总记录数</returns>
		Int64 FindCountWithLockToken(String whereClause);

		#endregion

		#region -- 导入导出XML/Json --

		// ## 苦竹 修改 2013.01.07 AM01:42 ##
		/// <summary>导入</summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		//[Obsolete("该成员在后续版本中将不再被支持！请使用实体访问器IEntityAccessor替代！")]
		IEntity FromXml(String xml);

		/// <summary>导入</summary>
		/// <param name="json"></param>
		/// <returns></returns>
		//[Obsolete("该成员在后续版本中将不再被支持！请使用实体访问器IEntityAccessor替代！")]
		IEntity FromJson(String json);

		#endregion

		#region -- 数据库操作 --

		/// <summary>数据操作对象。</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		DAL Dal { get; }

		//// ## 苦竹 添加 2012.12.12 PM 21:54 ##
		///// <summary>执行SQL查询，返回记录集</summary>
		///// <param name="builder">SQL语句</param>
		///// <param name="startRowIndex">开始行，0表示第一行</param>
		///// <param name="maximumRows">最大返回行数，0表示所有行</param>
		///// <returns></returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//DataSet Query(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows);

		///// <summary>查询</summary>
		///// <param name="sql">SQL语句</param>
		///// <returns>结果记录集</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//DataSet Query(String sql);

		///// <summary>查询记录数</summary>
		///// <param name="sql">SQL语句</param>
		///// <returns>记录数</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//Int64 QueryCount(String sql);

		///// <summary>查询记录数</summary>
		///// <param name="sb">查询生成器</param>
		///// <returns>记录数</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//Int64 QueryCount(SelectBuilder sb);

		///// <summary>执行</summary>
		///// <param name="sql">SQL语句</param>
		///// <returns>影响的结果</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//Int32 Execute(String sql);

		///// <summary>执行插入语句并返回新增行的自动编号</summary>
		///// <param name="sql">SQL语句</param>
		///// <returns>新增行的自动编号</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//Int64 InsertAndGetIdentity(String sql);

		///// <summary>执行</summary>
		///// <param name="sql">SQL语句</param>
		///// <param name="type">命令类型，默认SQL文本</param>
		///// <param name="ps">命令参数</param>
		///// <returns>影响的结果</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//Int32 Execute(String sql, CommandType type = CommandType.Text, params DbParameter[] ps);

		///// <summary>执行插入语句并返回新增行的自动编号</summary>
		///// <param name="sql">SQL语句</param>
		///// <param name="type">命令类型，默认SQL文本</param>
		///// <param name="ps">命令参数</param>
		///// <returns>新增行的自动编号</returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//Int64 InsertAndGetIdentity(String sql, CommandType type = CommandType.Text, params DbParameter[] ps);

		// ## 苦竹 添加 2012.12.12 PM 21:37 ##
		/// <summary>更新一批实体数据</summary>
		/// <param name="setClause">要更新的项和数据</param>
		/// <param name="whereClause">限制条件</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		Int32 AdvancedUpdate(String setClause, String whereClause, Boolean useTransition);

		/// <summary>更新一批实体数据</summary>
		/// <param name="setNames">更新属性列表</param>
		/// <param name="setValues">更新值列表</param>
		/// <param name="whereClause">限制条件</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns>返回受影响的行数</returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String whereClause, Boolean useTransition);

		// ## 苦竹 添加 2012.12.12 PM 21:37 ##
		/// <summary>更新一批实体数据</summary>
		/// <param name="setNames">更新属性列表</param>
		/// <param name="setValues">更新值列表</param>
		/// <param name="whereNames">条件属性列表</param>
		/// <param name="whereValues">条件值列表</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns>返回受影响的行数</returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String[] whereNames, Object[] whereValues, Boolean useTransition);

		// ## 苦竹 添加 2012.12.12 PM 21:37 ##
		/// <summary>从数据库中删除指定条件的实体对象。</summary>
		/// <param name="whereClause">限制条件</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		Int32 AdvancedDelete(String whereClause, Boolean useTransition);

		// ## 苦竹 添加 2012.12.12 PM 21:37 ##
		/// <summary>从数据库中删除指定属性列表和值列表所限定的实体对象。</summary>
		/// <param name="whereNames">条件属性列表</param>
		/// <param name="whereValues">条件值列表</param>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		Int32 AdvancedDelete(String[] whereNames, Object[] whereValues, Boolean useTransition);

		/// <summary>清除当前实体所在数据表所有数据，并重置标识列为该列的种子。</summary>
		/// <returns></returns>
		Int32 Truncate();

		#endregion

		#region -- 事务 --

		/// <summary>开始事务</summary>
		/// <returns></returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		Int32 BeginTransaction();

		/// <summary>提交事务</summary>
		/// <returns></returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		Int32 Commit();

		/// <summary>回滚事务</summary>
		/// <returns></returns>
		//[Obsolete("=>Session")]
		//[EditorBrowsable(EditorBrowsableState.Never)]
		Int32 Rollback();

		// ## 苦竹 添加 2014.01.10 PM 16:40 ##
		/// <summary>创建事务</summary>
		EntityTransaction CreateTrans();

		#endregion

		#region -- 参数化 --

		/// <summary>创建参数</summary>
		/// <returns></returns>
		[Obsolete("=>Session")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		DbParameter CreateParameter();

		/// <summary>格式化参数名</summary>
		/// <param name="name">名称</param>
		/// <returns></returns>
		[Obsolete("=>Session")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		String FormatParameterName(String name);

		#endregion

		#region -- 辅助方法 --

		/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
		IQuoter Quoter { get; }

		/// <summary>转义字段名称</summary>
		/// <param name="names">字段名称集合</param>
		/// <returns>返回转义后的字段名称</returns>
		String QuoteColumnNames(IEnumerable<String> names);

		/// <summary>取得一个值的Sql值。
		/// 当这个值是字符串类型时，会在该值前后加单引号；
		/// </summary>
		/// <param name="fieldName">字段</param>
		/// <param name="value">对象</param>
		/// <returns>Sql值的字符串形式</returns>
		String QuoteValue(String fieldName, Object value);

		/// <summary>格式化数据为SQL数据</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		String QuoteValue(FieldItem field, Object value);

		/// <summary>
		/// 根据属性列表和值列表，构造查询条件。
		/// 例如构造多主键限制查询条件。
		/// </summary>
		/// <param name="names">属性列表</param>
		/// <param name="values">值列表</param>
		/// <param name="action">联合方式</param>
		/// <returns>条件子串</returns>
		String MakeCondition(String[] names, Object[] values, String action);

		/// <summary>构造条件</summary>
		/// <param name="name">名称</param>
		/// <param name="value">值</param>
		/// <param name="action">大于小于等符号</param>
		/// <returns></returns>
		String MakeCondition(String name, Object value, String action);

		/// <summary>构造条件</summary>
		/// <param name="field">名称</param>
		/// <param name="value">值</param>
		/// <param name="action">大于小于等符号</param>
		/// <returns></returns>
		String MakeCondition(FieldItem field, Object value, String action);

		#endregion

		#region -- 一些设置 --

		/// <summary>是否允许向自增列插入数据。为免冲突，仅本线程有效</summary>
		Boolean AllowInsertIdentity { get; set; }

		///// <summary>自动设置Guid的字段。对实体类有效，可在实体类类型构造函数里面设置</summary>
		//FieldItem AutoSetGuidField { get; set; }

		/// <summary>默认累加字段</summary>
		ICollection<String> AdditionalFields { get; }

		#endregion

		#region -- 分表分库 --

		/// <summary>数据分片键值是否采用实体自身指定字段的值</summary>
		Boolean UsingSelfShardingKeyField { get; set; }

		/// <summary>默认数据分片键字段名称</summary>
		String ShardingKeyFieldName { get; set; }

		/// <summary>实体数据分片提供者工厂</summary>
		IShardingProviderFactory ShardingProviderFactory { get; }

		///// <summary>在分库上执行操作，自动还原</summary>
		///// <param name="connName">连接名</param>
		///// <param name="tableName">表名</param>
		///// <param name="func"></param>
		///// <returns></returns>
		//Object ProcessWithSharding(String connName, String tableName, Func<Object> func);

		///// <summary>创建分库会话，using结束时自动还原</summary>
		///// <param name="connName">连接名</param>
		///// <param name="tableName">表名</param>
		///// <returns></returns>
		//IDisposable CreateShard(String connName, String tableName);

		#endregion

		#region -- 读写锁令牌 --

		/// <summary>创建实体会话读锁令牌</summary>
		/// <returns></returns>
		IDisposable CreateReadLockToken();

		/// <summary>创建实体会话写锁令牌</summary>
		/// <returns></returns>
		IDisposable CreateWriteLockToken();

		#endregion
	}
}