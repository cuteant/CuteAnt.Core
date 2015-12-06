using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CuteAnt.Log;
using CuteAnt.Xml;

namespace CuteAnt.OrmLite.Configuration
{
	/// <summary>实体模型对象缓存配置</summary>
	[XmlConfigFile("Config\\EntityCache.config")]
	public class EntityCacheConfig : XmlConfig<EntityCacheConfig>
	{
		#region -- class EntityCacheInfo --

		/// <summary>单对象缓存配置信息</summary>
		public class EntityCacheInfo
		{
			#region - 构造 -

			internal EntityCacheInfo(Boolean disableSingleCache)
			{
				DisableSingleCache = disableSingleCache;
			}

			/// <summary>实例化</summary>
			public EntityCacheInfo()
			{
			}

			#endregion

			private Boolean _DisableEntityCache = false;

			/// <summary>是否禁用实体缓存缓存，默认值为否，启用。</summary>
			public Boolean DisableEntityCache
			{
				get { return _DisableEntityCache; }
				set { _DisableEntityCache = value; }
			}

			private Int32 _EntityCacheExpriod = -1;

			/// <summary>实体缓存过期时间。单位是秒，默认-1，使用系统统一配置。</summary>
			public Int32 EntityCacheExpriod
			{
				get { return _EntityCacheExpriod; }
				set { _EntityCacheExpriod = value; }
			}

			private Int32 _EntityCacheMaxCount = -1;

			/// <summary>实体缓存最大记录数。默认-1，使用系统统一配置。</summary>
			public Int32 EntityCacheMaxCount
			{
				get { return _EntityCacheMaxCount; }
				set { _EntityCacheMaxCount = value; }
			}

			private Int32 _HoldEntityCache = -1;

			/// <summary>在数据修改时保持缓存，不再过期。默认-1，使用系统统一配置。</summary>
			public Int32 HoldEntityCache
			{
				get { return _HoldEntityCache; }
				set { _HoldEntityCache = value; }
			}

			private Boolean _DisableSingleCache = true;

			/// <summary>是否禁用单对象缓存，默认值为真，禁用。</summary>
			public Boolean DisableSingleCache
			{
				get { return _DisableSingleCache; }
				set { _DisableSingleCache = value; }
			}

			private Int32 _SingleCacheExpriod = -1;

			/// <summary>单对象缓存过期时间。单位是秒，默认-1，使用系统统一配置。</summary>
			public Int32 SingleCacheExpriod
			{
				get { return _SingleCacheExpriod; }
				set { _SingleCacheExpriod = value; }
			}

			private Int32 _SingleCacheMaxCount = 10000;

			/// <summary>单对象缓存最大实体数，默认10000。</summary>
			public Int32 SingleCacheMaxCount
			{
				get { return _SingleCacheMaxCount; }
				set { _SingleCacheMaxCount = value; }
			}

			private Int32 _HoldSingleCache = -1;

			/// <summary>在数据修改时保持缓存，不再过期。默认-1，使用系统统一配置。</summary>
			public Int32 HoldSingleCache
			{
				get { return _HoldSingleCache; }
				set { _HoldSingleCache = value; }
			}
		}

		#endregion

		#region -- 属性 --

		private static readonly EntityCacheInfo _Default = new EntityCacheInfo();

		private SerializableDictionary<String, EntityCacheInfo> _EntityCaches;

		/// <summary>实体模型缓存集合</summary>
		public SerializableDictionary<String, EntityCacheInfo> EntityCaches
		{
			get { return _EntityCaches; }
			set { _EntityCaches = value; }
		}

		#endregion

		#region -- 构造 --

		//public EntityCacheConfig()
		//{
		//}

		#endregion

		#region -- 方法 --

		/// <summary>查询实体模型缓存配置</summary>
		/// <param name="connName">数据模型名称</param>
		/// <param name="tableName">实体模型名称</param>
		/// <returns></returns>
		public EntityCacheInfo Find(String connName, String tableName)
		{
			return Find(GetModelKey(connName, tableName));
		}

		/// <summary>查询实体模型缓存配置</summary>
		/// <param name="modelkey">模型关键字</param>
		/// <returns></returns>
		public EntityCacheInfo Find(String modelkey)
		{
			//DAL.Logger.Warn("开始查找：{0}".FormatWith(modelkey));

			if (EntityCaches == null || EntityCaches.Count <= 0) { return _Default; }

			EntityCacheInfo cache = null;

			if (EntityCaches.TryGetValue(modelkey, out cache))
			{
				//DAL.Logger.Warn("缓存配置找到：{0}", cache.EntityCacheExpriod);
				return cache;
			}
			else
			{
				//DAL.Logger.Warn("缓存配置未找到！");
				return _Default;
			}
		}

		/// <summary>获取实体模型关键字，自动格式化为小写字符串</summary>
		/// <param name="connName">数据连接名称</param>
		/// <param name="tableName">数据表名称</param>
		/// <returns></returns>
		private static String GetModelKey(String connName, String tableName)
		{
			return "{0}$$${1}".FormatWith(connName, tableName).ToLowerInvariant();
		}

		#endregion
	}
}
