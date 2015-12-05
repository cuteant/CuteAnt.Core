﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
	/// <summary>全局方法程序集</summary>
	internal class DynamicAssembly
	{
		private String _Name;

		/// <summary>名称</summary>
		public String Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		public DynamicAssembly(String name)
		{
			Name = name;
		}

		private AssemblyBuilder _AsmBuilder;

		/// <summary>程序集创建器</summary>
		public AssemblyBuilder AsmBuilder
		{
			get
			{
				if (_AsmBuilder == null)
				{
					AssemblyName aname = new AssemblyName(Name);
					_AsmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(aname, AssemblyBuilderAccess.RunAndSave);
				}
				return _AsmBuilder;
			}

			//set { _AsmBuilder = value; }
		}

		private ModuleBuilder _ModBuilder;

		/// <summary>模块创建器</summary>
		public ModuleBuilder ModBuilder
		{
			get
			{
				if (_ModBuilder == null)
				{
					_ModBuilder = AsmBuilder.DefineDynamicModule(Name + ".dll");
				}
				return _ModBuilder;
			}

			//set { _ModBuilder = value; }
		}

		private TypeBuilder _TypeBuilder;

		/// <summary>类型创建器</summary>
		public TypeBuilder TypeBuilder
		{
			get { return _TypeBuilder; }

			private set { _TypeBuilder = value; }
		}

		/// <summary>添加全局方法</summary>
		/// <param name="method"></param>
		/// <param name="fun"></param>
		public void AddGlobalMethod(MethodInfo method, Action<ILGenerator> fun)
		{
			ParameterInfo[] ps = method.GetParameters();

			//ListX<Type> paramTypes = ListX<Type>.From<ParameterInfo>(ps, delegate(ParameterInfo item) { return item.ParameterType; });
			Type[] paramTypes = ps.Select<ParameterInfo, Type>(item => item.ParameterType).ToArray<Type>();

			//TypeBuilder tbuilder = mbuilder.DefineType(method.DeclaringType.Name, TypeAttributes.Public);
			//MethodBuilder mb = tbuilder.DefineMethod(method.Name.Replace(".", "_"), method.Attributes, method.ReturnType, paramTypes);
			if (TypeBuilder == null)
			{
				TypeBuilder = ModBuilder.DefineType(Name, TypeAttributes.Public);
			}
			String name = method.Name.Replace(".", "_");
			if (name.IsNullOrWhiteSpace()) name = "Test" + DateTime.Now.Ticks;

			//MethodBuilder mb = ModBuilder.DefineGlobalMethod(name, method.Attributes, method.ReturnType, paramTypes.ToArray());
			MethodBuilder mb = TypeBuilder.DefineMethod(name, method.Attributes, method.ReturnType, paramTypes);
			ILGenerator il = mb.GetILGenerator();
			fun(il);
		}

		/// <summary>保存</summary>
		/// <param name="fileName"></param>
		public void Save(String fileName)
		{
			//ModBuilder.CreateGlobalFunctions();
			TypeBuilder.CreateType();
			if (fileName.IsNullOrWhiteSpace())
			{
				fileName = Name + ".dll";
			}
			AsmBuilder.Save(fileName);
		}
	}
}