﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Reflection
{
	/// <summary>快速Emit反射</summary>
	public class EmitReflect : DefaultReflect
	{
		#region 反射获取

		/// <summary>根据名称获取类型</summary>
		/// <param name="typeName">类型名</param>
		/// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
		/// <returns></returns>
		public override Type GetType(String typeName, Boolean isLoadAssembly)
		{
			return TypeX.GetType(typeName, isLoadAssembly);
		}

		/// <summary>获取方法</summary>
		/// <remarks>用于具有多个签名的同名方法的场合，不确定是否存在性能问题，不建议普通场合使用</remarks>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <param name="paramTypes">参数类型数组</param>
		/// <returns></returns>
		public override MethodInfo GetMethod(Type type, String name, params Type[] paramTypes)
		{
			// 参数数组必须为空，或者所有参数类型都不为null，才能精确匹配
			if (paramTypes == null || paramTypes.Length == 0 || paramTypes.All(t => t != null))
			{
				var method = base.GetMethod(type, name, paramTypes);
				if (method != null) return method;
			}

			// 任意参数类型为null，换一种匹配方式
			if (paramTypes.Any(t => t == null))
			{
				var ms = GetMethods(type, name, paramTypes.Length);
				if (ms == null || ms.Length == 0) return null;

				// 对比参数
				foreach (var mi in ms)
				{
					var ps = mi.GetParameters();
					var flag = true;
					for (int i = 0; i < ps.Length; i++)
					{
						if (paramTypes[i] != null && !ps[i].ParameterType.IsAssignableFrom(paramTypes[i]))
						{
							flag = false;
							break;
						}
					}
					if (flag) return mi;
				}
			}

			return TypeX.GetMethod(type, name, paramTypes);
		}

		#endregion

		#region 反射调用

		/// <summary>反射调用指定对象的方法</summary>
		/// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
		/// <param name="method">方法</param>
		/// <param name="parameters">方法参数</param>
		/// <returns></returns>
		[DebuggerHidden]
		public override Object Invoke(Object target, MethodBase method, params Object[] parameters)
		{
			if (method is MethodInfo)
			{
				return MethodInfoX.Create(method).Invoke(target, parameters);
			}
			else
			{
				return base.Invoke(target, method, parameters);
			}
		}

		/// <summary>反射调用指定对象的方法</summary>
		/// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
		/// <param name="method">方法</param>
		/// <param name="parameters">方法参数字典</param>
		/// <returns></returns>
		public override Object InvokeWithParams(Object target, MethodBase method, IDictionary parameters)
		{
			if (method is MethodInfo)
				return MethodInfoX.Create(method).InvokeWithParams(target, parameters);
			else
				return base.Invoke(target, method, parameters);
		}

		#endregion

		#region 类型辅助

		/// <summary>获取一个类型的元素类型</summary>
		/// <param name="type">类型</param>
		/// <returns></returns>
		public override Type GetElementType(Type type)
		{
			return TypeX.GetElementType(type);
		}

		/// <summary>类型转换</summary>
		/// <param name="value">数值</param>
		/// <param name="conversionType"></param>
		/// <returns></returns>
		public override Object ChangeType(Object value, Type conversionType)
		{
			return TypeX.ChangeType(value, conversionType);
		}

		/// <summary>获取类型的友好名称</summary>
		/// <param name="type">指定类型</param>
		/// <param name="isfull">是否全名，包含命名空间</param>
		/// <returns></returns>
		public override String GetName(Type type, Boolean isfull)
		{
			return TypeX.GetName(type, isfull);
		}

		#endregion

		#region 插件

		/// <summary>在指定程序集中查找指定基类的子类</summary>
		/// <param name="asm">指定程序集</param>
		/// <param name="baseType">基类或接口，为空时返回所有类型</param>
		/// <returns></returns>
		public override IEnumerable<Type> GetSubclasses(Assembly asm, Type baseType)
		{
			return AssemblyX.Create(asm).FindPlugins(baseType);
		}

		/// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
		/// <param name="baseType">基类或接口</param>
		/// <param name="isLoadAssembly">是否加载为加载程序集</param>
		/// <returns></returns>
		public override IEnumerable<Type> GetAllSubclasses(Type baseType, Boolean isLoadAssembly)
		{
			return AssemblyX.FindAllPlugins(baseType, isLoadAssembly);
		}

		#endregion
	}
}