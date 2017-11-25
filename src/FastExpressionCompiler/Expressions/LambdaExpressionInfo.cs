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

namespace FastExpressionCompiler
{
  /// <summary>LambdaExpression</summary>
  public class LambdaExpressionInfo : ArgumentsExpressionInfo
  {
    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Lambda;

    /// <inheritdoc />
    public override Type Type { get; }

    /// <summary>Lambda body.</summary>
    public readonly object Body;

    /// <summary>List of parameters.</summary>
    public object[] Parameters => Arguments;

    /// <inheritdoc />
    public override Expression ToExpression() => ToLambdaExpression();

    /// <summary>subject</summary>
    public LambdaExpression ToLambdaExpression()
        => Expression.Lambda(Body.ToExpression(), Parameters.Project(p => (ParameterExpression)p.ToExpression()));

    /// <summary>Constructor</summary>
    public LambdaExpressionInfo(object body, object[] parameters) 
      : base(parameters)
    {
      Body = body;
      var bodyType = body.GetResultType();
      Type = Tools.GetFuncOrActionType(Tools.GetParamExprTypes(parameters), bodyType);
    }
  }
}
