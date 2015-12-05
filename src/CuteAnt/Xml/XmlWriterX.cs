using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using CuteAnt.IO;
using CuteAnt.Serialization.Advanced.Serializing;
using CuteAnt.Serialization.Advanced.Xml;
using CuteAnt.Serialization.Core;

namespace CuteAnt.Xml
{
	/// <summary>Xml写入器</summary>
	/// <remarks></remarks>
	public class HmXmlWriterX : DisposeBase
	{
		#region -- Fields --

		private XmlWriterSettings m_settings;
		private CultureInfo m_cultureInfo;
		private ISimpleValueConverter m_simpleValueConverter;
		private ITypeNameConverter m_typeNameProvider;

		#endregion

		#region -- 属性 --

		private XmlWriter m_writer;

		/// <summary>内部XmlWriter实例</summary>
		/// <value>
		/// <para></para>
		/// </value>
		/// <remarks></remarks>
		public XmlWriter InnerWriter { get { return m_writer; } }

		#endregion

		#region -- 构造 --

		/// <summary>默认构造函数</summary>
		/// <param name="omitXmlDeclaration" type="bool">
		/// <para>是否忽略Xml声明</para>
		/// </param>
		public HmXmlWriterX(Boolean omitXmlDeclaration = false)
		{
			m_settings = new XmlWriterSettings();
			m_settings.Indent = true;
			m_settings.Encoding = Encoding.UTF8;
			m_settings.OmitXmlDeclaration = omitXmlDeclaration;
			m_settings.ConformanceLevel = ConformanceLevel.Document;
			m_cultureInfo = CultureInfo.InvariantCulture;
			InitConverter();
		}

		/// <summary>构造函数</summary>
		/// <param name="settings" type="System.Xml.XmlWriterSettings">
		/// <para>Xml写配置</para>
		/// </param>
		/// <param name="cultureInfo" type="System.Globalization.CultureInfo">
		/// <para>区域性特定的信息</para>
		/// </param>
		public HmXmlWriterX(XmlWriterSettings settings, CultureInfo cultureInfo)
		{
			ValidationHelper.ArgumentNull(settings, "settings");
			ValidationHelper.ArgumentNull(cultureInfo, "cultureInfo");
			m_settings = settings;
			m_cultureInfo = cultureInfo;
			InitConverter();
		}

		/// <summary>初始化转换器</summary>
		private void InitConverter()
		{
			// TypeNameConverter
			m_typeNameProvider = DefaultInitializer.GetTypeNameConverter(false, false, false);

			// SimpleValueConverter
			m_simpleValueConverter = DefaultInitializer.GetSimpleValueConverter(m_cultureInfo, m_typeNameProvider);
		}

		#endregion

		#region -- Dispose --

		/// <summary>子类重载实现资源释放逻辑</summary>
		/// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）</param>
		protected override void OnDispose(Boolean disposing)
		{
			base.OnDispose(disposing);
			Close();
			m_writer = null;
			m_settings = null;
			m_cultureInfo = null;
			m_simpleValueConverter = null;
			m_typeNameProvider = null;
		}

		#endregion

		#region -- method Open --

		/// <summary>使用指定的文件名创建并开启一个新的 XmlWriter 实例。</summary>
		/// <param name="outputFileName"></param>
		/// <param name="includeStandalone"></param>
		/// <param name="standalone"></param>
		public void Open(String outputFileName, Boolean includeStandalone = true, Boolean standalone = true)
		{
			ValidationHelper.ArgumentNullOrEmpty(outputFileName, "outputFileName");

			// 确保目录存在
			var directory = Path.GetDirectoryName(outputFileName);
			var dir = PathHelper.EnsureDirectory(directory);

			m_writer = XmlWriter.Create(outputFileName, m_settings);
			if (includeStandalone)
			{
				m_writer.WriteStartDocument(standalone);
			}
			else
			{
				m_writer.WriteStartDocument();
			}
		}

		/// <summary>使用指定的流创建并开启一个新的 XmlWriter 实例。</summary>
		/// <param name="output"></param>
		/// <param name="includeStandalone"></param>
		/// <param name="standalone"></param>
		public void Open(Stream output, Boolean includeStandalone = true, Boolean standalone = true)
		{
			ValidationHelper.ArgumentNull(output, "output");
			m_writer = XmlWriter.Create(output, m_settings);
			if (includeStandalone)
			{
				m_writer.WriteStartDocument(standalone);
			}
			else
			{
				m_writer.WriteStartDocument();
			}
		}

