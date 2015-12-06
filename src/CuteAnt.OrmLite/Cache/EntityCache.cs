/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using CuteAnt.Log;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite.Cache
{
	/// <summary>实体缓存</summary>
	/// <remarks>关于异步缓存，非常有用！
	/// 第一次读取缓存的时候，同步从数据库读取，这样子手上有一份数据。
	/// 以后更新，都开异步线程去读取，而当前马上返回，让大家继续用着旧数据，这么做性能非常好。</remarks>
	/// <typeparam name="TEntity">实体类型</typeparam>
	public class EntityCache<TEntity> : CacheBase<TEntity>, IEntityCache where TEntity : Entity<TEntity>, new()
	{
		#region -- 基础属性 --

		private DateTime _ExpiredTime;
		/// <summary>缓存过期时间</summary>
		private DateTime ExpiredTime { get { return _ExpiredTime; } set { _ExpiredTime = value; } }

		private const Int32 c_nullTimes = -1;
		/// <summary>缓存更新次数</summary>
		private Int32 Times = c_nullTimes;

		private Int32 _Expriod = CacheSetting.EntityCacheExpire;
		/// <summary>过期时间。单位是秒，默认60秒</summary>
		public Int32 Expriod { get { return _Expriod; } set { _Expriod = value; } }

		private Int32 _MaxCount = CacheSetting.EntityCacheMaxCount;
		/// <summary>实体缓存最大记录数，默认1000条记录；当实体记录总数超过这个阈值，系统会自动清空实体缓存。</summary>
		public Int32 MaxCount { get { return _MaxCount; } set { _MaxCount = value; } }

		private Boolean _HoldCache = CacheSetting.Alone;
		/// <summary>在数据修改时保持缓存，不再过期，独占数据库时默认打开，否则默认关闭</summary>
		public Boolean HoldCache { get { return _HoldCache; } set { _HoldCache = value; } }

		private Boolean _Asynchronous = true;
		/// <summary>异步更新，独占数据库时默认打开</summary>
		public Boolean Asynchronous { get { return _Asynchronous; } set { _Asynchronous = value; } }

		private const Int32 c_on = 1;
		private const Int32 c_zero = 0;
		private Int32 _Using = c_zero;

		/// <summary>是否在使用缓存，在不触发缓存动作的情况下检查是否有使用缓存</summary>
		public Boolean Using { get { return _Using != c_zero; } }

		/// <summary>当前获得锁的线程</summary>
		private Int32 m_thread = c_zero;

		#endregion

		#region -- 缓存核心 --

		private Dictionary<Object, TEntity> _dictEntities = new Dictionary<Object, TEntity>();
		private EntityList<TEntity> _Entities = new EntityList<TEntity>();

		/// <summary>实体集合。无数据返回空集合而不是null</summary>
		public EntityList<TEntity> Entities
		{
			get
			{
				// 更新统计信息
				HmCache.CheckShowStatics(ref NextShow, ref Total, ShowStatics);

				// 只要访问了实体缓存数据集合，就认为是使用了实体缓存，允许更新缓存数据期间向缓存集合添删数据
				Interlocked.CompareExchange(ref _Using, c_on, c_zero);

				// 两种情况更新缓存：1，缓存过期；2，不允许空但是集合又是空
				Boolean nodata = _Entities.Count == c_zero;

				// 独占模式下，缓存不再过期
				if (HoldCache && !nodata)
				{
					Interlocked.Increment(ref Shoot1);
					return _Entities;
				}

				if (nodata || DateTime.Now >= ExpiredTime)
				{
					// 为了确保缓存数据有效可用，这里必须加锁，保证第一个线程更新拿到数据之前其它线程全部排队
					// 即使打开了异步更新，首次读取数据也是同步
					// 这里特别要注意，第一个线程取得锁以后，如果因为设计失误，导致重复进入缓存，这是设计错误

					//!!! 所有加锁的地方都务必消息，同一个线程可以重入同一个锁
					//if (_thread == TaskShim.CurrentManagedThreadId) throw new XCodeException("设计错误！当前线程正在获取缓存，在完成之前，本线程不应该使用实体缓存！");
					// 同一个线程重入查询实体缓存时，直接返回已有缓存或者空，这符合一般设计逻辑
					if (m_thread == TaskShim.CurrentManagedThreadId) { return _Entities; }

					lock (this)
					{
						m_thread = TaskShim.CurrentManagedThreadId;

						nodata = _Entities.Count == c_zero;
						if (nodata || DateTime.Now >= ExpiredTime)
						{
							UpdateCache(nodata);
						}
						else
						{
							Interlocked.Increment(ref Shoot2);
						}

						m_thread = c_zero;
					}
				}
				else
				{
					Interlocked.Increment(ref Shoot1);
				}

				return _Entities;
			}
		}

		private Func<EntityList<TEntity>> _FillListMethod;

		/// <summary>填充数据的方法</summary>
		public Func<EntityList<TEntity>> FillListMethod
		{
			get
			{
				if (_FillListMethod == null)
				{
					_FillListMethod = Entity<TEntity>.FindAllWithLockToken;
				}
				return _FillListMethod;
			}
			set { _FillListMethod = value; }
		}

		#endregion

		#region -- 缓存操作 --

		private void UpdateCache(Boolean nodata)
		{
			// 异步更新时，如果为空，表明首次，同步获取数据
			// 有且仅有非首次且数据不为空时执行异步查询
			if (Times > c_nullTimes && Asynchronous && !nodata)
			{
				// 这里直接计算有效期，避免每次判断缓存有效期时进行的时间相加而带来的性能损耗
				// 设置时间放在获取缓存之前，让其它线程不要空等
				ExpiredTime = DateTime.Now.AddSeconds(Expriod);

				if (Debug)
				{
					var reason = Times >= c_zero ? (nodata ? "无缓存数据" : Expriod + "秒过期") : "第一次";
					DAL.WriteLog("异步更新实体缓存（第{2}次）：{0} 原因：{1} {3}", typeof(TEntity).FullName, reason, Times, HmTrace.GetCaller(3, 16));
				}
				if (Int32.MaxValue == Times) { Times = c_zero; }
				Times++;

				var taskFactory = Task.Factory;
				taskFactory.StartNew(FillWaper, Times, taskFactory.CancellationToken, AsyncUtils.GetCreationOptions(taskFactory.CreationOptions), taskFactory.Scheduler ?? TaskScheduler.Default);
				//ThreadPoolX.QueueUserWorkItem(FillWaper, Times);
			}
			else
			{
				if (Debug)
				{
					var reason = Times >= c_zero ? (nodata ? "无缓存数据" : Expriod + "秒过期") : "第一次";
					DAL.WriteLog("更新实体缓存（第{2}次）：{0} 原因：{1} {3}", typeof(TEntity).FullName, reason, Times, HmTrace.GetCaller(3, 16));
				}
				if (Int32.MaxValue == Times) { Times = c_zero; }
				Times++;

				FillWaper(Times);

				// 这里直接计算有效期，避免每次判断缓存有效期时进行的时间相加而带来的性能损耗
				// 设置时间放在获取缓存之后，避免缓存尚未拿到，其它线程拿到空数据
				ExpiredTime = DateTime.Now.AddSeconds(Expriod);
			}
		}

		private void FillWaper(Object state)
		{
			_Entities = Invoke<Object, EntityList<TEntity>>(s => FillListMethod(), null);
			if (s_uniqueField != null) { _dictEntities = _Entities.ToList().ToDictionary(e => e[s_uniqueField.Name]); }
			if (Debug) { DAL.WriteLog("完成更新缓存（第{1}次）：{0}", typeof(TEntity).FullName, state); }
		}

		/// <summary>清除缓存</summary>
		/// <param name="reloading">是否重新载入缓存，如果为否，则直接清空缓存不再读取数据库。</param>
		/// <param name="reason">清楚缓存原因</param>
		public void Clear(Boolean reloading, String reason = null)
		{
			lock (this)
			{
				if (_Entities.Count > c_zero && Debug)
				{
					DAL.WriteLog("清空实体缓存：{0} 原因：{1}", typeof(TEntity).FullName, reason);
				}

				if (reloading)
				{
					// 使用异步时，马上打开异步查询更新数据
					if (Asynchronous && _Entities.Count > c_zero)
					{
						UpdateCache(false);
					}
					else
					{
						// 修改为最小，确保过期
						ExpiredTime = DateTime.MinValue;
					}
				}
				else
				{
					_Entities.Clear();
					_dictEntities.Clear();
					// 修改为最小，确保过期
					ExpiredTime = DateTime.MinValue;
				}

				// 清空后，表示不使用缓存
				Interlocked.CompareExchange(ref _Using, c_zero, c_on);
			}
		}

		private static readonly FieldItem s_uniqueField = Entity<TEntity>.Meta.Factory.Unique;
		internal void Update(TEntity entity)
		{
			// 正在更新当前缓存，跳过
			//if (!Using || _thread > c_zero || _Entities == null) return;
			if (!Using) { return; }

			// 尽管用了事务保护，但是仍然可能有别的地方导致实体缓存更新，这点务必要注意
			#region ## 苦竹 修改 ##
			//var fi = Operate.Unique;
			//var e = UniqueField != null ? _Entities.Find(fi.Name, entity[fi.Name]) : null;
			//if (e != null)
			//{
			//	//if (e != entity)
			//	//{
			//	//	e.CopyFrom(entity);
			//	//	// 缓存中实体对象也需要清空脏数据
			//	//	e.Dirtys.Clear();
			//	//}
			//	// 更新实体缓存时，不做拷贝，避免产生脏数据，如果恰巧又使用单对象缓存，那会导致自动保存
			//	lock (_Entities)
			//	{
			//		_Entities.Remove(e);
			//	}
			//}

			//// 加入超级缓存的实体对象，需要标记来自数据库
			//entity.MarkDb(true);
			//lock (this)
			//{
			//	_Entities.Add(entity);
			//	if (s_uniqueField != null) { _dictEntities.Add(entity[s_uniqueField.Name], entity); }
			//}
			lock (this)
			{
				if (s_uniqueField != null)
				{
					TEntity e;
					var mk = entity[s_uniqueField.Name];
					if (_dictEntities.TryGetValue(mk, out e))
					{
						_dictEntities.Remove(mk);
						// 更新实体缓存时，不做拷贝，避免产生脏数据，如果恰巧又使用单对象缓存，那会导致自动保存
						_Entities.Remove(e);
					}
				}

				_Entities.Add(entity);
				if (s_uniqueField != null) { _dictEntities.Add(entity[s_uniqueField.Name], entity); }
			}
			#endregion
		}

		#endregion

		#region -- 统计 --

		/// <summary>总次数</summary>
		public Int32 Total;

		/// <summary>第一次命中</summary>
		public Int32 Shoot1;

		/// <summary>第二次命中</summary>
		public Int32 Shoot2;

		/// <summary>下一次显示时间</summary>
		public DateTime NextShow;

		/// <summary>显示统计信息</summary>
		public void ShowStatics()
		{
			if (Total > c_zero)
			{
				var sb = new StringBuilder();
				sb.AppendFormat("实体缓存<{0,-20}>", typeof(TEntity).Name);
				sb.AppendFormat("总次数{0,7:n0}", Total);
				if (Shoot1 > c_zero) { sb.AppendFormat("，命中{0,7:n0}（{1,6:P02}）", Shoot1, (Double)Shoot1 / Total); }
				if (Shoot2 > c_zero) { sb.AppendFormat("，二级命中{0,3:n0}（{1,6:P02}）", Shoot2, (Double)Shoot2 / Total); }

				DAL.Logger.Info(sb.ToString());
			}
		}

		#endregion

		#region -- IEntityCache 成员 --

		IEntityList IEntityCache.Entities
		{
			get { return Entities; }
		}

		/// <summary>根据指定项查找</summary>
		/// <param name="name">属性名</param>
		/// <param name="value">属性值</param>
		/// <returns></returns>
		public IEntity Find(string name, object value)
		{
			return Entities.Find(name, value);
		}

		/// <summary>根据指定项查找</summary>
		/// <param name="name">属性名</param>
		/// <param name="value">属性值</param>
		/// <returns></returns>
		public IEntityList FindAll(string name, object value)
		{
			return Entities.FindAll(name, value);
		}

		/// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
		/// <param name="match">条件</param>
		/// <returns></returns>
		public IEntityList FindAll(Predicate<IEntity> match)
		{
			return Entities.FindAll(e => match(e));
		}

		#endregion

		#region -- 辅助 --

		internal EntityCache<TEntity> CopySettingFrom(EntityCache<TEntity> ec)
		{
			this.Expriod = ec.Expriod;
			this.MaxCount = ec.MaxCount;
			this.Asynchronous = ec.Asynchronous;
			this.HoldCache = ec.HoldCache;

			this.FillListMethod = ec.FillListMethod;

			return this;
		}

		#endregion
	}
}