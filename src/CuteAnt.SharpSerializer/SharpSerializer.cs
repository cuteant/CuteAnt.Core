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
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
//using CuteAnt.IO;
using CuteAnt.Serialization.Advanced;
using CuteAnt.Serialization.Advanced.Deserializing;
using CuteAnt.Serialization.Advanced.Serializing;
using CuteAnt.Serialization.Advanced.Xml;
using CuteAnt.Serialization.Core;
using CuteAnt.Serialization.Deserializing;
using CuteAnt.Serialization.Serializing;

namespace CuteAnt.Serialization
{
  /// <summary>This is the main class of SharpSerializer. It serializes and deserializes objects.</summary>
  public sealed class SharpSerializer
  {
    #region -- 属性 --

    private IPropertyDeserializer m_deserializer;
    private IPropertySerializer m_serializer;

    private PropertyProvider _PropertyProvider;

    /// <summary>
    ///   Default it is an instance of PropertyProvider. It provides all properties to serialize.
    ///   You can use an Ihneritor and overwrite its GetAllProperties and IgnoreProperty methods to implement your custom rules.
    /// </summary>
    public PropertyProvider PropertyProvider
    {
      get
      {
        if (_PropertyProvider == null)
        {
          _PropertyProvider = new PropertyProvider();
        }
        return _PropertyProvider;
      }
      set { _PropertyProvider = value; }
    }

    private String _RootName;

    /// <summary>What name should have the root property. Default is "Root".</summary>
    public String RootName
    {
      get
      {
        if (_RootName == null)
        {
          _RootName = "Root";
        }
        return _RootName;
      }
      set { _RootName = value; }
    }

    #endregion

    #region -- 构造 --

    /// <summary>Standard Constructor. Default is Xml serializing</summary>
    /// <param name="xmlSerialization" type="bool">
    /// <para>true - binary serialization with SizeOptimized mode.</para>
    /// <para>false - xml. For more options use other overloaded constructors.</para>
    /// </param>
    public SharpSerializer(Boolean xmlSerialization = true)
    {
      if (xmlSerialization)
      {
        initialize(new SharpSerializerXmlSettings());
      }
      else
      {
        initialize(new SharpSerializerBinarySettings());
      }
    }

    /// <summary>Xml serialization with custom settings</summary>
    /// <param name="settings" type="CuteAnt.Serialization.SharpSerializerXmlSettings">
    /// <para></para>
    /// </param>
    public SharpSerializer(SharpSerializerXmlSettings settings)
    {
      ValidationHelper.ArgumentNull(settings, "settings");
      initialize(settings);
    }

    /// <summary>Binary serialization with custom settings</summary>
    /// <param name="settings" type="CuteAnt.Serialization.SharpSerializerBinarySettings">
    /// <para></para>
    /// </param>
    public SharpSerializer(SharpSerializerBinarySettings settings)
    {
      ValidationHelper.ArgumentNull(settings, "settings");
      initialize(settings);
    }

    /// <summary>Custom serializer and deserializer</summary>
    /// <param name="serializer" type="CuteAnt.Serialization.Advanced.Serializing.IPropertySerializer">
    /// <para></para>
    /// </param>
    /// <param name="deserializer" type="CuteAnt.Serialization.Advanced.Deserializing.IPropertyDeserializer">
    /// <para></para>
    /// </param>
    public SharpSerializer(IPropertySerializer serializer, IPropertyDeserializer deserializer)
    {
      ValidationHelper.ArgumentNull(serializer, "serializer");
      ValidationHelper.ArgumentNull(deserializer, "deserializer");
      m_serializer = serializer;
      m_deserializer = deserializer;
    }

    #region - Initialize XmlSettings -

    private void initialize(SharpSerializerXmlSettings settings)
    {
      // PropertiesToIgnore
      PropertyProvider.PropertiesToIgnore = settings.AdvancedSettings.PropertiesToIgnore;
      PropertyProvider.AttributesToIgnore = settings.AdvancedSettings.AttributesToIgnore;

      //RootName
      RootName = settings.AdvancedSettings.RootName;

      // TypeNameConverter)
      ITypeNameConverter typeNameConverter = settings.AdvancedSettings.TypeNameConverter ??
                                              DefaultInitializer.GetTypeNameConverter(
                                                  settings.IncludeAssemblyVersionInTypeName,
                                                  settings.IncludeCultureInTypeName,
                                                  settings.IncludePublicKeyTokenInTypeName);

      // SimpleValueConverter
      ISimpleValueConverter simpleValueConverter = settings.AdvancedSettings.SimpleValueConverter ??
                                                   DefaultInitializer.GetSimpleValueConverter(settings.Culture, typeNameConverter);

      // XmlWriterSettings
      XmlWriterSettings xmlWriterSettings = DefaultInitializer.GetXmlWriterSettings(settings.Encoding);

      // XmlReaderSettings
      XmlReaderSettings xmlReaderSettings = DefaultInitializer.GetXmlReaderSettings();

      // Create Serializer and Deserializer
      var reader = new DefaultXmlReader(typeNameConverter, simpleValueConverter, xmlReaderSettings);
      var writer = new DefaultXmlWriter(typeNameConverter, simpleValueConverter, xmlWriterSettings);
      m_serializer = new XmlPropertySerializer(writer);
      m_deserializer = new XmlPropertyDeserializer(reader);
    }

