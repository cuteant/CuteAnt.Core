/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.ComponentModel;
using System.Data.Common;
using CuteAnt.OrmLite.Cache;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>实体会话接口</summary>
	public partial interface IEntitySession
	{
		#region -- 属性 --

		/// <summary>连接名</summary>
		String ConnName { get; }

		/// <summary>表名</summary>
		String TableName { get; }

		/// <summary>用于标识会话的键值</summary>
		String Key { get; }

		#endregion

		#region -- 主要属性 --

		/// <summary>实体操作者</summary>
		IEntityOperate Operate { get; }

		/// <summary>数据操作层</summary>
		DAL Dal { get; }

		/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
		IQuoter Quoter { get; }

		/// <summary>已格式化的表名，带有中括号等</summary>
		String FormatedTableName { get; }

		/// <summary>是否启用读写锁机制</summary>
		Boolean ReadWriteLockEnable { get; set; }

		#endregion

		#region -- 缓存 --

		/// <summary>实体缓存是否已被禁用</summary>
		Boolean EntityCacheDisabled { get; }

		/// <summary>单对象缓存是否已被禁用</summary>
		Boolean SingleCacheDisabled { get; }

		/// <summary>实体缓存</summary>
		/// <returns></returns>
		IEntityCache Cache { get; }

		/// <summary>单对象实体缓存。
		/// 建议自定义查询数据方法，并从二级缓存中获取实体数据，以抵消因初次填充而带来的消耗。
		/// </summary>
		ISingleEntityCache SingleCache { get; }

		/// <summary>总记录数，小于等于1000时是精确的，大于1000时缓存10分钟</summary>
		/// <remarks>
		/// 1，检查静态字段，如果有数据且小于等于1000，直接返回，否则=>3
		/// 2，如果有数据但大于1000，则返回缓存里面的有效数据
		/// 3，来到这里，有可能是第一次访问，静态字段没有缓存，也有可能是大于1000的缓存过期
		/// 4，检查模型
		/// 5，根据需要查询数据
		/// 6，如果大于1000，缓存数据
		/// 7，检查数据初始化
		/// </remarks>
		Int64 Count { get; }

		/// <summary>清除缓存</summary>
		/// <param name="reason">原因</param>
		void ClearCache(String reason = null);

		/// <summary>在数据修改时保持缓存，直到数据过期，独占数据库时默认打开，否则默认关闭</summary>
		/// <remarks>实体缓存和单对象缓存能够自动维护更新数据，保持缓存数据最新，在普通CURD中足够使用</remarks>
		Boolean HoldCache { get; set; }

		#endregion

		#region -- 事务保护 --

		/// <summary>触发脏实体会话提交事务后的缓存更新操作</summary>
		/// <param name="updateCount">实体更新操作次数</param>
		/// <param name="directExecuteSQLCount">直接执行SQL语句次数</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		void RaiseCommitDataChange(Int32 updateCount, Int32 directExecuteSQLCount);

		/// <summary>触发脏实体会话回滚事务后的缓存更新操作</summary>
		/// <param name="updateCount">实体更新操作次数</param>
		/// <param name="directExecuteSQLCount">直接执行SQL语句次数</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		void RaiseRoolbackDataChange(Int32 updateCount, Int32 directExecuteSQLCount);

		#endregion

		#region -- 数据库操作 --

		/// <summary>根据条件把普通查询SQL格式化为分页SQL。</summary>
		/// <remarks>
		/// 因为需要继承重写的原因，在数据类中并不方便缓存分页SQL。
		/// 所以在这里做缓存。
		/// </remarks>
		/// <param name="builder">查询生成器</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>分页SQL</returns>
		SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>数据改变后触发。参数指定触发该事件的实体类</summary>
		event Action<Type> OnDataChange;

		#endregion

		#region -- 参数化 --

		/// <summary>创建参数</summary>
		/// <returns></returns>
		DbParameter CreateParameter();

		/// <summary>格式化参数名</summary>
		/// <param name="name">名称</param>
		/// <returns></returns>
		String FormatParameterName(String name);

		#endregion
	}
}