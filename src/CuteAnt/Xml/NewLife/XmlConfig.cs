﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.IO;
using System.Xml.Serialization;
using CuteAnt.Log;

namespace CuteAnt.Xml
{
	/// <summary>Xml配置文件基类</summary>
	/// <remarks>
	/// 标准用法：TConfig.Current
	///
	/// 配置实体类通过<see cref="XmlConfigFileAttribute"/>特性指定配置文件路径以及自动更新时间。
	/// Current将加载配置文件，如果文件不存在或者加载失败，将实例化一个对象返回。
	///
	/// 考虑到自动刷新，不提供LoadFile和SaveFile等方法，可通过扩展方法ToXmlFileEntity和ToXmlFile实现。
	///
	/// 用户也可以通过配置实体类的静态构造函数修改基类的<see cref="_.ConfigFile"/>和<see cref="_.ReloadTime"/>来动态配置加载信息。
	/// </remarks>
	/// <typeparam name="TConfig"></typeparam>
	public class XmlConfig<TConfig> where TConfig : XmlConfig<TConfig>, new()
	{
		#region 静态

		private static TConfig _Current;

		/// <summary>当前实例。通过置空可以使其重新加载。</summary>
		public static TConfig Current
		{
			get
			{
				// 这里要小心，避免_Current的null判断完成后，_Current被别人置空，而导致这里返回null
				var config = _Current;
				if (config != null)
				{
					// 现存有对象，尝试再次加载，可能因为未修改而返回null，这样只需要返回现存对象即可
					if (!config.IsUpdated) return config;
					HmTrace.WriteInfo("{0}的配置文件{1}有更新，重新加载配置！", typeof(TConfig), config.ConfigFile);

					var cfg = Load(_.ConfigFile);
					if (cfg == null) return config;

					_Current = cfg;
					return cfg;
				}
				else
				{
					// 现在没有对象，尝试加载，若返回null则实例化一个新的
					config = Load(_.ConfigFile);
					if (config == null)
					{
						config = new TConfig();
						config.ConfigFile = _.ConfigFile.GetFullPath();
						config.SetExpire();  // 设定过期时间
						config.OnLoaded();

						// 创建或覆盖
						HmTrace.WriteInfo("{0}的配置文件{1}不存在或加载出错，准备用默认配置覆盖！", typeof(TConfig).Name, _.ConfigFile);
						try
						{
							// 根据配置，有可能不保存，直接返回默认
							if (_.SaveNew) config.Save();
						}
						catch (Exception ex)
						{
							HmTrace.WriteException(ex);
						}
					}
					_Current = config;
					return config;
				}
			}
			set { _Current = value; }
		}

		/// <summary>一些设置。派生类可以在自己的静态构造函数中指定</summary>
		public static class _
		{
			private static String _ConfigFile;

			/// <summary>配置文件路径</summary>
			public static String ConfigFile { get { return _ConfigFile; } set { _ConfigFile = value; } }

			private static Int32 _ReloadTime;

			/// <summary>重新加载时间。单位：毫秒</summary>
			public static Int32 ReloadTime { get { return _ReloadTime; } set { _ReloadTime = value; } }

			private static Boolean _SaveNew = true;

			/// <summary>没有配置文件时是否保存新配置。默认true</summary>
			public static Boolean SaveNew { get { return _SaveNew; } set { _SaveNew = value; } }

			private static Boolean _CheckFormat = true;

			/// <summary>是否检查配置文件格式，当格式不一致是保存新格式配置文件。默认true</summary>
			public static Boolean CheckFormat { get { return _CheckFormat; } set { _CheckFormat = value; } }

			static _()
			{             // 获取XmlConfigFileAttribute特性，那里会指定配置文件名称
				var att = typeof(TConfig).GetCustomAttributeX<XmlConfigFileAttribute>(true);
				if (att == null || att.FileName.IsNullOrWhiteSpace())
				{
					// 这里不能着急，派生类可能通过静态构造函数指定配置文件路径
					//throw new XException("编码错误！请为配置类{0}设置{1}特性，指定配置文件！", typeof(TConfig), typeof(XmlConfigFileAttribute).Name);
				}
				else
				{
					_.ConfigFile = att.FileName;
					_.ReloadTime = att.ReloadTime;
				}

				// 实例化一次，用于触发派生类中可能的静态构造函数
				var config = new TConfig();
			}
		}

		//static XmlConfig()
		//{
		//    // 实例化一次，用于触发派生类中可能的静态构造函数
		//    //var config = new TConfig();
		//    var config = Current;

