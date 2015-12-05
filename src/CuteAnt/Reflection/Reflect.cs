/*
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using CuteAnt.Collections;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Reflection
{
	/// <summary>反射工具类</summary>
	public static class Reflect
	{
		#region -- 属性 --

		//private static IReflect _Current = new DefaultReflect();
		private static IReflect _Provider = new EmitReflect();

		/// <summary>当前反射提供者</summary>
		public static IReflect Provider
		{
			get { return _Provider; }
			set { _Provider = value; }
		}

		#endregion

		#region -- 反射获取 --

		/// <summary>根据名称获取类型。可搜索当前目录DLL，自动加载</summary>
		/// <param name="typeName">类型名</param>
		/// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Type GetTypeEx(this String typeName, Boolean isLoadAssembly = true)
		{
			if (typeName.IsNullOrWhiteSpace()) { return null; }

			return _Provider.GetType(typeName, isLoadAssembly);
		}

		private const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

		#region - GetEvent(s) -

		/// <summary>返回表示当前类型声明的指定公共事件的对象。</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static EventInfo GetDeclaredEventEx(this Type type, String name)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GetDeclaredEvent(name);
#else
			return type.GetEvent(name, DeclaredOnlyLookup);
#endif
		}

		/// <summary>获取当前类型定义的操作的集合</summary>
		/// <param name="type"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<EventInfo> GetDeclaredEventsEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().DeclaredEvents;
#else
			return type.GetEvents(DeclaredOnlyLookup);
#endif
		}

		#endregion

		#region - GetField(s) -

		/// <summary>返回表示当前类型声明的指定公共字段的对象</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static FieldInfo GetDeclaredFieldEx(this Type type, String name)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GetDeclaredField(name);
#else
			return type.GetField(name, DeclaredOnlyLookup);
#endif
		}

		/// <summary>获取当前类型定义的字段的集合</summary>
		/// <param name="type"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<FieldInfo> GetDeclaredFieldsEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().DeclaredFields;
#else
			return type.GetFields(DeclaredOnlyLookup);
#endif
		}

		/// <summary>获取字段。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <param name="ignoreCase">忽略大小写</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static FieldInfo GetFieldEx(this Type type, String name, Boolean ignoreCase = false)
		{
			if (name.IsNullOrWhiteSpace()) { return null; }

			return _Provider.GetField(type, name, ignoreCase);
		}

		/// <summary>获取成员。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <param name="ignoreCase">忽略大小写</param>
		/// <returns></returns>
		public static MemberInfo GetMemberEx(this Type type, String name, Boolean ignoreCase = false)
		{
			if (name.IsNullOrWhiteSpace()) { return null; }

			return _Provider.GetMember(type, name, ignoreCase);
		}

		#endregion

		#region - GetProperty(s) -

		/// <summary>返回表示当前类型声明的公共属性的对象</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static PropertyInfo GetDeclaredPropertyEx(this Type type, String name)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GetDeclaredProperty(name);
#else
			return type.GetProperty(name, DeclaredOnlyLookup);
#endif
		}

		/// <summary>获取指定类型定义的属性的集合</summary>
		/// <param name="type"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<PropertyInfo> GetDeclaredPropertiesEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().DeclaredProperties;
#else
			return type.GetProperties(DeclaredOnlyLookup);
#endif
		}

		/// <summary>获取属性。搜索私有、静态、基类，优先返回大小写精确匹配成员</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <param name="ignoreCase">忽略大小写</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static PropertyInfo GetPropertyEx(this Type type, String name, Boolean ignoreCase = false)
		{
			if (name.IsNullOrWhiteSpace()) { return null; }

			return _Provider.GetProperty(type, name, ignoreCase);
		}

		#endregion

		#region - GetMethod(s) -

		/// <summary>返回表示当前类型声明的指定公共方法的对象</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static MethodInfo GetDeclaredMethodEx(this Type type, String name)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GetDeclaredMethod(name);
#else
			return type.GetMethod(name, DeclaredOnlyLookup);
#endif
		}

		/// <summary>获取当前类型定义方法的集合</summary>
		/// <param name="type">类型</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<MethodInfo> GetDeclaredMethodsEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().DeclaredMethods;
#else
			return type.GetMethods(DeclaredOnlyLookup);
#endif
		}

		/// <summary>返回包含在当前类型声明的所有公共方法与指定的名称的集合</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<MethodInfo> GetDeclaredMethodsEx(this Type type, String name)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GetDeclaredMethods(name);
#else
			return type.GetMethods(DeclaredOnlyLookup).Where(m => m.Name == name);
#endif
		}

		/// <summary>获取方法</summary>
		/// <param name="type">类型</param>
		/// <param name="name">名称</param>
		/// <param name="paramTypes">参数类型数组</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static MethodInfo GetMethodEx(this Type type, String name, params Type[] paramTypes)
		{
			if (name.IsNullOrWhiteSpace()) { return null; }

			return _Provider.GetMethod(type, name, paramTypes);
		}

		/// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
		/// <returns></returns>
		public static MethodInfo[] GetMethodsEx(this Type type, String name, Int32 paramCount = -1)
		{
			if (name.IsNullOrWhiteSpace()) { return null; }

			return _Provider.GetMethods(type, name, paramCount);
		}

		#endregion

		#endregion

		#region -- 反射调用 --

		/// <summary>反射创建指定类型的实例</summary>
		/// <param name="type">类型</param>
		/// <param name="parameters">参数数组</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		[DebuggerHidden]
		public static Object CreateInstance(this Type type, params Object[] parameters)
		{
			ValidationHelper.ArgumentNull(type, "type");

			return _Provider.CreateInstance(type, parameters);
		}

		/// <summary>反射调用指定对象的方法。target为类型时调用其静态方法</summary>
		/// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
		/// <param name="name">方法名</param>
		/// <param name="parameters">方法参数</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Object Invoke(this Object target, String name, params Object[] parameters)
		{
			ValidationHelper.ArgumentNull(target, "target");
			ValidationHelper.ArgumentNullOrEmpty(name, "name");

			Object value = null;
			if (TryInvoke(target, name, out value, parameters)) { return value; }

			var type = GetType(ref target);
			throw new HmExceptionBase("类{0}中找不到名为{1}的方法！", type, name);
		}

		/// <summary>反射调用指定对象的方法</summary>
		/// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
		/// <param name="name">方法名</param>
		/// <param name="value">数值</param>
		/// <param name="parameters">方法参数</param>
		/// <remarks>反射调用是否成功</remarks>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Boolean TryInvoke(this Object target, String name, out Object value, params Object[] parameters)
		{
			value = null;

			if (name.IsNullOrWhiteSpace()) { return false; }

			var type = GetType(ref target);

			// 参数类型数组
			var list = new List<Type>();
			foreach (var item in parameters)
			{
				Type t = null;
				if (item != null) { t = item.GetType(); }
				list.Add(t);
			}

			// 如果参数数组出现null，则无法精确匹配，可按参数个数进行匹配
			var method = GetMethodEx(type, name, list.ToArray());
			if (method == null) { return false; }

			value = Invoke(target, method, parameters);
			return true;
		}

		/// <summary>反射调用指定对象的方法</summary>
		/// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
		/// <param name="method">方法</param>
		/// <param name="parameters">方法参数</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		[DebuggerHidden]
		public static Object Invoke(this Object target, MethodBase method, params Object[] parameters)
		{
			//ValidationHelper.ArgumentNull(target, "target");
			//ValidationHelper.ArgumentNull(method, "method");
			ValidationHelper.ArgumentNull(method, "method");
			if (!method.IsStatic)
			{
				ValidationHelper.ArgumentNull(target, "target");
			}

			return _Provider.Invoke(target, method, parameters);
		}

		/// <summary>反射调用指定对象的方法</summary>
		/// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
		/// <param name="method">方法</param>
		/// <param name="parameters">方法参数字典</param>
		/// <returns></returns>
		[DebuggerHidden]
		public static Object InvokeWithParams(this Object target, MethodBase method, IDictionary parameters)
		{
			//if (target == null) throw new ArgumentNullException("target");
			if (method == null) throw new ArgumentNullException("method");
			if (!method.IsStatic && target == null) throw new ArgumentNullException("target");

			return _Provider.InvokeWithParams(target, method, parameters);
		}

		/// <summary>获取目标对象指定名称的属性/字段值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="name">名称</param>
		/// <param name="throwOnError">出错时是否抛出异常</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		[DebuggerHidden]
		public static Object GetValue(this Object target, String name, Boolean throwOnError = true)
		{
			ValidationHelper.ArgumentNull(target, "target");
			ValidationHelper.ArgumentNullOrEmpty(name, "name");

			Object value = null;
			if (TryGetValue(target, name, out value)) { return value; }

			if (!throwOnError) { return null; }

			var type = GetType(ref target);
			throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
		}

		/// <summary>获取目标对象指定名称的属性/字段值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		/// <returns>是否成功获取数值</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Boolean TryGetValue(this Object target, String name, out Object value)
		{
			value = null;

			if (name.IsNullOrWhiteSpace()) { return false; }

			var type = GetType(ref target);
			//var pi = GetPropertyEx(type, name);
			//if (pi != null)
			//{
			//	value = target.GetValue(pi);
			//	return true;
			//}

			//var fi = GetFieldEx(type, name);
			//if (fi != null)
			//{
			//	value = target.GetValue(fi);
			//	return true;
			//}

			var mi = type.GetMemberEx(name, true);
			if (mi == null) { return false; }

			value = target.GetValue(mi);

			return false;
		}

		/// <summary>获取目标对象的属性值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="property">属性</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Object GetValue(this Object target, PropertyInfo property)
		{
			return _Provider.GetValue(target, property);
		}

		/// <summary>获取目标对象的字段值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="field">字段</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Object GetValue(this Object target, FieldInfo field)
		{
			return _Provider.GetValue(target, field);
		}

		/// <summary>获取目标对象的成员值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="member">成员</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Object GetValue(this Object target, MemberInfo member)
		{
			#region ## 苦竹 修改 2013.10.30 PM13:36 ##
			//if (member is PropertyInfo)
			//{
			//	return target.GetValue(member as PropertyInfo);
			//}
			//else if (member is FieldInfo)
			//{
			//	return target.GetValue(member as FieldInfo);
			//}
			//else
			//{
			//	throw new ArgumentOutOfRangeException("member");
			//}
			var property = member as PropertyInfo;
			if (property != null) { return target.GetValue(property); }
			var field = member as FieldInfo;
			if (field != null) { return target.GetValue(field); }
			throw new ArgumentOutOfRangeException("member");
			#endregion
		}

		/// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
		/// <param name="target">目标对象</param>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		/// <remarks>反射调用是否成功</remarks>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		[DebuggerHidden]
		public static Boolean SetValue(this Object target, String name, Object value)
		{
			if (name.IsNullOrWhiteSpace()) { return false; }

			var type = GetType(ref target);
			//var pi = GetPropertyEx(type, name);
			//if (pi != null) { target.SetValue(pi, value); return true; }

			//var fi = GetFieldEx(type, name);
			//if (fi != null) { target.SetValue(fi, value); return true; }
			var mi = type.GetMemberEx(name, true);
			if (mi == null) { return false; }

			target.SetValue(mi, value);

			//throw new ArgumentException("类[" + type.FullName + "]中不存在[" + name + "]属性或字段。");
			return true;
		}

		/// <summary>设置目标对象的属性值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="property">属性</param>
		/// <param name="value">数值</param>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void SetValue(this Object target, PropertyInfo property, Object value)
		{
			_Provider.SetValue(target, property, value);
		}

		/// <summary>设置目标对象的字段值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void SetValue(this Object target, FieldInfo field, Object value)
		{
			_Provider.SetValue(target, field, value);
		}

		/// <summary>设置目标对象的成员值</summary>
		/// <param name="target">目标对象</param>
		/// <param name="member">成员</param>
		/// <param name="value">数值</param>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		[DebuggerHidden]
		public static void SetValue(this Object target, MemberInfo member, Object value)
		{
			#region ## 苦竹 修改 2013.10.30 PM13:36 ##
			//if (member is PropertyInfo)
			//	_Current.SetValue(target, member as PropertyInfo, value);
			//else if (member is FieldInfo)
			//	_Current.SetValue(target, member as FieldInfo, value);
			//else
			//	throw new ArgumentOutOfRangeException("member");
			var property = member as PropertyInfo;
			if (property != null) { _Provider.SetValue(target, property, value); return; }
			var field = member as FieldInfo;
			if (field != null) { _Provider.SetValue(target, field, value); return; }
			throw new ArgumentOutOfRangeException("member");
			#endregion
		}

		#endregion

		#region -- 类型辅助 --

		#region - MemberInfo -

#if NET_3_5_GREATER
		/// <summary>获取包含该成员的自定义特性的集合。</summary>
		/// <param name="member"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<CustomAttributeData> CustomAttributesEx(this MemberInfo member)
		{
#if NET_4_0_GREATER
			return member.CustomAttributes;
#else
			return member.GetCustomAttributesData(); ;
#endif
		}
#endif

		/// <summary>获取成员绑定的显示名，优先DisplayName，然后Description</summary>
		/// <param name="member"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
		public static String GetDisplayName(this MemberInfo member, Boolean inherit = true)
		{
			var att = member.GetCustomAttributeX<DisplayNameAttribute>(inherit);
			if (att != null && !att.DisplayName.IsNullOrWhiteSpace()) { return att.DisplayName; }

			return null;
		}

		/// <summary>获取成员绑定的显示名，优先DisplayName，然后Description</summary>
		/// <param name="member"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
		public static String GetDescription(this MemberInfo member, Boolean inherit = true)
		{
			var att2 = member.GetCustomAttributeX<DescriptionAttribute>(inherit);
			if (att2 != null && !att2.Description.IsNullOrWhiteSpace()) { return att2.Description; }

			return null;
		}

		#endregion

		#region - MethodInfo -

#if !NET_4_0_GREATER
		/// <summary>创建指定类型的委托从此方法的</summary>
		/// <param name="method">MethodInfo</param>
		/// <param name="delegateType">创建委托的类型</param>
		/// <returns></returns>
		public static Delegate CreateDelegate(this MethodInfo method, Type delegateType)
		{
			return Delegate.CreateDelegate(delegateType, method);
		}

		/// <summary>使用从此方法的指定目标创建指定类型的委托</summary>
		/// <param name="method">MethodInfo</param>
		/// <param name="delegateType">创建委托的类型</param>
		/// <param name="target">委托面向的对象</param>
		/// <returns></returns>
		public static Delegate CreateDelegate(this MethodInfo method, Type delegateType, Object target)
		{
			return Delegate.CreateDelegate(delegateType, target, method);
		}
#endif

		#endregion

		#region - CustomAttributeData -

		/// <summary>获取属性的类型。</summary>
		/// <param name="attrdata"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Type AttributeTypeEx(this CustomAttributeData attrdata)
		{
#if NET_4_0_GREATER
			return attrdata.AttributeType;
#else
			return attrdata.Constructor.DeclaringType; ;
#endif
		}

		#endregion

		#region - PropertyInfo -

		/// <summary>获取此属性的 get 访问器。</summary>
		/// <param name="property"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static MethodInfo GetMethodEx(this PropertyInfo property)
		{
#if NET_4_0_GREATER
			return property.GetMethod;
#else
			return property.GetGetMethod(true);
#endif
		}

		/// <summary>获取此属性的 set 访问器。</summary>
		/// <param name="property"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static MethodInfo SetMethodEx(this PropertyInfo property)
		{
#if NET_4_0_GREATER
			return property.SetMethod;
#else
			return property.GetSetMethod(true); ;
#endif
		}

		#endregion

		#region - Type / TypeInfo -

		/// <summary>获取此类型通用类型参数的数组。</summary>
		/// <param name="type"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Type[] GenericTypeArgumentsEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GenericTypeArguments;
#else
			return type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericArguments() : Type.EmptyTypes;
#endif
		}

		/// <summary>获取当前类型的泛型参数的数组。</summary>
		/// <param name="type"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Type[] GenericTypeParametersEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().GenericTypeParameters;
#else
			return type.IsGenericTypeDefinition ? type.GetGenericArguments() : Type.EmptyTypes;
#endif
		}

		/// <summary>获取当前类型实现的接口的集合。</summary>
		/// <param name="type"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<Type> ImplementedInterfacesEx(this Type type)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().ImplementedInterfaces;
#else
			return type.GetInterfaces();
#endif
		}

		/// <summary>确定 Type 的实例是否可以从指定 Type 的实例分配。</summary>
		/// <param name="type"></param>
		/// <param name="c"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Boolean IsAssignableFromEx(this Type type, Type c)
		{
#if NET_4_0_GREATER
			return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
#else
			return type.IsAssignableFrom(c);
#endif
		}

		/// <summary>获取一个类型的元素类型</summary>
		/// <param name="type">类型</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Type GetElementTypeEx(this Type type)
		{
			return _Provider.GetElementType(type);
		}

		#endregion

		/// <summary>类型转换</summary>
		/// <param name="value">数值</param>
		/// <param name="conversionType"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Object ChangeType(this Object value, Type conversionType)
		{
			return _Provider.ChangeType(value, conversionType);
		}

		/// <summary>类型转换</summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="value">数值</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TResult ChangeType<TResult>(this Object value)
		{
			if (value is TResult) { return (TResult)value; }

			return (TResult)ChangeType(value, typeof(TResult));
		}

		/// <summary>获取类型的友好名称</summary>
		/// <param name="type">指定类型</param>
		/// <param name="isfull">是否全名，包含命名空间</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String GetName(this Type type, Boolean isfull = false)
		{
			return _Provider.GetName(type, isfull);
		}

		/// <summary>从参数数组中获取类型数组</summary>
		/// <param name="args"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Type[] GetTypeArray(this Object[] args)
		{
			if (args == null) { return Type.EmptyTypes; }

			var typeArray = new Type[args.Length];
			for (int i = 0; i < typeArray.Length; i++)
			{
				if (args[i] == null)
				{
					typeArray[i] = typeof(Object);
				}
				else
				{
					typeArray[i] = args[i].GetType();
				}
			}
			return typeArray;
		}

		/// <summary>获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身</summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static Type GetMemberType(this MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Constructor:
					return (member as ConstructorInfo).DeclaringType;
				case MemberTypes.Field:
					return (member as FieldInfo).FieldType;
				case MemberTypes.Method:
					return (member as MethodInfo).ReturnType;
				case MemberTypes.Property:
					return (member as PropertyInfo).PropertyType;
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					return member as Type;
				default:
					return null;
			}
		}

		#endregion

		#region -- 插件 --

		///// <summary>是否插件</summary>
		///// <param name="type">目标类型</param>
		///// <param name="baseType">基类或接口</param>
		///// <returns></returns>
		//public static Boolean IsSubclassOfEx(this Type type, Type baseType) { return _Provider.IsSubclassOf(type, baseType); }

		/// <summary>在指定程序集中查找指定基类的子类</summary>
		/// <param name="asm">指定程序集</param>
		/// <param name="baseType">基类或接口</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<Type> GetSubclasses(this Assembly asm, Type baseType)
		{
			return _Provider.GetSubclasses(asm, baseType);
		}

		/// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
		/// <param name="baseType">基类或接口</param>
		/// <param name="isLoadAssembly">是否加载为加载程序集</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<Type> GetAllSubclasses(this Type baseType, Boolean isLoadAssembly = false)
		{
			return _Provider.GetAllSubclasses(baseType, isLoadAssembly);
		}

		#endregion

		#region -- 辅助方法 --

		/// <summary>获取类型，如果target是Type类型，则表示要反射的是静态成员</summary>
		/// <param name="target">目标对象</param>
		/// <returns></returns>
		static Type GetType(ref Object target)
		{
			if (target == null) { throw new ArgumentNullException("target"); }

			var type = target as Type;
			if (type == null)
			{
				type = target.GetType();
			}
			else
			{
				target = null;
			}

			return type;
		}

		/// <summary>判断某个类型是否可空类型</summary>
		/// <param name="type">类型</param>
		/// <returns></returns>
		static Boolean IsNullable(Type type)
		{
			//if (type.IsValueType) return false;

			if (type.IsGenericType && !type.IsGenericTypeDefinition &&
					Object.ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>))) { return true; }

			return false;
		}

		/// <summary>把一个方法转为泛型委托，便于快速反射调用</summary>
		/// <typeparam name="TFunc"></typeparam>
		/// <param name="method"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static TFunc As<TFunc>(this MethodInfo method, Object target = null)
		{
			if (target == null)
			{
#if NET_4_0_GREATER
				return (TFunc)(Object)method.CreateDelegate(typeof(TFunc));
#else
				return (TFunc)(Object)Delegate.CreateDelegate(typeof(TFunc), method);
#endif
			}
			else
			{
#if NET_4_0_GREATER
				return (TFunc)(Object)method.CreateDelegate(typeof(TFunc), target);
#else
				return (TFunc)(Object)Delegate.CreateDelegate(typeof(TFunc), target, method);
#endif
			}
		}

		#endregion
	}
}