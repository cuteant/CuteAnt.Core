﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 *
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 *
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuteAnt.OrmLite
{
	/// <summary>条件表达式</summary>
	public class WhereExpression : Expression
	{
		#region -- 属性 --

		//private List<ExpItem> _Expressions = new List<ExpItem>();
		///// <summary>表达式集合</summary>
		//private List<ExpItem> Expressions { get { return _Expressions; } set { _Expressions = value; } }
		/// <summary>表达式集合</summary>
		private List<ExpItem> Expressions = new List<ExpItem>();

		private class ExpItem
		{
			internal Boolean IsAnd;
			internal Expression Exp;

			internal ExpItem(Boolean isAnd, Expression exp)
			{
				IsAnd = isAnd;
				Exp = exp;
			}

			public override String ToString()
			{
				return (IsAnd ? ExpressionConstants.AndSP : ExpressionConstants.OrSP) + Exp;
			}
		}

		/// <summary>空表达式</summary>
		public override Boolean IsEmpty
		{
			get
			{
				var exps = Expressions;
				if (exps.Count == 0) { return true; }

				foreach (var item in exps)
				{
					if (!item.Exp.IsEmpty) { return false; }
				}

				return true;
			}
		}

		#endregion

		#region -- 构造 --

		/// <summary>实例化</summary>
		public WhereExpression()
		{
		}

		/// <summary>把普通表达式包装为条件表达式表达式</summary>
		/// <param name="exp"></param>
		public WhereExpression(Expression exp)
		{
			And(exp);
		}

		#endregion

		#region -- 方法 --

		/// <summary>And操作</summary>
		/// <param name="exp"></param>
		/// <returns></returns>
		public WhereExpression And(Expression exp)
		{
			if (exp != null)
			{
				// 如果前面有Or，则整体推入下一层
				if (Expressions.Any(e => !e.IsAnd))
				{
					var where = new WhereExpression();
					where.Expressions.AddRange(Expressions);

					Expressions.Clear();
					Expressions.Add(new ExpItem(true, where));
				}

				Expressions.Add(new ExpItem(true, exp));
			}

			return this;
		}

		/// <summary>Or操作</summary>
		/// <param name="exp"></param>
		/// <returns></returns>
		public WhereExpression Or(Expression exp)
		{
			if (exp != null) { Expressions.Add(new ExpItem(false, exp)); }

			return this;
		}

		/// <summary>当前表达式作为子表达式</summary>
		/// <returns></returns>
		public WhereExpression AsChild()
		{
			return new WhereExpression(this);
		}

		/// <summary>输出条件表达式的字符串表示，遍历表达式集合并拼接起来</summary>
		/// <param name="needBracket">外部是否需要括号。如果外部要求括号，而内部又有Or，则加上括号</param>
		/// <returns></returns>
		public override String GetString(Boolean needBracket)
		{
			var exps = Expressions;
			if (exps.Count == 0) { return null; }

			// 重整表达式
			var list = new List<ExpItem>();
			var sub = new List<ExpItem>();

			var hasOr = false;
			// 优先计算And，所有And作为一个整体表达式进入内层，处理完以后当前层要么全是And，要么全是Or
			for (int i = 0; i < exps.Count; i++)
			{
				sub.Add(exps[i]);
				// 如果下一个是Or，或者已经是最后一个，则合并sub到list
				if (i < exps.Count - 1 && !exps[i + 1].IsAnd || i == exps.Count - 1)
				{
					// sub创建新exp加入list
					// 一个就不用创建了
					if (sub.Count == 1)
					{
						list.Add(sub[0]);
						if (list.Count > 0 && !sub[0].IsAnd) { hasOr = true; }
					}
					else if (i == exps.Count - 1 && list.Count == 0)
					{
						list.AddRange(sub);
					}
					else
					{
						// 这一片And凑成一个子表达式
						var where = new WhereExpression();
						where.Expressions.AddRange(sub);
						list.Add(new ExpItem(false, where));
						hasOr = true;
					}

					sub.Clear();
				}
			}
			// 第一个表达式的And/Or必须正确代表本层所有表达式
			list[0].IsAnd = !hasOr;

			// 开始计算
			var sb = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				var exp = item.Exp;
				exp.Strict = Strict;

				// 里面是Or的时候，外面前后任意一个And，需要括号
				var str = exp.GetString(item.IsAnd || i < list.Count - 1 && list[i + 1].IsAnd);
				// 跳过没有返回的表达式
				if (str.IsNullOrWhiteSpace()) { continue; }

				if (sb.Length > 0)
				{
					sb.AppendFormat(" {0} ", item.IsAnd ? ExpressionConstants.And : ExpressionConstants.Or);
					// 不能判断第一个，控制符可能不正确
					if (!item.IsAnd) hasOr = true;
				}
				sb.Append(str);
			}

			if (sb.Length == 0) { return null; }
			if (needBracket && hasOr) { return "({0})".FormatWith(sb.ToString()); }
			return sb.ToString();
		}

		/// <summary>有条件And操作</summary>
		/// <param name="condition"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		public WhereExpression AndIf(Boolean condition, Expression exp)
		{
			return condition ? And(exp) : this;
		}

		/// <summary>有条件Or操作</summary>
		/// <param name="condition"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		public WhereExpression OrIf(Boolean condition, Expression exp)
		{
			return condition ? Or(exp) : this;
		}

		/// <summary>输出该表达式的字符串形式</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return GetString();
		}

		#endregion

		#region -- 分组 --

		/// <summary>按照指定若干个字段分组。没有条件时使用分组请用FieldItem的GroupBy</summary>
		/// <param name="names"></param>
		/// <returns>返回条件语句加上分组语句</returns>
		public String GroupBy(params String[] names)
		{
			var where = GetString(false);
			var sb = new StringBuilder();
			foreach (var item in names)
			{
				sb.AppendSeparate(",").Append(item);
			}

			if (where.IsNullOrWhiteSpace())
				return "Group By {0}".FormatWith(sb.ToString());
			else
				return "{1} Group By {0}".FormatWith(sb.ToString(), where);
		}

		#endregion
	}
}