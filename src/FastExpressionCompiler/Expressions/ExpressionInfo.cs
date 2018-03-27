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
using System.Linq.Expressions;
using System.Reflection;

namespace FastExpressionCompiler
{
  /// <summary>Facade for constructing expression info.</summary>
  public abstract class ExpressionInfo
  {
    /// <summary>Expression node type.</summary>
    public abstract ExpressionType NodeType { get; }

    /// <summary>All expressions should have a Type.</summary>
    public abstract Type Type { get; }

    /// <summary>Converts back to respective expression so you may Compile it by usual means.</summary>
    public abstract Expression ToExpression();

    /// <summary>Converts to Expression and outputs its as string</summary>
    public override string ToString() => ToExpression().ToString();

    /// <summary>Analog of Expression.Parameter</summary>
    /// <remarks>For now it is return just an `Expression.Parameter`</remarks>
    public static ParameterExpressionInfo Parameter(Type type, string name = null) =>
        new ParameterExpressionInfo(type, name);

    /// <summary>Analog of Expression.Constant</summary>
    public static ConstantExpressionInfo Constant(object value, Type type = null) =>
        value == null && type == null ? _nullExprInfo
            : new ConstantExpressionInfo(value, type ?? value.GetType());

    private static readonly ConstantExpressionInfo
        _nullExprInfo = new ConstantExpressionInfo(null, typeof(object));

    /// <summary>Analog of Expression.New</summary>
    public static NewExpressionInfo New(ConstructorInfo ctor) =>
        new NewExpressionInfo(ctor, Tools.Empty<object>());

    /// <summary>Analog of Expression.New</summary>
    public static NewExpressionInfo New(ConstructorInfo ctor, params object[] arguments) =>
        new NewExpressionInfo(ctor, arguments);

    /// <summary>Analog of Expression.New</summary>
    public static NewExpressionInfo New(ConstructorInfo ctor, params ExpressionInfo[] arguments) =>
        new NewExpressionInfo(ctor, arguments);

    /// <summary>Static method call</summary>
    public static MethodCallExpressionInfo Call(MethodInfo method, params object[] arguments) =>
        new MethodCallExpressionInfo(null, method, arguments);

    /// <summary>Static method call</summary>
    public static MethodCallExpressionInfo Call(MethodInfo method, params ExpressionInfo[] arguments) =>
        new MethodCallExpressionInfo(null, method, arguments);

    /// <summary>Instance method call</summary>
    public static MethodCallExpressionInfo Call(
        ExpressionInfo instance, MethodInfo method, params object[] arguments) =>
        new MethodCallExpressionInfo(instance, method, arguments);

    /// <summary>Instance method call</summary>
    public static MethodCallExpressionInfo Call(
        ExpressionInfo instance, MethodInfo method, params ExpressionInfo[] arguments) =>
        new MethodCallExpressionInfo(instance, method, arguments);

    /// <summary>Static property</summary>
    public static PropertyExpressionInfo Property(PropertyInfo property) =>
        new PropertyExpressionInfo(null, property);

    /// <summary>Instance property</summary>
    public static PropertyExpressionInfo Property(ExpressionInfo instance, PropertyInfo property) =>
        new PropertyExpressionInfo(instance, property);

    /// <summary>Instance property</summary>
    public static PropertyExpressionInfo Property(object instance, PropertyInfo property) =>
        new PropertyExpressionInfo(instance, property);

    /// <summary>Static field</summary>
    public static FieldExpressionInfo Field(FieldInfo field) =>
        new FieldExpressionInfo(null, field);

    /// <summary>Instance field</summary>
    public static FieldExpressionInfo Field(ExpressionInfo instance, FieldInfo field) =>
        new FieldExpressionInfo(instance, field);

    /// <summary>Analog of Expression.Lambda</summary>
    public static LambdaExpressionInfo Lambda(ExpressionInfo body) =>
        new LambdaExpressionInfo(null, body, Tools.Empty<object>());

    /// <summary>Analog of Expression.Lambda</summary>
    public static LambdaExpressionInfo Lambda(ExpressionInfo body,
        params ParameterExpression[] parameters) =>
        new LambdaExpressionInfo(null, body, parameters);

    /// <summary>Analog of Expression.Lambda</summary>
    public static LambdaExpressionInfo Lambda(object body, params object[] parameters) =>
        new LambdaExpressionInfo(null, body, parameters);

    /// <summary>Analog of Expression.Lambda with lambda type specified</summary>
    public static LambdaExpressionInfo Lambda(Type delegateType, object body, params object[] parameters) =>
        new LambdaExpressionInfo(delegateType, body, parameters);

    /// <summary>Analog of Expression.Convert</summary>
    public static UnaryExpressionInfo Convert(ExpressionInfo operand, Type targetType) =>
        new UnaryExpressionInfo(ExpressionType.Convert, operand, targetType);

    /// <summary>Analog of Expression.Lambda</summary>
    public static ExpressionInfo<TDelegate> Lambda<TDelegate>(ExpressionInfo body) =>
        new ExpressionInfo<TDelegate>(body, Tools.Empty<ParameterExpression>());

