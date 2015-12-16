/*
 * EmeCoder v1.2.1.168
 * 作者：Administrator/PC4APPLE
 * 时间：2014-10-25 14:05:34
 * 版权：版权所有 (C) Eme Development Team 2014
*/

﻿using System;
using System.ComponentModel;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>实体模型索引</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型索引")]
	[BindIndex("PK__ModelInd__3214EC275070F446", true, "ID")]
	[BindIndex("IX_ModelIndex_Columns", true, "ModelTableID,Columns")]
	[BindIndex("IX_ModelIndex_ModelTableID", false, "ModelTableID")]
	[BindRelation("ModelTableID", false, "ModelTable", "ID")]
	[BindTable("ModelIndex", Description = "实体模型索引", ConnName = "EmeSprite")]
	public partial class ModelIndex : IModelIndex
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

		/// <summary>索引名称</summary>
		[DisplayName("索引名称")]
		[Description("索引名称")]
		[DataObjectField(false, false, false, 50)]
		[BindColumn(3, "Name", "索引名称", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String Name
		{
			get { return _Name; }
			set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } }
		}

		private String _Columns;

		/// <summary>字段名称</summary>
		[DisplayName("字段名称")]
		[Description("字段名称")]
		[DataObjectField(false, false, false, 250)]
		[BindColumn(4, "Columns", "字段名称", null, "nvarchar(250)", CommonDbType.String, true)]
		public virtual String Columns
		{
			get { return _Columns; }
			set { if (OnPropertyChanging(__.Columns, value)) { _Columns = value; OnPropertyChanged(__.Columns); } }
		}

		private Boolean _Unique;

		/// <summary>唯一</summary>
		[DisplayName("唯一")]
		[Description("唯一")]
		[DataObjectField(false, false, false)]
		[BindColumn(5, "Unique", "唯一", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean Unique
		{
			get { return _Unique; }
			set { if (OnPropertyChanging(__.Unique, value)) { _Unique = value; OnPropertyChanged(__.Unique); } }
		}

		private Boolean _PrimaryKey;

		/// <summary>主键</summary>
		[DisplayName("主键")]
		[Description("主键")]
		[DataObjectField(false, false, false)]
		[BindColumn(6, "PrimaryKey", "主键", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean PrimaryKey
		{
			get { return _PrimaryKey; }
			set { if (OnPropertyChanging(__.PrimaryKey, value)) { _PrimaryKey = value; OnPropertyChanged(__.PrimaryKey); } }
		}

		private Boolean _Clustered;

		/// <summary>是否聚集索引</summary>
		[DisplayName("是否聚集索引")]
		[Description("是否聚集索引")]
		[DataObjectField(false, false, true)]
		[BindColumn(7, "Clustered", "是否聚集索引", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean Clustered
		{
			get { return _Clustered; }
			set { if (OnPropertyChanging(__.Clustered, value)) { _Clustered = value; OnPropertyChanged(__.Clustered); } }
		}

		private Boolean _Computed;

		/// <summary>是否计算产生</summary>
		[DisplayName("是否计算产生")]
		[Description("是否计算产生")]
		[DataObjectField(false, false, false)]
		[BindColumn(8, "Computed", "是否计算产生", "0", "bit", CommonDbType.Boolean, false)]
		public virtual Boolean Computed
		{
			get { return _Computed; }
			set { if (OnPropertyChanging(__.Computed, value)) { _Computed = value; OnPropertyChanged(__.Computed); } }
		}

		private Boolean _AllowEdit;

		/// <summary>允许编辑</summary>
		[DisplayName("允许编辑")]
		[Description("允许编辑")]
		[DataObjectField(false, false, false)]
		[BindColumn(9, "AllowEdit", "允许编辑", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(10, "AllowDelete", "允许删除", "1", "bit", CommonDbType.Boolean, false)]
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
		[BindColumn(11, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(12, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(13, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(14, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(15, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(16, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.Columns: return _Columns;
					case __.Unique: return _Unique;
					case __.PrimaryKey: return _PrimaryKey;
					case __.Clustered: return _Clustered;
					case __.Computed: return _Computed;
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
					case __.Columns: _Columns = Convert.ToString(value); break;
					case __.Unique: _Unique = Convert.ToBoolean(value); break;
					case __.PrimaryKey: _PrimaryKey = Convert.ToBoolean(value); break;
					case __.Clustered: _Clustered = Convert.ToBoolean(value); break;
					case __.Computed: _Computed = Convert.ToBoolean(value); break;
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

		/// <summary>取得实体模型索引字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>编号</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>实体模型</summary>
			public static readonly FieldItem ModelTableID = FindByName(__.ModelTableID);

			///<summary>索引名称</summary>
			public static readonly FieldItem Name = FindByName(__.Name);

			///<summary>字段名称</summary>
			public static readonly FieldItem Columns = FindByName(__.Columns);

			///<summary>唯一</summary>
			public static readonly FieldItem Unique = FindByName(__.Unique);

			///<summary>主键</summary>
			public static readonly FieldItem PrimaryKey = FindByName(__.PrimaryKey);

			///<summary>是否聚集索引</summary>
			public static readonly FieldItem Clustered = FindByName(__.Clustered);

			///<summary>是否计算产生</summary>
			public static readonly FieldItem Computed = FindByName(__.Computed);

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

		/// <summary>取得实体模型索引字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>编号</summary>
			public const String ID = "ID";

			///<summary>实体模型</summary>
			public const String ModelTableID = "ModelTableID";

			///<summary>索引名称</summary>
			public const String Name = "Name";

			///<summary>字段名称</summary>
			public const String Columns = "Columns";

			///<summary>唯一</summary>
			public const String Unique = "Unique";

			///<summary>主键</summary>
			public const String PrimaryKey = "PrimaryKey";

			///<summary>是否聚集索引</summary>
			public const String Clustered = "Clustered";

			///<summary>是否计算产生</summary>
			public const String Computed = "Computed";

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

	/// <summary>实体模型索引接口</summary>
	public partial interface IModelIndex
	{
		#region 属性

		/// <summary>编号</summary>
		Int32 ID { get; set; }

		/// <summary>实体模型</summary>
		Int32 ModelTableID { get; set; }

		/// <summary>索引名称</summary>
		String Name { get; set; }

		/// <summary>字段名称</summary>
		String Columns { get; set; }

		/// <summary>唯一</summary>
		Boolean Unique { get; set; }

		/// <summary>主键</summary>
		Boolean PrimaryKey { get; set; }

		/// <summary>是否聚集索引</summary>
		Boolean Clustered { get; set; }

		/// <summary>是否计算产生</summary>
		Boolean Computed { get; set; }

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