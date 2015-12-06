using System;
using System.Collections.Generic;
using System.Text;

namespace CuteAnt.OrmLite
{
	#region -- interface IShardingProviderFactory --

	/// <summary>实体数据分片提供者工厂接口</summary>
	public interface IShardingProviderFactory
	{
		/// <summary>根据连接名、表名创建实体数据分片提供者</summary>
		/// <param name="connName">连接名</param>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		IShardingProvider Create(String connName, String tableName);

		/// <summary>根据指定的序列化片键创建实体数据分片提供者。</summary>
		/// <param name="shardingKey">片键</param>
		IShardingProvider CreateBySerializedShardingKey(String shardingKey);

		/// <summary>根据指定的片键创建实体数据分片提供者</summary>
		/// <param name="shardingKey">片键</param>
		IShardingProvider CreateByShardingKey(Object shardingKey);
	}

	/// <summary>实体数据分片提供者工厂接口</summary>
	public interface IShardingProviderFactory<TKey> : IShardingProviderFactory
	{
		/// <summary>根据指定的片键创建实体数据分片提供者</summary>
		/// <param name="shardingKey">片键</param>
		IShardingProvider Create(TKey shardingKey);
	}

	#endregion

	#region -- class ShardingProviderFactory<TEntity> --

	/// <summary>默认实体数据分片提供者工厂</summary>
	public class ShardingProviderFactory<TEntity> : ShardingProviderFactory<ShardingProviderFactory<TEntity>, ShardingProvider<TEntity>, TEntity>
		where TEntity : Entity<TEntity>, new()
	{
	}

	#endregion

	#region -- class ShardingProviderFactory<TFactory, TProvider, TEntity, TKey> --

	/// <summary>实体数据分片提供者工厂</summary>
	public class ShardingProviderFactory<TFactory, TProvider, TEntity, TKey> : ShardingProviderFactory<TFactory, TProvider, TEntity>, IShardingProviderFactory<TKey>
		where TFactory : ShardingProviderFactory<TFactory, TProvider, TEntity>, new()
		where TProvider : ShardingProvider<TEntity, TKey>, new()
		where TEntity : Entity<TEntity>, new()
	{
		/// <summary>根据片键创建实体数据分片提供者</summary>
		/// <param name="shardingKey">片键</param>
		/// <returns></returns>
		public IShardingProvider Create(TKey shardingKey)
		{
			var provider = new TProvider();
			provider.Startup(shardingKey);
			return provider;
		}
	}

	#endregion

	#region -- class ShardingProviderFactory<TFactory, TProvider, TEntity> --

	/// <summary>实体数据分片提供者工厂</summary>
	public class ShardingProviderFactory<TFactory, TProvider, TEntity> : IShardingProviderFactory
		where TFactory : ShardingProviderFactory<TFactory, TProvider, TEntity>, new()
		where TProvider : ShardingProvider<TEntity>, new()
		where TEntity : Entity<TEntity>, new()
	{
		/// <summary>实体数据分片提供者工厂的默认只读实例</summary>
		public static readonly TFactory Instance = new TFactory();

		/// <summary>根据连接名、表名创建实体数据分片提供者</summary>
		/// <param name="connName">连接名</param>
		/// <param name="tableName">表名</param>
		/// <returns></returns>
		public IShardingProvider Create(String connName, String tableName)
		{
			var provider = new TProvider();
			provider.Startup(connName, tableName);
			return provider;
		}

		/// <summary>根据指定的序列化片键创建实体数据分片提供者。</summary>
		/// <param name="shardingKey">片键</param>
		public IShardingProvider CreateBySerializedShardingKey(String shardingKey)
		{
			var provider = new TProvider();
			provider.StartupBySerializedShardingKey(shardingKey);
			return provider;
		}

		/// <summary>根据片键创建实体数据分片提供者</summary>
		/// <param name="shardingKey">片键</param>
		/// <returns></returns>
		public IShardingProvider CreateByShardingKey(Object shardingKey)
		{
			var provider = new TProvider();
			provider.StartupByShardingKey(shardingKey);
			return provider;
		}
	}

	#endregion
}
