using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt.Reflection;

namespace Grace.DependencyInjection.Impl.Expressions
{
    /// <summary>Creates linq expression that add instance to disposal scope</summary>
    public class DisposalScopeExpressionCreator : IDisposalScopeExpressionCreator
    {
#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        private MethodInfo _addAsyncMethod;
        private MethodInfo _addAsyncMethodWithCleanup;
#endif
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
            var addMethod = AddMethod;
            var addMethodWithCleanup = AddMethodWithCleanup;
#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
            if (activationConfiguration.ActivationType.GetTypeInfo()
                .ImplementedInterfaces.Contains(typeof(IAsyncDisposable)))
            {
                addMethod = AddAsyncMethod;
                addMethodWithCleanup = AddAsyncMethodWithCleanup;
            }
#endif
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

            if (disposalDelegate is not null)
            {
                closedGeneric = addMethodWithCleanup.MakeGenericMethod(activationConfiguration.ActivationType);
                parameterExpressions = new[]
                    {resultExpression, Expression.Convert(Expression.Constant(disposalDelegate), closedActionType)};
            }
            else
            {
                closedGeneric = addMethod.MakeGenericMethod(activationConfiguration.ActivationType);
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
        protected MethodInfo AddMethod => _addMethod ??
                                          (_addMethod = typeof(IDisposalScope).GetTypeInfo()
                                              .DeclaredMethods.First(m =>
                                                  m.Name == nameof(IDisposalScope.AddDisposable) &&
                                                  m.GetParameters().Length == 1));

        /// <summary>Method info for add method on IDisposalScope with cleanup delegate</summary>
        protected MethodInfo AddMethodWithCleanup => _addMethodWithCleanup ??
                                                     (_addMethodWithCleanup = typeof(IDisposalScope).GetTypeInfo()
                                                         .DeclaredMethods.First(m =>
                                                             m.Name == nameof(IDisposalScope.AddDisposable) &&
                                                             m.GetParameters().Length == 2));
#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
        /// <summary>Method info for add async method on IDisposalScope</summary>
        protected MethodInfo AddAsyncMethod => _addAsyncMethod ??
                                               (_addAsyncMethod =
                                                   typeof(IDisposalScope).GetTypeInfo()
                                                       .DeclaredMethods.First(m =>
                                                           m.Name == nameof(IDisposalScope.AddAsyncDisposable) &&
                                                           m.GetParameters().Length == 1));

        /// <summary>Method info for add method on IDisposalScope with cleanup delegate</summary>
        protected MethodInfo AddAsyncMethodWithCleanup => _addAsyncMethodWithCleanup ??
                                                          (_addAsyncMethodWithCleanup =
                                                              typeof(IDisposalScope).GetTypeInfo()
                                                                  .DeclaredMethods.First(m =>
                                                                      m.Name ==
                                                                      nameof(IDisposalScope.AddAsyncDisposable) &&
                                                                      m.GetParameters().Length == 2));
#endif
    }
}