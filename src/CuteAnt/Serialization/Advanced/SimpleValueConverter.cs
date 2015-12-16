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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using CuteAnt.Serialization.Advanced.Serializing;
using CuteAnt.Serialization.Advanced.Xml;
using CuteAnt.Serialization.Core;

namespace CuteAnt.Serialization.Advanced
{
	/// <summary>Converts simple types to/from their text representation</summary>
	/// <remarks>
	///   It is important to use the same ISimpleValueConverter during serialization and deserialization
	///   Especially Format of the DateTime and Single types can be differently converted in different cultures.
	///   To customize it, please use the Constructor with the specified CultureInfo,
	///   or inherit your own converter from ISimpleValueConverter
	/// </remarks>
	public sealed class SimpleValueConverter : ISimpleValueConverter
	{
		private readonly CultureInfo _cultureInfo;
		private readonly ITypeNameConverter _typeNameConverter;
		private const Char NullChar = (Char)0;
		private const String NullCharAsString = "&#x0;";

		/// <summary>Default is CultureInfo.InvariantCulture used</summary>
		public SimpleValueConverter()
		{
			_cultureInfo = CultureInfo.InvariantCulture;
			_typeNameConverter = new TypeNameConverter();

			// Alternatively
			//_cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
		}

		/// <summary>Here you can customize the culture. I.e. System.Threading.Thread.CurrentThread.CurrentCulture</summary>
		/// <param name = "cultureInfo"></param>
		/// <param name="typeNameConverter"></param>
		public SimpleValueConverter(CultureInfo cultureInfo, ITypeNameConverter typeNameConverter)
		{
			_cultureInfo = cultureInfo;
			_typeNameConverter = typeNameConverter;
		}

		#region ISimpleValueConverter Members

		/// <summary>
		/// </summary>
		/// <param name = "value"></param>
		/// <returns>String.Empty if the value is null</returns>
		public String ConvertToString(object value)
		{
			if (value == null) { return String.Empty; }

			// Array of Byte
			if (value.GetType() == typeof(Byte[]))
			{
				return Convert.ToBase64String((Byte[])value);
			}

			// Font
			if (value.GetType() == typeof(Font))
			{
				var typeConv = TypeDescriptor.GetConverter(typeof(Font));
				return typeConv.ConvertToInvariantString(value);
			}

			// Type
			if (isType(value))
			{
				return _typeNameConverter.ConvertToTypeName((Type)value);
			}

			// Char which is \0
			if (value.Equals(NullChar))
			{
				return NullCharAsString;
			}
			return Convert.ToString(value, _cultureInfo);
		}

		/// <summary>
		/// </summary>
		/// <param name = "text"></param>
		/// <param name = "type">expected type. Result should be of this type.</param>
		/// <returns>null if the text is null</returns>
		public object ConvertFromString(String text, Type type)
		{
			try
			{
				if (type == typeof(String)) { return text; }
				if (type == typeof(Boolean))
				{
					return Boolean.Parse(text);

					//return Convert.ToBoolean(text, _cultureInfo);
				}
				if (type == typeof(Byte))
				{
					return Byte.Parse(text, _cultureInfo);

					//return Convert.ToByte(text, _cultureInfo);
				}
				if (type == typeof(Char))
				{
					if (text == NullCharAsString)
					{
						// this is a null termination
						return NullChar;
					}

					//other chars
					return Char.Parse(text);

					//return Convert.ToChar(text, _cultureInfo);
				}
				if (type == typeof(DateTime))
				{
					return DateTime.Parse(text, _cultureInfo);

					//return Convert.ToDateTime(text, _cultureInfo);
				}
				if (type == typeof(Decimal))
				{
					return Decimal.Parse(text, _cultureInfo);

					//return Convert.ToDecimal(text, _cultureInfo);
				}
				if (type == typeof(Double))
				{
					return Double.Parse(text, _cultureInfo);

					//return Convert.ToDouble(text, _cultureInfo);
				}
				if (type == typeof(Int16))
				{
					return Int16.Parse(text, _cultureInfo);

					//return Convert.ToInt16(text, _cultureInfo);
				}
				if (type == typeof(Int32))
				{
					return Int32.Parse(text, _cultureInfo);

					//return Convert.ToInt32(text, _cultureInfo);
				}
				if (type == typeof(Int64))
				{
					return Int64.Parse(text, _cultureInfo);

					//return Convert.ToInt64(text, _cultureInfo);
				}
				if (type == typeof(SByte))
				{
					return SByte.Parse(text, _cultureInfo);

					//return Convert.ToSByte(text, _cultureInfo);
				}
				if (type == typeof(Single))
				{
					return Single.Parse(text, _cultureInfo);

					//return Convert.ToSingle(text, _cultureInfo);
				}
				if (type == typeof(UInt16))
				{
					return UInt16.Parse(text, _cultureInfo);

					//return Convert.ToUInt16(text, _cultureInfo);
				}
				if (type == typeof(UInt32))
				{
					return UInt32.Parse(text, _cultureInfo);

					//return Convert.ToUInt32(text, _cultureInfo);
				}
				if (type == typeof(UInt64))
				{
					return UInt64.Parse(text, _cultureInfo);

					//return Convert.ToUInt64(text, _cultureInfo);
				}
				if (type == typeof(TimeSpan))
				{
					return TimeSpan.Parse(text);
				}
				if (type == typeof(Guid))
				{
					return new Guid(text);
				}

				// Enumeration
				if (type.IsEnum) { return Enum.Parse(type, text, true); }

				// Array of Byte
				if (type == typeof(Byte[]))
				{
					return Convert.FromBase64String(text);
				}
				if (type == typeof(Color))
				{
					var color = text.Substring(text.IndexOf('[') + 1).Replace("]", "");
					var argb = color.Split(',', '=');
					if (argb.Length > 1)
					{
						return Color.FromArgb(Int32.Parse(argb[1]), Int32.Parse(argb[3]), Int32.Parse(argb[5]), Int32.Parse(argb[7]));
					}
					else
					{
						if (color.Equals("Empty", StringComparison.InvariantCultureIgnoreCase))
						{
							return Color.Empty;
						}
						else
						{
							return Color.FromName(color);
						}
					}
				}
				if (type == typeof(Font))
				{
					var conv = TypeDescriptor.GetConverter(type);
					return conv.ConvertFromInvariantString(text);
				}

				// Type-check must be last
				if (isType(type))
				{
					return _typeNameConverter.ConvertToType(text);
				}
				throw new InvalidOperationException(String.Format("Unknown simple type: {0}", type.FullName));
			}
			catch (Exception ex)
			{
				throw new SimpleValueParsingException(
						String.Format("Invalid value: {0}. See details in the inner exception.", text), ex);
			}
		}

		#endregion

		private static Boolean isType(object value)
		{
			return (value as Type) != null;
		}
	}
}