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

namespace CuteAnt.Serialization.Core.Xml
{
	/// <summary>These elements are used as tags during the xml serialization.</summary>
	public static class Elements
	{
		///<summary>
		///</summary>
		public const String Collection = "Collection";

		///<summary>
		///</summary>
		public const String ComplexObject = "Complex";

		///<summary>
		/// internal used as an id for referencing already serialized items
		/// Since v.2.12 Elements.Reference is used instead.
		///</summary>
		public const String OldReference = "ComplexReference";

		///<summary>
		/// used as an id for referencing already serialized items
		///</summary>
		public const String Reference = "Reference";

		///<summary>
		///</summary>
		public const String Dictionary = "Dictionary";

		///<summary>
		///</summary>
		public const String MultiArray = "MultiArray";

		///<summary>
		///</summary>
		public const String Null = "Null";

		///<summary>
		///</summary>
		public const String SimpleObject = "Simple";

		///<summary>
		///</summary>
		public const String SingleArray = "SingleArray";
	}

	/// <summary>These elements are used as tags during the xml serialization.</summary>
	public static class SubElements
	{
		///<summary>
		///</summary>
		public const String Dimension = "Dimension";

		///<summary>
		///</summary>
		public const String Dimensions = "Dimensions";

		///<summary>
		///</summary>
		public const String Item = "Item";

		///<summary>
		///</summary>
		public const String Items = "Items";

		///<summary>
		///</summary>
		public const String Properties = "Properties";
	}

	/// <summary>These attributes are used during the xml serialization.</summary>
	public static class Attributes
	{
		///<summary>
		///</summary>
		public const String DimensionCount = "dimensionCount";

		///<summary>
		///</summary>
		public const String ElementType = "elementType";

		///<summary>
		///</summary>
		public const String Indexes = "indexes";

		///<summary>
		///</summary>
		public const String KeyType = "keyType";

		///<summary>
		///</summary>
		public const String Length = "length";

		///<summary>
		///</summary>
		public const String LowerBound = "lowerBound";

		///<summary>
		///</summary>
		public const String Name = "name";

		///<summary>
		///</summary>
		public const String Type = "type";

		///<summary>
		///</summary>
		public const String Value = "value";

		///<summary>
		///</summary>
		public const String ValueType = "valueType";

		///<summary>
		/// used as an id to identify and refere already serialized items
		///</summary>
		public const String ReferenceId = "id";
	}
}