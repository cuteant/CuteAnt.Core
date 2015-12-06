using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.OrmLite.Entity
{
	public class Entity2<TEntity> : Entity<TEntity>
		where TEntity : Entity2<TEntity>, new()
	{
		private SimpleRecord _DynamicFields;

		public dynamic DynamicFields
		{
			get
			{
				if (_DynamicFields == null) { Interlocked.CompareExchange<SimpleRecord>(ref _DynamicFields, new SimpleRecord(), null); }
				return _DynamicFields;
			}
		}
	}
}
