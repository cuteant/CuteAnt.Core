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
	/// <summary>实体模型</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型")]
	[BindIndex("PK__ModelTab__3214EC2759FA5E80", true, "ID")]
	[BindIndex("IX_ModelTable_DataModelID", false, "DataModelID")]
	[BindIndex("IX_ModelTable", true, "DataModelID,Name")]
	[BindRelation("ID", true, "ModelView", "ModelTableID")]
	[BindRelation("ID", true, "ModelTemplate", "ModelTableID")]
	[BindRelation("DataModelID", false, "DataModel", "ID")]
	[BindRelation("ID", true, "ModelRelation", "ModelTableID")]
	[BindRelation("ID", true, "ModelIndex", "ModelTableID")]
	[BindRelation("ID", true, "ModelColumn", "ModelTableID")]
	[BindTable("ModelTable", Description = "实体模型", ConnName = "EmeSprite")]
	public partial class ModelTable : IModelTable
	{
		#region 属性

		private Int32 _DataModelID;

		/// <summary>数据模型</summary>
		[DisplayName("数据模型")]
		[Description("数据模型")]
		[DataObjectField(false, false, false)]
		[BindColumn(2, "DataModelID", "数据模型", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 DataModelID
		{
			get { return _DataModelID; }
			set { if (OnPropertyChanging(__.DataModelID, value)) { _DataModelID = value; OnPropertyChanged(__.DataModelID); } }
		}

		private String _Name;

		/// <summary>名称</summary>
		[DisplayName("名称")]
		[Description("名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(3, "Name", "名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Name
		{
			get { return _Name; }
			set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } }
		}

		private String _TableName;

		/// <summary>数据表名称</summary>
		[DisplayName("数据表名称")]
		[Description("数据表名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(4, "TableName", "数据表名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String TableName
		{
			get { return _TableName; }
			set { if (OnPropertyChanging(__.TableName, value)) { _TableName = value; OnPropertyChanged(__.TableName); } }
		}

		private String _FormatedName;

		/// <summary>格式化名称</summary>
		[DisplayName("格式化名称")]
		[Description("格式化名称")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(5, "FormatedName", "格式化名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String FormatedName
		{
			get { return _FormatedName; }
			set { if (OnPropertyChanging(__.FormatedName, value)) { _FormatedName = value; OnPropertyChanged(__.FormatedName); } }
		}

		private Boolean _IsSystem;

		/// <summary>系统</summary>
		[DisplayName("系统")]
		[Description("系统")]
		[DataObjectField(false, false, true)]
		[BindColumn(6, "IsSystem", "系统", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsSystem
		{
			get { return _IsSystem; }
			set { if (OnPropertyChanging(__.IsSystem, value)) { _IsSystem = value; OnPropertyChanged(__.IsSystem); } }
		}

		private Boolean _IsPublic;

		/// <summary>公开</summary>
		[DisplayName("公开")]
		[Description("公开")]
		[DataObjectField(false, false, true)]
		[BindColumn(7, "IsPublic", "公开", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsPublic
		{
			get { return _IsPublic; }
			set { if (OnPropertyChanging(__.IsPublic, value)) { _IsPublic = value; OnPropertyChanged(__.IsPublic); } }
		}

		private String _Owner;

		/// <summary>所有者</summary>
		[DisplayName("所有者")]
		[Description("所有者")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(8, "Owner", "所有者", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Owner
		{
			get { return _Owner; }
			set { if (OnPropertyChanging(__.Owner, value)) { _Owner = value; OnPropertyChanged(__.Owner); } }
		}

		private Int32 _DbType;

		/// <summary>数据库类型</summary>
		[DisplayName("数据库类型")]
		[Description("数据库类型")]
		[DataObjectField(false, false, false)]
		[BindColumn(9, "DbType", "数据库类型", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 DbType
		{
			get { return _DbType; }
			set { if (OnPropertyChanging(__.DbType, value)) { _DbType = value; OnPropertyChanged(__.DbType); } }
		}

		private Boolean _IsView;

		/// <summary>视图</summary>
		[DisplayName("视图")]
		[Description("视图")]
		[DataObjectField(false, false, false)]
		[BindColumn(10, "IsView", "视图", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsView
		{
			get { return _IsView; }
			set { if (OnPropertyChanging(__.IsView, value)) { _IsView = value; OnPropertyChanged(__.IsView); } }
		}

		private String _BaseType;

		/// <summary>基类</summary>
		[DisplayName("基类")]
		[Description("基类")]
		[DataObjectField(false, false, true, 100)]
		[BindColumn(11, "BaseType", "基类", null, "nvarchar(100)", CommonDbType.String, true)]
		public virtual String BaseType
		{
			get { return _BaseType; }
			set { if (OnPropertyChanging(__.BaseType, value)) { _BaseType = value; OnPropertyChanged(__.BaseType); } }
		}

		private String _TemplatePath;

		/// <summary>模版路径</summary>
		[DisplayName("模版路径")]
		[Description("模版路径")]
		[DataObjectField(false, false, true, 250)]
		[BindColumn(12, "TemplatePath", "模版路径", null, "nvarchar(250)", CommonDbType.String, true)]
		public virtual String TemplatePath
		{
			get { return _TemplatePath; }
			set { if (OnPropertyChanging(__.TemplatePath, value)) { _TemplatePath = value; OnPropertyChanged(__.TemplatePath); } }
		}

		private String _StylePath;

		/// <summary>样式路径</summary>
		[DisplayName("样式路径")]
		[Description("样式路径")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(13, "StylePath", "样式路径", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String StylePath
		{
			get { return _StylePath; }
			set { if (OnPropertyChanging(__.StylePath, value)) { _StylePath = value; OnPropertyChanged(__.StylePath); } }
		}

		private Int32 _Sort;

		/// <summary>排序</summary>
		[DisplayName("排序")]
		[Description("排序")]
		[DataObjectField(false, false, false)]
		[BindColumn(14, "Sort", "排序", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 Sort
		{
			get { return _Sort; }
			set { if (OnPropertyChanging(__.Sort, value)) { _Sort = value; OnPropertyChanged(__.Sort); } }
		}

		private String _Description;

		/// <summary>注释</summary>
		[DisplayName("注释")]
		[Description("注释")]
		[DataObjectField(false, false, true, 200)]
		[BindColumn(15, "Description", "注释", null, "nvarchar(200)", CommonDbType.String, true)]
		public virtual String Description
		{
			get { return _Description; }
			set { if (OnPropertyChanging(__.Description, value)) { _Description = value; OnPropertyChanged(__.Description); } }
		}

		private String _PrimaryColumn;

		/// <summary>主显示字段</summary>
		[DisplayName("主显示字段")]
		[Description("主显示字段")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(16, "PrimaryColumn", "主显示字段", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String PrimaryColumn
		{
			get { return _PrimaryColumn; }
			set { if (OnPropertyChanging(__.PrimaryColumn, value)) { _PrimaryColumn = value; OnPropertyChanged(__.PrimaryColumn); } }
		}

		private String _Template;

		/// <summary>模版</summary>
		[DisplayName("模版")]
		[Description("模版")]
		[DataObjectField(false, false, true)]
		[BindColumn(17, "Template", "模版", null, "ntext", CommonDbType.Text, true)]
		public virtual String Template
		{
			get { return _Template; }
			set { if (OnPropertyChanging(__.Template, value)) { _Template = value; OnPropertyChanged(__.Template); } }
		}

		private Boolean _AllowImport;

		/// <summary>允许导入</summary>
		[DisplayName("允许导入")]
		[Description("允许导入")]
		[DataObjectField(false, false, false)]
		[BindColumn(18, "AllowImport", "允许导入", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowImport
		{
			get { return _AllowImport; }
			set { if (OnPropertyChanging(__.AllowImport, value)) { _AllowImport = value; OnPropertyChanged(__.AllowImport); } }
		}

		private Boolean _AllowExport;

		/// <summary>允许导出</summary>
		[DisplayName("允许导出")]
		[Description("允许导出")]
		[DataObjectField(false, false, false)]
		[BindColumn(19, "AllowExport", "允许导出", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowExport
		{
			get { return _AllowExport; }
			set { if (OnPropertyChanging(__.AllowExport, value)) { _AllowExport = value; OnPropertyChanged(__.AllowExport); } }
		}

		private Boolean _AllLogicalDelete;

		/// <summary>允许逻辑删除</summary>
		[DisplayName("允许逻辑删除")]
		[Description("允许逻辑删除")]
		[DataObjectField(false, false, true)]
		[BindColumn(20, "AllLogicalDelete", "允许逻辑删除", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllLogicalDelete
		{
			get { return _AllLogicalDelete; }
			set { if (OnPropertyChanging(__.AllLogicalDelete, value)) { _AllLogicalDelete = value; OnPropertyChanged(__.AllLogicalDelete); } }
		}

		private Boolean _IsTreeEntity;

		/// <summary>树型实体</summary>
		[DisplayName("树型实体")]
		[Description("树型实体")]
		[DataObjectField(false, false, false)]
		[BindColumn(21, "IsTreeEntity", "树型实体", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsTreeEntity
		{
			get { return _IsTreeEntity; }
			set { if (OnPropertyChanging(__.IsTreeEntity, value)) { _IsTreeEntity = value; OnPropertyChanged(__.IsTreeEntity); } }
		}

		private Boolean _IsTrackingRecords;

		/// <summary>记录跟踪</summary>
		[DisplayName("记录跟踪")]
		[Description("记录跟踪")]
		[DataObjectField(false, false, true)]
		[BindColumn(22, "IsTrackingRecords", "记录跟踪", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsTrackingRecords
		{
			get { return _IsTrackingRecords; }
			set { if (OnPropertyChanging(__.IsTrackingRecords, value)) { _IsTrackingRecords = value; OnPropertyChanged(__.IsTrackingRecords); } }
		}

		private Boolean _IsAttachCreateUserPermissions;

		/// <summary>是否附加创建用户权限</summary>
		[DisplayName("是否附加创建用户权限")]
		[Description("是否附加创建用户权限")]
		[DataObjectField(false, false, true)]
		[BindColumn(23, "IsAttachCreateUserPermissions", "是否附加创建用户权限", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsAttachCreateUserPermissions
		{
			get { return _IsAttachCreateUserPermissions; }
			set { if (OnPropertyChanging(__.IsAttachCreateUserPermissions, value)) { _IsAttachCreateUserPermissions = value; OnPropertyChanged(__.IsAttachCreateUserPermissions); } }
		}

		private Boolean _AllowEdit;

		/// <summary>允许编辑</summary>
		[DisplayName("允许编辑")]
		[Description("允许编辑")]
		[DataObjectField(false, false, false)]
		[BindColumn(24, "AllowEdit", "允许编辑", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowEdit
		{
			get { return _AllowEdit; }
			set { if (OnPropertyChanging(__.AllowEdit, value)) { _AllowEdit = value; OnPropertyChanged(__.AllowEdit); } }
		}

		private Boolean _AllowDelete;

		/// <summary>允许删除</summary>
		[DisplayName("允许删除")]
		[Description("允许删除")]
		[DataObjectField(false, false, false)]
		[BindColumn(25, "AllowDelete", "允许删除", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowDelete
		{
			get { return _AllowDelete; }
			set { if (OnPropertyChanging(__.AllowDelete, value)) { _AllowDelete = value; OnPropertyChanged(__.AllowDelete); } }
		}

		private DateTime _ModifiedOn;

		/// <summary>修改时间</summary>
		[DisplayName("修改时间")]
		[Description("修改时间")]
		[DataObjectField(false, false, false)]
		[BindColumn(26, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(27, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(28, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(29, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(30, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(31, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.DataModelID: return _DataModelID;
					case __.Name: return _Name;
					case __.TableName: return _TableName;
					case __.FormatedName: return _FormatedName;
					case __.IsSystem: return _IsSystem;
					case __.IsPublic: return _IsPublic;
					case __.Owner: return _Owner;
					case __.DbType: return _DbType;
					case __.IsView: return _IsView;
					case __.BaseType: return _BaseType;
					case __.TemplatePath: return _TemplatePath;
					case __.StylePath: return _StylePath;
					case __.Sort: return _Sort;
					case __.Description: return _Description;
					case __.PrimaryColumn: return _PrimaryColumn;
					case __.Template: return _Template;
					case __.AllowImport: return _AllowImport;
					case __.AllowExport: return _AllowExport;
					case __.AllLogicalDelete: return _AllLogicalDelete;
					case __.IsTreeEntity: return _IsTreeEntity;
					case __.IsTrackingRecords: return _IsTrackingRecords;
					case __.IsAttachCreateUserPermissions: return _IsAttachCreateUserPermissions;
					case __.AllowEdit: return _AllowEdit;
					case __.AllowDelete: return _AllowDelete;
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
					case __.DataModelID: _DataModelID = Convert.ToInt32(value); break;
					case __.Name: _Name = Convert.ToString(value); break;
					case __.TableName: _TableName = Convert.ToString(value); break;
					case __.FormatedName: _FormatedName = Convert.ToString(value); break;
					case __.IsSystem: _IsSystem = Convert.ToBoolean(value); break;
					case __.IsPublic: _IsPublic = Convert.ToBoolean(value); break;
					case __.Owner: _Owner = Convert.ToString(value); break;
					case __.DbType: _DbType = Convert.ToInt32(value); break;
					case __.IsView: _IsView = Convert.ToBoolean(value); break;
					case __.BaseType: _BaseType = Convert.ToString(value); break;
					case __.TemplatePath: _TemplatePath = Convert.ToString(value); break;
					case __.StylePath: _StylePath = Convert.ToString(value); break;
					case __.Sort: _Sort = Convert.ToInt32(value); break;
					case __.Description: _Description = Convert.ToString(value); break;
					case __.PrimaryColumn: _PrimaryColumn = Convert.ToString(value); break;
					case __.Template: _Template = Convert.ToString(value); break;
					case __.AllowImport: _AllowImport = Convert.ToBoolean(value); break;
					case __.AllowExport: _AllowExport = Convert.ToBoolean(value); break;
					case __.AllLogicalDelete: _AllLogicalDelete = Convert.ToBoolean(value); break;
					case __.IsTreeEntity: _IsTreeEntity = Convert.ToBoolean(value); break;
					case __.IsTrackingRecords: _IsTrackingRecords = Convert.ToBoolean(value); break;
					case __.IsAttachCreateUserPermissions: _IsAttachCreateUserPermissions = Convert.ToBoolean(value); break;
					case __.AllowEdit: _AllowEdit = Convert.ToBoolean(value); break;
					case __.AllowDelete: _AllowDelete = Convert.ToBoolean(value); break;
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

		/// <summary>取得实体模型字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>编号</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>数据模型</summary>
			public static readonly FieldItem DataModelID = FindByName(__.DataModelID);

			///<summary>名称</summary>
			public static readonly FieldItem Name = FindByName(__.Name);

			///<summary>数据表名称</summary>
			public static readonly FieldItem TableName = FindByName(__.TableName);

			///<summary>格式化名称</summary>
			public static readonly FieldItem FormatedName = FindByName(__.FormatedName);

			///<summary>系统</summary>
			public static readonly FieldItem IsSystem = FindByName(__.IsSystem);

			///<summary>公开</summary>
			public static readonly FieldItem IsPublic = FindByName(__.IsPublic);

			///<summary>所有者</summary>
			public static readonly FieldItem Owner = FindByName(__.Owner);

			///<summary>数据库类型</summary>
			public static readonly FieldItem DbType = FindByName(__.DbType);

			///<summary>视图</summary>
			public static readonly FieldItem IsView = FindByName(__.IsView);

			///<summary>基类</summary>
			public static readonly FieldItem BaseType = FindByName(__.BaseType);

			///<summary>模版路径</summary>
			public static readonly FieldItem TemplatePath = FindByName(__.TemplatePath);

			///<summary>样式路径</summary>
			public static readonly FieldItem StylePath = FindByName(__.StylePath);

			///<summary>排序</summary>
			public static readonly FieldItem Sort = FindByName(__.Sort);

			///<summary>注释</summary>
			public static readonly FieldItem Description = FindByName(__.Description);

			///<summary>主显示字段</summary>
			public static readonly FieldItem PrimaryColumn = FindByName(__.PrimaryColumn);

			///<summary>模版</summary>
			public static readonly FieldItem Template = FindByName(__.Template);

			///<summary>允许导入</summary>
			public static readonly FieldItem AllowImport = FindByName(__.AllowImport);

			///<summary>允许导出</summary>
			public static readonly FieldItem AllowExport = FindByName(__.AllowExport);

			///<summary>允许逻辑删除</summary>
			public static readonly FieldItem AllLogicalDelete = FindByName(__.AllLogicalDelete);

			///<summary>树型实体</summary>
			public static readonly FieldItem IsTreeEntity = FindByName(__.IsTreeEntity);

			///<summary>记录跟踪</summary>
			public static readonly FieldItem IsTrackingRecords = FindByName(__.IsTrackingRecords);

			///<summary>是否附加创建用户权限</summary>
			public static readonly FieldItem IsAttachCreateUserPermissions = FindByName(__.IsAttachCreateUserPermissions);

			///<summary>允许编辑</summary>
			public static readonly FieldItem AllowEdit = FindByName(__.AllowEdit);

			///<summary>允许删除</summary>
			public static readonly FieldItem AllowDelete = FindByName(__.AllowDelete);

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

		/// <summary>取得实体模型字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>编号</summary>
			public const String ID = "ID";

			///<summary>数据模型</summary>
			public const String DataModelID = "DataModelID";

			///<summary>名称</summary>
			public const String Name = "Name";

			///<summary>数据表名称</summary>
			public const String TableName = "TableName";

			///<summary>格式化名称</summary>
			public const String FormatedName = "FormatedName";

			///<summary>系统</summary>
			public const String IsSystem = "IsSystem";

			///<summary>公开</summary>
			public const String IsPublic = "IsPublic";

			///<summary>所有者</summary>
			public const String Owner = "Owner";

			///<summary>数据库类型</summary>
			public const String DbType = "DbType";

			///<summary>视图</summary>
			public const String IsView = "IsView";

			///<summary>基类</summary>
			public const String BaseType = "BaseType";

			///<summary>模版路径</summary>
			public const String TemplatePath = "TemplatePath";

			///<summary>样式路径</summary>
			public const String StylePath = "StylePath";

			///<summary>排序</summary>
			public const String Sort = "Sort";

			///<summary>注释</summary>
			public const String Description = "Description";

			///<summary>主显示字段</summary>
			public const String PrimaryColumn = "PrimaryColumn";

			///<summary>模版</summary>
			public const String Template = "Template";

			///<summary>允许导入</summary>
			public const String AllowImport = "AllowImport";

			///<summary>允许导出</summary>
			public const String AllowExport = "AllowExport";

			///<summary>允许逻辑删除</summary>
			public const String AllLogicalDelete = "AllLogicalDelete";

			///<summary>树型实体</summary>
			public const String IsTreeEntity = "IsTreeEntity";

			///<summary>记录跟踪</summary>
			public const String IsTrackingRecords = "IsTrackingRecords";

			///<summary>是否附加创建用户权限</summary>
			public const String IsAttachCreateUserPermissions = "IsAttachCreateUserPermissions";

			///<summary>允许编辑</summary>
			public const String AllowEdit = "AllowEdit";

			///<summary>允许删除</summary>
			public const String AllowDelete = "AllowDelete";

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

	/// <summary>实体模型接口</summary>
	public partial interface IModelTable
	{
		#region 属性

		/// <summary>编号</summary>
		Int32 ID { get; set; }

		/// <summary>数据模型</summary>
		Int32 DataModelID { get; set; }

		/// <summary>名称</summary>
		String Name { get; set; }

		/// <summary>数据表名称</summary>
		String TableName { get; set; }

		/// <summary>格式化名称</summary>
		String FormatedName { get; set; }

		/// <summary>系统</summary>
		Boolean IsSystem { get; set; }

		/// <summary>公开</summary>
		Boolean IsPublic { get; set; }

		/// <summary>所有者</summary>
		String Owner { get; set; }

		/// <summary>数据库类型</summary>
		Int32 DbType { get; set; }

		/// <summary>视图</summary>
		Boolean IsView { get; set; }

		/// <summary>基类</summary>
		String BaseType { get; set; }

		/// <summary>模版路径</summary>
		String TemplatePath { get; set; }

		/// <summary>样式路径</summary>
		String StylePath { get; set; }

		/// <summary>排序</summary>
		Int32 Sort { get; set; }

		/// <summary>注释</summary>
		String Description { get; set; }

		/// <summary>主显示字段</summary>
		String PrimaryColumn { get; set; }

		/// <summary>模版</summary>
		String Template { get; set; }

		/// <summary>允许导入</summary>
		Boolean AllowImport { get; set; }

		/// <summary>允许导出</summary>
		Boolean AllowExport { get; set; }

		/// <summary>允许逻辑删除</summary>
		Boolean AllLogicalDelete { get; set; }

		/// <summary>树型实体</summary>
		Boolean IsTreeEntity { get; set; }

		/// <summary>记录跟踪</summary>
		Boolean IsTrackingRecords { get; set; }

		/// <summary>是否附加创建用户权限</summary>
		Boolean IsAttachCreateUserPermissions { get; set; }

		/// <summary>允许编辑</summary>
		Boolean AllowEdit { get; set; }

		/// <summary>允许删除</summary>
		Boolean AllowDelete { get; set; }

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