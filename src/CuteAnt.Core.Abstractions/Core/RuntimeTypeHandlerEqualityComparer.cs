using System;
using System.Collections.Generic;

namespace CuteAnt
{
    public sealed class RuntimeTypeHandlerEqualityComparer : IEqualityComparer<RuntimeTypeHandle>
    {
        public static readonly RuntimeTypeHandlerEqualityComparer Instance = new RuntimeTypeHandlerEqualityComparer();

        private RuntimeTypeHandlerEqualityComparer() { }

        public int GetHashCode(RuntimeTypeHandle handle) => handle.GetHashCode();

        public bool Equals(RuntimeTypeHandle first, RuntimeTypeHandle second) => first.Equals(second);
    }
}
