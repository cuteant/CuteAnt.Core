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
using System.IO;
using System.Text;

namespace CuteAnt.Serialization.Advanced.Xml
{
	/// <summary>Writes data to xml or other node oriented format</summary>
	/// <remarks></remarks>
	public interface IXmlWriter
	{
		///<summary>
		///  Writes start tag/node/element
		///</summary>
		///<param name = "elementId"></param>
		void WriteStartElement(String elementId);

		///<summary>
		///  Writes end tag/node/element
		///</summary>
		void WriteEndElement();

		///<summary>
		///  Writes attribute of type String
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "text"></param>
		void WriteAttribute(String attributeId, String text);

		///<summary>
		///  Writes attribute of type Type
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "type"></param>
		void WriteAttribute(String attributeId, Type type);

		///<summary>
		///  Writes attribute of type integer
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "number"></param>
		void WriteAttribute(String attributeId, Int32 number);

		///<summary>
		///  Writes attribute of type array of Int32
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "numbers"></param>
		void WriteAttribute(String attributeId, Int32[] numbers);

		///<summary>
		///  Writes attribute of a simple type (value of a SimpleProperty)
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "value"></param>
		void WriteAttribute(String attributeId, object value);

		///<summary>
		///  Opens the stream
		///</summary>
		///<param name = "stream"></param>
		void Open(Stream stream);

		/// <summary>Opens the file</summary>
		/// <param name="outputFileName" type="string">
		/// <para></para>
		/// </param>
		void Open(String outputFileName);

		/// <summary>Opens the textwriter</summary>
		/// <param name="output" type="System.IO.TextWriter">
		/// <para></para>
		/// </param>
		void Open(TextWriter output);

		/// <summary>Opens the stringbuilder</summary>
		/// <param name="output" type="System.Text.StringBuilder">
		/// <para></para>
		/// </param>
		void Open(StringBuilder output);

		/// <summary>Writes all data to the stream, the stream can be further used.</summary>
		void Close();
	}
}