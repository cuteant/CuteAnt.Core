using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CuteAnt
{
    public sealed class ReferenceEqualsComparer : EqualityComparer<object>
    {
        /// <summary>Gets an instance of this class.</summary>
        public static readonly ReferenceEqualsComparer Instance = new ReferenceEqualsComparer();

        private ReferenceEqualsComparer() { }

        /// <summary>Defines object equality by reference equality (eq, in LISP).</summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public override bool Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            return obj is null ? 0 : RuntimeHelpers.GetHashCode(obj);
        }
    }

    public sealed class ReferenceEqualsComparer<T> : IEqualityComparer<T> where T : class
    {
        /// <summary>
        /// Gets an instance of this class.
        /// </summary>
        public static readonly ReferenceEqualsComparer<T> Instance = new();

        /// <summary>
        /// Defines object equality by reference equality (eq, in LISP).
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public bool Equals(T x, T y) => object.ReferenceEquals(x, y);

        public int GetHashCode(T obj) => obj is null ? 0 : RuntimeHelpers.GetHashCode(obj);
    }
}