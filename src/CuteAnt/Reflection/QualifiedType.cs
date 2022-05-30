using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CuteAnt.Reflection
{
    /// <summary>Represents an assembly-qualifies type.</summary>
    public readonly struct QualifiedType : IEquatable<QualifiedType>
    {
        internal readonly string AssemblyName;
        internal readonly string TypeName;

        public QualifiedType(string assemblyName, string typeName)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
        }

        public override int GetHashCode()
        {
#if NETSTANDARD2_0
            var comparer = StringComparer.Ordinal;
            return (AssemblyName is not null ? comparer.GetHashCode(AssemblyName) : 0) ^
                   (TypeName is not null ? comparer.GetHashCode(TypeName) : 0);
#else
            return HashCode.Combine(AssemblyName, TypeName);
#endif
        }

        public override bool Equals(object obj)
        {
            if (obj is QualifiedType nameKey) { return Equals(nameKey); }
            return false;
        }

        public bool Equals(QualifiedType other)
        {
            return (string.Equals(AssemblyName, other.AssemblyName) &&
                    string.Equals(TypeName, other.TypeName));
        }
    }

    public sealed class QualifiedTypeComparer : IComparer, IEqualityComparer, IComparer<QualifiedType>, IEqualityComparer<QualifiedType>
    {
        /// <summary>Default</summary>
        public static readonly QualifiedTypeComparer Default;

        private static readonly StringComparer s_stringComparer;

        static QualifiedTypeComparer()
        {
            Default = new QualifiedTypeComparer();
            s_stringComparer = StringComparer.Ordinal;
        }

#region -- IComparer Members --

        /// <inheritdoc />
        public int Compare(object x, object y)
        {
            if (x == y) { return 0; }
            if (x == null) { return -1; }
            if (y == null) { return 1; }

            if (x is QualifiedType obj1 && y is QualifiedType obj2)
            {
                return Compare(obj1, obj2);
            }
            return ThrowArgumentException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int ThrowArgumentException()
        {
            throw GetArgumentException();

            ArgumentException GetArgumentException()
            {
                return new ArgumentException("类型不是 TypeNameKey");
            }
        }

#endregion

#region -- IComparer<TypeNameKey> Members --

        /// <inheritdoc />
        public int Compare(QualifiedType x, QualifiedType y)
        {
            var v = s_stringComparer.Compare(x.TypeName, y.TypeName);
            if (0u >= (uint)v)
            {
                return s_stringComparer.Compare(x.AssemblyName, y.AssemblyName);
            }
            return v;
        }

#endregion

#region -- IEqualityComparer --

        /// <inheritdoc />
        public new bool Equals(object x, object y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;

            if (x is QualifiedType key)
            {
                return key.Equals(y);
            }
            return x.Equals(y);
        }

        /// <inheritdoc />
        public int GetHashCode(object obj)
        {
            if (obj == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj); }

            if (obj is QualifiedType) { return ((QualifiedType)obj).GetHashCode(); }

            return obj.GetHashCode();
        }

#endregion

#region -- IEqualityComparer<TypeNameKey> Members --

        /// <inheritdoc />
        public bool Equals(QualifiedType x, QualifiedType y)
        {
            return s_stringComparer.Equals(x.AssemblyName, y.AssemblyName) &&
                   s_stringComparer.Equals(x.TypeName, y.TypeName);
        }

        /// <inheritdoc />
        public int GetHashCode(QualifiedType obj) => obj.GetHashCode();

#endregion
    }
}
