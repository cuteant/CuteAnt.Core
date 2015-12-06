/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
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

/*
 * 检查表结构流程：
 * Create           创建实体会话，此时不能做任何操作，原则上各种操作要延迟到最后
 * Query/Execute    查询修改数据
 *      WaitForInitData     等待数据初始化
 *          Monitor.TryEnter    只会被调用一次，后续线程进入需要等待
 *          CheckModel          lock阻塞检查模型架构
 *              CheckTable      检查表
 *                  FixIndexName    修正索引名称
 *                  SetTables       设置表架构
 *              CheckTableAync  异步检查表
 *          InitData
 *          Monitor.Exit        初始化完成
 * Count            总记录数
 *      CheckModel
 *      WaitForInitData
 *
 * 缓存更新规则如下：
 * 1、直接执行SQL语句进行新增、编辑、删除操作，强制清空实体缓存、单对象缓存
 * 2、无论独占模式或非独占模式，使用实体对象或实体列表进行的对象操作，不再主动清空缓存。
 * 3、事务提交时对缓存的操作参考前两条
 * 4、事务回滚时一律强制清空缓存（因为无法判断什么异常触发回滚，回滚之前对缓存进行了哪些修改无法记录）
 * 5、强制清空缓存时传入执行更新操作记录数，如果存在更新操作清除实体缓存、单对象缓存。
 * 6、强制清空缓存时如果只有新增和删除操作，先判断当前实体类有无使用实体缓存，如果使用了实体缓存证明当前实体总记录数不大，
 *    清空实体缓存的同时清空单对象缓存，确保单对象缓存和实体缓存引用同一实体对象；没有实体缓存则不对单对象缓存进行清空操作。
 * */

namespace CuteAnt.OrmLite
{
	/// <summary>实体会话。每个实体类、连接名和表名形成一个实体会话</summary>
	public partial class EntitySession<TEntity> : IEntitySession where TEntity : Entity<TEntity>, new()
	{
		#region -- 属性 --

		private String _ConnName;

		/// <summary>连接名</summary>
		public String ConnName
		{
			get { return _ConnName; }
			private set { _ConnName = value; _Key = null; }
		}

		private String _TableName;

		/// <summary>表名</summary>
		public String TableName
		{
			get { return _TableName; }
			private set { _TableName = value; _Key = null; }
		}

		private String[] _TableNames;
		/// <summary>表名数组</summary>
		internal String[] TableNames { get { return _TableNames ?? (_TableNames = new String[] { TableName }); } }

		private String _Key;

		/// <summary>用于标识会话的键值</summary>
		public String Key
		{
			get { return _Key ?? (_Key = String.Format("{0}$$${1}", ConnName, TableName)); }
		}

		#endregion

		#region -- 构造 --

		private EntitySession()
		{
		}

		private static DictionaryCache<String, EntitySession<TEntity>> _es = new DictionaryCache<String, EntitySession<TEntity>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>创建指定表名连接名的会话</summary>
		/// <param name="connName"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public static EntitySession<TEntity> Create(String connName, String tableName)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");
			ValidationHelper.ArgumentNullOrEmpty(tableName, "tableName");

			var key = connName + "$$$" + tableName;
			return _es.GetItem<String, String>(key, connName, tableName, (k, c, t) => new EntitySession<TEntity> { ConnName = c, TableName = t });
		}

		#endregion

		#region -- 主要属性 --

		private static Type ThisType
		{
			get { return typeof(TEntity); }
		}

		/// <summary>表信息</summary>
		private static TableItem Table
		{
			get { return TableItem.Create(ThisType); }
		}

		/// <summary>实体操作者</summary>
		public IEntityOperate Operate
		{
			get { return EntityFactory.CreateOperate(ThisType); }
		}

		private DAL _Dal;

		/// <summary>数据操作层</summary>
		public DAL Dal
		{
			get { return _Dal ?? (_Dal = DAL.Create(ConnName)); }
		}

		/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
		public IQuoter Quoter
		{
			get { return Dal.Db.Quoter; }
		}

		private String _FormatedTableName;

		/// <summary>已格式化的表名，带有中括号等</summary>
		public virtual String FormatedTableName
		{
			get { return _FormatedTableName ?? (_FormatedTableName = (Dal.Db as DbBase).FormatTableName(TableName)); }
		}

