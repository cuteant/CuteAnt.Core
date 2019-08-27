using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Grace.DependencyInjection.Impl.Wrappers
{
    /// <summary>
    /// Typed activation strategy delegate wrapper
    /// </summary>
    public class TypedActivationStrategyDelegateWrapperStrategy : BaseWrapperStrategy
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="injectionScope"></param>
        public TypedActivationStrategyDelegateWrapperStrategy(IInjectionScope injectionScope) : base(typeof(TypedActivationStrategyDelegate<>), injectionScope)
        {
        }

        /// <summary>
        /// Get Wrapped type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override Type GetWrappedType(Type type)
        {
#if NET40
            if (type.IsConstructedGenericType())
            {
                return type.GenericTypeArguments()[0];
            }
#else
            if (type.IsConstructedGenericType)
            {
                return type.GenericTypeArguments[0];
            }
#endif

            return null;
        }

        /// <summary>
        /// get activation strategy delegate
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
#if NET40
            var closedClass = typeof(TypedDelegateExpression<>).MakeGenericType(request.ActivationType.GenericTypeArguments());
#else
            var closedClass = typeof(TypedDelegateExpression<>).MakeGenericType(request.ActivationType.GenericTypeArguments);
#endif

            var closedMethod = closedClass.GetRuntimeMethod("CreateFunc", new Type[] {  });

            var instance = Activator.CreateInstance(closedClass, scope, request, this);

            request.RequireExportScope();
            request.RequireDisposalScope();

            var callExpression =
                Expression.Call(Expression.Constant(instance), closedMethod);

            return request.Services.Compiler.CreateNewResult(request, callExpression);
        }

        /// <summary>
        /// Func helper class 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        public class TypedDelegateExpression<TResult>
        {
            private readonly ActivationStrategyDelegate _action;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="scope"></param>
            /// <param name="request"></param>
            /// <param name="activationStrategy"></param>
            public TypedDelegateExpression(IInjectionScope scope, IActivationExpressionRequest request, IActivationStrategy activationStrategy)
            {
#if NET40
                var requestType = request.ActivationType.GenericTypeArguments()[0];
#else
                var requestType = request.ActivationType.GenericTypeArguments[0];
#endif

                var newRequest = request.NewRequest(requestType, activationStrategy, typeof(TypedActivationStrategyDelegate<TResult>), RequestType.Other, null, true);

                newRequest.SetLocateKey(request.LocateKey);
                newRequest.DisposalScopeExpression = request.Constants.RootDisposalScope;

                var activationExpression = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

                _action = request.Services.Compiler.CompileDelegate(scope, activationExpression);
            }

            /// <summary>
            /// Create func
            /// </summary>
            /// <returns></returns>
            public TypedActivationStrategyDelegate<TResult> CreateFunc()
            {
                return (scope, disposalScope, context) => (TResult)_action(scope, disposalScope, context);
            }
        }
    }
}
