using System;
using System.IO;
using CuteAnt.Collections;
using CuteAnt.IO;

namespace CuteAnt.Configuration
{
	public class ConfigFileManager<TConfig> where TConfig : ConfigInfoBase, new()
	{
		/// <summary>锁对象</summary>
		private object m_lockHelper = new object();

		/// <summary>配置文件修改时间</summary>
		private DateTime m_fileOldChangeTime;

		private TConfig m_configInfo;
		private readonly String m_configType;
		private readonly String m_configPath;

		public readonly String ConfigFile;

		#region -- 构造 --

		/// <summary>初始化/载入配置信息，如果配置文件不存在则创建包含默认配置信息的配置文件</summary>
		/// <param name="type">配置文件类型，也是配置文件存储名称，默认扩展名为.config</param>
		/// <param name="path">配置文件存储路径</param>
		private ConfigFileManager(String type, String path)
		{
			m_configType = type;
			m_configPath = path;
			String fileName = type.Contains(".") ? type : type + ".config";
			ConfigFile = PathHelper.ApplicationStartupPathCombine(path, fileName);
			String file = FileHelper.FileExists(ConfigFile);
			m_configInfo = new TConfig();
			m_configInfo.Init();
			if (file.IsNullOrWhiteSpace())
			{
				SerializeInfo();
			}
			else
			{
				DeserializeInfo();
			}
			m_fileOldChangeTime = File.GetLastWriteTime(ConfigFile);
		}

		private static DictionaryCache<String, ConfigFileManager<TConfig>> cache = new DictionaryCache<String, ConfigFileManager<TConfig>>();

		/// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
		/// <param name="type"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static ConfigFileManager<TConfig> Create(String type, String path)
		{
			String key = type.ToLower();
			return cache.GetItem<String, String>(key, type, path, (k, t, p) => new ConfigFileManager<TConfig>(t, p));
		}

		#endregion

		#region -- 序列化 --

		/// <summary>加载(反序列化)指定对象类型的配置对象</summary>
		/// <param name="checkTime">是否检查并更新传递进来的"文件加载时间"变量</param>
		/// <returns></returns>
		public TConfig LoadConfig(Boolean checkTime = true)
		{
			if (checkTime)
			{
				DateTime fileNewChangeTime = File.GetLastWriteTime(ConfigFile);

				//当程序运行中config文件发生变化时则对config重新赋值
				if (m_fileOldChangeTime != fileNewChangeTime)
				{
					m_fileOldChangeTime = fileNewChangeTime;
					DeserializeInfo();
				}
			}
			else
			{
				DeserializeInfo();
			}
			return m_configInfo as TConfig;
		}

		/// <summary>保存(序列化)指定对象类型的配置对象</summary>
		public void SaveConfig(TConfig config)
		{
			if (config != null)
			{
				m_configInfo = config;
			}
			SerializeInfo();
			m_fileOldChangeTime = File.GetLastWriteTime(ConfigFile);
		}

		#endregion

		#region -- Private Methods --

		private void DeserializeInfo()
		{
			if (m_configInfo.UsingXmlSerializer)
			{
				lock (m_lockHelper)
				{
					m_configInfo = SerializationHelper.LoadXMLFile<TConfig>(ConfigFile);
				}
			}
			else
			{
				lock (m_lockHelper)
				{
					m_configInfo.LoadConfig(ConfigFile);
				}
			}
		}

		private void SerializeInfo()
		{
			FileInfo fi = new FileInfo(ConfigFile);
			if (!fi.Directory.Exists)
			{
				fi.Directory.Create();
			}
			if (m_configInfo.UsingXmlSerializer)
			{
				lock (m_lockHelper)
				{
					SerializationHelper.SaveAsXML(m_configInfo, ConfigFile);
				}
			}
			else
			{
				lock (m_lockHelper)
				{
					m_configInfo.SaveConfig(ConfigFile);
				}
			}
		}

		#endregion
	}
}