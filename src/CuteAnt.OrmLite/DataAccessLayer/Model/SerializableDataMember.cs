/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>可序列化数据成员</summary>
	internal abstract class SerializableDataMember : IXmlSerializable
	{
		#region IXmlSerializable 成员

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			ModelHelper.ReadXml(reader, this);

			// 跳过当前节点
			reader.Skip();
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			ModelHelper.WriteXml(writer, this);
		}

		#endregion
	}
}