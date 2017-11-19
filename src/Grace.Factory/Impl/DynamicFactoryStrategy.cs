using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grace.Data;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Impl.Expressions;
using Grace.DependencyInjection.Impl.InstanceStrategies;
using Grace.DependencyInjection.Lifestyle;
using Grace.Utilities;

namespace Grace.Factory.Impl
{
  /// <summary>Creates a dynamic factory class for interface type</summary>
  public class DynamicFactoryStrategy : BaseInstanceExportStrategy
  {
    private Type _proxyType;
    private List<DynamicTypeBuilder.DelegateInfo> _delegateInfo;
    private readonly object _proxyTypeLock = new object();

    /// <summary>Default constructor</summary>
    /// <param name="activationType"></param>
    /// <param name="injectionScope"></param>
    public DynamicFactoryStrategy(Type activationType, IInjectionScope injectionScope)
      : base(activationType, injectionScope) { }

    /// <inheritdoc/>
    protected override IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request, ICompiledLifestyle lifestyle)
    {
      if (_proxyType == null)
      {
        lock (_proxyTypeLock)
        {
          if (_proxyType == null)
          {
            var builder = new DynamicTypeBuilder();

            _proxyType = builder.CreateType(ActivationType, out _delegateInfo);
          }
        }
      }

      var parameters = new List<Expression>
      {
        request.ScopeParameter,
        request.DisposalScopeExpression,
        request.InjectionContextParameter
      };

      var uniqueId = UniqueStringId.Generate();

      const string _getPrefix = "Get";

      foreach (var delegateInfo in _delegateInfo)
      {
        var locateType = delegateInfo.Method.ReturnType;

        var newRequest = request.NewRequest(locateType, this, ActivationType, RequestType.Other, null, true);

        newRequest.AddKnownValueExpression(CreateKnownValueExpression(newRequest, ActivationType, uniqueId));

        if (delegateInfo.Method.Name.StartsWith(_getPrefix, StringComparison.Ordinal))
        {
          newRequest.SetLocateKey(delegateInfo.Method.Name.Substring(_getPrefix.Length));
        }

        if (delegateInfo.ParameterInfos != null)
        {
          foreach (var parameter in delegateInfo.ParameterInfos)
          {
            newRequest.AddKnownValueExpression(
                CreateKnownValueExpression(newRequest, parameter.ParameterInfo.ParameterType, parameter.UniqueId, parameter.ParameterInfo.Name, parameter.ParameterInfo.Position));
          }
        }

        var result = request.Services.ExpressionBuilder.GetActivationExpression(request.RequestingScope, newRequest);

        var compiledDelegate = request.Services.Compiler.CompileDelegate(request.RequestingScope, result);

        parameters.Add(Expression.Constant(compiledDelegate));
      }

#if NET40
      var constructor = _proxyType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).First();
#else
      var constructor = _proxyType.GetTypeInfo().DeclaredConstructors.First();
#endif

      request.RequireInjectionContext();

      var newStatement = Expression.New(constructor, parameters);

      const string _setExtraDataMethod = "SetExtraData";
#if NET40
      var setMethod = typeof(IExtraDataContainer).GetMethod(_setExtraDataMethod,
#else
      var setMethod = typeof(IExtraDataContainer).GetRuntimeMethod(_setExtraDataMethod,
#endif
                new[] { typeof(object), typeof(object), typeof(bool) });

      var invokeStatement = Expression.Call(request.InjectionContextParameter, setMethod,
          Expression.Constant(uniqueId), newStatement, Expression.Constant(true));

      var castStatement = Expression.Convert(invokeStatement, ActivationType);

      return request.Services.Compiler.CreateNewResult(request, castStatement);
    }

    private IKnownValueExpression CreateKnownValueExpression(IActivationExpressionRequest request, Type argType, string valueId, string nameHint = null, int? position = null)
    {
      const string _getExtraDataMethod = "GetExtraData";
#if NET40
      var getMethod = typeof(IExtraDataContainer).GetMethod(_getExtraDataMethod, new[] { typeof(object) });
#else
      var getMethod = typeof(IExtraDataContainer).GetRuntimeMethod(_getExtraDataMethod, new[] { typeof(object) });
#endif

      var callExpression = Expression.Call(request.InjectionContextParameter, getMethod, Expression.Constant(valueId));

      return new SimpleKnownValueExpression(argType, Expression.Convert(callExpression, argType), nameHint, position);
    }
  }
}