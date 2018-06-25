using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Grace.Dynamic.Impl
{
  /// <summary>interface for collecting information about constants</summary>
  public interface IConstantExpressionCollector
  {
    /// <summary>Get a list of constants from an expression</summary>
    /// <param name="expression">expression</param>
    /// <param name="constants">list of constants</param>
    /// <returns></returns>
    bool GetConstantExpressions(Expression expression, List<object> constants);
  }

  /// <summary>class for collecting constants from a Linq Expression</summary>
  public class ConstantExpressionCollector : IConstantExpressionCollector
  {
    /// <summary>Get a list of constants from an expression</summary>
    /// <param name="expression">expression</param>
    /// <param name="constants">list of constants</param>
    /// <returns></returns>
    public bool GetConstantExpressions(Expression expression, List<object> constants)
    {
      if (expression == null) { return true; }

      switch (expression.NodeType)
      {
        case ExpressionType.Constant:
          return ProcessConstantExpression((ConstantExpression)expression, constants);

        case ExpressionType.New:
          return ProcessListOfExpression(((NewExpression)expression).Arguments, constants);

        case ExpressionType.MemberInit:
          return ProcessMemberInit(expression, constants);

        case ExpressionType.MemberAccess:
          return GetConstantExpressions(((MemberExpression)expression).Expression, constants);

        case ExpressionType.Call:
          var callExpression = (MethodCallExpression)expression;
          return GetConstantExpressions(callExpression.Object, constants) &&
                 ProcessListOfExpression(callExpression.Arguments, constants);

        case ExpressionType.NewArrayInit:
          return ProcessListOfExpression(((NewArrayExpression)expression).Expressions, constants);

        case ExpressionType.Parameter:
          return true;
      }

      return ProcessDefaultExpressionType(expression, constants);
    }

    private bool ProcessDefaultExpressionType(Expression expression, List<object> constants)
    {
      switch (expression)
      {
        case UnaryExpression unaryExpression:
          return GetConstantExpressions(unaryExpression.Operand, constants);

        case BinaryExpression binaryExpression:
          return GetConstantExpressions(binaryExpression.Left, constants) &&
                 GetConstantExpressions(binaryExpression.Right, constants);

        default:
          return false;
      }
    }

    private bool ProcessMemberInit(Expression expression, List<object> constants)
    {
      var memberInit = (MemberInitExpression)expression;
      if (!GetConstantExpressions(memberInit.NewExpression, constants)) { return false; }

      foreach (var binding in memberInit.Bindings)
      {
        if (binding.BindingType == MemberBindingType.Assignment &&
            !GetConstantExpressions(((MemberAssignment)binding).Expression, constants))
        {
          return false;
        }
      }

      return true;
    }

    private static readonly HashSet<Type> s_expressionValueTypes = new HashSet<Type>(new[]
    {
      typeof(int), typeof(double), typeof(bool), typeof(string)
    });
    private bool ProcessConstantExpression(ConstantExpression expression, List<object> constants)
    {
      var exprValue = expression.Value;
      if (exprValue != null)
      {
        var valueType = exprValue.GetType();

        if (valueType == typeof(Delegate)) { return false; }

        if (!s_expressionValueTypes.Contains(valueType) && !constants.Contains(exprValue))
        {
          constants.Add(exprValue);
        }
      }

      return true;
    }

    private bool ProcessListOfExpression(IEnumerable<Expression> expressions, List<object> constants)
    {
      foreach (var expression in expressions)
      {
        if (!GetConstantExpressions(expression, constants)) { return false; }
      }

      return true;
    }
  }
}