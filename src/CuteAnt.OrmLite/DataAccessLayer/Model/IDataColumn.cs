/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>数据列</summary>
	public interface IDataColumn
	{
		#region 属性

		/// <summary>顺序编号</summary>
		Int32 ID { get; set; }

		/// <summary>名称</summary>
		String Name { get; set; }

		/// <summary>列名</summary>
		String ColumnName { get; set; }

		/// <summary>.Net FX 数据类型</summary>
		Type DataType { get; set; }

		/// <summary>原始数据类型。
		/// 当且仅当目标数据库同为该数据库类型时，采用实体属性信息上的RawType作为反向工程的目标字段类型，以期获得开发和生产的最佳兼容。
		/// </summary>
		String RawType { get; set; }

		/// <summary>通用数据库数据类型</summary>
		CommonDbType DbType { get; set; }

		/// <summary>标识</summary>
		Boolean Identity { get; set; }

		/// <summary>主键</summary>
		Boolean PrimaryKey { get; set; }

		/// <summary>是否主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
		Boolean Master { get; set; }

		/// <summary>长度</summary>
		Int32 Length { get; set; }

		///// <summary>字节数</summary>
		//Int32 NumOfByte { get; set; }

		/// <summary>精度</summary>
		Int32 Precision { get; set; }

		/// <summary>位数</summary>
		Int32 Scale { get; set; }

		/// <summary>允许空</summary>
		Boolean Nullable { get; set; }

		/// <summary>是否Unicode</summary>
		Boolean IsUnicode { get; set; }

		/// <summary>默认值</summary>
		String Default { get; set; }

		/// <summary>显示名。如果有Description则使用Description，否则使用Name</summary>
		String DisplayName { get; set; }

		/// <summary>说明</summary>
		String Description { get; set; }

		#endregion

		#region 扩展属性

		/// <summary>说明数据表</summary>
		IDataTable Table { get; }

		/// <summary>扩展属性</summary>
		IDictionary<String, String> Properties { get; }

		///// <summary>数据列修改方式</summary>
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//DataColumnModificationType ModificationType { get; set; }

		#endregion

		#region 方法

		/// <summary>重新计算修正别名。避免与其它字段名或表名相同，避免关键字</summary>
		/// <returns></returns>
		IDataColumn Fix();

		/// <summary>克隆到指定的数据表</summary>
		/// <param name="table"></param>
		IDataColumn Clone(IDataTable table);

		#endregion
	}

	///// <summary>数据列修改方式</summary>
	//public enum DataColumnModificationType
	//{
	//	/// <summary>创建数据列。</summary>
	//	Create,

	//	/// <summary>修改数据列。</summary>
	//	Alter
	//}
}