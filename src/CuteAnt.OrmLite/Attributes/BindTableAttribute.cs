﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>指定实体类所绑定的数据表信息。</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class BindTableAttribute : Attribute
	{
		private String _Name;

		/// <summary>
		/// 表名。
		/// 可以在配置文件中通过ORMConfigInfo.ConnMaps把实体映射到别的数据表上
		/// </summary>
		public String Name { get { return _Name; } set { _Name = value; } }

		private String _Description;

		/// <summary>描述</summary>
		public String Description { get { return _Description; } set { _Description = value; } }

		private String _ConnName;

		/// <summary>
		/// 连接名。
		/// 实体类的所有数据库操作，将发生在该连接名指定的数据库连接上。
		/// 此外，可动态修改实体类在当前线程上的连接名（改Meta.ConnName）；
		/// 也可以在配置文件中通过ORMConfigInfo.ConnMaps把连接名映射到别的连接上。
		/// </summary>
		public String ConnName { get { return _ConnName; } set { _ConnName = value; } }

		private DatabaseType _DbType = DatabaseType.SQLServer;

		/// <summary>
		/// 数据库类型。
		/// 仅用于记录实体类由何种类型数据库生成，当且仅当目标数据库同为该数据库类型时，采用实体属性信息上的RawType作为反向工程的目标字段类型，以期获得开发和生产的最佳兼容。
		/// </summary>
		public DatabaseType DbType { get { return _DbType; } set { _DbType = value; } }

		private Boolean _IsView;

		/// <summary>是否视图</summary>
		public Boolean IsView { get { return _IsView; } set { _IsView = value; } }

		/// <summary>构造函数</summary>
		/// <param name="name">表名</param>
		public BindTableAttribute(String name)
		{
			Name = name;
		}

		/// <summary>构造函数</summary>
		/// <param name="name">表名</param>
		/// <param name="description">描述</param>
		public BindTableAttribute(String name, String description)
		{
			Name = name;
			Description = description;
		}

		/// <summary>构造函数</summary>
		/// <param name="name">表名</param>
		/// <param name="description">描述</param>
		/// <param name="connName"></param>
		/// <param name="isView"></param>
		public BindTableAttribute(String name, String description, String connName, Boolean isView)
		{
			Name = name;
			Description = description;
			ConnName = connName;
			IsView = isView;
		}

		///// <summary>构造函数</summary>
		///// <param name="name">表名</param>
		///// <param name="description">描述</param>
		///// <param name="connName"></param>
		///// <param name="dbType"></param>
		///// <param name="isView"></param>
		//public BindTableAttribute(String name, String description, String connName, DatabaseType dbType, Boolean isView)
		//{
		//	Name = name;
		//	Description = description;
		//	ConnName = connName;
		//	DbType = dbType;
		//	IsView = isView;
		//}
	}
}