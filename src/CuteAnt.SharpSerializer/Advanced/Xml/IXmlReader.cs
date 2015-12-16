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
using System.IO;

namespace CuteAnt.Serialization.Advanced.Xml
{
	/// <summary>Reads data from Xml or other node oriented format</summary>
	/// <remarks></remarks>
	public interface IXmlReader
	{
		/// <summary>Reads next valid element</summary>
		/// <returns>null if nothing was found</returns>
		String ReadElement();

		/// <summary>Reads all sub elements of the current element</summary>
		/// <returns></returns>
		IEnumerable<String> ReadSubElements();

		/// <summary>Reads attribute as String</summary>
		/// <param name = "attributeName"></param>
		/// <returns>null if nothing was found</returns>
		String GetAttributeAsString(String attributeName);

		/// <summary>Reads attribute and converts it to type</summary>
		/// <param name = "attributeName"></param>
		/// <returns>null if nothing found</returns>
		Type GetAttributeAsType(String attributeName);

		/// <summary>Reads attribute and converts it to integer</summary>
		/// <param name = "attributeName"></param>
		/// <returns>0 if nothing found</returns>
		Int32 GetAttributeAsInt(String attributeName);

		/// <summary>Reads attribute and converts it as array of Int32</summary>
		/// <param name = "attributeName"></param>
		/// <returns>empty array if nothing found</returns>
		Int32[] GetAttributeAsArrayOfInt(String attributeName);

		/// <summary>Reads attribute and converts it to object of the expectedType</summary>
		/// <param name = "attributeName"></param>
		/// <param name = "expectedType"></param>
		/// <returns></returns>
		object GetAttributeAsObject(String attributeName, Type expectedType);

		/// <summary>Open the stream</summary>
		/// <param name = "stream"></param>
		void Open(Stream stream);

		/// <summary>Open the Uri</summary>
		/// <param name="inputUri" type="System.IO.Stream">
		/// <para></para>
		/// </param>
		void Open(String inputUri);

		/// <summary>Open the textreader</summary>
		/// <param name="input" type="System.IO.TextReader">
		/// <para></para>
		/// </param>
		void Open(TextReader input);

		/// <summary>Stream can be further used</summary>
		void Close();
	}
}