    /// <summary>Analog of Expression.Lambda</summary>
    public static ExpressionInfo<TDelegate> Lambda<TDelegate>(ExpressionInfo body,
        params ParameterExpression[] parameters) =>
        new ExpressionInfo<TDelegate>(body, parameters);

    /// <summary>Analog of Expression.Lambda</summary>
    public static ExpressionInfo<TDelegate> Lambda<TDelegate>(ExpressionInfo body,
        params ParameterExpressionInfo[] parameters) =>
        new ExpressionInfo<TDelegate>(body, parameters);

    /// <summary>Analog of Expression.ArrayIndex</summary>
    public static BinaryExpressionInfo ArrayIndex(ExpressionInfo array, ExpressionInfo index) =>
        new ArrayIndexExpressionInfo(array, index, array.Type.GetElementType());

    /// <summary>Analog of Expression.ArrayIndex</summary>
    public static BinaryExpressionInfo ArrayIndex(object array, object index) =>
        new ArrayIndexExpressionInfo(array, index, array.GetResultType().GetElementType());

    /// <summary>Expression.Bind used in Expression.MemberInit</summary>
    public static MemberAssignmentInfo Bind(MemberInfo member, ExpressionInfo expression) =>
        new MemberAssignmentInfo(member, expression);

    /// <summary>Analog of Expression.MemberInit</summary>
    public static MemberInitExpressionInfo MemberInit(NewExpressionInfo newExpr,
        params MemberAssignmentInfo[] bindings) =>
        new MemberInitExpressionInfo(newExpr, bindings);

    /// <summary>Enables member assignment on existing instance expression.</summary>
    public static ExpressionInfo MemberInit(ExpressionInfo instanceExpr,
        params MemberAssignmentInfo[] assignments) =>
        new MemberInitExpressionInfo(instanceExpr, assignments);

    /// <summary>Constructs an array given the array type and item initializer expressions.</summary>
    public static NewArrayExpressionInfo NewArrayInit(Type type, params object[] initializers) =>
        new NewArrayExpressionInfo(type, initializers);

    /// <summary>Constructs an array given the array type and item initializer expressions.</summary>
    public static NewArrayExpressionInfo NewArrayInit(Type type, params ExpressionInfo[] initializers) =>
        new NewArrayExpressionInfo(type, initializers);

    /// <summary>Constructs assignment expression.</summary>
    public static ExpressionInfo Assign(ExpressionInfo left, ExpressionInfo right) =>
        new AssignBinaryExpressionInfo(left, right, left.Type);

    /// <summary>Constructs assignment expression from possibly mixed types of left and right.</summary>
    public static ExpressionInfo Assign(object left, object right) =>
        new AssignBinaryExpressionInfo(left, right, left.GetResultType());

    /// <summary>Invoke</summary>
    public static ExpressionInfo Invoke(ExpressionInfo lambda, params object[] args) =>
        new InvocationExpressionInfo(lambda, args, lambda.Type);

    /// <summary>Binary add</summary>
    public static ExpressionInfo Add(ExpressionInfo left, ExpressionInfo right) =>
        new ArithmeticBinaryExpressionInfo(ExpressionType.Add, left, right, left.Type);

    /// <summary>Binary substract</summary>
    public static ExpressionInfo Substract(ExpressionInfo left, ExpressionInfo right) =>
        new ArithmeticBinaryExpressionInfo(ExpressionType.Subtract, left, right, left.Type);

    /// <summary>Binary multiply</summary>
    public static ExpressionInfo Multiply(ExpressionInfo left, ExpressionInfo right) =>
        new ArithmeticBinaryExpressionInfo(ExpressionType.Multiply, left, right, left.Type);

    /// <summary>Binary divide</summary>
    public static ExpressionInfo Divide(ExpressionInfo left, ExpressionInfo right) =>
        new ArithmeticBinaryExpressionInfo(ExpressionType.Divide, left, right, left.Type);

    public static BlockExpressionInfo Block(params object[] expressions) =>
        new BlockExpressionInfo(expressions[expressions.Length - 1].GetResultType(),
            Tools.Empty<ParameterExpressionInfo>(), expressions);

    public static TryExpressionInfo TryCatch(object body, params CatchBlockInfo[] handlers) =>
        new TryExpressionInfo(body, null, handlers);

    public static TryExpressionInfo TryCatchFinally(object body, ExpressionInfo @finally, params CatchBlockInfo[] handlers) =>
        new TryExpressionInfo(body, @finally, handlers);

    public static TryExpressionInfo TryFinally(object body, ExpressionInfo @finally) =>
        new TryExpressionInfo(body, @finally, null);

    public static CatchBlockInfo Catch(ParameterExpressionInfo variable, ExpressionInfo body) =>
        new CatchBlockInfo(variable, body, null, variable.Type);

    public static CatchBlockInfo Catch(Type test, ExpressionInfo body) =>
        new CatchBlockInfo(null, body, null, test);

    public static UnaryExpressionInfo Throw(ExpressionInfo value) =>
        new UnaryExpressionInfo(ExpressionType.Throw, value, typeof(void));
  }
}
