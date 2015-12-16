/*
 * EmeCoder v1.2.1.168
 * 作者：Administrator/PC4APPLE
 * 时间：2014-09-18 22:57:33
 * 版权：版权所有 (C) Eme Development Team 2014
*/

﻿using System;
using System.ComponentModel;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>实体模型视图</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型视图")]
	[BindIndex("PK__ModelVie__3214EC270F975522", true, "ID")]
	[BindIndex("IX_ModelView_Name", true, "ModelTableID,Name")]
	[BindIndex("IX_ModelView_ModelTableID", false, "ModelTableID")]
	[BindRelation("ID", true, "ModelTemplate", "ModelViewID")]
	[BindRelation("ModelTableID", false, "ModelTable", "ID")]
	[BindRelation("ID", true, "ModelViewColumn", "ModelViewID")]
	[BindRelation("ID", true, "ModelOrderClause", "ModelViewID")]
	[BindTable("ModelView", Description = "实体模型视图", ConnName = "EmeSprite")]
	public partial class ModelView : IModelView
	{
		#region 属性

		private Int32 _ModelTableID;

		/// <summary>实体模型</summary>
		[DisplayName("实体模型")]
		[Description("实体模型")]
		[DataObjectField(false, false, false)]
		[BindColumn(2, "ModelTableID", "实体模型", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 ModelTableID
		{
			get { return _ModelTableID; }
			set { if (OnPropertyChanging(__.ModelTableID, value)) { _ModelTableID = value; OnPropertyChanged(__.ModelTableID); } }
		}

		private String _Name;

		/// <summary>视图名称</summary>
		[DisplayName("视图名称")]
		[Description("视图名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(3, "Name", "视图名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Name
		{
			get { return _Name; }
			set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } }
		}

		private String _Description;

		/// <summary>注释</summary>
		[DisplayName("注释")]
		[Description("注释")]
		[DataObjectField(false, false, true, 200)]
		[BindColumn(4, "Description", "注释", null, "nvarchar(200)", CommonDbType.String, true)]
		public virtual String Description
		{
			get { return _Description; }
			set { if (OnPropertyChanging(__.Description, value)) { _Description = value; OnPropertyChanged(__.Description); } }
		}

		private String _WhereClause;

		/// <summary>视图规则</summary>
		[DisplayName("视图规则")]
		[Description("视图规则")]
		[DataObjectField(false, false, true)]
		[BindColumn(5, "WhereClause", "视图规则", null, "ntext", CommonDbType.Text, true)]
		public virtual String WhereClause
		{
			get { return _WhereClause; }
			set { if (OnPropertyChanging(__.WhereClause, value)) { _WhereClause = value; OnPropertyChanged(__.WhereClause); } }
		}

		private String _WhereClauseSql;

		/// <summary>查询规则SQL语句</summary>
		[DisplayName("查询规则SQL语句")]
		[Description("查询规则SQL语句")]
		[DataObjectField(false, false, true, 800)]
		[BindColumn(6, "WhereClauseSql", "查询规则SQL语句", null, "nvarchar(800)", CommonDbType.String, true)]
		public virtual String WhereClauseSql
		{
			get { return _WhereClauseSql; }
			set { if (OnPropertyChanging(__.WhereClauseSql, value)) { _WhereClauseSql = value; OnPropertyChanged(__.WhereClauseSql); } }
		}

		private Int32 _Sort;

		/// <summary>排序</summary>
		[DisplayName("排序")]
		[Description("排序")]
		[DataObjectField(false, false, false)]
		[BindColumn(7, "Sort", "排序", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 Sort
		{
			get { return _Sort; }
			set { if (OnPropertyChanging(__.Sort, value)) { _Sort = value; OnPropertyChanged(__.Sort); } }
		}

		private DateTime _ModifiedOn;

		/// <summary>修改时间</summary>
		[DisplayName("修改时间")]
		[Description("修改时间")]
		[DataObjectField(false, false, false)]
		[BindColumn(8, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
		public virtual DateTime ModifiedTime
		{
			get { return _ModifiedOn; }
			set { if (OnPropertyChanging(__.ModifiedTime, value)) { _ModifiedOn = value; OnPropertyChanged(__.ModifiedTime); } }
		}

		private Int32 _ModifiedUserID;

		/// <summary>修改用户</summary>
		[DisplayName("修改用户")]
		[Description("修改用户")]
		[DataObjectField(false, false, false)]
		[BindColumn(9, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 ModifiedByUserID
		{
			get { return _ModifiedUserID; }
			set { if (OnPropertyChanging(__.ModifiedByUserID, value)) { _ModifiedUserID = value; OnPropertyChanged(__.ModifiedByUserID); } }
		}

		private String _ModifiedBy;

		/// <summary>修改用户名</summary>
		[DisplayName("修改用户名")]
		[Description("修改用户名")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(10, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String ModifiedByUser
		{
			get { return _ModifiedBy; }
			set { if (OnPropertyChanging(__.ModifiedByUser, value)) { _ModifiedBy = value; OnPropertyChanged(__.ModifiedByUser); } }
		}

		private DateTime _CreateOn;

		/// <summary>创建时间</summary>
		[DisplayName("创建时间")]
		[Description("创建时间")]
		[DataObjectField(false, false, false)]
		[BindColumn(11, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
		public virtual DateTime CreatedTime
		{
			get { return _CreateOn; }
			set { if (OnPropertyChanging(__.CreatedTime, value)) { _CreateOn = value; OnPropertyChanged(__.CreatedTime); } }
		}

		private Int32 _CreateUserID;

		/// <summary>创建用户</summary>
		[DisplayName("创建用户")]
		[Description("创建用户")]
		[DataObjectField(false, false, false)]
		[BindColumn(12, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 CreatedByUserID
		{
			get { return _CreateUserID; }
			set { if (OnPropertyChanging(__.CreatedByUserID, value)) { _CreateUserID = value; OnPropertyChanged(__.CreatedByUserID); } }
		}

		private String _CreateBy;

		/// <summary>创建用户名</summary>
		[DisplayName("创建用户名")]
		[Description("创建用户名")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(13, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CreatedByUser
		{
			get { return _CreateBy; }
			set { if (OnPropertyChanging(__.CreatedByUser, value)) { _CreateBy = value; OnPropertyChanged(__.CreatedByUser); } }
		}

		#endregion

		#region 获取/设置 字段值

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				switch (name)
				{
					//case __.ID: return _ID;
					case __.ModelTableID: return _ModelTableID;
					case __.Name: return _Name;
					case __.Description: return _Description;
					case __.WhereClause: return _WhereClause;
					case __.WhereClauseSql: return _WhereClauseSql;
					case __.Sort: return _Sort;
					case __.ModifiedTime: return _ModifiedOn;
					case __.ModifiedByUserID: return _ModifiedUserID;
					case __.ModifiedByUser: return _ModifiedBy;
					case __.CreatedTime: return _CreateOn;
					case __.CreatedByUserID: return _CreateUserID;
					case __.CreatedByUser: return _CreateBy;
					default: return base[name];
				}
			}
			set
			{
				switch (name)
				{
					//case __.ID: _ID = Convert.ToInt32(value); break;
					case __.ModelTableID: _ModelTableID = Convert.ToInt32(value); break;
					case __.Name: _Name = Convert.ToString(value); break;
					case __.Description: _Description = Convert.ToString(value); break;
					case __.WhereClause: _WhereClause = Convert.ToString(value); break;
					case __.WhereClauseSql: _WhereClauseSql = Convert.ToString(value); break;
					case __.Sort: _Sort = Convert.ToInt32(value); break;
					case __.ModifiedTime: _ModifiedOn = Convert.ToDateTime(value); break;
					case __.ModifiedByUserID: _ModifiedUserID = Convert.ToInt32(value); break;
					case __.ModifiedByUser: _ModifiedBy = Convert.ToString(value); break;
					case __.CreatedTime: _CreateOn = Convert.ToDateTime(value); break;
					case __.CreatedByUserID: _CreateUserID = Convert.ToInt32(value); break;
					case __.CreatedByUser: _CreateBy = Convert.ToString(value); break;
					default: base[name] = value; break;
				}
			}
		}

		#endregion

		#region 字段名

		/// <summary>取得实体模型视图字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>主键</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>实体模型</summary>
			public static readonly FieldItem ModelTableID = FindByName(__.ModelTableID);

			///<summary>视图名称</summary>
			public static readonly FieldItem Name = FindByName(__.Name);

			///<summary>注释</summary>
			public static readonly FieldItem Description = FindByName(__.Description);

			///<summary>视图规则</summary>
			public static readonly FieldItem WhereClause = FindByName(__.WhereClause);

			///<summary>查询规则SQL语句</summary>
			public static readonly FieldItem WhereClauseSql = FindByName(__.WhereClauseSql);

			///<summary>排序</summary>
			public static readonly FieldItem Sort = FindByName(__.Sort);

			///<summary>修改时间</summary>
			public static readonly FieldItem ModifiedTime = FindByName(__.ModifiedTime);

			///<summary>修改用户</summary>
			public static readonly FieldItem ModifiedByUserID = FindByName(__.ModifiedByUserID);

			///<summary>修改用户名</summary>
			public static readonly FieldItem ModifiedByUser = FindByName(__.ModifiedByUser);

			///<summary>创建时间</summary>
			public static readonly FieldItem CreatedTime = FindByName(__.CreatedTime);

			///<summary>创建用户</summary>
			public static readonly FieldItem CreatedByUserID = FindByName(__.CreatedByUserID);

			///<summary>创建用户名</summary>
			public static readonly FieldItem CreatedByUser = FindByName(__.CreatedByUser);

			private static FieldItem FindByName(String name)
			{
				return Meta.Table.FindByName(name);
			}
		}

		/// <summary>取得实体模型视图字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>主键</summary>
			public const String ID = "ID";

			///<summary>实体模型</summary>
			public const String ModelTableID = "ModelTableID";

			///<summary>视图名称</summary>
			public const String Name = "Name";

			///<summary>注释</summary>
			public const String Description = "Description";

			///<summary>视图规则</summary>
			public const String WhereClause = "WhereClause";

			///<summary>查询规则SQL语句</summary>
			public const String WhereClauseSql = "WhereClauseSql";

			///<summary>排序</summary>
			public const String Sort = "Sort";

			///<summary>修改时间</summary>
			public const String ModifiedTime = "ModifiedTime";

			///<summary>修改用户</summary>
			public const String ModifiedByUserID = "ModifiedByUserID";

			///<summary>修改用户名</summary>
			public const String ModifiedByUser = "ModifiedByUser";

			///<summary>创建时间</summary>
			public const String CreatedTime = "CreatedTime";

			///<summary>创建用户</summary>
			public const String CreatedByUserID = "CreatedByUserID";

			///<summary>创建用户名</summary>
			public const String CreatedByUser = "CreatedByUser";
		}

		#endregion
	}

	/// <summary>实体模型视图接口</summary>
	public partial interface IModelView
	{
		#region 属性

		/// <summary>主键</summary>
		Int32 ID { get; set; }

		/// <summary>实体模型</summary>
		Int32 ModelTableID { get; set; }

		/// <summary>视图名称</summary>
		String Name { get; set; }

		/// <summary>注释</summary>
		String Description { get; set; }

		/// <summary>视图规则</summary>
		String WhereClause { get; set; }

		/// <summary>查询规则SQL语句</summary>
		String WhereClauseSql { get; set; }

		/// <summary>排序</summary>
		Int32 Sort { get; set; }

		/// <summary>修改时间</summary>
		DateTime ModifiedTime { get; set; }

		/// <summary>修改用户</summary>
		Int32 ModifiedByUserID { get; set; }

		/// <summary>修改用户名</summary>
		String ModifiedByUser { get; set; }

		/// <summary>创建时间</summary>
		DateTime CreatedTime { get; set; }

		/// <summary>创建用户</summary>
		Int32 CreatedByUserID { get; set; }

		/// <summary>创建用户名</summary>
		String CreatedByUser { get; set; }

		#endregion

		#region 获取/设置 字段值

		/// <summary>获取/设置 字段值。</summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		Object this[String name] { get; set; }

		#endregion
	}
}