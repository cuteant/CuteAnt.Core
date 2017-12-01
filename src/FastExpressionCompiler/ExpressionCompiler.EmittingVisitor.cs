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

namespace FastExpressionCompiler
{
  partial class ExpressionCompiler
  {
    /// <summary>Supports emitting of selected expressions, e.g. lambdaExpr are not supported yet.
    /// When emitter find not supported expression it will return false from <see cref="TryEmit"/>, so I could fallback
    /// to normal and slow Expression.Compile.</summary>
    private static class EmittingVisitor
    {
      private static readonly MethodInfo _getTypeFromHandleMethod = typeof(Type)
#if NET40
          .GetTypeDeclaredMethods()
#else
          .GetTypeInfo().DeclaredMethods
#endif
          .First(m => m.IsStatic && m.Name == "GetTypeFromHandle");

      private static readonly MethodInfo _objectEqualsMethod = typeof(object)
#if NET40
          .GetTypeDeclaredMethods()
#else
          .GetTypeInfo().DeclaredMethods
#endif
          .First(m => m.IsStatic && m.Name == "Equals");

      #region -- TryEmit --

      public static bool TryEmit(object exprObj, ExpressionType exprNodeType, Type exprType,
        object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        switch (exprNodeType)
        {
          case ExpressionType.Parameter:
            return EmitParameter(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.Convert:
            return EmitConvert(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.ArrayIndex:
            return EmitArrayIndex(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.Constant:
            return EmitConstant(exprObj, exprType, il, closure);
          case ExpressionType.Call:
            return EmitMethodCall(exprObj, paramExprs, il, closure);
          case ExpressionType.MemberAccess:
            return EmitMemberAccess(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.New:
            return EmitNew(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.NewArrayBounds:
          case ExpressionType.NewArrayInit:
            return EmitNewArray(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.MemberInit:
            return EmitMemberInit(exprObj, exprType, paramExprs, il, closure);
          case ExpressionType.Lambda:
            return EmitNestedLambda(exprObj, paramExprs, il, closure);

          case ExpressionType.Invoke:
            return EmitInvokeLambda(exprObj, paramExprs, il, closure);

          case ExpressionType.GreaterThan:
          case ExpressionType.GreaterThanOrEqual:
          case ExpressionType.LessThan:
          case ExpressionType.LessThanOrEqual:
          case ExpressionType.Equal:
          case ExpressionType.NotEqual:
            return EmitComparison((BinaryExpression)exprObj, exprNodeType, paramExprs, il, closure);

          case ExpressionType.Add:
          case ExpressionType.AddChecked:
          case ExpressionType.Subtract:
          case ExpressionType.SubtractChecked:
          case ExpressionType.Multiply:
          case ExpressionType.MultiplyChecked:
          case ExpressionType.Divide:
            return EmitArithmeticOperation(exprObj, exprType, exprNodeType, paramExprs, il, closure);

          case ExpressionType.AndAlso:
          case ExpressionType.OrElse:
            return EmitLogicalOperator((BinaryExpression)exprObj, paramExprs, il, closure);

          case ExpressionType.Coalesce:
            return EmitCoalesceOperator((BinaryExpression)exprObj, paramExprs, il, closure);

          case ExpressionType.Conditional:
            return EmitConditional((ConditionalExpression)exprObj, paramExprs, il, closure);

          case ExpressionType.Assign:
            return EmitAssign(exprObj, exprType, paramExprs, il, closure);

          case ExpressionType.Block:
            return EmitBlock((BlockExpression)exprObj, paramExprs, il, closure);

          case ExpressionType.Try:
            return EmitTryCatchFinallyBlock((TryExpression)exprObj, paramExprs, il, closure);

          case ExpressionType.Throw:
            return EmitThrow((UnaryExpression)exprObj, paramExprs, il, closure);

          case ExpressionType.Default:
            return EmitDefault((DefaultExpression)exprObj, il);

          case ExpressionType.Index:
            return EmitIndex((IndexExpression)exprObj, paramExprs, il, closure);

          default:
            return false;
        }
      }

      #endregion

      #region ** EmitIndex **

      private static bool EmitIndex(IndexExpression exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var obj = exprObj.Object;
        if (obj != null && !TryEmit(obj, obj.NodeType, obj.Type, paramExprs, il, closure)) { return false; }

        var argLength = exprObj.Arguments.Count;
        for (var i = 0; i < argLength; i++)
        {
          var arg = exprObj.Arguments[i];
          if (!TryEmit(arg, arg.NodeType, arg.Type, paramExprs, il, closure)) { return false; }
        }

        return EmitIndexAccess(exprObj, obj?.Type, exprObj.Type, il);
      }

      #endregion

      #region ** EmitCoalesceOperator **

      private static bool EmitCoalesceOperator(BinaryExpression exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var labelFalse = il.DefineLabel();
        var labelDone = il.DefineLabel();

        var left = exprObj.Left;
        var right = exprObj.Right;

        if (!TryEmit(left, left.NodeType, left.Type, paramExprs, il, closure)) { return false; }

        il.Emit(OpCodes.Dup); // duplicate left, if it's not null, after the branch this value will be on the top of the stack
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Brfalse, labelFalse);

        il.Emit(OpCodes.Pop); // left is null, pop its value from the stack

        if (!TryEmit(right, right.NodeType, right.Type, paramExprs, il, closure)) { return false; }

        if (right.Type != exprObj.Type)
        {
#if NET40
          if (right.Type.IsValueType)
#else
          if (right.Type.GetTypeInfo().IsValueType)
#endif
          {
            il.Emit(OpCodes.Box, right.Type);
          }
          else
          {
            il.Emit(OpCodes.Castclass, exprObj.Type);
          }
        }

        il.Emit(OpCodes.Br, labelDone);

        il.MarkLabel(labelFalse);
        if (left.Type != exprObj.Type) { il.Emit(OpCodes.Castclass, exprObj.Type); }

        il.MarkLabel(labelDone);
        return true;
      }

      #endregion

      #region ** EmitDefault **

      private static bool EmitDefault(DefaultExpression exprObj, ILGenerator il)
      {
        var type = exprObj.Type;

        if (type == typeof(void)) { return true; }
        else if (type == typeof(string)) { il.Emit(OpCodes.Ldnull); }
        else if (type == typeof(bool) ||
                type == typeof(byte) ||
                type == typeof(char) ||
                type == typeof(sbyte) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(short) ||
                type == typeof(ushort))
        {
          il.Emit(OpCodes.Ldc_I4_0);
        }
        else if (type == typeof(long) ||
                type == typeof(ulong))
        {
          il.Emit(OpCodes.Ldc_I4_0);
          il.Emit(OpCodes.Conv_I8);
        }
        else if (type == typeof(float))
        {
          il.Emit(OpCodes.Ldc_R4, default(float));
        }
        else if (type == typeof(double))
        {
          il.Emit(OpCodes.Ldc_R8, default(double));
        }
#if NET40
        else if (type.IsValueType)
#else
        else if (type.GetTypeInfo().IsValueType)
#endif
        {
          LocalBuilder lb = il.DeclareLocal(type);
          il.Emit(OpCodes.Ldloca, lb);
          il.Emit(OpCodes.Initobj, type);
          il.Emit(OpCodes.Ldloc, lb);
        }
        else
        {
          il.Emit(OpCodes.Ldnull);
        }

        return true;
      }

      #endregion

      #region ** EmitBlock **

      private static bool EmitBlock(BlockExpression exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        closure = closure ?? new ClosureInfo();
        closure.PushBlockAndConstructLocalVars(exprObj, il);
        if (!EmitMany(exprObj.Expressions, paramExprs, il, closure)) { return false; }
        closure.PopBlock();
        return true;
      }

      #endregion

      #region ** EmitTryCatchFinallyBlock **

      private static bool EmitTryCatchFinallyBlock(TryExpression exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var returnLabel = default(Label);
        var returnResult = default(LocalBuilder);
        var hasResult = exprObj.Type != typeof(void);
        if (hasResult)
        {
          returnLabel = il.DefineLabel();
          returnResult = il.DeclareLocal(exprObj.Type);
        }

        il.BeginExceptionBlock();

        if (!TryEmit(exprObj.Body, exprObj.Body.NodeType, exprObj.Body.Type, paramExprs, il, closure)) { return false; }

        if (hasResult)
        {
          il.Emit(OpCodes.Stloc_S, returnResult);
          il.Emit(OpCodes.Leave_S, returnLabel);
        }

        var catchBlocks = exprObj.Handlers;
        for (var i = 0; i < catchBlocks.Count; i++)
        {
          var catchBlock = catchBlocks[i];

          if (catchBlock.Filter != null)
          {
            return false; // todo: Add support for filters on catch expression
          }

          il.BeginCatchBlock(catchBlock.Test);

          // at the beginning of catch the Exception value is on the stack,
          // we will store into local variable.
          var catchExpr = catchBlock.Body;
          var exceptionVarExpr = catchBlock.Variable;
          if (exceptionVarExpr != null)
          {
            var exceptionVar = il.DeclareLocal(exceptionVarExpr.Type);

            closure = closure ?? new ClosureInfo();
            closure.PushBlock(catchBlock.Body, new[] { exceptionVarExpr }, new[] { exceptionVar });

            // store the values of exception on stack into the variable
            il.Emit(OpCodes.Stloc_S, exceptionVar);
          }

          if (!TryEmit(catchExpr, catchExpr.NodeType, catchExpr.Type, paramExprs, il, closure)) { return false; }

          if (exceptionVarExpr != null) { closure.PopBlock(); }

          if (hasResult)
          {
            il.Emit(OpCodes.Stloc_S, returnResult);
            il.Emit(OpCodes.Leave_S, returnLabel);
          }
          else
          {
            il.Emit(OpCodes.Pop);
          }
        }

        if (exprObj.Finally != null)
        {
          il.BeginFinallyBlock();

          if (!TryEmit(exprObj.Finally, exprObj.Finally.NodeType, exprObj.Finally.Type, paramExprs, il, closure)) { return false; }
        }

        il.EndExceptionBlock();

        if (hasResult)
        {
          il.MarkLabel(returnLabel);
          il.Emit(OpCodes.Ldloc, returnResult);
        }

        return true;
      }

      #endregion

      #region ** EmitThrow **

      private static bool EmitThrow(UnaryExpression exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var exceptionExpr = exprObj.Operand;
        if (!TryEmit(exceptionExpr, exceptionExpr.NodeType, exceptionExpr.Type, paramExprs, il, closure)) { return false; }

        il.ThrowException(exceptionExpr.Type);
        return true;
      }

      #endregion

      #region ** EmitParameter **

      private static bool EmitParameter(object paramExprObj, Type paramType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var paramIndex = paramExprs.GetFirstIndex(paramExprObj);

        // if parameter is passed, then just load it on stack
        if (paramIndex != -1)
        {
          if (closure != null && closure.HasBoundClosure)
          {
            paramIndex += 1; // shift parameter indices by one, because the first one will be closure
          }

          LoadParamArg(il, paramIndex);
          return true;
        }

        // if parameter isn't passed, then it is passed into some outer lambda or it is a local variable,
        // so it should be loaded from closure or from the locals. Then the closure is null will be an invalid state.
        if (closure == null) { return false; }

        // expression may represent variables as a parameters, so first look if this is the case
        var variable = closure.GetDefinedLocalVarOrDefault(paramExprObj);
        if (variable != null)
        {
          il.Emit(OpCodes.Ldloc, variable);
          return true;
        }

        // the only possibility that we are here is because we are in nested lambda,
        // and it uses some parameter or variable from the outer lambda
        var nonPassedParamIndex = closure.NonPassedParameters.GetFirstIndex(paramExprObj);
        if (nonPassedParamIndex == -1)
        {
          return false;  // what??? no chance
        }

        var closureItemIndex = closure.Constants.Length + nonPassedParamIndex;
        LoadClosureFieldOrItem(closure, il, closureItemIndex, paramType);

        return true;
      }

      #endregion

      #region ** LoadParamArg **

      private static void LoadParamArg(ILGenerator il, int paramIndex)
      {
        // todo: consider using Ldarga_S for ValueType
        switch (paramIndex)
        {
          case 0:
            il.Emit(OpCodes.Ldarg_0);
            break;
          case 1:
            il.Emit(OpCodes.Ldarg_1);
            break;
          case 2:
            il.Emit(OpCodes.Ldarg_2);
            break;
          case 3:
            il.Emit(OpCodes.Ldarg_3);
            break;
          default:
            if (paramIndex <= byte.MaxValue)
              il.Emit(OpCodes.Ldarg_S, (byte)paramIndex);
            else
              il.Emit(OpCodes.Ldarg, paramIndex);
            break;
        }
      }

      #endregion

      #region ** EmitBinary **

      private static bool EmitBinary(object exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        if (exprObj is BinaryExpressionInfo exprInfo)
        {
          var left = exprInfo.Left;
          var right = exprInfo.Right;
          return TryEmit(left, left.GetNodeType(), left.GetResultType(), paramExprs, il, closure)
              && TryEmit(right, right.GetNodeType(), right.GetResultType(), paramExprs, il, closure);
        }

        var expr = (BinaryExpression)exprObj;
        var leftExpr = expr.Left;
        var rightExpr = expr.Right;
        return TryEmit(leftExpr, leftExpr.NodeType, leftExpr.Type, paramExprs, il, closure)
            && TryEmit(rightExpr, rightExpr.NodeType, rightExpr.Type, paramExprs, il, closure);
      }

      #endregion

      #region ** EmitMany **

      private static bool EmitMany(IList<Expression> exprs, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        for (int i = 0, n = exprs.Count; i < n; i++)
        {
          var expr = exprs[i];
          if (!TryEmit(expr, expr.NodeType, expr.Type, paramExprs, il, closure)) { return false; }
        }
        return true;
      }

      private static bool EmitMany(IList<object> exprObjects, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        for (int i = 0, n = exprObjects.Count; i < n; i++)
        {
          var exprObj = exprObjects[i];
          var exprMeta = GetExpressionMeta(exprObj);
          if (!TryEmit(exprObj, exprMeta.Key, exprMeta.Value, paramExprs, il, closure)) { return false; }
        }
        return true;
      }

      #endregion

      #region ** EmitConvert **

      private static bool EmitConvert(object exprObj, Type targetType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var exprInfo = exprObj as UnaryExpressionInfo;
        Type sourceType;
        if (exprInfo != null)
        {
          var opInfo = exprInfo.Operand;
          if (!TryEmit(opInfo, opInfo.NodeType, opInfo.Type, paramExprs, il, closure)) { return false; }
          sourceType = opInfo.Type;
        }
        else
        {
          var expr = (UnaryExpression)exprObj;
          var opExpr = expr.Operand;
          if (!TryEmit(opExpr, opExpr.NodeType, opExpr.Type, paramExprs, il, closure)) { return false; }
          sourceType = opExpr.Type;
        }

        if (targetType == sourceType)
        {
          return true; // do nothing, no conversion is needed
        }

        if (targetType == typeof(object))
        {
#if NET40
          if (sourceType.IsValueType)
#else
          if (sourceType.GetTypeInfo().IsValueType)
#endif
          {
            il.Emit(OpCodes.Box, sourceType); // for value type to object, just box a value
          }
          return true; // for reference type we don't need to convert
        }

        // Just un-box type object to the target value type
#if NET40
        if (targetType.IsValueType && sourceType == typeof(object))
#else
        if (targetType.GetTypeInfo().IsValueType && sourceType == typeof(object))
#endif
        {
          il.Emit(OpCodes.Unbox_Any, targetType);
          return true;
        }

        // Conversion to nullable: new Nullable<T>(T val);
        if (targetType.IsNullable())
        {
          var wrappedType = targetType.GetWrappedTypeFromNullable();
          var ctor = targetType.GetConstructorByArgs(wrappedType);
          il.Emit(OpCodes.Newobj, ctor);
          return true;
        }

        if (targetType == typeof(int)) { il.Emit(OpCodes.Conv_I4); }
        else if (targetType == typeof(float)) { il.Emit(OpCodes.Conv_R4); }
        else if (targetType == typeof(uint)) { il.Emit(OpCodes.Conv_U4); }
        else if (targetType == typeof(sbyte)) { il.Emit(OpCodes.Conv_I1); }
        else if (targetType == typeof(byte)) { il.Emit(OpCodes.Conv_U1); }
        else if (targetType == typeof(short)) { il.Emit(OpCodes.Conv_I2); }
        else if (targetType == typeof(ushort)) { il.Emit(OpCodes.Conv_U2); }
        else if (targetType == typeof(long)) { il.Emit(OpCodes.Conv_I8); }
        else if (targetType == typeof(ulong)) { il.Emit(OpCodes.Conv_U8); }
        else if (targetType == typeof(double)) { il.Emit(OpCodes.Conv_R8); }
        else { il.Emit(OpCodes.Castclass, targetType); }

        return true;
      }

      #endregion

      #region ** EmitConstant **

      private static bool EmitConstant(object exprObj, Type exprType, ILGenerator il, ClosureInfo closure)
      {
        var constExprInfo = exprObj as ConstantExpressionInfo;
        var constantValue = constExprInfo != null ? constExprInfo.Value : ((ConstantExpression)exprObj).Value;
        if (constantValue == null)
        {
          il.Emit(OpCodes.Ldnull);
          return true;
        }

        var constantActualType = constantValue.GetType();
#if NET40
        if (constantActualType.IsEnum)
#else
        if (constantActualType.GetTypeInfo().IsEnum)
#endif
        {
          constantActualType = Enum.GetUnderlyingType(constantActualType);
        }

        if (constantActualType == typeof(int))
        {
          EmitLoadConstantInt(il, (int)constantValue);
        }
        else if (constantActualType == typeof(char))
        {
          EmitLoadConstantInt(il, (char)constantValue);
        }
        else if (constantActualType == typeof(short))
        {
          EmitLoadConstantInt(il, (short)constantValue);
        }
        else if (constantActualType == typeof(byte))
        {
          EmitLoadConstantInt(il, (byte)constantValue);
        }
        else if (constantActualType == typeof(ushort))
        {
          EmitLoadConstantInt(il, (ushort)constantValue);
        }
        else if (constantActualType == typeof(sbyte))
        {
          EmitLoadConstantInt(il, (sbyte)constantValue);
        }
        else if (constantActualType == typeof(uint))
        {
          unchecked
          {
            EmitLoadConstantInt(il, (int)(uint)constantValue);
          }
        }
        else if (constantActualType == typeof(long))
        {
          il.Emit(OpCodes.Ldc_I8, (long)constantValue);
        }
        else if (constantActualType == typeof(ulong))
        {
          unchecked
          {
            il.Emit(OpCodes.Ldc_I8, (long)(ulong)constantValue);
          }
        }
        else if (constantActualType == typeof(float))
        {
          il.Emit(OpCodes.Ldc_R8, (float)constantValue);
        }
        else if (constantActualType == typeof(double))
        {
          il.Emit(OpCodes.Ldc_R8, (double)constantValue);
        }
        else if (constantActualType == typeof(bool))
        {
          il.Emit((bool)constantValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
        else if (constantValue is string)
        {
          il.Emit(OpCodes.Ldstr, (string)constantValue);
        }
        else if (constantValue is Type)
        {
          il.Emit(OpCodes.Ldtoken, (Type)constantValue);
          il.Emit(OpCodes.Call, _getTypeFromHandleMethod);
        }
        else if (closure != null && closure.HasBoundClosure)
        {
          var constantIndex = closure.Constants.GetFirstIndex(it => it.ConstantExpr == exprObj);
          if (constantIndex == -1) { return false; }

          LoadClosureFieldOrItem(closure, il, constantIndex, exprType);
        }
        else return false;

        // todo: consider how to remove boxing where it is not required
        // boxing the value type, otherwise we can get a strange result when 0 is treated as Null.
#if NET40
        if (exprType == typeof(object) && constantActualType.IsValueType)
#else
        if (exprType == typeof(object) && constantActualType.GetTypeInfo().IsValueType)
#endif
        {
          il.Emit(OpCodes.Box, constantValue.GetType()); // using normal type for Enum instead of underlying type
        }

        return true;
      }

      #endregion

      #region ** LoadClosureFieldOrItem **

      // if itemType is null, then itemExprObj should be not null
      private static void LoadClosureFieldOrItem(ClosureInfo closure, ILGenerator il,
        int itemIndex, Type itemType, object itemExprObj = null)
      {
        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument

        // todo: consider using Ldarga for ValueType
        if (closure.Fields != null)
        {
          il.Emit(OpCodes.Ldfld, closure.Fields[itemIndex]);
        }
        else
        {
          // for ArrayClosure load an array field
          il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField);

          // load array item index
          EmitLoadConstantInt(il, itemIndex);

          // load item from index
          il.Emit(OpCodes.Ldelem_Ref);

          // cast or un-box the object item depending if it is a class or value type
          itemType = itemType ?? itemExprObj.GetResultType();
#if NET40
          if (itemType.IsValueType)
#else
          if (itemType.GetTypeInfo().IsValueType)
#endif
          {
            il.Emit(OpCodes.Unbox_Any, itemType);
          }
          else
          {
            il.Emit(OpCodes.Castclass, itemType);
          }
        }
      }

      #endregion

      #region ** EmitNew **

      private static bool EmitNew(object exprObj, Type exprType, object[] paramExprs,
        ILGenerator il, ClosureInfo closure, LocalBuilder resultValueVar = null)
      {
        ConstructorInfo ctor;
        if (exprObj is NewExpressionInfo exprInfo)
        {
          if (!EmitMany(exprInfo.Arguments, paramExprs, il, closure)) { return false; }
          ctor = exprInfo.Constructor;
        }
        else
        {
          var expr = (NewExpression)exprObj;
          if (!EmitMany(expr.Arguments, paramExprs, il, closure)) { return false; }
          ctor = expr.Constructor;
        }

        if (ctor != null)
        {
          il.Emit(OpCodes.Newobj, ctor);
        }
        else
        {
#if NET40
          if (!exprType.IsValueType)
#else
          if (!exprType.GetTypeInfo().IsValueType)
#endif
          {
            return false; // null constructor and not a value type, better fallback
          }

          var valueVar = resultValueVar ?? il.DeclareLocal(exprType);
          il.Emit(OpCodes.Ldloca, valueVar);
          il.Emit(OpCodes.Initobj, exprType);
          if (resultValueVar == null)
          {
            il.Emit(OpCodes.Ldloc, valueVar);
          }
        }

        return true;
      }

      #endregion

      #region ** EmitNewArray **

      private static bool EmitNewArray(object exprObj, Type arrayType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        if (exprObj is NewArrayExpressionInfo exprInfo)
        {
          return EmitNewArrayInfo(exprInfo, arrayType, paramExprs, il, closure);
        }

        var expr = (NewArrayExpression)exprObj;
        var elems = expr.Expressions;
        var elemType = arrayType.GetElementType();
        if (elemType == null) { return false; }

#if NET40
        var isElemOfValueType = elemType.IsValueType;
#else
        var isElemOfValueType = elemType.GetTypeInfo().IsValueType;
#endif

        var arrVar = il.DeclareLocal(arrayType);

        var rank = arrayType.GetArrayRank();
        if (rank == 1) // one dimensional
        {
          EmitLoadConstantInt(il, elems.Count);
        }
        else // multi dimensional
        {
          var boundsLength = elems.Count;
          for (var i = 0; i < boundsLength; i++)
          {
            var bound = elems[i];
            if (!TryEmit(bound, bound.NodeType, bound.Type, paramExprs, il, closure)) { return false; }
          }

#if NET40
          var constructor = arrayType.GetTypeDeclaredConstructors().GetFirst();
#else
          var constructor = arrayType.GetTypeInfo().DeclaredConstructors.GetFirst();
#endif
          if (constructor == null) { return false; }
          il.Emit(OpCodes.Newobj, constructor);

          return true;
        }

        il.Emit(OpCodes.Newarr, elemType);
        il.Emit(OpCodes.Stloc, arrVar);

        for (int i = 0, n = elems.Count; i < n; i++)
        {
          il.Emit(OpCodes.Ldloc, arrVar);
          EmitLoadConstantInt(il, i);

          // loading element address for later copying of value into it.
          if (isElemOfValueType) { il.Emit(OpCodes.Ldelema, elemType); }

          var elemExpr = elems[i];
          if (!TryEmit(elemExpr, elemExpr.NodeType, elemExpr.Type, paramExprs, il, closure)) { return false; }

          if (isElemOfValueType)
          {
            il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
          }
          else
          {
            il.Emit(OpCodes.Stelem_Ref);
          }
        }

        il.Emit(OpCodes.Ldloc, arrVar);
        return true;
      }

      #endregion

      #region ** EmitNewArrayInfo **

      private static bool EmitNewArrayInfo(NewArrayExpressionInfo expr, Type arrayType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var elemExprObjects = expr.Arguments;
        var elemType = arrayType.GetElementType();
        if (elemType == null) { return false; }

        var arrVar = il.DeclareLocal(arrayType);

        EmitLoadConstantInt(il, elemExprObjects.Length);
        il.Emit(OpCodes.Newarr, elemType);
        il.Emit(OpCodes.Stloc, arrVar);

#if NET40
        var isElemOfValueType = elemType.IsValueType;
#else
        var isElemOfValueType = elemType.GetTypeInfo().IsValueType;
#endif

        for (var i = 0; i < elemExprObjects.Length; i++)
        {
          il.Emit(OpCodes.Ldloc, arrVar);
          EmitLoadConstantInt(il, i);

          // loading element address for later copying of value into it.
          if (isElemOfValueType) { il.Emit(OpCodes.Ldelema, elemType); }

          var elemExprObject = elemExprObjects[i];
          var elemExprMeta = GetExpressionMeta(elemExprObject);
          if (!TryEmit(elemExprObject, elemExprMeta.Key, elemExprMeta.Value, paramExprs, il, closure)) { return false; }

          if (isElemOfValueType)
          {
            il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
          }
          else
          {
            il.Emit(OpCodes.Stelem_Ref);
          }
        }

        il.Emit(OpCodes.Ldloc, arrVar);
        return true;
      }

      #endregion

      #region ** EmitArrayIndex **

      private static bool EmitArrayIndex(object exprObj, Type exprType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        if (!EmitBinary(exprObj, paramExprs, il, closure)) { return false; }
#if NET40
        if (exprType.IsValueType)
#else
        if (exprType.GetTypeInfo().IsValueType)
#endif
        {
          il.Emit(OpCodes.Ldelem, exprType);
        }
        else
        {
          il.Emit(OpCodes.Ldelem_Ref);
        }
        return true;
      }

      #endregion

      #region ** EmitMemberInit **

      private static bool EmitMemberInit(object exprObj, Type exprType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        if (exprObj is MemberInitExpressionInfo exprInfo)
        {
          return EmitMemberInitInfo(exprInfo, exprType, paramExprs, il, closure);
        }

        LocalBuilder valueVar = null;
#if NET40
        if (exprType.IsValueType) { valueVar = il.DeclareLocal(exprType); }
#else
        if (exprType.GetTypeInfo().IsValueType) { valueVar = il.DeclareLocal(exprType); }
#endif

        var expr = (MemberInitExpression)exprObj;
        if (!EmitNew(expr.NewExpression, exprType, paramExprs, il, closure, valueVar)) { return false; }

        var bindings = expr.Bindings;
        for (var i = 0; i < bindings.Count; i++)
        {
          var binding = bindings[i];
          if (binding.BindingType != MemberBindingType.Assignment) { return false; }

          if (valueVar != null) // load local value address, to set its members
          {
            il.Emit(OpCodes.Ldloca, valueVar);
          }
          else
          {
            il.Emit(OpCodes.Dup); // duplicate member owner on stack
          }

          var bindingExpr = ((MemberAssignment)binding).Expression;
          if (!TryEmit(bindingExpr, bindingExpr.NodeType, bindingExpr.Type, paramExprs, il, closure) ||
              !EmitMemberAssign(il, binding.Member))
          {
            return false;
          }
        }

        if (valueVar != null) { il.Emit(OpCodes.Ldloc, valueVar); }

        return true;
      }

      #endregion

      #region ** EmitMemberInitInfo **

      private static bool EmitMemberInitInfo(MemberInitExpressionInfo exprInfo, Type exprType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        LocalBuilder valueVar = null;
#if NET40
        if (exprType.IsValueType) { valueVar = il.DeclareLocal(exprType); }
#else
        if (exprType.GetTypeInfo().IsValueType) { valueVar = il.DeclareLocal(exprType); }
#endif

        var objInfo = exprInfo.ExpressionInfo;
        if (objInfo == null)
        {
          return false; // static initialization is Not supported
        }

        var newExpr = exprInfo.NewExpressionInfo;
        if (newExpr != null)
        {
          if (!EmitNew(newExpr, exprType, paramExprs, il, closure, valueVar)) { return false; }
        }
        else
        {
          if (!TryEmit(objInfo, objInfo.NodeType, objInfo.Type, paramExprs, il, closure)) { return false; }
        }

        var bindings = exprInfo.Bindings;
        for (var i = 0; i < bindings.Length; i++)
        {
          var binding = bindings[i];

          if (valueVar != null) // load local value address, to set its members
          {
            il.Emit(OpCodes.Ldloca, valueVar);
          }
          else
          {
            il.Emit(OpCodes.Dup); // duplicate member owner on stack
          }

          var bindingExpr = binding.Expression;
          if (!TryEmit(bindingExpr, bindingExpr.NodeType, bindingExpr.Type, paramExprs, il, closure) ||
              !EmitMemberAssign(il, binding.Member))
          {
            return false;
          }
        }

        if (valueVar != null) { il.Emit(OpCodes.Ldloc, valueVar); }

        return true;
      }

      #endregion

      #region ** EmitMemberAssign **

      private static bool EmitMemberAssign(ILGenerator il, MemberInfo member)
      {
        if (member is PropertyInfo prop)
        {
          var propSetMethodName = "set_" + prop.Name;
          var setMethod = prop.DeclaringType
#if NET40
              .GetTypeDeclaredMethods()
#else
              .GetTypeInfo().DeclaredMethods
#endif
              .GetFirst(m => m.Name == propSetMethodName);
          if (setMethod == null) { return false; }
          EmitMethodCall(il, setMethod);
        }
        else
        {
          var field = member as FieldInfo;
          if (field == null) { return false; }
          il.Emit(OpCodes.Stfld, field);
        }
        return true;
      }

      #endregion

      #region ** EmitAssign **

      private static bool EmitAssign(object exprObj, Type exprType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        object left, right;
        ExpressionType leftNodeType, rightNodeType;

        if (exprObj is BinaryExpression expr)
        {
          left = expr.Left;
          right = expr.Right;
          leftNodeType = expr.Left.NodeType;
          rightNodeType = expr.Right.NodeType;
        }
        else
        {
          var info = (BinaryExpressionInfo)exprObj;
          left = info.Left;
          right = info.Right;
          leftNodeType = left.GetNodeType();
          rightNodeType = right.GetNodeType();
        }

        // if this assignment is part of a single body-less expression or the result of a block
        // we should put its result to the evaluation stack before the return, otherwise we are
        // somewhere inside the block, so we shouldn't return with the result
        var shouldPushResult = closure == null
            || closure.CurrentBlock.IsEmpty
            || closure.CurrentBlock.ResultExpr == exprObj;

        switch (leftNodeType)
        {
          case ExpressionType.Parameter:
            var paramIndex = paramExprs.GetFirstIndex(left);
            if (paramIndex != -1)
            {
              if (closure != null && closure.HasBoundClosure)
              {
                paramIndex += 1; // shift parameter indices by one, because the first one will be closure
              }

              if (paramIndex >= byte.MaxValue) { return false; }

              if (!TryEmit(right, rightNodeType, exprType, paramExprs, il, closure)) { return false; }

              if (shouldPushResult)
              {
                il.Emit(OpCodes.Dup); // dup value to assign and return
              }

              il.Emit(OpCodes.Starg_S, paramIndex);
              return true;
            }

            // if parameter isn't passed, then it is passed into some outer lambda or it is a local variable,
            // so it should be loaded from closure or from the locals. Then the closure is null will be an invalid state.
            if (closure == null) { return false; }

            // if it's a local variable, then store the right value in it
            var localVariable = closure.GetDefinedLocalVarOrDefault(left);
            if (localVariable != null)
            {
              if (!TryEmit(right, rightNodeType, exprType, paramExprs, il, closure)) { return false; }

              if (shouldPushResult) // if we have to push the result back, dup the right value
              {
                il.Emit(OpCodes.Dup);
              }

              il.Emit(OpCodes.Stloc, localVariable);
              return true;
            }

            // check that it's a captured parameter by closure
            var nonPassedParamIndex = closure.NonPassedParameters.GetFirstIndex(left);
            if (nonPassedParamIndex == -1)
            {
              return false;  // what??? no chance
            }

            var paramInClosureIndex = closure.Constants.Length + nonPassedParamIndex;

            il.Emit(OpCodes.Ldarg_0); // closure is always a first argument

            if (shouldPushResult)
            {
              if (!TryEmit(right, rightNodeType, exprType, paramExprs, il, closure)) { return false; }

              var valueVar = il.DeclareLocal(exprType); // store left value in variable
              if (closure.Fields != null)
              {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, valueVar);
                il.Emit(OpCodes.Stfld, closure.Fields[paramInClosureIndex]);
                il.Emit(OpCodes.Ldloc, valueVar);
              }
              else
              {
                il.Emit(OpCodes.Stloc, valueVar);
                il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField); // load array field
                EmitLoadConstantInt(il, paramInClosureIndex); // load array item index
                il.Emit(OpCodes.Ldloc, valueVar);
#if NET40
                if (exprType.IsValueType) { il.Emit(OpCodes.Box, exprType); }
#else
                if (exprType.GetTypeInfo().IsValueType) { il.Emit(OpCodes.Box, exprType); }
#endif
                il.Emit(OpCodes.Stelem_Ref); // put the variable into array
                il.Emit(OpCodes.Ldloc, valueVar);
              }
            }
            else
            {
              var isArrayClosure = closure.Fields == null;
              if (isArrayClosure)
              {
                il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField); // load array field
                EmitLoadConstantInt(il, paramInClosureIndex); // load array item index
              }

              if (!TryEmit(right, rightNodeType, exprType, paramExprs, il, closure)) { return false; }

              if (isArrayClosure)
              {
#if NET40
                if (exprType.IsValueType) { il.Emit(OpCodes.Box, exprType); }
#else
                if (exprType.GetTypeInfo().IsValueType) { il.Emit(OpCodes.Box, exprType); }
#endif
                il.Emit(OpCodes.Stelem_Ref); // put the variable into array
              }
              else
              {
                il.Emit(OpCodes.Stfld, closure.Fields[paramInClosureIndex]);
              }
            }
            return true;

          case ExpressionType.MemberAccess:

            object objExpr;
            MemberInfo member;

            var memberExpr = left as MemberExpression;
            if (memberExpr != null)
            {
              objExpr = memberExpr.Expression;
              member = memberExpr.Member;
            }
            else
            {
              var memberExprInfo = (MemberExpressionInfo)left;
              objExpr = memberExprInfo.Expression;
              member = memberExprInfo.Member;
            }

            if (objExpr != null && !TryEmit(objExpr, objExpr.GetNodeType(), objExpr.GetResultType(), paramExprs, il, closure)) { return false; }

            if (!TryEmit(right, rightNodeType, exprType, paramExprs, il, closure)) { return false; }

            if (!shouldPushResult) { return EmitMemberAssign(il, member); }

            il.Emit(OpCodes.Dup);

            var rightVar = il.DeclareLocal(exprType); // store right value in variable
            il.Emit(OpCodes.Stloc, rightVar);

            if (!EmitMemberAssign(il, member)) { return false; }

            il.Emit(OpCodes.Ldloc, rightVar);
            return true;

          case ExpressionType.Index:
            var indexExpr = (IndexExpression)left;

            var obj = indexExpr.Object;
            if (obj != null && !TryEmit(obj, obj.NodeType, obj.Type, paramExprs, il, closure)) { return false; }

            var argLength = indexExpr.Arguments.Count;
            for (var i = 0; i < argLength; i++)
            {
              var arg = indexExpr.Arguments[i];
              if (!TryEmit(arg, arg.NodeType, arg.Type, paramExprs, il, closure)) { return false; }
            }

            if (!TryEmit(right, rightNodeType, exprType, paramExprs, il, closure)) { return false; }

            if (!shouldPushResult) { return EmitIndexAssign(indexExpr, obj?.Type, exprType, il); }

            var variable = il.DeclareLocal(exprType); // store value in variable to return
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Stloc, variable);

            if (!EmitIndexAssign(indexExpr, obj?.Type, exprType, il)) { return false; }

            il.Emit(OpCodes.Ldloc, variable);

            return true;

          default: // not yet support assignment targets
            return false;
        }
      }

      #endregion

      #region ** EmitIndexAssign **

      private static bool EmitIndexAssign(IndexExpression indexExpr, Type instType, Type elementType, ILGenerator il)
      {
        if (indexExpr.Indexer != null) { return EmitMemberAssign(il, indexExpr.Indexer); }

        if (indexExpr.Arguments.Count == 1) // one dimensional array
        {
#if NET40
          if (elementType.IsValueType)
#else
          if (elementType.GetTypeInfo().IsValueType)
#endif
          {
            il.Emit(OpCodes.Stelem, elementType);
          }
          else
          {
            il.Emit(OpCodes.Stelem_Ref);
          }
        }
        else // multi dimensional array
        {
          if (instType == null) { return false; }

#if NET40
          var setMethod = instType.GetDeclaredMethod("Set");
#else
          var setMethod = instType.GetTypeInfo().GetDeclaredMethod("Set");
#endif
          EmitMethodCall(il, setMethod);
        }

        return true;
      }

      #endregion

      #region ** EmitIndexAccess **

      private static bool EmitIndexAccess(IndexExpression indexExpr, Type instType, Type elementType, ILGenerator il)
      {
        if (indexExpr.Indexer != null) { return EmitMemberAccess(il, indexExpr.Indexer); }

        if (indexExpr.Arguments.Count == 1) // one dimensional array
        {
#if NET40
          if (elementType.IsValueType)
#else
          if (elementType.GetTypeInfo().IsValueType)
#endif
          {
            il.Emit(OpCodes.Ldelem, elementType);
          }
          else
          {
            il.Emit(OpCodes.Ldelem_Ref);
          }
        }
        else // multi dimensional array
        {
          if (instType == null) { return false; }

#if NET40
          var setMethod = instType.GetDeclaredMethod("Get");
#else
          var setMethod = instType.GetTypeInfo().GetDeclaredMethod("Get");
#endif
          EmitMethodCall(il, setMethod);
        }

        return true;
      }

      #endregion

      #region ** EmitMethodCall **

      private static bool EmitMethodCall(object exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var exprInfo = exprObj as MethodCallExpressionInfo;
        if (exprInfo != null)
        {
          var objInfo = exprInfo.Object;
          if (objInfo != null)
          {
            var objType = objInfo.Type;
            if (!TryEmit(objInfo, objInfo.NodeType, objType, paramExprs, il, closure)) { return false; }
#if NET40
            if (objType.IsValueType)
#else
            if (objType.GetTypeInfo().IsValueType)
#endif
            {
              il.Emit(OpCodes.Box, objType); // todo: not optimal, should be replaced by Ldloca
            }
          }

          if (exprInfo.Arguments.Length != 0 && !EmitMany(exprInfo.Arguments, paramExprs, il, closure)) { return false; }
        }
        else
        {
          var expr = (MethodCallExpression)exprObj;
          var objExpr = expr.Object;
          if (objExpr != null)
          {
            var objType = objExpr.Type;
            if (!TryEmit(objExpr, objExpr.NodeType, objType, paramExprs, il, closure)) { return false; }
#if NET40
            if (objType.IsValueType)
#else
            if (objType.GetTypeInfo().IsValueType)
#endif
            {
              il.Emit(OpCodes.Box, objType); // todo: not optimal, should be replaced by Ldloca
            }
          }

          if (expr.Arguments.Count != 0 && !EmitMany(expr.Arguments, paramExprs, il, closure)) { return false; }
        }

        var method = exprInfo != null ? exprInfo.Method : ((MethodCallExpression)exprObj).Method;
        EmitMethodCall(il, method);
        return true;
      }

      #endregion

      #region ** EmitMemberAccess **

      private static bool EmitMemberAccess(object exprObj, Type exprType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        MemberInfo member;
        Type instanceType = null;
        if (exprObj is MemberExpressionInfo exprInfo)
        {
          var instance = exprInfo.Expression;
          if (instance != null)
          {
            instanceType = instance.GetResultType();
            if (!TryEmit(instance, instance.GetNodeType(), instanceType, paramExprs, il, closure)) { return false; }
          }
          member = exprInfo.Member;
        }
        else
        {
          var expr = (MemberExpression)exprObj;
          var instExpr = expr.Expression;
          if (instExpr != null)
          {
            instanceType = instExpr.Type;
            if (!TryEmit(instExpr, instExpr.NodeType, instanceType, paramExprs, il, closure)) { return false; }
          }
          member = expr.Member;
        }

        if (instanceType != null) // it is a non-static member access
        {
          // value type special treatment to load address of value instance
          // in order to access value member or call a method (does a copy).
          // todo: May be optimized for method call to load address of initial variable without copy
#if NET40
          if (instanceType.IsValueType)
#else
          if (instanceType.GetTypeInfo().IsValueType)
#endif
          {
#if NET40
            if (exprType.IsValueType || member is PropertyInfo)
#else
            if (exprType.GetTypeInfo().IsValueType || member is PropertyInfo)
#endif
            {
              var valueVar = il.DeclareLocal(instanceType);
              il.Emit(OpCodes.Stloc, valueVar);
              il.Emit(OpCodes.Ldloca, valueVar);
            }
          }
        }

        return EmitMemberAccess(il, member);
      }

      private static bool EmitMemberAccess(ILGenerator il, MemberInfo member)
      {
        if (member is FieldInfo field)
        {
          il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
          return true;
        }

        if (member is PropertyInfo prop)
        {
          var getMethod = TryGetPropertyGetMethod(prop);
          if (getMethod == null) { return false; }
          EmitMethodCall(il, getMethod);
          return true;
        }

        return false;
      }

      #endregion

      #region ** TryGetPropertyGetMethod **

      private static MethodInfo TryGetPropertyGetMethod(PropertyInfo prop)
      {
        var propGetMethodName = "get_" + prop.Name;
        var getMethod = prop.DeclaringType
#if NET40
            .GetTypeDeclaredMethods()
#else
            .GetTypeInfo().DeclaredMethods
#endif
            .GetFirst(m => m.Name == propGetMethodName);
        return getMethod;
      }

      #endregion

      #region ** EmitNestedLambda **

      private static bool EmitNestedLambda(object lambdaExpr, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        // First, find in closed compiled lambdas the one corresponding to the current lambda expression.
        // Situation with not found lambda is not possible/exceptional,
        // it means that we somehow skipped the lambda expression while collecting closure info.
        var outerNestedLambdas = closure.NestedLambdas;
        var outerNestedLambdaIndex = outerNestedLambdas.GetFirstIndex(it => it.LambdaExpr == lambdaExpr);
        if (outerNestedLambdaIndex == -1) { return false; }

        var nestedLambdaInfo = outerNestedLambdas[outerNestedLambdaIndex];
        var nestedLambda = nestedLambdaInfo.Lambda;

        var outerConstants = closure.Constants;
        var outerNonPassedParams = closure.NonPassedParameters;

        // Load compiled lambda on stack counting the offset
        outerNestedLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

        LoadClosureFieldOrItem(closure, il, outerNestedLambdaIndex, nestedLambda.GetType());

        // If lambda does not use any outer parameters to be set in closure, then we're done
        var nestedClosureInfo = nestedLambdaInfo.ClosureInfo;
        if (nestedClosureInfo == null) { return true; }

        // If closure is array-based, the create a new array to represent closure for the nested lambda
        var isNestedArrayClosure = nestedClosureInfo.Fields == null;
        if (isNestedArrayClosure)
        {
          EmitLoadConstantInt(il, nestedClosureInfo.ClosedItemCount); // size of array
          il.Emit(OpCodes.Newarr, typeof(object));
        }

        // Load constants on stack
        var nestedConstants = nestedClosureInfo.Constants;
        if (nestedConstants.Length != 0)
        {
          for (var nestedConstIndex = 0; nestedConstIndex < nestedConstants.Length; nestedConstIndex++)
          {
            var nestedConstant = nestedConstants[nestedConstIndex];

            // Find constant index in the outer closure
            var outerConstIndex = outerConstants.GetFirstIndex(it => it.ConstantExpr == nestedConstant.ConstantExpr);
            if (outerConstIndex == -1)
            {
              return false; // some error is here
            }

            if (isNestedArrayClosure)
            {
              // Duplicate nested array on stack to store the item, and load index to where to store
              il.Emit(OpCodes.Dup);
              EmitLoadConstantInt(il, nestedConstIndex);
            }

            LoadClosureFieldOrItem(closure, il, outerConstIndex, nestedConstant.Type);

            if (isNestedArrayClosure)
            {
#if NET40
              if (nestedConstant.Type.IsValueType)
#else
              if (nestedConstant.Type.GetTypeInfo().IsValueType)
#endif
              {
                il.Emit(OpCodes.Box, nestedConstant.Type);
              }
              il.Emit(OpCodes.Stelem_Ref); // store the item in array
            }
          }
        }

        // Load used and closed parameter values on stack
        var nestedNonPassedParams = nestedClosureInfo.NonPassedParameters;
        for (var nestedParamIndex = 0; nestedParamIndex < nestedNonPassedParams.Length; nestedParamIndex++)
        {
          var nestedUsedParam = nestedNonPassedParams[nestedParamIndex];

          Type nestedUsedParamType = null;
          if (isNestedArrayClosure)
          {
            // get a param type for the later
            nestedUsedParamType = nestedUsedParam.GetResultType();

            // Duplicate nested array on stack to store the item, and load index to where to store
            il.Emit(OpCodes.Dup);
            EmitLoadConstantInt(il, nestedConstants.Length + nestedParamIndex);
          }

          var paramIndex = paramExprs.GetFirstIndex(nestedUsedParam);
          if (paramIndex != -1) // load param from input params
          {
            // +1 is set cause of added first closure argument
            LoadParamArg(il, 1 + paramIndex);
          }
          else // load parameter from outer closure or from the locals
          {
            if (outerNonPassedParams.Length == 0)
            {
              return false; // impossible, better to throw?
            }

            var variable = closure.GetDefinedLocalVarOrDefault(nestedUsedParam);
            if (variable != null) // it's a local variable
            {
              il.Emit(OpCodes.Ldloc, variable);
            }
            else // it's a parameter from outer closure
            {
              var outerParamIndex = outerNonPassedParams.GetFirstIndex(nestedUsedParam);
              if (outerParamIndex == -1)
              {
                return false; // impossible, better to throw?
              }

              LoadClosureFieldOrItem(closure, il, outerConstants.Length + outerParamIndex, nestedUsedParamType, nestedUsedParam);
            }
          }

          if (isNestedArrayClosure)
          {
#if NET40
            if (nestedUsedParamType.IsValueType)
#else
            if (nestedUsedParamType.GetTypeInfo().IsValueType)
#endif
            {
              il.Emit(OpCodes.Box, nestedUsedParamType);
            }


            il.Emit(OpCodes.Stelem_Ref); // store the item in array
          }
        }

        // Load nested lambdas on stack
        var nestedNestedLambdas = nestedClosureInfo.NestedLambdas;
        if (nestedNestedLambdas.Length != 0)
        {
          for (var nestedLambdaIndex = 0; nestedLambdaIndex < nestedNestedLambdas.Length; nestedLambdaIndex++)
          {
            var nestedNestedLambda = nestedNestedLambdas[nestedLambdaIndex];

            // Find constant index in the outer closure
            var outerLambdaIndex = outerNestedLambdas.GetFirstIndex(it => it.LambdaExpr == nestedNestedLambda.LambdaExpr);
            if (outerLambdaIndex == -1)
            {
              return false; // some error is here
            }

            // Duplicate nested array on stack to store the item, and load index to where to store
            if (isNestedArrayClosure)
            {
              il.Emit(OpCodes.Dup);
              EmitLoadConstantInt(il, nestedConstants.Length + nestedNonPassedParams.Length + nestedLambdaIndex);
            }

            outerLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

            LoadClosureFieldOrItem(closure, il, outerLambdaIndex, nestedNestedLambda.Lambda.GetType());

            if (isNestedArrayClosure)
            {
              il.Emit(OpCodes.Stelem_Ref); // store the item in array
            }
          }
        }

        // Create nested closure object composed of all constants, params, lambdas loaded on stack
        if (isNestedArrayClosure)
        {
          il.Emit(OpCodes.Newobj, ArrayClosure.Constructor);
        }
        else
        {
#if NET40
          il.Emit(OpCodes.Newobj, nestedClosureInfo.ClosureType.GetTypeDeclaredConstructors().GetFirst());
#else
          il.Emit(OpCodes.Newobj, nestedClosureInfo.ClosureType.GetTypeInfo().DeclaredConstructors.GetFirst());
#endif
        }

        EmitMethodCall(il, GetCurryClosureMethod(nestedLambda, nestedLambdaInfo.IsAction));
        return true;
      }

      #endregion

      #region ** GetCurryClosureMethod **

      private static MethodInfo GetCurryClosureMethod(object lambda, bool isAction)
      {
#if NET40
        var lambdaTypeArgs = lambda.GetType().GenericTypeArguments();
#else
        var lambdaTypeArgs = lambda.GetType().GetTypeInfo().GenericTypeArguments;
#endif
        return isAction
            ? CurryClosureActions.Methods[lambdaTypeArgs.Length - 1].MakeGenericMethod(lambdaTypeArgs)
            : CurryClosureFuncs.Methods[lambdaTypeArgs.Length - 2].MakeGenericMethod(lambdaTypeArgs);
      }

      #endregion

      #region ** EmitInvokeLambda **

      private static bool EmitInvokeLambda(object exprObj, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var expr = exprObj as InvocationExpression;
        Type lambdaType;
        if (expr != null)
        {
          var lambdaExpr = expr.Expression;
          lambdaType = lambdaExpr.Type;
          if (!TryEmit(lambdaExpr, lambdaExpr.NodeType, lambdaType, paramExprs, il, closure) ||
              !EmitMany(expr.Arguments, paramExprs, il, closure))
            return false;
        }
        else
        {
          var exprInfo = (InvocationExpressionInfo)exprObj;
          var lambdaExprInfo = exprInfo.ExprToInvoke;
          lambdaType = lambdaExprInfo.Type;
          if (!TryEmit(lambdaExprInfo, lambdaExprInfo.NodeType, lambdaType, paramExprs, il, closure) ||
              !EmitMany(exprInfo.Arguments, paramExprs, il, closure))
          {
            return false;
          }
        }

        var invokeMethod = lambdaType
#if NET40
            .GetTypeDeclaredMethods()
#else
            .GetTypeInfo().DeclaredMethods
#endif
            .GetFirst(m => m.Name == "Invoke");
        EmitMethodCall(il, invokeMethod);
        return true;
      }

      #endregion

      #region ** EmitComparison **

      private static bool EmitComparison(BinaryExpression expr, ExpressionType exprNodeType,
        object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        if (!EmitBinary(expr, paramExprs, il, closure)) { return false; }

#if NET40
        var leftOpType = expr.Left.Type;
        var leftOpTypeInfo = leftOpType;
#else
        var leftOpType = expr.Left.Type;
        var leftOpTypeInfo = leftOpType.GetTypeInfo();
#endif
        if (!leftOpTypeInfo.IsPrimitive && !leftOpTypeInfo.IsEnum)
        {
          var methodName
              = exprNodeType == ExpressionType.Equal ? "op_Equality"
              : exprNodeType == ExpressionType.NotEqual ? "op_Inequality"
              : exprNodeType == ExpressionType.GreaterThan ? "op_GreaterThan"
              : exprNodeType == ExpressionType.GreaterThanOrEqual ? "op_GreaterThanOrEqual"
              : exprNodeType == ExpressionType.LessThan ? "op_LessThan"
              : exprNodeType == ExpressionType.LessThanOrEqual ? "op_LessThanOrEqual" :
              null;

          if (methodName == null) { return false; }

          // todo: for now handling only parameters of the same type
#if NET40
          var method = leftOpTypeInfo.GetTypeDeclaredMethods()
#else
          var method = leftOpTypeInfo.DeclaredMethods
#endif
              .GetFirst(m =>
                  m.IsStatic && m.Name == methodName &&
                  m.GetParameters().All(p => p.ParameterType == leftOpType));

          if (method != null)
          {
            EmitMethodCall(il, method);
          }
          else
          {
            if (exprNodeType != ExpressionType.Equal && exprNodeType != ExpressionType.NotEqual)
            {
              return false;
            }

            EmitMethodCall(il, _objectEqualsMethod);
            if (exprNodeType == ExpressionType.NotEqual) // add not to equal
            {
              il.Emit(OpCodes.Ldc_I4_0);
              il.Emit(OpCodes.Ceq);
            }
          }

          return true;
        }

        // emit for primitives
        switch (exprNodeType)
        {
          case ExpressionType.Equal:
            il.Emit(OpCodes.Ceq);
            return true;

          case ExpressionType.NotEqual:
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            return true;

          case ExpressionType.LessThan:
            il.Emit(OpCodes.Clt);
            return true;

          case ExpressionType.GreaterThan:
            il.Emit(OpCodes.Cgt);
            return true;

          case ExpressionType.LessThanOrEqual:
            il.Emit(OpCodes.Cgt);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            return true;

          case ExpressionType.GreaterThanOrEqual:
            il.Emit(OpCodes.Clt);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            return true;
        }
        return false;
      }

      #endregion

      #region ** EmitArithmeticOperation **

      private static bool EmitArithmeticOperation(object exprObj, Type exprType,
        ExpressionType exprNodeType, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        if (!EmitBinary(exprObj, paramExprs, il, closure)) { return false; }

#if NET40
        if (!exprType.IsPrimitive)
#else
        var exprTypeInfo = exprType.GetTypeInfo();
        if (!exprTypeInfo.IsPrimitive)
#endif
        {
          var methodName
              = exprNodeType == ExpressionType.Add ? "op_Addition"
              : exprNodeType == ExpressionType.AddChecked ? "op_Addition"
              : exprNodeType == ExpressionType.Subtract ? "op_Subtraction"
              : exprNodeType == ExpressionType.SubtractChecked ? "op_Subtraction"
              : exprNodeType == ExpressionType.Multiply ? "op_Multiply"
              : exprNodeType == ExpressionType.Divide ? "op_Division"
              : null;

          if (methodName == null) { return false; }

#if NET40
          EmitMethodCall(il, exprType.GetDeclaredMethod(methodName));
#else
          EmitMethodCall(il, exprTypeInfo.GetDeclaredMethod(methodName));
#endif
          return true;
        }

        switch (exprNodeType)
        {
          case ExpressionType.Add:
            il.Emit(OpCodes.Add);
            return true;

          case ExpressionType.AddChecked:
            il.Emit(IsUnsigned(exprType) ? OpCodes.Add_Ovf_Un : OpCodes.Add_Ovf);
            return true;

          case ExpressionType.Subtract:
            il.Emit(OpCodes.Sub);
            return true;

          case ExpressionType.SubtractChecked:
            il.Emit(IsUnsigned(exprType) ? OpCodes.Sub_Ovf_Un : OpCodes.Sub_Ovf);
            return true;

          case ExpressionType.Multiply:
            il.Emit(OpCodes.Mul);
            return true;

          case ExpressionType.MultiplyChecked:
            il.Emit(IsUnsigned(exprType) ? OpCodes.Mul_Ovf_Un : OpCodes.Mul_Ovf);
            return true;

          case ExpressionType.Divide:
            il.Emit(OpCodes.Div);
            return true;
        }

        return false;
      }

      #endregion

      #region ** IsUnsigned **

      private static bool IsUnsigned(Type type) => type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

      #endregion

      #region ** EmitLogicalOperator **

      private static bool EmitLogicalOperator(BinaryExpression expr, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var leftExpr = expr.Left;
        if (!TryEmit(leftExpr, leftExpr.NodeType, leftExpr.Type, paramExprs, il, closure)) { return false; }

        var labelSkipRight = il.DefineLabel();
        var isAnd = expr.NodeType == ExpressionType.AndAlso;
        il.Emit(isAnd ? OpCodes.Brfalse : OpCodes.Brtrue, labelSkipRight);

        var rightExpr = expr.Right;
        if (!TryEmit(rightExpr, rightExpr.NodeType, rightExpr.Type, paramExprs, il, closure)) { return false; }

        var labelDone = il.DefineLabel();
        il.Emit(OpCodes.Br, labelDone);

        il.MarkLabel(labelSkipRight); // label the second branch
        il.Emit(isAnd ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
        il.MarkLabel(labelDone);
        return true;
      }

      #endregion

      #region ** EmitConditional **

      private static bool EmitConditional(ConditionalExpression expr, object[] paramExprs, ILGenerator il, ClosureInfo closure)
      {
        var testExpr = expr.Test;
        if (!TryEmit(testExpr, testExpr.NodeType, testExpr.Type, paramExprs, il, closure)) { return false; }

        var labelIfFalse = il.DefineLabel();
        il.Emit(OpCodes.Brfalse, labelIfFalse);

        var ifTrueExpr = expr.IfTrue;
        if (!TryEmit(ifTrueExpr, ifTrueExpr.NodeType, ifTrueExpr.Type, paramExprs, il, closure)) { return false; }

        var labelDone = il.DefineLabel();
        il.Emit(OpCodes.Br, labelDone);

        il.MarkLabel(labelIfFalse);
        var ifFalseExpr = expr.IfFalse;
        if (!TryEmit(ifFalseExpr, ifFalseExpr.NodeType, ifFalseExpr.Type, paramExprs, il, closure)) { return false; }

        il.MarkLabel(labelDone);
        return true;
      }

      #endregion

      #region ** EmitMethodCall **

      private static void EmitMethodCall(ILGenerator il, MethodInfo method) => il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);

      #endregion

      #region ** EmitLoadConstantInt **

      private static void EmitLoadConstantInt(ILGenerator il, int i)
      {
        switch (i)
        {
          case -1:
            il.Emit(OpCodes.Ldc_I4_M1);
            break;
          case 0:
            il.Emit(OpCodes.Ldc_I4_0);
            break;
          case 1:
            il.Emit(OpCodes.Ldc_I4_1);
            break;
          case 2:
            il.Emit(OpCodes.Ldc_I4_2);
            break;
          case 3:
            il.Emit(OpCodes.Ldc_I4_3);
            break;
          case 4:
            il.Emit(OpCodes.Ldc_I4_4);
            break;
          case 5:
            il.Emit(OpCodes.Ldc_I4_5);
            break;
          case 6:
            il.Emit(OpCodes.Ldc_I4_6);
            break;
          case 7:
            il.Emit(OpCodes.Ldc_I4_7);
            break;
          case 8:
            il.Emit(OpCodes.Ldc_I4_8);
            break;
          default:
            il.Emit(OpCodes.Ldc_I4, i);
            break;
        }
      }

      #endregion
    }
  }
}
