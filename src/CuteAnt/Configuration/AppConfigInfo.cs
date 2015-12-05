using System;
using System.Reflection;
using System.Xml.Serialization;

using CuteAnt.Reflection;

namespace CuteAnt.Configuration
{
	/// <summary>HmFrame组件设置描述类, 加[Serializable]标记为可序列化</summary>
	[Serializable]
	[XmlRoot("configuration", IsNullable = false)]
	public class AppConfigInfo : ConfigInfoBase
	{
		//#region -- IO配置 --

		///// <summary>
		///// IO配置
		///// </summary>
		//public class StreamHandlerItem
		//{
		//	/// <summary>
		//	/// 总线名称
		//	/// </summary>
		//	public String Name { get; set; }

		//	/// <summary>
		//	/// 数据流处理器，多个处理器用竖线|隔开，前面的处理器比后面的先处理。
		//	/// </summary>
		//	public String Processor { get; set; }
		//}

		//#endregion

		//#region -- 对象容器配置 --

		///// <summary>
		///// 对象容器配置
		///// </summary>
		//public class ObjectContainerItem
		//{
		//	/// <summary>
		//	/// 接口名称
		//	/// </summary>
		//	public String Interface { get; set; }

		//	/// <summary>
		//	/// 标示
		//	/// </summary>
		//	public String Name { get; set; }

		//	/// <summary>
		//	/// 类型名称
		//	/// </summary>
		//	public String Type { get; set; }

		//	//public Boolean IsSingleton { get; set; }

		//	/// <summary>
		//	/// 优先级
		//	/// </summary>
		//	public Int32 Priority { get; set; }

		//	/// <summary>
		//	/// 模式标记
		//	/// </summary>
		//	public String Mode { get; set; }
		//}

		//#endregion

		//#region -- Properties implementation --

		//// The XmlArrayAttribute changes the XML element name
		//// from the default of "StreamHandlers" to "StreamHandlerItems".
		////[XmlArrayAttribute("StreamHandlerItems")]
		//public StreamHandlerItem[] StreamHandlerItems;

		//public ObjectContainerItem[] ObjectContainerItems;

		//#endregion

		//protected internal override void Init()
		//{
		//	StreamHandlerItems = null;

		//	ObjectContainerItems = null;
		//}
	}

	public class AppConfigs : BaseConfigs<AppConfigs, AppConfigInfo>
	{
		static AppConfigs()
		{
			if (Runtime.IsWeb)
			{
				m_configManager = ConfigFileManager<AppConfigInfo>.Create("Web", "Config");
			}
			else
			{
				var asmx = AssemblyX.Create(Assembly.GetEntryAssembly());
				String type = asmx.Name + ".app.config";
				m_configManager = ConfigFileManager<AppConfigInfo>.Create(type, "Config");
			}
		}
	}
}