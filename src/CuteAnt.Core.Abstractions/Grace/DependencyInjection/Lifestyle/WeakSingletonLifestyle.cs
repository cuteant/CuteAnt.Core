using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Grace.DependencyInjection.Lifestyle
{
    /// <summary>Weak singleton lifestyle</summary>
    [DebuggerDisplay("Weak Singleton")]
    public class WeakSingletonLifestyle : ICompiledLifestyle
    {
        private readonly WeakReference _weakReference = new WeakReference(null);
        private ActivationStrategyDelegate _delegate;
        private readonly object _lockObject = new object();

        /// <summary>Generalization for lifestyle</summary>
        public LifestyleType LifestyleType { get; } = LifestyleType.Singleton;

        /// <summary>Clone the lifestyle</summary>
        /// <returns></returns>
        public ICompiledLifestyle Clone() => new WeakSingletonLifestyle();

        /// <summary>Provide an expression that uses the lifestyle</summary>
        /// <param name="scope">scope for the strategy</param>
        /// <param name="request">activation request</param>
        /// <param name="activationExpression">expression to create strategy type</param>
        /// <returns></returns>
        public IActivationExpressionResult ProvideLifestyleExpression(IInjectionScope scope, IActivationExpressionRequest request, Func<IActivationExpressionRequest, IActivationExpressionResult> activationExpression)
        {
            if (_delegate is null)
            {
                // new request as we don't want to carry any info over from parent request
                var newRequest = request.NewRootedRequest(request.ActivationType, scope, true);

                var newDelegate = request.Services.Compiler.CompileDelegate(scope, activationExpression(newRequest));

                Interlocked.CompareExchange(ref _delegate, newDelegate, null);
            }

            var getMethod = typeof(WeakSingletonLifestyle).GetRuntimeMethod(_getValueMethodName, new[] { typeof(IInjectionScope) });

            var closedMethod = getMethod.MakeGenericMethod(request.ActivationType);

            var expression = Expression.Call(Expression.Constant(this), closedMethod, Expression.Constant(scope));

            return request.Services.Compiler.CreateNewResult(request, expression);
        }

        const string _getValueMethodName = nameof(GetValue);
        /// <summary>Get value</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <returns></returns>
        public T GetValue<T>(IInjectionScope scope)
        {
            var target = _weakReference.Target ?? GetValueSlow(scope);

            return (T)target;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private object GetValueSlow(IInjectionScope scope)
        {
            lock (_lockObject)
            {
                var target = _weakReference.Target;

                if (target is object) { return target; }

                target = _delegate(scope, scope, null);

                _weakReference.Target = target;

                return target;
            }
        }
    }
}