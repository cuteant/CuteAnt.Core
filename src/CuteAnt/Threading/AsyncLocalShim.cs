using System;
#if NETSTANDARD || NET_4_5_GREATER
using System.Threading;
#else
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#endif

namespace CuteAnt
{
  /// <summary>AsyncLocalShim</summary>
  /// <typeparam name="T"></typeparam>
  public sealed class AsyncLocalShim<T>
  {
    private readonly Func<T> _defaultFn;
    private static T GetDefaultValueInternal() => default(T);

    /// <summary>Constructor</summary>
    public AsyncLocalShim()
    {
      _defaultFn = GetDefaultValueInternal;
    }

    /// <summary>Constructor</summary>
    /// <param name="defaultFn"></param>
    public AsyncLocalShim(Func<T> defaultFn)
    {
      if (defaultFn == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.defaultFn); }
      _defaultFn = defaultFn;
    }

#if NETSTANDARD || NET_4_5_GREATER
    private readonly AsyncLocal<T> _localValue = new AsyncLocal<T>();

    /// <summary>The current value</summary>
    public T Value
    {
      get
      {
        var current = _localValue.Value;
        if (current == null) { current = _defaultFn.Invoke(); }
        return current;
      }
      set { _localValue.Value = value; }
    }
#else
    private readonly string _currentKey = $"{nameof(T)}#{Guid.NewGuid().ToString("N")}";

    /// <summary>The current value</summary>
    public T Value
    {
      get
      {
        ObjectHandle objectHandle;
        try
        {
          objectHandle = CallContext.LogicalGetData(_currentKey) as ObjectHandle;
        }
        catch (NotImplementedException)
        {
          // Fixed in Mono master: https://github.com/mono/mono/pull/817
          objectHandle = CallContext.GetData(_currentKey) as ObjectHandle;
        }
        if (objectHandle == null) { return _defaultFn.Invoke(); }
        return (T)objectHandle.Unwrap();
      }
      set
      {
        var objHandle = new ObjectHandle(value);
        try
        {
          CallContext.LogicalSetData(_currentKey, objHandle);
        }
        catch (NotImplementedException)
        {
          // Fixed in Mono master: https://github.com/mono/mono/pull/817
          CallContext.SetData(_currentKey, objHandle);
        }
      }
    }
#endif

    /// <summary>clear</summary>
    public void Clear()
    {
#if NETSTANDARD || NET_4_5_GREATER
      _localValue.Value = default(T);
#else
      CallContext.FreeNamedDataSlot(_currentKey);
#endif
    }
  }
}
