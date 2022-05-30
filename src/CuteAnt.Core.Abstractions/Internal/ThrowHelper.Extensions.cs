using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CuteAnt
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        d,
        s,

        pi,
        fi,
        ts,

        asm,
        key,
        obj,
        str,

        comb,
        list,
        func,
        name,
        item,
        path,
        pool,
        type,

        array,
        other,
        paths,
        input,
        inner,
        types,
        value,
        radix,
        count,
        match,
        index,

        action,
        buffer,
        length,
        source,
        offset,
        values,
        method,
        policy,
        target,
        member,

        options,
        feature,
        manager,
        results,
        context,
        invoker,

        assembly,
        callback,
        fullName,
        capacity,
        typeInfo,
        typeName,

        arrayPool,
        filenName,
        defaultFn,
        predicate,
        fieldInfo,
        converter,

        collection,
        startIndex,
        memberInfo,
        returnType,

        destination,
        directories,
        dirEnumArgs,

        instanceType,
        propertyInfo,
        valueFactory,
        attributeType,
        parameterTypes,
        referenceCounter,
        qualifiedTypeName,
        assemblyPredicate,
        includedAssemblies,
        updateValueFactory,
    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
        Type_Name_Must_Not_Null,
        Capacity_May_Not_Be_Negative,
        Value_Cannot_Be_Null,
        Value_Is_Of_Incorrect_Type,
        Dest_Array_Cannot_Be_Null,
        ArgumentOutOfRange_Index,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_Count,
        Format_GuidDashes,
        Format_GuidInvLen,
        Format_GuidInvalidChar,
        Format_GuidUnrecognized,
        Format_InvalidGuidFormatSpecification,
    }

    #endregion

    partial class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentException GetArgumentException_InvalidCombGuid()
        {
            return new ArgumentException("value 类型不是 CombGuid");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_GuidInvLen_D()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("d 的长度不是 8 个字节。");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_GuidInvLen()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("value 的长度不是 16 个字节。");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static NotSupportedException GetNotSupportedException(MemberInfo memberInfo)
        {
            return new NotSupportedException($"Not supported for MemberInfo of type: {memberInfo.GetType().Name}");
        }
    }
}
