﻿using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Grace.DependencyInjection.Exceptions;
using Grace.Utilities;

namespace Grace.DependencyInjection.Lifestyle
{
    /// <summary>Singleton per a named scope</summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplayValue) + ",nq}")]
    public class SingletonPerNamedScopeLifestyle : ICompiledLifestyle
    {
        private readonly string _scopeName;

        /// <summary>Unique id</summary>
        protected readonly string UniqueId = UniqueStringId.Generate();

        /// <summary>Compiled delegate</summary>
        protected ActivationStrategyDelegate CompiledDelegate;

        /// <summary>Default constructor</summary>
        /// <param name="scopeName"></param>
        public SingletonPerNamedScopeLifestyle(string scopeName) => _scopeName = scopeName;

        /// <summary>Generalization for lifestyle</summary>
        public LifestyleType LifestyleType { get; } = LifestyleType.Scoped;

        /// <summary>Clone the lifestyle</summary>
        /// <returns></returns>
        public ICompiledLifestyle Clone() => new SingletonPerNamedScopeLifestyle(_scopeName);

        /// <summary>Provide an expression that uses the lifestyle</summary>
        /// <param name="scope">scope for the strategy</param>
        /// <param name="request">activation request</param>
        /// <param name="activationExpression">expression to create strategy type</param>
        /// <returns></returns>
        public IActivationExpressionResult ProvideLifestyleExpression(IInjectionScope scope, IActivationExpressionRequest request, Func<IActivationExpressionRequest, IActivationExpressionResult> activationExpression)
        {
            if (CompiledDelegate == null)
            {
                // new request as we don't want to carry any info over from parent request
                var newRequest = request.NewRootedRequest(request.ActivationType, scope, true);

                var localDelegate = request.Services.Compiler.CompileDelegate(scope, activationExpression(newRequest));

                Interlocked.CompareExchange(ref CompiledDelegate, localDelegate, null);
            }

            const string _getValueFromScopeMethodName = nameof(GetValueFromScope);
            var getValueFromScopeMethod =
                typeof(SingletonPerNamedScopeLifestyle).GetRuntimeMethod(_getValueFromScopeMethodName,
                    new[]
                    {
                        typeof(IExportLocatorScope),
                        typeof(ActivationStrategyDelegate),
                        typeof(string),
                        typeof(string),
                        typeof(bool),
                        typeof(IInjectionContext),
                        typeof(StaticInjectionContext)
                    });

            var closedMethod = getValueFromScopeMethod.MakeGenericMethod(request.ActivationType);

            var expression = Expression.Call(closedMethod,
                                             request.ScopeParameter,
                                             Expression.Constant(CompiledDelegate),
                                             Expression.Constant(UniqueId),
                                             Expression.Constant(_scopeName),
                                             Expression.Constant(scope.ScopeConfiguration.SingletonPerScopeShareContext),
                                             request.InjectionContextParameter,
                                             Expression.Constant(request.GetStaticInjectionContext()));

            request.RequireExportScope();

            return request.Services.Compiler.CreateNewResult(request, expression);
        }

        /// <summary>Get value from scope</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <param name="creationDelegate"></param>
        /// <param name="uniqueId"></param>
        /// <param name="scopeName"></param>
        /// <param name="context"></param>
        /// <param name="staticContext"></param>
        /// <param name="shareContext"></param>
        /// <returns></returns>
        public static T GetValueFromScope<T>(IExportLocatorScope scope, ActivationStrategyDelegate creationDelegate,
            string uniqueId, string scopeName, bool shareContext, IInjectionContext context, StaticInjectionContext staticContext)
        {
            while (scope is object)
            {
                if (scope.ScopeName == scopeName) { break; }

                scope = scope.Parent;
            }

            if (scope is null) { throw GetNamedScopeLocateException(scopeName, staticContext); }

            var value = scope.GetExtraData(uniqueId) ??
                GetValueFromScopeSlow(scope, creationDelegate, uniqueId, shareContext, context);

            return (T)value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object GetValueFromScopeSlow(IExportLocatorScope scope, ActivationStrategyDelegate creationDelegate,
            string uniqueId, bool shareContext, IInjectionContext context)
        {
            lock (scope.GetLockObject(uniqueId))
            {
                var value = scope.GetExtraData(uniqueId);

                if (value is null)
                {
                    value = creationDelegate(scope, scope, shareContext ? context : null);

                    scope.SetExtraData(uniqueId, value);
                }

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NamedScopeLocateException GetNamedScopeLocateException(string scopeName, StaticInjectionContext staticContext)
        {
            return new NamedScopeLocateException(scopeName, staticContext);
        }

        private string DebuggerDisplayValue => $"Singleton Per Named Scope ({_scopeName})";
    }
}