		private EntitySession<TEntity> _Default;

		/// <summary>该实体类的默认会话。</summary>
		private EntitySession<TEntity> Default
		{
			get
			{
				if (_Default != null) { return _Default; }

				if (ConnName == Table.ConnName && TableName == Table.TableName)
				{
					_Default = this;
				}
				else
				{
					_Default = Create(Table.ConnName, Table.TableName);
				}

				return _Default;
			}
		}

		private IDataTable _DbTable;

		internal IDataTable DbTable
		{
			get
			{
				if (_DbTable == null)
				{
					try
					{
						_DbTable = Dal.Db.SchemaProvider.GetTable(TableName);
					}
					catch { }
				}
				return _DbTable;
			}
		}

		#endregion

		#region -- 读写锁令牌 --

		private Boolean? _ReadWriteLockEnable;

		/// <summary>是否启用读写锁机制</summary>
		public Boolean ReadWriteLockEnable
		{
			get
			{
				if (_ReadWriteLockEnable == null) { return DAL.ReadWriteLockEnable; }
				return _ReadWriteLockEnable.Value;
			}
			set
			{
				// 如果设定值跟DAL.ReadWriteLockEnable相同，则直接使用DAL.ReadWriteLockEnable
				if (value == DAL.ReadWriteLockEnable)
				{
					_ReadWriteLockEnable = null;
				}
				else
				{
					_ReadWriteLockEnable = value;
				}
			}
		}

		/// <summary>创建实体会话读锁令牌</summary>
		/// <returns></returns>
		public IDisposable CreateReadLockToken()
		{
			return new LockToken(MsSQLShareRWLock, true);
		}

		/// <summary>创建实体会话写锁令牌</summary>
		/// <returns></returns>
		public IDisposable CreateWriteLockToken()
		{
			return new LockToken(MsSQLShareRWLock, false);
		}

		#region - 读写锁 -

		private Boolean? _IsMsSQL;

		/// <summary>是否SQL Server</summary>
		private Boolean IsMsSQL
		{
			get
			{
				if (_IsMsSQL.HasValue) { return _IsMsSQL.Value; }

				_IsMsSQL = Dal.DbType == DatabaseType.SQLServer;

				return _IsMsSQL.Value;
			}
		}

		//private ReaderWriterLockSlim _MsSQLRWLock;

		///// <summary>SQL Server数据库读写锁</summary>
		//private ReaderWriterLockSlim MsSQLRWLock
		//{
		//	get
		//	{
		//		if (_MsSQLRWLock == null)
		//		{
		//			var rwlock = new ReaderWriterLockSlim();
		//			Interlocked.CompareExchange<ReaderWriterLockSlim>(ref _MsSQLRWLock, rwlock, null);
		//		}
		//		return _MsSQLRWLock;
		//	}
		//}

		private ReaderWriterLockSlim _MsSQLShareRWLock;

		/// <summary>SQL Server事务读写锁</summary>
		private ReaderWriterLockSlim MsSQLShareRWLock
		{
			get
			{
				if (ReadWriteLockEnable && IsMsSQL && _MsSQLShareRWLock == null)
				{
					var rwlock = new ReaderWriterLockSlim();
					Interlocked.CompareExchange<ReaderWriterLockSlim>(ref _MsSQLShareRWLock, rwlock, null);
				}
				return _MsSQLShareRWLock;
			}
		}

		#endregion

		#region - LockToken -

		struct LockToken : IDisposable
		{
			private ReaderWriterLockSlim m_rwlock;
			private Boolean m_isReadMode;
			private Boolean m_hasStarted;

			public LockToken(ReaderWriterLockSlim rwlock, Boolean isReadMode)
			{
				m_rwlock = rwlock;
				m_isReadMode = isReadMode;
				m_hasStarted = false;
				if (m_rwlock == null) { return; }
				try
				{
					if (m_isReadMode)
					{
						m_rwlock.EnterReadLock();
					}
					else
					{
						m_rwlock.EnterWriteLock();
					}
					m_hasStarted = true;
				}
				catch { }
			}

			public void Dispose()
			{
				if (m_hasStarted)
				{
					try
					{
						if (m_isReadMode)
						{
							m_rwlock.ExitReadLock();
						}
						else
						{
							m_rwlock.ExitWriteLock();
						}
					}
					catch { }
				}
				m_rwlock = null;
			}
		}

