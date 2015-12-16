using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace CuteAnt.OrmLite
{
	#region -- 模板类型 --

	/// <summary>模板类型</summary>
	[DataContract]
	public enum EntityTemplateTypes : int
	{
		/// <summary>新建模板</summary>
		[Description("新建模板")]
		EntityTemplateAdd = 1,

		/// <summary>快速新建模板</summary>
		[Description("快速新建模板")]
		EntityTemplateFastAdd = 2,

		/// <summary>编辑模板</summary>
		[Description("编辑模板")]
		EntityTemplateEdit = 3,

		/// <summary>查看模板</summary>
		[Description("查看模板")]
		EntityTemplateView = 4,

		/// <summary>管理模板</summary>
		[Description("管理模板")]
		EntityTemplateAdmin = 5,

		/// <summary>选择模板</summary>
		[Description("选择模板")]
		EntityTemplateSelect = 6
	}

	#endregion

	#region -- 简单数据类型 --

	/// <summary>简单数据类型</summary>
	[DataContract]
	public enum SimpleDataType : int
	{
		/// <summary>Unicode可变长度文本</summary>
		[Description("Unicode可变长度文本")]
		String = 1,

		/// <summary>自动编号</summary>
		[Description("自动编号")]
		AutomaticIdentification = 2,

		/// <summary>整数</summary>
		[Description("整数")]
		Integer = 3,

		/// <summary>64位浮点型</summary>
		[Description("64位浮点型")]
		Double = 4,

		/// <summary>货币</summary>
		[Description("货币")]
		Currency = 5,

		/// <summary>精确数值</summary>
		[Description("精确数值")]
		Decimal = 6,

		/// <summary>是否</summary>
		[Description("是否")]
		Boolean = 7,

		/// <summary>日期</summary>
		[Description("日期")]
		Date = 8,

		/// <summary>日期时间</summary>
		[Description("日期时间")]
		DateTime = 9,

		/// <summary>大段文本</summary>
		[Description("大段文本")]
		Text = 10,

		/// <summary>单选引用</summary>
		[Description("单选引用")]
		SingleReference = 11,

		/// <summary>多选引用</summary>
		[Description("多选引用")]
		MultiReference = 12,

		/// <summary>系统代码</summary>
		[Description("系统代码")]
		SystemCode = 13,

		/// <summary>系统状态</summary>
		[Description("系统状态")]
		SystemStatus = 14,

		/// <summary>下拉单选</summary>
		[Description("下拉单选")]
		SingleSelectDropDown = 15,

		/// <summary>多选</summary>
		[Description("多选")]
		MultiSelect = 16,

		/// <summary>图片</summary>
		[Description("图片")]
		Image = 17,

		/// <summary>一个 8 位无符号整数，范围在 0 到 255 之间。</summary>
		[Description("8位整数")]
		TinyInt = 18,

		/// <summary>16位整数</summary>
		[Description("16位整数")]
		SmallInt = 19,

		/// <summary>64位整数</summary>
		[Description("64位整数")]
		BigInt = 20,

		/// <summary>全局唯一标识符</summary>
		[Description("全局唯一标识符")]
		GUID = 21,

		/// <summary>单选关联</summary>
		[Description("单选关联")]
		SingleRelationAssociate = 22,

		/// <summary>Unicode固定长度文本</summary>
		[Description("Unicode固定长度文本")]
		StringFixedLength = 23,

		/// <summary>普通可变长度文本</summary>
		[Description("普通可变长度文本")]
		AnsiString = 24,

		/// <summary>普通固定长度文本</summary>
		[Description("普通固定长度文本")]
		AnsiStringFixedLength = 25,

		/// <summary>全局唯一标识符（32位字符）</summary>
		[Description("全局唯一标识符（32位字符）")]
		Guid32Digits = 26,

		/// <summary>可排序全局唯一标识符</summary>
		[Description("可排序全局唯一标识符")]
		CombGuid = 27,

		/// <summary>可排序全局唯一标识符（32位字符）</summary>
		[Description("可排序全局唯一标识符（32位字符）")]
		CombGuid32Digits = 28,

		/// <summary>精确日期和时间数据</summary>
		[Description("精确日期时间")]
		DateTime2 = 29,

		/// <summary>精确带时区的日期和时间数据</summary>
		[Description("精确带时区的日期时间")]
		DateTimeOffset = 30,

		/// <summary>表示值介于 -128 到 127 之间的有符号 8 位整数</summary>
		[Description("8位有符号整数")]
		SignedTinyInt = 31,

		/// <summary>一日内时间</summary>
		[Description("时间")]
		Time = 32,

		/// <summary>32位浮点型</summary>
		[Description("32位浮点型")]
		Float = 33,

		/// <summary>XML</summary>
		[Description("XML")]
		Xml = 34,

		/// <summary>Json</summary>
		[Description("Json")]
		Json = 35
	}

	#endregion
}
