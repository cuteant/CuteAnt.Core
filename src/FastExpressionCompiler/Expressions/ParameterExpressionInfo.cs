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
  /// <summary>Wraps ParameterExpression and just it.</summary>
  public class ParameterExpressionInfo : ExpressionInfo
  {
    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Parameter;

    /// <inheritdoc />
    public override Type Type => ParamExpr.Type;

    /// <summary>Wrapped parameter expression.</summary>
    public ParameterExpression ParamExpr { get; }

    /// <summary>Allow to change parameter expression as info interchangeable.</summary>
    public static implicit operator ParameterExpression(ParameterExpressionInfo info) => info.ParamExpr;

    /// <summary>Optional name.</summary>
    public string Name => ParamExpr.Name;

    /// <inheritdoc />
    public override Expression ToExpression() => ParamExpr;

    /// <summary>Constructor</summary>
    public ParameterExpressionInfo(ParameterExpression paramExpr)
    {
      ParamExpr = paramExpr;
    }
  }
}
