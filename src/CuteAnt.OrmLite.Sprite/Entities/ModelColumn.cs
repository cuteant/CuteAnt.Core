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
	/// <summary>实体模型字段</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型字段")]
	[BindIndex("PK__ModelCol__3214EC273C69FB99", true, "ID")]
	[BindIndex("IX_ModelColumn_ModelTableID", false, "ModelTableID")]
	[BindIndex("IX_ModelColumnName", true, "ModelTableID,Name")]
	[BindRelation("ModelTableID", false, "ModelTable", "ID")]
	[BindTable("ModelColumn", Description = "实体模型字段", ConnName = "EmeSprite")]
	public partial class ModelColumn : IModelColumn
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

		/// <summary>字段名称</summary>
		[DisplayName("字段名称")]
		[Description("字段名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(3, "Name", "字段名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Name
		{
			get { return _Name; }
			set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } }
		}

		private String _ColumnName;

		/// <summary>数据列名称</summary>
		[DisplayName("数据列名称")]
		[Description("数据列名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(4, "ColumnName", "数据列名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String ColumnName
		{
			get { return _ColumnName; }
			set { if (OnPropertyChanging(__.ColumnName, value)) { _ColumnName = value; OnPropertyChanged(__.ColumnName); } }
		}

		private String _DataType;

		/// <summary>数据类型</summary>
		[DisplayName("数据类型")]
		[Description("数据类型")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(5, "DataType", "数据类型", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String DataType
		{
			get { return _DataType; }
			set { if (OnPropertyChanging(__.DataType, value)) { _DataType = value; OnPropertyChanged(__.DataType); } }
		}

		private String _RawType;

		/// <summary>原始类型</summary>
		[DisplayName("原始类型")]
		[Description("原始类型")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(6, "RawType", "原始类型", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String RawType
		{
			get { return _RawType; }
			set { if (OnPropertyChanging(__.RawType, value)) { _RawType = value; OnPropertyChanged(__.RawType); } }
		}

		private Int32 _DbType;

		/// <summary>通用数据库数据类型</summary>
		[DisplayName("通用数据库数据类型")]
		[Description("通用数据库数据类型")]
		[DataObjectField(false, false, true)]
		[BindColumn(7, "DbType", "通用数据库数据类型", null, "int", CommonDbType.Integer, false)]
		public virtual Int32 DbType
		{
			get { return _DbType; }
			set { if (OnPropertyChanging(__.DbType, value)) { _DbType = value; OnPropertyChanged(__.DbType); } }
		}

		private Boolean _Identity;

		/// <summary>标识</summary>
		[DisplayName("标识")]
		[Description("标识")]
		[DataObjectField(false, false, false)]
		[BindColumn(8, "Identity", "标识", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean Identity
		{
			get { return _Identity; }
			set { if (OnPropertyChanging(__.Identity, value)) { _Identity = value; OnPropertyChanged(__.Identity); } }
		}

		private Boolean _PrimaryKey;

		/// <summary>主键</summary>
		[DisplayName("主键")]
		[Description("主键")]
		[DataObjectField(false, false, false)]
		[BindColumn(9, "PrimaryKey", "主键", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean PrimaryKey
		{
			get { return _PrimaryKey; }
			set { if (OnPropertyChanging(__.PrimaryKey, value)) { _PrimaryKey = value; OnPropertyChanged(__.PrimaryKey); } }
		}

		private Int32 _ControlType;

		/// <summary>字段类型</summary>
		[DisplayName("字段类型")]
		[Description("字段类型")]
		[DataObjectField(false, false, false)]
		[BindColumn(10, "ControlType", "字段类型", "1", "int", CommonDbType.Integer, false)]
		public virtual Int32 ControlType
		{
			get { return _ControlType; }
			set { if (OnPropertyChanging(__.ControlType, value)) { _ControlType = value; OnPropertyChanged(__.ControlType); } }
		}

		private Int32 _Length;

		/// <summary>长度</summary>
		[DisplayName("长度")]
		[Description("长度")]
		[DataObjectField(false, false, false)]
		[BindColumn(11, "Length", "长度", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 Length
		{
			get { return _Length; }
			set { if (OnPropertyChanging(__.Length, value)) { _Length = value; OnPropertyChanged(__.Length); } }
		}

		private Int32 _Precision;

		/// <summary>精度</summary>
		[DisplayName("精度")]
		[Description("精度")]
		[DataObjectField(false, false, false)]
		[BindColumn(12, "Precision", "精度", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 Precision
		{
			get { return _Precision; }
			set { if (OnPropertyChanging(__.Precision, value)) { _Precision = value; OnPropertyChanged(__.Precision); } }
		}

		private Int32 _Scale;

		/// <summary>小数位数</summary>
		[DisplayName("小数位数")]
		[Description("小数位数")]
		[DataObjectField(false, false, false)]
		[BindColumn(13, "Scale", "小数位数", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 Scale
		{
			get { return _Scale; }
			set { if (OnPropertyChanging(__.Scale, value)) { _Scale = value; OnPropertyChanged(__.Scale); } }
		}

		private Boolean _Nullable;

		/// <summary>允许空</summary>
		[DisplayName("允许空")]
		[Description("允许空")]
		[DataObjectField(false, false, false)]
		[BindColumn(14, "Nullable", "允许空", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean Nullable
		{
			get { return _Nullable; }
			set { if (OnPropertyChanging(__.Nullable, value)) { _Nullable = value; OnPropertyChanged(__.Nullable); } }
		}

		private Boolean _IsUnicode;

		/// <summary>Unicode</summary>
		[DisplayName("Unicode")]
		[Description("Unicode")]
		[DataObjectField(false, false, false)]
		[BindColumn(15, "IsUnicode", "Unicode", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsUnicode
		{
			get { return _IsUnicode; }
			set { if (OnPropertyChanging(__.IsUnicode, value)) { _IsUnicode = value; OnPropertyChanged(__.IsUnicode); } }
		}

		private String _Selects;

		/// <summary>选项</summary>
		[DisplayName("选项")]
		[Description("选项")]
		[DataObjectField(false, false, true, 200)]
		[BindColumn(16, "Selects", "选项", null, "nvarchar(200)", CommonDbType.String, true)]
		public virtual String Selects
		{
			get { return _Selects; }
			set { if (OnPropertyChanging(__.Selects, value)) { _Selects = value; OnPropertyChanged(__.Selects); } }
		}

		private String _Default;

		/// <summary>默认值</summary>
		[DisplayName("默认值")]
		[Description("默认值")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(17, "Default", "默认值", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Default
		{
			get { return _Default; }
			set { if (OnPropertyChanging(__.Default, value)) { _Default = value; OnPropertyChanged(__.Default); } }
		}

		private Int32 _Sort;

		/// <summary>排序</summary>
		[DisplayName("排序")]
		[Description("排序")]
		[DataObjectField(false, false, false)]
		[BindColumn(18, "Sort", "排序", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 Sort
		{
			get { return _Sort; }
			set { if (OnPropertyChanging(__.Sort, value)) { _Sort = value; OnPropertyChanged(__.Sort); } }
		}

		private String _Description;

		/// <summary>注释</summary>
		[DisplayName("注释")]
		[Description("注释")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(19, "Description", "注释", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Description
		{
			get { return _Description; }
			set { if (OnPropertyChanging(__.Description, value)) { _Description = value; OnPropertyChanged(__.Description); } }
		}

		private Boolean _ReadOnly;

		/// <summary>只读</summary>
		[DisplayName("只读")]
		[Description("只读")]
		[DataObjectField(false, false, false)]
		[BindColumn(20, "ReadOnly", "只读", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean ReadOnly
		{
			get { return _ReadOnly; }
			set { if (OnPropertyChanging(__.ReadOnly, value)) { _ReadOnly = value; OnPropertyChanged(__.ReadOnly); } }
		}

		private String _BindModel;

		/// <summary>绑定模型</summary>
		[DisplayName("绑定模型")]
		[Description("绑定模型")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(21, "BindModel", "绑定模型", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String BindModel
		{
			get { return _BindModel; }
			set { if (OnPropertyChanging(__.BindModel, value)) { _BindModel = value; OnPropertyChanged(__.BindModel); } }
		}

		private String _BindTable;

		/// <summary>绑定实体模型</summary>
		[DisplayName("绑定实体模型")]
		[Description("绑定实体模型")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(22, "BindTable", "绑定实体模型", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String BindTable
		{
			get { return _BindTable; }
			set { if (OnPropertyChanging(__.BindTable, value)) { _BindTable = value; OnPropertyChanged(__.BindTable); } }
		}

		private String _BindField;

		/// <summary>绑定字段</summary>
		[DisplayName("绑定字段")]
		[Description("绑定字段")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(23, "BindField", "绑定字段", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String BindField
		{
			get { return _BindField; }
			set { if (OnPropertyChanging(__.BindField, value)) { _BindField = value; OnPropertyChanged(__.BindField); } }
		}

		private Boolean _IsModelBinding;

		/// <summary>是否关联绑定模型</summary>
		[DisplayName("是否关联绑定模型")]
		[Description("是否关联绑定模型")]
		[DataObjectField(false, false, true)]
		[BindColumn(24, "IsModelBinding", "是否关联绑定模型", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsModelBinding
		{
			get { return _IsModelBinding; }
			set { if (OnPropertyChanging(__.IsModelBinding, value)) { _IsModelBinding = value; OnPropertyChanged(__.IsModelBinding); } }
		}

		private Boolean _AllowNormalSearch;

		/// <summary>允许普通搜索</summary>
		[DisplayName("允许普通搜索")]
		[Description("允许普通搜索")]
		[DataObjectField(false, false, true)]
		[BindColumn(25, "AllowNormalSearch", "允许普通搜索", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowNormalSearch
		{
			get { return _AllowNormalSearch; }
			set { if (OnPropertyChanging(__.AllowNormalSearch, value)) { _AllowNormalSearch = value; OnPropertyChanged(__.AllowNormalSearch); } }
		}

		private Boolean _AllowAdvSearch;

		/// <summary>允许高级搜索</summary>
		[DisplayName("允许高级搜索")]
		[Description("允许高级搜索")]
		[DataObjectField(false, false, true)]
		[BindColumn(26, "AllowAdvSearch", "允许高级搜索", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowAdvSearch
		{
			get { return _AllowAdvSearch; }
			set { if (OnPropertyChanging(__.AllowAdvSearch, value)) { _AllowAdvSearch = value; OnPropertyChanged(__.AllowAdvSearch); } }
		}

		private Boolean _AllowImport;

		/// <summary>允许导入</summary>
		[DisplayName("允许导入")]
		[Description("允许导入")]
		[DataObjectField(false, false, false)]
		[BindColumn(27, "AllowImport", "允许导入", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(28, "AllowExport", "允许导出", "1", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowExport
		{
			get { return _AllowExport; }
			set { if (OnPropertyChanging(__.AllowExport, value)) { _AllowExport = value; OnPropertyChanged(__.AllowExport); } }
		}

		private Boolean _IsTracking;

		/// <summary>跟踪</summary>
		[DisplayName("跟踪")]
		[Description("跟踪")]
		[DataObjectField(false, false, true)]
		[BindColumn(29, "IsTracking", "跟踪", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean IsTracking
		{
			get { return _IsTracking; }
			set { if (OnPropertyChanging(__.IsTracking, value)) { _IsTracking = value; OnPropertyChanged(__.IsTracking); } }
		}

		private Boolean _AllowEdit;

		/// <summary>允许编辑</summary>
		[DisplayName("允许编辑")]
		[Description("允许编辑")]
		[DataObjectField(false, false, false)]
		[BindColumn(30, "AllowEdit", "允许编辑", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(31, "AllowDelete", "允许删除", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(32, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(33, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(34, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(35, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(36, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(37, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.ColumnName: return _ColumnName;
					case __.DataType: return _DataType;
					case __.RawType: return _RawType;
					case __.DbType: return _DbType;
					case __.Identity: return _Identity;
					case __.PrimaryKey: return _PrimaryKey;
					case __.ControlType: return _ControlType;
					case __.Length: return _Length;
					case __.Precision: return _Precision;
					case __.Scale: return _Scale;
					case __.Nullable: return _Nullable;
					case __.IsUnicode: return _IsUnicode;
					case __.Selects: return _Selects;
					case __.Default: return _Default;
					case __.Sort: return _Sort;
					case __.Description: return _Description;
					case __.ReadOnly: return _ReadOnly;
					case __.BindModel: return _BindModel;
					case __.BindTable: return _BindTable;
					case __.BindField: return _BindField;
					case __.IsModelBinding: return _IsModelBinding;
					case __.AllowNormalSearch: return _AllowNormalSearch;
					case __.AllowAdvSearch: return _AllowAdvSearch;
					case __.AllowImport: return _AllowImport;
					case __.AllowExport: return _AllowExport;
					case __.IsTracking: return _IsTracking;
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
					case __.ModelTableID: _ModelTableID = Convert.ToInt32(value); break;
					case __.Name: _Name = Convert.ToString(value); break;
					case __.ColumnName: _ColumnName = Convert.ToString(value); break;
					case __.DataType: _DataType = Convert.ToString(value); break;
					case __.RawType: _RawType = Convert.ToString(value); break;
					case __.DbType: _DbType = Convert.ToInt32(value); break;
					case __.Identity: _Identity = Convert.ToBoolean(value); break;
					case __.PrimaryKey: _PrimaryKey = Convert.ToBoolean(value); break;
					case __.ControlType: _ControlType = Convert.ToInt32(value); break;
					case __.Length: _Length = Convert.ToInt32(value); break;
					case __.Precision: _Precision = Convert.ToInt32(value); break;
					case __.Scale: _Scale = Convert.ToInt32(value); break;
					case __.Nullable: _Nullable = Convert.ToBoolean(value); break;
					case __.IsUnicode: _IsUnicode = Convert.ToBoolean(value); break;
					case __.Selects: _Selects = Convert.ToString(value); break;
					case __.Default: _Default = Convert.ToString(value); break;
					case __.Sort: _Sort = Convert.ToInt32(value); break;
					case __.Description: _Description = Convert.ToString(value); break;
					case __.ReadOnly: _ReadOnly = Convert.ToBoolean(value); break;
					case __.BindModel: _BindModel = Convert.ToString(value); break;
					case __.BindTable: _BindTable = Convert.ToString(value); break;
					case __.BindField: _BindField = Convert.ToString(value); break;
					case __.IsModelBinding: _IsModelBinding = Convert.ToBoolean(value); break;
					case __.AllowNormalSearch: _AllowNormalSearch = Convert.ToBoolean(value); break;
					case __.AllowAdvSearch: _AllowAdvSearch = Convert.ToBoolean(value); break;
					case __.AllowImport: _AllowImport = Convert.ToBoolean(value); break;
					case __.AllowExport: _AllowExport = Convert.ToBoolean(value); break;
					case __.IsTracking: _IsTracking = Convert.ToBoolean(value); break;
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

		/// <summary>取得实体模型字段字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>编号</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>实体模型</summary>
			public static readonly FieldItem ModelTableID = FindByName(__.ModelTableID);

			///<summary>字段名称</summary>
			public static readonly FieldItem Name = FindByName(__.Name);

			///<summary>数据列名称</summary>
			public static readonly FieldItem ColumnName = FindByName(__.ColumnName);

			///<summary>数据类型</summary>
			public static readonly FieldItem DataType = FindByName(__.DataType);

			///<summary>原始类型</summary>
			public static readonly FieldItem RawType = FindByName(__.RawType);

			///<summary>通用数据库数据类型</summary>
			public static readonly FieldItem DbType = FindByName(__.DbType);

			///<summary>标识</summary>
			public static readonly FieldItem Identity = FindByName(__.Identity);

			///<summary>主键</summary>
			public static readonly FieldItem PrimaryKey = FindByName(__.PrimaryKey);

			///<summary>字段类型</summary>
			public static readonly FieldItem ControlType = FindByName(__.ControlType);

			///<summary>长度</summary>
			public static readonly FieldItem Length = FindByName(__.Length);

			///<summary>精度</summary>
			public static readonly FieldItem Precision = FindByName(__.Precision);

			///<summary>小数位数</summary>
			public static readonly FieldItem Scale = FindByName(__.Scale);

			///<summary>允许空</summary>
			public static readonly FieldItem Nullable = FindByName(__.Nullable);

			///<summary>Unicode</summary>
			public static readonly FieldItem IsUnicode = FindByName(__.IsUnicode);

			///<summary>选项</summary>
			public static readonly FieldItem Selects = FindByName(__.Selects);

			///<summary>默认值</summary>
			public static readonly FieldItem Default = FindByName(__.Default);

			///<summary>排序</summary>
			public static readonly FieldItem Sort = FindByName(__.Sort);

			///<summary>注释</summary>
			public static readonly FieldItem Description = FindByName(__.Description);

			///<summary>只读</summary>
			public static readonly FieldItem ReadOnly = FindByName(__.ReadOnly);

			///<summary>绑定模型</summary>
			public static readonly FieldItem BindModel = FindByName(__.BindModel);

			///<summary>绑定实体模型</summary>
			public static readonly FieldItem BindTable = FindByName(__.BindTable);

			///<summary>绑定字段</summary>
			public static readonly FieldItem BindField = FindByName(__.BindField);

			///<summary>是否关联绑定模型</summary>
			public static readonly FieldItem IsModelBinding = FindByName(__.IsModelBinding);

			///<summary>允许普通搜索</summary>
			public static readonly FieldItem AllowNormalSearch = FindByName(__.AllowNormalSearch);

			///<summary>允许高级搜索</summary>
			public static readonly FieldItem AllowAdvSearch = FindByName(__.AllowAdvSearch);

			///<summary>允许导入</summary>
			public static readonly FieldItem AllowImport = FindByName(__.AllowImport);

			///<summary>允许导出</summary>
			public static readonly FieldItem AllowExport = FindByName(__.AllowExport);

			///<summary>跟踪</summary>
			public static readonly FieldItem IsTracking = FindByName(__.IsTracking);

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

		/// <summary>取得实体模型字段字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>编号</summary>
			public const String ID = "ID";

			///<summary>实体模型</summary>
			public const String ModelTableID = "ModelTableID";

			///<summary>字段名称</summary>
			public const String Name = "Name";

			///<summary>数据列名称</summary>
			public const String ColumnName = "ColumnName";

			///<summary>数据类型</summary>
			public const String DataType = "DataType";

			///<summary>原始类型</summary>
			public const String RawType = "RawType";

			///<summary>通用数据库数据类型</summary>
			public const String DbType = "DbType";

			///<summary>标识</summary>
			public const String Identity = "Identity";

			///<summary>主键</summary>
			public const String PrimaryKey = "PrimaryKey";

			///<summary>字段类型</summary>
			public const String ControlType = "ControlType";

			///<summary>长度</summary>
			public const String Length = "Length";

			///<summary>精度</summary>
			public const String Precision = "Precision";

			///<summary>小数位数</summary>
			public const String Scale = "Scale";

			///<summary>允许空</summary>
			public const String Nullable = "Nullable";

			///<summary>Unicode</summary>
			public const String IsUnicode = "IsUnicode";

			///<summary>选项</summary>
			public const String Selects = "Selects";

			///<summary>默认值</summary>
			public const String Default = "Default";

			///<summary>排序</summary>
			public const String Sort = "Sort";

			///<summary>注释</summary>
			public const String Description = "Description";

			///<summary>只读</summary>
			public const String ReadOnly = "ReadOnly";

			///<summary>绑定模型</summary>
			public const String BindModel = "BindModel";

			///<summary>绑定实体模型</summary>
			public const String BindTable = "BindTable";

			///<summary>绑定字段</summary>
			public const String BindField = "BindField";

			///<summary>是否关联绑定模型</summary>
			public const String IsModelBinding = "IsModelBinding";

			///<summary>允许普通搜索</summary>
			public const String AllowNormalSearch = "AllowNormalSearch";

			///<summary>允许高级搜索</summary>
			public const String AllowAdvSearch = "AllowAdvSearch";

			///<summary>允许导入</summary>
			public const String AllowImport = "AllowImport";

			///<summary>允许导出</summary>
			public const String AllowExport = "AllowExport";

			///<summary>跟踪</summary>
			public const String IsTracking = "IsTracking";

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

	/// <summary>实体模型字段接口</summary>
	public partial interface IModelColumn
	{
		#region 属性

		/// <summary>编号</summary>
		Int32 ID { get; set; }

		/// <summary>实体模型</summary>
		Int32 ModelTableID { get; set; }

		/// <summary>字段名称</summary>
		String Name { get; set; }

		/// <summary>数据列名称</summary>
		String ColumnName { get; set; }

		/// <summary>数据类型</summary>
		String DataType { get; set; }

		/// <summary>原始类型</summary>
		String RawType { get; set; }

		/// <summary>通用数据库数据类型</summary>
		Int32 DbType { get; set; }

		/// <summary>标识</summary>
		Boolean Identity { get; set; }

		/// <summary>主键</summary>
		Boolean PrimaryKey { get; set; }

		/// <summary>字段类型</summary>
		Int32 ControlType { get; set; }

		/// <summary>长度</summary>
		Int32 Length { get; set; }

		/// <summary>精度</summary>
		Int32 Precision { get; set; }

		/// <summary>小数位数</summary>
		Int32 Scale { get; set; }

		/// <summary>允许空</summary>
		Boolean Nullable { get; set; }

		/// <summary>Unicode</summary>
		Boolean IsUnicode { get; set; }

		/// <summary>选项</summary>
		String Selects { get; set; }

		/// <summary>默认值</summary>
		String Default { get; set; }

		/// <summary>排序</summary>
		Int32 Sort { get; set; }

		/// <summary>注释</summary>
		String Description { get; set; }

		/// <summary>只读</summary>
		Boolean ReadOnly { get; set; }

		/// <summary>绑定模型</summary>
		String BindModel { get; set; }

		/// <summary>绑定实体模型</summary>
		String BindTable { get; set; }

		/// <summary>绑定字段</summary>
		String BindField { get; set; }

		/// <summary>是否关联绑定模型</summary>
		Boolean IsModelBinding { get; set; }

		/// <summary>允许普通搜索</summary>
		Boolean AllowNormalSearch { get; set; }

		/// <summary>允许高级搜索</summary>
		Boolean AllowAdvSearch { get; set; }

		/// <summary>允许导入</summary>
		Boolean AllowImport { get; set; }

		/// <summary>允许导出</summary>
		Boolean AllowExport { get; set; }

		/// <summary>跟踪</summary>
		Boolean IsTracking { get; set; }

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