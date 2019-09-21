using System;
using System.Runtime.CompilerServices;

namespace CuteAnt.Collections
{
    partial class StackX<T>
    {
        public bool IsEmpty
        {
            [MethodImpl(InlineMethod.Value)]
            get { return _size <= 0; }
        }

        public bool NonEmpty
        {
            [MethodImpl(InlineMethod.Value)]
            get { return _size > 0; }
        }

        #region -- TryPopIf --

        public bool TryPopIf(Predicate<T> predicate, out T result)
        {
            int size = _size - 1;
            T[] array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                result = default;
                return false;
            }

            result = array[size];
            if (!predicate(result)) { return false; }

            _version++;
            _size = size;
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                array[size] = default;     // Free memory quicker.
            }
#else
            array[size] = default;     // Free memory quicker.
#endif
            return true;
        }

        #endregion

        #region -- ForEach --

        public void ForEach(Action<T> action)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var idx = _size - 1;
            while (idx >= 0)
            {
                action(_array[idx]);
                idx--;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void ForEach<TArg>(Action<T, TArg> action, TArg arg)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var idx = _size - 1;
            while (idx >= 0)
            {
                action(_array[idx], arg);
                idx--;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void ForEach<TArg1, TArg2>(Action<T, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var idx = _size - 1;
            while (idx >= 0)
            {
                action(_array[idx], arg1, arg2);
                idx--;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void ForEach(Action<T, int> action)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var index = 0;
            var idx = _size - 1;
            while (idx >= 0)
            {
                action(_array[idx], index);
                idx--;
                index++;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void ForEach<TArg>(Action<T, int, TArg> action, TArg arg)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var index = 0;
            var idx = _size - 1;
            while (idx >= 0)
            {
                action(_array[idx], index, arg);
                idx--;
                index++;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void ForEach<TArg1, TArg2>(Action<T, int, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var index = 0;
            var idx = _size - 1;
            while (idx >= 0)
            {
                action(_array[idx], index, arg1, arg2);
                idx--;
                index++;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        #endregion

        #region -- Reverse --

        public void Reverse(Action<T> action)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var idx = 0;
            while (idx < _size)
            {
                action(_array[idx]);
                idx++;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void Reverse<TArg>(Action<T, TArg> action, TArg arg)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var idx = 0;
            while (idx < _size)
            {
                action(_array[idx], arg);
                idx++;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void Reverse<TArg1, TArg2>(Action<T, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var idx = 0;
            while (idx < _size)
            {
                action(_array[idx], arg1, arg2);
                idx++;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }


        public void Reverse(Action<T, int> action)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var index = _size - 1; ;
            var idx = 0;
            while (idx < _size)
            {
                action(_array[idx], index);
                idx++;
                index--;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void Reverse<TArg>(Action<T, int, TArg> action, TArg arg)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var index = _size - 1; ;
            var idx = 0;
            while (idx < _size)
            {
                action(_array[idx], index, arg);
                idx++;
                index--;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        public void Reverse<TArg1, TArg2>(Action<T, int, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (null == action) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var version = _version;

            var index = _size - 1; ;
            var idx = 0;
            while (idx < _size)
            {
                action(_array[idx], index, arg1, arg2);
                idx++;
                index--;
            }

            if (version != _version) { ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion(); }
        }

        #endregion

        #region -- TrueForAll --

        public bool TrueForAll(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            if (IsEmpty) { return false; }

            var idx = _size - 1;
            while (idx >= 0)
            {
                if (!match(_array[idx])) { return false; }
                idx--;
            }
            return true;
        }

        #endregion

        #region ** ThrowHelper **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException_Action()
        {
            throw GetArgumentNullException();

            ArgumentNullException GetArgumentNullException()
            {
                return new ArgumentNullException("action");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion()
        {
            throw GetInvalidOperationException();
            InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion); ;
            }
        }

        #endregion

        #region ** SR **

        static class SR
        {
            internal const string InvalidOperation_EmptyStack = "Stack empty.";

            internal const string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";

            internal const string InvalidOperation_EnumNotStarted = "Enumeration has not started. Call MoveNext.";

            internal const string InvalidOperation_EnumEnded = "Enumeration already finished.";

            internal const string Argument_InvalidArrayType = "Target array type is not compatible with the type of items in the collection.";

            internal const string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";

            internal const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";

            internal const string Arg_NonZeroLowerBound = "The lower bound of target array must be zero.";

            internal const string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";

            internal const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
        }

        #endregion
    }
}
