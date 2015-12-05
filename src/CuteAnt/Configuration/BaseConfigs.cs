using System;
using CuteAnt.IO;

namespace CuteAnt.Configuration
{
	public class BaseConfigs<TConfigs, TConfigInfo>
		where TConfigs : BaseConfigs<TConfigs, TConfigInfo>, new()
		where TConfigInfo : ConfigInfoBase, new()
	{
		protected static ConfigFileManager<TConfigInfo> m_configManager;

		#region -- 静态构造函数 --

		static BaseConfigs()
		{
			// 实例化一个对象，为了触发子类的静态构造函数
			TConfigs configs = new TConfigs();
		}

		#endregion

		/// <summary>配置文件是否存在</summary>
		/// <returns></returns>
		public static Boolean ConfigFileExist()
		{
			String file = FileHelper.FileExists(m_configManager.ConfigFile);
			return !file.IsNullOrWhiteSpace();
		}

		/// <summary>加载配置实例</summary>
		/// <returns></returns>
		public static TConfigInfo GetConfig()
		{
			return m_configManager.LoadConfig();
		}

		/// <summary>保存配置实例</summary>
		/// <returns></returns>
		public static void SaveConfig(TConfigInfo config)
		{
			m_configManager.SaveConfig(config);
		}

		/// <summary>保存配置实例</summary>
		public static void SaveConfig()
		{
			m_configManager.SaveConfig(default(TConfigInfo));
		}
	}
}