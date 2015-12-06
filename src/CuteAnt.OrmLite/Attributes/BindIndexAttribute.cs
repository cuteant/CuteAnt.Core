/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>用于指定数据类所绑定到的索引</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class BindIndexAttribute : Attribute
	{
		#region -- 属性 --

		private String _Name;

		/// <summary>名称</summary>
		public String Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		private Boolean _Unique;

		/// <summary>是否唯一</summary>
		public Boolean Unique
		{
			get { return _Unique; }
			set { _Unique = value; }
		}

		private Boolean _Clustered;

		/// <summary>是否聚集索引，只适用于SQL Server</summary>
		public Boolean Clustered
		{
			get { return _Clustered; }
			set { _Clustered = value; }
		}

		private String _Columns;

		/// <summary>数据列集合</summary>
		public String Columns
		{
			get { return _Columns; }
			set { _Columns = value; }
		}

		#endregion

		#region -- 构造 --

		/// <summary>指定一个索引</summary>
		/// <param name="name">名称</param>
		/// <param name="unique"></param>
		/// <param name="columns"></param>
		public BindIndexAttribute(String name, Boolean unique, String columns)
		{
			Name = name;
			Unique = unique;
			Columns = columns;
		}

		#endregion

		#region -- 方法 --

		///// <summary>
		///// 检索应用于类型成员的自定义属性。
		///// </summary>
		///// <param name="element"></param>
		///// <returns></returns>
		//public new static BindIndexAttribute[] GetCustomAttributes(MemberInfo element)
		//{
		//  Attribute[] atts = GetCustomAttributes(element, typeof(BindIndexAttribute), true);
		//  if (atts == null || atts.Length < 1) { return null; }
		//  List<BindIndexAttribute> list = new List<BindIndexAttribute>();

		//  foreach (Attribute item in atts)
		//  {
		//    list.Add(item as BindIndexAttribute);
		//  }
		//  return list.ToArray();
		//}

		/// <summary>填充索引</summary>
		/// <param name="index"></param>
		internal void Fill(IDataIndex index)
		{
			if (!Name.IsNullOrWhiteSpace()) { index.Name = Name; }
			index.Unique = Unique;
			index.Clustered = Clustered;

			if (!Columns.IsNullOrWhiteSpace())
			{
				String[] ss = Columns.Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
				List<String> list = new List<String>();

				foreach (String item in ss)
				{
					String column = item.Trim();
					if (!column.IsNullOrWhiteSpace()) { list.Add(column); }
				}
				index.Columns = list.ToArray();
			}
		}

		#endregion
	}
}