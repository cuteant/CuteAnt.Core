using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using CuteAnt.Serialization.Advanced.Serializing;
using CuteAnt.Serialization.Advanced.Xml;
using CuteAnt.Serialization.Core;

namespace CuteAnt.Xml
{
	/// <summary>Xml读取器</summary>
	/// <remarks></remarks>
	public class HmXmlReaderX : DisposeBase
	{
		#region -- Fields --

		private XmlReaderSettings m_settings;
		private CultureInfo m_cultureInfo;
		private ISimpleValueConverter m_simpleValueConverter;
		private ITypeNameConverter m_typeNameConverter;

		#endregion

		#region -- 属性 --

		private XmlReader m_currentReader;
		private Stack<XmlReader> m_readerStack;

		public XmlReader CurrentReader { get { return m_currentReader; } }

		#endregion

		#region -- 构造 --

		/// <summary>默认构造函数</summary>
		public HmXmlReaderX()
			: this(null, null)
		{
		}

		/// <summary>构造函数</summary>
		/// <param name="settings" type="System.Xml.XmlReaderSettings">
		/// <para></para>
		/// </param>
		/// <param name="cultureInfo" type="System.Globalization.CultureInfo">
		/// <para></para>
		/// </param>
		public HmXmlReaderX(XmlReaderSettings settings = null, CultureInfo cultureInfo = null)
		{
			if (settings != null)
			{
				m_settings = settings;
			}
			else
			{
				m_settings = new XmlReaderSettings();
				m_settings.IgnoreComments = true;
				m_settings.IgnoreWhitespace = true;
			}
			if (cultureInfo != null)
			{
				m_cultureInfo = cultureInfo;
			}
			else
			{
				m_cultureInfo = CultureInfo.InvariantCulture;
			}

			// TypeNameConverter
			m_typeNameConverter = DefaultInitializer.GetTypeNameConverter(false, false, false);

			// SimpleValueConverter
			m_simpleValueConverter = DefaultInitializer.GetSimpleValueConverter(m_cultureInfo, m_typeNameConverter);
		}

		#endregion

		#region -- Dispose --

		/// <summary>子类重载实现资源释放逻辑</summary>
		/// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）</param>
		protected override void OnDispose(Boolean disposing)
		{
			base.OnDispose(disposing);
			Close();
			m_readerStack = null;
			m_currentReader = null;
			m_settings = null;
			m_cultureInfo = null;
			m_simpleValueConverter = null;
			m_typeNameConverter = null;
		}

		#endregion

		#region -- method Open --

		/// <summary>Open the stream</summary>
		/// <param name = "stream"></param>
		public void Open(Stream stream)
		{
			m_readerStack = new Stack<XmlReader>();
			XmlReader reader = XmlReader.Create(stream, m_settings);

			// set the main reader
			pushCurrentReader(reader);
		}

		/// <summary>Open the Uri</summary>
		/// <param name="inputUri" type="string">
		/// <para></para>
		/// </param>
		public void Open(String inputUri)
		{
			m_readerStack = new Stack<XmlReader>();
			XmlReader reader = XmlReader.Create(inputUri, m_settings);

			// set the main reader
			pushCurrentReader(reader);
		}

		/// <summary>Open the TextReader</summary>
		/// <param name="input" type="System.IO.TextReader">
		/// <para></para>
		/// </param>
		public void Open(TextReader input)
		{
			m_readerStack = new Stack<XmlReader>();
			XmlReader reader = XmlReader.Create(input, m_settings);

			// set the main reader
			pushCurrentReader(reader);
		}

		#endregion

		#region -- method Close --

		/// <summary>Stream can be further used</summary>
		public void Close()
		{
			if (m_currentReader != null)
			{
				m_currentReader.Close();
			}
		}

		#endregion

		#region -- method popCurrentReader --

