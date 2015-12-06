﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>连接字符串构造器</summary>
	/// <remarks>未稳定，仅供CuteAnt.OrmLite内部使用，不建议外部使用</remarks>
	internal class HmDbConnectionStringBuilder : Dictionary<String, String>
	{
		#region -- 属性 --

		//private Dictionary<String, String> _Keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		///// <summary>已重载。</summary>
		//public override ICollection Keys
		//{
		//	get { return _Keys.Keys; }
		//}

		#region ## 苦竹 2012.11.06 AM03:04 ##

		private String _OptionalConnStrs;

		/// <summary>可选参数，手动在配置文件中输入</summary>
		public String OptionalConnStrs
		{
			get { return _OptionalConnStrs; }
			set { _OptionalConnStrs = value; }
		}

		#endregion

		#endregion

		#region -- 构造 --

		/// <summary>实例化不区分大小写的哈希集合</summary>
		public HmDbConnectionStringBuilder()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		#endregion

		#region -- 方法 --

		///// <summary>已重载。</summary>
		///// <param name="key"></param>
		///// <param name="value">数值</param>
		//public override void Add(string key, string value)
		//{
		//	base.Add(key, value);
		//	_Keys.Add(key, key.ToLowerInvariant());
		//}

		///// <summary>已重载。</summary>
		///// <param name="key"></param>
		//public override void Remove(string key)
		//{
		//	base.Remove(key);
		//	_Keys.Remove(key);
		//}

		///// <summary>已重载。</summary>
		//public override void Clear()
		//{
		//	base.Clear();
		//	_Keys.Clear();
		//}

		///// <summary>已重载。</summary>
		///// <param name="key"></param>
		///// <returns></returns>
		//public override String this[string key]
		//{
		//	get { return base[key]; }
		//	set
		//	{
		//		if (ContainsKey(key))
		//		{
		//			base[key] = value;
		//		}
		//		else
		//		{
		//			Add(key, value);
		//		}
		//	}
		//}

		#endregion

		#region -- 连接字符串 --

		/// <summary>连接字符串</summary>
		public String ConnectionString
		{
			get
			{
				if (Count <= 0) { return null; }

				var sb = new StringBuilder();
				foreach (var item in this)
				{
					if (sb.Length > 0) { sb.Append(";"); }
					sb.AppendFormat("{0}={1}", item.Key, item.Value);
				}
				if (!_OptionalConnStrs.IsNullOrWhiteSpace())
				{
					sb.Append(";");
					sb.Append(_OptionalConnStrs);
				}
				return sb.ToString();
			}
			set
			{
				Clear();
				if (value.IsNullOrWhiteSpace()) { return; }

				var kvs = value.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
				if (kvs == null || kvs.Length <= 0) { return; }

				foreach (var item in kvs)
				{
					Int32 p = item.IndexOf("=");

					// 没有等号，或者等号在第一位，都不合法
					if (p <= 0) { continue; }
					String name = item.Substring(0, p);
					String val = "";
					if (p < item.Length - 1)
					{
						val = item.Substring(p + 1);
					}
					Add(name.Trim(), val.Trim());
				}
			}
		}

		#endregion

		#region -- 方法 --

		///// <summary>尝试获取值</summary>
		///// <param name="key"></param>
		///// <param name="value">数值</param>
		///// <returns></returns>
		//public Boolean TryGetValue(String key, out String value)
		//{
		//	value = null;
		//	if (!ContainsKey(key)) { return false; }
		//	value = this[key];
		//	return true;
		//}

		/// <summary>获取并删除连接字符串中的项</summary>
		/// <param name="key"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Boolean TryGetAndRemove(String key, out String value)
		{
			value = null;
			if (!ContainsKey(key)) { return false; }
			value = this[key];
			Remove(key);
			return true;
		}

		#endregion

		#region -- 辅助 --

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return ConnectionString;
		}

		#endregion
	}
}