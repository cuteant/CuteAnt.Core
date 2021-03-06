﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Grace.DependencyInjection.Impl.Wrappers
{
    /// <summary>Wrapper for Meta&lt;T&gt;</summary>
    public class MetaWrapperStrategy : BaseWrapperStrategy
    {
        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        public MetaWrapperStrategy(IInjectionScope injectionScope)
          : base(typeof(Meta<>), injectionScope) { }

        /// <summary>Get type that wrapper wraps</summary>
        /// <param name="wrappedType">wrapper type</param>
        /// <returns>type that has been wrapped</returns>
        public override Type GetWrappedType(Type wrappedType)
        {
#if NET40
            if (wrappedType.IsConstructedGenericType())
#else
            if (wrappedType.IsConstructedGenericType)
#endif
            {
                var genericType = wrappedType.GetGenericTypeDefinition();

                if (genericType == typeof(Meta<>))
                {
#if NET40
                    return wrappedType.GenericTypeArguments()[0];
#else
                    return wrappedType.GenericTypeArguments[0];
#endif
                }
            }

            return null;
        }

        /// <summary>Get an activation expression for this strategy</summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
#if NET40
            var requestType = request.ActivationType.GenericTypeArguments()[0];
#else
            var requestType = request.ActivationType.GenericTypeArguments[0];
#endif

            var constructor = request.ActivationType.GetTypeInfo().DeclaredConstructors.First();

            var newRequest = request.NewRequest(requestType, this, request.ActivationType, RequestType.Other, null, true, true);

            var strategy = request.GetWrappedExportStrategy();

            if (strategy == null) { ThrowException(); }

            var expressionResult = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

            var newExpression = Expression.New(constructor, expressionResult.Expression, Expression.Constant(strategy.Metadata));

            var newResult = request.Services.Compiler.CreateNewResult(request, newExpression);

            newResult.AddExpressionResult(expressionResult);

            return newResult;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowException()
        {
            throw GetException();
            Exception GetException()
            {
                return new Exception("Could not find export stragegy to get metadata from");
            }
        }

        /// <summary>Compiles delegate</summary>
        /// <param name="scope"></param>
        /// <param name="compiler"></param>
        /// <param name="activationType"></param>
        /// <returns></returns>
        protected override ActivationStrategyDelegate CompileDelegate(
          IInjectionScope scope, IActivationStrategyCompiler compiler, Type activationType)
        {
            var request = compiler.CreateNewRequest(activationType, 1, scope);

            var expressionResult = request.Services.ExpressionBuilder.GetActivationExpression(scope, request);

            return compiler.CompileDelegate(scope, expressionResult);
        }
    }
}