		/// <summary>Remove one reader from stack and reset the current reader</summary>
		private void popCurrentReader()
		{
			// Remove one reader from the stack
			if (m_readerStack.Count > 0)
			{
				m_readerStack.Pop();
			}
			if (m_readerStack.Count > 0)
			{
				m_currentReader = m_readerStack.Peek();
				return;
			}
			m_currentReader = null;
		}

		#endregion

		#region -- method pushCurrentReader --

		/// <summary>Add reader to stack and set it the current reader</summary>
		/// <param name = "reader"></param>
		private void pushCurrentReader(XmlReader reader)
		{
			m_readerStack.Push(reader);
			m_currentReader = reader;
		}

		#endregion

		/// <summary>Reads next valid element</summary>
		/// <returns>null if nothing was found</returns>
		public String ReadElement()
		{
			while (m_currentReader.Read())
			{
				if (m_currentReader.NodeType != XmlNodeType.Element) { continue; }
				return m_currentReader.Name;
			}
			return null;
		}

		/// <summary>Reads all sub elements of the current element</summary>
		/// <returns></returns>
		public IEnumerable<String> ReadSubElements()
		{
			// Position the reader on an element
			m_currentReader.MoveToElement();

			// create the subReader
			XmlReader subReader = m_currentReader.ReadSubtree();

			// positions the new XmlReader on the node that was current before the call to ReadSubtree method
			// http://msdn.microsoft.com/query/dev10.query?appId=Dev10IDEF1&l=EN-US&k=k%28SYSTEM.XML.XMLREADER.READSUBTREE%29;k%28TargetFrameworkMoniker-%22.NETFRAMEWORK%2cVERSION%3dV2.0%22%29;k%28DevLang-CSHARP%29&rd=true
			subReader.Read();
			pushCurrentReader(subReader);

			try
			{
				// read the first valid element
				String name = ReadElement();

				// read further elements
				while (!name.IsNullOrWhiteSpace())
				{
					yield return name;
					name = ReadElement();
				}
			}
			finally
			{
				// Close the current reader,
				// it positions the parent reader on the last node of the subReader
				subReader.Close();

				// aktualise the current Reader
				popCurrentReader();
			}
		}

		/// <summary>Reads attribute as String</summary>
		/// <param name = "attributeName"></param>
		/// <returns>null if nothing was found</returns>
		public String GetAttributeAsString(String attributeName)
		{
			if (!m_currentReader.MoveToAttribute(attributeName)) { return null; }
			return m_currentReader.Value;
		}

		/// <summary>Reads attribute and converts it to type</summary>
		/// <param name = "attributeName"></param>
		/// <returns>null if nothing found</returns>
		public Type GetAttributeAsType(String attributeName)
		{
			String typeName = GetAttributeAsString(attributeName);
			return m_typeNameConverter.ConvertToType(typeName);
		}

		/// <summary>Reads attribute and converts it to integer</summary>
		/// <param name = "attributeName"></param>
		/// <returns>0 if nothing found</returns>
		public Int32 GetAttributeAsInt(String attributeName)
		{
			if (!m_currentReader.MoveToAttribute(attributeName)) { return 0; }
			return m_currentReader.ReadContentAsInt();
		}

		/// <summary>Reads attribute and converts it as array of Int32</summary>
		/// <param name = "attributeName"></param>
		/// <returns>empty array if nothing found</returns>
		public T[] GetAttributeAsArrayOfInt<T>(String attributeName)
		{
			if (!m_currentReader.MoveToAttribute(attributeName)) { return null; }
			return m_currentReader.Value.SplitDefaultSeparator<T>();
		}

		/// <summary>Reads attribute and converts it to object of the expectedType</summary>
		/// <param name = "attributeName"></param>
		/// <param name = "expectedType"></param>
		/// <returns></returns>
		public object GetAttributeAsObject(String attributeName, Type expectedType)
		{
			String objectAsText = GetAttributeAsString(attributeName);
			return m_simpleValueConverter.ConvertFromString(objectAsText, expectedType);
		}
	}
}