﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt.Reflection;
using Grace.Utilities;

namespace Grace.DependencyInjection.Impl.Wrappers
{
    /// <summary>Strategy for creating two argument Func</summary>
    public class FuncTwoArgWrapperStrategy : BaseWrapperStrategy
    {
        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        public FuncTwoArgWrapperStrategy(IInjectionScope injectionScope)
          : base(typeof(Func<,,>), injectionScope) { }

        /// <summary>Get the type that is being wrapped</summary>
        /// <param name="type">requested type</param>
        /// <returns>wrapped type</returns>
        public override Type GetWrappedType(Type type)
        {
#if NET40
            if (!type.IsConstructedGenericType()) { return null; }
#else
            if (!type.IsConstructedGenericType) { return null; }
#endif

            var genericType = type.GetGenericTypeDefinition();

#if NET40
            return genericType == typeof(Func<,,>) ? type.GenericTypeArguments()[2] : null;
#else
            return genericType == typeof(Func<,,>) ? type.GenericTypeArguments[2] : null;
#endif
        }

        /// <summary>Get an activation expression for this strategy</summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
            // ## 苦竹 修改 ##
            //var closedClass = typeof(FuncExpression<,,>).MakeGenericType(request.ActivationType.GenericTypeArguments());
#if NET40
            var closedClass = typeof(FuncExpression<,,>).GetCachedGenericType(request.ActivationType.GenericTypeArguments());
#else
            var closedClass = typeof(FuncExpression<,,>).GetCachedGenericType(request.ActivationType.GenericTypeArguments);
#endif

            var closedMethod = closedClass.GetRuntimeMethod(CreateFuncMethodName,
                new[] { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) });

            // ## 苦竹 修改 ##
            //var instance = Activator.CreateInstance(closedClass, scope, request, request.Services.InjectionContextCreator, this);
            var instance = ActivatorUtils.CreateInstance(closedClass, scope, request, request.Services.InjectionContextCreator, this);

            request.RequireExportScope();
            request.RequireDisposalScope();

            var callExpression =
                Expression.Call(Expression.Constant(instance), closedMethod, request.ScopeParameter,
                    request.DisposalScopeExpression, request.InjectionContextParameter);

            return request.Services.Compiler.CreateNewResult(request, callExpression);
        }

        /// <summary>Helper class for creating 2 arg Func</summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        public class FuncExpression<T1, T2, TResult>
        {
            private readonly IInjectionContextCreator _injectionContextCreator;
            private readonly string _t1Id = UniqueStringId.Generate();
            private readonly string _t2Id = UniqueStringId.Generate();
            private readonly ActivationStrategyDelegate _action;

            /// <summary>Default constructor</summary>
            /// <param name="scope"></param>
            /// <param name="request"></param>
            /// <param name="injectionContextCreator"></param>
            /// <param name="activationStrategy"></param>
            public FuncExpression(IInjectionScope scope, IActivationExpressionRequest request,
              IInjectionContextCreator injectionContextCreator, IActivationStrategy activationStrategy)
            {
                _injectionContextCreator = injectionContextCreator;

#if NET40
                var requestActivationTypeArgus = request.ActivationType.GenericTypeArguments();
#else
                var requestActivationTypeArgus = request.ActivationType.GenericTypeArguments;
#endif
                var arg1Type = requestActivationTypeArgus[0];
                var arg2Type = requestActivationTypeArgus[1];
                var requestType = requestActivationTypeArgus[2];

                var newRequest = request.NewRequest(requestType, activationStrategy, typeof(Func<T1, T2, TResult>), RequestType.Other, null, true);

                newRequest.AddKnownValueExpression(CreateKnownValueExpression(request, arg1Type, _t1Id));
                newRequest.AddKnownValueExpression(CreateKnownValueExpression(request, arg2Type, _t2Id));

                newRequest.SetLocateKey(request.LocateKey);
                newRequest.DisposalScopeExpression = request.Constants.RootDisposalScope;

                var activationExpression = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

                _action = request.Services.Compiler.CompileDelegate(scope, activationExpression);
            }

            /// <summary>Method creates 2 arg Func</summary>
            /// <param name="scope"></param>
            /// <param name="disposalScope"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public Func<T1, T2, TResult> CreateFunc(IExportLocatorScope scope, IDisposalScope disposalScope, IInjectionContext context)
            {
                return (arg1, arg2) =>
                {
                    var newContext = context?.Clone() ?? _injectionContextCreator.CreateContext(null);

                    newContext.SetExtraData(_t1Id, arg1);
                    newContext.SetExtraData(_t2Id, arg2);

                    return (TResult)_action(scope, disposalScope, newContext);
                };
            }
        }
    }
}