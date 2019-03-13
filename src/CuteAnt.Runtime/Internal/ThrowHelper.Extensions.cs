namespace CuteAnt.Runtime
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
        str,
        source,
        type,
        types,
        value,
        values,
        valueFactory,
        name,
        item,
        options,
        list,
        ts,
        other,
        pool,
        inner,
        policy,
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
        arrayPool,
        index,
        sb,
        bufferManager,
        stream,
        bytes,
        charCount,
        segments,
        maxBufferPoolSize,
        maxBufferSize,
        bufferSize,
        size,
        chars,
        initialSize,
        sizeHint,
        initialCapacity,
    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
        ArgumentOutOfRange_NeedNonNegNum,
        ObjectDisposed_WriterClosed,
        Argument_InvalidOffLen,
        Lambda_Filter_Requires,
        Offset_Before_Start_Stream,
        Offset_Beyond_End_Stream,
        Segments_NeedNonNegNum,
        ValueMustBeNonNegative,
        Arg_SurrogatesNotAllowedAsSingleChar,
        BufferIsNotRightSizeForBufferManager,
    }

    #endregion
}
