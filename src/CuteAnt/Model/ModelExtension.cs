/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;

namespace System
{
	/// <summary>模型扩展</summary>
	public static class ModelExtension
	{
		/// <summary>获取指定类型的服务对象</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static T GetService<T>(this IServiceProvider provider)
		{
			if (provider == null) { throw new ArgumentNullException("provider"); }
			return (T)provider.GetService(typeof(T));
		}
	}
}