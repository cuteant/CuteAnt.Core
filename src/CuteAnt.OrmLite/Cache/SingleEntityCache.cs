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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Log;
using CuteAnt.Reflection;
using CuteAnt.Threading;
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.OrmLite.Cache
{
	/// <summary>单对象缓存</summary>
	/// <remarks>
	/// 用一个值为实体的字典作为缓存（键一般就是主键），适用于单表大量互相没有关系的数据。
	/// 同时，AutoSave能够让缓存项在过期时自动保存数据，该特性特别适用于点击计数等场合。
	/// </remarks>
	/// <typeparam name="TKey">主键类型</typeparam>
	/// <typeparam name="TEntity">实体类型</typeparam>
	public class SingleEntityCache<TKey, TEntity> : CacheBase<TEntity>, ISingleEntityCache
		where TEntity : Entity<TEntity>, new()
	{
		#region -- 属性 --

		private Int32 _Expriod = CacheSetting.SingleCacheExpire;

		/// <summary>过期时间。单位是秒，默认60秒</summary>
		public Int32 Expriod
		{
			get { return _Expriod; }
			set { _Expriod = value; }
		}

		private Int32 _MaxCount = 10000;

		/// <summary>最大实体数。默认10000</summary>
		public Int32 MaxCount
		{
			get { return _MaxCount; }
			set { _MaxCount = value; }
		}

		private Boolean _AutoSave = false;

		/// <summary>缓存到期时自动保存，默认false</summary>
		public Boolean AutoSave
		{
			get { return _AutoSave; }
			set { _AutoSave = value; }
		}

		private Boolean _AllowNull = false;

		/// <summary>允许缓存空对象，默认false</summary>
		public Boolean AllowNull
		{
			get { return _AllowNull; }
			set { _AllowNull = value; }
		}

		#region 主键

		//private Boolean _MasterKeyUsingUniqueField = true;

		///// <summary>单对象缓存主键是否使用实体模型唯一键（第一个标识列或者唯一的主键）</summary>
		//public Boolean MasterKeyUsingUniqueField
		//{
		//	get { return _MasterKeyUsingUniqueField; }
		//	set { _MasterKeyUsingUniqueField = value; }
		//}

		private Func<TEntity, TKey> _GetKeyMethod;

		/// <summary>获取缓存主键的方法，默认方法为获取实体主键值</summary>
		public Func<TEntity, TKey> GetKeyMethod
		{
			get
			{
				if (_GetKeyMethod == null)
				{
					var fi = Entity<TEntity>.Meta.Unique;
					if (fi != null) { _GetKeyMethod = entity => (TKey)entity[fi.Name]; }
				}
				return _GetKeyMethod;
			}
			set { _GetKeyMethod = value; }
		}

		private Func<TKey, TEntity> _FindKeyMethod;

		/// <summary>根据主键查找数据的方法</summary>
		public Func<TKey, TEntity> FindKeyMethod
		{
			get
			{
				if (_FindKeyMethod == null)
				{
					_FindKeyMethod = key => Entity<TEntity>.FindByKey(key);
					if (_FindKeyMethod == null) // 防止外部赋空
					{
						throw new ArgumentNullException("FindKeyMethod", "没有找到FindByKey方法，请先设置查找数据的方法！");
					}
				}
				return _FindKeyMethod;
			}
			set { _FindKeyMethod = value; }
		}

		#endregion

		#region 从键

		private Boolean _SlaveKeyIgnoreCase = false;

		/// <summary>从键是否区分大小写</summary>
		public Boolean SlaveKeyIgnoreCase
		{
			get { return _SlaveKeyIgnoreCase; }
			set { _SlaveKeyIgnoreCase = value; }
		}

		private Func<String, TEntity> _FindSlaveKeyMethod;

		/// <summary>根据从键查找数据的方法</summary>
		public Func<String, TEntity> FindSlaveKeyMethod
		{
			get { return _FindSlaveKeyMethod; }
			set { _FindSlaveKeyMethod = value; }
		}

		private Func<TEntity, String> _GetSlaveKeyMethod;

		/// <summary>获取缓存从键的方法，默认为空</summary>
		public Func<TEntity, String> GetSlaveKeyMethod
		{
			get { return _GetSlaveKeyMethod; }
			set { _GetSlaveKeyMethod = value; }
		}

		#endregion

		private Action _InitializeMethod;

		/// <summary>初始化缓存的方法，默认为空</summary>
		public Action InitializeMethod
		{
			get { return _InitializeMethod; }
			set { _InitializeMethod = value; }
		}

		private Boolean _HoldCache = CacheSetting.Alone;

		/// <summary>在数据修改时保持缓存，不再过期，独占数据库时默认打开，否则默认关闭</summary>
		public Boolean HoldCache
		{
			get { return _HoldCache; }
			set { _HoldCache = value; }
		}

		private Boolean _Using = false;

		/// <summary>是否在使用缓存</summary>
		internal Boolean Using
		{
			get { return _Using; }
			private set { _Using = value; }
		}

		#endregion

		#region -- 构造、检查过期缓存 --

		private TimerX _Timer = null;

		/// <summary>实例化一个实体缓存</summary>
		public SingleEntityCache()
		{
			// 启动一个定时器，用于定时清理过期缓存。因为比较耗时，最后一个参数采用线程池
			_Timer = new TimerX(d => Check(), null, Expriod * 1000, Expriod * 1000, true);
		}

		/// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
		/// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
		/// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
		protected override void OnDispose(Boolean disposing)
		{
			base.OnDispose(disposing);

			try
			{
				Clear("资源释放");
			}
			catch (Exception ex)
			{
				DAL.WriteLog(ex);
			}

			if (_Timer != null) { _Timer.Dispose(); }
		}

		/// <summary>定期检查实体，如果过期，则触发保存</summary>
		private void Check()
		{
			var hold = HoldCache;
			var autoSave = AutoSave;
			// 独占缓存不删除缓存，仅判断自动保存
			if (hold && !autoSave) { return; }

			var entities = Entities;
			if (entities.Count <= 0) { return; }

			IEnumerable<CacheItem> cs = null; ;
			if (autoSave)
			{
				// 是否过期
				// 单对象缓存每次缓存的时候，设定一个将来的过期时间，然后以后只需要比较过期时间和当前时间就可以了
				cs = entities.Where(e => e.Value != null && e.Value.Expired).Select(e => e.Value);
				if (cs == null || !cs.Any()) { return; }
				foreach (var item in cs)
				{
					if (item.NextSave <= DateTime.Now)
					{
						// 捕获异常，不影响别人
						try
						{
							// 需要在原连接名表名里面更新对象
							AutoUpdate(item, "定时检查过期");
						}
						catch { }
					}
				}
			}

			// 独占缓存不删除缓存
			if (hold) { return; }

			if (null == cs) { cs = entities.Where(e => e.Value != null && e.Value.Expired).Select(e => e.Value); }
			var slaveEntities = SlaveEntities;
			foreach (var c in cs)
			{
				if (c == null) { continue; }

				CacheItem item;
				entities.TryRemove(c.Key, out item);
				if (!c.SlaveKey.IsNullOrWhiteSpace()) { slaveEntities.TryRemove(c.SlaveKey, out item); }

				c.Entity = null;
			}

			Using = entities.Count > 0;
		}

		#endregion

		#region -- 缓存对象 --

		/// <summary>缓存对象</summary>
		private class CacheItem
		{
			internal SingleEntityCache<TKey, TEntity> sc;

			/// <summary>键</summary>
			internal TKey Key;

			/// <summary>从键</summary>
			internal String SlaveKey;

			/// <summary>实体</summary>
			internal TEntity Entity;

			/// <summary>缓存过期时间</summary>
			private DateTime _ExpiredTime;
			///// <summary>缓存过期时间</summary>
			//private DateTime ExpiredTime { get { return _ExpiredTime; } set { _ExpiredTime = value; NextSave = value; } }

			/// <summary>下一次保存的时间</summary>
			internal DateTime NextSave;

			/// <summary>是否已经过期</summary>
			internal Boolean Expired { get { return _ExpiredTime <= DateTime.Now; } }

			internal void SetEntity(TEntity entity)
			{
				// 如果原来有对象，则需要自动保存
				if (Entity != null && Entity != entity) { sc.AutoUpdate(this, "设置新的缓存对象"); }

				Entity = entity;
				//ExpiredTime = DateTime.Now.AddSeconds(sc.Expriod);
				_ExpiredTime = NextSave = DateTime.Now.AddSeconds(sc.Expriod);
			}
		}

		#endregion

		#region -- 单对象缓存 --

		// The default capacity, i.e. the initial # of buckets. When choosing this value, we are making
		// a trade-off between the size of a very small dictionary, and the number of resizes when
		// constructing a large dictionary. Also, the capacity should not be divisible by a small prime.
		private const int DEFAULT_CAPACITY = 31;

		// The default concurrency level is DEFAULT_CONCURRENCY_MULTIPLIER * #CPUs. The higher the
		// DEFAULT_CONCURRENCY_MULTIPLIER, the more concurrent writes can take place without interference
		// and blocking, but also the more expensive operations that require all locks become (e.g. table
		// resizing, ToArray, Count, etc). According to brief benchmarks that we ran, 4 seems like a good
		// compromise.
		private const Int32 DEFAULT_CONCURRENCY_MULTIPLIER = 4;

		/// <summary>The number of concurrent writes for which to optimize by default.</summary>
		private static Int32 DefaultConcurrencyLevel
		{
			get { return DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount; }
		}

		//private SortedList<TKey, CacheItem> _Entities;
		//! Dictionary在集合方面具有较好查找性能，直接用字段，提高可能的性能
		private ConcurrentDictionary<TKey, CacheItem> _Entities;
		/// <summary>单对象缓存，主键查询使用</summary>
		private ConcurrentDictionary<TKey, CacheItem> Entities
		{
			get
			{
				if (_Entities == null)
				{
					//ConcurrentDictionary<String, CacheItem> dic;
					//if (KeyIgnoreCase)
					//{
					//	dic = new ConcurrentDictionary<String, CacheItem>(StringComparer.OrdinalIgnoreCase);
					//}
					//else
					//{
					var dic = new ConcurrentDictionary<TKey, CacheItem>();
					//}
					if (Interlocked.CompareExchange<ConcurrentDictionary<TKey, CacheItem>>(ref _Entities, dic, null) != null)
					{
						dic = null;
					}
				}
				return _Entities;
			}
		}

		private ConcurrentDictionary<String, CacheItem> _SlaveEntities;
		/// <summary>单对象缓存，从键查询使用</summary>
		private ConcurrentDictionary<String, CacheItem> SlaveEntities
		{
			get
			{
				if (_SlaveEntities == null)
				{
					ConcurrentDictionary<String, CacheItem> dic;
					if (SlaveKeyIgnoreCase)
					{
						dic = new ConcurrentDictionary<String, CacheItem>(StringComparer.OrdinalIgnoreCase);
					}
					else
					{
						dic = new ConcurrentDictionary<String, CacheItem>();
					}
					if (Interlocked.CompareExchange<ConcurrentDictionary<String, CacheItem>>(ref _SlaveEntities, dic, null) != null)
					{
						dic = null;
					}
				}
				return _SlaveEntities;
			}
		}

		#endregion

		#region -- 统计 --

		/// <summary>总次数</summary>
		public Int32 Total;

		/// <summary>命中</summary>
		public Int32 Shoot;

		/// <summary>第一次命中，加锁之前</summary>
		public Int32 Shoot1;

		/// <summary>第二次命中，加锁之后</summary>
		public Int32 Shoot2;

		/// <summary>无效次数，不允许空但是查到对象又为空</summary>
		public Int32 Invalid;

		/// <summary>下一次显示时间</summary>
		public DateTime NextShow;

		/// <summary>显示统计信息</summary>
		public void ShowStatics()
		{
			if (Total > 0)
			{
				var sb = new StringBuilder();
				var name = "<{0},{1}>({2})".FormatWith(typeof(TKey).Name, typeof(TEntity).Name, Entities.Count);
				sb.AppendFormat("单对象缓存{0,-20}", name);
				sb.AppendFormat("总次数{0,7:n0}", Total);
				if (Shoot > 0) { sb.AppendFormat("，命中{0,7:n0}（{1,6:P02}）", Shoot, (Double)Shoot / Total); }
				// 一级命中和总命中相等时不显示
				if (Shoot1 > 0 && Shoot1 != Shoot) { sb.AppendFormat("，一级命中{0,7:n0}（{1,6:P02}）", Shoot1, (Double)Shoot1 / Total); }
				if (Shoot2 > 0) { sb.AppendFormat("，二级命中{0}（{1,6:P02}）", Shoot2, (Double)Shoot2 / Total); }
				if (Invalid > 0) { sb.AppendFormat("，无效次数{0}（{1,6:P02}）", Invalid, (Double)Invalid / Total); }

				DAL.Logger.LogInformation(sb.ToString());
			}
		}

		#endregion

		#region -- 获取数据 --

		#region - 批量主键获取 -

		/// <summary>根据主键获取实体记录列表</summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public EntityList<TEntity> FindAllInKeys<T>(IEnumerable<T> keys)
		{
			if (keys.IsNullOrEmpty()) { return new EntityList<TEntity>(); }

			var type = typeof(TKey);
			var skeys = keys.Select(e => (TKey)e.ChangeType(type)).ToList();
			return FindAllInKeys(skeys);
		}

		/// <summary>根据主键获取实体记录列表</summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public EntityList<TEntity> FindAllInKeys(ICollection<TKey> keys)
		{
			if (keys.IsNullOrEmpty()) { return new EntityList<TEntity>(); }

			var list = new EntityList<TEntity>(keys.Count);
			foreach (var key in keys)
			{
				var entity = GetItem(key);
				if (entity != null) { list.Add(entity); }
			}
			return list;
		}

		/// <summary>根据主键获取实体记录列表</summary>
		/// <typeparam name="T">主键原始类型</typeparam>
		/// <param name="keys">主键字符串，以逗号或分号分割</param>
		/// <returns></returns>
		public EntityList<TEntity> FindAllInKeys<T>(String keys)
		{
			if (keys.IsNullOrWhiteSpace()) { return new EntityList<TEntity>(); }

			var srctype = typeof(T);
			var desttype = typeof(TKey);
			var kvs = keys.Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (srctype == typeof(String))
			{
				return FindAllInKeys(kvs.Select(e => (TKey)e.ChangeType(desttype)).ToList());
			}
			else
			{
				return FindAllInKeys(kvs.Select(e => (T)e.ChangeType(srctype)).Select(e => (TKey)e.ChangeType(desttype)).ToList());
			}
		}

		#endregion

		#region - 批量从键获取 -

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		public EntityList<TEntity> FindAllInSlaveKeys<T>(IEnumerable<T> slavekeys)
		{
			if (slavekeys.IsNullOrEmpty()) { return new EntityList<TEntity>(); }

			var skeys = slavekeys.Select(e => "" + e).ToList();
			return FindAllInSlaveKeys(skeys);
		}

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		public EntityList<TEntity> FindAllInSlaveKeys(ICollection<String> slavekeys)
		{
			if (slavekeys.IsNullOrEmpty()) { return new EntityList<TEntity>(); }

			var list = new EntityList<TEntity>(slavekeys.Count);
			if (slavekeys != null)
			{
				foreach (var key in slavekeys)
				{
					var entity = GetItemWithSlaveKey(key);
					if (entity != null) { list.Add(entity); }
				}
			}
			return list;
		}

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		public EntityList<TEntity> FindAllInSlaveKeys(String slavekeys)
		{
			return FindAllInSlaveKeys(slavekeys.SplitDefaultSeparator());
		}

		#endregion

		#region - 获取实体对象 -

		/// <summary>根据主键获取实体数据</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TEntity this[TKey key]
		{
			get { return GetItem(key); }
			set { TryAdd(key, value); }
		}

		/// <summary>根据从键获取实体数据</summary>
		/// <param name="slaveKey"></param>
		/// <returns></returns>
		public TEntity GetItemWithSlaveKey<T>(T slaveKey)
		{
			return GetItemWithSlaveKey("" + slaveKey);
		}

		/// <summary>根据从键获取实体数据</summary>
		/// <param name="slaveKey"></param>
		/// <returns></returns>
		public TEntity GetItemWithSlaveKey(String slaveKey)
		{
			return GetItem(SlaveEntities, slaveKey, false);
		}

		private TEntity GetItem(TKey key)
		{
			return GetItem(Entities, key, true);
		}

		private TEntity GetItem<TKey2>(ConcurrentDictionary<TKey2, CacheItem> entities, TKey2 key, Boolean masterCache)
		{
			// 为空的key，直接返回null，不进行缓存查找
			if (key == null) { return null; }
			//var skey = key as String;
			//if (skey != null && skey.Length == 0) { return null; }

			// 更新统计信息
			HmCache.CheckShowStatics(ref NextShow, ref Total, ShowStatics);

			// 如果找到项，返回
			CacheItem item = null;
			if (entities.TryGetValue(key, out item))
			{
				Interlocked.Increment(ref Shoot1);
				// 下面的GetData里会判断过期并处理
				return GetData(item);
			}

			// 队列满时，移除最老的一个
			while (entities.Count >= MaxCount) { RemoveFirst(); }

			// 如果找到项，返回
			if (entities.TryGetValue(key, out item))
			{
				Interlocked.Increment(ref Shoot2);
				return GetData(item);
			}

			#region 新增

			// TODO 未能阻止多线程做重复查询
			// 开始更新数据，然后加入缓存
			TEntity entity = null;
			if (masterCache)
			{
				entity = Invoke(FindKeyMethod, (TKey)(Object)key);
			}
			else
			{
				entity = Invoke(FindSlaveKeyMethod, key + "");
			}
			if (entity == null && !AllowNull)
			{
				Interlocked.Increment(ref Invalid);
			}
			else
			{
				TryAdd(entity);
			}

			return entity;

			#endregion
		}

		#endregion

		#endregion

		#region -- 方法 --

		#region - 初始化 -

		/// <summary>初始化单对象缓存，服务端启动时预载入实体记录集</summary>
		/// <remarks>注意事项：
		/// <para>调用方式：TEntity.Meta.Factory.Session.SingleCache.Initialize()，不要使用TEntity.Meta.Session.SingleCache.Initialize()；
		/// 因为Factory的调用会联级触发静态构造函数，确保单对象缓存设置成功</para>
		/// <para>服务端启动时，如果使用异步方式初始化单对象缓存，请将同一数据模型（ConnName）下的实体类型放在同一异步方法内执行，否则实体类型的架构检查抛异常</para>
		/// </remarks>
		public void Initialize()
		{
			if (HoldCache && InitializeMethod != null) { InitializeMethod(); }
		}

		#endregion

		#region - 是否包含指定键 -

		#region 主键

		/// <summary>是否包含指定主键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Boolean ContainsKey(TKey key)
		{
			return Entities.ContainsKey(key);
		}

		#endregion

		#region 从键

		/// <summary>是否包含指定从键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Boolean ContainsSlaveKey(Int32 key)
		{
			return SlaveEntities.ContainsKey("" + key);
		}

		/// <summary>是否包含指定从键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Boolean ContainsSlaveKey(Int64 key)
		{
			return SlaveEntities.ContainsKey("" + key);
		}

		/// <summary>是否包含指定从键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Boolean ContainsSlaveKey(String key)
		{
			return SlaveEntities.ContainsKey(key);
		}

		#endregion

		#endregion

		#region - 添加 -

		/// <summary>向单对象缓存添加项</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		internal void AddOrUpdate(TEntity entity)
		{
			if (entity == null) { return; }

			//// 加入缓存的实体对象，需要标记来自数据库
			//entity.MarkDb(true);
			CacheItem item;
			var key = GetKeyMethod(entity);
			if (Entities.TryGetValue(key, out item))
			{
				// 未过期，也替换保存
				item.SetEntity(entity);
			}
			else
			{
				TryAdd(entity);
			}
		}

		/// <summary>根据主键向单对象缓存添加项</summary>
		/// <param name="key">主键</param>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		public Boolean TryAdd(TKey key, TEntity entity)
		{
			// 如果找到项，返回
			CacheItem item = null;
			if (Entities.TryGetValue(key, out item))
			{
				if (!item.Expired) { return false; }

				item.SetEntity(entity);
				return false;
			}
			else
			{
				return TryAdd(entity);
			}
		}

		/// <summary>尝试向两个字典加入数据</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		private Boolean TryAdd(TEntity entity)
		{
			if (!Using)
			{
				Using = true;
				if (Debug) { DAL.WriteLog("单对象缓存首次使用 {0} {1}", typeof(TEntity).FullName, HmTrace.GetCaller(1, 16)); }
			}

			var item = new CacheItem { sc = this };

			String slaveKey = null;
			if (GetSlaveKeyMethod != null) { slaveKey = GetSlaveKeyMethod(entity); }

			var mkey = GetKeyMethod(entity);
			item.Key = mkey;
			item.SlaveKey = slaveKey;
			item.SetEntity(entity);

			var success = Entities.TryAdd(mkey, item);
			if (success && !slaveKey.IsNullOrWhiteSpace())
			{
				// 新增或更新
				SlaveEntities.AddOrUpdate(slaveKey, item, (k, v) => item);
			}
			return success;
		}

		#endregion

		#region - 删除 -

		/// <summary>根据主键移除指定项</summary>
		/// <param name="key"></param>
		public void RemoveKey(TKey key)
		{
			RemoveKey(key, true);
		}

		/// <summary>移除指定项</summary>
		/// <param name="key">键值</param>
		/// <param name="save">是否自动保存实体对象</param>
		public void RemoveKey(TKey key, Boolean save)
		{
			CacheItem item;
			var entities = Entities;
			var slaveentities = SlaveEntities;
			if (entities.TryRemove(key, out item))
			{
				if (save) { AutoUpdate(item, "移除缓存" + key); }
				var slavekey = item.SlaveKey;
				if (!slavekey.IsNullOrWhiteSpace()) { slaveentities.TryRemove(slavekey, out item); }
			}
			Using = entities.Count > 0;
		}

		/// <summary>根据主键移除指定项</summary>
		/// <param name="entity"></param>
		public void Remove(TEntity entity)
		{
			Remove(entity, true);
		}

		/// <summary>根据主键移除指定项</summary>
		/// <param name="entity"></param>
		/// <param name="save">是否自动保存实体对象</param>
		public void Remove(TEntity entity, Boolean save)
		{
			if (entity == null) { return; }
			var key = GetKeyMethod(entity);
			RemoveKey(key, save);
		}

		/// <summary>清除所有缓存数据</summary>
		/// <param name="reason">清除缓存原因</param>
		public void Clear(String reason = null)
		{
			if (Debug) { DAL.WriteLog("清空单对象缓存：{0} 原因：{1}", typeof(TEntity).FullName, reason); }

			if (AutoSave)
			{
				try
				{
					var cacheentityes = Entities.Where(e => e.Value != null && e.Value.Entity != null).Select(e => e.Value);
					foreach (var item in cacheentityes)
					{
						AutoUpdate(item, "清空缓存 " + reason);
					}
				}
				catch (Exception ex) { DAL.WriteLog(ex); }
			}

			Entities.Clear();
			SlaveEntities.Clear();

			Using = false;
		}

		#endregion

		#region * GetData *

		/// <summary>内部处理返回对象。
		/// 把对象传进来，而不是只传键值然后查找，是为了避免别的线程移除该项
		/// </summary>
		/// <remarks>此方法只做更新操作，不再进行缓存新增操作</remarks>
		/// <param name="item"></param>
		/// <returns></returns>
		private TEntity GetData(CacheItem item)
		{
			// 未过期，直接返回
			//if (HoldCache || item.ExpiredTime > DateTime.Now)
			// 这里不能判断独占缓存，否则将失去自动保存的机会
			if (!item.Expired)
			{
				Interlocked.Increment(ref Shoot);
				return item.Entity;
			}

			// 自动保存
			AutoUpdate(item, "获取缓存过期");

			// 判断别的线程是否已更新
			if (HoldCache || !item.Expired) { return item.Entity; }

			// 更新过期缓存，在原连接名表名里面获取
			var entity = Invoke(FindKeyMethod, item.Key);
			if (entity != null || AllowNull)
			{
				item.SetEntity(entity);
			}
			else
			{
				Interlocked.Increment(ref Invalid);
			}

			return entity;
		}

		#endregion

		#region * RemoveFirst *

		/// <summary>移除第一个缓存项</summary>
		private void RemoveFirst()
		{
			var entities = Entities;
			if (entities.Count <= 0) { return; }

			try
			{
				var first = entities.First();
				if (Debug) { DAL.WriteLog("单实体缓存{0}超过最大数量限制{1}，准备移除第一项{2}", typeof(TEntity).FullName, MaxCount, first.Key); }
				var cacheitem = first.Value;
				CacheItem item;
				if (entities.TryRemove(cacheitem.Key, out item))
				{
					//自动保存
					AutoUpdate(item, "缓存达到最大数移除第一项");
				}
				if (!cacheitem.SlaveKey.IsNullOrWhiteSpace())
				{
					SlaveEntities.TryRemove(cacheitem.SlaveKey, out item);
				}
			}
			catch { }
		}

		#endregion

		#region * AutoUpdate *

		/// <summary>自动更新，最主要是在原连接名和表名里面更新对象</summary>
		/// <param name="item"></param>
		/// <param name="reason"></param>
		private void AutoUpdate(CacheItem item, String reason)
		{
			if (AutoSave && item != null && item.Entity != null)
			{
				item.NextSave = DateTime.Now.AddSeconds(Expriod);
				Invoke<CacheItem, Object>(e =>
				{
					var rs = e.Entity.Update();

					if (Debug && rs > 0)
					{
						DAL.WriteLog("单对象缓存AutoSave {0}/{1} {2}", Entity<TEntity>.Meta.TableName, Entity<TEntity>.Meta.ConnName, reason);
					}

					return null;
				}, item);
			}
		}

		#endregion

		#endregion

		#region -- ISingleEntityCache 成员 --

		#region - 批量主键获取 -

		/// <summary>根据主键获取实体记录列表</summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEntityList ISingleEntityCache.FindAllInKeys<T>(IEnumerable<T> keys)
		{
			return FindAllInKeys<T>(keys);
		}

		///// <summary>根据主键获取实体记录列表</summary>
		///// <param name="keys"></param>
		///// <returns></returns>
		//IEntityList ISingleEntityCache.FindAllInKeys(IEnumerable<Int64> keys)
		//{
		//	return FindAllInKeys(keys);
		//}

		///// <summary>根据主键获取实体记录列表</summary>
		///// <param name="keys"></param>
		///// <returns></returns>
		//IEntityList ISingleEntityCache.FindAllInKeys(IEnumerable<Decimal> keys)
		//{
		//	return FindAllInKeys(keys);
		//}

		///// <summary>根据主键获取实体记录列表</summary>
		///// <param name="keys"></param>
		///// <returns></returns>
		//IEntityList ISingleEntityCache.FindAllInKeys(IEnumerable<Guid> keys)
		//{
		//	return FindAllInKeys(keys);
		//}

		///// <summary>根据主键获取实体记录列表</summary>
		///// <param name="keys"></param>
		///// <returns></returns>
		//IEntityList ISingleEntityCache.FindAllInKeys(IEnumerable<CombGuid> keys)
		//{
		//	return FindAllInKeys(keys);
		//}

		///// <summary>根据主键获取实体记录列表</summary>
		///// <param name="keys"></param>
		///// <returns></returns>
		//IEntityList ISingleEntityCache.FindAllInKeys(IEnumerable<Object> keys)
		//{
		//	return FindAllInKeys(keys);
		//}

		/// <summary>根据主键获取实体记录列表</summary>
		/// <typeparam name="T">主键原始类型</typeparam>
		/// <param name="keys">主键字符串，以逗号或分号分割</param>
		/// <returns></returns>
		IEntityList ISingleEntityCache.FindAllInKeys<T>(String keys)
		{
			return FindAllInKeys<T>(keys);
		}

		#endregion

		#region - 批量从键获取 -

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		IEntityList ISingleEntityCache.FindAllInSlaveKeys<T>(IEnumerable<T> slavekeys)
		{
			return FindAllInSlaveKeys<T>(slavekeys);
		}

		///// <summary>根据从键获取实体记录列表</summary>
		///// <param name="slavekeys"></param>
		///// <returns></returns>
		//IEntityList ISingleEntityCache.FindAllInSlaveKeys(IEnumerable<Int64> slavekeys)
		//{
		//	return FindAllInSlaveKeys(slavekeys);
		//}

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		IEntityList ISingleEntityCache.FindAllInSlaveKeys(ICollection<String> slavekeys)
		{
			return FindAllInSlaveKeys(slavekeys);
		}

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		IEntityList ISingleEntityCache.FindAllInSlaveKeys(String slavekeys)
		{
			return FindAllInSlaveKeys(slavekeys);
		}

		#endregion

		#region - 获取数据 -

		/// <summary>获取数据</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IEntity ISingleEntityCache.this[Object key]
		{
			get { return GetItem((TKey)key); }
		}

		/// <summary>根据从键获取实体数据</summary>
		/// <param name="slaveKey"></param>
		/// <returns></returns>
		IEntity ISingleEntityCache.GetItemWithSlaveKey<T>(T slaveKey)
		{
			return GetItemWithSlaveKey("" + slaveKey);
		}

		///// <summary>根据从键获取实体数据</summary>
		///// <param name="slaveKey"></param>
		///// <returns></returns>
		//IEntity ISingleEntityCache.GetItemWithSlaveKey(Int64 slaveKey)
		//{
		//	return GetItemWithSlaveKey("" + slaveKey);
		//}

		/// <summary>根据从键获取实体数据</summary>
		/// <param name="slaveKey"></param>
		/// <returns></returns>
		IEntity ISingleEntityCache.GetItemWithSlaveKey(String slaveKey)
		{
			return GetItemWithSlaveKey(slaveKey);
		}

		#endregion

		/// <summary>是否包含指定主键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		Boolean ISingleEntityCache.ContainsKey(Object key)
		{
			return ContainsKey((TKey)key);
		}

		/// <summary>移除指定项</summary>
		/// <param name="key"></param>
		void ISingleEntityCache.RemoveKey(Object key)
		{
			RemoveKey((TKey)key);
		}

		/// <summary>移除指定项</summary>
		/// <param name="key"></param>
		/// <param name="save">是否自动保存实体对象</param>
		void ISingleEntityCache.RemoveKey(Object key, Boolean save)
		{
			RemoveKey((TKey)key, save);
		}

		/// <summary>移除指定项</summary>
		/// <param name="entity"></param>
		void ISingleEntityCache.Remove(IEntity entity)
		{
			Remove(entity as TEntity);
		}

		/// <summary>移除指定项</summary>
		/// <param name="entity"></param>
		/// <param name="save">是否自动保存实体对象</param>
		void ISingleEntityCache.Remove(IEntity entity, Boolean save)
		{
			Remove(entity as TEntity, save);
		}

		/// <summary>向单对象缓存添加项</summary>
		/// <param name="key"></param>
		/// <param name="value">实体对象</param>
		/// <returns></returns>
		Boolean ISingleEntityCache.Add(Object key, IEntity value)
		{
			var entity = value as TEntity;
			if (entity == null) { return false; }
			return TryAdd((TKey)key, entity);
		}

		/// <summary>向单对象缓存添加项</summary>
		/// <param name="value">实体对象</param>
		/// <returns></returns>
		Boolean ISingleEntityCache.Add(IEntity value)
		{
			var entity = value as TEntity;
			if (entity == null) { return false; }
			var key = GetKeyMethod(entity);
			return TryAdd(key, entity);
		}

		#endregion

		#region -- 辅助 --

		internal SingleEntityCache<TKey, TEntity> CopySettingFrom(SingleEntityCache<TKey, TEntity> ec)
		{
			this.Expriod = ec.Expriod;
			this.MaxCount = ec.MaxCount;
			this.AutoSave = ec.AutoSave;
			this.AllowNull = ec.AllowNull;
			this.HoldCache = ec.HoldCache;

			//this.MasterKeyUsingUniqueField = ec.MasterKeyUsingUniqueField;
			//this.KeyIgnoreCase = ec.KeyIgnoreCase;
			this.GetKeyMethod = ec.GetKeyMethod;
			this.FindKeyMethod = ec.FindKeyMethod;

			this.SlaveKeyIgnoreCase = ec.SlaveKeyIgnoreCase;
			this.GetSlaveKeyMethod = ec.GetSlaveKeyMethod;
			this.FindSlaveKeyMethod = ec.FindSlaveKeyMethod;

			this.InitializeMethod = ec.InitializeMethod;

			return this;
		}

		#endregion
	}
}