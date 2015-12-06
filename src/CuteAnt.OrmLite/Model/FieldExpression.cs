using System;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite
{
	/// <summary>字段表达式</summary>
	public class FieldExpression : Expression
	{
		#region -- 属性 --

		private FieldItem _Field;
		/// <summary>字段</summary>
		public FieldItem Field { get { return _Field; } set { _Field = value; } }

		private String _Action;
		/// <summary>动作</summary>
		public String Action { get { return _Action; } set { _Action = value; } }

		private Object _Value;
		/// <summary>值</summary>
		public Object Value { get { return _Value; } set { _Value = value; } }

		/// <summary>空表达式</summary>
		public override Boolean IsEmpty
		{
			get
			{
				if (Field == null) { return true; }

				// 严格模式下，判断字段表达式是否有效
				if (Strict > ExpressionStrictMode.Default)
				{
					// 所有空值无效
					if (Value == null) { return true; }

					// 如果数据为空，则返回
					if (Strict > ExpressionStrictMode.Strict)
					{
						// 整型
						var dc = Field.Field;
						if (dc != null)
						{
							if (dc.DbType.IsIntType() && Value.ToInt() <= 0) { return true; }
						}
						else
						{
							if (Field.DataType.IsIntType() && Value.ToInt() <= 0) { return true; }
						}
						// 字符串
						if (Field.DataType == TypeX._.String && Value + "" == "") { return true; }
						// 时间
						if (Field.DataType == typeof(DateTime) && Value.ToDateTime() <= DateTime.MinValue) { return true; }
					}
				}
				return false;
			}
		}

		#endregion

		#region -- 构造 --

		/// <summary>构造字段表达式</summary>
		/// <param name="field"></param>
		/// <param name="action"></param>
		/// <param name="value"></param>
		/// <param name="strict"></param>
		public FieldExpression(FieldItem field, String action, Object value, ExpressionStrictMode strict)
		{
			Field = field;
			Action = action;
			Value = value;
			Strict = strict;
		}

		#endregion

		#region -- 输出 --

		/// <summary>已重载。输出字段表达式的字符串形式</summary>
		/// <param name="needBracket">外部是否需要括号。如果外部要求括号，而内部又有Or，则加上括号</param>
		/// <returns></returns>
		public override String GetString(Boolean needBracket)
		{
			if (IsEmpty) { return null; }

			var op = Field.Factory;
			var fi = Value as FieldItem;
			if (fi != null)
			{
				return "{0}{1}{2}".FormatWith(Field.QuotedColumnName, Action, fi.QuotedColumnName);
			}
			else
			{
				return "{0}{1}{2}".FormatWith(Field.QuotedColumnName, Action, op.QuoteValue(Field, Value));
			}
		}

		/// <summary>输出该表达式的字符串形式</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return GetString();
		}

		#endregion
	}
}