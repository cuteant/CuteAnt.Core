using System;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx
{
  public class MultiTaskCompletionSource
  {
    private readonly TaskCompletionSource<bool> _tcs;
    private int _count;
    private readonly object _lockable;

    public MultiTaskCompletionSource(int count)
    {
      if (count <= 0)
      {
        throw new ArgumentOutOfRangeException("count", "count has to be positive.");
      }
      _tcs = new TaskCompletionSource<bool>();
      this._count = count;
      _lockable = new object();
    }

    public Task Task
    {
      get { return _tcs.Task; }
    }

    public void SetOneResult()
    {
      lock (_lockable)
      {
        if (_count <= 0)
        {
          throw new InvalidOperationException("SetOneResult was called more times than initialy specified by the count argument.");
        }
        _count--;
        if (_count == 0)
        {
          _tcs.SetResult(true);
        }
      }
    }

    public void SetMultipleResults(int num)
    {
      lock (_lockable)
      {
        if (num <= 0)
        {
          throw new ArgumentOutOfRangeException("num", "num has to be positive.");
        }
        if (_count - num < 0)
        {
          throw new ArgumentOutOfRangeException("num", "num is too large, count - num < 0.");
        }
        _count = _count - num;
        if (_count == 0)
        {
          _tcs.SetResult(true);
        }
      }
    }
  }
}
