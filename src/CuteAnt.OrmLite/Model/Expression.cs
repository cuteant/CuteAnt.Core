using System;
using System.Text.RegularExpressions;

namespace CuteAnt.OrmLite
{
	/// <summary>严格模式</summary>
	public enum ExpressionStrictMode
	{
		/// <summary>默认</summary>
		Default = 0,

		/// <summary>严格模式。为Null的参数都忽略</summary>
		Strict = 1,

		/// <summary>完全严格模式。整型0、时间最小值、空字符串，都忽略</summary>
		FullStrict = 2
	}

	/// <summary>表达式基类</summary>
	public class Expression
	{
		#region @@ 属性 @@

		private String _Text;

		/// <summary>文本表达式</summary>
		public String Text { get { return _Text; } set { _Text = value; } }

		private ExpressionStrictMode _Strict = ExpressionStrictMode.Default;

		/// <summary>严格模式。在严格模式下将放弃一些不满足要求的表达式。</summary>
		public ExpressionStrictMode Strict { get { return _Strict; } set { _Strict = value; } }

		/// <summary>空表达式</summary>
		public virtual Boolean IsEmpty { get { return String.IsNullOrEmpty(_Text); } }

		#endregion

		#region @@ 构造 @@

		/// <summary>实例化简单表达式</summary>
		public Expression()
		{
		}

		/// <summary>用一段文本实例化简单表达式</summary>
		/// <param name="value"></param>
		public Expression(String value)
		{
			Text = value;
		}

		#endregion

		#region -- 方法 --

		/// <summary>设置严格模式</summary>
		/// <param name="mode">严格模式。为Null的参数都忽略；完全严格模式。整型0、时间最小值、空字符串，都忽略</param>
		/// <returns></returns>
		public Expression SetStrict(ExpressionStrictMode mode)
		{
			Strict = mode;
			return this;
		}

		/// <summary>用于匹配Or关键字的正则表达式</summary>
		protected internal static Regex _regOr = new Regex(@"\bOr\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>获取表达式的文本表示</summary>
		/// <param name="needBracket">外部是否需要括号。如果外部要求括号，而内部又有Or，则加上括号</param>
		/// <returns></returns>
		public virtual String GetString(Boolean needBracket)
		{
			if (Text.IsNullOrWhiteSpace()) { return Text; }

			// 如果外部要求括号，而内部又有Or，则加上括号
			if (needBracket && _regOr.IsMatch(Text)) { return "({0})".FormatWith(Text); }

			return Text;
		}

		/// <summary>获取表达式的文本表示</summary>
		/// <returns></returns>
		public String GetString()
		{
			return GetString(false);
		}

		/// <summary>输出该表达式的字符串形式</summary>
		/// <returns></returns>
		public override String ToString()
		{
			//if (this.GetType() == typeof(Expression)) { return Text; }

			return GetString();
		}

		/// <summary>类型转换</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static implicit operator String(Expression obj)
		{
			return obj != null ? obj.GetString() : null;
		}

		#endregion

		#region -- 重载运算符 --

		/// <summary>重载运算符实现And操作</summary>
		/// <param name="exp"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static WhereExpression operator &(Expression exp, Expression value)
		{
			return And(exp, value);
		}

		/// <summary>重载运算符实现And操作</summary>
		/// <param name="exp"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static WhereExpression operator &(Expression exp, String value)
		{
			return And(exp, new Expression(value));
		}

		private static WhereExpression And(Expression exp, Expression value)
		{
			// 如果exp为空，主要考虑右边
			if (exp == null) { CreateWhere(value); }

			// 左边构造条件表达式，自己是也好，新建立也好
			var where = CreateWhere(exp);
			if (value == null) { return where; }

			// 如果右边为空，创建的表达式将会失败，直接返回左边
			return where.And(value);
		}

		/// <summary>重载运算符实现Or操作</summary>
		/// <param name="exp"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static WhereExpression operator |(Expression exp, Expression value)
		{
			return Or(exp, value);
		}

		/// <summary>重载运算符实现Or操作</summary>
		/// <param name="exp"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static WhereExpression operator |(Expression exp, String value)
		{
			return Or(exp, new Expression(value));
		}

		private static WhereExpression Or(Expression exp, Expression value)
		{
			// 如果exp为空，主要考虑右边
			if (exp == null) { CreateWhere(value); }

			// 左边构造条件表达式，自己是也好，新建立也好
			var where = CreateWhere(exp);
			if (value == null) { return where; }

			// 如果右边为空，创建的表达式将会失败，直接返回左边
			return where.Or(value);
		}

		private static WhereExpression CreateWhere(Expression value)
		{
			if (value == null) { return new WhereExpression(); }
			var whereExp = value as WhereExpression;
			if (whereExp != null) { return (whereExp); }

			return new WhereExpression(value);
		}

		#endregion
	}
}