		#endregion

		#endregion

		#region -- 缓存 --

		private static EntityCacheConfig.EntityCacheInfo _EntityCacheSetting;

		private static EntityCacheConfig.EntityCacheInfo EntityCacheSetting
		{
			get { return _EntityCacheSetting ?? (_EntityCacheSetting = EntityCacheConfig.Current.Find(Table.ConnName, Table.TableName)); }
		}

		/// <summary>实体缓存是否已被禁用</summary>
		public Boolean EntityCacheDisabled
		{
			get { return EntityCacheSetting.DisableEntityCache || Count > Cache.MaxCount; }
		}

		/// <summary>单对象缓存是否已被禁用</summary>
		public Boolean SingleCacheDisabled
		{
			get { return EntityCacheSetting.DisableSingleCache; }
		}

		internal EntityCache<TEntity> _cache;

		/// <summary>实体缓存</summary>
		/// <returns></returns>
		public EntityCache<TEntity> Cache
		{
			get
			{
				// 以连接名和表名为key，因为不同的库不同的表，缓存也不一样
				//return _cache ?? (_cache = new EntityCache<TEntity> { ConnName = ConnName, TableName = TableName });

				//if (_cache == null)
				//{
				//	_cache = new EntityCache<TEntity>();
				//	_cache.ConnName = ConnName;
				//	_cache.TableName = TableName;
				//	_cache.Expriod = Entity<TEntity>.Meta.EntityCacheExpriod;
				//}
				//return _cache;

				if (_cache == null)
				{
					var ec = new EntityCache<TEntity>();
					ec.ConnName = ConnName;
					ec.TableName = TableName;
					// 从默认会话复制参数
					if (Default == this)
					{
						var cacheInfo = EntityCacheSetting;
						if (cacheInfo.EntityCacheExpriod > 0) { ec.Expriod = cacheInfo.EntityCacheExpriod; }
						if (cacheInfo.EntityCacheMaxCount > 0) { ec.MaxCount = cacheInfo.EntityCacheMaxCount; }
						if (cacheInfo.HoldEntityCache > -1) { ec.HoldCache = cacheInfo.HoldEntityCache > 0; }
					}
					else
					{
						ec.CopySettingFrom(Default.Cache);
					}
					//_cache = ec;
					Interlocked.CompareExchange<EntityCache<TEntity>>(ref _cache, ec, null);
				}
				return _cache;
			}
		}

		internal SingleEntityCache<Object, TEntity> _singleCache;

		/// <summary>单对象实体缓存。
		/// 建议自定义查询数据方法，并从二级缓存中获取实体数据，以抵消因初次填充而带来的消耗。
		/// </summary>
		public SingleEntityCache<Object, TEntity> SingleCache
		{
			get
			{
				// 以连接名和表名为key，因为不同的库不同的表，缓存也不一样
				//return _singleCache ?? (_singleCache = new SingleEntityCache<Object, TEntity> { ConnName = ConnName, TableName = TableName });

				if (_singleCache == null)
				{
					var sc = new SingleEntityCache<Object, TEntity>();
					sc.ConnName = ConnName;
					sc.TableName = TableName;
					sc.HoldCache = HoldCache; // 非独占模式下需要启动计时器清理过期缓存项

					// 从默认会话复制参数
					if (Default == this)
					{
						var cacheInfo = EntityCacheSetting;
						if (cacheInfo.SingleCacheExpriod > 0) { sc.Expriod = cacheInfo.SingleCacheExpriod; }
						if (cacheInfo.SingleCacheMaxCount > 0) { sc.MaxCount = cacheInfo.SingleCacheMaxCount; }
						if (cacheInfo.HoldSingleCache > -1) { sc.HoldCache = cacheInfo.HoldSingleCache > 0; }
					}
					else
					{
						sc.CopySettingFrom(Default.SingleCache);
					}

					//_singleCache = sc;
					Interlocked.CompareExchange<SingleEntityCache<Object, TEntity>>(ref _singleCache, sc, null);
				}
				return _singleCache;
			}
		}

		IEntityCache IEntitySession.Cache
		{
			get { return Cache; }
		}

		ISingleEntityCache IEntitySession.SingleCache
		{
			get { return SingleCache; }
		}

