﻿/*
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
  internal class ArithmeticBinaryExpressionInfo : BinaryExpressionInfo
  {
    public override Expression ToExpression() =>
        NodeType == ExpressionType.Add ? Expression.Add(Left.ToExpression(), Right.ToExpression()) :
        NodeType == ExpressionType.Subtract ? Expression.Subtract(Left.ToExpression(), Right.ToExpression()) :
        NodeType == ExpressionType.Multiply ? Expression.Multiply(Left.ToExpression(), Right.ToExpression()) :
        NodeType == ExpressionType.Divide ? Expression.Divide(Left.ToExpression(), Right.ToExpression()) :
        throw new NotSupportedException($"Not valid {NodeType} for arithmetic binary expression.");

    public ArithmeticBinaryExpressionInfo(ExpressionType nodeType, object left, object right, Type type)
      : base(nodeType, left, right, type) { }
  }
}