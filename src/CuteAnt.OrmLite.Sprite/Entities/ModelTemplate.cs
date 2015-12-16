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
	/// <summary>实体模型模板</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型模板")]
	[BindIndex("PK__ModelTem__3214EC2708EA5793", true, "ID")]
	[BindIndex("IX_ModelTemplate_ModelTableID", false, "ModelTableID")]
	[BindIndex("IX_ModelTemplate_TemplateType", false, "ModelTableID,TemplateType")]
	[BindRelation("ModelViewID", false, "ModelView", "ID")]
	[BindRelation("ModelTableID", false, "ModelTable", "ID")]
	[BindTable("ModelTemplate", Description = "实体模型模板", ConnName = "EmeSprite")]
	public partial class ModelTemplate : IModelTemplate
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

		private Int32 _TemplateType;

		/// <summary>模板类型</summary>
		[DisplayName("模板类型")]
		[Description("模板类型")]
		[DataObjectField(false, false, false)]
		[BindColumn(3, "TemplateType", "模板类型", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 TemplateType
		{
			get { return _TemplateType; }
			set { if (OnPropertyChanging(__.TemplateType, value)) { _TemplateType = value; OnPropertyChanged(__.TemplateType); } }
		}

		private Boolean _IsStatic;

		/// <summary>静态</summary>
		[DisplayName("静态")]
		[Description("静态")]
		[DataObjectField(false, false, false)]
		[BindColumn(4, "IsStatic", "静态", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsStatic
		{
			get { return _IsStatic; }
			set { if (OnPropertyChanging(__.IsStatic, value)) { _IsStatic = value; OnPropertyChanged(__.IsStatic); } }
		}

		private Int32 _ModelViewID;

		/// <summary>所属视图</summary>
		[DisplayName("所属视图")]
		[Description("所属视图")]
		[DataObjectField(false, false, false)]
		[BindColumn(5, "ModelViewID", "所属视图", null, "int", CommonDbType.Integer, false)]
		public virtual Int32 ModelViewID
		{
			get { return _ModelViewID; }
			set { if (OnPropertyChanging(__.ModelViewID, value)) { _ModelViewID = value; OnPropertyChanged(__.ModelViewID); } }
		}

		private String _FormName;

		/// <summary>窗体名称</summary>
		[DisplayName("窗体名称")]
		[Description("窗体名称")]
		[DataObjectField(false, false, true, 100)]
		[BindColumn(6, "FormName", "窗体名称", null, "nvarchar(100)", CommonDbType.String, true)]
		public virtual String FormName
		{
			get { return _FormName; }
			set { if (OnPropertyChanging(__.FormName, value)) { _FormName = value; OnPropertyChanged(__.FormName); } }
		}

		private String _PermissionSets;

		/// <summary>权限集</summary>
		[DisplayName("权限集")]
		[Description("权限集")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(7, "PermissionSets", "权限集", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String PermissionSets
		{
			get { return _PermissionSets; }
			set { if (OnPropertyChanging(__.PermissionSets, value)) { _PermissionSets = value; OnPropertyChanged(__.PermissionSets); } }
		}

		private Int32 _Sort;

		/// <summary>排序</summary>
		[DisplayName("排序")]
		[Description("排序")]
		[DataObjectField(false, false, true)]
		[BindColumn(8, "Sort", "排序", null, "int", CommonDbType.Integer, false)]
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
		[BindColumn(9, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(10, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(11, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(12, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(13, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(14, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.TemplateType: return _TemplateType;
					case __.IsStatic: return _IsStatic;
					case __.ModelViewID: return _ModelViewID;
					case __.FormName: return _FormName;
					case __.PermissionSets: return _PermissionSets;
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
					case __.TemplateType: _TemplateType = Convert.ToInt32(value); break;
					case __.IsStatic: _IsStatic = Convert.ToBoolean(value); break;
					case __.ModelViewID: _ModelViewID = Convert.ToInt32(value); break;
					case __.FormName: _FormName = Convert.ToString(value); break;
					case __.PermissionSets: _PermissionSets = Convert.ToString(value); break;
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

		/// <summary>取得实体模型模板字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>主键</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>实体模型</summary>
			public static readonly FieldItem ModelTableID = FindByName(__.ModelTableID);

			///<summary>模板类型</summary>
			public static readonly FieldItem TemplateType = FindByName(__.TemplateType);

			///<summary>静态</summary>
			public static readonly FieldItem IsStatic = FindByName(__.IsStatic);

			///<summary>所属视图</summary>
			public static readonly FieldItem ModelViewID = FindByName(__.ModelViewID);

			///<summary>窗体名称</summary>
			public static readonly FieldItem FormName = FindByName(__.FormName);

			///<summary>权限集</summary>
			public static readonly FieldItem PermissionSets = FindByName(__.PermissionSets);

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

		/// <summary>取得实体模型模板字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>主键</summary>
			public const String ID = "ID";

			///<summary>实体模型</summary>
			public const String ModelTableID = "ModelTableID";

			///<summary>模板类型</summary>
			public const String TemplateType = "TemplateType";

			///<summary>静态</summary>
			public const String IsStatic = "IsStatic";

			///<summary>所属视图</summary>
			public const String ModelViewID = "ModelViewID";

			///<summary>窗体名称</summary>
			public const String FormName = "FormName";

			///<summary>权限集</summary>
			public const String PermissionSets = "PermissionSets";

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

	/// <summary>实体模型模板接口</summary>
	public partial interface IModelTemplate
	{
		#region 属性

		/// <summary>主键</summary>
		Int32 ID { get; set; }

		/// <summary>实体模型</summary>
		Int32 ModelTableID { get; set; }

		/// <summary>模板类型</summary>
		Int32 TemplateType { get; set; }

		/// <summary>静态</summary>
		Boolean IsStatic { get; set; }

		/// <summary>所属视图</summary>
		Int32 ModelViewID { get; set; }

		/// <summary>窗体名称</summary>
		String FormName { get; set; }

		/// <summary>权限集</summary>
		String PermissionSets { get; set; }

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