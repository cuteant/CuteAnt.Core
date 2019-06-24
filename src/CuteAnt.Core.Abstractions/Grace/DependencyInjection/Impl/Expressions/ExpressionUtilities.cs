using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grace.Data;
using Grace.DependencyInjection.Exceptions;

namespace Grace.DependencyInjection.Impl.Expressions
{
    /// <summary>Expression utilities</summary>
    public static class ExpressionUtilities
    {
        #region CreateExpressionsForTypes

        /// <summary>Create an array of expressions based off an array of types</summary>
        /// <param name="strategy"></param>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <param name="resultType"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IActivationExpressionResult[] CreateExpressionsForTypes(IActivationStrategy strategy, IInjectionScope scope,
          IActivationExpressionRequest request, Type resultType, params Type[] types)
        {
            var resultArray = new IActivationExpressionResult[types.Length];

            for (var i = 0; i < types.Length; i++)
            {
                var arg1Request = request.NewRequest(types[i], strategy, resultType, RequestType.Other, null, true, true);

                resultArray[i] = request.Services.ExpressionBuilder.GetActivationExpression(scope, arg1Request);
            }

            return resultArray;
        }

        #endregion

        #region Create Expression to call delegate

        private const string _closureName = "System.Runtime.CompilerServices.Closure";

        /// <summary>Create an expression to call delegate and apply null check and disposal logic</summary>
        /// <param name="delegateInstance"></param>
        /// <param name="allowDisposableTracking"></param>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <param name="requestingStrategy"></param>
        /// <returns></returns>
        public static IActivationExpressionResult CreateExpressionForDelegate(Delegate delegateInstance, bool allowDisposableTracking,
          IInjectionScope scope, IActivationExpressionRequest request, IActivationStrategy requestingStrategy)
        {
            var methodInfo = delegateInstance.GetMethodInfo();

            Expression expression = null;
            IActivationExpressionResult[] resultsExpressions;

            // Handle closure based delegates differently
            if (delegateInstance.Target != null && string.Equals(_closureName, delegateInstance.Target.GetType().FullName, StringComparison.Ordinal))
            {
                resultsExpressions = CreateExpressionsForTypes(requestingStrategy, scope, request, methodInfo.ReturnType,
                    methodInfo.GetParameters()
                        .Where(p => !(p.Position == 0 && string.Equals(_closureName, p.ParameterType.FullName, StringComparison.Ordinal)))
                        .Select(p => p.ParameterType).ToArray());

                expression = Expression.Invoke(Expression.Constant(delegateInstance),
                    resultsExpressions.Select(e => e.Expression));
            }
            else
            {
                resultsExpressions = CreateExpressionsForTypes(requestingStrategy, scope, request, methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());

                expression = methodInfo.IsStatic
                    ? Expression.Call(methodInfo, resultsExpressions.Select(e => e.Expression))
                    : Expression.Call(Expression.Constant(delegateInstance.Target),
                        methodInfo, resultsExpressions.Select(e => e.Expression));
            }

            var allowNull = (requestingStrategy as IInstanceActivationStrategy)?.AllowNullReturn ?? false;

            expression = ApplyNullCheckAndAddDisposal(scope, request, expression, allowDisposableTracking, allowNull);

            var result = request.Services.Compiler.CreateNewResult(request, expression);

            foreach (var expressionResult in resultsExpressions)
            {
                result.AddExpressionResult(expressionResult);
            }

            return result;
        }

        #endregion

        #region Apply null check

        /// <summary>Applies null check and disposal scope tracking logic to an expression</summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <param name="expression"></param>
        /// <param name="allowDisposableTracking"></param>
        /// <param name="allowNull"></param>
        /// <returns></returns>
        public static Expression ApplyNullCheckAndAddDisposal(IInjectionScope scope,
          IActivationExpressionRequest request, Expression expression, bool allowDisposableTracking, bool allowNull)
        {
            if (expression.Type != request.ActivationType &&
               !ReflectionService.CheckTypeIsBasedOnAnotherType(expression.Type, request.ActivationType))
            {
                expression = Expression.Convert(expression, request.ActivationType);
            }

            if (!allowDisposableTracking)
            {
                if (request.DefaultValue != null)
                {
                    var method = typeof(ExpressionUtilities).GetRuntimeMethods()
                        .FirstOrDefault(m => string.Equals(_valueOrDefaultMethodName, m.Name, StringComparison.Ordinal));

                    var closedMethod = method.MakeGenericMethod(request.ActivationType);

                    return Expression.Call(closedMethod, expression, Expression.Constant(request.DefaultValue.DefaultValue, request.ActivationType));
                }

                if (!allowNull &&
                    !scope.ScopeConfiguration.Behaviors.AllowInstanceAndFactoryToReturnNull &&
                    request.IsRequired)
                {
                    var closedMethod = CheckForNullMethodInfo.MakeGenericMethod(request.ActivationType);

                    return Expression.Call(closedMethod,
                                           Expression.Constant(request.GetStaticInjectionContext()),
                                           expression);
                }

                return expression;
            }

            if (request.DefaultValue != null)
            {
                request.RequireDisposalScope();

                var method = typeof(ExpressionUtilities).GetRuntimeMethods()
                    .FirstOrDefault(m => string.Equals(_addToDisposableScopeOrDefaultMethodName, m.Name, StringComparison.Ordinal));

                var closedMethod = method.MakeGenericMethod(request.ActivationType);

                return Expression.Call(closedMethod, request.DisposalScopeExpression, expression,
                    Expression.Constant(request.DefaultValue.DefaultValue, request.ActivationType));
            }

            if (allowNull ||
                scope.ScopeConfiguration.Behaviors.AllowInstanceAndFactoryToReturnNull ||
                !request.IsRequired)
            {
                request.RequireDisposalScope();

                var closedMethod = AddToDisposalScopeMethodInfo.MakeGenericMethod(request.ActivationType);

                return Expression.Call(closedMethod, request.DisposalScopeExpression, expression);
            }
            else
            {
                request.RequireDisposalScope();

                var closedMethod = CheckForNullAndAddToDisposalScopeMethodInfo.MakeGenericMethod(request.ActivationType);

                return Expression.Call(closedMethod,
                                       request.DisposalScopeExpression,
                                       Expression.Constant(request.GetStaticInjectionContext()), expression);
            }
        }

