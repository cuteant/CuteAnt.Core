/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Web.Caching;
using CuteAnt.Threading;

namespace CuteAnt.OrmLite.Cache
{
	/// <summary>实体依赖。用于HttpRuntime.Cache，一旦指定的实体类数据改变，马上让缓存过期。</summary>
	/// <typeparam name="TEntity"></typeparam>
	public class EntityDependency<TEntity> : CacheDependency where TEntity : Entity<TEntity>, new()
	{
		/// <summary>实例化一个实体依赖。</summary>
		public EntityDependency()
			: this(0)
		{
		}

		private TimerX timer = null;
		private Int64 count = 0;

		/// <summary>通过指定一个检查周期实例化一个实体依赖。
		/// 利用线程池定期去检查该实体类的总记录数，一旦改变则让缓存过期。
		/// 这样子就避免了其它方式修改数据而没能及时更新缓存问题
		/// </summary>
		/// <param name="period">检查周期，单位毫秒。必须大于1000（1秒），以免误用。</param>
		public EntityDependency(Int32 period)
		{
			var session = Entity<TEntity>.Meta.Session;
			session.OnDataChange += new Action<Type>(Meta_OnDataChange);

			if (period > 1000)
			{
				count = session.Count;
				timer = new TimerX(d => CheckCount(), null, period, period);
			}
		}

		private void Meta_OnDataChange(Type obj)
		{
			NotifyDependencyChanged(this, EventArgs.Empty);
		}

		private void CheckCount()
		{
			if (Entity<TEntity>.Meta.Session.Count != count)
			{
				NotifyDependencyChanged(this, EventArgs.Empty);

				if (timer != null) { timer.Dispose(); }
			}
		}

		/// <summary>释放资源</summary>
		protected override void DependencyDispose()
		{
			base.DependencyDispose();
			if (timer != null) { timer.Dispose(); }
		}
	}
}