    #endregion

    #region - Initialize BinarySettings -

    private void initialize(SharpSerializerBinarySettings settings)
    {
      // PropertiesToIgnore
      PropertyProvider.PropertiesToIgnore = settings.AdvancedSettings.PropertiesToIgnore;
      PropertyProvider.AttributesToIgnore = settings.AdvancedSettings.AttributesToIgnore;

      //RootName
      RootName = settings.AdvancedSettings.RootName;

      // TypeNameConverter)
      ITypeNameConverter typeNameConverter = settings.AdvancedSettings.TypeNameConverter ??
                                             DefaultInitializer.GetTypeNameConverter(
                                                 settings.IncludeAssemblyVersionInTypeName,
                                                 settings.IncludeCultureInTypeName,
                                                 settings.IncludePublicKeyTokenInTypeName);

      // Create Serializer and Deserializer
      CuteAnt.Serialization.Advanced.Binary.IBinaryReader reader = null;
      CuteAnt.Serialization.Advanced.Binary.IBinaryWriter writer = null;
      if (settings.Mode == BinarySerializationMode.Burst)
      {
        // Burst mode
        writer = new BurstBinaryWriter(typeNameConverter, settings.Encoding);
        reader = new BurstBinaryReader(typeNameConverter, settings.Encoding);
      }
      else
      {
        // Size optimized mode
        writer = new SizeOptimizedBinaryWriter(typeNameConverter, settings.Encoding);
        reader = new SizeOptimizedBinaryReader(typeNameConverter, settings.Encoding);
      }
      m_deserializer = new BinaryPropertyDeserializer(reader);
      m_serializer = new BinaryPropertySerializer(writer);
    }

    #endregion

    #endregion

    #region -- 静态 XML Serializing/Deserializing --

    #region - method XmlSerialize -

    /// <summary>Xml 序列化</summary>
    /// <param name="data" type="object">
    /// <para></para>
    /// </param>
    /// <param name="outputFileName" type="string">
    /// <para></para>
    /// </param>
    public static void XmlSerialize(object data, String outputFileName, SharpSerializerXmlSettings settings = null)
    {
      ValidationHelper.ArgumentNull(data, "data");
      ValidationHelper.ArgumentNullOrEmpty(outputFileName, "outputFileName");

      var serializer = (settings != null) ? new SharpSerializer(settings) : new SharpSerializer();
      serializer.XmlSerializeInternal(data, outputFileName);
    }

    private void XmlSerializeInternal(object data, String outputFileName)
    {
      createDirectoryIfNeccessary(outputFileName);

      var factory = new PropertyFactory(PropertyProvider);
      Property property = factory.CreateProperty(RootName, data);

      var serializer = m_serializer as XmlPropertySerializer;
      try
      {
        serializer.Open(outputFileName);
        serializer.Serialize(property);
      }
      finally
      {
        serializer.Close();
      }
    }

    /// <summary>Xml 序列化</summary>
    /// <param name="data" type="object">
    /// <para></para>
    /// </param>
    /// <param name="output" type="System.IO.Stream">
    /// <para></para>
    /// </param>
    public static void XmlSerialize(object data, Stream output, SharpSerializerXmlSettings settings = null)
    {
      ValidationHelper.ArgumentNull(data, "data");
      ValidationHelper.ArgumentNull(output, "output");

      var serializer = (settings != null) ? new SharpSerializer(settings) : new SharpSerializer();
      serializer.XmlSerializeInternal(data, output);
    }

    private void XmlSerializeInternal(object data, Stream output)
    {
      var factory = new PropertyFactory(PropertyProvider);
      Property property = factory.CreateProperty(RootName, data);

      var serializer = m_serializer as XmlPropertySerializer;
      try
      {
        serializer.Open(output);
        serializer.Serialize(property);
      }
      finally
      {
        serializer.Close();
      }
    }

