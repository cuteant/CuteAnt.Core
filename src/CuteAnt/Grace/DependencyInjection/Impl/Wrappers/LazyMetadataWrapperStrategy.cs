using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt.Reflection;
using Grace.DependencyInjection.Exceptions;
using Grace.DependencyInjection.Impl.Expressions;

namespace Grace.DependencyInjection.Impl.Wrappers
{
    /// <summary>Wrapper for Lazy&lt;T,IActivationStrategyMetadata&gt;</summary>
    public class LazyMetadataWrapperStrategy : BaseWrapperStrategy
    {
        private readonly IStrongMetadataInstanceProvider _strongMetadataInstanceProvider;
        private readonly IWrapperExpressionCreator _wrapperExpressionCreator;

        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        /// <param name="strongMetadataInstanceProvider"></param>
        /// <param name="wrapperExpressionCreator"></param>
        public LazyMetadataWrapperStrategy(IInjectionScope injectionScope, IStrongMetadataInstanceProvider strongMetadataInstanceProvider, IWrapperExpressionCreator wrapperExpressionCreator)
          : base(typeof(Lazy<,>), injectionScope)
        {
            _strongMetadataInstanceProvider = strongMetadataInstanceProvider;
            _wrapperExpressionCreator = wrapperExpressionCreator;
        }

        /// <summary>Get type that wrapper wraps</summary>
        /// <param name="type">wrapper type</param>
        /// <returns>type that has been wrapped</returns>
        public override Type GetWrappedType(Type type)
        {
#if NET40
            if (!type.IsConstructedGenericType()) { return null; }
#else
            if (!type.IsConstructedGenericType) { return null; }
#endif

            var genericType = type.GetGenericTypeDefinition();

#if NET40
            return genericType == typeof(Lazy<,>) ? type.GenericTypeArguments()[0] : null;
#else
            return genericType == typeof(Lazy<,>) ? type.GenericTypeArguments[0] : null;
#endif
        }

        /// <summary>Compile a delegate</summary>
        /// <param name="scope">scope</param>
        /// <param name="compiler">compiler</param>
        /// <param name="activationType">activation type</param>
        /// <returns></returns>
        protected override ActivationStrategyDelegate CompileDelegate(
          IInjectionScope scope, IActivationStrategyCompiler compiler, Type activationType)
        {
            var request = compiler.CreateNewRequest(activationType, 1, scope);

            if (!_wrapperExpressionCreator.SetupWrappersForRequest(scope, request))
            {
                ThrowLocateException0(request);
            }

            var expressionResult = GetActivationExpression(scope, request);

            ActivationStrategyDelegate returnValue = null;

            if (expressionResult != null)
            {
                returnValue = compiler.CompileDelegate(scope, expressionResult);
            }

            return returnValue;
        }

        /// <summary>Get an activation expression for this strategy</summary>
        /// <param name="scope"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        {
            // ## 苦竹 修改 ##
            //var closedClass = typeof(LazyExpression<,>).MakeGenericType(request.ActivationType.GenericTypeArguments());
#if NET40
            var requestGenericTypeArguments = request.ActivationType.GenericTypeArguments();
#else
            var requestGenericTypeArguments = request.ActivationType.GenericTypeArguments;
#endif
            var closedClass = typeof(LazyExpression<,>).GetCachedGenericType(requestGenericTypeArguments);

            var closedMethod = closedClass.GetRuntimeMethod(CreateLazyMethodName,
                new[] { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) });

            var wrappedStrategy = request.GetWrappedStrategy();

            if (wrappedStrategy == null)
            {
                ThrowLocateException1(request);
            }

            var metadata = _strongMetadataInstanceProvider.GetMetadata(requestGenericTypeArguments[1], wrappedStrategy.Metadata);

            // ## 苦竹 修改 ##
            //var instance = Activator.CreateInstance(closedClass, scope, request, this, metadata);
            var instance = ActivatorUtils.CreateInstance(closedClass, scope, request, this, metadata);

            request.RequireExportScope();
            request.RequireDisposalScope();

            var callExpression =
                Expression.Call(Expression.Constant(instance), closedMethod, request.ScopeParameter,
                    request.DisposalScopeExpression, request.InjectionContextParameter);

            request.RequireInjectionContext();

            return request.Services.Compiler.CreateNewResult(request, callExpression);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowLocateException0(IActivationExpressionRequest request)
        {
            throw GetLocateException();
            LocateException GetLocateException()
            {
                return new LocateException(request.GetStaticInjectionContext(), "Could not calculate wrapper");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowLocateException1(IActivationExpressionRequest request)
        {
            throw GetLocateException();
            LocateException GetLocateException()
            {
                return new LocateException(request.GetStaticInjectionContext(), "Could not find strategy that is wrapped");
            }
        }

        /// <summary>Lazy expression helper class</summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TMetadata"></typeparam>
        public class LazyExpression<TResult, TMetadata>
        {
            private ActivationStrategyDelegate _delegate;
            private IActivationExpressionRequest _request;
            private IInjectionScope _scope;
            private IActivationStrategy _activationStrategy;
            private readonly TMetadata _metadata;
            private readonly object _lock = new object();

            /// <summary>Default constructor</summary>
            /// <param name="scope"></param>
            /// <param name="request"></param>
            /// <param name="activationStrategy"></param>
            /// <param name="metadata"></param>
            public LazyExpression(IInjectionScope scope,
              IActivationExpressionRequest request, IActivationStrategy activationStrategy, TMetadata metadata)
            {
                _scope = scope;
                _request = request;
                _activationStrategy = activationStrategy;
                _metadata = metadata;
            }

            /// <summary>Create lazy instance</summary>
            /// <param name="scope"></param>
            /// <param name="disposalScope"></param>
            /// <param name="injectionContext"></param>
            /// <returns></returns>
            public Lazy<TResult, TMetadata> CreateLazy(IExportLocatorScope scope, IDisposalScope disposalScope, IInjectionContext injectionContext)
            {
                return new Lazy<TResult, TMetadata>(() =>
                {
                    if (_delegate == null) { _delegate = CompileDelegate(); }

                    return (TResult)_delegate(scope, disposalScope, injectionContext);
                }, _metadata);
            }

            private ActivationStrategyDelegate CompileDelegate()
            {
                lock (_lock)
                {
                    if (_delegate == null)
                    {
#if NET40
                        var requestType = _request.ActivationType.GenericTypeArguments()[0];
#else
                        var requestType = _request.ActivationType.GenericTypeArguments[0];
#endif

                        var newRequest = _request.NewRequest(requestType, _activationStrategy, typeof(Lazy<TResult>),
                            RequestType.Other, null, true, true);

                        newRequest.DisposalScopeExpression = _request.Constants.RootDisposalScope;

                        var activationExpression = _request.Services.ExpressionBuilder.GetActivationExpression(_scope, newRequest);

                        _delegate = _request.Services.Compiler.CompileDelegate(_scope, activationExpression);

                        _scope = null;
                        _request = null;
                        _activationStrategy = null;
                    }
                }

                return _delegate;
            }
        }
    }
}