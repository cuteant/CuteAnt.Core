using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace CuteAnt.OrmLite
{
	/// <summary>分页参数信息</summary>
	public class PageParameter
	{
		#region 核心属性

		//private String _Sort;
		///// <summary>排序字段</summary>
		//public virtual String Sort
		//{
		//	get { return _Sort; }
		//	set
		//	{
		//		_Sort = value;

		//		// 自动识别带有Asc/Desc的排序
		//		if (!_Sort.IsNullOrEmpty())
		//		{
		//			_Sort = _Sort.Trim();
		//			var p = _Sort.LastIndexOf(" ");
		//			if (p > 0)
		//			{
		//				var dir = _Sort.Substring(p + 1);
		//				if (dir.EqualIgnoreCase("asc"))
		//				{
		//					Desc = false;
		//					_Sort = _Sort.Substring(0, p).Trim();
		//				}
		//				else if (dir.EqualIgnoreCase("desc"))
		//				{
		//					Desc = true;
		//					_Sort = _Sort.Substring(0, p).Trim();
		//				}
		//			}
		//		}
		//	}
		//}

		//private Boolean _Desc;
		///// <summary>是否降序</summary>
		//public virtual Boolean Desc { get { return _Desc; } set { _Desc = value; } }

		private AdvancedWhereExpression _WhereExp;
		/// <summary>查询表达式</summary>
		public AdvancedWhereExpression WhereExp { get { return _WhereExp; } set { _WhereExp = value; } }

		private String _OrderBy;
		/// <summary>排序表达式</summary>
		public String OrderBy { get { return _OrderBy; } set { _OrderBy = value; } }

		private String _SelectFields;
		/// <summary>查询列，多个字段使用逗号分隔，默认null表示所有字段。</summary>
		public String SelectFields { get { return _SelectFields; } set { _SelectFields = value; } }

		private Int64 _RowIndex = 0;
		/// <summary>开始行，0表示第一行</summary>
		public Int64 RowIndex { get { return _RowIndex; } set { _RowIndex = value >= 0 ? value : 0; } }

		private Int32 _RowCount = 20;
		/// <summary>返回行数，0表示所有行</summary>
		public Int32 RowCount { get { return _RowCount; } set { _RowCount = value >= 0 ? value : 20; } }

		#endregion

		#region 扩展属性

		private Int64 _TotalCount;
		/// <summary>总记录数</summary>
		[IgnoreDataMember, XmlIgnore]
		public Int64 TotalCount { get { return _TotalCount; } set { _TotalCount = value; } }

		/// <summary>页数</summary>
		[IgnoreDataMember, XmlIgnore]
		public Int32 PageCount
		{
			get
			{
				var count = TotalCount / RowCount;
				if ((TotalCount % RowCount) != 0) { count++; }
				return (Int32)count;
			}
		}

		#endregion

		#region 构造克隆

		/// <summary>实例化分页参数</summary>
		public PageParameter() { }

		/// <summary>通过另一个分页参数来实例化当前分页参数</summary>
		/// <param name="pm"></param>
		public PageParameter(PageParameter pm) { CopyFrom(pm); }

		/// <summary>从另一个分页参数拷贝到当前分页参数</summary>
		/// <param name="pm"></param>
		/// <returns></returns>
		public PageParameter CopyFrom(PageParameter pm)
		{
			//Sort = pm.Sort;
			//Desc = pm.Desc;

			// TODO 查询表达式不能简单引用拷贝
			WhereExp = pm.WhereExp;
			OrderBy = pm.OrderBy;
			SelectFields = pm.SelectFields;
			RowIndex = pm.RowIndex;
			RowCount = pm.RowCount;

			TotalCount = pm.TotalCount;

			return this;
		}

		#endregion
	}
}
