/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace FastExpressionCompiler
{
  /// <summary>Compiles expression to delegate ~20 times faster than Expression.Compile.
  /// Partial to extend with your things when used as source file.</summary>
  // ReSharper disable once PartialTypeWithSinglePart
  public static partial class ExpressionCompiler
  {
    #region Expression.CompileFast overloads for Delegate, Funcs, and Actions

    /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static TDelegate CompileFast<TDelegate>(this LambdaExpression lambdaExpr, bool ifFastFailedReturnNull = false)
        where TDelegate : class =>
        TryCompile<TDelegate>(lambdaExpr) ??
        (ifFastFailedReturnNull ? null : (TDelegate)(object)lambdaExpr.Compile());

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Delegate CompileFast(this LambdaExpression lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Delegate>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<R> CompileFast<R>(this Expression<Func<R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, R> CompileFast<T1, R>(this Expression<Func<T1, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, R> CompileFast<T1, T2, R>(this Expression<Func<T1, T2, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, R> CompileFast<T1, T2, T3, R>(
        this Expression<Func<T1, T2, T3, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, T4, R> CompileFast<T1, T2, T3, T4, R>(
        this Expression<Func<T1, T2, T3, T4, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, T4, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, T4, T5, R> CompileFast<T1, T2, T3, T4, T5, R>(
        this Expression<Func<T1, T2, T3, T4, T5, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, T4, T5, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, R> CompileFast<T1, T2, T3, T4, T5, T6, R>(
        this Expression<Func<T1, T2, T3, T4, T5, T6, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, T4, T5, T6, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action CompileFast(this Expression<Action> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1> CompileFast<T1>(this Expression<Action<T1>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2> CompileFast<T1, T2>(this Expression<Action<T1, T2>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3> CompileFast<T1, T2, T3>(this Expression<Action<T1, T2, T3>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3, T4> CompileFast<T1, T2, T3, T4>(
        this Expression<Action<T1, T2, T3, T4>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3, T4>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3, T4, T5> CompileFast<T1, T2, T3, T4, T5>(
        this Expression<Action<T1, T2, T3, T4, T5>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3, T4, T5>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3, T4, T5, T6> CompileFast<T1, T2, T3, T4, T5, T6>(
        this Expression<Action<T1, T2, T3, T4, T5, T6>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3, T4, T5, T6>>(ifFastFailedReturnNull);

    #endregion

    #region ExpressionInfo.CompileFast overloads for Delegate, Funcs, and Actions

    /// <summary>Compiles lambda expression info to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static TDelegate CompileFast<TDelegate>(this LambdaExpressionInfo lambdaExpr, bool ifFastFailedReturnNull = false)
        where TDelegate : class =>
        TryCompile<TDelegate>(lambdaExpr) ??
        (ifFastFailedReturnNull ? null : (TDelegate)(object)lambdaExpr.ToLambdaExpression().Compile());

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Delegate CompileFast(this LambdaExpressionInfo lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Delegate>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<R> CompileFast<R>(this ExpressionInfo<Func<R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, R> CompileFast<T1, R>(this ExpressionInfo<Func<T1, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, R> CompileFast<T1, T2, R>(this ExpressionInfo<Func<T1, T2, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, R> CompileFast<T1, T2, T3, R>(
        this ExpressionInfo<Func<T1, T2, T3, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to TDelegate type. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, T4, R> CompileFastInfo<T1, T2, T3, T4, R>(
        this Expression<Func<T1, T2, T3, T4, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, T4, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, T4, T5, R> CompileFast<T1, T2, T3, T4, T5, R>(
        this ExpressionInfo<Func<T1, T2, T3, T4, T5, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, T4, T5, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Func<T1, T2, T3, T4, T5, T6, R> CompileFast<T1, T2, T3, T4, T5, T6, R>(
        this ExpressionInfo<Func<T1, T2, T3, T4, T5, T6, R>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Func<T1, T2, T3, T4, T5, T6, R>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action CompileFast(this ExpressionInfo<Action> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1> CompileFast<T1>(this ExpressionInfo<Action<T1>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2> CompileFast<T1, T2>(this ExpressionInfo<Action<T1, T2>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3> CompileFast<T1, T2, T3>(this ExpressionInfo<Action<T1, T2, T3>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3, T4> CompileFast<T1, T2, T3, T4>(
        this ExpressionInfo<Action<T1, T2, T3, T4>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3, T4>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3, T4, T5> CompileFast<T1, T2, T3, T4, T5>(
        this ExpressionInfo<Action<T1, T2, T3, T4, T5>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3, T4, T5>>(ifFastFailedReturnNull);

    /// <summary>Compiles lambda expression info to delegate. Use ifFastFailedReturnNull parameter to Not fallback to Expression.Compile, useful for testing.</summary>
    public static Action<T1, T2, T3, T4, T5, T6> CompileFast<T1, T2, T3, T4, T5, T6>(
        this ExpressionInfo<Action<T1, T2, T3, T4, T5, T6>> lambdaExpr, bool ifFastFailedReturnNull = false) =>
        lambdaExpr.CompileFast<Action<T1, T2, T3, T4, T5, T6>>(ifFastFailedReturnNull);

    #endregion

    /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate"/>.</summary>
    /// <typeparam name="TDelegate">The compatible delegate type, otherwise it will throw.</typeparam>
    /// <param name="lambdaExpr">Lambda expression to compile.</param>
    /// <returns>Compiled delegate.</returns>
    public static TDelegate TryCompile<TDelegate>(this LambdaExpression lambdaExpr)
      where TDelegate : class
    {
      var paramExprs = lambdaExpr.Parameters;
      var paramTypes = Tools.GetParamExprTypes(paramExprs);
      var expr = lambdaExpr.Body;
      return TryCompile<TDelegate>(expr, paramExprs, paramTypes, expr.Type);
    }

    /// <summary>Compiles expression to delegate by emitting the IL. 
    /// If sub-expressions are not supported by emitter, then the method returns null.
    /// The usage should be calling the method, if result is null then calling the Expression.Compile.</summary>
    /// <param name="bodyExpr">Lambda body.</param>
    /// <param name="paramExprs">Lambda parameter expressions.</param>
    /// <param name="paramTypes">The types of parameters.</param>
    /// <param name="returnType">The return type.</param>
    /// <returns>Result delegate or null, if unable to compile.</returns>
    public static TDelegate TryCompile<TDelegate>(Expression bodyExpr, IList<ParameterExpression> paramExprs, Type[] paramTypes, Type returnType)
      where TDelegate : class
    {
      ClosureInfo ignored = null;
      return (TDelegate)TryCompile(ref ignored,
          typeof(TDelegate), paramTypes, returnType,
          bodyExpr, bodyExpr.NodeType, bodyExpr.Type, paramExprs);
    }

    /// <summary>Tries to compile lambda expression info.</summary>
    /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
    /// <param name="lambdaExpr">Lambda expression to compile.</param>
    /// <returns>Compiled delegate or null.</returns>
    public static TDelegate TryCompile<TDelegate>(this LambdaExpressionInfo lambdaExpr)
      where TDelegate : class
    {
      var paramExprs = lambdaExpr.Parameters;
      var paramTypes = Tools.GetParamExprTypes(paramExprs);
      var body = lambdaExpr.Body;
      var bodyExpr = body as Expression;
      return bodyExpr != null
          ? TryCompile<TDelegate>(bodyExpr, paramExprs, paramTypes, bodyExpr.Type)
          : TryCompile<TDelegate>((ExpressionInfo)body, paramExprs, paramTypes, body.GetResultType());
    }

    /// <summary>Tries to compile lambda expression info.</summary>
    /// <param name="lambdaExpr">Lambda expression to compile.</param>
    /// <returns>Compiled delegate or null.</returns>
    public static Delegate TryCompile(this LambdaExpressionInfo lambdaExpr) => TryCompile<Delegate>(lambdaExpr);

    /// <summary>Tries to compile lambda expression info.</summary>
    /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
    /// <param name="lambdaExpr">Lambda expression to compile.</param>
    /// <returns>Compiled delegate or null.</returns>
    public static TDelegate TryCompile<TDelegate>(this ExpressionInfo<TDelegate> lambdaExpr) where TDelegate : class
        => TryCompile<TDelegate>((LambdaExpressionInfo)lambdaExpr);

    /// <summary>Compiles expression to delegate by emitting the IL. 
    /// If sub-expressions are not supported by emitter, then the method returns null.
    /// The usage should be calling the method, if result is null then calling the Expression.Compile.</summary>
    /// <param name="bodyExpr">Lambda body.</param>
    /// <param name="paramExprs">Lambda parameter expressions.</param>
    /// <param name="paramTypes">The types of parameters.</param>
    /// <param name="returnType">The return type.</param>
    /// <returns>Result delegate or null, if unable to compile.</returns>
    public static TDelegate TryCompile<TDelegate>(ExpressionInfo bodyExpr, IList<ParameterExpression> paramExprs, Type[] paramTypes, Type returnType)
      where TDelegate : class
    {
      ClosureInfo ignored = null;
      return (TDelegate)TryCompile(ref ignored,
          typeof(TDelegate), paramTypes, returnType,
          bodyExpr, bodyExpr.NodeType, bodyExpr.Type, paramExprs);
    }

    private static object TryCompile(ref ClosureInfo closureInfo,
      Type delegateType, Type[] paramTypes, Type returnType,
      object exprObj, ExpressionType exprNodeType, Type exprType,
      IList<ParameterExpression> paramExprs,
      bool isNestedLambda = false)
    {
      if (!TryCollectBoundConstants(ref closureInfo, exprObj, exprNodeType, exprType, paramExprs)) { return null; }

      closureInfo?.FinishAnalysis();

      if (closureInfo == null || !closureInfo.HasBoundClosure)
      {
        return TryCompileStaticDelegate(delegateType,
            paramTypes, returnType, exprObj, exprNodeType, exprType, paramExprs);
      }

      var closureObject = closureInfo.ConstructClosure(closureTypeOnly: isNestedLambda);
      var closureAndParamTypes = GetClosureAndParamTypes(paramTypes, closureInfo.ClosureType);

      var methodWithClosure = new DynamicMethod(string.Empty, returnType, closureAndParamTypes,
          closureInfo.ClosureType, skipVisibility: true);

      if (!TryEmit(methodWithClosure, exprObj, exprNodeType, exprType, paramExprs, closureInfo)) { return null; }

      // include closure as the first parameter, BUT don't bound to it. It will be bound later in EmitNestedLambda.
      if (isNestedLambda)
      {
        return methodWithClosure.CreateDelegate(Tools.GetFuncOrActionType(closureAndParamTypes, returnType));
      }

      // create a specific delegate if user requested delegate is untyped, otherwise CreateMethod will fail
      if (delegateType == typeof(Delegate))
      {
        delegateType = Tools.GetFuncOrActionType(paramTypes, returnType);
      }

      return methodWithClosure.CreateDelegate(delegateType, closureObject);
    }

    private static object TryCompileStaticDelegate(Type delegateType, Type[] paramTypes, Type returnType, object exprObj,
      ExpressionType exprNodeType, Type exprType, IList<ParameterExpression> paramExprs)
    {
      var method = new DynamicMethod(string.Empty, returnType, paramTypes,
          typeof(ExpressionCompiler), skipVisibility: true);

      if (!TryEmit(method, exprObj, exprNodeType, exprType, paramExprs, null)) { return null; }

      // create a specific delegate if user requested delegate is untyped, otherwise CreateMethod will fail
      if (delegateType == typeof(Delegate))
      {
        delegateType = Tools.GetFuncOrActionType(paramTypes, returnType);
      }

      return method.CreateDelegate(delegateType);
    }

    private static bool TryEmit(DynamicMethod method, object exprObj, ExpressionType exprNodeType,
      Type exprType, IList<ParameterExpression> paramExprs, ClosureInfo closureInfo)
    {
      var il = method.GetILGenerator();
      if (!EmittingVisitor.TryEmit(exprObj, exprNodeType, exprType, paramExprs, il, closureInfo)) { return false; }

      il.Emit(OpCodes.Ret); // emits return from generated method
      return true;
    }

    private static Type[] GetClosureAndParamTypes(Type[] paramTypes, Type closureType)
    {
      var paramCount = paramTypes.Length;
      if (paramCount == 0) { return new[] { closureType }; }

      if (paramCount == 1) { return new[] { closureType, paramTypes[0] }; }

      var closureAndParamTypes = new Type[paramCount + 1];
      closureAndParamTypes[0] = closureType;
      Array.Copy(paramTypes, 0, closureAndParamTypes, 1, paramCount);
      return closureAndParamTypes;
    }


  }




















}
