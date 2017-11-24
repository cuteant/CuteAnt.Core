﻿using System;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Reflection
{
  /// <summary>ConstructorMatcher</summary>
  public class ConstructorMatcher<TInstance> : MethodBaseMatcher<ConstructorInfo>
  {
    #region @@ Fields @@

    public readonly CtorInvoker<TInstance> Invocation;

    public static readonly ConstructorMatcher<TInstance>[] ConstructorMatchers;
    public static readonly ConstructorMatcher<TInstance> Default;
    public static readonly CtorInvoker<TInstance> DefaultInvocation;

    internal static readonly ConstructorMatcher<TInstance>[] DIConstructorMatchers;
    #endregion

    #region @@ Constructors @@

    static ConstructorMatcher()
    {
      var thisType = typeof(TInstance);
      Default = new ConstructorMatcher<TInstance>(thisType.MakeDelegateForCtor<TInstance>());
      DefaultInvocation = Default.Invocation;

      var typeInfo = thisType.GetTypeInfo();
      if (typeInfo.IsAbstract)
      {
        DIConstructorMatchers = ConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance;
        return;
      }

      if (typeInfo.AsType() == TypeConstants.StringType || typeInfo.IsArray)
      {
        ConstructorMatchers = new ConstructorMatcher<TInstance>[] { Default };
        DIConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance;
        return;
      }

      try
      {
        var matchers = typeInfo
            .DeclaredConstructors
            .Where(_ => !_.IsStatic && _.IsPublic)
            .Select(_ => new ConstructorMatcher<TInstance>(_))
            .ToArray() ?? EmptyArray<ConstructorMatcher<TInstance>>.Instance;
        DIConstructorMatchers = ConstructorMatchers = matchers;
        if (ConstructorMatchers.Length == 0) { ConstructorMatchers = new ConstructorMatcher<TInstance>[] { Default }; }
      }
      catch
      {
        ConstructorMatchers = new ConstructorMatcher<TInstance>[] { Default };
        DIConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance;
      }
    }

    internal ConstructorMatcher(ConstructorInfo constructor)
      : base(constructor)
    {
      Invocation = ReflectUtils.MakeDelegateForCtor<TInstance>(typeof(TInstance), Parameters.Select(_ => _.ParameterType).ToArray(), Value);
    }
    internal ConstructorMatcher(CtorInvoker<TInstance> invoker)
      : base(null)
    {
      Invocation = invoker ?? throw new ArgumentNullException(nameof(invoker));
    }

    #endregion
  }
}
