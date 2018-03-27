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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace FastExpressionCompiler
{
  partial class ExpressionCompiler
  {
    private sealed class BlockInfo
    {
      public static readonly BlockInfo Empty = new BlockInfo();

      public bool IsEmpty => Parent == null;
      public readonly BlockInfo Parent;
      public readonly object ResultExpr;
      public readonly object[] VarExprs;
      public readonly LocalBuilder[] LocalVars;

      private BlockInfo() { }

      internal BlockInfo(BlockInfo parent, object resultExpr, object[] varExprs, LocalBuilder[] localVars)
      {
        Parent = parent;
        ResultExpr = resultExpr;
        VarExprs = varExprs;
        LocalVars = localVars;
      }
    }

    [DebuggerDisplay("Expression={ConstantExpr}")]
    private readonly struct ConstantInfo
    {
      public readonly object ConstantExpr, Value;
      public readonly Type Type;

      public ConstantInfo(object constantExpr, object value, Type type)
      {
        ConstantExpr = constantExpr;
        Value = value;
        Type = type;
      }
    }

    // todo: Rename to Context in next major version, cause it is not only about closure anymore
    private sealed class ClosureInfo
    {
      // Closed values used by expression and by its nested lambdas
      public ConstantInfo[] Constants = Tools.Empty<ConstantInfo>();

      // Parameters not passed through lambda parameter list But used inside lambda body.
      // The top expression should not! contain non passed parameters. 
      public object[] NonPassedParameters = Tools.Empty<object>();

      // All nested lambdas recursively nested in expression
      public NestedLambdaInfo[] NestedLambdas = Tools.Empty<NestedLambdaInfo>();

      // FieldInfos are needed to load field of closure object on stack in emitter
      // It is also an indicator that we use typed Closure object and not an array
      public FieldInfo[] Fields { get; private set; }

      // Type of constructed closure, is known after ConstructClosure call
      public Type ClosureType { get; private set; }

      // Known after ConstructClosure call
      public int ClosedItemCount { get; private set; }

      // Helper member to decide when we are inside in a block or not
      public BlockInfo CurrentBlock = BlockInfo.Empty;

      // Tells that we should construct a bounded closure object for the compiled delegate,
      // also indicates that we have to shift when we are operating on arguments 
      // because the first argument should be the closure
      public bool HasBoundClosure { get; private set; }

      public void AddConstant(object expr, object value, Type type)
      {
        if (Constants.Length == 0 ||
            Constants.GetFirstIndex(it => it.ConstantExpr == expr) == -1)
        {
          Constants = Constants.WithLast(new ConstantInfo(expr, value, type));
        }
      }

      public void AddConstant(ConstantInfo info)
      {
        if (Constants.Length == 0 ||
            Constants.GetFirstIndex(it => it.ConstantExpr == info.ConstantExpr) == -1)
        {
          Constants = Constants.WithLast(info);
        }
      }

      public void AddNonPassedParam(object exprObj)
      {
        if (NonPassedParameters.Length == 0 ||
            NonPassedParameters.GetFirstIndex(exprObj) == -1)
          NonPassedParameters = NonPassedParameters.WithLast(exprObj);
      }

      public void AddNestedLambda(object lambdaExpr, object lambda, ClosureInfo closureInfo, bool isAction)
      {
        if (NestedLambdas.Length == 0 ||
            NestedLambdas.GetFirstIndex(it => it.LambdaExpr == lambdaExpr) == -1)
        {
          NestedLambdas = NestedLambdas.WithLast(new NestedLambdaInfo(closureInfo, lambdaExpr, lambda, isAction));
        }
      }

      public void AddNestedLambda(NestedLambdaInfo info)
      {
        if (NestedLambdas.Length == 0 ||
            NestedLambdas.GetFirstIndex(it => it.LambdaExpr == info.LambdaExpr) == -1)
        {
          NestedLambdas = NestedLambdas.WithLast(info);
        }
      }

      public object ConstructClosure(bool closureTypeOnly)
      {
        var constants = Constants;
        var nonPassedParams = NonPassedParameters;
        var nestedLambdas = NestedLambdas;

        var constPlusParamCount = constants.Length + nonPassedParams.Length;
        var totalItemCount = constPlusParamCount + nestedLambdas.Length;

        ClosedItemCount = totalItemCount;

        var closureCreateMethods = Closure.CreateMethods;

        // Construct the array based closure when number of values is bigger than
        // number of fields in biggest supported Closure class.
        if (totalItemCount > closureCreateMethods.Length)
        {
          ClosureType = typeof(ArrayClosure);

          if (closureTypeOnly) { return null; }

          var items = new object[totalItemCount];
          if (constants.Length != 0)
          {
            for (var i = 0; i < constants.Length; i++)
            {
              items[i] = constants[i].Value;
            }
          }

          // skip non passed parameters as it is only for nested lambdas

          if (nestedLambdas.Length != 0)
          {
            for (var i = 0; i < nestedLambdas.Length; i++)
            {
              items[constPlusParamCount + i] = nestedLambdas[i].Lambda;
            }
          }

          return new ArrayClosure(items);
        }

        // Construct the Closure Type and optionally Closure object with closed values stored as fields:
        object[] fieldValues = null;
        var fieldTypes = new Type[totalItemCount];
        if (closureTypeOnly)
        {
          if (constants.Length != 0)
          {
            for (var i = 0; i < constants.Length; i++)
            {
              fieldTypes[i] = constants[i].Type;
            }
          }

          if (nonPassedParams.Length != 0)
          {
            for (var i = 0; i < nonPassedParams.Length; i++)
            {
              fieldTypes[constants.Length + i] = nonPassedParams[i].GetResultType();
            }
          }

          if (nestedLambdas.Length != 0)
          {
            for (var i = 0; i < nestedLambdas.Length; i++)
            {
              fieldTypes[constPlusParamCount + i] = nestedLambdas[i].Lambda.GetType();
            }
          }
        }
        else
        {
          fieldValues = new object[totalItemCount];

          if (constants.Length != 0)
          {
            for (var i = 0; i < constants.Length; i++)
            {
              var constantExpr = constants[i];
              fieldTypes[i] = constantExpr.Type;
              fieldValues[i] = constantExpr.Value;
            }
          }

          if (nonPassedParams.Length != 0)
          {
            for (var i = 0; i < nonPassedParams.Length; i++)
            {
              fieldTypes[constants.Length + i] = nonPassedParams[i].GetResultType();
            }
          }

          if (nestedLambdas.Length != 0)
          {
            for (var i = 0; i < nestedLambdas.Length; i++)
            {
              var lambda = nestedLambdas[i].Lambda;
              fieldValues[constPlusParamCount + i] = lambda;
              fieldTypes[constPlusParamCount + i] = lambda.GetType();
            }
          }
        }

        var createClosureMethod = closureCreateMethods[totalItemCount - 1];
        var createClosure = createClosureMethod.MakeGenericMethod(fieldTypes);
        ClosureType = createClosure.ReturnType;

#if NET40
        var fields = ClosureType.GetTypeDeclaredFields();
#else
        var fields = ClosureType.GetTypeInfo().DeclaredFields;
#endif
        Fields = fields as FieldInfo[] ?? fields.ToArray();

        if (fieldValues == null) { return null; }
        return createClosure.Invoke(null, fieldValues);
      }

      public void FinishAnalysis() => HasBoundClosure = Constants.Length != 0 || NestedLambdas.Length != 0 || NonPassedParameters.Length != 0;

      public void PushBlock(object blockResultExpr, object[] blockVarExprs, LocalBuilder[] localVars) =>
          CurrentBlock = new BlockInfo(CurrentBlock, blockResultExpr, blockVarExprs, localVars);

      public void PushBlockAndConstructLocalVars(object blockResultExpr, object[] blockVarExprs, ILGenerator il)
      {
        var localVars = Tools.Empty<LocalBuilder>();
        if (blockVarExprs.Length != 0)
        {
          localVars = new LocalBuilder[blockVarExprs.Length];
          for (var i = 0; i < localVars.Length; i++)
          {
            localVars[i] = il.DeclareLocal(blockVarExprs[i].GetResultType());
          }
        }

        CurrentBlock = new BlockInfo(CurrentBlock, blockResultExpr, blockVarExprs, localVars);
      }

      public void PopBlock() => CurrentBlock = CurrentBlock.Parent;

      public bool IsLocalVar(object varParamExpr)
      {
        var i = -1;
        for (var block = CurrentBlock; i == -1 && !block.IsEmpty; block = block.Parent)
        {
          i = block.VarExprs.GetFirstIndex(varParamExpr);
        }

        return i != -1;
      }

      public LocalBuilder GetDefinedLocalVarOrDefault(object varParamExpr)
      {
        for (var block = CurrentBlock; !block.IsEmpty; block = block.Parent)
        {
          if (block.LocalVars.Length == 0) { continue; }
          var varIndex = block.VarExprs.GetFirstIndex(varParamExpr);
          if (varIndex != -1) { return block.LocalVars[varIndex]; }
        }
        return null;
      }
    }

  }
}