		/// <summary>使用指定的 TextWriter 创建并开启一个新的 XmlWriter 实例。</summary>
		/// <param name="output"></param>
		/// <param name="includeStandalone"></param>
		/// <param name="standalone"></param>
		public void Open(TextWriter output, Boolean includeStandalone = true, Boolean standalone = true)
		{
			ValidationHelper.ArgumentNull(output, "output");
			m_writer = XmlWriter.Create(output, m_settings);
			if (includeStandalone)
			{
				m_writer.WriteStartDocument(standalone);
			}
			else
			{
				m_writer.WriteStartDocument();
			}
		}

		/// <summary>使用指定的 StringBuilder 创建并开启一个新的 XmlWriter 实例。</summary>
		/// <param name="output"></param>
		/// <param name="includeStandalone"></param>
		/// <param name="standalone"></param>
		public void Open(StringBuilder output, Boolean includeStandalone = true, Boolean standalone = true)
		{
			ValidationHelper.ArgumentNull(output, "output");
			m_writer = XmlWriter.Create(output, m_settings);
			if (includeStandalone)
			{
				m_writer.WriteStartDocument(standalone);
			}
			else
			{
				m_writer.WriteStartDocument();
			}
		}

		#endregion

		#region -- Element 操作 --

		#region - method WriteStartElement -

		/// <summary>Writes start tag/node/element</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		public void WriteStartElement(String elementId)
		{
			m_writer.WriteStartElement(null, elementId, null);
		}

		/// <summary>Writes start tag/node/element</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		public void WriteStartElement(String elementId, String ns)
		{
			m_writer.WriteStartElement(null, elementId, ns);
		}

		/// <summary>Writes start tag/node/element</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		public void WriteStartElement(String prefix, String elementId, String ns)
		{
			m_writer.WriteStartElement(prefix, elementId, ns);
		}

		#endregion

		#region - method WriteEndElement -

		///<summary>Writes end tag/node/element</summary>
		public void WriteEndElement()
		{
			m_writer.WriteEndElement();
		}

		#endregion

		#region - method WriteElement(String elementId, String text) -

		/// <summary>编写具有指定的本地名称和值的元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="text" type="string">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, String text)
		{
			if (text == null) { return; }
			m_writer.WriteElementString(elementId, null, text);
		}

		/// <summary>编写具有指定的本地名称和值的元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="type" type="System.Type">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, Type type)
		{
			if (type == null) { return; }
			String valueAsText = m_typeNameProvider.ConvertToTypeName(type);
			m_writer.WriteElementString(elementId, null, valueAsText);
		}

		/// <summary>编写具有指定的本地名称和值的元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="number" type="int">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, Int32 number)
		{
			m_writer.WriteElementString(elementId, null, number.ToString());
		}

		/// <summary>编写具有指定的本地名称和值的元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="array" type="T[]">
		/// <para></para>
		/// </param>
		public void WriteElement<T>(String elementId, T[] array)
		{
			if (array == null) { return; }
			String valueAsText = getArrayAsText<T>(array);
			m_writer.WriteElementString(elementId, null, valueAsText);
		}

		/// <summary>编写具有指定的本地名称和值的元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="value" type="object">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, object value)
		{
			if (value == null) { return; }
			String valueAsText = m_simpleValueConverter.ConvertToString(value);
			m_writer.WriteElementString(elementId, null, valueAsText);
		}

		#endregion

		#region -- method WriteElement(String elementId, String ns, String text) --

		/// <summary>使用指定的本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="text" type="string">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, String ns, String text)
		{
			m_writer.WriteElementString(elementId, ns, text);
		}

		/// <summary>使用指定的本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="type" type="System.Type">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, String ns, Type type)
		{
			if (type == null) { return; }
			String valueAsText = m_typeNameProvider.ConvertToTypeName(type);
			m_writer.WriteElementString(elementId, ns, valueAsText);
		}

		/// <summary>使用指定的本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="number" type="int">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, String ns, Int32 number)
		{
			m_writer.WriteElementString(elementId, ns, number.ToString());
		}

		/// <summary>使用指定的本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="array" type="T[]">
		/// <para></para>
		/// </param>
		public void WriteElement<T>(String elementId, String ns, T[] array)
		{
			if (array == null) { return; }
			String valueAsText = getArrayAsText<T>(array);
			m_writer.WriteElementString(elementId, ns, valueAsText);
		}

