/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Data;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite
{
	// ## 苦竹 屏蔽IEnumerable<IEntityEntry> 2013.09.09 PM 18:53 ##
	/// <summary>数据实体接口</summary>
	public partial interface IEntity : IIndexAccessor//, IEnumerable<IEntityEntry>//, IBinaryAccessor
	{
		#region -- 属性 --

		/// <summary>脏属性。存储哪些属性的数据被修改过了。</summary>
		IDictionary<String, Boolean> Dirtys { get; }

		/// <summary>扩展属性</summary>
		IDictionary<String, Object> Extends { get; }

		#endregion

		#region -- 空主键 --

		/// <summary>主键是否为空</summary>
		Boolean IsNullKey { get; }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		void SetNullKey();

		#endregion

		#region -- 操作 --

		/// <summary>把该对象持久化到数据库</summary>
		/// <returns></returns>
		Int32 Insert();

		/// <summary>把该对象持久化到数据库，不需要验证</summary>
		/// <remarks>## 苦竹 添加 2014.04.01 23:45 ##</remarks>
		/// <returns></returns>
		Int32 InsertWithoutValid();

		/// <summary>更新数据库</summary>
		/// <returns></returns>
		Int32 Update();

		/// <summary>从数据库中删除该对象</summary>
		/// <returns></returns>
		Int32 Delete();

		/// <summary>保存。根据主键检查数据库中是否已存在该对象，再决定调用Insert或Update</summary>
		/// <returns></returns>
		Int32 Save();

		/// <summary>不需要验证的保存，不执行Valid，一般用于快速导入数据</summary>
		/// <returns></returns>
		Int32 SaveWithoutValid();

		#endregion

		#region -- 获取/设置 字段值 --

		///// <summary>获取/设置 字段值。</summary>
		///// <param name="name">字段名</param>
		///// <returns></returns>
		//Object this[String name] { get; set; }

		/// <summary>设置字段值</summary>
		/// <param name="name">字段名</param>
		/// <param name="value">值</param>
		/// <returns>返回是否成功设置了数据</returns>
		Boolean SetItem(String name, Object value);

		/// <summary>克隆实体。创建当前对象的克隆对象，仅拷贝基本字段</summary>
		/// <param name="setDirty">是否设置脏数据</param>
		/// <returns></returns>
		IEntity CloneEntity(Boolean setDirty);

		/// <summary>复制来自指定实体的成员，可以是不同类型的实体，只复制共有的基本字段，影响脏数据</summary>
		/// <param name="entity">来源实体对象</param>
		/// <param name="setDirty">是否设置脏数据</param>
		/// <returns>实际复制成员数</returns>
		Int32 CopyFrom(IEntity entity, Boolean setDirty);

		#endregion

		#region -- 导入导出 --

		/// <summary>导出XML</summary>
		/// <returns></returns>
		//[Obsolete("该成员在后续版本中将不再被支持！")]
		String ToXml();

		// ## 苦竹 添加 2012.12.15 AM 02:59 ##
		/// <summary>导出Json</summary>
		/// <returns></returns>
		String ToJson();

		#endregion

		#region -- 实体相等 --

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		Boolean IsEqualTo(IEntity entity);

		#endregion
	}
}