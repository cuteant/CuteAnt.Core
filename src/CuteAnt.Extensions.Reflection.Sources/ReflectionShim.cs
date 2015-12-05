using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if (NET45 || NET451 || NET46 || NET461)
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Reflection
{
	internal static class ReflectionShim
	{
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Assembly AssemblyX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsClassX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().IsClass;
#else
			return type.IsClass;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsEnumX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().IsEnum;
#else
			return type.IsEnum;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsInterfaceX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().IsInterface;
#else
			return type.IsInterface;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsGenericTypeX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().IsGenericType;
#else
			return type.IsGenericType;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsGenericTypeDefinitionX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().IsGenericTypeDefinition;
#else
			return type.IsGenericTypeDefinition;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsValueTypeX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().IsValueType;
#else
			return type.IsValueType;
#endif
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Type BaseTypeX(this Type type)
		{
#if (NET45 || NET451 || NET46 || NET461)
			return type.GetTypeInfo().BaseType;
#else
			return type.BaseType;
#endif
		}
	}
}