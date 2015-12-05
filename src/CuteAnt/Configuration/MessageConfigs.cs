using System;
using System.Xml.Serialization;

namespace CuteAnt.Configuration
{
	/// <summary>网络配置, 加[Serializable]标记为可序列化</summary>
	[Serializable]
	[XmlRoot("configuration", IsNullable = false)]
	public class MessageConfigInfo : ConfigInfoBase
	{
		#region -- 属性 --

		private Boolean _IsMessageDebug = false;

		/// <summary>消息配置：是否启用消息调试，输出序列化过程。默认为不启用</summary>
		public Boolean IsMessageDebug
		{
			get { return _IsMessageDebug; }
			set { _IsMessageDebug = value; }
		}

		private Boolean _IsDumpStreamWhenError = false;

		/// <summary>消息配置：是否在出错时Dump数据流到文件中。默认为不启用</summary>
		public Boolean IsDumpStreamWhenError
		{
			get { return _IsDumpStreamWhenError; }
			set { _IsDumpStreamWhenError = value; }
		}

		#endregion

		#region -- 辅助方法 --

		#endregion
	}

	/// <summary>网络配置管理类</summary>
	public class MessageConfigs : BaseConfigs<MessageConfigs, MessageConfigInfo>
	{
		static MessageConfigs()
		{
			m_configManager = ConfigFileManager<MessageConfigInfo>.Create("Message", "Config");
		}
	}
}