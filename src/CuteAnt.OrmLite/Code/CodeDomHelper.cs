/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.CodeDom;
using System.Linq;

namespace CuteAnt.OrmLite.Code
{
	internal static class CodeDomHelper
	{
		internal static CodeExpression ToExp(this Object p)
		{
			if (p == null) { return new CodePrimitiveExpression(p); }

			#region ## 苦竹 修改 2013.08.12 AM 01:43 ##
			//if (p is CodeExpression) { return p as CodeExpression; }
			//if (p is Type) { return new CodeTypeReferenceExpression(p as Type); }
			var ce = p as CodeExpression;
			if (ce != null) { return ce; }
			var type = p as Type;
			if (type != null) { return new CodeTypeReferenceExpression(type); }
			#endregion

			var str = p.ToString();
			if (str == "") { return new CodePrimitiveExpression(p); }

			if (str[0] == '_') { return new CodeFieldReferenceExpression(null, str); }

			if (str[0] == '@') { return new CodeArgumentReferenceExpression(str.Substring(1)); }

			if (str[0] == '$')
			{
				var name = str.Substring(1);
				if (name == "this")
				{
					return new CodeThisReferenceExpression();
				}
				else if (name == "base")
				{
					return new CodeBaseReferenceExpression();
				}
				else if (name[name.Length - 1] == ']')
				{
					var idx = name.IndexOf('[');
					if (idx > 0)
					{
						return new CodeIndexerExpression(str.Substring(0, idx + 1).ToExp(), name.Substring(idx + 1, name.Length - idx - 2).ToExp());
					}
				}

				return new CodeVariableReferenceExpression(name);
			}

			if (p.GetType().IsEnum)
			{
				return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(p.GetType()), p.ToString());
			}

			return new CodePrimitiveExpression(p);
		}

		internal static CodeTypeMember AddAttribute<TAttribute>(this CodeTypeMember member, params Object[] ps)
		{
			//var cs = ps.Select(p =>
			//{
			//    if (p != null && p.GetType().IsEnum)
			//        return new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(p.GetType()), p.ToString()));
			//    else
			//        return new CodeAttributeArgument(new CodePrimitiveExpression(p));
			//}).ToArray();
			var cs = ps.Select(p => new CodeAttributeArgument(p.ToExp())).ToArray();
			member.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(TAttribute)), cs));

			return member;
		}

		internal static CodeCompileUnit AddAttribute<TAttribute>(this CodeCompileUnit unit, params Object[] ps)
		{
			var cs = ps.Select(p => new CodeAttributeArgument(p.ToExp())).ToArray();
			unit.AssemblyCustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(TAttribute)), cs));

			return unit;
		}

		internal static CodeMethodInvokeExpression Invoke(this String methodName, params Object[] ps)
		{
			var cs = ps.Select(p => p.ToExp()).ToArray();
			return new CodeMethodInvokeExpression(null, methodName, cs);
		}

		internal static CodeMethodInvokeExpression Invoke(this CodeExpression targetObject, String methodName, params Object[] ps)
		{
			var cs = ps.Select(p => p.ToExp()).ToArray();
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(targetObject, methodName), cs);
		}

		internal static CodeStatement ToStat(this CodeExpression exp)
		{
			return new CodeExpressionStatement(exp);
		}

		internal static CodeMethodReturnStatement Return(this Object exp)
		{
			return new CodeMethodReturnStatement(exp.ToExp());
		}

		internal static CodeAssignStatement Assign(this CodeExpression left, Object right)
		{
			return new CodeAssignStatement(left, right.ToExp());
		}

		internal static CodeAssignStatement Assign(this String left, Object right)
		{
			return new CodeAssignStatement(left.ToExp(), right.ToExp());
		}

		internal static CodeExpression Equal(this CodeExpression left, Object right)
		{
			return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, right.ToExp());
		}

		internal static CodeExpression Equal(this String left, Object right)
		{
			return new CodeBinaryOperatorExpression(left.ToExp(), CodeBinaryOperatorType.ValueEquality, right.ToExp());
		}

		internal static CodeConditionStatement IfTrue(this CodeExpression condition, params  CodeStatement[] trueStatements)
		{
			return new CodeConditionStatement(condition, trueStatements);
		}

		internal static CodeCastExpression Cast(this Object exp, Type targetType)
		{
			return new CodeCastExpression(new CodeTypeReference(targetType), exp.ToExp());
		}

		#region 注释

		internal static CodeTypeMember AddComment(this CodeTypeMember member, String name, String comment)
		{
			member.Comments.Add(new CodeCommentStatement(String.Format("<{0}>{1}</{0}>", name, comment), true));

			return member;
		}

		internal static CodeTypeMember AddSummary(this CodeTypeMember member, String comment)
		{
			member.AddComment("summary", comment);

			return member;
		}

		internal static CodeTypeMember AddParamComment(this CodeTypeMember member, String name, String comment)
		{
			member.Comments.Add(new CodeCommentStatement(String.Format("<param name=\"{0}\">{1}</param>", name, comment), true));

			return member;
		}

		internal static CodeTypeMember AddReturnComment(this CodeTypeMember member, String comment)
		{
			member.AddComment("return", comment);

			return member;
		}

		#endregion
	}
}