		//    var filename = _.ConfigFile.GetFullPath();
		//    if (!filename.IsNullOrWhiteSpace() && File.Exists(filename))
		//    {
		//        // 如果默认加载后的配置与保存的配置不一致，说明可能配置实体类已变更，需要强制覆盖
		//        if (_.CheckFormat) config.CheckFormat(filename);
		//    }
		//}

		#endregion

		#region 属性

		[NonSerialized]
		private String _ConfigFile;

		/// <summary>配置文件</summary>
		[XmlIgnore]
		public String ConfigFile { get { return _ConfigFile; } set { _ConfigFile = value; } }

		/// <summary>最后写入时间</summary>
		[XmlIgnore]
		private DateTime lastWrite;

		/// <summary>过期时间。如果在这个时间之后再次访问，将检查文件修改时间</summary>
		[XmlIgnore]
		private DateTime expire;

		/// <summary>是否已更新</summary>
		[XmlIgnore]
		protected Boolean IsUpdated
		{
			get
			{
				var now = DateTime.Now;
				if (_.ReloadTime > 0 && expire < now)
				{
					var fi = new FileInfo(ConfigFile);
					fi.Refresh();
					expire = now.AddMilliseconds(_.ReloadTime);

					if (lastWrite < fi.LastWriteTime)
					{
						lastWrite = fi.LastWriteTime;
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>设置过期重新加载配置的时间</summary>
		private void SetExpire()
		{
			if (_.ReloadTime > 0)
			{
				// 这里必须在加载后即可设置过期时间和最后写入时间，否则下一次访问的时候，IsUpdated会报告文件已更新
				var fi = new FileInfo(ConfigFile);
				if (fi.Exists)
				{
					fi.Refresh();
					lastWrite = fi.LastWriteTime;
				}
				else
				{
					lastWrite = DateTime.Now;
				}
				expire = DateTime.Now.AddMilliseconds(_.ReloadTime);
			}
		}

		#endregion

		#region 加载

		/// <summary>加载指定配置文件</summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static TConfig Load(String filename)
		{
			if (filename.IsNullOrWhiteSpace()) return null;
			filename = filename.GetFullPath();
			if (!File.Exists(filename)) return null;

			try
			{
				//var config = filename.ToXmlFileEntity<TConfig>();

				/*
				 * 初步现象：在不带sp的.Net 2.0中，两种扩展方法加泛型的写法都会导致一个诡异异常
				 * System.BadImageFormatException: 试图加载格式不正确的程序
				 *
				 * 经过多次尝试，不用扩展方法也不行，但是不用泛型可以！
				 */

				TConfig config = null;
				using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					//config = stream.ToXmlEntity<TConfig>();
					config = stream.ToXmlEntity(typeof(TConfig)) as TConfig;
				}
				if (config == null) return null;

				config.ConfigFile = filename;
				config.SetExpire();  // 设定过期时间
				config.OnLoaded();

				return config;
			}
			catch (Exception ex) { HmTrace.WriteException(ex); return null; }
		}

		#endregion

		#region 成员方法

		/// <summary>从配置文件中读取完成后触发</summary>
		protected virtual void OnLoaded()
		{
			// 如果默认加载后的配置与保存的配置不一致，说明可能配置实体类已变更，需要强制覆盖
			if (_.CheckFormat) CheckFormat();
		}

		/// <summary>是否检查配置文件格式，当格式不一致是保存新格式配置文件</summary>
		protected virtual void CheckFormat()
		{
			var config = this;
			try
			{
				var flag = File.Exists(ConfigFile);
				if (flag)
				{
					var xml1 = File.ReadAllText(ConfigFile);
					var xml2 = config.ToXml(null, "", "", true, true);
					flag = xml1 == xml2;
				}
				if (!flag)
				{
					HmTrace.WriteInfo("配置文件{0}格式不一致，保存为最新格式！", ConfigFile);
					config.Save();
				}
			}
			catch (Exception ex)
			{
				HmTrace.WriteException(ex);
			}
		}

		/// <summary>保存到配置文件中去</summary>
		/// <param name="filename"></param>
		public virtual void Save(String filename)
		{
			//var filename = _.ConfigFile;
			if (filename.IsNullOrWhiteSpace()) filename = ConfigFile;
			if (filename.IsNullOrWhiteSpace()) throw new HmExceptionBase("未指定{0}的配置文件路径！", typeof(TConfig).Name);
			filename = filename.GetFullPath();

			// 加锁避免多线程保存同一个文件冲突
			lock (this)
			{
				this.ToXmlFile(filename, null, "", "", true, true);
			}
		}

		/// <summary>保存到配置文件中去</summary>
		public virtual void Save() { Save(null); }

		#endregion
	}
}