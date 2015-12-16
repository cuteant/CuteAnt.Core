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
using System.IO;

namespace CuteAnt.Serialization.Core.Binary
{
	/// <summary>Some methods which are used by IBinaryWriter</summary>
	public static class BinaryWriterTools
	{
		///<summary>
		///</summary>
		///<param name = "number"></param>
		///<param name = "writer"></param>
		public static void WriteNumber(Int32 number, BinaryWriter writer)
		{
			// Write size
			Byte size = NumberSize.GetNumberSize(number);
			writer.Write(size);

			// Write number
			if (size > NumberSize.Zero)
			{
				switch (size)
				{
					case NumberSize.B1:
						writer.Write((Byte)number);
						break;

					case NumberSize.B2:
						writer.Write((Int16)number);
						break;

					default:
						writer.Write(number);
						break;
				}
			}
		}

		///<summary>
		///</summary>
		///<param name = "numbers"></param>
		///<param name = "writer"></param>
		public static void WriteNumbers(Int32[] numbers, BinaryWriter writer)
		{
			// Length
			WriteNumber(numbers.Length, writer);

			// Numbers
			foreach (Int32 number in numbers)
			{
				WriteNumber(number, writer);
			}
		}

		///<summary>
		///</summary>
		///<param name = "value"></param>
		///<param name = "writer"></param>
		public static void WriteValue(object value, BinaryWriter writer)
		{
			if (value == null)
			{
				writer.Write(false);
			}
			else
			{
				writer.Write(true);
				writeValueCore(value, writer);
			}
		}

		/// <summary>BinaryWriter.Write(String...) can not be used as it produces exception if the text is null.</summary>
		/// <param name = "text"></param>
		/// <param name = "writer"></param>
		public static void WriteString(String text, BinaryWriter writer)
		{
			if (text.IsNullOrWhiteSpace())
			{
				// no exception if the text is null
				writer.Write(false);
			}
			else
			{
				writer.Write(true);
				writer.Write(text);
			}
		}

		private static void writeValueCore(object value, BinaryWriter writer)
		{
			if (value == null) throw new ArgumentNullException("value", "Written data can not be null.");

			// Write argument data
			Type type = value.GetType();
			if (type == typeof(Byte[]))
			{
				writeArrayOfByte((Byte[])value, writer);
				return;
			}
			if (type == typeof(String))
			{
				writer.Write((String)value);
				return;
			}
			if (type == typeof(Boolean))
			{
				writer.Write((Boolean)value);
				return;
			}
			if (type == typeof(Byte))
			{
				writer.Write((Byte)value);
				return;
			}
			if (type == typeof(Char))
			{
				writer.Write((Char)value);
				return;
			}
			if (type == typeof(DateTime))
			{
				writer.Write(((DateTime)value).Ticks);
				return;
			}
			if (type == typeof(Guid))
			{
				writer.Write(((Guid)value).ToByteArray());
				return;
			}
#if DEBUG || PORTABLE || SILVERLIGHT
			if (type == typeof(Decimal))
			{
				writeDecimal((Decimal)value, writer);
				return;
			}
#else
			if (type == typeof(Decimal))
			{
				writer.Write((Decimal)value);
				return;
			}
#endif
			if (type == typeof(Double))
			{
				writer.Write((Double)value);
				return;
			}
			if (type == typeof(Int16))
			{
				writer.Write((Int16)value);
				return;
			}
			if (type == typeof(Int32))
			{
				writer.Write((Int32)value);
				return;
			}
			if (type == typeof(Int64))
			{
				writer.Write((Int64)value);
				return;
			}
			if (type == typeof(SByte))
			{
				writer.Write((SByte)value);
				return;
			}
			if (type == typeof(Single))
			{
				writer.Write((Single)value);
				return;
			}
			if (type == typeof(UInt16))
			{
				writer.Write((UInt16)value);
				return;
			}
			if (type == typeof(UInt32))
			{
				writer.Write((UInt32)value);
				return;
			}
			if (type == typeof(UInt64))
			{
				writer.Write((UInt64)value);
				return;
			}
			if (type == typeof(TimeSpan))
			{
				writer.Write(((TimeSpan)value).Ticks);
				return;
			}
			if (type == typeof(TimeSpan))
			{
				writer.Write(((TimeSpan)value).Ticks);
				return;
			}
			if (type == typeof(Color))
			{
				writer.Write(value.ToString());
				return;
			}
			if (type == typeof(Font))
			{
				var typeConv = TypeDescriptor.GetConverter(typeof(Font));
				writer.Write(typeConv.ConvertToInvariantString(value));
				return;
			}

			// Enumeration
			if (type.IsEnum)
			{
				writer.Write(Convert.ToInt32(value));
				return;
			}

			// Type
			if (isType(type))
			{
				writer.Write(((Type)value).AssemblyQualifiedName);
				return;
			}
			throw new InvalidOperationException(String.Format("Unknown simple type: {0}", type.FullName));
		}

		private static void writeDecimal(Decimal value, BinaryWriter writer)
		{
			var bits = Decimal.GetBits(value);
			writer.Write(bits[0]);
			writer.Write(bits[1]);
			writer.Write(bits[2]);
			writer.Write(bits[3]);
		}

		private static Boolean isType(Type type)
		{
			return type == typeof(Type) || type.IsSubclassOf(typeof(Type));
		}

		private static void writeArrayOfByte(Byte[] data, BinaryWriter writer)
		{
			WriteNumber(data.Length, writer);
			writer.Write(data);
		}
	}
}