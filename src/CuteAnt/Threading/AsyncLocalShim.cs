using System;
using System.Threading;

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

        /// <summary>clear</summary>
        public void Clear()
        {
            _localValue.Value = default(T);
        }
    }
}
