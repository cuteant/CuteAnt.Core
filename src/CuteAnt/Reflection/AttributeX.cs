/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CuteAnt.Collections;
using CuteAnt.Reflection;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace System
{
	/// <summary>特性辅助类</summary>
	public static class AttributeX
	{
		#region 静态方法

		private static DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>> _miCache = new DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>>();
		private static DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>> _miCache2 = new DictionaryCache<MemberInfo, DictionaryCache<Type, Attribute[]>>();
		private static readonly Attribute[] _emptyAttributes = new Attribute[0];

		/// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
		/// <param name="member"></param>
		/// <param name="attributeType"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<Attribute> GetCustomAttributesX(this MemberInfo member, Type attributeType = null, Boolean inherit = true)
		{
			if (member == null) { return _emptyAttributes; }

			var micache = _miCache;
			if (!inherit) { micache = _miCache2; }

			if (attributeType == null) { attributeType = typeof(Attribute); }

			// 二级字典缓存
			var cache = micache.GetItem(member, m => new DictionaryCache<Type, Attribute[]>());
			var atts = cache.GetItem<MemberInfo, Boolean>(attributeType, member, inherit, (t, m, inh) =>
			{
				return Attribute.GetCustomAttributes(m, t, inh);
			});

			return atts != null ? atts : _emptyAttributes;
		}

		/// <summary>获取自定义特性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="member"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this MemberInfo member, Boolean inherit = true)
			where TAttribute : Attribute
		{
			var atts = GetCustomAttributesX(member, typeof(TAttribute), inherit);

			return atts.Any() ? (IEnumerable<TAttribute>)atts : new TAttribute[0];
		}

		/// <summary>获取自定义属性</summary>
		/// <param name="member"></param>
		/// <param name="attributeType"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
		public static Attribute GetCustomAttributeX(this MemberInfo member, Type attributeType, Boolean inherit = true)
		{
			var atts = GetCustomAttributesX(member, attributeType, false);
			if (atts.Any()) return atts.First();

			if (inherit)
			{
				atts = GetCustomAttributesX(member, attributeType, inherit);
				return atts.FirstOrDefault();
			}

			return default(Attribute);
		}

		/// <summary>获取自定义属性</summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="member"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TAttribute GetCustomAttributeX<TAttribute>(this MemberInfo member, Boolean inherit = true)
			where TAttribute : Attribute
		{
			var att = GetCustomAttributeX(member, typeof(TAttribute), inherit);
			return att as TAttribute;
		}

		private static DictionaryCache<String, Attribute[]> _asmCache = new DictionaryCache<String, Attribute[]>();

		/// <summary>获取自定义属性，带有缓存功能，避免因.Net内部GetCustomAttributes没有缓存而带来的损耗</summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="assembly"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static IEnumerable<TAttribute> GetCustomAttributesX<TAttribute>(this Assembly assembly)
			where TAttribute : Attribute
		{
			if (assembly == null) { return new TAttribute[0]; }

			var key = String.Format("{0}_{1}", assembly.FullName, typeof(TAttribute).FullName);

			return (IEnumerable<TAttribute>)_asmCache.GetItem<Assembly>(key, assembly, (k, m) =>
			{
				var atts = Attribute.GetCustomAttributes(m, typeof(TAttribute));
				return atts == null ? new TAttribute[0] : atts;
			});
		}

		/// <summary>获取自定义属性</summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <param name="assembly"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TAttribute GetCustomAttributeX<TAttribute>(this Assembly assembly)
			where TAttribute : Attribute
		{
			var avs = GetCustomAttributesX<TAttribute>(assembly);
			return avs.FirstOrDefault();
		}

		/// <summary>获取自定义属性的值。可用于ReflectionOnly加载的程序集</summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TResult GetCustomAttributeValue<TAttribute, TResult>(this Assembly target)
			where TAttribute : Attribute
		{
			if (target == null) return default(TResult);

			var list = CustomAttributeData.GetCustomAttributes(target);
			if (list == null || list.Count < 1) return default(TResult);

			foreach (var item in list)
			{
				if (typeof(TAttribute) != item.Constructor.DeclaringType) continue;

				var args = item.ConstructorArguments;
				if (args != null && args.Count > 0) return (TResult)args[0].Value;
			}

			return default(TResult);
		}

		/// <summary>获取自定义属性的值。可用于ReflectionOnly加载的程序集</summary>
		/// <typeparam name="TAttribute"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="target">目标对象</param>
		/// <param name="inherit">是否递归</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TResult GetCustomAttributeValue<TAttribute, TResult>(this MemberInfo target, Boolean inherit = true)
			where TAttribute : Attribute
		{
			if (target == null) return default(TResult);

			try
			{
				var list = CustomAttributeData.GetCustomAttributes(target);
				if (list != null && list.Count > 0)
				{
					foreach (var item in list)
					{
						if (!TypeX.Equal(typeof(TAttribute), item.Constructor.DeclaringType)) continue;

						var args = item.ConstructorArguments;
						if (args != null && args.Count > 0) return (TResult)args[0].Value;
					}
				}
				if (inherit && target is Type)
				{
					target = (target as Type).BaseType;
					if (target != null && target != typeof(Object))
						return GetCustomAttributeValue<TAttribute, TResult>(target, inherit);
				}
			}
			catch
			{
				// 出错以后，如果不是仅反射加载，可以考虑正面来一次
				if (!target.Module.Assembly.ReflectionOnly)
				{
					var att = GetCustomAttributeX<TAttribute>(target, inherit);
					if (att != null)
					{
						var pi = typeof(TAttribute).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(TResult));
						if (pi != null) return (TResult)att.GetValue(pi);
					}
				}
			}

			return default(TResult);
		}

		#endregion 静态方法
	}
}