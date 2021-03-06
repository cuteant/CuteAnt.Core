﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt.Reflection;

namespace Grace.DependencyInjection.Impl.Expressions
{
    /// <summary>Creates linq expression that add instance to disposal scope</summary>
    public class DisposalScopeExpressionCreator : IDisposalScopeExpressionCreator
    {
        private MethodInfo _addMethod;
        private MethodInfo _addMethodWithCleanup;

        /// <summary>Create expression to add instance to disposal scope</summary>
        /// <param name="scope">scope for strategy</param>
        /// <param name="request">request</param>
        /// <param name="activationConfiguration">activation configuration</param>
        /// <param name="result">result for instantiation</param>
        /// <returns></returns>
        public IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request,
            TypeActivationConfiguration activationConfiguration, IActivationExpressionResult result)
        {
            // ## 苦竹 修改 ##
            //var closedActionType = typeof(Action<>).MakeGenericType(activationConfiguration.ActivationType);
            var closedActionType = typeof(Action<>).GetCachedGenericType(activationConfiguration.ActivationType);

            object disposalDelegate = null;

            if (closedActionType == activationConfiguration.DisposalDelegate?.GetType())
            {
                disposalDelegate = activationConfiguration.DisposalDelegate;
            }

            MethodInfo closedGeneric;
            Expression[] parameterExpressions;

            var resultExpression = result.Expression;

            if (resultExpression.Type != activationConfiguration.ActivationType)
            {
                resultExpression = Expression.Convert(resultExpression, activationConfiguration.ActivationType);
            }

            if (disposalDelegate != null)
            {
                closedGeneric = AddMethodWithCleanup.MakeGenericMethod(activationConfiguration.ActivationType);
                parameterExpressions = new[] { resultExpression, Expression.Convert(Expression.Constant(disposalDelegate), closedActionType) };
            }
            else
            {
                closedGeneric = AddMethod.MakeGenericMethod(activationConfiguration.ActivationType);
                parameterExpressions = new[] { resultExpression };
            }

            request.RequireDisposalScope();

            var disposalCall = Expression.Call(request.DisposalScopeExpression, closedGeneric, parameterExpressions);

            var disposalResult = request.Services.Compiler.CreateNewResult(request, disposalCall);

            disposalResult.AddExpressionResult(result);

            return disposalResult;
        }

        const string _addDisposableMethodName = nameof(IDisposalScope.AddDisposable);
        /// <summary>Method info for add method on IDisposalScope</summary>
        protected MethodInfo AddMethod
            => _addMethod ??
                (_addMethod = typeof(IDisposalScope).GetTypeInfo().DeclaredMethods.First(m => string.Equals(_addDisposableMethodName, m.Name) && m.GetParameters().Length == 1));

        /// <summary>Method info for add method on IDisposalScope with cleanup delegate</summary>
        protected MethodInfo AddMethodWithCleanup
            => _addMethodWithCleanup ??
                (_addMethodWithCleanup = typeof(IDisposalScope).GetTypeInfo().DeclaredMethods.First(m => string.Equals(_addDisposableMethodName, m.Name) && m.GetParameters().Length == 2));
    }
}