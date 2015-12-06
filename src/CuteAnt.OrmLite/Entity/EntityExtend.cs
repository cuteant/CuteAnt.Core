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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CuteAnt.Collections;
using System.Threading;

namespace CuteAnt.OrmLite
{
	/// <summary>实体扩展</summary>
	public class EntityExtend : DictionaryCache<String, Object>, IDictionary<String, Object>
	{
		#region -- 构造 --

		/// <summary>实例化一个不区分键大小写的实体扩展</summary>
		public EntityExtend() : base(StringComparer.OrdinalIgnoreCase) { }

		#endregion

		#region -- 依赖项 --

		private sealed class DependItem
		{
			internal Boolean RemoveExtendEventRegistered { get; set; }

			internal ConcurrentHashSet<String> ExtendProperties { get; set; }

			internal DependItem()
			{
				RemoveExtendEventRegistered = false;
				ExtendProperties = new ConcurrentHashSet<String>();
			}
		}

		#endregion

		#region -- 扩展属性依赖类型集合 --

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private ConcurrentDictionary<Type, Lazy<DependItem>> _depends;

		/// <summary>类型依赖</summary>
		private ConcurrentDictionary<Type, Lazy<DependItem>> Depends
		{
			get
			{
				if (_depends == null)
				{
					Interlocked.CompareExchange<ConcurrentDictionary<Type, Lazy<DependItem>>>(
						ref _depends, new ConcurrentDictionary<Type, Lazy<DependItem>>(), null);
				}
				return _depends;
			}
		}

		#endregion

		#region -- 单类型依赖关系 --

		/// <summary>获取扩展属性，获取数据时向指定的依赖实体类注册数据更改事件</summary>
		/// <typeparam name="TDependEntity">依赖实体类，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TResult">返回类型</typeparam>
		/// <param name="key">键值</param>
		/// <param name="func">回调</param>
		/// <param name="cacheDefault">是否缓存默认值，可选参数，默认缓存</param>
		/// <returns></returns>
		[DebuggerHidden]
		public virtual TResult GetExtend<TDependEntity, TResult>(String key, Func<String, Object> func, Boolean cacheDefault = true)
			where TDependEntity : Entity<TDependEntity>, new()
		{
			Object value = null;
			if (TryGetValue(key, out value)) { return (TResult)value; }

			// 针对每个类型，仅注册一个事件
			var type = typeof(TDependEntity);
			var dependItem = Depends.GetOrAdd(type, (k) => new Lazy<DependItem>(() => new DependItem())).Value;

			CacheDefault = cacheDefault;

			// 这里使用了成员方法GetExtend<TDependEntity>而不是匿名函数，为了避免生成包装类，且每次调用前实例化包装类带来较大开销
			return (TResult)GetItem<Func<String, Object>, DependItem>(key, func, dependItem, GetExtend<TDependEntity>);
		}

		[DebuggerHidden]
		private Object GetExtend<TDependEntity>(String key, Func<String, Object> func, DependItem dependItem)
			where TDependEntity : Entity<TDependEntity>, new()
		{
			Object value = null;
			if (func != null) { value = func(key); }

			dependItem.ExtendProperties.TryAdd(key);
			if (!dependItem.RemoveExtendEventRegistered)
			{
				// 这里使用RemoveExtend而不是匿名函数，为了避免生成包装类，事件的Target将指向包装类的实例，
				// 而内部要对Target实行弱引用，就必须保证事件的Target是实体对象本身。
				// OnDataChange内部对事件进行了拆分，弱引用Target，反射调用Method，那样性能较低，所以使用了快速方法访问器MethodInfoEx，
				Entity<TDependEntity>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem.RemoveExtendEventRegistered = true;
			}

			return value;
		}

		/// <summary>设置扩展属性</summary>
		/// <typeparam name="TDependEntity">依赖实体类，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <param name="key"></param>
		/// <param name="value">数值</param>
		[DebuggerHidden]
		public virtual void SetExtend<TDependEntity>(String key, Object value)
			where TDependEntity : Entity<TDependEntity>, new()
		{
			// 针对每个类型，仅注册一个事件
			Type type = typeof(TDependEntity);
			var dependItem = Depends.GetOrAdd(type, (k) => new Lazy<DependItem>(() => new DependItem())).Value;

			this[key] = value;
			dependItem.ExtendProperties.TryAdd(key);

			if (!dependItem.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem.RemoveExtendEventRegistered = true;
			}
		}

