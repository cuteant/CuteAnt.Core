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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace FastExpressionCompiler
{
  partial class ExpressionCompiler
  {
    private static bool IsBoundConstant(object value)
    {
      if (value == null) { return false; }

#if NET40
      var typeInfo = value.GetType();
#else
      var typeInfo = value.GetType().GetTypeInfo();
#endif
      return !typeInfo.IsPrimitive
             && !(value is string)
             && !(value is Type)
             && !typeInfo.IsEnum;
    }

    // @paramExprs is required for nested lambda compilation
    private static bool TryCollectBoundConstants(ref ClosureInfo closure, object exprObj, ExpressionType exprNodeType, Type exprType, IList<ParameterExpression> paramExprs)
    {
      if (exprObj == null) { return false; }

      switch (exprNodeType)
      {
        case ExpressionType.Constant:
          var constExprInfo = exprObj as ConstantExpressionInfo;
          var value = constExprInfo != null ? constExprInfo.Value : ((ConstantExpression)exprObj).Value;
          if (value is Delegate || IsBoundConstant(value))
          {
            (closure ?? (closure = new ClosureInfo())).AddConstant(exprObj, value, exprType);
          }
          return true;

        case ExpressionType.Parameter:
          // if parameter is used BUT is not in passed parameters and not in local variables,
          // it means parameter is provided by outer lambda and should be put in closure for current lambda
          var exprInfo = exprObj as ParameterExpressionInfo;
          var paramExpr = exprInfo ?? (ParameterExpression)exprObj;
          if (paramExprs.IndexOf(paramExpr) == -1 && (closure == null || !closure.IsLocalVar(paramExpr)))
          {
            (closure ?? (closure = new ClosureInfo())).AddNonPassedParam(paramExpr);
          }
          return true;

        case ExpressionType.Call:
          return TryCollectCallExprConstants(ref closure, exprObj, paramExprs);

        case ExpressionType.MemberAccess:
          var memberExprInfo = exprObj as MemberExpressionInfo;
          if (memberExprInfo != null)
          {
            var maExpr = memberExprInfo.Expression;
            return maExpr == null
                || TryCollectBoundConstants(ref closure, maExpr, maExpr.GetNodeType(), maExpr.GetResultType(), paramExprs);
          }

          var memberExpr = ((MemberExpression)exprObj).Expression;
          return memberExpr == null
              || TryCollectBoundConstants(ref closure, memberExpr, memberExpr.NodeType, memberExpr.Type, paramExprs);

        case ExpressionType.New:
          var newExprInfo = exprObj as NewExpressionInfo;
          return newExprInfo != null
              ? TryCollectBoundConstants(ref closure, newExprInfo.Arguments, paramExprs)
              : TryCollectBoundConstants(ref closure, ((NewExpression)(Expression)exprObj).Arguments, paramExprs);

        case ExpressionType.NewArrayBounds:
        case ExpressionType.NewArrayInit:
          var newArrayInitInfo = exprObj as NewArrayExpressionInfo;
          if (newArrayInitInfo != null)
          {
            return TryCollectBoundConstants(ref closure, newArrayInitInfo.Arguments, paramExprs);
          }
          return TryCollectBoundConstants(ref closure, ((NewArrayExpression)exprObj).Expressions, paramExprs);

        case ExpressionType.MemberInit:
          return TryCollectMemberInitExprConstants(ref closure, exprObj, paramExprs);

        case ExpressionType.Lambda:
          return TryCompileNestedLambda(ref closure, exprObj, paramExprs);

        case ExpressionType.Invoke:
          var invokeExpr = exprObj as InvocationExpression;
          if (invokeExpr != null)
          {
            var lambda = invokeExpr.Expression;
            return TryCollectBoundConstants(ref closure, lambda, lambda.NodeType, lambda.Type, paramExprs)
                   && TryCollectBoundConstants(ref closure, invokeExpr.Arguments, paramExprs);
          }
          else
          {
            var invokeInfo = (InvocationExpressionInfo)exprObj;
            var lambda = invokeInfo.LambdaExprInfo;
            return TryCollectBoundConstants(ref closure, lambda, lambda.NodeType, lambda.Type, paramExprs)
                   && TryCollectBoundConstants(ref closure, invokeInfo.Arguments, paramExprs);
          }

        case ExpressionType.Conditional:
          var condExpr = (ConditionalExpression)exprObj;
          return TryCollectBoundConstants(ref closure, condExpr.Test, condExpr.Test.NodeType, condExpr.Type, paramExprs)
              && TryCollectBoundConstants(ref closure, condExpr.IfTrue, condExpr.IfTrue.NodeType, condExpr.Type, paramExprs)
              && TryCollectBoundConstants(ref closure, condExpr.IfFalse, condExpr.IfFalse.NodeType, condExpr.IfFalse.Type, paramExprs);

        case ExpressionType.Block:
          var blockExpr = (BlockExpression)exprObj;
          closure = closure ?? new ClosureInfo();
          closure.PushBlock(blockExpr.Result, blockExpr.Variables, Tools.Empty<LocalBuilder>());
          var result = TryCollectBoundConstants(ref closure, blockExpr.Expressions, paramExprs);
          closure.PopBlock();
          return result;

        case ExpressionType.Index:
          var indexExpr = (IndexExpression)exprObj;
          var obj = indexExpr.Object;
          return obj == null
              || TryCollectBoundConstants(ref closure, indexExpr.Object, indexExpr.Object.NodeType, indexExpr.Object.Type, paramExprs)
              && TryCollectBoundConstants(ref closure, indexExpr.Arguments, paramExprs);

        case ExpressionType.Try:
          return TryCollectTryExprConstants(ref closure, exprObj, paramExprs);

        case ExpressionType.Default:
          return true;

        default:
          return TryCollectUnaryOrBinaryExprConstants(ref closure, exprObj, paramExprs);
      }
    }

    private static bool TryCollectBoundConstants(ref ClosureInfo closure, object[] exprObjects, IList<ParameterExpression> paramExprs)
    {
      for (var i = 0; i < exprObjects.Length; i++)
      {
        var exprObj = exprObjects[i];
        var exprMeta = GetExpressionMeta(exprObj);
        if (!TryCollectBoundConstants(ref closure, exprObj, exprMeta.Key, exprMeta.Value, paramExprs))
        {
          return false;
        }
      }
      return true;
    }

    private static bool TryCompileNestedLambda(ref ClosureInfo closure, object exprObj, IList<ParameterExpression> paramExprs)
    {
      // 1. Try to compile nested lambda in place
      // 2. Check that parameters used in compiled lambda are passed or closed by outer lambda
      // 3. Add the compiled lambda to closure of outer lambda for later invocation

      object compiledLambda;
      Type bodyType;
      ClosureInfo nestedClosure = null;

      if (exprObj is LambdaExpressionInfo lambdaExprInfo)
      {
        var lambdaParamExprs = lambdaExprInfo.Parameters;
        var body = lambdaExprInfo.Body;
        bodyType = body.GetResultType();
        compiledLambda = TryCompile(ref nestedClosure,
            lambdaExprInfo.Type, Tools.GetParamExprTypes(lambdaParamExprs), bodyType,
            body, body.GetNodeType(), bodyType,
            lambdaParamExprs, isNestedLambda: true);
      }
      else
      {
        var lambdaExpr = (LambdaExpression)exprObj;
        var lambdaParamExprs = lambdaExpr.Parameters;
        var bodyExpr = lambdaExpr.Body;
        bodyType = bodyExpr.Type;
        compiledLambda = TryCompile(ref nestedClosure,
            lambdaExpr.Type, Tools.GetParamExprTypes(lambdaParamExprs), bodyType,
            bodyExpr, bodyExpr.NodeType, bodyExpr.Type,
            lambdaParamExprs, isNestedLambda: true);
      }

      if (compiledLambda == null) { return false; }

      // add the nested lambda into closure
      (closure ?? (closure = new ClosureInfo()))
          .AddNestedLambda(exprObj, compiledLambda, nestedClosure, isAction: bodyType == typeof(void));

      if (nestedClosure == null)
      {
        return true; // done
      }

      // if nested non passed parameter is no matched with any outer passed parameter, 
      // then ensure it goes to outer non passed parameter.
      // But check that have non passed parameter in root expression is invalid.
      var nestedNonPassedParams = nestedClosure.NonPassedParameters;
      if (nestedNonPassedParams.Length != 0)
      {
        for (var i = 0; i < nestedNonPassedParams.Length; i++)
        {
          var nestedNonPassedParam = nestedNonPassedParams[i];
          if (paramExprs.Count == 0 ||
              paramExprs.IndexOf(nestedNonPassedParam) == -1)
          {
            closure.AddNonPassedParam(nestedNonPassedParam);
          }
        }
      }

      // Promote found constants and nested lambdas into outer closure
      var nestedConstants = nestedClosure.Constants;
      if (nestedConstants.Length != 0)
      {
        for (var i = 0; i < nestedConstants.Length; i++)
        {
          closure.AddConstant(nestedConstants[i]);
        }
      }

      var nestedNestedLambdas = nestedClosure.NestedLambdas;
      if (nestedNestedLambdas.Length != 0)
      {
        for (var i = 0; i < nestedNestedLambdas.Length; i++)
        {
          closure.AddNestedLambda(nestedNestedLambdas[i]);
        }
      }

      return true;

    }

    private static bool TryCollectMemberInitExprConstants(ref ClosureInfo closure, object exprObj, IList<ParameterExpression> paramExprs)
    {
      if (exprObj is MemberInitExpressionInfo memberInitExprInfo)
      {
        var miNewInfo = memberInitExprInfo.ExpressionInfo;
        if (!TryCollectBoundConstants(ref closure, miNewInfo, miNewInfo.NodeType, miNewInfo.Type, paramExprs))
        {
          return false;
        }

        var memberBindingInfos = memberInitExprInfo.Bindings;
        for (var i = 0; i < memberBindingInfos.Length; i++)
        {
          var maInfo = memberBindingInfos[i].Expression;
          if (!TryCollectBoundConstants(ref closure, maInfo, maInfo.NodeType, maInfo.Type, paramExprs))
          {
            return false;
          }
        }
        return true;
      }
      else
      {
        var memberInitExpr = (MemberInitExpression)exprObj;
        var miNewExpr = memberInitExpr.NewExpression;
        if (!TryCollectBoundConstants(ref closure, miNewExpr, miNewExpr.NodeType, miNewExpr.Type, paramExprs))
        {
          return false;
        }

        var memberBindings = memberInitExpr.Bindings;
        for (var i = 0; i < memberBindings.Count; ++i)
        {
          var memberBinding = memberBindings[i];
          var maExpr = ((MemberAssignment)memberBinding).Expression;
          if (memberBinding.BindingType == MemberBindingType.Assignment && !TryCollectBoundConstants(ref closure, maExpr, maExpr.NodeType, maExpr.Type, paramExprs))
          {
            return false;
          }
        }
      }

      return true;

    }

    private static bool TryCollectTryExprConstants(ref ClosureInfo closure, object exprObj, IList<ParameterExpression> paramExprs)
    {
      var tryExpr = (TryExpression)exprObj;
      if (!TryCollectBoundConstants(ref closure, tryExpr.Body, tryExpr.Body.NodeType, tryExpr.Type, paramExprs))
      {
        return false;
      }

      var catchBlocks = tryExpr.Handlers;
      for (var i = 0; i < catchBlocks.Count; i++)
      {
        var block = catchBlocks[i];
        var blockBody = block.Body;

        var blockExceptionVar = block.Variable;
        if (blockExceptionVar != null)
        {
          closure = closure ?? new ClosureInfo();
          closure.PushBlock(blockBody, new[] { blockExceptionVar }, Tools.Empty<LocalBuilder>());

          if (!TryCollectBoundConstants(ref closure, blockExceptionVar, blockExceptionVar.NodeType, blockExceptionVar.Type, paramExprs))
          {
            return false;
          }
        }

        if (block.Filter != null && !TryCollectBoundConstants(ref closure, block.Filter, block.Filter.NodeType, block.Filter.Type, paramExprs))
        {
          return false;
        }

        if (!TryCollectBoundConstants(ref closure, blockBody, blockBody.NodeType, block.Test, paramExprs))
        {
          return false;
        }

        if (blockExceptionVar != null) { closure.PopBlock(); }
      }

      if (tryExpr.Finally != null && !TryCollectBoundConstants(ref closure, tryExpr.Finally, tryExpr.Finally.NodeType, tryExpr.Finally.Type, paramExprs))
      {
        return false;
      }

      return true;
    }

    private static bool TryCollectUnaryOrBinaryExprConstants(ref ClosureInfo closure, object exprObj, IList<ParameterExpression> paramExprs)
    {
      if (exprObj is ExpressionInfo)
      {
        if (exprObj is UnaryExpressionInfo unaryExprInfo)
        {
          var opInfo = unaryExprInfo.Operand;
          return TryCollectBoundConstants(ref closure, opInfo, opInfo.NodeType, opInfo.Type, paramExprs);
        }

        if (exprObj is BinaryExpressionInfo binInfo)
        {
          var left = binInfo.Left;
          var right = binInfo.Right;
          return TryCollectBoundConstants(ref closure, left, left.GetNodeType(), left.GetResultType(), paramExprs)
                 && TryCollectBoundConstants(ref closure, right, right.GetNodeType(), right.GetResultType(), paramExprs);
        }

        return false;
      }

      if (exprObj is UnaryExpression unaryExpr)
      {
        var opExpr = unaryExpr.Operand;
        return TryCollectBoundConstants(ref closure, opExpr, opExpr.NodeType, opExpr.Type, paramExprs);
      }

      if (exprObj is BinaryExpression binaryExpr)
      {
        var leftExpr = binaryExpr.Left;
        var rightExpr = binaryExpr.Right;
        return TryCollectBoundConstants(ref closure, leftExpr, leftExpr.NodeType, leftExpr.Type, paramExprs)
               && TryCollectBoundConstants(ref closure, rightExpr, rightExpr.NodeType, rightExpr.Type, paramExprs);
      }

      return false;
    }

    private static bool TryCollectCallExprConstants(ref ClosureInfo closure, object exprObj, IList<ParameterExpression> paramExprs)
    {
      if (exprObj is MethodCallExpressionInfo callInfo)
      {
        var objInfo = callInfo.Object;
        return (objInfo == null
                || TryCollectBoundConstants(ref closure, objInfo, objInfo.NodeType, objInfo.Type, paramExprs))
               && TryCollectBoundConstants(ref closure, callInfo.Arguments, paramExprs);
      }

      var callExpr = (MethodCallExpression)exprObj;
      var objExpr = callExpr.Object;
      return (objExpr == null
              || TryCollectBoundConstants(ref closure, objExpr, objExpr.NodeType, objExpr.Type, paramExprs))
             && TryCollectBoundConstants(ref closure, callExpr.Arguments, paramExprs);
    }

    private static KeyValuePair<ExpressionType, Type> GetExpressionMeta(object exprObj)
    {
      if (exprObj is Expression expr)
      {
        return new KeyValuePair<ExpressionType, Type>(expr.NodeType, expr.Type);
      }

      var exprInfo = (ExpressionInfo)exprObj;
      return new KeyValuePair<ExpressionType, Type>(exprInfo.NodeType, exprInfo.Type);
    }

    private static bool TryCollectBoundConstants(ref ClosureInfo closure, IList<Expression> exprs, IList<ParameterExpression> paramExprs)
    {
      for (var i = 0; i < exprs.Count; i++)
      {
        var expr = exprs[i];
        if (!TryCollectBoundConstants(ref closure, expr, expr.NodeType, expr.Type, paramExprs)) { return false; }
      }
      return true;
    }
  }
}