		private const Int64 c_nullCount = -1L;
		private const Int64 c_emptyCount = 0L;
		/// <summary>上一次记录数，用于衡量缓存策略，不受缓存清空</summary>
		private Int64 _LastCount = c_nullCount;
		/// <summary>总记录数较小时，使用静态字段，较大时增加使用Cache</summary>
		internal Int64 _Count = c_nullCount;

		/// <summary>总记录数，小于等于1000时是精确的，大于1000时缓存10分钟</summary>
		/// <remarks>
		/// 1，检查静态字段，如果有数据且小于等于1000，直接返回，否则=>3
		/// 2，如果有数据但大于1000，则返回缓存里面的有效数据
		/// 3，来到这里，有可能是第一次访问，静态字段没有缓存，也有可能是大于1000的缓存过期
		/// 4，检查模型
		/// 5，根据需要查询数据
		/// 6，如果大于1000，缓存数据
		/// 7，检查数据初始化
		/// </remarks>
		public Int64 Count
		{
			get
			{
				var key = CacheKey;

				// 当前缓存的值
				Int64 n = _Count;

				// 如果有缓存，则考虑返回吧
				if (n > c_nullCount)
				{
					// 等于0的时候也应该缓存，否则会一直查询这个表
					if (n <= Cache.MaxCount) { return n; }

					// 大于1000，使用HttpCache
					Int64? k = (Int64?)HttpRuntime.Cache[key];
					//if (k != null && k.HasValue) { return k.Value; }
					if (k.HasValue) { return k.Value; }
				}

				// 来到这里，有可能是第一次访问，静态字段没有缓存，也有可能是大于1000的缓存过期
				CheckModel(false);

				const Int64 _MaxSQLiteCount = 500000L;

				Int64 m = c_emptyCount;
				// 小于等于1000的精确查询，大于1000的快速查询
				if (n > c_nullCount && n <= Cache.MaxCount)
				{
					var sb = new SelectBuilder();
					sb.Table = FormatedTableName;

					WaitForInitData();
					m = Dal.SelectCount(sb, TableNames);
				}
				else
				{
					// 第一次访问，SQLite的Select Count非常慢，数据大于阀值时，使用最大ID作为表记录数
					var max = c_emptyCount;
					if (Dal.DbType == DatabaseType.SQLite && Table.Identity != null)
					{
						// 除第一次外，将依据上一次记录数决定是否使用最大ID
						if (_LastCount < c_emptyCount || _LastCount >= _MaxSQLiteCount)
						{
							// 先查一下最大值
							//max = Entity<TEntity>.FindMax(Table.Identity.ColumnName);
							// 依赖关系FindMax=>FindAll=>Query=>InitData=>Meta.Count，所以不能使用

							if (DAL.Debug) { DAL.WriteLog("第一次访问，SQLite的Select Count非常慢，数据大于阀值时，使用最大ID作为表记录数"); }

							var builder = new SelectBuilder();
							builder.Table = FormatedTableName;
							builder.OrderBy = Table.Identity.Desc();
							var ds = Dal.Select(builder, 0, 1, TableName);
							if (ds.Tables[0].Rows.Count > 0)
							{
								max = Convert.ToInt64(ds.Tables[0].Rows[0][Table.Identity.ColumnName]);
							}
						}
					}

					// 100w数据时，没有预热Select Count需要3000ms，预热后需要500ms
					if (max < _MaxSQLiteCount)
					{
						m = Dal.Session.QueryCountFast(TableName);
					}
					else
					{
						m = max;
					}
				}

				_Count = m;
				_LastCount = m;

				if (m > Cache.MaxCount)
				{
					HttpRuntime.Cache.Insert(key, m, null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);
				}

				// 先拿到记录数再初始化，因为初始化时会用到记录数，同时也避免了死循环
				WaitForInitData();

				return m;
			}
			internal set
			{
				if (value == c_emptyCount)
				{
					_LastCount = c_nullCount;
					_Count = c_emptyCount;
					HttpRuntime.Cache.Remove(CacheKey);
				}
			}
		}

