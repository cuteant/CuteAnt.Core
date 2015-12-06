using System;
using System.ComponentModel;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary></summary>
	internal enum SchemaObjectType
	{
		/// <summary>表</summary>
		[Description("表")]
		Table,

		/// <summary>视图</summary>
		[Description("视图")]
		View,

		/// <summary>数据列</summary>
		[Description("数据列")]
		Column,

		/// <summary>约束</summary>
		[Description("约束")]
		Constraint,

		/// <summary>索引</summary>
		[Description("索引")]
		Index,

		/// <summary>触发器</summary>
		[Description("触发器")]
		Trigger,

		/// <summary>存储过程</summary>
		[Description("存储过程")]
		StoredProcedure,

		///// <summary>function</summary>
		//Function,

		/// <summary>序列</summary>
		[Description("序列")]
		Sequence,

		///// <summary>package</summary>
		//Package,
	}
}