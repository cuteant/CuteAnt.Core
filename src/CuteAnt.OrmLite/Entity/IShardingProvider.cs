using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	#region -- interface IShardingProvider --

	/// <summary>实体数据分片提供者接口</summary>
	public interface IShardingProvider : IDisposable
	{
		/// <summary>数据提供者名称，默认为空，取实体初始的数据连接配置。</summary>
		String DbProviderName { get; set; }

		/// <summary>根据指定的连接名、表名，启动实体数据分片。</summary>
		/// <param name="connName">连接名</param>
		/// <param name="tableName">表名</param>
		void Startup(String connName, String tableName);

		/// <summary>根据指定的序列化片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		void StartupBySerializedShardingKey(String shardingKey);

		/// <summary>根据指定的片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		void StartupByShardingKey(Object shardingKey);

		/// <summary>恢复实体数据分片配置</summary>
		void Recover();
	}
	/// <summary>实体数据分片提供者接口</summary>
	public interface IShardingProvider<TKey> : IShardingProvider
	{
		/// <summary>根据指定的片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		void Startup(TKey shardingKey);
	}

	#endregion

	#region -- class ShardingProvider<TEntity, TKey> --

	/// <summary>实体数据分片提供者</summary>
	public abstract class ShardingProvider<TEntity, TKey> : ShardingProvider<TEntity>, IShardingProvider<TKey>
		where TEntity : Entity<TEntity>, new()
	{
		/// <summary>根据指定的片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		public sealed override void StartupByShardingKey(Object shardingKey)
		{
			Startup((TKey)shardingKey);
		}

		/// <summary>根据指定的片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		public abstract void Startup(TKey shardingKey);
	}

	#endregion

	#region -- class ShardingProvider<TEntity> --

	/// <summary>默认实体数据分片提供者</summary>
	public class ShardingProvider<TEntity> : ShardingProviderBase
		where TEntity : Entity<TEntity>, new()
	{
		#region -- 属性 --

		private String _LastConnName;
		/// <summary>连接名</summary>
		public String LastConnName
		{
			get { return _LastConnName; }
			set { _LastConnName = value; }
		}

		private String _LastTableName;
		/// <summary>表名</summary>
		public String LastTableName
		{
			get { return _LastTableName; }
			set { _LastTableName = value; }
		}

		#endregion

		#region -- Startup --

		/// <summary>根据指定的连接名、表名，启动实体数据分片。</summary>
		/// <param name="connName">连接名</param>
		/// <param name="tableName">表名</param>
		public override void Startup(String connName, String tableName)
		{
			LastConnName = Entity<TEntity>.Meta.ConnName;
			LastTableName = Entity<TEntity>.Meta.TableName;

			Entity<TEntity>.Meta.ConnName = EnsureConn(Entity<TEntity>.Meta.ThisType, connName);
			Entity<TEntity>.Meta.TableName = tableName;
		}

		/// <summary>根据指定的序列化片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		public override void StartupBySerializedShardingKey(String shardingKey)
		{
			// nothing to do
		}

		/// <summary>根据指定的片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		public override void StartupByShardingKey(Object shardingKey)
		{
			// nothing to do
		}

		#endregion

		#region -- Recover --

		/// <summary>恢复实体数据分片配置</summary>
		public override void Recover()
		{
			Entity<TEntity>.Meta.ConnName = LastConnName;
			Entity<TEntity>.Meta.TableName = LastTableName;
		}

		#endregion
	}

	#endregion

	#region -- class ShardingProviderBase --

	/// <summary>实体数据分片提供者基类</summary>
	public abstract class ShardingProviderBase : IShardingProvider
	{
		private String _DbProviderName = String.Empty;

		/// <summary>数据提供者名称，默认为空，取实体初始的数据连接配置。
		/// <para>如果需要手动赋值，必须在实体数据分片提供者实例执行 Startup 方法之前进行赋值。</para>
		/// </summary>
		public String DbProviderName
		{
			get { return _DbProviderName; }
			set { _DbProviderName = value; }
		}

		/// <summary>根据指定的连接名、表名，启动实体数据分片。</summary>
		/// <param name="connName">连接名</param>
		/// <param name="tableName">表名</param>
		public abstract void Startup(String connName, String tableName);

		/// <summary>根据指定的序列化片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		public abstract void StartupBySerializedShardingKey(String shardingKey);

		/// <summary>根据指定的片键解析连接名、表名，启动实体数据分片。</summary>
		/// <param name="shardingKey">片键</param>
		public abstract void StartupByShardingKey(Object shardingKey);

		/// <summary>恢复实体数据分片配置</summary>
		public abstract void Recover();

		/// <summary>恢复实体数据分片配置</summary>
		public void Dispose() { Recover(); }

		private ConcurrentDictionary<String, String> _ConnNames = new ConcurrentDictionary<String, String>();

		/// <summary>确保数据连接存在</summary>
		/// <param name="entityType">实体类型</param>
		/// <param name="connName">数据连接名称</param>
		/// <returns></returns>
		protected String EnsureConn(Type entityType, String connName)
		{
			if (connName.IsNullOrWhiteSpace()) { return null; }

			String realConnName = null;

			if (_ConnNames.TryGetValue(connName, out realConnName)) { return realConnName; }

			var dbProviderName = DbProviderName;
			if (dbProviderName.IsNullOrWhiteSpace())
			{
				// 获取实体初始连接配置
				var eop = EntityFactory.CreateOperate(entityType);
				dbProviderName = DAL.GetDbProviderName(eop.Table.ConnName);
			}

			realConnName = "{0}{1}".FormatWith(dbProviderName, connName);
			if (!DAL.ConnStrs.ContainsKey(realConnName))
			{
				DAL.AddConnStr(realConnName, connName, dbProviderName);
			}

			_ConnNames.TryAdd(connName, realConnName);

			return realConnName;
		}
	}

	#endregion

	//	/// <summary>数据分片方式</summary>
	//	public enum DatabaseShardingType
	//	{
	//		/// <summary>自定义数据分片</summary>
	//		[Description("自定义数据分片")]
	//		Custom,

	//		/// <summary>基于特定类型键进行数据分片</summary>
	//		[Description("基于特定类型键进行数据分片")]
	//		BasedShardKey,
	//	}

	//	/// <summary>数据分片键类型</summary>
	//	public enum ShardingKeyType
	//	{
	//		/// <summary>日期时间</summary>
	//		[Description("日期时间")]
	//		DateTime,

	//		/// <summary>全局唯一标识符</summary>
	//		[Description("全局唯一标识符")]
	//		Guid,

	//		/// <summary>哈希字符串类型</summary>
	//		[Description("哈希字符串")]
	//		HashString
	//	}
}
