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
	/// <summary>实体模型视图字段</summary>
	//[Serializable]
	[DataObject]
	[Description("实体模型视图字段")]
	[BindIndex("PK__ModelVie__3214EC27173876EA", true, "ID")]
	[BindIndex("IX_ModelViewColumn_ModelViewID", false, "ModelViewID")]
	[BindRelation("ModelViewID", false, "ModelView", "ID")]
	[BindTable("ModelViewColumn", Description = "实体模型视图字段", ConnName = "EmeSprite")]
	public partial class ModelViewColumn : IModelViewColumn
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

		private String _CustomColumnName;

		/// <summary>自定义列标题</summary>
		[DisplayName("自定义列标题")]
		[Description("自定义列标题")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(4, "CustomColumnName", "自定义列标题", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomColumnName
		{
			get { return _CustomColumnName; }
			set { if (OnPropertyChanging(__.CustomColumnName, value)) { _CustomColumnName = value; OnPropertyChanged(__.CustomColumnName); } }
		}

		private String _BindModel;

		/// <summary>绑定数据模型</summary>
		[DisplayName("绑定数据模型")]
		[Description("绑定数据模型")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(5, "BindModel", "绑定数据模型", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(6, "BindTable", "绑定实体模型", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String BindTable
		{
			get { return _BindTable; }
			set { if (OnPropertyChanging(__.BindTable, value)) { _BindTable = value; OnPropertyChanged(__.BindTable); } }
		}

		private String _RelatedField;

		/// <summary>关联字段</summary>
		[DisplayName("关联字段")]
		[Description("关联字段")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(7, "RelatedField", "关联字段", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String RelatedField
		{
			get { return _RelatedField; }
			set { if (OnPropertyChanging(__.RelatedField, value)) { _RelatedField = value; OnPropertyChanged(__.RelatedField); } }
		}

		private Boolean _AllowSelectTemplate;

		/// <summary>允许选择模板使用</summary>
		[DisplayName("允许选择模板使用")]
		[Description("允许选择模板使用")]
		[DataObjectField(false, false, false)]
		[BindColumn(8, "AllowSelectTemplate", "允许选择模板使用", null, "bit", CommonDbType.Boolean, false)]
		public virtual Boolean AllowSelectTemplate
		{
			get { return _AllowSelectTemplate; }
			set { if (OnPropertyChanging(__.AllowSelectTemplate, value)) { _AllowSelectTemplate = value; OnPropertyChanged(__.AllowSelectTemplate); } }
		}

		private Int32 _DisplayType;

		/// <summary>显示方式</summary>
		[DisplayName("显示方式")]
		[Description("显示方式")]
		[DataObjectField(false, false, false)]
		[BindColumn(9, "DisplayType", "显示方式", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 DisplayType
		{
			get { return _DisplayType; }
			set { if (OnPropertyChanging(__.DisplayType, value)) { _DisplayType = value; OnPropertyChanged(__.DisplayType); } }
		}

		private String _CustomFormat;

		/// <summary>自定义格式</summary>
		[DisplayName("自定义格式")]
		[Description("自定义格式")]
		[DataObjectField(false, false, true, 100)]
		[BindColumn(10, "CustomFormat", "自定义格式", null, "nvarchar(100)", CommonDbType.String, true)]
		public virtual String CustomFormat
		{
			get { return _CustomFormat; }
			set { if (OnPropertyChanging(__.CustomFormat, value)) { _CustomFormat = value; OnPropertyChanged(__.CustomFormat); } }
		}

		private Int32 _HeaderAlignment;

		/// <summary>列标题对齐方式</summary>
		[DisplayName("列标题对齐方式")]
		[Description("列标题对齐方式")]
		[DataObjectField(false, false, true)]
		[BindColumn(11, "HeaderAlignment", "列标题对齐方式", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 HeaderAlignment
		{
			get { return _HeaderAlignment; }
			set { if (OnPropertyChanging(__.HeaderAlignment, value)) { _HeaderAlignment = value; OnPropertyChanged(__.HeaderAlignment); } }
		}

		private Int32 _CellAlignment;

		/// <summary>单元格对齐方式</summary>
		[DisplayName("单元格对齐方式")]
		[Description("单元格对齐方式")]
		[DataObjectField(false, false, true)]
		[BindColumn(12, "CellAlignment", "单元格对齐方式", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 CellAlignment
		{
			get { return _CellAlignment; }
			set { if (OnPropertyChanging(__.CellAlignment, value)) { _CellAlignment = value; OnPropertyChanged(__.CellAlignment); } }
		}

		private Int32 _BackColor;

		/// <summary>背景色</summary>
		[DisplayName("背景色")]
		[Description("背景色")]
		[DataObjectField(false, false, true)]
		[BindColumn(13, "BackColor", "背景色", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 BackColor
		{
			get { return _BackColor; }
			set { if (OnPropertyChanging(__.BackColor, value)) { _BackColor = value; OnPropertyChanged(__.BackColor); } }
		}

		private Int32 _ForeColor;

		/// <summary>显示颜色</summary>
		[DisplayName("显示颜色")]
		[Description("显示颜色")]
		[DataObjectField(false, false, true)]
		[BindColumn(14, "ForeColor", "显示颜色", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 ForeColor
		{
			get { return _ForeColor; }
			set { if (OnPropertyChanging(__.ForeColor, value)) { _ForeColor = value; OnPropertyChanged(__.ForeColor); } }
		}

		private Int32 _CustomColor1;

		/// <summary>自定义颜色1</summary>
		[DisplayName("自定义颜色1")]
		[Description("自定义颜色1")]
		[DataObjectField(false, false, true)]
		[BindColumn(15, "CustomColor1", "自定义颜色1", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 CustomColor1
		{
			get { return _CustomColor1; }
			set { if (OnPropertyChanging(__.CustomColor1, value)) { _CustomColor1 = value; OnPropertyChanged(__.CustomColor1); } }
		}

		private Int32 _CustomColor2;

		/// <summary>自定义颜色2</summary>
		[DisplayName("自定义颜色2")]
		[Description("自定义颜色2")]
		[DataObjectField(false, false, true)]
		[BindColumn(16, "CustomColor2", "自定义颜色2", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 CustomColor2
		{
			get { return _CustomColor2; }
			set { if (OnPropertyChanging(__.CustomColor2, value)) { _CustomColor2 = value; OnPropertyChanged(__.CustomColor2); } }
		}

		private Int32 _CustomColor3;

		/// <summary>自定义颜色3</summary>
		[DisplayName("自定义颜色3")]
		[Description("自定义颜色3")]
		[DataObjectField(false, false, true)]
		[BindColumn(17, "CustomColor3", "自定义颜色3", "0", "int", CommonDbType.Integer, false)]
		public virtual Int32 CustomColor3
		{
			get { return _CustomColor3; }
			set { if (OnPropertyChanging(__.CustomColor3, value)) { _CustomColor3 = value; OnPropertyChanged(__.CustomColor3); } }
		}

		private String _CustomValue0;

		/// <summary>自定义值0</summary>
		[DisplayName("自定义值0")]
		[Description("自定义值0")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(18, "CustomValue0", "自定义值0", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue0
		{
			get { return _CustomValue0; }
			set { if (OnPropertyChanging(__.CustomValue0, value)) { _CustomValue0 = value; OnPropertyChanged(__.CustomValue0); } }
		}

		private String _CustomValue1;

		/// <summary>自定义值1</summary>
		[DisplayName("自定义值1")]
		[Description("自定义值1")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(19, "CustomValue1", "自定义值1", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue1
		{
			get { return _CustomValue1; }
			set { if (OnPropertyChanging(__.CustomValue1, value)) { _CustomValue1 = value; OnPropertyChanged(__.CustomValue1); } }
		}

		private String _CustomValue2;

		/// <summary>自定义值2</summary>
		[DisplayName("自定义值2")]
		[Description("自定义值2")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(20, "CustomValue2", "自定义值2", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue2
		{
			get { return _CustomValue2; }
			set { if (OnPropertyChanging(__.CustomValue2, value)) { _CustomValue2 = value; OnPropertyChanged(__.CustomValue2); } }
		}

		private String _CustomValue3;

		/// <summary>自定义值3</summary>
		[DisplayName("自定义值3")]
		[Description("自定义值3")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(21, "CustomValue3", "自定义值3", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue3
		{
			get { return _CustomValue3; }
			set { if (OnPropertyChanging(__.CustomValue3, value)) { _CustomValue3 = value; OnPropertyChanged(__.CustomValue3); } }
		}

		private String _CustomValue4;

		/// <summary>自定义值4</summary>
		[DisplayName("自定义值4")]
		[Description("自定义值4")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(22, "CustomValue4", "自定义值4", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue4
		{
			get { return _CustomValue4; }
			set { if (OnPropertyChanging(__.CustomValue4, value)) { _CustomValue4 = value; OnPropertyChanged(__.CustomValue4); } }
		}

		private String _CustomValue5;

		/// <summary>自定义值5</summary>
		[DisplayName("自定义值5")]
		[Description("自定义值5")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(23, "CustomValue5", "自定义值5", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue5
		{
			get { return _CustomValue5; }
			set { if (OnPropertyChanging(__.CustomValue5, value)) { _CustomValue5 = value; OnPropertyChanged(__.CustomValue5); } }
		}

		private String _CustomValue6;

		/// <summary>自定义值6</summary>
		[DisplayName("自定义值6")]
		[Description("自定义值6")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(24, "CustomValue6", "自定义值6", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue6
		{
			get { return _CustomValue6; }
			set { if (OnPropertyChanging(__.CustomValue6, value)) { _CustomValue6 = value; OnPropertyChanged(__.CustomValue6); } }
		}

		private String _CustomValue7;

		/// <summary>自定义值7</summary>
		[DisplayName("自定义值7")]
		[Description("自定义值7")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(25, "CustomValue7", "自定义值7", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue7
		{
			get { return _CustomValue7; }
			set { if (OnPropertyChanging(__.CustomValue7, value)) { _CustomValue7 = value; OnPropertyChanged(__.CustomValue7); } }
		}

		private String _CustomValue8;

		/// <summary>自定义值8</summary>
		[DisplayName("自定义值8")]
		[Description("自定义值8")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(26, "CustomValue8", "自定义值8", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue8
		{
			get { return _CustomValue8; }
			set { if (OnPropertyChanging(__.CustomValue8, value)) { _CustomValue8 = value; OnPropertyChanged(__.CustomValue8); } }
		}

		private String _CustomValue9;

		/// <summary>自定义值9</summary>
		[DisplayName("自定义值9")]
		[Description("自定义值9")]
		[DataObjectField(false, false, true, 50)]
		[BindColumn(27, "CustomValue9", "自定义值9", null, "nvarchar(50)", CommonDbType.String, true)]
		public virtual String CustomValue9
		{
			get { return _CustomValue9; }
			set { if (OnPropertyChanging(__.CustomValue9, value)) { _CustomValue9 = value; OnPropertyChanged(__.CustomValue9); } }
		}

		private Int32 _Sort;

		/// <summary>排序</summary>
		[DisplayName("排序")]
		[Description("排序")]
		[DataObjectField(false, false, false)]
		[BindColumn(28, "Sort", "排序", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(29, "ModifiedTime", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(30, "ModifiedByUserID", "修改用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(31, "ModifiedByUser", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
		[BindColumn(32, "CreatedTime", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
		[BindColumn(33, "CreatedByUserID", "创建用户", "0", "int", CommonDbType.Integer, false)]
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
		[BindColumn(34, "CreatedByUser", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
					case __.CustomColumnName: return _CustomColumnName;
					case __.BindModel: return _BindModel;
					case __.BindTable: return _BindTable;
					case __.RelatedField: return _RelatedField;
					case __.AllowSelectTemplate: return _AllowSelectTemplate;
					case __.DisplayType: return _DisplayType;
					case __.CustomFormat: return _CustomFormat;
					case __.HeaderAlignment: return _HeaderAlignment;
					case __.CellAlignment: return _CellAlignment;
					case __.BackColor: return _BackColor;
					case __.ForeColor: return _ForeColor;
					case __.CustomColor1: return _CustomColor1;
					case __.CustomColor2: return _CustomColor2;
					case __.CustomColor3: return _CustomColor3;
					case __.CustomValue0: return _CustomValue0;
					case __.CustomValue1: return _CustomValue1;
					case __.CustomValue2: return _CustomValue2;
					case __.CustomValue3: return _CustomValue3;
					case __.CustomValue4: return _CustomValue4;
					case __.CustomValue5: return _CustomValue5;
					case __.CustomValue6: return _CustomValue6;
					case __.CustomValue7: return _CustomValue7;
					case __.CustomValue8: return _CustomValue8;
					case __.CustomValue9: return _CustomValue9;
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
					case __.CustomColumnName: _CustomColumnName = Convert.ToString(value); break;
					case __.BindModel: _BindModel = Convert.ToString(value); break;
					case __.BindTable: _BindTable = Convert.ToString(value); break;
					case __.RelatedField: _RelatedField = Convert.ToString(value); break;
					case __.AllowSelectTemplate: _AllowSelectTemplate = Convert.ToBoolean(value); break;
					case __.DisplayType: _DisplayType = Convert.ToInt32(value); break;
					case __.CustomFormat: _CustomFormat = Convert.ToString(value); break;
					case __.HeaderAlignment: _HeaderAlignment = Convert.ToInt32(value); break;
					case __.CellAlignment: _CellAlignment = Convert.ToInt32(value); break;
					case __.BackColor: _BackColor = Convert.ToInt32(value); break;
					case __.ForeColor: _ForeColor = Convert.ToInt32(value); break;
					case __.CustomColor1: _CustomColor1 = Convert.ToInt32(value); break;
					case __.CustomColor2: _CustomColor2 = Convert.ToInt32(value); break;
					case __.CustomColor3: _CustomColor3 = Convert.ToInt32(value); break;
					case __.CustomValue0: _CustomValue0 = Convert.ToString(value); break;
					case __.CustomValue1: _CustomValue1 = Convert.ToString(value); break;
					case __.CustomValue2: _CustomValue2 = Convert.ToString(value); break;
					case __.CustomValue3: _CustomValue3 = Convert.ToString(value); break;
					case __.CustomValue4: _CustomValue4 = Convert.ToString(value); break;
					case __.CustomValue5: _CustomValue5 = Convert.ToString(value); break;
					case __.CustomValue6: _CustomValue6 = Convert.ToString(value); break;
					case __.CustomValue7: _CustomValue7 = Convert.ToString(value); break;
					case __.CustomValue8: _CustomValue8 = Convert.ToString(value); break;
					case __.CustomValue9: _CustomValue9 = Convert.ToString(value); break;
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

		/// <summary>取得实体模型视图字段字段信息的快捷方式</summary>
		public partial class _
		{
			///<summary>主键</summary>
			public static readonly FieldItem ID = FindByName(__.ID);

			///<summary>视图</summary>
			public static readonly FieldItem ModelViewID = FindByName(__.ModelViewID);

			///<summary>字段名称</summary>
			public static readonly FieldItem ColumnName = FindByName(__.ColumnName);

			///<summary>自定义列标题</summary>
			public static readonly FieldItem CustomColumnName = FindByName(__.CustomColumnName);

			///<summary>绑定数据模型</summary>
			public static readonly FieldItem BindModel = FindByName(__.BindModel);

			///<summary>绑定实体模型</summary>
			public static readonly FieldItem BindTable = FindByName(__.BindTable);

			///<summary>关联字段</summary>
			public static readonly FieldItem RelatedField = FindByName(__.RelatedField);

			///<summary>允许选择模板使用</summary>
			public static readonly FieldItem AllowSelectTemplate = FindByName(__.AllowSelectTemplate);

			///<summary>显示方式</summary>
			public static readonly FieldItem DisplayType = FindByName(__.DisplayType);

			///<summary>自定义格式</summary>
			public static readonly FieldItem CustomFormat = FindByName(__.CustomFormat);

			///<summary>列标题对齐方式</summary>
			public static readonly FieldItem HeaderAlignment = FindByName(__.HeaderAlignment);

			///<summary>单元格对齐方式</summary>
			public static readonly FieldItem CellAlignment = FindByName(__.CellAlignment);

			///<summary>背景色</summary>
			public static readonly FieldItem BackColor = FindByName(__.BackColor);

			///<summary>显示颜色</summary>
			public static readonly FieldItem ForeColor = FindByName(__.ForeColor);

			///<summary>自定义颜色1</summary>
			public static readonly FieldItem CustomColor1 = FindByName(__.CustomColor1);

			///<summary>自定义颜色2</summary>
			public static readonly FieldItem CustomColor2 = FindByName(__.CustomColor2);

			///<summary>自定义颜色3</summary>
			public static readonly FieldItem CustomColor3 = FindByName(__.CustomColor3);

			///<summary>自定义值0</summary>
			public static readonly FieldItem CustomValue0 = FindByName(__.CustomValue0);

			///<summary>自定义值1</summary>
			public static readonly FieldItem CustomValue1 = FindByName(__.CustomValue1);

			///<summary>自定义值2</summary>
			public static readonly FieldItem CustomValue2 = FindByName(__.CustomValue2);

			///<summary>自定义值3</summary>
			public static readonly FieldItem CustomValue3 = FindByName(__.CustomValue3);

			///<summary>自定义值4</summary>
			public static readonly FieldItem CustomValue4 = FindByName(__.CustomValue4);

			///<summary>自定义值5</summary>
			public static readonly FieldItem CustomValue5 = FindByName(__.CustomValue5);

			///<summary>自定义值6</summary>
			public static readonly FieldItem CustomValue6 = FindByName(__.CustomValue6);

			///<summary>自定义值7</summary>
			public static readonly FieldItem CustomValue7 = FindByName(__.CustomValue7);

			///<summary>自定义值8</summary>
			public static readonly FieldItem CustomValue8 = FindByName(__.CustomValue8);

			///<summary>自定义值9</summary>
			public static readonly FieldItem CustomValue9 = FindByName(__.CustomValue9);

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

		/// <summary>取得实体模型视图字段字段名称的快捷方式</summary>
		public partial class __
		{
			///<summary>主键</summary>
			public const String ID = "ID";

			///<summary>视图</summary>
			public const String ModelViewID = "ModelViewID";

			///<summary>字段名称</summary>
			public const String ColumnName = "ColumnName";

			///<summary>自定义列标题</summary>
			public const String CustomColumnName = "CustomColumnName";

			///<summary>绑定数据模型</summary>
			public const String BindModel = "BindModel";

			///<summary>绑定实体模型</summary>
			public const String BindTable = "BindTable";

			///<summary>关联字段</summary>
			public const String RelatedField = "RelatedField";

			///<summary>允许选择模板使用</summary>
			public const String AllowSelectTemplate = "AllowSelectTemplate";

			///<summary>显示方式</summary>
			public const String DisplayType = "DisplayType";

			///<summary>自定义格式</summary>
			public const String CustomFormat = "CustomFormat";

			///<summary>列标题对齐方式</summary>
			public const String HeaderAlignment = "HeaderAlignment";

			///<summary>单元格对齐方式</summary>
			public const String CellAlignment = "CellAlignment";

			///<summary>背景色</summary>
			public const String BackColor = "BackColor";

			///<summary>显示颜色</summary>
			public const String ForeColor = "ForeColor";

			///<summary>自定义颜色1</summary>
			public const String CustomColor1 = "CustomColor1";

			///<summary>自定义颜色2</summary>
			public const String CustomColor2 = "CustomColor2";

			///<summary>自定义颜色3</summary>
			public const String CustomColor3 = "CustomColor3";

			///<summary>自定义值0</summary>
			public const String CustomValue0 = "CustomValue0";

			///<summary>自定义值1</summary>
			public const String CustomValue1 = "CustomValue1";

			///<summary>自定义值2</summary>
			public const String CustomValue2 = "CustomValue2";

			///<summary>自定义值3</summary>
			public const String CustomValue3 = "CustomValue3";

			///<summary>自定义值4</summary>
			public const String CustomValue4 = "CustomValue4";

			///<summary>自定义值5</summary>
			public const String CustomValue5 = "CustomValue5";

			///<summary>自定义值6</summary>
			public const String CustomValue6 = "CustomValue6";

			///<summary>自定义值7</summary>
			public const String CustomValue7 = "CustomValue7";

			///<summary>自定义值8</summary>
			public const String CustomValue8 = "CustomValue8";

			///<summary>自定义值9</summary>
			public const String CustomValue9 = "CustomValue9";

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

	/// <summary>实体模型视图字段接口</summary>
	public partial interface IModelViewColumn
	{
		#region 属性

		/// <summary>主键</summary>
		Int32 ID { get; set; }

		/// <summary>视图</summary>
		Int32 ModelViewID { get; set; }

		/// <summary>字段名称</summary>
		String ColumnName { get; set; }

		/// <summary>自定义列标题</summary>
		String CustomColumnName { get; set; }

		/// <summary>绑定数据模型</summary>
		String BindModel { get; set; }

		/// <summary>绑定实体模型</summary>
		String BindTable { get; set; }

		/// <summary>关联字段</summary>
		String RelatedField { get; set; }

		/// <summary>允许选择模板使用</summary>
		Boolean AllowSelectTemplate { get; set; }

		/// <summary>显示方式</summary>
		Int32 DisplayType { get; set; }

		/// <summary>自定义格式</summary>
		String CustomFormat { get; set; }

		/// <summary>列标题对齐方式</summary>
		Int32 HeaderAlignment { get; set; }

		/// <summary>单元格对齐方式</summary>
		Int32 CellAlignment { get; set; }

		/// <summary>背景色</summary>
		Int32 BackColor { get; set; }

		/// <summary>显示颜色</summary>
		Int32 ForeColor { get; set; }

		/// <summary>自定义颜色1</summary>
		Int32 CustomColor1 { get; set; }

		/// <summary>自定义颜色2</summary>
		Int32 CustomColor2 { get; set; }

		/// <summary>自定义颜色3</summary>
		Int32 CustomColor3 { get; set; }

		/// <summary>自定义值0</summary>
		String CustomValue0 { get; set; }

		/// <summary>自定义值1</summary>
		String CustomValue1 { get; set; }

		/// <summary>自定义值2</summary>
		String CustomValue2 { get; set; }

		/// <summary>自定义值3</summary>
		String CustomValue3 { get; set; }

		/// <summary>自定义值4</summary>
		String CustomValue4 { get; set; }

		/// <summary>自定义值5</summary>
		String CustomValue5 { get; set; }

		/// <summary>自定义值6</summary>
		String CustomValue6 { get; set; }

		/// <summary>自定义值7</summary>
		String CustomValue7 { get; set; }

		/// <summary>自定义值8</summary>
		String CustomValue8 { get; set; }

		/// <summary>自定义值9</summary>
		String CustomValue9 { get; set; }

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