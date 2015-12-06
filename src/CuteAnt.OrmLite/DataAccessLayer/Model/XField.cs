﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using CuteAnt.Collections;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>字段构架</summary>
	//[Serializable]
	[DisplayName("字段模型")]
	[Description("字段模型")]
	[XmlRoot("Column")]
	internal class OrmLiteColumn : SerializableDataMember, IDataColumn, ICloneable
	{
		#region 属性

		private Int32 _ID;

		/// <summary>顺序编号</summary>
		[XmlAttribute]
		[DisplayName("编号")]
		[Description("编号")]
		public Int32 ID { get { return _ID; } set { _ID = value; } }

		private String _Name;

		/// <summary>名称</summary>
		[XmlAttribute]
		[DisplayName("名称")]
		[Description("名称")]
		public String Name
		{
			get
			{
				if (!_Name.IsNullOrWhiteSpace()) return _Name;

				//!! 先赋值，非常重要。后面GetAlias时会用到其它列的别名，然后可能形成死循环。先赋值之后，下一次来到这里时将直接返回。
				_Name = ColumnName;
				_Name = ModelResolver.Current.GetName(this);

				return _Name;
			}
			set { _Name = value; }
		}

		private String _ColumnName;

		/// <summary>列名</summary>
		[XmlAttribute]
		[DisplayName("列名")]
		[Description("列名")]
		public String ColumnName { get { return _ColumnName; } set { _ColumnName = value; } }

		private Type _DataType;

		/// <summary>数据类型</summary>
		[XmlAttribute]
		[DisplayName("数据类型")]
		[Description("数据类型")]
		public Type DataType { get { return _DataType; } set { _DataType = value; } }

		/// <summary>字段类型</summary>
		[IgnoreDataMember, XmlIgnore]
		[DisplayName("字段类型")]
		[Description("字段类型")]
		public String FieldType { get { return DataType == null ? null : DataType.Name; } set { _DataType = value.GetTypeEx(); } }

		private String _RawType;

		/// <summary>原始数据类型</summary>
		[XmlAttribute]
		[DisplayName("原始类型")]
		[Description("原始类型")]
		public String RawType { get { return _RawType; } set { _RawType = value; } }

		private CommonDbType _DbType;

		/// <summary>通用数据库数据类型</summary>
		[XmlAttribute]
		[DisplayName("通用数据库数据类型")]
		[Description("通用数据库数据类型")]
		public CommonDbType DbType { get { return _DbType; } set { _DbType = value; } }

		private Boolean _Identity;

		/// <summary>标识</summary>
		[XmlAttribute]
		[DisplayName("标识")]
		[Description("标识")]
		public Boolean Identity { get { return _Identity; } set { _Identity = value; } }

		private Boolean _PrimaryKey;

		/// <summary>主键</summary>
		[XmlAttribute]
		[DisplayName("主键")]
		[Description("主键")]
		public Boolean PrimaryKey { get { return _PrimaryKey; } set { _PrimaryKey = value; } }

		private Boolean _Master;
		/// <summary>是否主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
		[XmlAttribute]
		[DisplayName("主字段")]
		[Description("主字段")]
		public Boolean Master { get { return _Master; } set { _Master = value; } }

		private Int32 _Length;

		/// <summary>长度</summary>
		[XmlAttribute]
		[DisplayName("长度")]
		[Description("长度")]
		public Int32 Length { get { return _Length; } set { _Length = value; } }

		//private Int32 _NumOfByte;

		///// <summary>字节数</summary>
		//[XmlAttribute]
		//[DisplayName("字节数")]
		//[Description("字节数")]
		//public Int32 NumOfByte { get { return _NumOfByte; } set { _NumOfByte = value; } }

		private Int32 _Precision;

		/// <summary>精度</summary>
		[XmlAttribute]
		[DisplayName("精度")]
		[Description("精度")]
		public Int32 Precision { get { return _Precision; } set { _Precision = value; } }

		private Int32 _Scale;

		/// <summary>位数</summary>
		[XmlAttribute]
		[DisplayName("位数")]
		[Description("位数")]
		public Int32 Scale { get { return _Scale; } set { _Scale = value; } }

		private Boolean _Nullable;

		/// <summary>允许空</summary>
		[XmlAttribute]
		[DisplayName("允许空")]
		[Description("允许空")]
		public Boolean Nullable { get { return _Nullable; } set { _Nullable = value; } }

		private Boolean _IsUnicode;

		/// <summary>是否Unicode</summary>
		[XmlAttribute]
		[DisplayName("Unicode")]
		[Description("Unicode")]
		public Boolean IsUnicode { get { return _IsUnicode; } set { _IsUnicode = value; } }

		private String _Default;

		/// <summary>默认值</summary>
		[XmlAttribute]
		[DisplayName("默认值")]
		[Description("默认值")]
		public String Default { get { return _Default; } set { _Default = value; } }

		private String _DisplayName;

		/// <summary>显示名</summary>
		[XmlAttribute]
		[DisplayName("显示名")]
		[Description("显示名")]
		public String DisplayName
		{
			get
			{
				if (_DisplayName.IsNullOrWhiteSpace())
				{
					_DisplayName = ModelResolver.Current.GetDisplayName(_Name, _Description);
				}
				return _DisplayName;
			}
			set
			{
				if (!value.IsNullOrWhiteSpace())
				{
					value = value.Replace("\r\n", "。").Replace("\r", " ").Replace("\n", " ");
				}
				_DisplayName = value;

				if (_Description.IsNullOrWhiteSpace())
				{
					_Description = _DisplayName;
				}
				else if (!_Description.StartsWith(_DisplayName))
				{
					_Description = _DisplayName + "。" + _Description;
				}
			}
		}

		private String _Description;

		/// <summary>描述</summary>
		[XmlAttribute]
		[DisplayName("描述")]
		[Description("描述")]
		public String Description
		{
			get { return _Description; }
			set
			{
				if (!value.IsNullOrWhiteSpace())
				{
					value = value.Replace("\r\n", "。").Replace("\r", " ").Replace("\n", " ");
				}
				_Description = value;
			}
		}

		#endregion

		#region 扩展属性

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private IDataTable _Table;

		/// <summary>表</summary>
		[IgnoreDataMember, XmlIgnore]
		public IDataTable Table
		{
			get { return _Table; }
			set { _Table = value; }
		}

		///// <summary>显示名。如果有Description则使用Description，否则使用Name</summary>
		//[NonSerialized, IgnoreDataMember, XmlIgnore]
		//public String DisplayName
		//{
		//	get { return ModelResolver.Current.GetDisplayName(Name ?? ColumnName, Description); }
		//}

		private IDictionary<String, String> _Properties;

		/// <summary>扩展属性</summary>
		[Category("扩展")]
		[DisplayName("扩展属性")]
		[Description("扩展属性")]
		public IDictionary<String, String> Properties
		{
			get { return _Properties ?? (_Properties = new NullableDictionary<String, String>(StringComparer.OrdinalIgnoreCase)); }
		}

		//[NonSerialized, IgnoreDataMember, XmlIgnore]
		//private DataColumnModificationType _ModificationType = DataColumnModificationType.Create;

		///// <summary>数据列修改方式</summary>
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//[NonSerialized, IgnoreDataMember, XmlIgnore]
		//public DataColumnModificationType ModificationType { get { return _ModificationType; } set { _ModificationType = value; } }

		#endregion

		#region 构造

		//private HmField() { }

		//private HmField(IDataTable table) { Table = table; }

		///// <summary>为制定表创建字段</summary>
		///// <param name="table"></param>
		///// <returns></returns>
		//internal static HmField Create(IDataTable table)
		//{
		//    if (table == null) throw new ArgumentNullException("table");

		//    return new HmField(table);
		//}

		#endregion

		#region 方法

		/// <summary>重新计算修正别名。避免与其它字段名或表名相同，避免关键字</summary>
		/// <returns></returns>
		public IDataColumn Fix()
		{
			//_Alias = ModelResolver.Current.GetAlias(this);
			return ModelResolver.Current.Fix(this);
		}

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			if (!DisplayName.IsNullOrWhiteSpace() && DisplayName != Name)
			{
				return String.Format("ID={0} Name={1} FieldType={2} RawType={3} DisplayName={4}", ID, ColumnName, FieldType, RawType, DisplayName);
			}
			else
			{
				return String.Format("ID={0} Name={1} FieldType={2} RawType={3}", ID, ColumnName, FieldType, RawType);
			}
		}

		#endregion

		#region ICloneable 成员

		/// <summary>克隆</summary>
		/// <returns></returns>
		object ICloneable.Clone()
		{
			return Clone(Table);
		}

		/// <summary>克隆</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public IDataColumn Clone(IDataTable table)
		{
			var field = base.MemberwiseClone() as OrmLiteColumn;
			field.Table = table;
			field.Fix();
			return field;
		}

		#endregion
	}
}