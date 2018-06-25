﻿using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Grace.Dynamic.Impl
{
  /// <summary>Interface for generating IL for a MemeberInitExpression</summary>
  public interface IMemeberInitExpressionGenerator
  {
    /// <summary>Generate IL for member init expression</summary>
    /// <param name="request">request</param>
    /// <param name="expression">expression</param>
    /// <returns></returns>
    bool GenerateIL(DynamicMethodGenerationRequest request, MemberInitExpression expression);
  }

  /// <summary>Class for generating IL for MemberInit expression</summary>
  public class MemeberInitExpressionGenerator : IMemeberInitExpressionGenerator
  {
    /// <summary>Generate IL for member init expression</summary>
    /// <param name="request">request</param>
    /// <param name="expression">expression</param>
    /// <returns></returns>
    public bool GenerateIL(DynamicMethodGenerationRequest request, MemberInitExpression expression)
    {
      if (!request.TryGenerateIL(request, expression.NewExpression)) { return false; }

      foreach (var binding in expression.Bindings)
      {
        if (binding.BindingType != MemberBindingType.Assignment) { return false; }

        request.ILGenerator.Emit(OpCodes.Dup);

        if (!request.TryGenerateIL(request, ((MemberAssignment)binding).Expression)) { return false; }

        switch (binding.Member)
        {
          case PropertyInfo propertyInfo:
#if NET40
            var setMethod = propertyInfo.GetSetMethod(true);
#else
            var setMethod = propertyInfo.SetMethod;
#endif

            if (setMethod == null) { return false; }

            request.ILGenerator.EmitMethodCall(setMethod);
            break;

          case FieldInfo fieldInfo:
            request.ILGenerator.Emit(OpCodes.Stfld, fieldInfo);
            break;

          default:
            return false;
        }
      }

      return true;
    }
  }
}