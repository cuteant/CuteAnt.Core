/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
//using CuteAnt.OrmLite.Accessors;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Model;

namespace CuteAnt.OrmLite.Model
{
	/// <summary>CuteAnt.OrmLite服务对象提供者</summary>
	internal class OrmLiteService //: ServiceContainer<HmCodeService>
	{
		#region -- 当前静态服务容器 --

		/// <summary>当前对象容器</summary>
		public static IObjectContainer Container
		{
			get { return ObjectContainer.Current; }
		}

		#endregion

		static OrmLiteService()
		{
			var container = Container;
			container.Register<IDataTable, OrmLiteTable>()
					//.AutoRegister<IDataRowEntityAccessorProvider, DataRowEntityAccessorProvider>()
					//.AutoRegister<IEntityPersistence, EntityPersistence>()
					.AutoRegister<IModelResolver, ModelResolver>()
					.AutoRegister<IEntityAddition, EntityAddition>();

			DbFactory.Reg(container);

			//EntityAccessorFactory.Reg(container);
		}

		#region 方法

		//public static Type ResolveType<TInterface>(Func<IObjectMap, Boolean> func)
		//{
		//	foreach (var item in Container.ResolveAllMaps(typeof(TInterface)))
		//	{
		//		if (func(item)) { return item.ImplementType; }
		//	}
		//	return null;
		//}

		#endregion

		#region 使用

		/// <summary>创建模型数据表</summary>
		/// <returns></returns>
		public static IDataTable CreateTable()
		{
			return Container.Resolve<IDataTable>();
		}

		///// <summary>创建实体类的数据行访问器</summary>
		///// <param name="entityType"></param>
		///// <returns></returns>
		//public static IDataRowEntityAccessor CreateDataRowEntityAccessor(Type entityType)
		//{
		//	return Container.ResolveInstance<IDataRowEntityAccessorProvider>().CreateDataRowEntityAccessor(entityType);
		//}

		#endregion
	}
}