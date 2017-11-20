using System;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Reflection
{
  /// <summary>ConstructorMatcher</summary>
  public class ConstructorMatcher<TInstance> : MethodBaseMatcher<ConstructorInfo>
  {
    #region @@ Fields @@

    public readonly CtorInvoker<TInstance> Invoker;

    public static readonly ConstructorMatcher<TInstance>[] ConstructorMatchers;

    #endregion

    #region @@ Constructors @@

    static ConstructorMatcher()
    {
      var thisType = typeof(TInstance);
      var emptyCtorMatchers = new ConstructorMatcher<TInstance>[] { new ConstructorMatcher<TInstance>(thisType.MakeDelegateForCtor<TInstance>()) };
      if (thisType.IsAbstract) { ConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance; return; }
      if (thisType == TypeConstants.StringType ||
          thisType.IsArray || thisType.IsInterface || thisType.IsGenericTypeDefinition)
      {
        ConstructorMatchers = emptyCtorMatchers;
        return;
      }
      try
      {

        ConstructorMatchers = thisType
            .GetTypeInfo().DeclaredConstructors
            .Where(_ => !_.IsStatic)
            .Select(_ => new ConstructorMatcher<TInstance>(_))
            .ToArray();
        if (ConstructorMatchers.Length == 0) { ConstructorMatchers = emptyCtorMatchers; }
      }
      catch { ConstructorMatchers = emptyCtorMatchers; }
    }

    internal ConstructorMatcher(ConstructorInfo constructor)
      : base(constructor)
    {
      Invoker = ReflectUtils.MakeDelegateForCtor<TInstance>(typeof(TInstance), Parameters.Select(_ => _.ParameterType).ToArray(), Value);
    }
    internal ConstructorMatcher(CtorInvoker<TInstance> invoker)
      : base(null)
    {
      Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
    }

    #endregion
  }
}
