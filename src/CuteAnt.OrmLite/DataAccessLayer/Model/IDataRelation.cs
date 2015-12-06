﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>
	/// 数据关系。
	/// 一个表如果有多个数据关系，表明是多对多的关系表；如果只有一个关系，需要看是否唯一，它决定是一对一还是一对多。并可根据关系，生成对应的数据索引。
	/// 可根据数据关系生成扩展属性。
	/// 正向工程将会为所有数据关系建立相对应的索引。
	/// </summary>
	public interface IDataRelation
	{
		#region 属性

		/// <summary>数据列</summary>
		String Column { get; set; }

		/// <summary>引用表</summary>
		String RelationTable { get; set; }

		/// <summary>引用列</summary>
		String RelationColumn { get; set; }

		/// <summary>是否唯一</summary>
		Boolean Unique { get; set; }

		/// <summary>是否计算出来的，而不是数据库内置的</summary>
		Boolean Computed { get; set; }

		#endregion

		#region 扩展属性

		/// <summary>说明数据表</summary>
		IDataTable Table { get; }

		#endregion

		#region 方法

		/// <summary>克隆到指定的数据表</summary>
		/// <param name="table"></param>
		IDataRelation Clone(IDataTable table);

		#endregion
	}
}