        #region Check For Null

        const string _checkForNullMethodName = nameof(CheckForNull);
        /// <summary>Check value for null</summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="context">static context</param>
        /// <param name="value">value to check</param>
        /// <returns>non null value</returns>
        public static T CheckForNull<T>(StaticInjectionContext context, T value)
        {
            if (value == null) { throw new NullValueProvidedException(context); }

            return value;
        }

        private static MethodInfo _checkForNullMethodInfo;

        /// <summary>Method info for CheckForNull</summary>
        public static MethodInfo CheckForNullMethodInfo
        {
            get
            {
                return _checkForNullMethodInfo ??
                       (_checkForNullMethodInfo =
                           typeof(ExpressionUtilities).GetRuntimeMethods().First(m => string.Equals(_checkForNullMethodName, m.Name, StringComparison.Ordinal)));
            }
        }

        #endregion

        #region AddToDisposalScope

        const string _addToDisposalScopeMethodName = "AddToDisposalScope";
        /// <summary>Add instance to disposal scope and return it</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposalScope"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T AddToDisposalScope<T>(IDisposalScope disposalScope, T value)
        {
            if (value is IDisposable disposable)
            {
                disposalScope.AddDisposable(disposable);
            }

            return value;
        }

        private static MethodInfo _addToDisposalScopeMethodInfo;

        /// <summary>Method info for AddToDisposalScope</summary>
        public static MethodInfo AddToDisposalScopeMethodInfo
        {
            get
            {
                return _addToDisposalScopeMethodInfo ??
                       (_addToDisposalScopeMethodInfo =
                           typeof(ExpressionUtilities).GetRuntimeMethods().First(m => string.Equals(_addToDisposalScopeMethodName, m.Name, StringComparison.Ordinal)));
            }
        }

        #endregion

        #region CheckForNullAndAddToDisposalScope

        const string _checkForNullAndAddToDisposalScopeMethodName = nameof(CheckForNullAndAddToDisposalScope);
        /// <summary>Check for null and then add to disposal scope</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposalScope"></param>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CheckForNullAndAddToDisposalScope<T>(IDisposalScope disposalScope, StaticInjectionContext context, T value)
        {
            if (value == null) { throw new NullValueProvidedException(context); }

            if (value is IDisposable disposable)
            {
                disposalScope.AddDisposable(disposable);
            }

            return value;
        }

        private static MethodInfo _checkForNullAndAddToDisposalScopeMethodInfo;

        /// <summary>Method info for CheckForNullAndAddToDisposalScope</summary>
        public static MethodInfo CheckForNullAndAddToDisposalScopeMethodInfo
        {
            get
            {
                return _checkForNullAndAddToDisposalScopeMethodInfo ??
                       (_checkForNullAndAddToDisposalScopeMethodInfo =
                           typeof(ExpressionUtilities).GetRuntimeMethods().First(m => string.Equals(_checkForNullAndAddToDisposalScopeMethodName, m.Name, StringComparison.Ordinal)));
            }
        }

        #endregion

        #region ValueOrDefault

        const string _valueOrDefaultMethodName = "ValueOrDefault";
        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ValueOrDefault<T>(T tValue, T defaultValue) => tValue != null ? tValue : defaultValue;

        #endregion

        #region AddToDisposableScopeOrDefault

        const string _addToDisposableScopeOrDefaultMethodName = "AddToDisposableScopeOrDefault";
        /// <summary>Add to disposal scope or use default</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="disposalScope"></param>
        /// <param name="tValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T AddToDisposableScopeOrDefault<T>(IDisposalScope disposalScope, T tValue, T defaultValue)
        {
            if (tValue != null)
            {
                if (tValue is IDisposable disposable)
                {
                    disposalScope.AddDisposable(disposable);
                }

                return tValue;
            }

            return defaultValue;
        }

        #endregion

        #endregion
    }
}