using System;
using System.Collections.Generic;

namespace CuteAnt
{
  public class ArraySegmentWrapperList : List<ArraySegmentWrapper<byte>>, IList<ArraySegment<byte>>
  {
    #region -- Constructors --

    public ArraySegmentWrapperList() : base() { }
    public ArraySegmentWrapperList(int capacity) : base(capacity) { }
    public ArraySegmentWrapperList(IEnumerable<ArraySegmentWrapper<byte>> collection) : base(collection) { }

    #endregion

    public void Add(ArraySegment<byte> item, bool canReturn)
    {
      this.Add(new ArraySegmentWrapper<byte>(item, canReturn));
    }

    public void Insert(int index, ArraySegment<byte> item, bool canReturn)
    {
      this.Insert(index, new ArraySegmentWrapper<byte>(item, canReturn));
    }

    public IList<ArraySegment<byte>> AsList()
    {
      return this;
    }

    public IList<ArraySegmentWrapper<byte>> AsWrapperList()
    {
      return this;
    }

    ArraySegment<byte> IList<ArraySegment<byte>>.this[int index]
    {
      get { return this[index].Segment; }
      set { this[index] = value; }
    }

    int ICollection<ArraySegment<byte>>.Count
    {
      get { return this.Count; }
    }

    bool ICollection<ArraySegment<byte>>.IsReadOnly
    {
      get { return false; }
    }

    void ICollection<ArraySegment<byte>>.Add(ArraySegment<byte> item)
    {
      this.Add(item);
    }

    void ICollection<ArraySegment<byte>>.Clear()
    {
      this.Clear();
    }

    //bool ICollection<ArraySegment<byte>>.Contains(ArraySegment<byte> item)
    //{
    //}

    // Contains returns true if the specified element is in the List.
    // It does a linear, O(n) search.  Equality is determined by calling
    // item.Equals().
    //
    public bool Contains(ArraySegment<byte> item)
    {
      //if ((Object)item == null)
      //{
      //  for (int i = 0; i < Count; i++)
      //  {
      //    if ((Object)this[i] == null) { return true; }
      //  }
      //  return false;
      //}
      //else
      //{
      var c = EqualityComparer<ArraySegment<byte>>.Default;
      for (int i = 0; i < Count; i++)
      {
        if (c.Equals(this[i].Segment, item)) { return true; }
      }
      return false;
      //}
    }

    void ICollection<ArraySegment<byte>>.CopyTo(ArraySegment<byte>[] array, int arrayIndex)
    {
      for (var i = 0; i < Count; i++)
      {
        array[arrayIndex + i] = this[i].Segment;
      }
    }

    IEnumerator<ArraySegment<byte>> IEnumerable<ArraySegment<byte>>.GetEnumerator()
    {
      foreach (var item in this)
      {
        yield return item.Segment;
      }
    }

    int IList<ArraySegment<byte>>.IndexOf(ArraySegment<byte> item)
    {
      throw new NotImplementedException();
    }

    void IList<ArraySegment<byte>>.Insert(int index, ArraySegment<byte> item)
    {
      this.Insert(index, item);
    }

    bool ICollection<ArraySegment<byte>>.Remove(ArraySegment<byte> item)
    {
      throw new NotImplementedException();
    }

    void IList<ArraySegment<byte>>.RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    private static bool IsCompatibleObject(object value)
    {
      // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
      // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
      return ((value is ArraySegment<byte>) || (value == null && default(ArraySegment<byte>) == null));
    }
  }
}