		#endregion

		#region -- 双类型依赖关系 --

		/// <summary>获取扩展属性，获取数据时向指定的依赖实体类注册数据更改事件</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TResult">返回类型</typeparam>
		/// <param name="key">键值</param>
		/// <param name="func">回调</param>
		/// <param name="cacheDefault">是否缓存默认值，可选参数，默认缓存</param>
		/// <returns></returns>
		[DebuggerHidden]
		public virtual TResult GetExtend<TDependEntity1, TDependEntity2, TResult>(String key, Func<String, Object> func, Boolean cacheDefault = true)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
		{
			Object value = null;
			if (TryGetValue(key, out value)) { return (TResult)value; }

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;

			CacheDefault = cacheDefault;

			// 这里使用了成员方法GetExtend<TDependEntity>而不是匿名函数，为了避免生成包装类，且每次调用前实例化包装类带来较大开销
			return (TResult)GetItem<Func<String, Object>, DependItem, DependItem>(key, func, dependItem1, dependItem2, GetExtend<TDependEntity1, TDependEntity2>);
		}

		[DebuggerHidden]
		private Object GetExtend<TDependEntity1, TDependEntity2>(String key, Func<String, Object> func, DependItem dependItem1, DependItem dependItem2)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
		{
			Object value = null;
			if (func != null) { value = func(key); }

			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			return value;
		}

		/// <summary>设置扩展属性</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <param name="key"></param>
		/// <param name="value">数值</param>
		[DebuggerHidden]
		public virtual void SetExtend<TDependEntity1, TDependEntity2>(String key, Object value)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
		{
			this[key] = value;

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}
		}

		#endregion

		#region -- 三类型依赖关系 --

		/// <summary>获取扩展属性，获取数据时向指定的依赖实体类注册数据更改事件</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity3">依赖实体类3，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TResult">返回类型</typeparam>
		/// <param name="key">键值</param>
		/// <param name="func">回调</param>
		/// <param name="cacheDefault">是否缓存默认值，可选参数，默认缓存</param>
		/// <returns></returns>
		[DebuggerHidden]
		public virtual TResult GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TResult>(String key, Func<String, Object> func, Boolean cacheDefault = true)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
		{
			Object value = null;
			if (TryGetValue(key, out value)) { return (TResult)value; }

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem3 = Depends.GetOrAdd(typeof(TDependEntity3), (k) => new Lazy<DependItem>(() => new DependItem())).Value;

			CacheDefault = cacheDefault;

			// 这里使用了成员方法GetExtend<TDependEntity>而不是匿名函数，为了避免生成包装类，且每次调用前实例化包装类带来较大开销
			return (TResult)GetItem<Func<String, Object>, DependItem, DependItem, DependItem>(key,
															func, dependItem1, dependItem2, dependItem3,
															GetExtend<TDependEntity1, TDependEntity2, TDependEntity3>);
		}

		[DebuggerHidden]
		private Object GetExtend<TDependEntity1, TDependEntity2, TDependEntity3>(String key, Func<String, Object> func,
			DependItem dependItem1, DependItem dependItem2, DependItem dependItem3)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
		{
			Object value = null;
			if (func != null) { value = func(key); }

			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			dependItem3.ExtendProperties.TryAdd(key);
			if (!dependItem3.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity3>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem3.RemoveExtendEventRegistered = true;
			}

			return value;
		}

