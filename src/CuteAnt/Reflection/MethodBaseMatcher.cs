using System;
using System.Linq;
using System.Reflection;

namespace CuteAnt.Reflection
{
    public abstract class MethodBaseMatcher<TMethod> where TMethod : MethodBase
    {
        protected readonly ParameterInfo[] Parameters;
        public readonly TMethod Value;

        protected MethodBaseMatcher(TMethod method) : this(method, false) { }

        protected MethodBaseMatcher(TMethod method, bool throwOnError)
        {
            if (throwOnError && null == method) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method);
            Value = method;
            Parameters = method?.GetParameters() ?? EmptyArray<ParameterInfo>.Instance;
        }

        internal bool StrictMatch(Type[] argumentTypes)
        {
            if (argumentTypes is null) { argumentTypes = Type.EmptyTypes; }

            if (argumentTypes.Length != Parameters.Length) { return false; }

            // 所有参数类型都不为null，才能精确匹配
            if (argumentTypes.Length > 0 && argumentTypes.Any(_ => _ == null)) { return false; }

            for (var idx = 0; idx != argumentTypes.Length; idx++)
            {
                if (!Parameters[idx].ParameterType.IsAssignableFrom(argumentTypes[idx])) { return false; }
            }

            return true;
        }

        internal int Match(object[] givenParameters, out object[] parameterValues, out ParameterInfo[] paramInfos)
        {
            parameterValues = new object[Parameters.Length];
            paramInfos = Parameters;

            var applyIndexStart = 0;
            var applyExactLength = 0;

            for (var givenIndex = 0; givenIndex != givenParameters.Length; givenIndex++)
            {
                var givenType = givenParameters[givenIndex]?.GetType();
                var givenMatched = false;

                for (var applyIndex = applyIndexStart; givenMatched == false && applyIndex != Parameters.Length; ++applyIndex)
                {
                    if (parameterValues[applyIndex] is null &&
                        Parameters[applyIndex].ParameterType.IsAssignableFrom(givenType))
                    {
                        givenMatched = true;
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
