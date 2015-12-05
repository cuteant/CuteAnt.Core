/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System.Reflection;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
	/// <summary>事件扩展</summary>
	public class EventInfoX : MemberInfoX
	{
		#region -- 属性 --

		private EventInfo _Event;

		/// <summary>事件</summary>
		public EventInfo Event
		{
			get { return _Event; }
			set { _Event = value; }
		}

		#endregion

		#region -- 构造 --

		private EventInfoX(EventInfo ev)
			: base(ev)
		{
			Event = ev;
		}

		private static DictionaryCache<EventInfo, EventInfoX> cache = new DictionaryCache<EventInfo, EventInfoX>();

		/// <summary>创建</summary>
		/// <param name="ev"></param>
		/// <returns></returns>
		public static EventInfoX Create(EventInfo ev)
		{
			if (ev == null) { return null; }
			return cache.GetItem(ev, delegate(EventInfo key)
			{
				return new EventInfoX(key);
			});
		}

		#endregion
	}
}