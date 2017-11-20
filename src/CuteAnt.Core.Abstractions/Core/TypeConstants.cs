using System;
using System.Collections.Generic;

namespace CuteAnt
{
  public static class TypeConstants
  {
    #region -- 常用类型 --

    /// <summary>类型</summary>
    public static readonly Type TypeType = typeof(Type);

    /// <summary>值类型</summary>
    public static readonly Type ValueType = typeof(ValueType);

    /// <summary>枚举类型</summary>
    public static readonly Type EnumType = typeof(Enum);

    /// <summary>对象类型</summary>
    public static readonly Type ObjectType = typeof(Object);
    public static readonly Type ObjectArrayType = typeof(Object[]);

    /// <summary>字符串类型</summary>
    public static readonly Type StringType = typeof(String);

    /// <summary>Guid</summary>
    public static readonly Type GuidType = typeof(Guid);

    /// <summary>CombGuid</summary>
    public static readonly Type CombGuidType = typeof(CombGuid);

    /// <summary>Guid</summary>
    public static readonly Type ByteArrayType = typeof(byte[]);

    public static readonly Type ArrayType = typeof(Array);

    public static readonly Type BoolType = typeof(bool);

    public static readonly Type GenericCollectionType = typeof(ICollection<>);

    public static readonly Type ByteType = typeof(byte);

    public static readonly Type SByteType = typeof(sbyte);

    public static readonly Type CharType = typeof(char);

    public static readonly Type ShortType = typeof(short);

    public static readonly Type UShortType = typeof(ushort);

    public static readonly Type IntType = typeof(int);

    public static readonly Type UIntType = typeof(uint);

    public static readonly Type LongType = typeof(long);

    public static readonly Type ULongType = typeof(ulong);

    public static readonly Type FloatType = typeof(float);

    public static readonly Type DoubleType = typeof(double);

    public static readonly Type DecimalType = typeof(decimal);

    public static readonly Type ExceptionType = typeof(Exception);

    public static readonly Type NullableType = typeof(Nullable<>);

    public static readonly Type VoidType = typeof(void);

    #endregion
  }
}