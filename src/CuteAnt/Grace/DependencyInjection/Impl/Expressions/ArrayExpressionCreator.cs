﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt.Reflection;
using Grace.Data.Immutable;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Creates linq expression for array initialization</summary>
  public class ArrayExpressionCreator : IArrayExpressionCreator
  {
    private readonly IWrapperExpressionCreator _wrapperExpressionCreator;

    /// <summary>Default constructor</summary>
    /// <param name="wrapperExpressionCreator"></param>
    public ArrayExpressionCreator(IWrapperExpressionCreator wrapperExpressionCreator)
        => _wrapperExpressionCreator = wrapperExpressionCreator;

    /// <summary>Get linq expression to create</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <returns></returns>
    public IActivationExpressionResult GetArrayExpression(IInjectionScope scope, IActivationExpressionRequest request)
    {
      var arrayElementType = request.ActivationType.GetElementType();

      var arrayExpressionList = GetArrayExpressionList(scope, request, arrayElementType);

      Expression arrayInit = Expression.NewArrayInit(arrayElementType, arrayExpressionList.Select(e => e.Expression));

      if (request.EnumerableComparer != null)
      {
        arrayInit = CreateSortedArrayExpression(arrayInit, arrayElementType, request);
      }

      var returnResult = request.Services.Compiler.CreateNewResult(request, arrayInit);

      foreach (var result in arrayExpressionList)
      {
        returnResult.AddExpressionResult(result);
      }

      return returnResult;
    }

    /// <summary>Create an expression to sort the array</summary>
    /// <param name="arrayInit"></param>
    /// <param name="arrayElementType"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual Expression CreateSortedArrayExpression(
      Expression arrayInit, Type arrayElementType, IActivationExpressionRequest request)
    {
      // ## 苦竹 修改 ##
      //var compareInterface = typeof(IComparer<>).MakeGenericType(arrayElementType);
      var compareInterface = typeof(IComparer<>).GetCachedGenericType(arrayElementType);

      if (request.EnumerableComparer.GetType().GetTypeInfo().IsAssignableFrom(compareInterface.GetTypeInfo()))
      {
        return arrayInit;
      }

      const string _sortArrayMethodName = nameof(ArrayExpressionCreator.SortArray);
      var openMethod = typeof(ArrayExpressionCreator).GetRuntimeMethods()
          .First(m => string.Equals(_sortArrayMethodName, m.Name, StringComparison.Ordinal));

      var closedMethod = openMethod.MakeGenericMethod(arrayElementType);

      return Expression.Call(closedMethod, arrayInit, Expression.Constant(request.EnumerableComparer));
    }

    /// <summary>Get list of expressions to populate array</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <param name="arrayElementType"></param>
    /// <returns></returns>
    protected virtual List<IActivationExpressionResult> GetArrayExpressionList(
      IInjectionScope scope, IActivationExpressionRequest request, Type arrayElementType)
    {
      var expressions = GetActivationExpressionResultsFromStrategies(scope, request, arrayElementType);

      if (expressions.Count != 0) { return expressions; }

      lock (scope.GetLockObject(InjectionScope.ActivationStrategyAddLockName))
      {
        expressions = GetActivationExpressionResultsFromStrategies(scope, request, arrayElementType);

        if (expressions.Count != 0) { return expressions; }

        request.Services.Compiler.ProcessMissingStrategyProviders(scope,
            request.Services.Compiler.CreateNewRequest(arrayElementType, request.ObjectGraphDepth + 1, scope));

        expressions = GetActivationExpressionResultsFromStrategies(scope, request, arrayElementType);
      }

      return expressions;
    }

    /// <summary>Get activation expression for export strategies</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <param name="arrayElementType"></param>
    /// <returns></returns>
    protected virtual List<IActivationExpressionResult> GetActivationExpressionResultsFromStrategies(
      IInjectionScope scope, IActivationExpressionRequest request, Type arrayElementType)
    {
      var parentStrategy = GetRequestingStrategy(request);

      List<object> keys = null;
      var collection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(arrayElementType);
      var expressions = new List<IActivationExpressionResult>();

      if (request.LocateKey != null)
      {
        if (request.LocateKey is IEnumerable enumerableKey && !(request.LocateKey is string))
        {
          keys = new List<object>();

          foreach (var value in enumerableKey)
          {
            keys.Add(value);
          }
        }
        else
        {
          keys = new List<object> { request.LocateKey };
        }
      }

      if (collection != null)
      {
        if (keys != null)
        {
          for (var i = 0; i < keys.Count;)
          {
            var strategy = collection.GetKeyedStrategy(keys[i]);

            if (strategy != null && parentStrategy != strategy)
            {
              var newRequest = request.NewRequest(arrayElementType, request.RequestingStrategy,
                  request.RequestingStrategy?.ActivationType, request.RequestType, request.Info, true, true);

              var expression = strategy.GetActivationExpression(scope, newRequest);

              if (expression != null)
              {
                expressions.Add(expression);
                keys.RemoveAt(i);
              }
              else
              {
                i++;
              }
            }
            else
            {
              i++;
            }
          }
        }
        else
        {
          foreach (var strategy in collection.GetStrategies())
          {
            // skip as part of the composite pattern
            if (strategy == parentStrategy)
            {
              continue;
            }

            // filter strategies
            if (request.Filter != null && !request.Filter(strategy))
            {
              continue;
            }

            var newRequest = request.NewRequest(arrayElementType, request.RequestingStrategy,
                request.RequestingStrategy?.ActivationType, request.RequestType, request.Info, true, true);

            var expression = strategy.GetActivationExpression(scope, newRequest);

            if (expression != null)
            {
              expressions.Add(expression);
            }
          }
        }
      }

      // check for generic
      if (arrayElementType.IsConstructedGenericType())
      {
        var genericType = arrayElementType.GetGenericTypeDefinition();

        var strategies = scope.StrategyCollectionContainer.GetActivationStrategyCollection(genericType);

        if (strategies != null)
        {
          if (keys != null)
          {
            for (var i = 0; i < keys.Count;)
            {
              var strategy = strategies.GetKeyedStrategy(keys[i]);

              if (strategy != null && strategy != parentStrategy)
              {
                var newRequest = request.NewRequest(arrayElementType, request.RequestingStrategy,
                    request.RequestingStrategy?.ActivationType, request.RequestType,
                    request.Info, true, true);

                var expression = strategy.GetActivationExpression(scope, newRequest);

                if (expression != null)
                {
                  expressions.Add(expression);
                  keys.RemoveAt(i);
                }
                else
                {
                  i++;
                }
              }
              else
              {
                i++;
              }
            }
          }
          else
          {
            foreach (var strategy in strategies.GetStrategies())
            {
              // skip as part of the composite pattern
              if (strategy == parentStrategy)
              {
                continue;
              }

              // filter strategies
              if (request.Filter != null && !request.Filter(strategy))
              {
                continue;
              }

              var newRequest = request.NewRequest(arrayElementType, request.RequestingStrategy,
                  request.RequestingStrategy?.ActivationType, request.RequestType,
                  request.Info, true, true);

              var expression = strategy.GetActivationExpression(scope, newRequest);

              if (expression != null)
              {
                expressions.Add(expression);
              }
            }
          }
        }
      }

      if (expressions.Count == 0)
      {
        ProcessWrappers(scope, arrayElementType, request, expressions);
      }

      return expressions;
    }

    private IActivationStrategy GetRequestingStrategy(IActivationExpressionRequest request)
    {
      if (request == null) { return null; }

      if (request.RequestingStrategy != null &&
          request.RequestingStrategy.StrategyType == ActivationStrategyType.ExportStrategy)
      {
        return request.RequestingStrategy;
      }

      return GetRequestingStrategy(request.Parent);
    }

    /// <summary>Process wrappers looking for matching type</summary>
    /// <param name="scope"></param>
    /// <param name="arrayElementType"></param>
    /// <param name="request"></param>
    /// <param name="expressions"></param>
    protected virtual void ProcessWrappers(IInjectionScope scope, Type arrayElementType,
      IActivationExpressionRequest request, List<IActivationExpressionResult> expressions)
    {
      var wrappers = _wrapperExpressionCreator.GetWrappers(scope, arrayElementType, request, out Type wrappedType);

      if (wrappers != ImmutableLinkedList<IActivationPathNode>.Empty)
      {
        wrappers = wrappers.Reverse();

        GetExpressionsFromCollections(scope, arrayElementType, request, expressions, wrappedType, wrappers);

        if (expressions.Count == 0)
        {
          lock (scope.GetLockObject(InjectionScope.ActivationStrategyAddLockName))
          {
            GetExpressionsFromCollections(scope, arrayElementType, request, expressions, wrappedType, wrappers);

            if (expressions.Count == 0)
            {
              var newRequest = request.NewRequest(arrayElementType, request.RequestingStrategy,
                  request.RequestingStrategy?.ActivationType, RequestType.Other, null, true, true);

              request.Services.Compiler.ProcessMissingStrategyProviders(scope, newRequest);

              GetExpressionsFromCollections(scope, arrayElementType, request, expressions, wrappedType, wrappers);
            }
          }
        }
      }
    }

    /// <summary>Sort an array using a IComparer</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arrayOfT"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static T[] SortArray<T>(T[] arrayOfT, IComparer<T> comparer)
    {
      Array.Sort(arrayOfT, comparer);

      return arrayOfT;
    }

    /// <summary>Get expression from collections</summary>
    /// <param name="scope"></param>
    /// <param name="arrayElementType"></param>
    /// <param name="request"></param>
    /// <param name="expressions"></param>
    /// <param name="wrappedType"></param>
    /// <param name="wrappers"></param>
    public static void GetExpressionsFromCollections(IInjectionScope scope, Type arrayElementType, IActivationExpressionRequest request,
      List<IActivationExpressionResult> expressions, Type wrappedType, ImmutableLinkedList<IActivationPathNode> wrappers)
    {
      var collection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(wrappedType);

      if (collection != null)
      {
        GetExpressionFromCollection(scope, arrayElementType, request, expressions, collection, wrappedType, wrappers);
      }

      var isGenericType = wrappedType.IsConstructedGenericType();

      if (isGenericType)
      {
        var genericType = wrappedType.GetGenericTypeDefinition();

        collection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(genericType);

        if (collection != null)
        {
          GetExpressionFromCollection(scope, arrayElementType, request, expressions, collection, wrappedType, wrappers);
        }
      }
    }

    /// <summary>Get expression from an activation strategy collection</summary>
    /// <param name="scope"></param>
    /// <param name="arrayElementType"></param>
    /// <param name="request"></param>
    /// <param name="expressions"></param>
    /// <param name="collection"></param>
    /// <param name="wrappedType"></param>
    /// <param name="wrappers"></param>
    public static void GetExpressionFromCollection(IInjectionScope scope, Type arrayElementType, IActivationExpressionRequest request,
      List<IActivationExpressionResult> expressions, IActivationStrategyCollection<ICompiledExportStrategy> collection, Type wrappedType,
      ImmutableLinkedList<IActivationPathNode> wrappers)
    {
      foreach (var strategy in collection.GetStrategies())
      {
        if (strategy.HasConditions)
        {
          var staticContext = request.GetStaticInjectionContext();
          var pass = true;

          foreach (var condition in strategy.Conditions)
          {
            if (!condition.MeetsCondition(strategy, staticContext))
            {
              pass = false;
              break;
            }
          }

          if (!pass) { continue; }
        }

        var newRequest = request.NewRequest(arrayElementType, request.RequestingStrategy, request.RequestingStrategy?.ActivationType,
            RequestType.Other, null, true, true);

        var newPath =
            ImmutableLinkedList<IActivationPathNode>.Empty.Add(
                new WrapperActivationPathNode(strategy,
                    wrappedType, null)).AddRange(wrappers);

        newRequest.SetWrapperPath(newPath);

        var wrapper = newRequest.PopWrapperPathNode();

        expressions.Add(wrapper.GetActivationExpression(scope, newRequest));
      }
    }
  }
}