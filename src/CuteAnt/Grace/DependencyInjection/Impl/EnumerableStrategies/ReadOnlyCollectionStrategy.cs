using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt.Reflection;

namespace Grace.DependencyInjection.Impl.EnumerableStrategies
{
  /// <summary>Strategy for creating ReadOnly(T)</summary>
  public class ReadOnlyCollectionStrategy : BaseGenericEnumerableStrategy
  {
    /// <summary>Default constructor</summary>
    /// <param name="injectionScope"></param>
    public ReadOnlyCollectionStrategy(IInjectionScope injectionScope)
      : base(typeof(ReadOnlyCollection<>), injectionScope)
    {
      AddExportAs(typeof(ReadOnlyCollection<>));
#if !NET40
      AddExportAs(typeof(IReadOnlyList<>));
      AddExportAs(typeof(IReadOnlyCollection<>));
#endif
    }

    /// <summary>Get an activation expression for this strategy</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
    {
#if NET40
      var elementType = request.ActivationType.GenericTypeArguments()[0];
#else
      var elementType = request.ActivationType.GenericTypeArguments[0];
#endif

      // ## 苦竹 修改 ##
      //var closedType = typeof(ReadOnlyCollection<>).MakeGenericType(elementType);
      //var newRequest = request.NewRequest(typeof(IList<>).MakeGenericType(elementType), this, closedType, RequestType.Other, null, true);
      var closedType = typeof(ReadOnlyCollection<>).GetCachedGenericType(elementType);
      var newRequest = request.NewRequest(typeof(IList<>).GetCachedGenericType(elementType), this, closedType, RequestType.Other, null, true);

      newRequest.SetFilter(request.Filter);
      newRequest.SetEnumerableComparer(request.EnumerableComparer);

      var listResult = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

      var constructor = closedType.GetTypeInfo().DeclaredConstructors.First(c =>
      {
        var parameters = c.GetParameters();

        if (parameters.Length == 1)
        {
          var parameterType = parameters[0].ParameterType;
#if NET40
          return parameterType.IsConstructedGenericType() && parameterType.GetGenericTypeDefinition() == typeof(IList<>);
#else
          return parameterType.IsConstructedGenericType && parameterType.GetGenericTypeDefinition() == typeof(IList<>);
#endif
        }

        return false;
      });

      var expression = Expression.New(constructor, listResult.Expression);

      var result = request.Services.Compiler.CreateNewResult(request, expression);

      result.AddExpressionResult(listResult);

      return result;
    }
  }

#if NET40
  /// <summary>Strategy for creating ReadOnly(T)</summary>
  public class ReadOnlyCollectionXStrategy : BaseGenericEnumerableStrategy
  {
    /// <summary>Default constructor</summary>
    /// <param name="injectionScope"></param>
    public ReadOnlyCollectionXStrategy(IInjectionScope injectionScope)
      : base(typeof(ReadOnlyCollectionX<>), injectionScope)
    {
      AddExportAs(typeof(ReadOnlyCollectionX<>));
      AddExportAs(typeof(IReadOnlyList<>));
      AddExportAs(typeof(IReadOnlyCollection<>));
    }

    /// <summary>Get an activation expression for this strategy</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
    {
      var elementType = request.ActivationType.GenericTypeArguments()[0];

      var closedType = typeof(ReadOnlyCollectionX<>).GetCachedGenericType(elementType);

      var newRequest = request.NewRequest(typeof(IList<>).GetCachedGenericType(elementType), this, closedType, RequestType.Other, null, true);

      newRequest.SetFilter(request.Filter);
      newRequest.SetEnumerableComparer(request.EnumerableComparer);

      var listResult = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

      var constructor = closedType.GetTypeInfo().DeclaredConstructors.First(c =>
      {
        var parameters = c.GetParameters();

        if (parameters.Length == 1)
        {
          var parameterType = parameters[0].ParameterType;
          return parameterType.IsConstructedGenericType() &&
                       parameterType.GetGenericTypeDefinition() == typeof(IList<>);
        }

        return false;
      });

      var expression = Expression.New(constructor, listResult.Expression);

      var result = request.Services.Compiler.CreateNewResult(request, expression);

      result.AddExpressionResult(listResult);

      return result;
    }
  }
#endif
}