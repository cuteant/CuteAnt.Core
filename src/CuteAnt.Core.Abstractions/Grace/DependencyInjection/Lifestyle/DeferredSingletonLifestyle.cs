using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Grace.DependencyInjection.Lifestyle
{

    /// <summary>
    /// Singleton lifestyle that defers the creation of the instance till requested.
    /// The standard singleton lifestyle creates the instance early. 
    /// </summary>
    public class DeferredSingletonLifestyle : ICompiledLifestyle
    {
        private volatile object _singleton;
        private readonly object _lockObject = new object();
        private ActivationStrategyDelegate _activationDelegate;

        /// <summary>
        /// Constant expression
        /// </summary>
        protected Expression ConstantExpression;

        public LifestyleType LifestyleType => LifestyleType.Singleton;

        public ICompiledLifestyle Clone()
        {
            return new DeferredSingletonLifestyle();
        }

        public IActivationExpressionResult ProvideLifestyleExpression(IInjectionScope scope,
            IActivationExpressionRequest request,
            Func<IActivationExpressionRequest, IActivationExpressionResult> activationExpression)
        {
            if (ConstantExpression != null)
            {
                return request.Services.Compiler.CreateNewResult(request, ConstantExpression);
            }

            // Create new request as we shouldn't carry over anything from the previous request
            var newRequest = request.NewRootedRequest(request.ActivationType, scope, true);

            _activationDelegate =
                request.Services.Compiler.CompileDelegate(scope, activationExpression(newRequest));

            var singletonMethod = GetType().GetTypeInfo().GetDeclaredMethod(nameof(SingletonActivation));

            ConstantExpression = Expression.Call(Expression.Constant(this), singletonMethod,
                request.Constants.ScopeParameter, request.Constants.RootDisposalScope,
                request.Constants.InjectionContextParameter);

            ConstantExpression = Expression.Convert(ConstantExpression, request.ActivationType);

            return request.Services.Compiler.CreateNewResult(request, ConstantExpression);
        }

        private object SingletonActivation(IExportLocatorScope scope, IDisposalScope disposalScope, IInjectionContext context)
        {
            return _singleton ?? SingletonActivationSlow(scope, disposalScope, context);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private object SingletonActivationSlow(IExportLocatorScope scope, IDisposalScope disposalScope, IInjectionContext context)
        {
            lock (_lockObject)
            {
                if (_singleton is null)
                {
                    _singleton = _activationDelegate(scope, disposalScope, context);
                }
            }

            return _singleton;
        }
    }
}
