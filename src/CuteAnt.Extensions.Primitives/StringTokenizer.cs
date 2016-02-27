﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace CuteAnt.Extensions.Primitives
{
    /// <summary>
    /// Tokenizes a <c>string</c> into <see cref="StringSegment"/>s.
    /// </summary>
    public struct StringTokenizer :  IEnumerable<StringSegment>
    {
        private readonly string _value;
        private readonly char[] _separators;

        /// <summary>
        /// Initializes a new instance of <see cref="StringTokenizer"/>.
        /// </summary>
        /// <param name="value">The <c>string</c> to tokenize.</param>
        /// <param name="separators">The characters to tokenize by.</param>
        public StringTokenizer(string value, char[] separators)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            if (separators == null)
            {
                throw new ArgumentNullException(nameof(separators));
            }

            _value = value;
            _separators = separators;
        }

        public Enumerator GetEnumerator() => new Enumerator(ref this);

        IEnumerator<StringSegment> IEnumerable<StringSegment>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<StringSegment>
        {
            private readonly string _value;
            private readonly char[] _separators;
            private int _index;

            public Enumerator(ref StringTokenizer tokenizer)
            {
                _value = tokenizer._value;
                _separators = tokenizer._separators;
                Current = default(StringSegment);
                _index = 0;
            }

            public StringSegment Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_value == null || _index > _value.Length)
                {
                    Current = default(StringSegment);
                    return false;
                }

                var next = _value.IndexOfAny(_separators, _index);
                if (next == -1)
                {
                    // No separator found. Consume the remainder of the string.
                    next = _value.Length;
                }

                Current = new StringSegment(_value, _index, next - _index);
                _index = next + 1;

                return true;
            }

            public void Reset()
            {
                Current = default(StringSegment);
                _index = 0;
            }
        }
    }
}
