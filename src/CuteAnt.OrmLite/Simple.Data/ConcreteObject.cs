using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CuteAnt.OrmLite
{
	internal sealed class ConcreteObject
	{
		private static readonly Object CastFailureObject = new Object();
		private WeakReference _concreteObject;

		internal Object Get(Type type, IDictionary<String, Object> data)
		{
			if (_concreteObject == null || !_concreteObject.IsAlive)
			{
				return ConvertAndCacheReference(type, data);
			}

			if (!ReferenceEquals(CastFailureObject, _concreteObject.Target))
			{
				return _concreteObject.Target;
			}

			return null;
		}

		private Object ConvertAndCacheReference(Type type, IDictionary<String, Object> data)
		{
			_concreteObject = null;
			Object result;
			if (ConcreteTypeCreator.Get(type).TryCreate(data, out result))
			{
				Interlocked.CompareExchange(ref _concreteObject, new WeakReference(result), null);
				return _concreteObject.Target;
			}

			Interlocked.CompareExchange(ref _concreteObject, new WeakReference(CastFailureObject), null);
			return null;
		}
	}
}