		/// <summary>使用指定的本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="value" type="object">
		/// <para></para>
		/// </param>
		public void WriteElement(String elementId, String ns, object value)
		{
			if (value == null) { return; }
			String valueAsText = m_simpleValueConverter.ConvertToString(value);
			m_writer.WriteElementString(elementId, ns, valueAsText);
		}

		#endregion

		#region -- method WriteElement(String prefix, String elementId, String ns, String text) --

		/// <summary>使用具有指定的前缀、本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="text" type="string">
		/// <para></para>
		/// </param>
		public void WriteElement(String prefix, String elementId, String ns, String text)
		{
			m_writer.WriteElementString(prefix, elementId, ns, text);
		}

		/// <summary>使用具有指定的前缀、本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="type" type="System.Type">
		/// <para></para>
		/// </param>
		public void WriteElement(String prefix, String elementId, String ns, Type type)
		{
			if (type == null) { return; }
			String valueAsText = m_typeNameProvider.ConvertToTypeName(type);
			m_writer.WriteElementString(prefix, elementId, ns, valueAsText);
		}

		/// <summary>使用具有指定的前缀、本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="number" type="int">
		/// <para></para>
		/// </param>
		public void WriteElement(String prefix, String elementId, String ns, Int32 number)
		{
			m_writer.WriteElementString(prefix, elementId, ns, number.ToString());
		}

		/// <summary>使用具有指定的前缀、本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="array" type="T[]">
		/// <para></para>
		/// </param>
		public void WriteElement<T>(String prefix, String elementId, String ns, T[] array)
		{
			if (array == null) { return; }
			String valueAsText = getArrayAsText<T>(array);
			m_writer.WriteElementString(prefix, elementId, ns, valueAsText);
		}

		/// <summary>使用具有指定的前缀、本地名称、命名空间 URI 和值编写元素</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="elementId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="value" type="object">
		/// <para></para>
		/// </param>
		public void WriteElement(String prefix, String elementId, String ns, object value)
		{
			if (value == null) { return; }
			String valueAsText = m_simpleValueConverter.ConvertToString(value);
			m_writer.WriteElementString(prefix, elementId, ns, valueAsText);
		}

		#endregion

		#endregion

		#region -- Attribute 操作 --

		#region - method WriteStartAttribute -

		/// <summary>用指定的本地名称编写特性的起点。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		public void WriteStartAttribute(String attributeId)
		{
			m_writer.WriteStartAttribute(null, attributeId, null);
		}

		/// <summary>编写具有指定本地名称和命名空间 URI 的特性的起始内容。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		public void WriteStartAttribute(String attributeId, String ns)
		{
			m_writer.WriteStartAttribute(null, attributeId, ns);
		}

		/// <summary>编写具有指定的前缀、本地名称和命名空间 URI 的特性的起始内容。</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		public void WriteStartAttribute(String prefix, String attributeId, String ns)
		{
			m_writer.WriteStartAttribute(prefix, attributeId, ns);
		}

		#endregion

		#region - method WriteEndAttribute -

		/// <summary>关闭上一个 WriteStartAttribute 调用。</summary>
		public void WriteEndAttribute()
		{
			m_writer.WriteEndAttribute();
		}

		#endregion

		#region - method WriteAttribute(String attributeId, String text) -

		/// <summary>写出具有指定的本地名称和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="text" type="string">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, String text)
		{
			if (text == null) { return; }
			m_writer.WriteAttributeString(attributeId, text);
		}

		/// <summary>写出具有指定的本地名称和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="type" type="System.Type">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, Type type)
		{
			if (type == null) { return; }
			String valueAsText = m_typeNameProvider.ConvertToTypeName(type);
			m_writer.WriteAttributeString(attributeId, valueAsText);
		}

		/// <summary>写出具有指定的本地名称和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="number" type="int">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, Int32 number)
		{
			m_writer.WriteAttributeString(attributeId, number.ToString());
		}

		///<summary>写出具有指定的本地名称和值的特性。</summary>
		///<param name = "attributeId"></param>
		///<param name = "array"></param>
		public void WriteAttribute<T>(String attributeId, T[] array)
		{
			if (array == null) { return; }
			String valueAsText = getArrayAsText<T>(array);
			m_writer.WriteAttributeString(attributeId, valueAsText);
		}

		///<summary>写出具有指定的本地名称和值的特性。</summary>
		///<param name = "attributeId"></param>
		///<param name = "value"></param>
		public void WriteAttribute(String attributeId, object value)
		{
			if (value == null) { return; }
			String valueAsText = m_simpleValueConverter.ConvertToString(value);
			m_writer.WriteAttributeString(attributeId, valueAsText);
		}

