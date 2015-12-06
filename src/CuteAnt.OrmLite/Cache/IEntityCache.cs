/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;

namespace CuteAnt.OrmLite.Cache
{
	#region -- interface IEntityCacheBase --

	/// <summary>缓存基接口</summary>
	public interface IEntityCacheBase
	{
		/// <summary>连接名</summary>
		String ConnName { get; set; }

		/// <summary>表名</summary>
		String TableName { get; set; }
	}

	#endregion

	#region -- interface IEntityCache --

	/// <summary>实体缓存接口</summary>
	public interface IEntityCache : IEntityCacheBase
	{
		/// <summary>实体缓存最大记录数，默认1000条记录；当实体记录总数超过这个阈值，系统会自动清空实体缓存。</summary>
		Int32 MaxCount { get; set; }

		/// <summary>在数据修改时保持缓存，不再过期，独占数据库时默认打开，否则默认关闭</summary>
		Boolean HoldCache { get; set; }

		/// <summary>是否在使用缓存</summary>
		Boolean Using { get; }

		/// <summary>实体集合。因为涉及一个转换，数据量大时很耗性能，建议不要使用。</summary>
		IEntityList Entities { get; }

		/// <summary>根据指定项查找</summary>
		/// <param name="name">属性名</param>
		/// <param name="value">属性值</param>
		/// <returns></returns>
		IEntity Find(String name, Object value);

		/// <summary>根据指定项查找</summary>
		/// <param name="name">属性名</param>
		/// <param name="value">属性值</param>
		/// <returns></returns>
		IEntityList FindAll(String name, Object value);

		/// <summary>检索与指定谓词定义的条件匹配的所有元素。</summary>
		/// <param name="match">条件</param>
		/// <returns></returns>
		IEntityList FindAll(Predicate<IEntity> match);

		/// <summary>清除缓存</summary>
		/// <param name="reloading">是否重新载入缓存，如果为否，则直接清空缓存不再读取数据库。</param>
		/// <param name="reason">清除缓存原因</param>
		void Clear(Boolean reloading, String reason);
	}

	#endregion

	#region -- interface ISingleEntityCache --

	/// <summary>单对象缓存接口</summary>
	public interface ISingleEntityCache : IEntityCacheBase
	{
		///// <summary>单对象缓存主键是否使用实体模型唯一键（第一个标识列或者唯一的主键）</summary>
		//Boolean MasterKeyUsingUniqueField { get; set; }

		/// <summary>在数据修改时保持缓存，不再过期，独占数据库时默认打开，否则默认关闭</summary>
		Boolean HoldCache { get; set; }

		#region - 批量主键获取 -

		/// <summary>根据主键获取实体记录列表</summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		IEntityList FindAllInKeys<T>(IEnumerable<T> keys);

		/// <summary>根据主键获取实体记录列表</summary>
		/// <typeparam name="T">主键原始类型</typeparam>
		/// <param name="keys">主键字符串，以逗号或分号分割</param>
		/// <returns></returns>
		IEntityList FindAllInKeys<T>(String keys);

		#endregion

		#region - 批量从键获取 -

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		IEntityList FindAllInSlaveKeys<T>(IEnumerable<T> slavekeys);

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		IEntityList FindAllInSlaveKeys(ICollection<String> slavekeys);

		/// <summary>根据从键获取实体记录列表</summary>
		/// <param name="slavekeys"></param>
		/// <returns></returns>
		IEntityList FindAllInSlaveKeys(String slavekeys);

		#endregion

		#region - 获取实体数据 -

		/// <summary>获取数据</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IEntity this[Object key] { get; }

		/// <summary>根据从键获取实体数据</summary>
		/// <param name="slaveKey"></param>
		/// <returns></returns>
		IEntity GetItemWithSlaveKey<T>(T slaveKey);

		/// <summary>根据从键获取实体数据</summary>
		/// <param name="slaveKey"></param>
		/// <returns></returns>
		IEntity GetItemWithSlaveKey(String slaveKey);

		#endregion

		#region - 方法 -

		/// <summary>初始化单对象缓存，服务端启动时预载入实体记录集</summary>
		/// <remarks>注意事项：
		/// <para>调用方式：TEntity.Meta.Factory.Session.SingleCache.Initialize()，不要使用TEntity.Meta.Session.SingleCache.Initialize()；
		/// 因为Factory的调用会联级触发静态构造函数，确保单对象缓存设置成功</para>
		/// <para>服务端启动时，如果使用异步方式初始化单对象缓存，请将同一数据模型（ConnName）下的实体类型放在同一异步方法内执行，否则实体类型的架构检查抛异常</para>
		/// </remarks>
		void Initialize();

		/// <summary>是否包含指定主键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		Boolean ContainsKey(Object key);

		/// <summary>是否包含指定从键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		Boolean ContainsSlaveKey(Int32 key);

		/// <summary>是否包含指定从键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		Boolean ContainsSlaveKey(Int64 key);

		/// <summary>是否包含指定从键</summary>
		/// <param name="key"></param>
		/// <returns></returns>
		Boolean ContainsSlaveKey(String key);

		/// <summary>向单对象缓存添加项</summary>
		/// <param name="key"></param>
		/// <param name="value">实体对象</param>
		/// <returns></returns>
		Boolean Add(Object key, IEntity value);

		/// <summary>向单对象缓存添加项</summary>
		/// <param name="value">实体对象</param>
		/// <returns></returns>
		Boolean Add(IEntity value);

		/// <summary>移除指定项</summary>
		/// <param name="entity"></param>
		void Remove(IEntity entity);

		/// <summary>移除指定项</summary>
		/// <param name="entity"></param>
		/// <param name="save">是否自动保存实体对象</param>
		void Remove(IEntity entity, Boolean save);

		/// <summary>移除指定项</summary>
		/// <param name="key"></param>
		void RemoveKey(Object key);

		/// <summary>移除指定项</summary>
		/// <param name="key">键值</param>
		/// <param name="save">是否自动保存实体对象</param>
		void RemoveKey(Object key, Boolean save);

		/// <summary>清除所有数据</summary>
		/// <param name="reason">清除缓存原因</param>
		void Clear(String reason);

		#endregion
	}

	#endregion
}