    /// <summary>Xml 序列化</summary>
    /// <param name="data" type="object">
    /// <para></para>
    /// </param>
    /// <param name="output" type="System.IO.TextWriter">
    /// <para></para>
    /// </param>
    public static void XmlSerialize(object data, TextWriter output, SharpSerializerXmlSettings settings = null)
    {
      ValidationHelper.ArgumentNull(data, "data");
      ValidationHelper.ArgumentNull(output, "output");

      var serializer = (settings != null) ? new SharpSerializer(settings) : new SharpSerializer();
      serializer.XmlSerializeInternal(data, output);
    }

    private void XmlSerializeInternal(object data, TextWriter output)
    {
      var factory = new PropertyFactory(PropertyProvider);
      Property property = factory.CreateProperty(RootName, data);

      var serializer = m_serializer as XmlPropertySerializer;
      try
      {
        serializer.Open(output);
        serializer.Serialize(property);
      }
      finally
      {
        serializer.Close();
      }
    }

    /// <summary>Xml 序列化</summary>
    /// <param name="data" type="object">
    /// <para></para>
    /// </param>
    /// <param name="output" type="System.Text.StringBuilder">
    /// <para></para>
    /// </param>
    public static void XmlSerialize(object data, StringBuilder output, SharpSerializerXmlSettings settings = null)
    {
      ValidationHelper.ArgumentNull(data, "data");
      ValidationHelper.ArgumentNull(output, "output");

      var serializer = (settings != null) ? new SharpSerializer(settings) : new SharpSerializer();
      serializer.XmlSerializeInternal(data, output);
    }

    private void XmlSerializeInternal(object data, StringBuilder output)
    {
      var factory = new PropertyFactory(PropertyProvider);
      Property property = factory.CreateProperty(RootName, data);

      var serializer = m_serializer as XmlPropertySerializer;
      try
      {
        serializer.Open(output);
        serializer.Serialize(property);
      }
      finally
      {
        serializer.Close();
      }
    }

    #endregion

    #region - method XmlDeserialize --

    /// <summary>Xml 反序列化</summary>
    /// <param name="outputFileName" type="string">
    /// <para></para>
    /// </param>
    public static Object XmlDeserialize(String inputUri)
    {
      ValidationHelper.ArgumentNullOrEmpty(inputUri, "inputUri");
      var serializer = new SharpSerializer();
      return serializer.XmlDeserializeInternal(inputUri);
    }

    /// <summary>Xml 反序列化</summary>
    /// <param name="inputUri" type="string">
    /// <para></para>
    /// </param>
    /// <returns>A T value...</returns>
    public static T XmlDeserialize<T>(String inputUri) where T : class
    {
      T obj = XmlDeserialize(inputUri) as T;
      if (obj != null)
      {
        return obj;
      }
      else
      {
        throw new DeserializingException("反序列化对象失败！");
      }
    }

    private Object XmlDeserializeInternal(String inputUri)
    {
      var deserializer = m_deserializer as XmlPropertyDeserializer;
      try
      {
        // Deserialize Property
        deserializer.Open(inputUri);
        Property property = deserializer.Deserialize();
        deserializer.Close();

        // create object from Property
        var factory = new ObjectFactory();
        return factory.CreateObject(property);
      }
      catch (Exception exception)
      {
        // corrupted Stream
        throw new DeserializingException("An error occured during the deserialization. Details are in the inner exception.", exception);
      }
    }

    /// <summary>Xml 反序列化</summary>
    /// <param name="output" type="System.IO.Stream">
    /// <para></para>
    /// </param>
    /// <returns>A object value...</returns>
    public static Object XmlDeserialize(Stream input)
    {
      ValidationHelper.ArgumentNull(input, "input");
      var serializer = new SharpSerializer();
      return serializer.XmlDeserializeInternal(input);
    }

    /// <summary>Xml 反序列化</summary>
    /// <param name="input" type="System.IO.Stream">
    /// <para></para>
    /// </param>
    /// <returns>A T value...</returns>
    public static T XmlDeserialize<T>(Stream input) where T : class
    {
      T obj = XmlDeserialize(input) as T;
      if (obj != null)
      {
        return obj;
      }
      else
      {
        throw new DeserializingException("反序列化对象失败！");
      }
    }

    private Object XmlDeserializeInternal(Stream input)
    {
      var deserializer = m_deserializer as XmlPropertyDeserializer;
      try
      {
        // Deserialize Property
        deserializer.Open(input);
        Property property = deserializer.Deserialize();
        deserializer.Close();

        // create object from Property
        var factory = new ObjectFactory();
        return factory.CreateObject(property);
      }
      catch (Exception exception)
      {
        // corrupted Stream
        throw new DeserializingException("An error occured during the deserialization. Details are in the inner exception.", exception);
      }
    }