		/// <summary>设置扩展属性</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity3">依赖实体类3，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <param name="key"></param>
		/// <param name="value">数值</param>
		[DebuggerHidden]
		public virtual void SetExtend<TDependEntity1, TDependEntity2, TDependEntity3>(String key, Object value)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
		{
			this[key] = value;

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem3 = Depends.GetOrAdd(typeof(TDependEntity3), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem3.ExtendProperties.TryAdd(key);
			if (!dependItem3.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity3>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem3.RemoveExtendEventRegistered = true;
			}
		}

		#endregion

		#region -- 四类型依赖关系 --

		/// <summary>获取扩展属性，获取数据时向指定的依赖实体类注册数据更改事件</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity3">依赖实体类3，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity4">依赖实体类4，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TResult">返回类型</typeparam>
		/// <param name="key">键值</param>
		/// <param name="func">回调</param>
		/// <param name="cacheDefault">是否缓存默认值，可选参数，默认缓存</param>
		/// <returns></returns>
		[DebuggerHidden]
		public virtual TResult GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4, TResult>(
			String key, Func<String, Object> func, Boolean cacheDefault = true)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
			where TDependEntity4 : Entity<TDependEntity4>, new()
		{
			Object value = null;
			if (TryGetValue(key, out value)) { return (TResult)value; }

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem3 = Depends.GetOrAdd(typeof(TDependEntity3), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem4 = Depends.GetOrAdd(typeof(TDependEntity4), (k) => new Lazy<DependItem>(() => new DependItem())).Value;

			CacheDefault = cacheDefault;

			// 这里使用了成员方法GetExtend<TDependEntity>而不是匿名函数，为了避免生成包装类，且每次调用前实例化包装类带来较大开销
			return (TResult)GetItem<Func<String, Object>, DependItem, DependItem, DependItem, DependItem>(key,
															func, dependItem1, dependItem2, dependItem3, dependItem4,
															GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4>);
		}

		[DebuggerHidden]
		private Object GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4>(String key, Func<String, Object> func,
			DependItem dependItem1, DependItem dependItem2, DependItem dependItem3, DependItem dependItem4)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
			where TDependEntity4 : Entity<TDependEntity4>, new()
		{
			Object value = null;
			if (func != null) { value = func(key); }

			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			dependItem3.ExtendProperties.TryAdd(key);
			if (!dependItem3.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity3>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem3.RemoveExtendEventRegistered = true;
			}

			dependItem4.ExtendProperties.TryAdd(key);
			if (!dependItem4.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity4>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem4.RemoveExtendEventRegistered = true;
			}

			return value;
		}

		/// <summary>设置扩展属性</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity3">依赖实体类3，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity4">依赖实体类4，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <param name="key"></param>
		/// <param name="value">数值</param>
		[DebuggerHidden]
		public virtual void SetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4>(String key, Object value)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
			where TDependEntity4 : Entity<TDependEntity4>, new()
		{
			this[key] = value;

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem3 = Depends.GetOrAdd(typeof(TDependEntity3), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem3.ExtendProperties.TryAdd(key);
			if (!dependItem3.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity3>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem3.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem4 = Depends.GetOrAdd(typeof(TDependEntity4), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem4.ExtendProperties.TryAdd(key);
			if (!dependItem4.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity4>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem4.RemoveExtendEventRegistered = true;
			}
		}

		#endregion

		#region -- 五类型依赖关系 --

		/// <summary>获取扩展属性，获取数据时向指定的依赖实体类注册数据更改事件</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity3">依赖实体类3，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity4">依赖实体类4，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity5">依赖实体类5，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TResult">返回类型</typeparam>
		/// <param name="key">键值</param>
		/// <param name="func">回调</param>
		/// <param name="cacheDefault">是否缓存默认值，可选参数，默认缓存</param>
		/// <returns></returns>
		[DebuggerHidden]
		public virtual TResult GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4, TDependEntity5, TResult>(
			String key, Func<String, Object> func, Boolean cacheDefault = true)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
			where TDependEntity4 : Entity<TDependEntity4>, new()
			where TDependEntity5 : Entity<TDependEntity5>, new()
		{
			Object value = null;
			if (TryGetValue(key, out value)) { return (TResult)value; }

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem3 = Depends.GetOrAdd(typeof(TDependEntity3), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem4 = Depends.GetOrAdd(typeof(TDependEntity4), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			var dependItem5 = Depends.GetOrAdd(typeof(TDependEntity5), (k) => new Lazy<DependItem>(() => new DependItem())).Value;

			CacheDefault = cacheDefault;

			// 这里使用了成员方法GetExtend<TDependEntity>而不是匿名函数，为了避免生成包装类，且每次调用前实例化包装类带来较大开销
			return (TResult)GetItem<Func<String, Object>, DependItem, DependItem, DependItem, DependItem, DependItem>(key,
															func, dependItem1, dependItem2, dependItem3, dependItem4, dependItem5,
															GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4, TDependEntity5>);
		}

		[DebuggerHidden]
		private Object GetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4, TDependEntity5>(String key, Func<String, Object> func,
			DependItem dependItem1, DependItem dependItem2, DependItem dependItem3, DependItem dependItem4, DependItem dependItem5)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
			where TDependEntity4 : Entity<TDependEntity4>, new()
			where TDependEntity5 : Entity<TDependEntity5>, new()
		{
			Object value = null;
			if (func != null) { value = func(key); }

			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			dependItem3.ExtendProperties.TryAdd(key);
			if (!dependItem3.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity3>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem3.RemoveExtendEventRegistered = true;
			}

			dependItem4.ExtendProperties.TryAdd(key);
			if (!dependItem4.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity4>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem4.RemoveExtendEventRegistered = true;
			}

			dependItem5.ExtendProperties.TryAdd(key);
			if (!dependItem5.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity5>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem5.RemoveExtendEventRegistered = true;
			}

			return value;
		}

		/// <summary>设置扩展属性</summary>
		/// <typeparam name="TDependEntity1">依赖实体类1，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity2">依赖实体类2，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity3">依赖实体类3，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity4">依赖实体类4，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <typeparam name="TDependEntity5">依赖实体类5，该实体类数据更改时清空所有依赖于实体类的扩展属性</typeparam>
		/// <param name="key"></param>
		/// <param name="value">数值</param>
		[DebuggerHidden]
		public virtual void SetExtend<TDependEntity1, TDependEntity2, TDependEntity3, TDependEntity4, TDependEntity5>(String key, Object value)
			where TDependEntity1 : Entity<TDependEntity1>, new()
			where TDependEntity2 : Entity<TDependEntity2>, new()
			where TDependEntity3 : Entity<TDependEntity3>, new()
			where TDependEntity4 : Entity<TDependEntity4>, new()
			where TDependEntity5 : Entity<TDependEntity5>, new()
		{
			this[key] = value;

			// 针对每个类型，仅注册一个事件
			var dependItem1 = Depends.GetOrAdd(typeof(TDependEntity1), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem1.ExtendProperties.TryAdd(key);
			if (!dependItem1.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity1>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem1.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem2 = Depends.GetOrAdd(typeof(TDependEntity2), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem2.ExtendProperties.TryAdd(key);
			if (!dependItem2.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity2>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem2.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem3 = Depends.GetOrAdd(typeof(TDependEntity3), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem3.ExtendProperties.TryAdd(key);
			if (!dependItem3.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity3>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem3.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem4 = Depends.GetOrAdd(typeof(TDependEntity4), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem4.ExtendProperties.TryAdd(key);
			if (!dependItem4.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity4>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem4.RemoveExtendEventRegistered = true;
			}

			// 针对每个类型，仅注册一个事件
			var dependItem5 = Depends.GetOrAdd(typeof(TDependEntity5), (k) => new Lazy<DependItem>(() => new DependItem())).Value;
			dependItem5.ExtendProperties.TryAdd(key);
			if (!dependItem5.RemoveExtendEventRegistered)
			{
				Entity<TDependEntity5>.Meta.Session.OnDataChange += RemoveExtend;
				dependItem5.RemoveExtendEventRegistered = true;
			}
		}

		#endregion

		#region -- 清理依赖于某类型的缓存 --

		/// <summary>清理依赖于某类型的缓存</summary>
		/// <param name="dependType">依赖类型</param>
		private void RemoveExtend(Type dependType)
		{
			if (Depends == null || Count < 1) { return; }

			// 找到依赖类型的扩展属性键值集合
			Lazy<DependItem> lazyDependItem = null;
			if (!Depends.TryGetValue(dependType, out lazyDependItem)) { return; }
			if (lazyDependItem == null) { return; }
			lazyDependItem.Value.RemoveExtendEventRegistered = false;
			if (lazyDependItem.Value.ExtendProperties.Count < 1) { return; }

			//System.Threading.Tasks.Parallel.ForEach(lazyDependItem.Value.ExtendProperties, s =>
			//{
			//	Remove(s);
			//});
			var extendProperties = lazyDependItem.Value.ExtendProperties;
			foreach (var item in extendProperties)
			{
				Remove(item);
			}
			lazyDependItem.Value.ExtendProperties.Clear();
		}

		#endregion
	}
}