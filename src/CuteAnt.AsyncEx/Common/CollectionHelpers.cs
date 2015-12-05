using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.AsyncEx
{
  internal static class CollectionHelpers
  {
    internal static IReadOnlyCollection<T> ReifyCollection<T>(IEnumerable<T> source)
    {
      var result = source as IReadOnlyCollection<T>;
      if (result != null) { return result; }
      var collection = source as ICollection<T>;
      if (collection != null) { return new CollectionWrapper<T>(collection); }
      var nongenericCollection = source as ICollection;
      if (nongenericCollection != null) { return new NongenericCollectionWrapper<T>(nongenericCollection); }

#if NET_4_0_GREATER
      return new List<T>(source);
#else
      return System.Collections.Immutable.ImmutableList.CreateRange(source);
#endif
    }

    private sealed class NongenericCollectionWrapper<T> : IReadOnlyCollection<T>
    {
      private readonly ICollection _collection;

      internal NongenericCollectionWrapper(ICollection collection)
      {
        _collection = collection;
      }

      public int Count
      {
        get { return _collection.Count; }
      }

      public IEnumerator<T> GetEnumerator()
      {
        foreach (T item in _collection)
        {
          yield return item;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _collection.GetEnumerator();
      }
    }

    private sealed class CollectionWrapper<T> : IReadOnlyCollection<T>
    {
      private readonly ICollection<T> _collection;

      internal CollectionWrapper(ICollection<T> collection)
      {
        _collection = collection;
      }

      public int Count
      {
        get { return _collection.Count; }
      }

      public IEnumerator<T> GetEnumerator()
      {
        return _collection.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _collection.GetEnumerator();
      }
    }
  }
}