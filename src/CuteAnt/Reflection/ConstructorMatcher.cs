using System;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Reflection
{
  public sealed class ConstructorMatcher : MethodBaseMatcher<ConstructorInfo>
  {
    #region @@ Fields @@

    public readonly CtorInvoker<object> Invocation;
    public readonly Type InstanceType;

    #endregion

    #region @@ Constructors @@

    internal ConstructorMatcher(Type instanceType, ConstructorInfo constructor)
      : base(constructor)
    {
      InstanceType = instanceType ?? throw new ArgumentNullException(nameof(instanceType));
      Invocation = ReflectUtils.MakeDelegateForCtor<object>(InstanceType, Parameters.Select(_ => _.ParameterType).ToArray(), Value);
    }
    internal ConstructorMatcher(Type instanceType, CtorInvoker<object> invoker)
      : base(null)
    {
      InstanceType = instanceType ?? throw new ArgumentNullException(nameof(instanceType));
      Invocation = invoker ?? throw new ArgumentNullException(nameof(invoker));
    }

    #endregion
  }
}
