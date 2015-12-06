#region Copyright © 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

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
using CuteAnt.Serialization.Advanced;
using CuteAnt.Serialization.Advanced.Serializing;
using CuteAnt.Serialization.Advanced.Xml;

namespace CuteAnt.Serialization.Core
{
	#region -- class SharpSerializerSettings<T> --

	/// <summary>Base class for the settings of the SharpSerializer. Is passed to its constructor.</summary>
	/// <typeparam name = "T"></typeparam>
	public abstract class SharpSerializerSettings<T> where T : AdvancedSharpSerializerSettings, new()
	{
		#region - 属性 -

		#region ## 苦竹 屏蔽 ##
		private Boolean _IncludeAssemblyVersionInTypeName = false;

		/// <summary>Version=x.x.x.x will be inserted to the type name</summary>
		public Boolean IncludeAssemblyVersionInTypeName
		{
			get { return _IncludeAssemblyVersionInTypeName; }
			set { _IncludeAssemblyVersionInTypeName = value; }
		}

		private Boolean _IncludeCultureInTypeName = false;

		/// <summary>Culture=.... will be inserted to the type name</summary>
		public Boolean IncludeCultureInTypeName
		{
			get { return _IncludeCultureInTypeName; }
			set { _IncludeCultureInTypeName = value; }
		}

		private Boolean _IncludePublicKeyTokenInTypeName = false;

		/// <summary>PublicKeyToken=.... will be inserted to the type name</summary>
		public Boolean IncludePublicKeyTokenInTypeName
		{
			get { return _IncludePublicKeyTokenInTypeName; }
			set { _IncludePublicKeyTokenInTypeName = value; }
		}
		#endregion

		private T _advancedSettings;

		/// <summary>Contains mostly classes from the namespace CuteAnt.Serialization.Advanced</summary>
		public T AdvancedSettings
		{
			get
			{
				if (_advancedSettings == default(T))
				{
					_advancedSettings = new T();
				}
				return _advancedSettings;
			}
			set { _advancedSettings = value; }
		}

		#endregion

		/// <summary>默认构造函数</summary>
		protected SharpSerializerSettings()
		{
		}
	}

	#endregion

	#region -- class AdvancedSharpSerializerXmlSettings --

	/// <summary>Base class for the advanced settings. Is common for the binary and xml serialization.</summary>
	/// <remarks></remarks>
	public sealed class AdvancedSharpSerializerXmlSettings : AdvancedSharpSerializerSettings
	{
		/// <summary>
		///   Converts simple values to String and vice versa. Default it is an instance of SimpleValueConverter with CultureInfo.InvariantCulture.
		///   You can override the default converter to implement your own converting to/from String.
		/// </summary>
		public ISimpleValueConverter SimpleValueConverter { get; set; }
	}

	#endregion

	#region -- class AdvancedSharpSerializerBinarySettings --

	/// <summary></summary>
	/// <remarks></remarks>
	public sealed class AdvancedSharpSerializerBinarySettings : AdvancedSharpSerializerSettings
	{
	}

	#endregion

	#region -- class AdvancedSharpSerializerSettings --

	public class AdvancedSharpSerializerSettings
	{
		#region - 属性 -

		private PropertiesToIgnore _PropertiesToIgnore;

		/// <summary>Which properties should be ignored during the serialization.</summary>
		/// <remarks>
		///   In your business objects you can mark these properties with ExcludeFromSerializationAttribute
		///   In built in .NET Framework classes you can not do this. Therefore you define these properties here.
		///   I.e. System.Collections.Generic.List has property Capacity which is irrelevant for
		///   the whole Serialization and should be ignored.
		/// </remarks>
		public PropertiesToIgnore PropertiesToIgnore
		{
			get
			{
				if (_PropertiesToIgnore == null)
				{
					_PropertiesToIgnore = new PropertiesToIgnore();
				}
				return _PropertiesToIgnore;
			}
			set { _PropertiesToIgnore = value; }
		}

		private IList<Type> _AttributesToIgnore;

		/// <summary>
		/// All Properties marked with one of the contained attribute-types will be ignored on save.
		/// As default, this list contains only ExcludeFromSerializationAttribute.
		/// For performance reasons it would be better to clear this list if this attribute
		/// is not used in serialized classes.
		/// </summary>
		public IList<Type> AttributesToIgnore
		{
			get
			{
				if (_AttributesToIgnore == null)
				{
					_AttributesToIgnore = new List<Type>();
				}
				return _AttributesToIgnore;
			}
			set { _AttributesToIgnore = value; }
		}

		private String _RootName = "Root";

		/// <summary>What name has the root item of your serialization. Default is "Root".</summary>
		public String RootName
		{
			get { return _RootName; }
			set { _RootName = value; }
		}

		private ITypeNameConverter _TypeNameConverter = null;

		/// <summary>
		///   Converts Type to String and vice versa. Default is an instance of TypeNameConverter which serializes Types as "type name, assembly name"
		///   If you want to serialize your objects as fully qualified assembly name, you should set this setting with an instance of TypeNameConverter
		///   with overloaded constructor.
		/// </summary>
		public ITypeNameConverter TypeNameConverter
		{
			get { return _TypeNameConverter; }
			set { _TypeNameConverter = value; }
		}

		#endregion

		/// <summary>默认构造函数</summary>
		public AdvancedSharpSerializerSettings()
		{
			AttributesToIgnore.Add(typeof(ExcludeFromSerializationAttribute));
		}
	}

	#endregion
}