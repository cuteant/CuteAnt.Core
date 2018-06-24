namespace Grace
{
  #region -- ExceptionArgument --

  /// <summary>The convention for this enum is using the argument name as the enum name</summary>
  internal enum ExceptionArgument
  {
    action,
    array,
    assembly,
    buffer,
    destination,
    enumerable,
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
    iterateAction,
    range,
    condition,
    addAction,
    t,
    attributeType,
    consider,
    typeTest,
    context,
    parameterName,
    baseType,
    typeFilter,
    scopeName,
    ancestorType,
    KeyedTypeSelector,
    instance,
    scope,
    typeFilters,
    index,
    parameterInfo,
    paramValue,
    factory,
    secondaryStrategy,
    del,
    activationType,
    requestingScope,
    request,
    selector,
    module,
    instanceFunc,
    expression,
    apply,
    inspector,
    provider,
    activationStrategy,
    strategyProvider,
    conditionFunc,
    typeDelegate,
    interfaceType,
    keyedDelegate,
    exclude,
    method,
    lifestyleFunc,
    defaultValueFunc,
    filter,
    locateKey,
    constructorInfo,
    activationMethod,
    enrichmentDelegate,
    registrationAction,
    configuration,
  }

  #endregion

  #region -- ExceptionResource --

  /// <summary>The convention for this enum is using the resource name as the enum name</summary>
  internal enum ExceptionResource
  {
    ImmutableArray_Arrays_Not_Initialized,
    ImmutableArray_Not_Array,
    Provide_At_Least_One_Type,
    Value_Must_Not_Be_Null,
  }

  #endregion
}
