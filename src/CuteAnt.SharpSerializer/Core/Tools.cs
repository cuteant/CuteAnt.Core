﻿#region Copyright ?2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

//  ***********************************************************************
//  Project: sharpSerializer
//  Web: http://www.sharpserializer.com
//
//  This software is provided 'as-is', without any express or implied warranty.
//  In no event will the author(s) be held liable for any damages arising from
//  the use of this software.
//
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//
//      1. The origin of this software must not be misrepresented; you must not
//        claim that you wrote the original software. If you use this software
//        in a product, an acknowledgment in the product documentation would be
//        appreciated but is not required.
//
//      2. Altered source versions must be plainly marked as such, and must not
//        be misrepresented as being the original software.
//
//      3. This notice may not be removed or altered from any source distribution.
//
//  ***********************************************************************

#endregion

using System;
using System.Collections;
using System.Drawing;

namespace CuteAnt.Serialization.Core
{
	/// <summary>
	///   Some help functions for the serializing framework. As these functions are complexer
	///   they can be converted to single classes.
	/// </summary>
	internal static class Tools
	{
		/// <summary>Is the simple type (String, DateTime, TimeSpan, Decimal, Enumeration or other primitive type)</summary>
		/// <param name = "type"></param>
		/// <returns></returns>
		public static Boolean IsSimple(Type type)
		{
			if (type == typeof(String))
			{
				return true;
			}
			if (type == typeof(DateTime))
			{
				return true;
			}
			if (type == typeof(TimeSpan))
			{
				return true;
			}
			if (type == typeof(Decimal))
			{
				// new since the version 2
				return true;
			}
			if (type == typeof(Guid))
			{
				// new since the version 2.8
				return true;
			}
			if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
			{
				// new since v.2.11
				return true;
			}
			if (type.IsEnum)
			{
				return true;
			}
			if (type == typeof(Byte[]))
			{
				// since v.2.16 is Byte[] a simple type
				return true;
			}
			if (type == typeof(Color))
			{
				// ## 苦竹 2012.10.12 ##
				return true;
			}
			if (type == typeof(Font))
			{
				// ## 苦竹 2012.10.12 ##
				return true;
			}
			return type.IsPrimitive;
		}

		/// <summary>Is type an IEnumerable</summary>
		/// <param name = "type"></param>
		/// <returns></returns>
		public static Boolean IsEnumerable(Type type)
		{
			Type referenceType = typeof(IEnumerable);
			return referenceType.IsAssignableFrom(type);
		}

		/// <summary>Is type ICollection</summary>
		/// <param name = "type"></param>
		/// <returns></returns>
		public static Boolean IsCollection(Type type)
		{
			Type referenceType = typeof(ICollection);
			return referenceType.IsAssignableFrom(type);
		}

		/// <summary>Is type IDictionary</summary>
		/// <param name = "type"></param>
		/// <returns></returns>
		public static Boolean IsDictionary(Type type)
		{
			Type referenceType = typeof(IDictionary);
			return referenceType.IsAssignableFrom(type);
		}

		/// <summary>Is it array? It does not matter if singledimensional or multidimensional</summary>
		/// <param name = "type"></param>
		/// <returns></returns>
		public static Boolean IsArray(Type type)
		{
			return type.IsArray;
		}

		/// <summary>Creates instance from type. There must be a standard constructor (without parameters) in the type.</summary>
		/// <param name = "type"></param>
		/// <returns></returns>
		public static object CreateInstance(Type type)
		{
			if (type == null) { return null; }

			try
			{
				object result = Activator.CreateInstance(type);
				return result;
			}
			catch (Exception ex)
			{
				throw new CreatingInstanceException(
						String.Format(
								"Error during creating an object. Please check if the type \"{0}\" has public parameterless constructor, or if the settings IncludeAssemblyVersionInTypeName, IncludeCultureInTypeName, IncludePublicKeyTokenInTypeName are set to true. Details are in the inner exception.",
								type.AssemblyQualifiedName), ex);
			}
		}
	}
}