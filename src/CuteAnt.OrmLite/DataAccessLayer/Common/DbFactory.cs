/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using CuteAnt.Collections;
using CuteAnt.OrmLite.Model;
using CuteAnt.Log;
using CuteAnt.Model;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>数据库工厂</summary>
	public static class DbFactory
	{
		#region -- 创建 --

		/// <summary>根据数据库类型创建提供者</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static IDatabase Create(DatabaseType dbType)
		{
			return OrmLiteService.Container.ResolveInstance<IDatabase>(dbType);
		}

		#endregion

		#region -- 静态构造 --

		internal static void Reg(IObjectContainer container)
		{
			//container
			//		.Reg<Access>()
			//		.Reg<SqlServer>()
			//		.Reg<Oracle>()
			//		.Reg<MySql>()
			//		.Reg<SQLite>()
			//		.Reg<Firebird>()
			//		.Reg<PostgreSQL>()
			//		.Reg<SqlCe>()
			//		.Reg<Access>(String.Empty);
			container
					.Reg<SQLite>()
					.Reg<SqlServer>()
					.Reg<Oracle>()
					.Reg<MySql>()
					.Reg<Firebird>()
					.Reg<PostgreSQL>()
					.Reg<SqlCe>()
					.Reg<SQLite>(String.Empty);

			// Access作为默认实现
		}

		private static IObjectContainer Reg<T>(this IObjectContainer container, Object id = null)
		{
			try
			{
				var db = typeof(T).CreateInstance() as IDatabase;
				if (id == null) { id = db.DbType; }

				// 把这个实例注册进去，作为默认实现
				return container.Register(typeof(IDatabase), null, db, id);
			}
			catch (Exception ex)
			{
				DAL.WriteLog(ex);
				throw;
			}
		}

		#endregion

		#region -- 默认提供者 --

		private static DictionaryCache<Type, IDatabase> defaultDbs2 = new DictionaryCache<Type, IDatabase>();

		/// <summary>根据名称获取默认提供者</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		internal static IDatabase GetDefault(Type dbType)
		{
			if (dbType == null) { return null; }
			return defaultDbs2.GetItem(dbType, dt => (IDatabase)dt.CreateInstance());
		}

		#endregion

		#region -- 方法 --

		/// <summary>从提供者和连接字符串猜测数据库处理器</summary>
		/// <param name="connStr"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		internal static Type GetProviderType(String connStr, String provider)
		{
			if (!provider.IsNullOrWhiteSpace())
			{
				var n = 0;
				foreach (var item in OrmLiteService.Container.ResolveAll(typeof(IDatabase)))
				{
					n++;
					if ("" + item.Identity == "") { continue; }

					var db = item.Instance as IDatabase;
					if (db != null && db.Support(provider)) { return item.ImplementType; }
				}

				if (DAL.Debug) { DAL.WriteLog("无法从{0}个默认数据库提供者中识别到{1}！", n, provider); }

				var type = provider.GetTypeEx(true);
				if (type != null) { OrmLiteService.Container.Register<IDatabase>(type, provider); }
				return type;
			}
			else
			{
				// 这里的默认值来自于上面Reg里面的最后那个
				return OrmLiteService.Container.ResolveType<IDatabase>(String.Empty);
			}
		}

		#endregion
	}
}