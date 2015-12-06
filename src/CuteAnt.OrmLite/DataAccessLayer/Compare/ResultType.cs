using System;
using System.ComponentModel;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal enum ResultType
	{
		/// <summary>修改</summary>
		[Description("修改")]
		Change,

		/// <summary>创建</summary>
		[Description("创建")]
		Add,

		/// <summary>删除</summary>
		[Description("删除")]
		Delete,

		/// <summary>重建</summary>
		[Description("重建")]
		RebulidTable
	}
}