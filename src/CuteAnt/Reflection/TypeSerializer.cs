using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
    public static class TypeSerializer
    {
        // The default concurrency level is DEFAULT_CONCURRENCY_MULTIPLIER * #CPUs. The higher the
        // DEFAULT_CONCURRENCY_MULTIPLIER, the more concurrent writes can take place without interference
        // and blocking, but also the more expensive operations that require all locks become (e.g. table
        // resizing, ToArray, Count, etc). According to brief benchmarks that we ran, 4 seems like a good
        // compromise.
        private const Int32 DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        /// <summary>The number of concurrent writes for which to optimize by default.</summary>
        private static Int32 DefaultConcurrencyLevel => DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount;

        private static readonly ConcurrentDictionary<Type, TypeKey> _typeCache =
            new ConcurrentDictionary<Type, TypeKey>(DefaultConcurrencyLevel, DictionaryCacheConstants.SIZE_MEDIUM);

        private static readonly ConcurrentDictionary<int, (TypeKey Key, Type Type)> _typeKeyCache =
            new ConcurrentDictionary<int, (TypeKey, Type)>();

        private static readonly Func<Type, TypeKey> _getTypeKey =
            type => new TypeKey(StringHelper.UTF8NoBOM.GetBytes(RuntimeTypeNameFormatter.Format(type)));

        public static TypeKey GetTypeKeyFromType(Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            return _typeCache.GetOrAdd(type, _getTypeKey);
        }

        public static unsafe bool TryGetType(in TypeKey key, out Type type)
        {
            // Search through 
            var hashCode = key.HashCode;
            var typeNameBytes = key.TypeName;
            var candidateHashCode = hashCode;
            while (_typeKeyCache.TryGetValue(candidateHashCode, out var entry))
            {
                var existingKey = entry.Key;
                if (existingKey.HashCode != hashCode) { break; }

                if (existingKey.TypeName.AsSpan().SequenceEqual(key.TypeName))
                {
                    type = entry.Type;
                    return true;
                }

                // Try the next entry.
                ++candidateHashCode;
            }

            // Allocate a string for the type name.
            string typeNameString = Encoding.UTF8.GetString(typeNameBytes);

            if (TypeUtils.TryResolveType(typeNameString, out type))
            {
                while (!_typeKeyCache.TryAdd(candidateHashCode++, (key, type)))
                {
                    // Insert the type at the first available position.
                }

                return true;
            }

            return false;
        }
    }

    /// <summary>Represents a named type for the purposes of serialization.</summary>
    public readonly struct TypeKey
    {
        public readonly int HashCode;

        public readonly byte[] TypeName;

        public TypeKey(int hashCode, byte[] key)
        {
            this.HashCode = hashCode;
            this.TypeName = key;
        }

        public TypeKey(byte[] key)
        {
            this.HashCode = unchecked((int)JenkinsHash.ComputeHash(key));
            this.TypeName = key;
        }

        public TypeKey(string typeName) : this(StringHelper.UTF8NoBOM.GetBytes(typeName)) { }

        public string GetTypeName() => Encoding.UTF8.GetString(this.TypeName);

        public bool Equals(TypeKey other) => IsEquals(this, other);

        private static bool IsEquals(in TypeKey x, in TypeKey y)
        {
            if (x.HashCode != y.HashCode) { return false; }
            var a = x.TypeName;
            var b = y.TypeName;
            if (ReferenceEquals(a, b)) { return true; }
            return a.AsSpan().SequenceEqual(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeKey other)
            {
                return IsEquals(this, other);
            }
            return false;
        }

        public override int GetHashCode() => this.HashCode;

        public override string ToString() => $"TypeName \"{Encoding.UTF8.GetString(TypeName)}\" (hash {HashCode:X8})";

        internal sealed class Comparer : IEqualityComparer<TypeKey>
        {
            public bool Equals(TypeKey x, TypeKey y) => IsEquals(x, y);

            public int GetHashCode(TypeKey obj) => obj.HashCode;
        }
    }
}