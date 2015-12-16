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

namespace CuteAnt.Serialization.Core.Binary
{
	/// <summary>These elements are used during the binary serialization. They should be unique from SubElements and Attributes.</summary>
	public static class Elements
	{
		///<summary>
		///</summary>
		public const Byte Collection = 1;

		///<summary>
		///</summary>
		public const Byte ComplexObject = 2;

		///<summary>
		///</summary>
		public const Byte Dictionary = 3;

		///<summary>
		///</summary>
		public const Byte MultiArray = 4;

		///<summary>
		///</summary>
		public const Byte Null = 5;

		///<summary>
		///</summary>
		public const Byte SimpleObject = 6;

		///<summary>
		///</summary>
		public const Byte SingleArray = 7;

		///<summary>
		/// For binary compatibility reason extra type-id: same as ComplexObjectWith, but contains
		///</summary>
		public const Byte ComplexObjectWithId = 8;

		///<summary>
		/// reference to previosly serialized  ComplexObjectWithId
		///</summary>
		public const Byte Reference = 9;

		///<summary>
		///</summary>
		public const Byte CollectionWithId = 10;

		///<summary>
		///</summary>
		public const Byte DictionaryWithId = 11;

		///<summary>
		///</summary>
		public const Byte SingleArrayWithId = 12;

		///<summary>
		///</summary>
		public const Byte MultiArrayWithId = 13;

		///<summary>
		///</summary>
		///<param name="elementId"></param>
		///<returns></returns>
		public static Boolean IsElementWithId(Byte elementId)
		{
			if (elementId == ComplexObjectWithId) { return true; }
			if (elementId == CollectionWithId) { return true; }
			if (elementId == DictionaryWithId) { return true; }
			if (elementId == SingleArrayWithId) { return true; }
			if (elementId == MultiArrayWithId) { return true; }
			return false;
		}
	}

	/// <summary>These elements are used during the binary serialization. They should be unique from Elements and Attributes.</summary>
	public static class SubElements
	{
		///<summary>
		///</summary>
		public const Byte Dimension = 51;

		///<summary>
		///</summary>
		public const Byte Dimensions = 52;

		///<summary>
		///</summary>
		public const Byte Item = 53;

		///<summary>
		///</summary>
		public const Byte Items = 54;

		///<summary>
		///</summary>
		public const Byte Properties = 55;

		///<summary>
		///</summary>
		public const Byte Unknown = 254;

		///<summary>
		///</summary>
		public const Byte Eof = 255;
	}

	/// <summary>These attributes are used during the binary serialization. They should be unique from Elements and SubElements.</summary>
	public class Attributes
	{
		///<summary>
		///</summary>
		public const Byte DimensionCount = 101;

		///<summary>
		///</summary>
		public const Byte ElementType = 102;

		///<summary>
		///</summary>
		public const Byte Indexes = 103;

		///<summary>
		///</summary>
		public const Byte KeyType = 104;

		///<summary>
		///</summary>
		public const Byte Length = 105;

		///<summary>
		///</summary>
		public const Byte LowerBound = 106;

		///<summary>
		///</summary>
		public const Byte Name = 107;

		///<summary>
		///</summary>
		public const Byte Type = 108;

		///<summary>
		///</summary>
		public const Byte Value = 109;

		///<summary>
		///</summary>
		public const Byte ValueType = 110;
	}

	/// <summary>How many bytes occupies a number value</summary>
	public static class NumberSize
	{
		///<summary>
		///  is zero
		///</summary>
		public const Byte Zero = 0;

		///<summary>
		///  serializes as 1 Byte
		///</summary>
		public const Byte B1 = 1;

		///<summary>
		///  serializes as 2 bytes
		///</summary>
		public const Byte B2 = 2;

		///<summary>
		///  serializes as 4 bytes
		///</summary>
		public const Byte B4 = 4;

		/// <summary>Gives the least required Byte amount to store the number</summary>
		/// <param name = "value"></param>
		/// <returns></returns>
		public static Byte GetNumberSize(Int32 value)
		{
			if (value == 0) { return Zero; }
			if (value > Int16.MaxValue || value < Int16.MinValue) { return B4; }
			if (value < Byte.MinValue || value > Byte.MaxValue) { return B2; }
			return B1;
		}
	}
}