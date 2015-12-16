﻿#if NET40
using System;
using System.Collections.Generic;
//using System.Diagnostics.Tracing;
using CuteAnt.Reflection;

namespace System.Reflection
{
	internal static class RuntimeReflectionExtensions
	{
		private const BindingFlags everything = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		private const BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

		private static void CheckAndThrow(Type t)
		{
			if (t == null) throw new ArgumentNullException("type");
			//if (!(t is RuntimeType)) throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
		}

		private static void CheckAndThrow(MethodInfo m)
		{
			if (m == null) throw new ArgumentNullException("method");
			//if (!(m is RuntimeMethodInfo)) throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
		}

		internal static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeProperties(type.GetFullNameForEtw());
			//			}
			//#endif
			IEnumerable<PropertyInfo> properties = type.GetProperties(everything);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeProperties(type.GetFullNameForEtw());
			//			}
			//#endif
			return properties;
		}
		internal static IEnumerable<EventInfo> GetRuntimeEvents(this Type type)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeEvents(type.GetFullNameForEtw());
			//			}
			//#endif
			IEnumerable<EventInfo> events = type.GetEvents(everything);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeEvents(type.GetFullNameForEtw());
			//			}
			//#endif
			return events;
		}

		internal static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeMethods(type.GetFullNameForEtw());
			//			}
			//#endif
			IEnumerable<MethodInfo> methods = type.GetMethods(everything);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeMethods(type.GetFullNameForEtw());
			//			}
			//#endif
			return methods;
		}

		internal static IEnumerable<FieldInfo> GetRuntimeFields(this Type type)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeFields(type.GetFullNameForEtw());
			//			}
			//#endif
			IEnumerable<FieldInfo> fields = type.GetFields(everything);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeFields(type.GetFullNameForEtw());
			//			}
			//#endif

			return fields;
		}

		internal static PropertyInfo GetRuntimeProperty(this Type type, string name)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeProperty(type.GetFullNameForEtw(), name != null ? name : "");
			//			}
			//#endif

			PropertyInfo pi = DefaultReflect.GetSafeProperty(type, name, bf);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeProperty(type.GetFullNameForEtw(), pi != null ? pi.GetFullNameForEtw() : "");
			//			}
			//#endif

			return pi;
		}
		internal static EventInfo GetRuntimeEvent(this Type type, string name)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeEvent(type.GetFullNameForEtw(), name != null ? name : "");
			//			}
			//#endif
			EventInfo ei = DefaultReflect.GetSafeEvent(type, name, bf);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeEvent(type.GetFullNameForEtw(), ei != null ? ei.GetFullNameForEtw() : "");
			//			}
			//#endif

			return ei;
		}
		internal static MethodInfo GetRuntimeMethod(this Type type, string name, Type[] parameters)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeMethod(type.GetFullNameForEtw(), name != null ? name : "");
			//			}
			//#endif
			MethodInfo mi = type.GetMethod(name, parameters);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeMethod(type.GetFullNameForEtw(), mi != null ? mi.GetFullNameForEtw() : "");
			//			}
			//#endif

			return mi;
		}
		internal static FieldInfo GetRuntimeField(this Type type, string name)
		{
			CheckAndThrow(type);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.BeginGetRuntimeField(type.GetFullNameForEtw(), name != null ? name : "");
			//			}
			//#endif
			FieldInfo fi = DefaultReflect.GetSafeField(type, name, bf);
			//#if !FEATURE_CORECLR
			//			if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
			//			{
			//				FrameworkEventSource.Log.EndGetRuntimeField(type.GetFullNameForEtw(), fi != null ? fi.GetFullNameForEtw() : "");
			//			}
			//#endif

			return fi;
		}
		internal static MethodInfo GetRuntimeBaseDefinition(this MethodInfo method)
		{
			CheckAndThrow(method);
			return method.GetBaseDefinition();
		}

		//internal static InterfaceMapping GetRuntimeInterfaceMap(this TypeInfo typeInfo, Type interfaceType)
		//{
		//	if (typeInfo == null) throw new ArgumentNullException("typeInfo");
		//	//if (!(typeInfo is RuntimeType)) throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

		//	return typeInfo.GetInterfaceMap(interfaceType);
		//}

		internal static MethodInfo GetMethodInfo(this Delegate del)
		{
			if (del == null) throw new ArgumentNullException("del");

			return del.Method;
		}
	}
}
#endif