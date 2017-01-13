﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/
#if NET40
using System;

namespace CuteAnt
{
	/// <summary>弱引用</summary>
	/// <typeparam name="T"></typeparam>
	public class WeakReference<T> : WeakReference
	{
		/// <summary>实例化</summary>
		public WeakReference()
			: base(null)
		{
		}

		/// <summary>实例化</summary>
		/// <param name="target">目标对象</param>
		public WeakReference(T target)
			: base(target)
		{
		}

		/// <summary>实例化</summary>
		/// <param name="target">目标对象</param>
		/// <param name="trackResurrection"></param>
		public WeakReference(T target, Boolean trackResurrection)
			: base(target, trackResurrection)
		{
		}

		/// <summary>目标引用对象</summary>
		public new T Target
		{
			get { return (T)base.Target; }
			set { base.Target = value; }
		}

		/// <summary>尝试获取目标值</summary>
		/// <param name="target">目标对象</param>
		/// <returns></returns>
		public Boolean TryGetTarget(out T target)
		{
			target = Target;
			return IsAlive;
		}

		/// <summary>类型转换</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static implicit operator T(WeakReference<T> obj)
		{
			if (obj != null && obj.Target != null) { return obj.Target; }
			return default(T);
		}

		/// <summary>类型转换</summary>
		/// <param name="target">目标对象</param>
		/// <returns></returns>
		public static implicit operator WeakReference<T>(T target)
		{
			return new WeakReference<T>(target);
		}
	}
}
#endif