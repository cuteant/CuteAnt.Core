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
    collection,
    capacity,
    match,
    index,
    length,
    startIndex,
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
  }

  #endregion
}
