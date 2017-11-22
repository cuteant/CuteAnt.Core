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

    internal bool StrictMatch(Type[] argumentTypes)
    {
      if (null == argumentTypes) { argumentTypes = Type.EmptyTypes; }

      if (argumentTypes.Length != Parameters.Length) { return false; }

      // 所有参数类型都不为null，才能精确匹配
      if (argumentTypes.Length > 0 && argumentTypes.Any(_ => _ == null)) { return false; }

      for (var idx = 0; idx != argumentTypes.Length; idx++)
      {
        if (!Parameters[idx].ParameterType.GetTypeInfo().IsAssignableFrom(argumentTypes[idx]?.GetTypeInfo())) { return false; }
      }

      return true;
    }

    internal int Match(object[] givenParameters, out object[] parameterValues, out bool[] parameterValuesSet, out ParameterInfo[] paramInfos)
    {
      parameterValuesSet = new bool[Parameters.Length];
      parameterValues = new object[Parameters.Length];
      paramInfos = Parameters;

      var applyIndexStart = 0;
      var applyExactLength = 0;

      for (var givenIndex = 0; givenIndex != givenParameters.Length; givenIndex++)
      {
#if NET40
        var givenType = givenParameters[givenIndex]?.GetType();
#else
        var givenType = givenParameters[givenIndex]?.GetType().GetTypeInfo();
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
            parameterValues[applyIndex] = givenParameters[givenIndex];
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
  }
}
