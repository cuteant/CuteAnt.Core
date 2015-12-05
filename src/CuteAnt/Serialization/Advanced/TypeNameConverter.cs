#region Copyright ?2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

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
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CuteAnt.IO;
using CuteAnt.Reflection;
using CuteAnt.Serialization.Advanced.Serializing;

namespace CuteAnt.Serialization.Advanced
{
	/// <summary>
	///   Converts Type to its text representation and vice versa. Since v.2.12 all types serialize to the AssemblyQualifiedName.
	///   Use overloaded constructor to shorten type names.
	/// </summary>
	public sealed class TypeNameConverter : ITypeNameConverter
	{
		private readonly Dictionary<Type, String> _cache = new Dictionary<Type, String>();

		/// <summary>Since v.2.12 as default the type name is equal to Type.AssemblyQualifiedName</summary>
		public TypeNameConverter()
		{
		}

		/// <summary>Some values from the Type.AssemblyQualifiedName can be removed</summary>
		/// <param name = "includeAssemblyVersion"></param>
		/// <param name = "includeCulture"></param>
		/// <param name = "includePublicKeyToken"></param>
		public TypeNameConverter(Boolean includeAssemblyVersion, Boolean includeCulture, Boolean includePublicKeyToken)
		{
			IncludeAssemblyVersion = includeAssemblyVersion;
			IncludeCulture = includeCulture;
			IncludePublicKeyToken = includePublicKeyToken;
		}

		/// <summary>Version=x.x.x.x will be inserted to the type name</summary>
		public Boolean IncludeAssemblyVersion { get; private set; }

		/// <summary>Culture=.... will be inserted to the type name</summary>
		public Boolean IncludeCulture { get; private set; }

		/// <summary>PublicKeyToken=.... will be inserted to the type name</summary>
		public Boolean IncludePublicKeyToken { get; private set; }

		#region ITypeNameConverter Members

		/// <summary>Gives type as text</summary>
		/// <param name = "type"></param>
		/// <returns>String.Empty if the type is null</returns>
		public String ConvertToTypeName(Type type)
		{
			if (type == null) { return String.Empty; }

			// Search in cache
			if (_cache.ContainsKey(type)) { return _cache[type]; }
			String typename = type.AssemblyQualifiedName;
			if (!IncludeAssemblyVersion)
			{
				typename = removeAssemblyVersion(typename);
			}
			if (!IncludeCulture)
			{
				typename = removeCulture(typename);
			}
			if (!IncludePublicKeyToken)
			{
				typename = removePublicKeyToken(typename);
			}
			//if (typename.StartsWith("CuteAnt", StringComparison.InvariantCultureIgnoreCase))
			//{
			//	typename = typename.Replace(AssemblyInfo.VSuffix, String.Empty);
			//}

			// Adding to cache
			_cache.Add(type, typename);
			return typename;
		}

		/// <summary>Gives back Type from the text.</summary>
		/// <param name = "typeName"></param>
		/// <returns></returns>
		public Type ConvertToType(String typeName)
		{
			if (typeName.IsNullOrWhiteSpace()) { return null; }
			Type type = null;
			//if (typeName.StartsWith("CuteAnt", StringComparison.InvariantCultureIgnoreCase))
			//{
			//	var ts = typeName.Split(',');
			//	ts[1] += AssemblyInfo.VSuffix;
			//	if (typeName.Contains("Version="))
			//	{
			//		//// 这里是按标准Type.AssemblyQualifiedName属性值的标准顺序取Version索引的，
			//		//// System.Windows.Forms.Label, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
			//		//if (typeName.StartsWith("CuteAnt.Components", StringComparison.InvariantCultureIgnoreCase))
			//		//{
			//		//	ts[2] = " Version=" + AssemblyInfo.StaticVersion;
			//		//}
			//		//else
			//		//{
			//		//	var file = PathHelper.ApplicationStartupPathCombine(ts[1].Trim() + ".dll");
			//		//	var asm = Assembly.LoadFrom(file);
			//		//	var asmx = AssemblyX.Create(asm);
			//		//	ts[2] = " Version=" + asmx.Version;
			//		//}
			//		//typeName = String.Join(",", ts);

			//		// 需要把PublicKeyToken也替换
			//		var file = PathHelper.ApplicationStartupPathCombine(ts[1].Trim() + ".dll");
			//		var asm = Assembly.LoadFrom(file);
			//		typeName = "{0}, {1}".FormatWith(ts[0], asm.FullName);
			//	}
			//	type = Type.GetType(typeName, true);
			//}
			//else
			//{
				type = Type.GetType(typeName, true);
			//}
			return type;
		}

		#endregion

		private static String removePublicKeyToken(String typename)
		{
			return Regex.Replace(typename, @", PublicKeyToken=\w+", String.Empty);
		}

		private static String removeCulture(String typename)
		{
			return Regex.Replace(typename, @", Culture=\w+", String.Empty);
		}

		private static String removeAssemblyVersion(String typename)
		{
			return Regex.Replace(typename, @", Version=\d+.\d+.\d+.\d+", String.Empty);
		}
	}
}