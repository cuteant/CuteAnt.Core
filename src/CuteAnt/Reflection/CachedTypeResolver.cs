using System;

namespace CuteAnt.Reflection
{
    internal class CachedTypeResolver : ITypeResolver
    {
        /// <inheritdoc />
        public Type ResolveType(string name)
        {
            return TypeUtils.ResolveType(name);
        }

        /// <inheritdoc />
        public bool TryResolveType(string name, out Type type)
        {
            return TypeUtils.TryResolveType(name, out type);
        }
    }
}
