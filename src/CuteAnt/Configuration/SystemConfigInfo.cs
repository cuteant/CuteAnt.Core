using System;
using System.IO;
using System.Xml.Serialization;

namespace CuteAnt.Configuration
{
	/// <summary>系统配置, 加[Serializable]标记为可序列化</summary>
	[Serializable]
	[XmlRoot("configuration", IsNullable = false)]
	public class SystemConfigInfo : ConfigInfoBase
	{
		#region -- IO配置 --

		/// <summary>IO配置</summary>
		public class StreamHandlerItem
		{
			/// <summary>总线名称</summary>
			public String Name { get; set; }

			/// <summary>数据流处理器，多个处理器用竖线|隔开，前面的处理器比后面的先处理。</summary>
			public String Processor { get; set; }
		}

		#endregion

		#region -- 对象容器配置 --

		/// <summary>对象容器配置</summary>
		public class ObjectContainerItem
		{
			/// <summary>接口名称</summary>
			public String Interface { get; set; }

			/// <summary>标示</summary>
			public String Name { get; set; }

			/// <summary>类型名称</summary>
			public String Type { get; set; }

			//public Boolean IsSingleton { get; set; }

			/// <summary>优先级</summary>
			public Int32 Priority { get; set; }

			/// <summary>模式标记</summary>
			public String Mode { get; set; }
		}

		#endregion

		#region -- 属性 --

		private Boolean _IsDebug = false;

		/// <summary>是否启用全局调试。默认为不启用</summary>
		public Boolean IsDebug
		{
			get { return _IsDebug; }
			set { _IsDebug = value; }
		}

		private String _TempPath = "Temp4Hm" + Path.DirectorySeparatorChar;

		/// <summary>临时目录：默认为当前目录下的HmTemp文件夹。生产环境建议输出到站点外单独的HmTemp目录</summary>
		public String TempPath
		{
			get { return _TempPath; }
			set { _TempPath = value; }
		}

		private Boolean _IsThreadDebug = false;

		/// <summary>线程池配置：是否启用线程池调试。默认为不启用</summary>
		public Boolean IsThreadDebug
		{
			get { return _IsThreadDebug; }
			set { _IsThreadDebug = value; }
		}

		private Boolean _IsNetDebug = false;

		/// <summary>网络配置：是否启用网络调试。默认为不启用</summary>
		public Boolean IsNetDebug
		{
			get { return _IsNetDebug; }
			set { _IsNetDebug = value; }
		}

		// The XmlArrayAttribute changes the XML element name
		// from the default of "StreamHandlers" to "StreamHandlerItems".
		//[XmlArrayAttribute("StreamHandlerItems")]
		public StreamHandlerItem[] StreamHandlerItems;

		public ObjectContainerItem[] ObjectContainerItems;

		#endregion

		#region -- 辅助方法 --

		#endregion

		protected internal override void Init()
		{
			StreamHandlerItems = null;

			ObjectContainerItems = null;
		}
	}

	public class SystemConfigs : BaseConfigs<SystemConfigs, SystemConfigInfo>
	{
		static SystemConfigs()
		{
			m_configManager = ConfigFileManager<SystemConfigInfo>.Create("System", "Config");
		}
	}
}