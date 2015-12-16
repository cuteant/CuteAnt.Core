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
	/// <summary>实体模型视图排序规则</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型视图排序规则")]
	[BindIndex("PK__ModelOrd__3214EC277F60ED59", true, "ID")]
	[BindIndex("IX_ModelOrderClause_ModelTableID", false, "ModelViewID")]
	[BindIndex("IX_ModelOrderClause_ModelColumnName", true, "ModelViewID,ColumnName")]
	[BindRelation("ModelViewID", false, "ModelView", "ID")]
	[BindTable("ModelOrderClause", Description = "实体模型视图排序规则", ConnName = "EmeSprite")]
	public partial class ModelOrderClause : IModelOrderClause
	{
		#region 属性

		private Int32 _ModelViewID;

		/// <summary>视图</summary>
		[DisplayName("视图")]
		[Description("视图")]
		[DataObjectField(false, false, false)]
		[BindColumn(2, "ModelViewID", "视图", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 ModelViewID
		{
			get { return _ModelViewID; }
			set { if (OnPropertyChanging(__.ModelViewID, value)) { _ModelViewID = value; OnPropertyChanged(__.ModelViewID); } }
		}

		private String _ColumnName;

		/// <summary>字段名称</summary>
		[DisplayName("字段名称")]
		[Description("字段名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(3, "ColumnName", "字段名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String ColumnName
		{
			get { return _ColumnName; }
			set { if (OnPropertyChanging(__.ColumnName, value)) { _ColumnName = value; OnPropertyChanged(__.ColumnName); } }
		}

		private Int32 _OrderType;

		/// <summary>排序方式</summary>
		[DisplayName("排序方式")]
		[Description("排序方式")]
		[DataObjectField(false, false, false)]
		[BindColumn(4, "OrderType", "排序方式", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 OrderType
		{
			get { return _OrderType; }
			set { if (OnPropertyChanging(__.OrderType, value)) { _OrderType = value; OnPropertyChanged(__.OrderType); } }
		}

		private Int32 _Sort;

		/// <summary>排序</summary>
		[DisplayName("排序")]
		[Description("排序")]
		[DataObjectField(false, false, false)]
		[BindColumn(5, "Sort", "排序", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(6, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(7, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(8, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(9, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(10, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(11, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.ModelViewID: return _ModelViewID;
					case __.ColumnName: return _ColumnName;
					case __.OrderType: return _OrderType;
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
					case __.ModelViewID: _ModelViewID = Convert.ToInt32(value); break;
					case __.ColumnName: _ColumnName = Convert.ToString(value); break;
					case __.OrderType: _OrderType = Convert.ToInt32(value); break;
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

		/// <summary>取得实体模型视图排序规则字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>主键</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>视图</summary>
			public static readonly FieldItem ModelViewID = FindByName(__.ModelViewID);

			///<summary>字段名称</summary>
			public static readonly FieldItem ColumnName = FindByName(__.ColumnName);

			///<summary>排序方式</summary>
			public static readonly FieldItem OrderType = FindByName(__.OrderType);

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

		/// <summary>取得实体模型视图排序规则字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>主键</summary>
			public const String ID = "ID";

			///<summary>视图</summary>
			public const String ModelViewID = "ModelViewID";

			///<summary>字段名称</summary>
			public const String ColumnName = "ColumnName";

			///<summary>排序方式</summary>
			public const String OrderType = "OrderType";

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

	/// <summary>实体模型视图排序规则接口</summary>
	public partial interface IModelOrderClause
	{
		#region 属性

		/// <summary>主键</summary>
		Int32 ID { get; set; }

		/// <summary>视图</summary>
		Int32 ModelViewID { get; set; }

		/// <summary>字段名称</summary>
		String ColumnName { get; set; }

		/// <summary>排序方式</summary>
		Int32 OrderType { get; set; }

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