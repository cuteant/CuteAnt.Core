﻿using System;
using Grace.Data;

namespace Grace.DependencyInjection.Conditions
{
  /// <summary>Condition for testing if a strategy is being injected into another</summary>
  public class WhenInjectedInto : ICompiledCondition
  {
    private readonly Func<Type, bool> _typeTest;

    /// <summary>Default constructor takes list of types</summary>
    /// <param name="types"></param>
    public WhenInjectedInto(params Type[] types)
    {
      if (types == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.types);
      _typeTest = type => TestTypes(type, types);
    }

    /// <summary>Constructor that takes func to test with instead of array of types</summary>
    /// <param name="typeTest"></param>
    public WhenInjectedInto(Func<Type, bool> typeTest)
    {
      if (typeTest == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.typeTest);
      _typeTest = typeTest;
    }

    /// <summary>Test if being injected into a specific type</summary>
    /// <param name="strategy">strategy to test</param>
    /// <param name="staticInjectionContext">static injection context</param>
    /// <returns></returns>
    public bool MeetsCondition(IActivationStrategy strategy, StaticInjectionContext staticInjectionContext)
    {
      var targetInfo = staticInjectionContext.TargetInfo;

      return targetInfo?.InjectionType != null && _typeTest(targetInfo.InjectionType);
    }

    /// <summary>Tests for if one type is based on another</summary>
    /// <param name="injectionType"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    protected bool TestTypes(Type injectionType, Type[] types)
    {
      foreach (var type in types)
      {
        if (ReflectionService.CheckTypeIsBasedOnAnotherType(injectionType, type))
        {
          return true;
        }
      }

      return false;
    }
  }
}