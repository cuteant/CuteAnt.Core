using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace CuteAnt.OrmLite
{
	/// <summary>SimpleList</summary>
	/// <remarks></remarks>
	public class SimpleList : DynamicObject, IList<Object>
	{
		private readonly List<Object> _innerList;

		public Object this[Int32 index]
		{
			get { return _innerList[index]; }
			set { _innerList[index] = value; }
		}

		public Int32 Count
		{
			get { return _innerList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public SimpleList(IEnumerable<Object> other)
		{
			_innerList = new List<Object>(other);
		}

		public void Add(Object item)
		{
			_innerList.Add(item);
		}

		public void Clear()
		{
			_innerList.Clear();
		}

		public bool Contains(Object item)
		{
			return _innerList.Contains(item);
		}

		public void CopyTo(Object[] array, Int32 arrayIndex)
		{
			_innerList.CopyTo(array, arrayIndex);
		}

		public Int32 IndexOf(Object item)
		{
			return _innerList.IndexOf(item);
		}

		public void Insert(Int32 index, Object item)
		{
			_innerList.Insert(index, item);
		}

		public void RemoveAt(Int32 index)
		{
			_innerList.RemoveAt(index);
		}

		public bool Remove(Object item)
		{
			return _innerList.Remove(item);
		}

		public IEnumerator<Object> GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_innerList).GetEnumerator();
		}

		public dynamic ElementAt(Int32 index)
		{
			return _innerList.ElementAt(index);
		}

		public dynamic ElementAtOrDefault(Int32 index)
		{
			return _innerList.ElementAtOrDefault(index);
		}

		public dynamic First()
		{
			if (_innerList.Count == 0) throw new InvalidOperationException();
			return _innerList.First();
		}

		public dynamic FirstOrDefault()
		{
			return _innerList.FirstOrDefault();
		}

		public dynamic Last()
		{
			return _innerList.Last();
		}

		public dynamic LastOrDefault()
		{
			return _innerList.LastOrDefault();
		}

		public dynamic Single()
		{
			return _innerList.Single();
		}

		public dynamic SingleOrDefault()
		{
			return _innerList.SingleOrDefault();
		}

		public override bool TryConvert(ConvertBinder binder, out Object result)
		{
			if (ConcreteCollectionTypeCreator.IsCollectionType(binder.Type))
			{
				if (ConcreteCollectionTypeCreator.TryCreate(binder.Type, this, out result))
					return true;
			}

			return base.TryConvert(binder, out result);
		}
	}
}