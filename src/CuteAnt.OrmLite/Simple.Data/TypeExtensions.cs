using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CuteAnt.OrmLite
{
	internal static class TypeExtensions
	{
		internal static bool IsGenericCollection(this Type type)
		{
			return type.IsGenericType &&
					(type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
						 type.GetGenericTypeDefinition().GetInterfaces()
								 .Where(i => i.IsGenericType)
								 .Select(i => i.GetGenericTypeDefinition())
								 .Contains(typeof(ICollection<>)));
		}

		internal static MethodInfo GetInterfaceMethod(this Type type, string name)
		{
			return type.GetMethod(name)
						 ??
						 type.GetInterfaces()
								 .Select(t => t.GetInterfaceMethod(name))
								 .FirstOrDefault(m => m != null);
		}
	}
}