    /// <summary>Xml 反序列化</summary>
    /// <param name="output" type="System.IO.TextWriter">
    /// <para></para>
    /// </param>
    /// <returns>A object value...</returns>
    public static Object XmlDeserialize(TextReader input)
    {
      ValidationHelper.ArgumentNull(input, "input");
      var serializer = new SharpSerializer();
      return serializer.XmlDeserializeInternal(input);
    }

    /// <summary>Xml 反序列化</summary>
    /// <param name="input" type="System.IO.TextReader">
    /// <para></para>
    /// </param>
    /// <returns>A T value...</returns>
    public static T XmlDeserialize<T>(TextReader input) where T : class
    {
      T obj = XmlDeserialize(input) as T;
      if (obj != null)
      {
        return obj;
      }
      else
      {
        throw new DeserializingException("反序列化对象失败！");
      }
    }

    private Object XmlDeserializeInternal(TextReader input)
    {
      var deserializer = m_deserializer as XmlPropertyDeserializer;
      try
      {
        // Deserialize Property
        deserializer.Open(input);
        Property property = deserializer.Deserialize();
        deserializer.Close();

        // create object from Property
        var factory = new ObjectFactory();
        return factory.CreateObject(property);
      }
      catch (Exception exception)
      {
        // corrupted Stream
        throw new DeserializingException("An error occured during the deserialization. Details are in the inner exception.", exception);
      }
    }

    #endregion

    #endregion

    #region -- Serializing methods --

    /// <summary>Serializing to a file. File will be always new created and closed after the serialization.</summary>
    /// <param name = "data"></param>
    /// <param name = "filename"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Serialize(object data, String filename)
    {
      ValidationHelper.ArgumentNull(data, "data");
      ValidationHelper.ArgumentNullOrEmpty(filename, "filename");
      createDirectoryIfNeccessary(filename);

      using (Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
      {
        Serialize(data, stream);
      }
    }

    /// <summary>Serializing to the stream. After serialization the stream will NOT be closed.</summary>
    /// <param name = "data"></param>
    /// <param name = "stream"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Serialize(object data, Stream stream)
    {
      ValidationHelper.ArgumentNull(data, "data");
      var factory = new PropertyFactory(PropertyProvider);
      Property property = factory.CreateProperty(RootName, data);

      try
      {
        m_serializer.Open(stream);
        m_serializer.Serialize(property);
      }
      finally
      {
        m_serializer.Close();
      }
    }

    #region - method createDirectoryIfNeccessary -

    private void createDirectoryIfNeccessary(String filename)
    {
      var directory = Path.GetDirectoryName(filename);
      var dir = EnsureDirectory(directory);
    }
    private static String EnsureDirectory(String folder)
    {
      if (Directory.Exists(folder))
      {
        Directory.CreateDirectory(folder);
      }
      return folder;
    }

    #endregion

    #endregion

    #region -- Deserializing methods --

    /// <summary>Deserializing from the file. After deserialization the file will be closed.</summary>
    /// <param name="filename" type="string">
    /// <para></para>
    /// </param>
    /// <returns>A object value...</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Object Deserialize(String filename)
    {
      ValidationHelper.ArgumentNullOrEmpty(filename, "filename");

      using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        return Deserialize(stream);
      }
    }

    /// <summary>Deserializing from the file. After deserialization the file will be closed.</summary>
    /// <param name="filename" type="string">
    /// <para></para>
    /// </param>
    /// <returns>A object value...</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public T Deserialize<T>(String filename) where T : class
    {
      T obj = Deserialize(filename) as T;
      if (obj != null)
      {
        return obj;
      }
      else
      {
        throw new DeserializingException("反序列化对象失败！");
      }
    }

    /// <summary>Deserialization from the stream. After deserialization the stream will NOT be closed.</summary>
    /// <param name="stream" type="System.IO.Stream">
    /// <para></para>
    /// </param>
    /// <returns>A object value...</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Object Deserialize(Stream stream)
    {
      ValidationHelper.ArgumentNull(stream, "stream");

      try
      {
        // Deserialize Property
        m_deserializer.Open(stream);
        Property property = m_deserializer.Deserialize();
        m_deserializer.Close();

        // create object from Property
        var factory = new ObjectFactory();
        return factory.CreateObject(property);
      }
      catch (Exception exception)
      {
        // corrupted Stream
        throw new DeserializingException("An error occured during the deserialization. Details are in the inner exception.", exception);
      }
    }

    /// <summary>Deserialization from the stream. After deserialization the stream will NOT be closed.</summary>
    /// <param name="stream" type="System.IO.Stream">
    /// <para></para>
    /// </param>
    /// <returns>A object value...</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public T Deserialize<T>(Stream stream) where T : class
    {
      T obj = Deserialize(stream) as T;
      if (obj != null)
      {
        return obj;
      }
      else
      {
        throw new DeserializingException("反序列化对象失败！");
      }
    }

    #endregion
  }
}