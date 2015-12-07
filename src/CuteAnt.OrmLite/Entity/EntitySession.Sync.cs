using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web;
using CuteAnt.AsyncEx;
using CuteAnt.Collections;
using CuteAnt.OrmLite.Cache;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>实体会话。每个实体类、连接名和表名形成一个实体会话</summary>
	partial class EntitySession<TEntity>
	{
		#region -- 数据初始化 --

		/// <summary>记录已进行数据初始化</summary>
		internal Boolean hasCheckInitData = false;
		private Int32 initThread = 0;
		private Object _wait_lock = new Object();

		/// <summary>检查并初始化数据。参数等待时间为0表示不等待</summary>
		/// <param name="ignoreIndexs">忽略索引</param>
		/// <param name="ms">等待时间，-1表示不限，0表示不等待</param>
		/// <returns>如果等待，返回是否收到信号</returns>
		public Boolean WaitForInitData(Boolean ignoreIndexs = false, Int32 ms = 1000)
		{
			// 已初始化
			if (hasCheckInitData) { return true; }

			//!!! 一定一定小心堵塞的是自己
			if (initThread == TaskShim.CurrentManagedThreadId) { return true; }

			if (!Monitor.TryEnter(_wait_lock, ms))
			{
				//if (DAL.Debug) DAL.WriteLog("开始等待初始化{0}数据{1}ms，调用栈：{2}", name, ms, XTrace.GetCaller());
				if (DAL.Debug) { DAL.WriteLog("等待初始化{0}数据{1:n0}ms失败", ThisType.Name, ms); }
				return false;
			}
			initThread = TaskShim.CurrentManagedThreadId;
			try
			{
				// 已初始化
				if (hasCheckInitData) { return true; }

				var name = ThisType.Name;
				if (name == TableName)
				{
					name = String.Format("{0}@{1}", ThisType.Name, ConnName);
				}
				else
				{
					name = String.Format("{0}#{1}@{2}", ThisType.Name, TableName, ConnName);
				}

				// 如果该实体类是首次使用检查模型，则在这个时候检查
				try
				{
					CheckModel(ignoreIndexs);
				}
				catch { }

				// 输出调用者，方便调试
				//if (DAL.Debug) DAL.WriteLog("初始化{0}数据，调用栈：{1}", name, XTrace.GetCaller());
				//if (DAL.Debug) { DAL.WriteLog("初始化{0}数据", name); }

				var init = OrmLiteConfig.Current.InitData;
				if (init)
				{
					try
					{
						var entity = Operate.Default as EntityBase;
						if (entity != null) { entity.InitData(); }
					}
					catch (Exception ex)
					{
						DAL.Logger.Error(ex, "初始化数据出错！");
					}
				}

				return true;
			}
			finally
			{
				initThread = 0;
				hasCheckInitData = true;
				Monitor.Exit(_wait_lock);
			}
		}

		#endregion

		#region -- 架构检查 --

		private void CheckTable()
		{
			CheckTable(false);
		}

		private void CheckTable(Boolean ignoreIndexs)
		{
			//if (Dal.CheckAndAdd(TableName)) return;

#if DEBUG
			DAL.WriteLog("开始{2}检查表[{0}/{1}]的数据表架构……", Table.DataTable.Name, Dal.Db.DbType, DAL.NegativeCheckOnly ? "异步" : "同步");
#endif

			var sw = new Stopwatch();
			sw.Start();

			try
			{
				// 检查新表名对应的数据表
				var table = Table.DataTable;
				// 克隆一份，防止修改
				table = table.Clone() as IDataTable;

				if (table.TableName != TableName)
				{
					FixIndexName(table);
					table.TableName = TableName;
				}

				// 忽略实体模型索引
				if (ignoreIndexs) { table.Indexes.Clear(); }

				var set = new NegativeSetting();
				set.CheckOnly = DAL.NegativeCheckOnly;
				set.NoDelete = DAL.NegativeNoDelete;

				// 对于分库操作，强制检查架构，但不删除数据
				if (Default != this)
				{
					set.CheckOnly = false;
					set.NoDelete = true;
				}

				Dal.SetTables(set, table);
			}
			finally
			{
				sw.Stop();

#if DEBUG
				DAL.WriteLog("检查表[{0}/{1}]的数据表架构耗时{2:n0}ms", Table.DataTable.Name, Dal.DbType, sw.Elapsed.TotalMilliseconds);
#endif
			}
		}

		private void FixIndexName(IDataTable table)
		{
			// 修改一下索引名，否则，可能因为同一个表里面不同的索引冲突
			if (table.Indexes != null)
			{
				foreach (var di in table.Indexes)
				{
					var sb = new StringBuilder();
					sb.AppendFormat("IX_{0}", TableName);
					foreach (var item in di.Columns)
					{
						sb.Append("_");
						sb.Append(item);
					}

					di.Name = sb.ToString();
				}
			}
		}

		private Boolean IsGenerated { get { return ThisType.GetCustomAttributeX<CompilerGeneratedAttribute>(true) != null; } }

		private Boolean hasCheckModel = false;
		private Object _check_lock = new Object();

		/// <summary>检查模型。依据反向工程设置、是否首次使用检查、是否已常规检查等</summary>
		private void CheckModel(Boolean ignoreIndexs)
		{
			if (hasCheckModel) { return; }
			lock (_check_lock)
			{
				if (hasCheckModel) { return; }

				// 是否默认连接和默认表名，非默认则强制检查，并且不允许异步检查（异步检查会导致ConnName和TableName不对）
				var def = Default;

				if (def == this)
				{
					if (!DAL.NegativeEnable ||
							DAL.NegativeExclude.Contains(ConnName) ||
							DAL.NegativeExclude.Contains(TableName) ||
							IsGenerated)
					{
						hasCheckModel = true;
						return;
					}
				}
#if DEBUG
				else
				{
					DAL.WriteLog("[{0}@{1}]非默认表名连接名，强制要求检查架构！", TableName, ConnName);
				}
#endif

				// 输出调用者，方便调试
				//if (DAL.Debug) DAL.WriteLog("检查实体{0}的数据表架构，模式：{1}，调用栈：{2}", ThisType.FullName, Table.ModelCheckMode, XTrace.GetCaller(1, 0, "\r\n<-"));
				// CheckTableWhenFirstUse的实体类，在这里检查，有点意思，记下来
				var mode = Table.ModelCheckMode;
				if (DAL.Debug && mode == ModelCheckModes.CheckTableWhenFirstUse)
				{
					DAL.WriteLog("检查实体{0}的数据表架构，模式：{1}", ThisType.FullName, mode);
				}

				// 第一次使用才检查的，此时检查
				var ck = false;
				if (mode == ModelCheckModes.CheckTableWhenFirstUse) { ck = true; }
				// 或者前面初始化的时候没有涉及的，也在这个时候检查
				var dal = DAL.Create(ConnName);
				if (!dal.HasCheckTables.Contains(TableName))
				{
					if (!ck)
					{
						dal.HasCheckTables.TryAdd(TableName);

#if DEBUG
						if (!ck && DAL.Debug) { DAL.WriteLog("集中初始化表架构时没赶上，现在补上！"); }
#endif

						ck = true;
					}
				}
				else
				{
					ck = false;
				}

				if (ck)
				{
					// 打开了开关，并且设置为true时，使用同步方式检查
					// 设置为false时，使用异步方式检查，因为上级的意思是不大关心数据库架构
					if (!DAL.NegativeCheckOnly || def != this)
					{
						CheckTable(ignoreIndexs);
					}
					else
					{
						TaskShim.Run(new Action(CheckTable));
					}
				}

				hasCheckModel = true;
			}
		}

		#endregion

		#region -- 数据库操作 --

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private void InitData()
		{
			WaitForInitData();
		}

		#region - Query -

		/// <summary>尝试获取缓存的记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="ds">返回记录集</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <returns>是否获取成功</returns>
		internal Boolean TryQueryWithCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			out DataSet ds, out String pageSplitCacheKey)
		{
			InitData();

			return Dal.TrySelectWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey);
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <returns></returns>
		internal DataSet QueryWithoutCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			String pageSplitCacheKey)
		{
			//InitData();

			return Dal.SelectWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey, TableName);
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns></returns>
		public virtual DataSet Query(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			InitData();

			return Dal.Select(builder, startRowIndex, maximumRows, TableName);
		}

		/// <summary>查询</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>结果记录集</returns>
		//[Obsolete("请优先考虑使用SelectBuilder参数做查询！")]
		public virtual DataSet Query(String sql)
		{
			InitData();

			return Dal.Select(sql, TableName);
		}

		#endregion

		#region - QueryRecords -

		/// <summary>尝试获取缓存的记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="ds">返回记录集</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <returns>是否获取成功</returns>
		internal Boolean TryQueryRecordsWithCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			out IList<QueryRecords> ds, out String pageSplitCacheKey)
		{
			InitData();

			return Dal.TrySelectRecordsWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey);
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <returns></returns>
		internal IList<QueryRecords> QueryRecordsWithoutCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			String pageSplitCacheKey)
		{
			//InitData();

			return Dal.SelectRecordsWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey, TableName);
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns></returns>
		public virtual IList<QueryRecords> QueryRecords(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			InitData();

			return Dal.SelectRecords(builder, startRowIndex, maximumRows, TableName);
		}

		/// <summary>查询</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>结果记录集</returns>
		//[Obsolete("请优先考虑使用SelectBuilder参数做查询！")]
		public virtual IList<QueryRecords> QueryRecords(String sql)
		{
			InitData();

			return Dal.SelectRecords(sql, TableName);
		}

		#endregion

		#region - QueryCount -

		/// <summary>查询记录数</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>记录数</returns>
		public virtual Int64 QueryCount(String sql)
		{
			var sb = new SelectBuilder();
			sb.Parse(sql);
			return QueryCount(sb);
		}

		/// <summary>查询记录数</summary>
		/// <param name="builder">查询生成器</param>
		/// <returns>记录数</returns>
		public virtual Int64 QueryCount(SelectBuilder builder)
		{
			InitData();

			return Dal.SelectCount(builder, TableNames);
		}

		/// <summary>尝试获取缓存的总记录数</summary>
		/// <param name="sb">查询生成器</param>
		/// <param name="count">返回总记录数</param>
		/// <param name="cacheKey">返回缓存键值</param>
		/// <returns>是否查找成功</returns>
		internal Boolean TryQueryCountWithCache(SelectBuilder sb, out Int64 count, out String cacheKey)
		{
			InitData();

			return Dal.TrySelectCountWithCache(sb, out count, out cacheKey);
		}

		/// <summary>执行SQL查询，返回总记录数</summary>
		/// <param name="sb">查询生成器</param>
		/// <param name="cacheKey">缓存键值</param>
		/// <returns></returns>
		internal Int64 QueryCountWithoutCache(SelectBuilder sb, String cacheKey)
		{
			// TryQueryCountWithCache方法已经调用InitData
			//InitData();

			return Dal.SelectCountWithoutCache(sb, cacheKey, TableNames);
		}

		#endregion

		#region - Truncate -

		/// <summary>执行Truncate语句</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>影响的结果</returns>
		internal Int32 Truncate(String sql)
		{
			InitData();

			Int32 rs;
			if (Dal.DbType == DatabaseType.SQLite)
			{
				rs = Dal.Execute("Delete From {0}".FormatWith(FormatedTableName), TableName);
				Dal.Execute("VACUUM", TableName);
				Dal.Execute("update sqlite_sequence set seq=0 where name='{0}'".FormatWith(TableName), TableName);
				return rs;
			}
			else
			{
				rs = Dal.Execute(sql, TableName);
			}

			ClearCache("TRUNCATE TABLE");
			if (_OnDataChange != null) { _OnDataChange(ThisType); }

			Count = c_emptyCount;
			// 重新初始化
			hasCheckInitData = false;

			return rs;
		}

		#endregion

		#region - Execute -

		/// <summary>执行</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>影响的结果</returns>
		public Int32 Execute(String sql)
		{
			return Execute(true, false, sql);
		}

		/// <summary>执行</summary>
		/// <param name="forceClearCache">是否跨越实体操作，直接执行SQL语句</param>
		/// <param name="isUpdateMode">是否执行更新实体操作</param>
		/// <param name="sql">SQL语句</param>
		/// <returns>影响的结果</returns>
		internal Int32 Execute(Boolean forceClearCache, Boolean isUpdateMode, String sql)
		{
			InitData();

			Int32 rs = Dal.Execute(sql, TableName);
			_ExecuteCount++;
			if (forceClearCache) { _DirectExecuteSQLCount++; }
			if (isUpdateMode) { _UpdateCount++; }
			DataChange("Execute", forceClearCache, isUpdateMode);
			return rs;
		}

		/// <summary>执行</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>影响的结果</returns>
		public Int32 Execute(String sql, CommandType type, DbParameter[] ps)
		{
			return Execute(true, false, sql, type, ps);
		}

		/// <summary>执行</summary>
		/// <param name="forceClearCache">是否跨越实体操作，直接执行SQL语句</param>
		/// <param name="isUpdateMode">是否执行更新实体操作</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>影响的结果</returns>
		internal Int32 Execute(Boolean forceClearCache, Boolean isUpdateMode, String sql, CommandType type, DbParameter[] ps)
		{
			InitData();

			Int32 rs = Dal.Execute(sql, type, ps, TableName);
			_ExecuteCount++;
			if (forceClearCache) { _DirectExecuteSQLCount++; }
			if (isUpdateMode) { _UpdateCount++; }
			DataChange("Execute " + type, forceClearCache, isUpdateMode);
			return rs;
		}

		#endregion

		#region - InsertAndGetIdentity -

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>新增行的自动编号</returns>
		public Int64 InsertAndGetIdentity(String sql)
		{
			return InsertAndGetIdentity(true, sql);
		}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="forceClearCache">是否跨越实体操作，直接执行SQL语句</param>
		/// <param name="sql">SQL语句</param>
		/// <returns>新增行的自动编号</returns>
		internal Int64 InsertAndGetIdentity(Boolean forceClearCache, String sql)
		{
			InitData();

			Int64 rs = Dal.InsertAndGetIdentity(sql, TableName);
			_ExecuteCount++;
			if (forceClearCache) { _DirectExecuteSQLCount++; }
			DataChange("InsertAndGetIdentity", forceClearCache, false);
			return rs;
		}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		public Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			return InsertAndGetIdentity(true, sql, type, ps);
		}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="forceClearCache">是否跨越实体操作，直接执行SQL语句</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		internal Int64 InsertAndGetIdentity(Boolean forceClearCache, String sql, CommandType type, DbParameter[] ps)
		{
			InitData();

			Int64 rs = Dal.InsertAndGetIdentity(sql, type, ps, TableName);
			_ExecuteCount++;
			if (forceClearCache) { _DirectExecuteSQLCount++; }
			DataChange("InsertAndGetIdentity " + type, forceClearCache, false);
			return rs;
		}

		#endregion

		#endregion

		#region -- 事务保护 --

		/// <summary>事务计数</summary>
		[ThreadStatic]
		private static Int32 _TransCount;
		/// <summary>实体操作次数</summary>
		[ThreadStatic]
		private static Int32 _ExecuteCount = 0;
		/// <summary>实体更新操作次数</summary>
		[ThreadStatic]
		private static Int32 _UpdateCount = 0;
		/// <summary>直接执行SQL语句次数</summary>
		[ThreadStatic]
		private static Int32 _DirectExecuteSQLCount = 0;

		/// <summary>开始事务</summary>
		/// <returns>剩下的事务计数</returns>
		public virtual Int32 BeginTrans()
		{
			/* 这里也需要执行初始化检查架构，因为无法确定在调用此方法前是否已对实体类进行架构检查，如下调用会造成事务不平衡而上抛异常：
			 * Exception：执行SqlServer的Dispose时出错：System.InvalidOperationException: 此 SqlTransaction 已完成；它再也无法使用。
			 * 
			 * 如果实体的模型检查模式为CheckTableWhenFirstUse，直接执行静态操作
			 * using (var trans = new EntityTransaction<TEntity>())
			 * {
			 *   TEntity.Delete(whereExp);
			 *   TEntity.Update(new String[] { 字段1,字段2 }, new Object[] { 值1,值1 }, whereExp);
			 *   trans.Commit();
			 * }
			 */
			InitData();

			// 可能存在多层事务，这里不能把这个清零
			//executeCount = 0;

			_ExecuteCount = 0;
			_UpdateCount = 0;
			_DirectExecuteSQLCount = 0;

			return _TransCount = Dal.BeginTransaction();
		}

		/// <summary>提交事务</summary>
		/// <returns>剩下的事务计数</returns>
		public virtual Int32 Commit()
		{
			// 提交事务时更新数据，虽然不是绝对准确，但没有更好的办法
			// 即使提交了事务，但只要事务内没有执行更新数据的操作，也不更新
			// 2012-06-13 测试证明，修改数据后，提交事务后会更新缓存等数据
			if (_ExecuteCount > 0)
			{
				Dal.AddDirtiedEntitySession(Key, this, _ExecuteCount, _UpdateCount, _DirectExecuteSQLCount);

				_ExecuteCount = 0;
				_UpdateCount = 0;
				_DirectExecuteSQLCount = 0;
			}
			_TransCount = Dal.Commit();
			return _TransCount;
		}

		/// <summary>回滚事务，忽略异常</summary>
		/// <returns>剩下的事务计数</returns>
		public virtual Int32 Rollback()
		{
			if (_ExecuteCount > 0)
			{
				Dal.AddDirtiedEntitySession(Key, this, _ExecuteCount, _UpdateCount, _DirectExecuteSQLCount);

				//// 因为在事务保护中添加或删除实体时直接操作了实体缓存，所以需要更新
				//// TODO 缓存回滚是否清楚缓存？未测试；新增、编辑、删除-实体缓存、单对象缓存，单条保存不会有逻辑问题，批量事务应该就不行
				//DataChange("修改数据后回滚事务", _DirectExecuteSQLCount > 0, _UpdateCount > 0, true);
				_ExecuteCount = 0;
				_UpdateCount = 0;
				_DirectExecuteSQLCount = 0;
			}
			_TransCount = Dal.Rollback();
			return _TransCount;
		}

		/// <summary>是否在事务保护中</summary>
		internal Boolean UsingTrans { get { return _TransCount > 0; } }
		//internal Boolean UsingTrans { get { return _TransCount > 1;/*因为Insert上面一定有一层缓存，这里减去1*/ } }

		#endregion

		#region -- 实体操作 --

		//private EntityPersistence<TEntity> Persistence
		//{
		//	get { return Entity<TEntity>.Persistence; }
		//}

		/// <summary>把该对象持久化到数据库，添加/更新实体缓存和单对象缓存，增加总计数</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		Int32 IEntitySession.Insert(IEntity entity)
		{
			return Insert(entity as TEntity);
		}

		/// <summary>把该对象持久化到数据库，添加/更新实体缓存和单对象缓存，增加总计数</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		public virtual Int32 Insert(TEntity entity)
		{
			var rs = EntityPersistence<TEntity>.Insert(entity);
			entity.MarkDb(true);

			// 如果当前在事务中，并使用了缓存，则尝试更新缓存
			if (HoldCache || UsingTrans)
			{
				if (_cache != null) { _cache.Update(entity); }

				// 自动加入单对象缓存
				if (_singleCache != null && _singleCache.Using) { _singleCache.AddOrUpdate(entity); }
			}

			if (_Count >= c_emptyCount) { Interlocked.Increment(ref _Count); }

			return rs;
		}

		/// <summary>更新数据库，同时更新实体缓存</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		Int32 IEntitySession.Update(IEntity entity)
		{
			return Update(entity as TEntity);
		}

		/// <summary>更新数据库，同时更新实体缓存</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		public virtual Int32 Update(TEntity entity)
		{
			var rs = EntityPersistence<TEntity>.Update(entity);
			entity.MarkDb(true);

			// 如果当前在事务中，并使用了缓存，则尝试更新缓存
			if (HoldCache || UsingTrans)
			{
				if (_cache != null) { _cache.Update(entity); }

				// 自动加入单对象缓存
				if (_singleCache != null && _singleCache.Using) { _singleCache.AddOrUpdate(entity); }
			}

			//// 如果当前在事务中，并使用了缓存，则尝试更新缓存
			//if (HoldCache || UsingTrans)
			//{
			//	if (Cache.Using)
			//	{
			//		// 尽管用了事务保护，但是仍然可能有别的地方导致实体缓存更新，这点务必要注意
			//		var fi = Operate.Unique;
			//		var e = fi != null ? Cache.Entities.Find(fi.Name, entity[fi.Name]) : null;
			//		if (e != null)
			//		{
			//			if (e != entity)
			//			{
			//				e.CopyFrom(entity, false);
			//				// 缓存中实体对象也需要清空脏数据
			//				e.Dirtys.Clear();
			//			}
			//		}
			//		else
			//		{
			//			// 加入超级缓存的实体对象，需要标记来自数据库
			//			entity.OnLoad();

			//			Cache.Entities.Add(entity);
			//		}
			//	}

			//	// 自动加入单对象缓存
			//	if (SingleCache.Using)
			//	{
			//		var getkeymethod = SingleCache.GetKeyMethod;
			//		if (getkeymethod != null)
			//		{
			//			var key = getkeymethod(entity);
			//			// 复制到单对象缓存
			//			TEntity cacheitem;
			//			if (SingleCache.TryGetItem(key, out cacheitem))
			//			{
			//				if (cacheitem != null)
			//				{
			//					if (cacheitem != entity)
			//					{
			//						cacheitem.CopyFrom(entity, false);
			//						// 缓存中实体对象也需要清空脏数据
			//						cacheitem.Dirtys.Clear();
			//					}
			//				}
			//				else // 存在允许存储空对象的情况
			//				{
			//					SingleCache.RemoveKey(key);
			//					entity.OnLoad();
			//					SingleCache.Add(key, entity);
			//				}
			//			}
			//			else
			//			{
			//				entity.OnLoad();
			//				SingleCache.Add(key, entity);
			//			}
			//		}
			//	}
			//}

			return rs;
		}

		/// <summary>从数据库中删除该对象，同时从实体缓存和单对象缓存中删除，扣减总数量</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		Int32 IEntitySession.Delete(IEntity entity)
		{
			return Delete(entity as TEntity);
		}

		/// <summary>从数据库中删除该对象，同时从实体缓存和单对象缓存中删除，扣减总数量</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		public virtual Int32 Delete(TEntity entity)
		{
			var rs = EntityPersistence<TEntity>.Delete(entity);

			// 如果当前在事务中，并使用了缓存，则尝试更新缓存
			if (HoldCache || UsingTrans)
			{
				if (_cache != null)
				{
					var fi = Operate.Unique;
					if (fi != null)
					{
						var v = entity[fi.Name];
						Cache.Entities.RemoveAll(e => Object.Equals(e[fi.Name], v));
					}
				}
				// 自动加入单对象缓存
				if (_singleCache != null) { _singleCache.Remove(entity, false); }
			}

			if (_Count >= c_emptyCount) { Interlocked.Decrement(ref _Count); }

			return rs;
		}

		#endregion
	}
}