/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CuteAnt.OrmLite.Cache;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	partial class Entity<TEntity>
	{
		/// <summary>默认的实体操作者</summary>
		internal class EntityOperate : IEntityOperate
		{
			#region -- 主要属性 --

			/// <summary>实体类型</summary>
			public Type EntityType
			{
				get { return typeof(TEntity); }
			}

			/// <summary>实体会话</summary>
			public IEntitySession Session
			{
				get { return Meta.Session; }
			}

			#endregion

			#region -- 属性 --

			private IEntity _Default;

			/// <summary>默认实体</summary>
			public IEntity Default
			{
				get { return _Default ?? (_Default = new TEntity()); }
				set { _Default = value; }
			}

			/// <summary>数据表元数据</summary>
			public TableItem Table
			{
				get { return Meta.Table; }
			}


			// ## 苦竹 修改 ##
			///// <summary>所有数据属性</summary>
			//public FieldItem[] AllFields
			//{
			//	get { return Meta.AllFields; }
			//}

			///// <summary>所有绑定到数据表的属性</summary>
			//public FieldItem[] Fields
			//{
			//	get { return Meta.Fields; }
			//}

			///// <summary>字段名列表</summary>
			//public IList<String> FieldNames
			//{
			//	get { return Meta.FieldNames; }
			//}
			/// <summary>所有数据属性</summary>
			public IList<FieldItem> AllFields
			{
				get { return Meta.AllFields; }
			}

			/// <summary>所有绑定到数据表的属性</summary>
			public IList<FieldItem> Fields
			{
				get { return Meta.Fields; }
			}

			/// <summary>所有绑定到数据表的SQL语句转义字段名称</summary>
			public IEnumerable<String> QuotedColumnNames
			{
				get { return Meta.QuotedColumnNames; }
			}

			/// <summary>字段名集合，不区分大小写的哈希表存储，外部不要修改元素数据</summary>
			public ISet<String> FieldNames
			{
				get { return Meta.FieldNames; }
			}

			/// <summary>唯一键，返回第一个标识列或者唯一的主键</summary>
			public FieldItem Unique
			{
				get { return Meta.Unique; }
			}

			/// <summary>主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
			public FieldItem Master
			{
				get { return Meta.Master; }
			}

			/// <summary>连接名</summary>
			public String ConnName
			{
				get { return Meta.ConnName; }
				set { Meta.ConnName = value; }
			}

			/// <summary>表名</summary>
			public String TableName
			{
				get { return Meta.TableName; }
				set { Meta.TableName = value; }
			}

			/// <summary>已格式化的表名，带有中括号等</summary>
			public String FormatedTableName
			{
				get { return Session.FormatedTableName; }
			}

			/// <summary>实体缓存</summary>
			public IEntityCache Cache
			{
				get { return Session.Cache; }
			}

			/// <summary>单对象实体缓存</summary>
			public ISingleEntityCache SingleCache
			{
				get { return Session.SingleCache; }
			}

			/// <summary>总记录数</summary>
			public Int64 Count
			{
				get { return Session.Count; }
			}

			#endregion

			#region -- 创建实体、填充数据 --

			/// <summary>创建一个实体对象</summary>
			/// <param name="forEdit">是否为了编辑而创建，如果是，可以再次做一些相关的初始化工作</param>
			/// <returns></returns>
			public IEntity Create(Boolean forEdit = false)
			{
				return (Default as TEntity).CreateInstance(forEdit) as TEntity;
			}

			/// <summary>加载记录集</summary>
			/// <param name="ds">记录集</param>
			/// <returns>实体数组</returns>
			[Obsolete("请使用LoadDataToList")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public IEntityList LoadData(DataSet ds)
			{
				return Entity<TEntity>.LoadDataToList(ds);
			}

			/// <summary>加载记录集</summary>
			/// <param name="ds">记录集</param>
			/// <returns>实体集合</returns>
			public IEntityList LoadDataToList(DataSet ds)
			{
				return Entity<TEntity>.LoadDataToList(ds);
			}

			/// <summary>加载记录哈希集</summary>
			/// <param name="ds">记录集</param>
			/// <returns>实体哈希集合</returns>
			public IEntitySet LoadDataToSet(DataSet ds)
			{
				return Entity<TEntity>.LoadDataToSet(ds);
			}

			#endregion

			#region -- 批量操作 --

			#region - DeleteAll -

			/// <summary>根据条件删除实体记录，此操作跨越缓存，使用事务保护</summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="batchSize">每次删除记录数</param>
			public void DeleteAll(String whereClause, Int32 batchSize)
			{
				Entity<TEntity>.DeleteAll(whereClause, batchSize);
			}

			/// <summary>根据条件删除实体记录，使用读写锁令牌，缩小事务范围，删除时不再确保数据一致性，慎用！！！
			/// <para>如果删除操作不带业务，可直接使用静态方法 Delete(String whereClause)</para>
			/// </summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="batchSize">每次删除记录数</param>
			public void DeleteAllWithLockToken(String whereClause, Int32 batchSize)
			{
				Entity<TEntity>.DeleteAllWithLockToken(whereClause, batchSize);
			}

			#endregion

			#region - ProcessAll与Entity<TEntity>类的ProcessAll方法代码同步 -

			/// <summary>批量处理实体记录，此操作跨越缓存</summary>
			/// <param name="action">处理实体记录集方法</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <param name="batchSize">每次处理记录数</param>
			/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
			public void ProcessAll(Action<IEntityList> action,
				Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
			{
				ProcessAll(action, null, null, null, useTransition, batchSize, maxCount);
			}

			/// <summary>批量处理实体记录，此操作跨越缓存</summary>
			/// <param name="action">处理实体记录集方法</param>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <param name="batchSize">每次处理记录数</param>
			/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
			public void ProcessAll(Action<IEntityList> action, String whereClause,
				Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
			{
				ProcessAll(action, whereClause, null, null, useTransition, batchSize, maxCount);
			}

			/// <summary>批量处理实体记录，此操作跨越缓存，使用事务保护</summary>
			/// <param name="action">实体记录操作方法</param>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <param name="batchSize">每次处理记录数</param>
			/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
			public void ProcessAll(Action<IEntityList> action, String whereClause, String orderClause, String selects,
				Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
			{
				var count = Entity<TEntity>.FindCount(whereClause, orderClause, selects, 0L, 0);
				var total = maxCount <= 0 ? count : Math.Min(maxCount, count);
				var index = 0L;
				while (true)
				{
					var size = (Int32)Math.Min(batchSize, total - index);
					if (size <= 0) { break; }

					var list = Entity<TEntity>.FindAll(whereClause, orderClause, selects, index, size);
					if ((list == null) || (list.Count < 1)) { break; }
					index += list.Count;

					if (useTransition)
					{
						using (var trans = new EntityTransaction<TEntity>())
						{
							action(list);

							trans.Commit();
						}
					}
					else
					{
						action(list);
					}
				}
			}

			#endregion

			#region - ProcessAllWithLockToken与Entity<TEntity>类的ProcessAllWithLockToken方法代码同步 -

			/// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
			/// <param name="action">处理实体记录集方法</param>
			/// <param name="actionLockType">操作方法锁令牌方式</param>
			/// <param name="batchSize">每次处理记录数</param>
			/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
			public void ProcessAllWithLockToken(Action<IEntityList> action, ActionLockTokenType actionLockType,
				Int32 batchSize = 500, Int32 maxCount = 0)
			{
				ProcessAllWithLockToken(action, actionLockType, null, null, null, batchSize, maxCount);
			}

			/// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
			/// <param name="action">处理实体记录集方法</param>
			/// <param name="actionLockType">操作方法锁令牌方式</param>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="batchSize">每次处理记录数</param>
			/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
			public void ProcessAllWithLockToken(Action<IEntityList> action, ActionLockTokenType actionLockType,
				String whereClause, Int32 batchSize = 500, Int32 maxCount = 0)
			{
				ProcessAllWithLockToken(action, actionLockType, whereClause, null, null, batchSize, maxCount);
			}

			/// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
			/// <param name="action">处理实体记录集方法</param>
			/// <param name="actionLockType">操作方法锁令牌方式</param>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="batchSize">每次处理记录数</param>
			/// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
			public void ProcessAllWithLockToken(Action<IEntityList> action, ActionLockTokenType actionLockType,
				String whereClause, String orderClause, String selects, Int32 batchSize = 500, Int32 maxCount = 0)
			{
				var session = Meta.Session;

				var count = Entity<TEntity>.FindCountWithLockToken(whereClause);
				var total = maxCount <= 0 ? count : Math.Min(maxCount, count);
				var index = 0L;
				while (true)
				{
					var size = (Int32)Math.Min(batchSize, total - index);
					if (size <= 0) { break; }

					var list = Entity<TEntity>.FindAllWithLockToken(whereClause, orderClause, selects, index, size);
					if ((list == null) || (list.Count < 1)) { break; }
					index += list.Count;

					switch (actionLockType)
					{
						case ActionLockTokenType.UseReadLockToken:
							using (var token = session.CreateReadLockToken())
							{
								action(list);
							}
							break;
						case ActionLockTokenType.UseWriteLockToken:
							using (var token = session.CreateWriteLockToken())
							{
								action(list);
							}
							break;
						case ActionLockTokenType.None:
						default:
							action(list);
							break;
					}
				}
			}

			#endregion

			#region - TransformAll -

			/// <summary>实体数据迁移，调用此方法前请确定进行了数据分片配置。</summary>
			/// <param name="entities">实体数据列表</param>
			/// <param name="keepIdentity">是否允许向自增列插入数据</param>
			/// <param name="batchSize">单条SQL语句插入数据数</param>
			public void TransformAll(IEntityList entities, Boolean keepIdentity = true, Int32 batchSize = 10)
			{
				var list = entities as EntityList<TEntity>;
				if (list == null) { return; }
				Entity<TEntity>.TransformAll(list, keepIdentity, batchSize);
			}

			/// <summary>实体数据迁移，调用此方法前请确定进行了数据分片配置。</summary>
			/// <param name="dt">实体数据表</param>
			/// <param name="keepIdentity">是否允许向自增列插入数据</param>
			/// <remarks>SQL Server 2008或2008以上版本使用表值参数（Table-valued parameters）进行批量插入会更快，但需要为每个表单独建立TVP。</remarks>
			public void TransformAll(DataTable dt, Boolean keepIdentity = true)
			{
				Entity<TEntity>.TransformAll(dt, keepIdentity);
			}

			#endregion

			#endregion

			#region -- 查找单个实体 --

			/// <summary>根据属性以及对应的值，查找单个实体</summary>
			/// <param name="name">名称</param>
			/// <param name="value">数值</param>
			/// <returns></returns>
			public IEntity Find(String name, Object value)
			{
				return Entity<TEntity>.Find(name, value);
			}

			// ## 苦竹 添加 2012.12.12 PM 19:45 ##
			/// <summary>根据属性列表以及对应的值列表，查找单个实体</summary>
			/// <param name="names">属性名称集合</param>
			/// <param name="values">属性值集合</param>
			/// <returns></returns>
			public IEntity Find(String[] names, Object[] values)
			{
				return Entity<TEntity>.Find(names, values);
			}

			/// <summary>根据条件查找单个实体</summary>
			/// <param name="whereClause"></param>
			/// <returns></returns>
			public IEntity Find(String whereClause)
			{
				return Entity<TEntity>.Find(whereClause);
			}

			/// <summary>根据主键查找单个实体</summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public IEntity FindByKey(Object key)
			{
				return Entity<TEntity>.FindByKey(key);
			}

			/// <summary>根据主键查询一个实体对象用于表单编辑</summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public IEntity FindByKeyForEdit(Object key)
			{
				return Entity<TEntity>.FindByKeyForEdit(key);
			}

			#endregion

			#region -- 静态查询 --

			#region - IEntityList -

			/// <summary>获取所有实体对象。获取大量数据时会非常慢，慎用</summary>
			/// <returns>实体数组</returns>
			public IEntityList FindAll()
			{
				return Entity<TEntity>.FindAll();
			}

			/// <summary>查询并返回实体对象集合。
			/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
			/// </summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAll(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAll(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>同时查询满足条件的记录集和记录总数。</summary>
			/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
			/// <returns></returns>
			public IEntityList FindAll(PageParameter param)
			{
				return Entity<TEntity>.FindAll(param);
			}

			/// <summary>根据属性列表以及对应的值列表，获取所有实体对象</summary>
			/// <param name="names">属性列表</param>
			/// <param name="values">值列表</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAll(String[] names, Object[] values)
			{
				return Entity<TEntity>.FindAll(names, values);
			}

			/// <summary>根据属性以及对应的值，获取所有实体对象</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAll(String name, Object value)
			{
				return Entity<TEntity>.FindAll(name, value);
			}

			/// <summary>根据属性以及对应的值，获取所有实体对象</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAllByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllByName(name, value, orderClause, startRowIndex, maximumRows);
			}

			// ## 苦竹 添加 2012.12.12 PM 19:40 ##
			/// <summary>查询SQL并返回实体对象数组。
			/// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
			/// </summary>
			/// <param name="sql">查询语句</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAll(String sql)
			{
				return Entity<TEntity>.FindAll(sql);
			}

			#endregion

			#region - IEntityList WithLockToken -

			/// <summary>获取所有实体对象，执行SQL查询时使用读锁令牌</summary>
			/// <returns>实体数组</returns>
			public IEntityList FindAllWithLockToken()
			{
				return Entity<TEntity>.FindAllWithLockToken();
			}

			/// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌
			/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
			/// </summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAllWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllWithLockToken(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>同时查询满足条件的记录集和记录总数。</summary>
			/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
			/// <returns></returns>
			public IEntityList FindAllWithLockToken(PageParameter param)
			{
				return Entity<TEntity>.FindAllWithLockToken(param);
			}

			#endregion

			#region - IEntitySet -

			/// <summary>获取所有实体对象哈希集合。获取大量数据时会非常慢，慎用</summary>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSet()
			{
				return Entity<TEntity>.FindAllSet();
			}

			/// <summary>查询并返回实体对象哈希集合。
			/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
			/// </summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllSet(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>同时查询满足条件的记录集和记录总数。</summary>
			/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
			/// <returns></returns>
			public IEntitySet FindAllSet(PageParameter param)
			{
				return Entity<TEntity>.FindAllSet(param);
			}

			/// <summary>根据属性列表以及对应的值列表，获取所有实体对象哈希集合</summary>
			/// <param name="names">属性列表</param>
			/// <param name="values">值列表</param>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSet(String[] names, Object[] values)
			{
				return Entity<TEntity>.FindAllSet(names, values);
			}

			/// <summary>根据属性以及对应的值，获取所有实体对象哈希集合</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSet(String name, Object value)
			{
				return Entity<TEntity>.FindAllSet(name, value);
			}

			/// <summary>根据属性以及对应的值，获取所有实体对象哈希集合</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSetByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllSetByName(name, value, orderClause, startRowIndex, maximumRows);
			}

			/// <summary>查询SQL并返回实体对象哈希集合。
			/// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
			/// </summary>
			/// <param name="sql">查询语句</param>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSet(String sql)
			{
				return Entity<TEntity>.FindAllSet(sql);
			}

			#endregion

			#region - IEntitySet WithLockToken -

			/// <summary>获取所有实体对象哈希集合，执行SQL查询时使用读锁令牌</summary>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSetWithLockToken()
			{
				return Entity<TEntity>.FindAllSetWithLockToken();
			}

			/// <summary>查询并返回实体对象哈希集合，执行SQL查询时使用读锁令牌
			/// 表名以及所有字段名，请使用类名以及字段对应的属性名，方法内转换为表名和列名
			/// </summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public IEntitySet FindAllSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllSetWithLockToken(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>同时查询满足条件的记录集和记录总数。</summary>
			/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
			/// <returns></returns>
			public IEntitySet FindAllSetWithLockToken(PageParameter param)
			{
				return Entity<TEntity>.FindAllSetWithLockToken(param);
			}

			#endregion

			#region - DataSet -

			// ## 苦竹 添加 2012.12.13 PM 16:40 ##
			/// <summary>获取所有记录集。获取大量数据时会非常慢，慎用</summary>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSet()
			{
				return Entity<TEntity>.FindAllDataSet(null, null, null, 0L, 0);
			}

			// ## 苦竹 添加 2012.12.13 PM 16:40 ##
			/// <summary>查询并返回实体对象集合。
			/// 最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，你就明白各参数的意思了。
			/// </summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllDataSet(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>同时查询满足条件的记录集和记录总数。</summary>
			/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
			/// <returns></returns>
			public DataSet FindAllDataSet(PageParameter param)
			{
				return Entity<TEntity>.FindAllDataSet(param);
			}

			// ## 苦竹 添加 2012.12.13 PM 16:40 ##
			/// <summary>根据属性列表以及对应的值列表，获取所有实体对象</summary>
			/// <param name="names">属性列表</param>
			/// <param name="values">值列表</param>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSet(String[] names, Object[] values)
			{
				return Entity<TEntity>.FindAllDataSet(names, values);
			}

			// ## 苦竹 添加 2012.12.13 PM 16:40 ##
			/// <summary>根据属性以及对应的值，获取所有实体对象</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSet(String name, Object value)
			{
				return Entity<TEntity>.FindAllDataSet(name, value);
			}

			// ## 苦竹 添加 2012.12.13 PM 16:40 ##
			/// <summary>根据属性以及对应的值，获取所有实体对象</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体数组</returns>
			public DataSet FindAllByNameDataSet(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllByNameDataSet(name, value, orderClause, startRowIndex, maximumRows);
			}

			// ## 苦竹 添加 2012.12.13 PM 16:40 ##
			/// <summary>查询SQL并返回实体对象数组。
			/// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
			/// </summary>
			/// <param name="sql">查询语句</param>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSet(String sql)
			{
				return Entity<TEntity>.FindAllDataSet(sql);
			}

			#endregion

			#region - DataSet WithLockToken -

			/// <summary>获取所有记录集，执行SQL查询时使用读锁令牌。获取大量数据时会非常慢，慎用</summary>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSetWithLockToken()
			{
				return Entity<TEntity>.FindAllDataSetWithLockToken();
			}

			/// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌</summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>DataSet对象</returns>
			public DataSet FindAllDataSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindAllDataSetWithLockToken(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>同时查询满足条件的记录集和记录总数。</summary>
			/// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
			/// <returns></returns>
			public DataSet FindAllDataSetWithLockToken(PageParameter param)
			{
				return Entity<TEntity>.FindAllDataSetWithLockToken(param);
			}

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
			public SelectBuilder FindSQL(String whereClause, String orderClause, String selects, Int64 startRowIndex = 0, Int32 maximumRows = 0)
			{
				return Entity<TEntity>.FindSQL(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			// ## 苦竹 添加 2012.12.12 PM 20:20 ##
			/// <summary>获取查询唯一键的SQL。比如Select ID From Table</summary>
			/// <param name="whereClause"></param>
			/// <returns></returns>
			public SelectBuilder FindSQLWithKey(String whereClause = null)
			{
				return Entity<TEntity>.FindSQLWithKey(whereClause);
			}

			#endregion

			#region -- 高级查询 --

			// ## 苦竹 添加 2012.12.12 PM 20:25 ##
			/// <summary>查询满足条件的记录集，分页、排序</summary>
			/// <param name="key">关键字</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>实体集</returns>
			public IEntityList Search(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.Search(key, orderClause, startRowIndex, maximumRows);
			}

			// ## 苦竹 添加 2012.12.12 PM 20:25 ##
			/// <summary>查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一</summary>
			/// <param name="key">关键字</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>记录数</returns>
			public Int64 SearchCount(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.SearchCount(key, orderClause, startRowIndex, maximumRows);
			}

			/// <summary>根据空格分割的关键字集合构建查询条件</summary>
			/// <param name="keys">空格分割的关键字集合</param>
			/// <param name="fields">要查询的字段，默认为空表示查询所有字符串字段</param>
			/// <param name="func">处理每一个查询关键字的回调函数</param>
			/// <returns></returns>
			public WhereExpression SearchWhereByKeys(String keys, IEnumerable<FieldItem> fields, Func<String, IEnumerable<FieldItem>, WhereExpression> func)
			{
				return Entity<TEntity>.SearchWhereByKeys(keys, fields, func);
			}

			/// <summary>构建关键字查询条件</summary>
			/// <param name="key">关键字</param>
			/// <param name="fields">要查询的字段，默认为空表示查询所有字符串字段</param>
			/// <returns></returns>
			public WhereExpression SearchWhereByKey(String key, IEnumerable<FieldItem> fields)
			{
				return Entity<TEntity>.SearchWhereByKeys(key, fields);
			}

			#endregion

			#region -- 缓存查询 --

			/// <summary>根据属性以及对应的值，在缓存中查找单个实体</summary>
			/// <param name="name">属性名称</param>
			/// <param name="value">属性值</param>
			/// <returns></returns>
			public IEntity FindWithCache(String name, Object value)
			{
				// ## 苦竹 修改 2012.12.12 PM 23.55 ##
				//return Entity<TEntity>.FindWithCache(name, value);
				return Session.Cache.Entities.Find(name, value);
			}

			/// <summary>查找所有缓存</summary>
			/// <returns></returns>
			public IEntityList FindAllWithCache()
			{
				// ## 苦竹 修改 2012.12.12 PM 23.55 ##
				//return Entity<TEntity>.FindAllWithCache();
				return Session.Cache.Entities;
			}

			/// <summary>根据属性以及对应的值，在缓存中获取所有实体对象</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <returns>实体数组</returns>
			public IEntityList FindAllWithCache(String name, Object value)
			{
				// ## 苦竹 修改 2012.12.12 PM 23.55 ##
				//return Entity<TEntity>.FindAllWithCache(name, value);
				return Session.Cache.Entities.FindAll(name, value);
			}

			#endregion

			#region -- 取总记录数 --

			/// <summary>返回总记录数</summary>
			/// <returns></returns>
			public Int64 FindCount()
			{
				return Entity<TEntity>.FindCount();
			}

			/// <summary>返回总记录数</summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="selects">查询列</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>总行数</returns>
			public Int64 FindCount(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindCount(whereClause, orderClause, selects, startRowIndex, maximumRows);
			}

			/// <summary>根据属性列表以及对应的值列表，返回总记录数</summary>
			/// <param name="names">属性列表</param>
			/// <param name="values">值列表</param>
			/// <returns>总行数</returns>
			public Int64 FindCount(String[] names, Object[] values)
			{
				return Entity<TEntity>.FindCount(names, values);
			}

			/// <summary>根据属性以及对应的值，返回总记录数</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <returns>总行数</returns>
			public Int64 FindCount(String name, Object value)
			{
				return Entity<TEntity>.FindCount(name, value);
			}

			/// <summary>根据属性以及对应的值，返回总记录数</summary>
			/// <param name="name">属性</param>
			/// <param name="value">值</param>
			/// <param name="orderClause">排序，不带Order By</param>
			/// <param name="startRowIndex">开始行，0表示第一行</param>
			/// <param name="maximumRows">最大返回行数，0表示所有行</param>
			/// <returns>总行数</returns>
			public Int64 FindCountByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
			{
				return Entity<TEntity>.FindCountByName(name, value, orderClause, startRowIndex, maximumRows);
			}

			/// <summary>获取总记录数，执行SQL查询时使用读锁令牌</summary>
			/// <param name="whereClause">条件，不带Where</param>
			/// <returns>返回总记录数</returns>
			public Int64 FindCountWithLockToken(String whereClause = null)
			{
				return Entity<TEntity>.FindCountWithLockToken(whereClause);
			}

			#endregion

			#region -- 导入导出XML/Json --

			/// <summary>导入</summary>
			/// <param name="xml"></param>
			/// <returns></returns>
			//[Obsolete("该成员在后续版本中将不再被支持！请使用实体访问器IEntityAccessor替代！")]
			public IEntity FromXml(String xml)
			{
				return Entity<TEntity>.FromXml(xml);
			}

			/// <summary>导入</summary>
			/// <param name="json"></param>
			/// <returns></returns>
			//[Obsolete("该成员在后续版本中将不再被支持！请使用实体访问器IEntityAccessor替代！")]
			public IEntity FromJson(String json)
			{
				return Entity<TEntity>.FromJson(json);
			}

			#endregion

			#region -- 数据库操作 --

			/// <summary>数据操作对象。</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public DAL Dal
			{
				get { return DAL.Create(ConnName); }
			}

			//// ## 苦竹 添加 2012.12.12 PM 21:54 ##
			///// <summary>执行SQL查询，返回记录集</summary>
			///// <param name="builder">SQL语句</param>
			///// <param name="startRowIndex">开始行，0表示第一行</param>
			///// <param name="maximumRows">最大返回行数，0表示所有行</param>
			///// <returns></returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public DataSet Query(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
			//{
			//	return Session.Query(builder, startRowIndex, maximumRows);
			//}

			///// <summary>查询</summary>
			///// <param name="sql">SQL语句</param>
			///// <returns>结果记录集</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public DataSet Query(String sql)
			//{
			//	return Session.Query(sql);
			//}

			///// <summary>查询记录数</summary>
			///// <param name="sql">SQL语句</param>
			///// <returns>记录数</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public Int64 QueryCount(String sql)
			//{
			//	return Session.QueryCount(sql);
			//}

			///// <summary>查询记录数</summary>
			///// <param name="sb">查询生成器</param>
			///// <returns>记录数</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public Int64 QueryCount(SelectBuilder sb)
			//{
			//	return Session.QueryCount(sb);
			//}

			///// <summary>执行</summary>
			///// <param name="sql">SQL语句</param>
			///// <returns>影响的结果</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public Int32 Execute(String sql)
			//{
			//	return Session.Execute(sql);
			//}

			///// <summary>执行插入语句并返回新增行的自动编号</summary>
			///// <param name="sql">SQL语句</param>
			///// <returns>新增行的自动编号</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public Int64 InsertAndGetIdentity(String sql)
			//{
			//	return Session.InsertAndGetIdentity(sql);
			//}

			///// <summary>执行</summary>
			///// <param name="sql">SQL语句</param>
			///// <param name="type">命令类型，默认SQL文本</param>
			///// <param name="ps">命令参数</param>
			///// <returns>影响的结果</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public Int32 Execute(String sql, CommandType type, DbParameter[] ps)
			//{
			//	return Session.Execute(sql, type, ps);
			//}

			///// <summary>执行插入语句并返回新增行的自动编号</summary>
			///// <param name="sql">SQL语句</param>
			///// <param name="type">命令类型，默认SQL文本</param>
			///// <param name="ps">命令参数</param>
			///// <returns>新增行的自动编号</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			//public Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
			//{
			//	return Session.InsertAndGetIdentity(sql, type, ps);
			//}

			// ## 苦竹 添加 2012.12.12 PM 21:37 ##
			/// <summary>更新一批指定条件的实体数据，慎用！！！
			/// <para>此操作跨越缓存，如果实体开启了实体缓存或单对象缓存，缓存数据将不会同步更新</para></summary>
			/// <param name="setClause">要更新的项和数据</param>
			/// <param name="whereClause">限制条件</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <returns></returns>
			[EditorBrowsable(EditorBrowsableState.Advanced)]
			public Int32 AdvancedUpdate(String setClause, String whereClause, Boolean useTransition)
			{
				return Entity<TEntity>.AdvancedUpdate(setClause, whereClause, useTransition);
			}

			/// <summary>更新一批实体数据</summary>
			/// <param name="setNames">更新属性列表</param>
			/// <param name="setValues">更新值列表</param>
			/// <param name="whereClause">限制条件</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <returns>返回受影响的行数</returns>
			//[EditorBrowsable(EditorBrowsableState.Advanced)]
			public Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String whereClause, Boolean useTransition)
			{
				return Entity<TEntity>.AdvancedUpdate(setNames, setValues, whereClause, useTransition);
			}

			// ## 苦竹 添加 2012.12.12 PM 21:37 ##
			/// <summary>更新一批实体数据</summary>
			/// <param name="setNames">更新属性列表</param>
			/// <param name="setValues">更新值列表</param>
			/// <param name="whereNames">条件属性列表</param>
			/// <param name="whereValues">条件值列表</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <returns>返回受影响的行数</returns>
			//[EditorBrowsable(EditorBrowsableState.Advanced)]
			public Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String[] whereNames, Object[] whereValues, Boolean useTransition)
			{
				return Entity<TEntity>.AdvancedUpdate(setNames, setValues, whereNames, whereValues, useTransition);
			}

			// ## 苦竹 添加 2012.12.12 PM 21:37 ##
			/// <summary>从数据库中删除指定条件的实体对象。</summary>
			/// <param name="whereClause">限制条件</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <returns></returns>
			//[EditorBrowsable(EditorBrowsableState.Advanced)]
			public Int32 AdvancedDelete(String whereClause, Boolean useTransition)
			{
				return Entity<TEntity>.AdvancedDelete(whereClause, useTransition);
			}

			// ## 苦竹 添加 2012.12.12 PM 21:37 ##
			/// <summary>从数据库中删除指定属性列表和值列表所限定的实体对象。</summary>
			/// <param name="whereNames">条件属性列表</param>
			/// <param name="whereValues">条件值列表</param>
			/// <param name="useTransition">是否使用事务保护</param>
			/// <returns></returns>
			//[EditorBrowsable(EditorBrowsableState.Advanced)]
			public Int32 AdvancedDelete(String[] whereNames, Object[] whereValues, Boolean useTransition)
			{
				return Entity<TEntity>.AdvancedDelete(whereNames, whereValues, useTransition);
			}

			/// <summary>清除当前实体所在数据表所有数据，并重置标识列为该列的种子。</summary>
			/// <returns></returns>
			public Int32 Truncate()
			{
				return Entity<TEntity>.Truncate();
			}

			#endregion

			#region -- 事务 --

			/// <summary>开始事务</summary>
			/// <returns></returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public Int32 BeginTransaction()
			{
				return Session.BeginTrans();
			}

			/// <summary>提交事务</summary>
			/// <returns></returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public Int32 Commit()
			{
				return Session.Commit();
			}

			/// <summary>回滚事务</summary>
			/// <returns></returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public Int32 Rollback()
			{
				return Session.Rollback();
			}

			/// <summary>创建事务</summary>
			public EntityTransaction CreateTrans()
			{
				return new EntityTransaction<TEntity>();
			}

			#endregion

			#region -- 参数化 --

			/// <summary>创建参数</summary>
			/// <returns></returns>
			[Obsolete("=>Session")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public DbParameter CreateParameter()
			{
				return Session.CreateParameter();
			}

			/// <summary>格式化参数名</summary>
			/// <param name="name"></param>
			/// <returns></returns>
			[Obsolete("=>Session")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public String FormatParameterName(String name)
			{
				return Session.FormatParameterName(name);
			}

			#endregion

			#region -- 辅助方法 --

			/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
			public IQuoter Quoter { get { return Meta.Quoter; } }

			/// <summary>转义字段名称</summary>
			/// <param name="names">字段名称集合</param>
			/// <returns>返回转义后的字段名称</returns>
			public String QuoteColumnNames(IEnumerable<String> names)
			{
				return Meta.QuoteColumnNames(names);
			}

			/// <summary>取得一个值的Sql值。
			/// 当这个值是字符串类型时，会在该值前后加单引号；
			/// </summary>
			/// <param name="fieldName">字段名称</param>
			/// <param name="value">对象</param>
			/// <returns>Sql值的字符串形式</returns>
			public String QuoteValue(String fieldName, Object value)
			{
				return Meta.QuoteValue(fieldName, value);
			}

			/// <summary>格式化数据为SQL数据</summary>
			/// <param name="field">字段</param>
			/// <param name="value">数值</param>
			/// <returns></returns>
			public String QuoteValue(FieldItem field, Object value)
			{
				return Meta.QuoteValue(field, value);
			}

			/// <summary>
			/// 根据属性列表和值列表，构造查询条件。
			/// 例如构造多主键限制查询条件。
			/// </summary>
			/// <param name="names">属性列表</param>
			/// <param name="values">值列表</param>
			/// <param name="action">联合方式</param>
			/// <returns>条件子串</returns>
			public String MakeCondition(String[] names, Object[] values, String action)
			{
				return Entity<TEntity>.MakeCondition(names, values, action);
			}

			/// <summary>构造条件</summary>
			/// <param name="name">名称</param>
			/// <param name="value">值</param>
			/// <param name="action">大于小于等符号</param>
			/// <returns></returns>
			public String MakeCondition(String name, Object value, String action)
			{
				return Entity<TEntity>.MakeCondition(name, value, action);
			}

			/// <summary>构造条件</summary>
			/// <param name="field">名称</param>
			/// <param name="value">值</param>
			/// <param name="action">大于小于等符号</param>
			/// <returns></returns>
			public String MakeCondition(FieldItem field, Object value, String action)
			{
				return Entity<TEntity>.MakeCondition(field, value, action);
			}

			#endregion

			#region -- 一些设置 --

			[ThreadStatic]
			private static Boolean _AllowInsertIdentity;

			/// <summary>是否允许向自增列插入数据。为免冲突，仅本线程有效</summary>
			public Boolean AllowInsertIdentity
			{
				get { return _AllowInsertIdentity; }
				set { _AllowInsertIdentity = value; }
			}

			//private FieldItem _AutoSetGuidField;

			///// <summary>自动设置Guid的字段。对实体类有效，可在实体类类型构造函数里面设置</summary>
			//public FieldItem AutoSetGuidField
			//{
			//	get { return _AutoSetGuidField; }
			//	set { _AutoSetGuidField = value; }
			//}

			[NonSerialized, IgnoreDataMember, XmlIgnore]
			private ICollection<String> _AdditionalFields;

			/// <summary>默认累加字段</summary>
			public ICollection<String> AdditionalFields
			{
				//get { return _AdditionalFields ?? (_AdditionalFields = new ConcurrentHashSet<String>(StringComparer.OrdinalIgnoreCase)); }
				get
				{
					if (_AdditionalFields == null)
					{
						Interlocked.CompareExchange<ICollection<String>>(ref _AdditionalFields, new ConcurrentHashSet<String>(StringComparer.OrdinalIgnoreCase), null);
					}
					return _AdditionalFields;
				}
			}

			#endregion

			#region -- 分表分库 --

			[NonSerialized, IgnoreDataMember, XmlIgnore]
			private Boolean _UsingSelfShardingKeyField;

			/// <summary>数据分片键值是否采用实体自身指定字段的值</summary>
			public Boolean UsingSelfShardingKeyField
			{
				get { return _UsingSelfShardingKeyField; }
				set { _UsingSelfShardingKeyField = value; }
			}

			[NonSerialized, IgnoreDataMember, XmlIgnore]
			private String _ShardingKeyFieldName;

			/// <summary>默认数据分片键字段名称</summary>
			public String ShardingKeyFieldName
			{
				get { return _ShardingKeyFieldName; }
				set { _ShardingKeyFieldName = value; }
			}

			/// <summary>实体数据分片提供者工厂</summary>
			public IShardingProviderFactory ShardingProviderFactory
			{
				get { return Meta.ShardingProviderFactory; }
			}

			///// <summary>在分库上执行操作，自动还原</summary>
			///// <param name="connName">连接名</param>
			///// <param name="tableName">表名</param>
			///// <param name="func"></param>
			///// <returns></returns>
			//public Object ProcessWithSharding(String connName, String tableName, Func<Object> func)
			//{
			//	return Meta.ProcessWithSharding(connName, tableName, func);
			//}

			///// <summary>创建分库会话，using结束时自动还原</summary>
			///// <param name="connName">连接名</param>
			///// <param name="tableName">表名</param>
			///// <returns></returns>
			//public IDisposable CreateShard(String connName, String tableName)
			//{
			//	return Meta.CreateShard(connName, tableName);
			//}

			#endregion

			#region -- 读写锁令牌 --

			/// <summary>创建实体会话读锁令牌</summary>
			/// <returns></returns>
			public IDisposable CreateReadLockToken()
			{
				return Meta.Session.CreateReadLockToken();
			}

			/// <summary>创建实体会话写锁令牌</summary>
			/// <returns></returns>
			public IDisposable CreateWriteLockToken()
			{
				return Meta.Session.CreateWriteLockToken();
			}

			#endregion
		}
	}
}