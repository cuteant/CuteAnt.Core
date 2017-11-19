/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;

namespace CuteAnt
{
	/// <summary>索引器接访问口。
	/// 该接口用于通过名称快速访问对象属性或字段（属性优先）。</summary>
	//[Obsolete("=>IIndex")]
	public interface IIndexAccessor
	{
		/// <summary>获取/设置 指定名称的属性或字段的值</summary>
		/// <param name="name">名称</param>
		/// <returns></returns>
		Object this[string name] { get; set; }
	}
}