		#endregion

		#region - method WriteAttribute(String attributeId, String ns, String text) -

		/// <summary>写入具有指定的本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="text" type="string">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, String ns, String text)
		{
			if (text == null) { return; }
			m_writer.WriteAttributeString(attributeId, ns, text);
		}

		/// <summary>写入具有指定的本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="type" type="System.Type">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, String ns, Type type)
		{
			if (type == null) { return; }
			String valueAsText = m_typeNameProvider.ConvertToTypeName(type);
			m_writer.WriteAttributeString(attributeId, ns, valueAsText);
		}

		/// <summary>写入具有指定的本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="number" type="int">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, String ns, Int32 number)
		{
			m_writer.WriteAttributeString(attributeId, ns, number.ToString());
		}

		/// <summary>写入具有指定的本地名称、命名空间 URI 和值的特性。</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attributeId"></param>
		/// <param name="ns"></param>
		/// <param name="array"></param>
		public void WriteAttribute<T>(String attributeId, String ns, T[] array)
		{
			if (array == null) { return; }
			String valueAsText = getArrayAsText<T>(array);
			m_writer.WriteAttributeString(attributeId, ns, valueAsText);
		}

		/// <summary>写入具有指定的本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="value" type="object">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String attributeId, String ns, object value)
		{
			if (value == null) { return; }
			String valueAsText = m_simpleValueConverter.ConvertToString(value);
			m_writer.WriteAttributeString(attributeId, ns, valueAsText);
		}

		#endregion

		#region - method WriteAttribute(String prefix, String attributeId, String ns, String text) -

		/// <summary>写出具有指定的前缀、本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="text" type="string">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String prefix, String attributeId, String ns, String text)
		{
			if (text == null) { return; }
			m_writer.WriteAttributeString(prefix, attributeId, ns, text);
		}

		/// <summary>写出具有指定的前缀、本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="type" type="System.Type">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String prefix, String attributeId, String ns, Type type)
		{
			if (type == null) { return; }
			String valueAsText = m_typeNameProvider.ConvertToTypeName(type);
			m_writer.WriteAttributeString(prefix, attributeId, ns, valueAsText);
		}

		/// <summary>写出具有指定的前缀、本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="number" type="int">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String prefix, String attributeId, String ns, Int32 number)
		{
			m_writer.WriteAttributeString(prefix, attributeId, ns, number.ToString());
		}

		/// <summary>写出具有指定的前缀、本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="array" type="T[]">
		/// <para></para>
		/// </param>
		public void WriteAttribute<T>(String prefix, String attributeId, String ns, T[] array)
		{
			if (array == null) { return; }
			String valueAsText = getArrayAsText<T>(array);
			m_writer.WriteAttributeString(prefix, attributeId, ns, valueAsText);
		}

		/// <summary>写出具有指定的前缀、本地名称、命名空间 URI 和值的特性。</summary>
		/// <param name="prefix" type="string">
		/// <para></para>
		/// </param>
		/// <param name="attributeId" type="string">
		/// <para></para>
		/// </param>
		/// <param name="ns" type="string">
		/// <para></para>
		/// </param>
		/// <param name="value" type="object">
		/// <para></para>
		/// </param>
		public void WriteAttribute(String prefix, String attributeId, String ns, object value)
		{
			if (value == null) { return; }
			String valueAsText = m_simpleValueConverter.ConvertToString(value);
			m_writer.WriteAttributeString(prefix, attributeId, ns, valueAsText);
		}

		#endregion

		#endregion

		#region -- method Close --

		/// <summary></summary>
		public void Close()
		{
			if (m_writer != null)
			{
				m_writer.WriteEndDocument();
				m_writer.Close();
			}
		}

		#endregion

		#region -- method getArrayAsText --

		/// <summary>Converts Int32[] {1,2,3,4,5} to text "1,2,3,4,5"</summary>
		/// <param name="values" type="int[]">
		/// <para></para>
		/// </param>
		/// <returns>A string value...</returns>
		private static String getArrayAsText<T>(T[] values)
		{
			if (values.Length == 0) { return String.Empty; }
			var sb = new StringBuilder();

			foreach (T index in values)
			{
				sb.Append(index.ToString());
				sb.Append(",");
			}
			String result = sb.ToString().TrimEnd(new[] { ',' });
			return result;
		}

		#endregion
	}
}