		/// <summary>清除缓存</summary>
		/// <param name="reason">原因</param>
		/// <param name="isUpdateMode">是否执行更新实体操作</param>
		/// <param name="isTransRoolback">是否在事务回滚时执行更新实体操作</param>
		private void ClearCache(String reason, Boolean isUpdateMode, Boolean isTransRoolback)
		{
			// 无法判断事务回滚是由什么样的异常触发，缓存数据可能已被修改，所有强制清空缓存
			//if (HoldCache && !isTransRoolback)
			if (!isTransRoolback) // 非独占模式下也保持缓存，由缓存有效期设置来清空
			{
				if (_cache != null && _cache.Using)
				{
					// 在独占模式下，当实体记录数大于1000，清空实体缓存后不再使用
					// 如果不清空，业务代码在记录数大于1000后不再调用实体缓存，就没有清空的机会了，
					// 而且在独占模式或事务保护中实体的新增、编辑、删除操作会一直维护实体缓存
					// http://www.newlifex.com/showtopic-1152.aspx
					if (Count > _cache.MaxCount)
					{
						_cache.Clear(false, "实体记录数大于{0}条，销毁实体缓存！！！".FormatWith(_cache.MaxCount));
					}
				}
			}
			else
			{
				ForceClearCache(reason, isUpdateMode);
			}
		}

		/// <summary>清除缓存，直接执行SQl语句或事务回滚时使用</summary>
		/// <param name="reason">原因</param>
		/// <param name="isUpdateMode">是否执行更新实体操作</param>
		/// <remarks>http://www.newlifex.com/showtopic-1216.aspx</remarks>
		private void ForceClearCache(String reason, Boolean isUpdateMode)
		{
			var clearEntityCache = false;
			if (_cache != null && _cache.Using)
			{
				clearEntityCache = true;
				_cache.Clear(true, reason);
			}

			// 因为单对象缓存在实际应用场景中可能会非常庞大
			// 添加、删除不再维护单对象缓存，新添加的会自动获取，删除的一直保留在内存中等待SingleCache的定时清理或RemoveFirst方法慢慢清除
			// 如果清理了实体缓存证明当前实体数据量不大，也清除单对象缓存，尽量使单对象缓存的实体对象与实体缓存中的实体对象一致
			if (isUpdateMode || clearEntityCache)
			{
				if (_singleCache != null && _singleCache.Using)
				{
					TaskShim.Run(() =>
					{
						_singleCache.Clear(reason);
						_singleCache.Initialize();
					});
				}
			}

			if (!isUpdateMode)
			{
				Int64 n = _Count;
				if (n < c_emptyCount) { return; }

				// 只有小于等于1000时才清空_Count，因为大于1000时它要作为HttpCache的见证
				if (n > Cache.MaxCount)
				{
					HttpRuntime.Cache.Remove(CacheKey);
				}
				else
				{
					_Count = c_nullCount;
				}
			}
		}

		/// <summary>清除缓存</summary>
		/// <remarks>执行此方法，实体缓存清空后不再自动载入，下次调用会自动重新载入</remarks>
		/// <param name="reason">清楚缓存原因</param>
		public void ClearCache(String reason)
		{
			if (_cache != null && _cache.Using) { _cache.Clear(false, reason); }

			if (_singleCache != null && _singleCache.Using) { _singleCache.Clear(reason); }

			Int64 n = _Count;
			if (n < c_emptyCount) { return; }

			// 只有小于等于1000时才清空_Count，因为大于1000时它要作为HttpCache的见证
			if (n > Cache.MaxCount)
			{
				HttpRuntime.Cache.Remove(CacheKey);
			}
			else
			{
				_Count = c_nullCount;
			}
		}

		private String CacheKey
		{
			get { return String.Format("{0}_{1}_{2}_Count", ConnName, TableName, ThisType.Name); }
		}

		private Boolean _HoldCache = CacheSetting.Alone;

		/// <summary>在数据修改时保持缓存，直到数据过期，独占数据库时默认打开，否则默认关闭</summary>
		/// <remarks>实体缓存和单对象缓存能够自动维护更新数据，保持缓存数据最新，在普通CURD中足够使用</remarks>
		public Boolean HoldCache
		{
			get { return _HoldCache; }
			set
			{
				_HoldCache = value;
				Cache.HoldCache = value;
				SingleCache.HoldCache = value;
			}
		}

		#endregion

		#region -- 事务保护 --

