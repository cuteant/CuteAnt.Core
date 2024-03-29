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
            var emtpyConstructor = thisType.GetEmptyConstructor();
            Default = emtpyConstructor is not null
                    ? new ConstructorMatcher<TInstance>(emtpyConstructor)
                    : new ConstructorMatcher<TInstance>(thisType.MakeDelegateForCtor<TInstance>());
            DefaultInvocation = Default.Invocation;

            if (thisType.IsAbstract)
            {
                ConstructorMatchers = new ConstructorMatcher<TInstance>[] { Default };
                DIConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance;
                return;
            }

            if (thisType == TypeConstants.StringType || thisType.IsArray)
            {
                ConstructorMatchers = new ConstructorMatcher<TInstance>[] { Default };
                DIConstructorMatchers = EmptyArray<ConstructorMatcher<TInstance>>.Instance;
                return;
            }

            try
            {
                var matchers = thisType.GetTypeInfo()
                    .DeclaredConstructors
                    .Where(_ => !_.IsStatic && _.IsPublic)
                    .Select(_ => new ConstructorMatcher<TInstance>(_))
                    .ToArray() ?? EmptyArray<ConstructorMatcher<TInstance>>.Instance;
                DIConstructorMatchers = ConstructorMatchers = matchers;

                if (0u >= (uint)ConstructorMatchers.Length)
                {
                    ConstructorMatchers = new ConstructorMatcher<TInstance>[] { Default };
                }
                else if (emtpyConstructor is null)
                {
                    ConstructorMatchers = ConstructorMatchers.Concat(new ConstructorMatcher<TInstance>[] { Default }).ToArray();
                }
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
            if (invoker is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.invoker);
            Invocation = invoker;
        }

        #endregion
    }
}
