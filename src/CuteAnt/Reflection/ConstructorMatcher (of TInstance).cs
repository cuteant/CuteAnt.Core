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
    public static readonly ConstructorMatcher<TInstance> Default;
    public static readonly CtorInvoker<TInstance> DefaultInvoker;

    #endregion

    #region @@ Constructors @@

    static ConstructorMatcher()
    {
      var thisType = typeof(TInstance);
      var typeInfo = thisType.GetTypeInfo();
      if (typeInfo.IsAbstract) { ConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance; return; }

      Default = new ConstructorMatcher<TInstance>(typeInfo.AsType().MakeDelegateForCtor<TInstance>());
      DefaultInvoker = Default.Invoker;
      var defaultCtorMatchers = new ConstructorMatcher<TInstance>[] { Default };

      if (typeInfo.AsType() == TypeConstants.StringType ||
          typeInfo.IsArray || typeInfo.IsInterface || typeInfo.IsGenericTypeDefinition)
      {
        ConstructorMatchers = defaultCtorMatchers;
        return;
      }
      try
      {

        ConstructorMatchers = typeInfo
            .DeclaredConstructors
            .Where(_ => !_.IsStatic)
            .Select(_ => new ConstructorMatcher<TInstance>(_))
            .ToArray();
        if (ConstructorMatchers.Length == 0) { ConstructorMatchers = defaultCtorMatchers; }
      }
      catch { ConstructorMatchers = defaultCtorMatchers; }
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
