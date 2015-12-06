/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Text;
using CuteAnt.Configuration;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.Log;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite.Cache
{
	/// <summary>全局缓存设置</summary>
	public sealed class CacheSetting
	{
		private static Boolean? _Debug;
		/// <summary>是否调试缓存模块</summary>
		public static Boolean Debug
		{
			get
			{
				if (_Debug != null) { return _Debug.Value; }

				_Debug = OrmLiteConfig.Current.IsCacheDebug;  //Config.GetConfig<Boolean>("XCode.Cache.Debug", false);

				return _Debug.Value;
			}
			set { _Debug = value; }
		}

		private static Int32? _CacheExpiration;
		/// <summary>缓存相对有效期。
		/// -2	关闭缓存
		/// -1	非独占数据库，有外部系统操作数据库，使用请求级缓存；
		///  0	永久静态缓存；
		/// >0	静态缓存时间，单位是秒；
		/// </summary>
		public static Int32 CacheExpiration
		{
			get
			{
				if (_CacheExpiration.HasValue) { return _CacheExpiration.Value; }

				//var n = Alone ? 60 : -2;
				//_CacheExpiration = Config.GetMutilConfig<Int32>(n, "XCode.Cache.Expiration", "XCacheExpiration");
				_CacheExpiration = OrmLiteConfig.Current.CacheExpiration;

				return _CacheExpiration.Value;
			}
			set { _CacheExpiration = value; }
		}

		private static Int32? _CheckPeriod;
		/// <summary>缓存维护定时器的检查周期，默认5秒</summary>
		public static Int32 CheckPeriod
		{
			get
			{
				if (_CheckPeriod.HasValue) { return _CheckPeriod.Value; }

				_CheckPeriod = OrmLiteConfig.Current.CacheCheckPeriod; //Config.GetMutilConfig<Int32>(5, "XCode.Cache.CheckPeriod", "XCacheCheckPeriod");

				return _CheckPeriod.Value;
			}
			set { _CheckPeriod = value; }
		}

		private static Boolean? _Alone;
		/// <summary>是否独占数据库，独占时将大大加大缓存权重，默认true（Debug时为false）</summary>
		public static Boolean Alone
		{
			get
			{
				if (_Alone.HasValue) { return _Alone.Value; }

				_Alone = OrmLiteConfig.Current.IsCacheAlone; //Config.GetConfig<Boolean>("XCode.Cache.Alone", !Debug);
				DAL.WriteLog("使用数据库方式：{0}", _Alone.Value ? "独占，加大缓存权重" : "非独占");

				return _Alone.Value;
			}
			set { _Alone = value; }
		}

		private static Int32? _EntityCacheExpire;
		/// <summary>实体缓存过期时间，默认60秒</summary>
		public static Int32 EntityCacheExpire
		{
			get
			{
				if (_EntityCacheExpire.HasValue) { return _EntityCacheExpire.Value; }

				_EntityCacheExpire = OrmLiteConfig.Current.EntityCacheExpire;

				return _EntityCacheExpire.Value;
			}
			set { _EntityCacheExpire = value; }
		}

		private static Int32? _EntityCacheMaxCount;
		/// <summary>实体缓存最大记录数，默认1000条记录；当实体记录总数超过这个阈值，系统会自动清空实体缓存。</summary>
		public static Int32 EntityCacheMaxCount
		{
			get
			{
				if (_EntityCacheMaxCount.HasValue) { return _EntityCacheMaxCount.Value; }

				_EntityCacheMaxCount = OrmLiteConfig.Current.EntityCacheMaxCount;

				return _EntityCacheMaxCount.Value;
			}
			set { _EntityCacheMaxCount = value; }
		}

		private static Int32? _SingleCacheExpire;
		/// <summary>单对象缓存过期时间，默认60秒</summary>
		public static Int32 SingleCacheExpire
		{
			get
			{
				if (_SingleCacheExpire.HasValue) { return _SingleCacheExpire.Value; }

				_SingleCacheExpire = OrmLiteConfig.Current.SingleCacheExpire;

				return _SingleCacheExpire.Value;
			}
			set { _SingleCacheExpire = value; }
		}
	}
}