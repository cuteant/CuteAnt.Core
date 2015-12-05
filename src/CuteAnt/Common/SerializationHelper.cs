using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CuteAnt.IO;

namespace CuteAnt
{
	/// <summary>数据序列化帮助类</summary>
	public static class SerializationHelper
	{
		public static readonly Boolean CanBinarySerialize;

		static SerializationHelper()
		{
			SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);

			try
			{
				permission.Demand();
				CanBinarySerialize = true;
			}
			catch (SecurityException)
			{
				CanBinarySerialize = false;
			}
		}

		#region -- 序列化XML字符串 --

		/// <summary>xml序列化成字符串</summary>
		/// <param name="obj">对象</param>
		/// <param name="isRemoveDefaultXSNS"></param>
		/// <returns>xml字符串</returns>
		public static String ConvertToString(object obj, Boolean isRemoveDefaultXSNS = true)
		{
			ValidationHelper.ArgumentNull(obj, "ojb");

			var serial = new XmlSerializer(obj.GetType());
			using (var stream = new MemoryStream())
			{
				var setting = new XmlWriterSettings();
				setting.Encoding = new UTF8Encoding(false);
				setting.Indent = true;
				using (var writer = XmlWriter.Create(stream, setting))
				{
					if (isRemoveDefaultXSNS)
					{
						// 去掉默认命名空间xmlns:xsd和xmlns:xsi
						var xsns = new XmlSerializerNamespaces();
						xsns.Add("", "");
						serial.Serialize(writer, obj, xsns);
					}
					else
					{
						serial.Serialize(writer, obj);
					}
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			}
		}

		/// <summary>输出Xml</summary>
		/// <returns></returns>
		public static String ConvertToString(object obj, String prefix, String ns)
		{
			ValidationHelper.ArgumentNullOrEmpty(prefix, "prefix");
			ValidationHelper.ArgumentNullOrEmpty(ns, "ns");

			var serial = new XmlSerializer(obj.GetType());
			using (var stream = new MemoryStream())
			{
				var setting = new XmlWriterSettings();
				setting.Encoding = new UTF8Encoding(false);
				setting.Indent = true;
				using (var writer = XmlWriter.Create(stream, setting))
				{
					var xsns = new XmlSerializerNamespaces();
					xsns.Add(prefix, ns);
					serial.Serialize(writer, obj, xsns);
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			}
		}

		/// <summary>反序列化</summary>
		/// <param name="type">类型</param>
		/// <param name="xml">XML字符串</param>
		/// <returns></returns>
		public static object ConvertToObject(Type type, String xml)
		{
			ValidationHelper.ArgumentNull(type, "type");
			ValidationHelper.ArgumentNullOrEmpty(xml, "xml");
			Byte[] b = Encoding.UTF8.GetBytes(xml);
			XmlSerializer serializer = new XmlSerializer(type);
			return serializer.Deserialize(new MemoryStream(b));
		}

		public static T ConvertToObject<T>(String xml) where T : class
		{
			ValidationHelper.ArgumentNullOrEmpty(xml, "xml");
			Byte[] b = Encoding.UTF8.GetBytes(xml);
			XmlSerializer serializer = new XmlSerializer(typeof(T));

			using (MemoryStream ms = new MemoryStream(b))
			{
				return serializer.Deserialize(ms) as T;
			}
		}

		public static T ConvertToObject<T>(XmlNode node) where T : class
		{
			return ConvertToObject<T>(node.OuterXml);
		}

		#endregion

		#region -- 序列化Byte数组 --

		/// <summary>转Byte[]</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static Byte[] ConvertToBytes(Object obj)
		{
			ValidationHelper.ArgumentNull(obj, "obj");
			if (CanBinarySerialize)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, obj);
					return stream.ToArray();
				}
			}
			else
			{
				return null;
			}
		}

		/// <summary>Byte[]转obj</summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static Object ConvertToObject(Byte[] bytes)
		{
			ValidationHelper.ArgumentNull(bytes, "bytes");
			ValidationHelper.ArgumentCondition((bytes.Length <= 4), "数组长度太小。");
			object obj2 = null;
			if (CanBinarySerialize)
			{
				using (MemoryStream stream = new MemoryStream(bytes))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					obj2 = formatter.Deserialize(stream);
				}
			}
			return obj2;
		}

		public static T ConvertToObject<T>(Byte[] bytes) where T : class
		{
			ValidationHelper.ArgumentNull(bytes, "bytes");
			ValidationHelper.ArgumentCondition((bytes.Length <= 4), "数组长度太小。");
			T obj = null;
			if (CanBinarySerialize)
			{
				using (MemoryStream stream = new MemoryStream(bytes))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					obj = formatter.Deserialize(stream) as T;
				}
			}
			return obj;
		}

		#endregion

		#region -- 序列化Binary文件 --

		public static object LoadBinaryFile(String file)
		{
			ValidationHelper.ArgumentNullOrEmpty(file, "file");
			file = FileHelper.FileExists(file);
			ValidationHelper.ArgumentCondition(file.IsNullOrWhiteSpace(), "文件不存在！");

			using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return formatter.Deserialize(stream);
			}
		}

		public static T LoadBinaryFile<T>(String file) where T : class
		{
			ValidationHelper.ArgumentNullOrEmpty(file, "file");
			file = FileHelper.FileExists(file);
			ValidationHelper.ArgumentCondition(file.IsNullOrWhiteSpace(), "文件不存在！");

			T obj = null;
			using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				obj = formatter.Deserialize(stream) as T;
			}
			return obj;
		}

		public static Boolean SaveAsBinary(object objectToSave, String path)
		{
			ValidationHelper.ArgumentNull(objectToSave, "objectToSave");
			ValidationHelper.ArgumentNullOrEmpty(path, "path");
			if (CanBinarySerialize)
			{
				using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, objectToSave);
				}
			}
			return false;
		}

		#endregion

		#region -- 序列化XML文件 --

		/// <summary>反序列化</summary>
		/// <param name="type">对象类型</param>
		/// <param name="filename">文件路径</param>
		/// <returns></returns>
		public static object LoadXMLFile(Type type, String filename)
		{
			ValidationHelper.ArgumentNullOrEmpty(filename, "filename");
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				using (var reader = new StreamReader(fs, Encoding.UTF8))
				{
					XmlSerializer serializer = new XmlSerializer(type);
					return serializer.Deserialize(reader);
				}
			}
		}

		/// <summary>反序列化XML文件</summary>
		/// <typeparam name="T">返回类型</typeparam>
		/// <param name="file">需要反序列化的文件路径</param>
		/// <returns></returns>
		public static T LoadXMLFile<T>(String file) where T : class
		{
			ValidationHelper.ArgumentNullOrEmpty(file, "file");
			using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				using (var reader = new StreamReader(fs, Encoding.UTF8))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					return (serializer.Deserialize(reader) as T);
				}
			}
		}

		/// <summary>序列化XML文件</summary>
		/// <param name="obj">对象</param>
		/// <param name="filename">文件路径</param>
		/// <param name="isRemoveDefaultXSNS"></param>
		public static Boolean SaveAsXML(object obj, String filename, Boolean isRemoveDefaultXSNS = true)
		{
			ValidationHelper.ArgumentNull(obj, "obj");
			ValidationHelper.ArgumentNullOrEmpty(filename, "filename");

			Boolean success = false;
			FileStream fs = null;

			// serialize it...
			try
			{
				XmlSerializer serializer = new XmlSerializer(obj.GetType());

				//serializer.Serialize(fs, obj);
				fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
				using (var writer = new StreamWriter(fs, Encoding.UTF8))
				{
					if (isRemoveDefaultXSNS)
					{
						// 去掉默认命名空间xmlns:xsd和xmlns:xsi
						var xsns = new XmlSerializerNamespaces();
						xsns.Add("", "");

						serializer.Serialize((TextWriter)writer, obj, xsns);
					}
					else
					{
						serializer.Serialize((TextWriter)writer, obj);
					}
				}

				success = true;
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
				}
			}
			return success;
		}

		public static Boolean SaveAsXML(object obj, String filename, String prefix, String ns)
		{
			ValidationHelper.ArgumentNullOrEmpty(prefix, "prefix");
			ValidationHelper.ArgumentNullOrEmpty(ns, "ns");
			var xsns = new XmlSerializerNamespaces();
			xsns.Add(prefix, ns);

			return SaveAsXML(obj, filename, xsns);
		}

		public static Boolean SaveAsXML(object obj, String filename, XmlSerializerNamespaces xsns)
		{
			ValidationHelper.ArgumentNull(obj, "obj");
			ValidationHelper.ArgumentNullOrEmpty(filename, "filename");

			Boolean success = false;
			FileStream fs = null;

			// serialize it...
			try
			{
				XmlSerializer serializer = new XmlSerializer(obj.GetType());

				//serializer.Serialize(fs, obj);
				fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
				using (var writer = new StreamWriter(fs, Encoding.UTF8))
				{
					serializer.Serialize((TextWriter)writer, obj, xsns);
				}

				success = true;
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
				}
			}
			return success;
		}

		#endregion
	}
}