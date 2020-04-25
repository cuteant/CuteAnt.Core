using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Grace.DependencyInjection.Impl.Wrappers
{
    /// <summary>Wrapper strategy for Owned&lt;T&gt;</summary>
    public class OwnedWrapperStrategy : BaseWrapperStrategy
    {
        /// <summary>Default constructor</summary>
        /// <param name="injectionScope"></param>
        public OwnedWrapperStrategy(IInjectionScope injectionScope)
          : base(typeof(Owned<>), injectionScope) { }

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

                if (genericType == typeof(Owned<>))
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
            var constructor = request.ActivationType.GetTypeInfo().DeclaredConstructors.First();

#if NET40
            var wrappedType = request.ActivationType.GenericTypeArguments()[0];
#else
            var wrappedType = request.ActivationType.GenericTypeArguments[0];
#endif
            var ownedParameter = Expression.Parameter(request.ActivationType);

            var assign = Expression.Assign(ownedParameter, Expression.New(constructor));

            var newRequest = request.NewRequest(wrappedType, this, request.ActivationType, RequestType.Other, null, true, true);

            newRequest.DisposalScopeExpression = ownedParameter;

            var expressionResult = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

            const string _setValueMethodName = "SetValue";
            var setMethod = request.ActivationType.GetRuntimeMethods().First(
                m => string.Equals(_setValueMethodName, m.Name));

            var expression = Expression.Call(ownedParameter, setMethod, expressionResult.Expression);

            var returnExpression = request.Services.Compiler.CreateNewResult(request, expression);

            returnExpression.AddExpressionResult(expressionResult);

            returnExpression.AddExtraParameter(ownedParameter);
            returnExpression.AddExtraExpression(assign);

            return returnExpression;
        }
    }
}