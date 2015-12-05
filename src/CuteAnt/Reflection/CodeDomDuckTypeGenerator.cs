/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace CuteAnt.Reflection
{
	internal class CodeDomDuckTypeGenerator
	{
		public Type[] CreateDuckTypes(Type interfaceType, Type[] duckedTypes)
		{
			const String TYPE_PREFIX = "Duck";

			var namespaceName = this.GetType().Namespace + "." + interfaceType.Name;

			var codeCU = new CodeCompileUnit();
			var codeNsp = new CodeNamespace(namespaceName);
			codeCU.Namespaces.Add(codeNsp);

			//CodeTypeReference codeTRInterface = new CodeTypeReference(interfaceType);
			var codeTRInterface = new CodeTypeReference(interfaceType.GetName(true));
			var references = new ReferenceList();

			// ��������ÿһ����Ҫ�������
			for (Int32 i = 0; i < duckedTypes.Length; i++)
			{
				Type objectType = duckedTypes[i];

				//CodeTypeReference codeTRObject = new CodeTypeReference(objectType);
				var codeTRObject = new CodeTypeReference(objectType.GetName(true));
				references.AddReference(objectType);

				var codeType = new CodeTypeDeclaration(TYPE_PREFIX + i);
				codeNsp.Types.Add(codeType);

				codeType.TypeAttributes = TypeAttributes.Public;
				codeType.BaseTypes.Add(codeTRInterface);

				// ����һ���ֶ�
				CodeMemberField codeFldObj = new CodeMemberField(codeTRObject, "_obj");
				codeType.Members.Add(codeFldObj);
				CodeFieldReferenceExpression codeFldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeFldObj.Name);

				// ����һ�����캯��
				CodeConstructor codeCtor = new CodeConstructor();
				codeType.Members.Add(codeCtor);
				codeCtor.Attributes = MemberAttributes.Public;
				codeCtor.Parameters.Add(new CodeParameterDeclarationExpression(codeTRObject, "obj"));
				codeCtor.Statements.Add(
						new CodeAssignStatement(
								codeFldRef,
								new CodeArgumentReferenceExpression("obj")
						)
				);

				// ������Ա
				CreateMember(interfaceType, objectType, codeType, references, codeFldRef);
			}

			#region ����

			CSharpCodeProvider codeprov = new CSharpCodeProvider();
#if DEBUG
			{
				StringWriter sw = new StringWriter();
				codeprov.GenerateCodeFromCompileUnit(codeCU, sw, new CodeGeneratorOptions());
				String code = sw.ToString();
				Console.WriteLine(code);
			}
#endif
			CompilerParameters compilerParams = new CompilerParameters();
			compilerParams.GenerateInMemory = true;
			compilerParams.ReferencedAssemblies.Add(interfaceType.Assembly.Location);
			references.SetToCompilerParameters(compilerParams);
			CompilerResults cres = codeprov.CompileAssemblyFromDom(compilerParams, codeCU);
			if (cres.Errors.Count > 0)
			{
				StringWriter sw = new StringWriter();

				foreach (CompilerError err in cres.Errors)
				{
					sw.WriteLine(err.ErrorText);
				}
				throw new InvalidOperationException("�������: \n\n" + sw.ToString());
			}
			Assembly assembly = cres.CompiledAssembly;
			Type[] res = new Type[duckedTypes.Length];

			for (Int32 i = 0; i < duckedTypes.Length; i++)
			{
				res[i] = assembly.GetType(namespaceName + "." + TYPE_PREFIX + i);
			}
			return res;

			#endregion
		}

		private void CreateMember(Type interfaceType, Type duckType, CodeTypeDeclaration codeType, ReferenceList references, CodeFieldReferenceExpression codeFldRef)
		{
			var codeTRInterface = new CodeTypeReference(interfaceType.GetName(true));

			//// �ҵ�duckType�����Ƿ��й�����_obj;
			//FieldInfo fiObj = duckType.GetField("_obj", BindingFlags.Public | BindingFlags.Instance);
			//Type innerType = fiObj != null ? fiObj.FieldType : null;

			CodeFieldReferenceExpression fdRef = null;

			#region ����

			foreach (var mi in interfaceType.GetMethods())
			{
				// ����ר�����ֵķ����������Ե�get/set�����й��캯��
				if ((mi.Attributes & MethodAttributes.SpecialName) != 0) { continue; }
				CodeMemberMethod codeMethod = new CodeMemberMethod();
				codeType.Members.Add(codeMethod);
				codeMethod.Name = mi.Name;
				codeMethod.ReturnType = new CodeTypeReference(mi.ReturnType);
				codeMethod.PrivateImplementationType = codeTRInterface;
				references.AddReference(mi.ReturnType);
				ParameterInfo[] parameters = mi.GetParameters();
				CodeArgumentReferenceExpression[] codeArgs = new CodeArgumentReferenceExpression[parameters.Length];
				Int32 n = 0;
				Type[] pits = new Type[parameters.Length];

				foreach (ParameterInfo parameter in parameters)
				{
					pits[n] = parameter.ParameterType;
					references.AddReference(parameter.ParameterType);
					CodeParameterDeclarationExpression codeParam = new CodeParameterDeclarationExpression(parameter.ParameterType, parameter.Name);
					codeMethod.Parameters.Add(codeParam);
					codeArgs[n++] = new CodeArgumentReferenceExpression(parameter.Name);
				}
				CodeMethodInvokeExpression codeMethodInvoke = new CodeMethodInvokeExpression(FindMember(duckType, mi, codeFldRef), mi.Name, codeArgs);
				if (mi.ReturnType == typeof(void))
				{
					codeMethod.Statements.Add(codeMethodInvoke);
				}
				else
				{
					codeMethod.Statements.Add(new CodeMethodReturnStatement(codeMethodInvoke));
				}
			}

			#endregion

			#region ����

			foreach (PropertyInfo pi in interfaceType.GetProperties())
			{
				CodeMemberProperty property = new CodeMemberProperty();
				codeType.Members.Add(property);
				property.Name = pi.Name;
				property.Type = new CodeTypeReference(pi.PropertyType);
				property.Attributes = MemberAttributes.Public;
				property.PrivateImplementationType = codeTRInterface;
				references.AddReference(pi.PropertyType);
				ParameterInfo[] parameters = pi.GetIndexParameters();
				CodeArgumentReferenceExpression[] args = new CodeArgumentReferenceExpression[parameters.Length];
				Int32 n = 0;

				foreach (ParameterInfo parameter in parameters)
				{
					CodeParameterDeclarationExpression codeParam = new CodeParameterDeclarationExpression(parameter.ParameterType, parameter.Name);
					property.Parameters.Add(codeParam);
					references.AddReference(parameter.ParameterType);
					CodeArgumentReferenceExpression codeArgRef = new CodeArgumentReferenceExpression(parameter.Name);
					args[n++] = codeArgRef;
				}
				fdRef = FindMember(duckType, pi, codeFldRef);
				if (pi.CanRead)
				{
					property.HasGet = true;
					if (args.Length == 0)
					{
						property.GetStatements.Add(
								new CodeMethodReturnStatement(
										new CodePropertyReferenceExpression(
												fdRef,
												pi.Name
										)
								)
						);
					}
					else
					{
						property.GetStatements.Add(
								new CodeMethodReturnStatement(
										new CodeIndexerExpression(
												fdRef,
												args
										)
								)
						);
					}
				}
				if (pi.CanWrite)
				{
					property.HasSet = true;
					if (args.Length == 0)
					{
						property.SetStatements.Add(
								new CodeAssignStatement(
										new CodePropertyReferenceExpression(
												fdRef,
												pi.Name
										),
										new CodePropertySetValueReferenceExpression()
								)
						);
					}
					else
					{
						property.SetStatements.Add(
								new CodeAssignStatement(
										new CodeIndexerExpression(
												fdRef,
												args
										),
										new CodePropertySetValueReferenceExpression()
								)
						);
					}
				}
			}

			#endregion

			#region �¼�

			foreach (EventInfo ei in interfaceType.GetEvents())
			{
				fdRef = FindMember(duckType, ei, codeFldRef);
				StringBuilder sbCode = new StringBuilder();
				sbCode.Append("public event " + ei.EventHandlerType.FullName + " @" + ei.Name + "{");

				//sbCode.Append("add    {" + codeFldObj.Name + "." + ei.Name + "+=value;}");
				//sbCode.Append("remove {" + codeFldObj.Name + "." + ei.Name + "-=value;}");
				if (fdRef == codeFldRef)
				{
					sbCode.Append("add    {" + codeFldRef.FieldName + "." + ei.Name + "+=value;}");
					sbCode.Append("remove {" + codeFldRef.FieldName + "." + ei.Name + "-=value;}");
				}
				else
				{
					sbCode.Append("add    {" + fdRef.FieldName + "." + codeFldRef.FieldName + "." + ei.Name + "+=value;}");
					sbCode.Append("remove {" + fdRef.FieldName + "." + codeFldRef.FieldName + "." + ei.Name + "-=value;}");
				}
				sbCode.Append("}");
				references.AddReference(ei.EventHandlerType);
				codeType.Members.Add(new CodeSnippetTypeMember(sbCode.ToString()));
			}

			#endregion

			#region �ݹ���ӿ�

			Type[] ts = interfaceType.GetInterfaces();
			if (ts != null && ts.Length > 0)
			{
				foreach (Type item in ts)
				{
					CreateMember(item, duckType, codeType, references, codeFldRef);
				}
			}

			#endregion
		}

		private CodeFieldReferenceExpression FindMember(Type duckType, MemberInfo mi, CodeFieldReferenceExpression codeFldRef)
		{
			MemberInfo[] infos = duckType.GetMember(mi.Name);
			if (infos != null && infos.Length > 0)
			{
				return codeFldRef;
			}
			else
			{
				// �ҵ�duckType�����Ƿ��й�����_obj;
				FieldInfo fiObj = duckType.GetField("_obj", BindingFlags.Public | BindingFlags.Instance);
				if (fiObj != null)
				{
					Type innerType = fiObj.FieldType;
					if (mi.DeclaringType.IsAssignableFrom(innerType))
					{
						return new CodeFieldReferenceExpression(codeFldRef, fiObj.Name);
					}
				}
			}
			return codeFldRef;
		}

		private class ReferenceList
		{
			private List<String> _lst = new List<String>();
			private static readonly Assembly mscorlib = typeof(object).Assembly;

			public Boolean AddReference(Assembly assembly)
			{
				try
				{
					if (!_lst.Contains(assembly.Location) && assembly != mscorlib)
					{
						_lst.Add(assembly.Location);
						return true;
					}
				}
				catch { }
				return false;
			}

			public void AddReference(Type type)
			{
				AddReference(type.Assembly);
				if (type.BaseType != null && type.BaseType.Assembly != mscorlib)
				{
					AddReference(type.BaseType);
				}
			}

			public void SetToCompilerParameters(CompilerParameters parameters)
			{
				foreach (String reference in _lst)
				{
					parameters.ReferencedAssemblies.Add(reference);
				}
			}
		}
	}
}