		/// <summary>触发脏实体会话提交事务后的缓存更新操作</summary>
		/// <param name="updateCount">实体更新操作次数</param>
		/// <param name="directExecuteSQLCount">直接执行SQL语句次数</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RaiseCommitDataChange(Int32 updateCount, Int32 directExecuteSQLCount)
		{
			DataChange("修改数据后提交事务", directExecuteSQLCount > 0, updateCount > 0, false);
		}

		/// <summary>触发脏实体会话回滚事务后的缓存更新操作</summary>
		/// <param name="updateCount">实体更新操作次数</param>
		/// <param name="directExecuteSQLCount">直接执行SQL语句次数</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RaiseRoolbackDataChange(Int32 updateCount, Int32 directExecuteSQLCount)
		{
			DataChange("修改数据后回滚事务", directExecuteSQLCount > 0, updateCount > 0, true);
		}

		#endregion

		#region -- 数据库操作 --

		/// <summary>根据条件把普通查询SQL格式化为分页SQL。</summary>
		/// <remarks>
		/// 因为需要继承重写的原因，在数据类中并不方便缓存分页SQL。
		/// 所以在这里做缓存。
		/// </remarks>
		/// <param name="builder">查询生成器</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>分页SQL</returns>
		public virtual SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			return Dal.PageSplit(builder, startRowIndex, maximumRows);
		}

		internal void DataChange(String reason, Boolean forceClearCache, Boolean isUpdateMode)
		{
			// 还在事务保护里面，不更新缓存，最后提交或者回滚的时候再更新
			// 一般事务保护用于批量更新数据，频繁删除缓存将会打来巨大的性能损耗
			// 2012-07-17 当前实体类开启的事务保护，必须由当前类结束，否则可能导致缓存数据的错乱
			if (_TransCount > 0) { return; }

			DataChange(reason, forceClearCache, isUpdateMode, false);
		}

		internal void DataChange(String reason, Boolean forceClearCache, Boolean isUpdateMode, Boolean isTransRoolback)
		{
			if (!forceClearCache)
			{
				ClearCache("{0}（F{1}-U{2}）".FormatWith(reason, forceClearCache ? 1 : 0, isUpdateMode ? 1 : 0), isUpdateMode, isTransRoolback);
			}
			else
			{
				ForceClearCache("{0}（F{1}-U{2}）".FormatWith(reason, forceClearCache ? 1 : 0, isUpdateMode ? 1 : 0), isUpdateMode);
			}

			if (_OnDataChange != null) { _OnDataChange(ThisType); }
		}

		internal Action<Type> _OnDataChange;

		/// <summary>数据改变后触发。参数指定触发该事件的实体类</summary>
		public event Action<Type> OnDataChange
		{
			add
			{
				if (value != null)
				{
					// 这里不能对委托进行弱引用，因为GC会回收委托，应该改为对对象进行弱引用
					//WeakReference<Action<Type>> w = value;

					// 弱引用事件，只会执行一次，一次以后自动取消注册
					_OnDataChange += new WeakAction<Type>(value, handler => { _OnDataChange -= handler; }, true);
				}
			}
			remove { }
		}

		#endregion

		#region -- 参数化 --

		/// <summary>创建参数</summary>
		/// <returns></returns>
		public virtual DbParameter CreateParameter()
		{
			return Dal.Db.Factory.CreateParameter();
		}

		/// <summary>格式化参数名</summary>
		/// <param name="name">名称</param>
		/// <returns></returns>
		public virtual String FormatParameterName(String name)
		{
			return Dal.Db.FormatParameterName(name);
		}

		#endregion
	}

	#region -- 脏实体会话 --

	/// <summary>脏实体会话，嵌套事务回滚时使用</summary>
	/// <remarks>http://www.newlifex.com/showtopic-1216.aspx</remarks>
	public class DirtiedEntitySession
	{
		internal Int32 ExecuteCount;// { get; set; }

		internal Int32 UpdateCount;// { get; set; }

		internal Int32 DirectExecuteSQLCount;// { get; set; }

		internal IEntitySession Session { get; set; }

		internal DirtiedEntitySession(IEntitySession session, Int32 executeCount, Int32 updateCount, Int32 directExecuteSQLCount)
		{
			Session = session;
			ExecuteCount = executeCount;
			UpdateCount = updateCount;
			DirectExecuteSQLCount = directExecuteSQLCount;
		}
	}

	#endregion
}