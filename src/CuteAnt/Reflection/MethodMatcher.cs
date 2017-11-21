using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace CuteAnt.Reflection
{
  /// <summary>MethodMatcher</summary>
  internal sealed class MethodMatcher : MethodBaseMatcher<MethodInfo>
  {
    public readonly MethodCaller<object, object> Invocation;

    public MethodMatcher(MethodInfo method)
      : base(method, true)
    {
      Invocation = method.MakeDelegateForCall();
    }
  }

  /// <summary>MethodMatcher</summary>
  internal class MethodMatcher<TTarget, TReturn> : MethodBaseMatcher<MethodInfo>
  {
    public readonly MethodCaller<TTarget, TReturn> Invocation;

    public MethodMatcher(MethodInfo method)
      : base(method, true)
    {
      Invocation = method.MakeDelegateForCall<TTarget, TReturn>();
    }

    private static Dictionary<MethodInfo, MethodMatcher<TTarget, TReturn>> s_methodInvocationCache =
        new Dictionary<MethodInfo, MethodMatcher<TTarget, TReturn>>();

    public static MethodMatcher<TTarget, TReturn> GetMethodMatcher(MethodInfo method)
    {
      if (method == null) { throw new ArgumentNullException(nameof(method)); }

      if (s_methodInvocationCache.TryGetValue(method, out var defaultValue)) return defaultValue;

      defaultValue = new MethodMatcher<TTarget, TReturn>(method);

      Dictionary<MethodInfo, MethodMatcher<TTarget, TReturn>> snapshot, newCache;
      do
      {
        snapshot = s_methodInvocationCache;
        newCache = new Dictionary<MethodInfo, MethodMatcher<TTarget, TReturn>>(s_methodInvocationCache)
        {
          [method] = defaultValue
        };
      } while (!ReferenceEquals(Interlocked.CompareExchange(ref s_methodInvocationCache, newCache, snapshot), snapshot));

      return defaultValue;
    }
  }
}
