using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CuteAnt
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        array,
        assembly,
        buffer,
        destination,
        key,
        obj,
        s,
        d,
        str,
        source,
        callback,
        type,
        types,
        value,
        values,
        arrayPool,
        valueFactory,
        name,
        item,
        options,
        list,
        ts,
        other,
        comb,
        pool,
        paths,
        input,
        inner,
        policy,
        filenName,
        radix,
        offset,
        count,
        path,
        typeInfo,
        method,
        qualifiedTypeName,
        fullName,
        feature,
        manager,
        directories,
        dirEnumArgs,
        asm,
        includedAssemblies,
        func,
        defaultFn,
        returnType,
        propertyInfo,
        parameterTypes,
        fieldInfo,
        memberInfo,
        attributeType,
        pi,
        fi,
        invoker,
        instanceType,
        target,
        member,
        typeName,
        predicate,
        assemblyPredicate,
        collection,
        capacity,
        match,
        index,
        length,
        startIndex,
        updateValueFactory,
        converter,
        results,
        action,
        context,
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
