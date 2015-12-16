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
using System.IO;
using System.Text;
using System.Xml;
using CuteAnt.Serialization.Advanced.Serializing;
using CuteAnt.Serialization.Advanced.Xml;

namespace CuteAnt.Serialization.Advanced
{
	/// <summary>Stores data in xml format</summary>
	public sealed class DefaultXmlWriter : IXmlWriter
	{
		private readonly XmlWriterSettings _settings;
		private readonly ISimpleValueConverter _simpleValueConverter;
		private readonly ITypeNameConverter _typeNameProvider;
		private XmlWriter _writer;

		/// <summary>Constructor with custom ITypeNameProvider and ISimpleValueConverter and custom XmlWriterSettings</summary>
		public DefaultXmlWriter(ITypeNameConverter typeNameProvider, ISimpleValueConverter simpleValueConverter,
														XmlWriterSettings settings)
		{
			ValidationHelper.ArgumentNull(typeNameProvider, "typeNameProvider");
			ValidationHelper.ArgumentNull(simpleValueConverter, "simpleValueConverter");
			ValidationHelper.ArgumentNull(settings, "settings");
			_simpleValueConverter = simpleValueConverter;
			_settings = settings;
			_typeNameProvider = typeNameProvider;
		}

		#region IXmlWriter Members

		///<summary>
		///  Writes start tag/node/element
		///</summary>
		///<param name = "elementId"></param>
		public void WriteStartElement(String elementId)
		{
			_writer.WriteStartElement(elementId);
		}

		///<summary>
		///  Writes end tag/node/element
		///</summary>
		public void WriteEndElement()
		{
			_writer.WriteEndElement();
		}

		///<summary>
		///  Writes attribute of type String
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "text"></param>
		public void WriteAttribute(String attributeId, String text)
		{
			if (text == null) { return; }
			_writer.WriteAttributeString(attributeId, text);
		}

		///<summary>
		///  Writes attribute of type Type
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "type"></param>
		public void WriteAttribute(String attributeId, Type type)
		{
			if (type == null) { return; }
			String valueAsText = _typeNameProvider.ConvertToTypeName(type);
			WriteAttribute(attributeId, valueAsText);
		}

		///<summary>
		///  Writes attribute of type integer
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "number"></param>
		public void WriteAttribute(String attributeId, Int32 number)
		{
			_writer.WriteAttributeString(attributeId, number.ToString());
		}

		///<summary>
		///  Writes attribute of type array of Int32
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "numbers"></param>
		public void WriteAttribute(String attributeId, Int32[] numbers)
		{
			String valueAsText = getArrayOfIntAsText(numbers);
			_writer.WriteAttributeString(attributeId, valueAsText);
		}

		///<summary>
		///  Writes attribute of a simple type (value of a SimpleProperty)
		///</summary>
		///<param name = "attributeId"></param>
		///<param name = "value"></param>
		public void WriteAttribute(String attributeId, object value)
		{
			if (value == null) { return; }
			String valueAsText = _simpleValueConverter.ConvertToString(value);
			_writer.WriteAttributeString(attributeId, valueAsText);
		}

		///<summary>Opens the stream</summary>
		///<param name = "stream"></param>
		public void Open(Stream stream)
		{
			_writer = XmlWriter.Create(stream, _settings);
			_writer.WriteStartDocument(true);
		}

		/// <summary>Opens the file</summary>
		/// <param name="outputFileName" type="string">
		/// <para></para>
		/// </param>
		public void Open(String outputFileName)
		{
			_writer = XmlWriter.Create(outputFileName, _settings);
			_writer.WriteStartDocument(true);
		}

		/// <summary>Opens the textwriter</summary>
		/// <param name="output" type="System.IO.TextWriter">
		/// <para></para>
		/// </param>
		public void Open(TextWriter output)
		{
			_writer = XmlWriter.Create(output, _settings);
			_writer.WriteStartDocument(true);
		}

		/// <summary>Opens the stringbuilder</summary>
		/// <param name="output" type="System.Text.StringBuilder">
		/// <para></para>
		/// </param>
		public void Open(StringBuilder output)
		{
			_writer = XmlWriter.Create(output, _settings);
			_writer.WriteStartDocument(true);
		}

		/// <summary>Writes all data to the stream, the stream can be further used.</summary>
		public void Close()
		{
			_writer.WriteEndDocument();
#if PORTABLE
			_writer.Dispose();
#else
			_writer.Close();
#endif
		}

		#endregion

		/// <summary>Converts Int32[] {1,2,3,4,5} to text "1,2,3,4,5"</summary>
		/// <param name = "values"></param>
		/// <returns></returns>
		private static String getArrayOfIntAsText(Int32[] values)
		{
			if (values.Length == 0) { return String.Empty; }
			var sb = new StringBuilder();

			foreach (Int32 index in values)
			{
				sb.Append(index.ToString());
				sb.Append(",");
			}
			String result = sb.ToString().TrimEnd(new[] { ',' });
			return result;
		}
	}
}