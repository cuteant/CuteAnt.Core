using System;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>Comparison Result, comprising a <see cref="ResultType"/> (Add/Delete/Change) and a <see cref="SchemaObjectType"/> (Table, Column, Constraint etc)</summary>
	internal sealed class CompareResult
	{
		/// <summary>Gets or sets the type of the result  (Add/Delete/Change).</summary>
		/// <value>The type of the result.</value>
		internal ResultType ResultType { get; set; }

		/// <summary>Gets or sets the type of the schema object  (Table, Column, Constraint etc).</summary>
		/// <value>The type of the schema object.</value>
		internal SchemaObjectType SchemaObjectType { get; set; }

		/// <summary>Gets or sets the SQL script.</summary>
		/// <value>The script.</value>
		internal String Script { get; set; }

		internal String Remark { get; set; }

		internal CompareResult(ResultType resultType, SchemaObjectType objectType, String script, String remark)
		{
			ResultType = resultType;
			SchemaObjectType = objectType;
			Script = script;
			Remark = remark;
		}

		/// <summary>已重载，返回当前 CompareResult 对象的字符串表示。</summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "--{0}{1}：{2}{3}{4}".FormatWith(ResultType.GetDescription(), SchemaObjectType.GetDescription(), Remark, Environment.NewLine, Script);
		}
	}
}