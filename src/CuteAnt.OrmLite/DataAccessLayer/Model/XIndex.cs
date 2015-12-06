/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>索引</summary>
	//[Serializable]
	[DisplayName("索引模型")]
	[Description("索引模型")]
	[XmlRoot("Index")]
	internal class OrmLiteIndex : SerializableDataMember, IDataIndex, ICloneable
	{
		#region 属性

		private String _Name;

		/// <summary>名称</summary>
		[XmlAttribute]
		[DisplayName("名称")]
		[Description("名称")]
		public String Name
		{
			get
			{
				if (_Name.IsNullOrWhiteSpace()) { _Name = ModelResolver.Current.GetName(this); }
				return _Name;
			}
			set { _Name = value; }
		}

		private String[] _Columns;
		/// <summary>数据列集合</summary>
		[XmlAttribute]
		[DisplayName("数据列集合")]
		[Description("数据列集合")]
		public String[] Columns
		{
			get { return _Columns; }
			set { _Columns = value; }
		}

		private Boolean _Unique;

		/// <summary>是否唯一</summary>
		[XmlAttribute]
		[DisplayName("唯一")]
		[Description("唯一")]
		public Boolean Unique
		{
			get { return _Unique; }
			set { _Unique = value; }
		}

		private Boolean _PrimaryKey;

		/// <summary>是否主键</summary>
		[XmlAttribute]
		[DisplayName("主键")]
		[Description("主键")]
		public Boolean PrimaryKey
		{
			get { return _PrimaryKey; }
			set { _PrimaryKey = value; }
		}

		private Boolean _Clustered;

		/// <summary>是否聚集索引</summary>
		[XmlAttribute]
		[DisplayName("聚集索引")]
		[Description("聚集索引")]
		public Boolean Clustered
		{
			get { return _Clustered; }
			set { _Clustered = value; }
		}

		private Boolean _Computed;

		/// <summary>是否计算出来的，而不是数据库内置的</summary>
		[XmlAttribute]
		[DisplayName("计算")]
		[Description("是否计算出来的，而不是数据库内置的")]
		public Boolean Computed
		{
			get { return _Computed; }
			set { _Computed = value; }
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

		#endregion

		#region 方法名

		private String GetIndexName()
		{
			if (Columns == null || Columns.Length < 1) { return null; }
			String indexName = "IX";
			if (Table != null) indexName += "_" + Table.TableName;

			for (int i = 0; i < Columns.Length; i++)
			{
				indexName += "_" + Columns[i];
			}
			return indexName;
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
		public IDataIndex Clone(IDataTable table)
		{
			var field = base.MemberwiseClone() as OrmLiteIndex;
			field.Table = table;
			return field;
		}

		#endregion

		#region 辅助

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			if (Columns != null && Columns.Length > 0)
				return String.Format("{0}=>{1} {2}", Name, String.Join(",", Columns), Unique ? "U" : "");
			else
				return String.Format("{0} {1}", Name, Unique ? "U" : "");
		}

		#endregion
	}
}