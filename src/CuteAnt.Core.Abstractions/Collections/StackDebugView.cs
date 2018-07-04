// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace CuteAnt.Collections
{
    internal sealed class StackDebugView<T>
    {
        private readonly StackX<T> _stack;

        public StackDebugView(StackX<T> stack)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            _stack = stack;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return _stack.ToArray();
            }
        }
    }
}
