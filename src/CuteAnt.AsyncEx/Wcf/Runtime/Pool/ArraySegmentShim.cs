// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  ArraySegmentShim<T>
**
**
** Purpose: Convenient wrapper for an array, an offset, and
**          a count.  Ideally used in streams & collections.
**          Net Classes will consume an array of these.
**
**
===========================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace CuteAnt
{
  // Note: users should make sure they copy the fields out of an ArraySegmentShim onto their stack
  // then validate that the fields describe valid bounds within the array.  This must be done
  // because assignments to value types are not atomic, and also because one thread reading
  // three fields from an ArraySegmentShim may not see the same ArraySegmentShim from one call to another
  // (ie, users could assign a new value to the old location).
  [Serializable]
  // After .NET 4.5 RTMs, we can undo this and expose the full surface area to CoreCLR
  public struct ArraySegmentShim<T> : IList<T>, IReadOnlyList<T>
  {
    public static readonly ArraySegmentShim<T> Empty = default(ArraySegmentShim<T>);

    private T[] _array;
    private Int32 _offset;
    private Int32 _count;
    private Int32 _canReturn;

    public ArraySegmentShim(T[] array)
      : this(array, true)
    {
    }

    public ArraySegmentShim(T[] array, Boolean canReturn)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      Contract.EndContractBlock();

      _array = array;
      _offset = 0;
      _count = array.Length;
      _canReturn = canReturn ? 1 : 0;
    }

    public ArraySegmentShim(T[] array, int offset, int count)
      : this(array, offset, count, true)
    {
    }

    public ArraySegmentShim(T[] array, int offset, int count, Boolean canReturn)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
      if (array.Length - offset < count)
        throw new ArgumentException("Argument_InvalidOffLen");
      Contract.EndContractBlock();

      _array = array;
      _offset = offset;
      _count = count;
      _canReturn = canReturn ? 1 : 0;
    }

    public T[] Array
    {
      get
      {
        Contract.Assert((null == _array && 0 == _offset && 0 == _count)
                         || (null != _array && _offset >= 0 && _count >= 0 && _offset + _count <= _array.Length),
                        "ArraySegmentShim is invalid");

        return _array;
      }
    }

    public int Offset
    {
      get
      {
        // Since copying value types is not atomic & callers cannot atomically
        // read all three fields, we cannot guarantee that Offset is within
        // the bounds of Array.  That is our intent, but let's not specify
        // it as a postcondition - force callers to re-verify this themselves
        // after reading each field out of an ArraySegmentShim into their stack.
        Contract.Ensures(Contract.Result<int>() >= 0);

        Contract.Assert((null == _array && 0 == _offset && 0 == _count)
                         || (null != _array && _offset >= 0 && _count >= 0 && _offset + _count <= _array.Length),
                        "ArraySegmentShim is invalid");

        return _offset;
      }
    }

    public int Count
    {
      get
      {
        // Since copying value types is not atomic & callers cannot atomically
        // read all three fields, we cannot guarantee that Count is within
        // the bounds of Array.  That's our intent, but let's not specify
        // it as a postcondition - force callers to re-verify this themselves
        // after reading each field out of an ArraySegmentShim into their stack.
        Contract.Ensures(Contract.Result<int>() >= 0);

        Contract.Assert((null == _array && 0 == _offset && 0 == _count)
                          || (null != _array && _offset >= 0 && _count >= 0 && _offset + _count <= _array.Length),
                        "ArraySegmentShim is invalid");

        return _count;
      }
    }

    public Boolean CanReturn { get { return _canReturn == 1 && _array != null; } }

    public override int GetHashCode()
    {
      return null == _array
                  ? 0
                  : _array.GetHashCode() ^ _offset ^ _count ^ _canReturn;
    }

    public override bool Equals(Object obj)
    {
      if (obj is ArraySegmentShim<T>)
        return Equals((ArraySegmentShim<T>)obj);
      else
        return false;
    }

    public bool Equals(ArraySegmentShim<T> obj)
    {
      return obj._array == _array && obj._offset == _offset && obj._count == _count && obj._canReturn == _canReturn;
    }

    public static bool operator ==(ArraySegmentShim<T> a, ArraySegmentShim<T> b)
    {
      return a.Equals(b);
    }

    public static bool operator !=(ArraySegmentShim<T> a, ArraySegmentShim<T> b)
    {
      return !(a == b);
    }

    // After .NET 4.5 RTMs, we can undo this and expose the full surface area to CoreCLR

    #region IList<T>

    T IList<T>.this[int index]
    {
      get
      {
        if (_array == null)
          throw new InvalidOperationException("InvalidOperation_NullArray");
        if (index < 0 || index >= _count)
          throw new ArgumentOutOfRangeException("index");
        Contract.EndContractBlock();

        return _array[_offset + index];
      }

      set
      {
        if (_array == null)
          throw new InvalidOperationException("InvalidOperation_NullArray");
        if (index < 0 || index >= _count)
          throw new ArgumentOutOfRangeException("index");
        Contract.EndContractBlock();

        _array[_offset + index] = value;
      }
    }

    int IList<T>.IndexOf(T item)
    {
      if (_array == null)
        throw new InvalidOperationException("InvalidOperation_NullArray");
      Contract.EndContractBlock();

      int index = System.Array.IndexOf<T>(_array, item, _offset, _count);

      Contract.Assert(index == -1 ||
                      (index >= _offset && index < _offset + _count));

      return index >= 0 ? index - _offset : -1;
    }

    void IList<T>.Insert(int index, T item)
    {
      throw new NotSupportedException();
    }

    void IList<T>.RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region IReadOnlyList<T>

    T IReadOnlyList<T>.this[int index]
    {
      get
      {
        if (_array == null)
          throw new InvalidOperationException("InvalidOperation_NullArray");
        if (index < 0 || index >= _count)
          throw new ArgumentOutOfRangeException("index");
        Contract.EndContractBlock();

        return _array[_offset + index];
      }
    }

    #endregion IReadOnlyList<T>

    #region ICollection<T>

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        // the indexer setter does not throw an exception although IsReadOnly is true.
        // This is to match the behavior of arrays.
        return true;
      }
    }

    void ICollection<T>.Add(T item)
    {
      throw new NotSupportedException();
    }

    void ICollection<T>.Clear()
    {
      throw new NotSupportedException();
    }

    bool ICollection<T>.Contains(T item)
    {
      if (_array == null)
        throw new InvalidOperationException("InvalidOperation_NullArray");
      Contract.EndContractBlock();

      int index = System.Array.IndexOf<T>(_array, item, _offset, _count);

      Contract.Assert(index == -1 ||
                      (index >= _offset && index < _offset + _count));

      return index >= 0;
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      if (_array == null)
        throw new InvalidOperationException("InvalidOperation_NullArray");
      Contract.EndContractBlock();

      System.Array.Copy(_array, _offset, array, arrayIndex, _count);
    }

    bool ICollection<T>.Remove(T item)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region IEnumerable<T>

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      if (_array == null)
        throw new InvalidOperationException("InvalidOperation_NullArray");
      Contract.EndContractBlock();

      return new ArraySegmentEnumerator(this);
    }

    #endregion

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
      if (_array == null)
        throw new InvalidOperationException("InvalidOperation_NullArray");
      Contract.EndContractBlock();

      return new ArraySegmentEnumerator(this);
    }

    #endregion

    [Serializable]
    private sealed class ArraySegmentEnumerator : IEnumerator<T>
    {
      private T[] _array;
      private int _start;
      private int _end;
      private int _current;

      internal ArraySegmentEnumerator(ArraySegmentShim<T> arraySegment)
      {
        Contract.Requires(arraySegment.Array != null);
        Contract.Requires(arraySegment.Offset >= 0);
        Contract.Requires(arraySegment.Count >= 0);
        Contract.Requires(arraySegment.Offset + arraySegment.Count <= arraySegment.Array.Length);

        _array = arraySegment._array;
        _start = arraySegment._offset;
        _end = _start + arraySegment._count;
        _current = _start - 1;
      }

      public bool MoveNext()
      {
        if (_current < _end)
        {
          _current++;
          return (_current < _end);
        }
        return false;
      }

      public T Current
      {
        get
        {
          if (_current < _start) throw new InvalidOperationException("Enum not started"); // Environment.GetResourceString(ResId.InvalidOperation_EnumNotStarted)
          if (_current >= _end) throw new InvalidOperationException("Enum ended"); // Environment.GetResourceString(ResId.InvalidOperation_EnumEnded)
          return _array[_current];
        }
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      void IEnumerator.Reset()
      {
        _current = _start - 1;
      }

      public void Dispose()
      {
      }
    }
  }
}