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
	/// <summary>数据模型</summary>
	//[Serializable]
	[DataObject]
	[Description("数据模型")]
	[BindIndex("PK__DataMode__3214EC27276EDEB3", true, "ID")]
	[BindIndex("IX_DataModel_Name", true, "Name")]
	[BindRelation("ID", true, "ModelTable", "DataModelID")]
	[BindTable("DataModel", Description = "数据模型", ConnName = "EmeSprite")]
	public partial class DataModel : IDataModel
	{
		#region 属性

		private String _Name;

		/// <summary>模型名称</summary>
		[DisplayName("模型名称")]
		[Description("模型名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(2, "Name", "模型名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Name
		{
			get { return _Name; }
			set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } }
		}

		private Boolean _IsSystem;

		/// <summary>系统</summary>
		[DisplayName("系统")]
		[Description("系统")]
		[DataObjectField(false, false, true)]
		[BindColumn(3, "IsSystem", "系统", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsSystem
		{
			get { return _IsSystem; }
			set { if (OnPropertyChanging(__.IsSystem, value)) { _IsSystem = value; OnPropertyChanged(__.IsSystem); } }
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

		private Boolean _IsEnable;

		/// <summary>有效</summary>
		[DisplayName("有效")]
		[Description("有效")]
		[DataObjectField(false, false, false)]
		[BindColumn(5, "IsEnabled", "有效", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsEnabled
		{
			get { return _IsEnable; }
			set { if (OnPropertyChanging(__.IsEnabled, value)) { _IsEnable = value; OnPropertyChanged(__.IsEnabled); } }
		}

		private String _ConnName;

		/// <summary>连接名</summary>
		[DisplayName("连接名")]
		[Description("连接名")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(6, "ConnName", "连接名", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String ConnName
		{
			get { return _ConnName; }
			set { if (OnPropertyChanging(__.ConnName, value)) { _ConnName = value; OnPropertyChanged(__.ConnName); } }
		}

		private String _NameSpace;

		/// <summary>命名空间</summary>
		[DisplayName("命名空间")]
		[Description("命名空间")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(7, "NameSpace", "命名空间", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String NameSpace
		{
			get { return _NameSpace; }
			set { if (OnPropertyChanging(__.NameSpace, value)) { _NameSpace = value; OnPropertyChanged(__.NameSpace); } }
		}

		private String _TablePrefix;

		/// <summary>表前缀</summary>
		[DisplayName("表前缀")]
		[Description("表前缀")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(8, "TablePrefix", "表前缀", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String TablePrefix
		{
			get { return _TablePrefix; }
			set { if (OnPropertyChanging(__.TablePrefix, value)) { _TablePrefix = value; OnPropertyChanged(__.TablePrefix); } }
		}

		private String _TemplatePath;

		/// <summary>模版路径</summary>
		[DisplayName("模版路径")]
		[Description("模版路径")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(9, "TemplatePath", "模版路径", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(10, "StylePath", "样式路径", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String StylePath
		{
			get { return _StylePath; }
			set { if (OnPropertyChanging(__.StylePath, value)) { _StylePath = value; OnPropertyChanged(__.StylePath); } }
		}

		private String _RenderConfig;

		/// <summary>生成配置</summary>
		[DisplayName("生成配置")]
		[Description("生成配置")]
		[DataObjectField(false, false, true, 1000)]
		[BindColumn(11, "RenderConfig", "生成配置", null, "nvarchar(1000)", CommonDbType.String, true)]
		public virtual String RenderConfig
		{
			get { return _RenderConfig; }
			set { if (OnPropertyChanging(__.RenderConfig, value)) { _RenderConfig = value; OnPropertyChanged(__.RenderConfig); } }
		}

		private String _Description;

		/// <summary>注释</summary>
		[DisplayName("注释")]
		[Description("注释")]
		[DataObjectField(false, false, true, 200)]
		[BindColumn(12, "Description", "注释", null, "nvarchar(200)", CommonDbType.String, true)]
		public virtual String Description
		{
			get { return _Description; }
			set { if (OnPropertyChanging(__.Description, value)) { _Description = value; OnPropertyChanged(__.Description); } }
		}

		private Boolean _IsDelete;

		/// <summary>逻辑删除</summary>
		[DisplayName("逻辑删除")]
		[Description("逻辑删除")]
		[DataObjectField(false, false, false)]
		[BindColumn(13, "IsDeleted", "逻辑删除", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsDeleted
		{
			get { return _IsDelete; }
			set { if (OnPropertyChanging(__.IsDeleted, value)) { _IsDelete = value; OnPropertyChanged(__.IsDeleted); } }
		}

		private Boolean _AllowEdit;

		/// <summary>允许编辑</summary>
		[DisplayName("允许编辑")]
		[Description("允许编辑")]
		[DataObjectField(false, false, false)]
		[BindColumn(14, "AllowEdit", "允许编辑", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(15, "AllowDelete", "允许删除", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(16, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(17, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(18, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(19, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(20, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(21, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.Name: return _Name;
					case __.IsSystem: return _IsSystem;
					case __.IsStatic: return _IsStatic;
					case __.IsEnabled: return _IsEnable;
					case __.ConnName: return _ConnName;
					case __.NameSpace: return _NameSpace;
					case __.TablePrefix: return _TablePrefix;
					case __.TemplatePath: return _TemplatePath;
					case __.StylePath: return _StylePath;
					case __.RenderConfig: return _RenderConfig;
					case __.Description: return _Description;
					case __.IsDeleted: return _IsDelete;
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
					case __.Name: _Name = Convert.ToString(value); break;
					case __.IsSystem: _IsSystem = Convert.ToBoolean(value); break;
					case __.IsStatic: _IsStatic = Convert.ToBoolean(value); break;
					case __.IsEnabled: _IsEnable = Convert.ToBoolean(value); break;
					case __.ConnName: _ConnName = Convert.ToString(value); break;
					case __.NameSpace: _NameSpace = Convert.ToString(value); break;
					case __.TablePrefix: _TablePrefix = Convert.ToString(value); break;
					case __.TemplatePath: _TemplatePath = Convert.ToString(value); break;
					case __.StylePath: _StylePath = Convert.ToString(value); break;
					case __.RenderConfig: _RenderConfig = Convert.ToString(value); break;
					case __.Description: _Description = Convert.ToString(value); break;
					case __.IsDeleted: _IsDelete = Convert.ToBoolean(value); break;
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

		/// <summary>取得数据模型字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>编号</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>模型名称</summary>
			public static readonly FieldItem Name = FindByName(__.Name);

			///<summary>系统</summary>
			public static readonly FieldItem IsSystem = FindByName(__.IsSystem);

			///<summary>静态</summary>
			public static readonly FieldItem IsStatic = FindByName(__.IsStatic);

			///<summary>有效</summary>
			public static readonly FieldItem IsEnabled = FindByName(__.IsEnabled);

			///<summary>连接名</summary>
			public static readonly FieldItem ConnName = FindByName(__.ConnName);

			///<summary>命名空间</summary>
			public static readonly FieldItem NameSpace = FindByName(__.NameSpace);

			///<summary>表前缀</summary>
			public static readonly FieldItem TablePrefix = FindByName(__.TablePrefix);

			///<summary>模版路径</summary>
			public static readonly FieldItem TemplatePath = FindByName(__.TemplatePath);

			///<summary>样式路径</summary>
			public static readonly FieldItem StylePath = FindByName(__.StylePath);

			///<summary>生成配置</summary>
			public static readonly FieldItem RenderConfig = FindByName(__.RenderConfig);

			///<summary>注释</summary>
			public static readonly FieldItem Description = FindByName(__.Description);

			///<summary>逻辑删除</summary>
			public static readonly FieldItem IsDeleted = FindByName(__.IsDeleted);

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

		/// <summary>取得数据模型字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>编号</summary>
			public const String ID = "ID";

			///<summary>模型名称</summary>
			public const String Name = "Name";

			///<summary>系统</summary>
			public const String IsSystem = "IsSystem";

			///<summary>静态</summary>
			public const String IsStatic = "IsStatic";

			///<summary>有效</summary>
			public const String IsEnabled = "IsEnabled";

			///<summary>连接名</summary>
			public const String ConnName = "ConnName";

			///<summary>命名空间</summary>
			public const String NameSpace = "NameSpace";

			///<summary>表前缀</summary>
			public const String TablePrefix = "TablePrefix";

			///<summary>模版路径</summary>
			public const String TemplatePath = "TemplatePath";

			///<summary>样式路径</summary>
			public const String StylePath = "StylePath";

			///<summary>生成配置</summary>
			public const String RenderConfig = "RenderConfig";

			///<summary>注释</summary>
			public const String Description = "Description";

			///<summary>逻辑删除</summary>
			public const String IsDeleted = "IsDeleted";

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

	/// <summary>数据模型接口</summary>
	public partial interface IDataModel
	{
		#region 属性

		/// <summary>编号</summary>
		Int32 ID { get; set; }

		/// <summary>模型名称</summary>
		String Name { get; set; }

		/// <summary>系统</summary>
		Boolean IsSystem { get; set; }

		/// <summary>静态</summary>
		Boolean IsStatic { get; set; }

		/// <summary>有效</summary>
		Boolean IsEnabled { get; set; }

		/// <summary>连接名</summary>
		String ConnName { get; set; }

		/// <summary>命名空间</summary>
		String NameSpace { get; set; }

		/// <summary>表前缀</summary>
		String TablePrefix { get; set; }

		/// <summary>模版路径</summary>
		String TemplatePath { get; set; }

		/// <summary>样式路径</summary>
		String StylePath { get; set; }

		/// <summary>生成配置</summary>
		String RenderConfig { get; set; }

		/// <summary>注释</summary>
		String Description { get; set; }

		/// <summary>逻辑删除</summary>
		Boolean IsDeleted { get; set; }

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