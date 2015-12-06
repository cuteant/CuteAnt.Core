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
	/// <summary>数据关系</summary>
	//[Serializable]
	[DisplayName("关系模型")]
	[Description("关系模型")]
	[XmlRoot("Relation")]
	internal class OrmLiteRelation : SerializableDataMember, IDataRelation, ICloneable
	{
		#region 属性

		private String _Column;

		/// <summary>数据列</summary>
		[XmlAttribute]
		[DisplayName("数据列")]
		[Description("数据列")]
		public String Column
		{
			get { return _Column; }
			set { _Column = value; }
		}

		private String _RelationTable;

		/// <summary>引用表</summary>
		[XmlAttribute]
		[DisplayName("引用表")]
		[Description("引用表")]
		public String RelationTable
		{
			get { return _RelationTable; }
			set { _RelationTable = value; }
		}

		private String _RelationColumn;

		/// <summary>引用列</summary>
		[XmlAttribute]
		[DisplayName("引用列")]
		[Description("引用列")]
		public String RelationColumn
		{
			get { return _RelationColumn; }
			set { _RelationColumn = value; }
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

		[IgnoreDataMember, XmlIgnore]
		private IDataTable _Table;

		/// <summary>表</summary>
		[IgnoreDataMember, XmlIgnore]
		public IDataTable Table
		{
			get { return _Table; }
			set { _Table = value; }
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
		public IDataRelation Clone(IDataTable table)
		{
			OrmLiteRelation field = base.MemberwiseClone() as OrmLiteRelation;
			field.Table = table;
			return field;
		}

		#endregion

		#region 辅助

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return String.Format("{0}=>{1}.{2} {3}", Column, RelationTable, RelationColumn, Unique ? "U" : "");
		}

		#endregion
	}
}