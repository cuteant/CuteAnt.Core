// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Internal;

namespace Microsoft.Extensions.Logging
{
    partial class LoggerMessageFactory
    {
#if NET40
        private class LogValues : IList<KeyValuePair<string, object>>
#else
        private class LogValues : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues)state)._formatter.Format(((LogValues)state).ToArray());

            private static object[] _valueArray = new object[0];

            private readonly LogValuesFormatter _formatter;

            public LogValues(LogValuesFormatter formatter)
            {
                _formatter = formatter;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public int Count
            {
                get
                {
                    return 1;
                }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                yield return this[0];
            }

            public object[] ToArray() => _valueArray;

            public override string ToString() => _formatter.Format(ToArray());

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0>)state)._formatter.Format(((LogValues<T0>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            private readonly T0 _value0;

            public LogValues(LogValuesFormatter formatter, T0 value0)
            {
                _formatter = formatter;
                _value0 = value0;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                    }
                    else if (index == 1)
                    {
                        return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public int Count
            {
                get
                {
                    return 2;
                }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            public object[] ToArray() => new object[] { _value0 };

            public override string ToString() => _formatter.Format(ToArray());

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1>)state)._formatter.Format(((LogValues<T0, T1>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            private readonly T0 _value0;
            private readonly T1 _value1;

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public int Count
            {
                get
                {
                    return 3;
                }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            public object[] ToArray() => new object[] { _value0, _value1 };

            public override string ToString() => _formatter.Format(ToArray());

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2>)state)._formatter.Format(((LogValues<T0, T1, T2>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;

            public int Count
            {
                get
                {
                    return 4;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3>)state)._formatter.Format(((LogValues<T0, T1, T2, T3>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;

            public int Count
            {
                get
                {
                    return 5;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;

            public int Count
            {
                get
                {
                    return 6;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4, T5> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4, T5> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4, T5>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4, T5>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;
            public T5 _value5;

            public int Count
            {
                get
                {
                    return 7;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[5], _value5);
                        case 6:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4, _value5 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4, T5, T6> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4, T5, T6> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4, T5, T6>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4, T5, T6>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;
            public T5 _value5;
            public T6 _value6;

            public int Count
            {
                get
                {
                    return 8;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[5], _value5);
                        case 6:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[6], _value6);
                        case 7:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
                _value6 = value6;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4, _value5, _value6 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4, T5, T6, T7>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4, T5, T6, T7>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;
            public T5 _value5;
            public T6 _value6;
            public T7 _value7;

            public int Count
            {
                get
                {
                    return 9;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[5], _value5);
                        case 6:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[6], _value6);
                        case 7:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[7], _value7);
                        case 8:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
                _value6 = value6;
                _value7 = value7;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4, _value5, _value6, _value7 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;
            public T5 _value5;
            public T6 _value6;
            public T7 _value7;
            public T8 _value8;

            public int Count
            {
                get
                {
                    return 10;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[5], _value5);
                        case 6:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[6], _value6);
                        case 7:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[7], _value7);
                        case 8:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[8], _value8);
                        case 9:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
                _value6 = value6;
                _value7 = value7;
                _value8 = value8;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4, _value5, _value6, _value7, _value8 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;
            public T5 _value5;
            public T6 _value6;
            public T7 _value7;
            public T8 _value8;
            public T9 _value9;

            public int Count
            {
                get
                {
                    return 11;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[5], _value5);
                        case 6:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[6], _value6);
                        case 7:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[7], _value7);
                        case 8:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[8], _value8);
                        case 9:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[9], _value9);
                        case 10:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
                _value6 = value6;
                _value7 = value7;
                _value8 = value8;
                _value9 = value9;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4, _value5, _value6, _value7, _value8, _value9 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }

#if NET40
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IList<KeyValuePair<string, object>>
#else
        private class LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            public static Func<object, Exception, string> Callback = (state, exception) => ((LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)state)._formatter.Format(((LogValues<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)state).ToArray());

            private readonly LogValuesFormatter _formatter;
            public T0 _value0;
            public T1 _value1;
            public T2 _value2;
            public T3 _value3;
            public T4 _value4;
            public T5 _value5;
            public T6 _value6;
            public T7 _value7;
            public T8 _value8;
            public T9 _value9;
            public T10 _value10;

            public int Count
            {
                get
                {
                    return 12;
                }
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[0], _value0);
                        case 1:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[1], _value1);
                        case 2:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[2], _value2);
                        case 3:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[3], _value3);
                        case 4:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[4], _value4);
                        case 5:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[5], _value5);
                        case 6:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[6], _value6);
                        case 7:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[7], _value7);
                        case 8:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[8], _value8);
                        case 9:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[9], _value9);
                        case 10:
                            return new KeyValuePair<string, object>(_formatter.ValueNames[10], _value10);
                        case 11:
                            return new KeyValuePair<string, object>("{OriginalFormat}", _formatter.OriginalFormat);
                        default:
                            throw new IndexOutOfRangeException(nameof(index));
                    }
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public LogValues(LogValuesFormatter formatter, T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
            {
                _formatter = formatter;
                _value0 = value0;
                _value1 = value1;
                _value2 = value2;
                _value3 = value3;
                _value4 = value4;
                _value5 = value5;
                _value6 = value6;
                _value7 = value7;
                _value8 = value8;
                _value9 = value9;
                _value10 = value10;
            }

            public object[] ToArray() => new object[] { _value0, _value1, _value2, _value3, _value4, _value5, _value6, _value7, _value8, _value9, _value10 };

            public override string ToString() => _formatter.Format(ToArray());

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }
    }
}

