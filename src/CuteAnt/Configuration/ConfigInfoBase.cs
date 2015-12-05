using System;

using CuteAnt.Reflection;

namespace CuteAnt.Configuration
{
	public class ConfigInfoBase : DisposeBase
	{
		public ConfigInfoBase()
		{
			UsingXmlSerializer = true;
		}

		/// <summary>配置文件存储方式，分XML序列化和Dataset两种</summary>
		protected internal Boolean UsingXmlSerializer { get; set; }

		/// <summary>配置文件初始化</summary>
		protected internal virtual void Init()
		{
		}

		/// <summary>配置文件载入，只适用于DataSet方式</summary>
		protected internal virtual void LoadConfig(String file)
		{
			throw new NotImplementedException();
		}

		/// <summary>配置文件存储，只适用于DataSet方式</summary>
		protected internal virtual void SaveConfig(String file)
		{
			throw new NotImplementedException();
		}

		#region -- method TryGetConfig --

		private ConfigCustomItem[] _CustomItems;

		public ConfigCustomItem[] CustomItems
		{
			get { return _CustomItems; }
			set { _CustomItems = value; }
		}

		/// <summary>尝试获取指定名称的设置项</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Boolean TryGetConfig<T>(String name, out T value)
		{
			Object v = null;
			if (TryGetConfig(name, typeof(T), out v))
			{
				value = (T)v;
				return true;
			}
			value = default(T);
			return false;
		}

		/// <summary>尝试获取指定名称的设置项</summary>
		/// <param name="name">名称</param>
		/// <param name="type">类型</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Boolean TryGetConfig(String name, Type type, out Object value)
		{
			value = null;

			try
			{
				String str = TryGetConfig(name);
				if (str.IsNullOrWhiteSpace()) { return false; }
				TypeCode code = Type.GetTypeCode(type);
				if (code == TypeCode.String)
				{
					value = str;
				}
				else if (code == TypeCode.Int32)
				{
					value = Convert.ToInt32(str);
				}
				else if (code == TypeCode.Boolean)
				{
					Boolean b = false;
					if (str == "1" || str.EqualIgnoreCase(Boolean.TrueString))
					{
						value = true;
					}
					else if (str == "0" || str.EqualIgnoreCase(Boolean.FalseString))
					{
						value = false;
					}
					else if (Boolean.TryParse(str.ToLower(), out b))
					{
						value = b;
					}
				}
				else
				{
					value = TypeX.ChangeType(str, type);
				}
				return true;
			}
			catch { return false; }
		}

		protected virtual String TryGetConfig(String name)
		{
			if (_CustomItems != null && _CustomItems.Length > 0)
			{
				foreach (var item in _CustomItems)
				{
					if (item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					{
						return item.Value;
					}
				}
			}
			return null;
		}

		#endregion
	}
}