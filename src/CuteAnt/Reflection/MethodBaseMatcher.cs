using System;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Reflection
{
  public abstract class MethodBaseMatcher<TMethod> where TMethod : MethodBase
  {
    protected readonly ParameterInfo[] Parameters;
    public readonly TMethod Value;

    protected MethodBaseMatcher(TMethod method)
      : this(method, false) { }
    protected MethodBaseMatcher(TMethod method, bool throwOnError)
    {
      if (throwOnError && null == method) { throw new ArgumentNullException(nameof(method)); }
      Value = method;
      Parameters = method?.GetParameters() ?? EmptyArray<ParameterInfo>.Instance;
    }

    internal int Match(Type[] argumentTypes)
    {
      var applyIndexStart = 0;
      var applyExactLength = 0;
      var parameterValuesSet = new bool[Parameters.Length];
      for (var givenIndex = 0; givenIndex != argumentTypes.Length; givenIndex++)
      {
#if NET40
        var givenType = argumentTypes[givenIndex];
#else
        var givenType = argumentTypes[givenIndex]?.GetTypeInfo();
#endif
        var givenMatched = false;

        for (var applyIndex = applyIndexStart; givenMatched == false && applyIndex != Parameters.Length; ++applyIndex)
        {
          if (parameterValuesSet[applyIndex] == false &&
#if NET40
              Parameters[applyIndex].ParameterType.IsAssignableFrom(givenType))
#else
              Parameters[applyIndex].ParameterType.GetTypeInfo().IsAssignableFrom(givenType))
#endif
          {
            givenMatched = true;
            parameterValuesSet[applyIndex] = true;
            if (applyIndexStart == applyIndex)
            {
              applyIndexStart++;
              if (applyIndex == givenIndex)
              {
                applyExactLength = applyIndex;
              }
            }
          }
        }

        if (givenMatched == false) { return -1; }
      }
      return applyExactLength;
    }

    internal int Match(object[] givenParameters)
    {
      if (null == givenParameters || givenParameters.Length == 0)
      {
        return Match(Type.EmptyTypes);
      }
      return Match(givenParameters.Select(_ => _?.GetType()).ToArray());
    }
  }
}
