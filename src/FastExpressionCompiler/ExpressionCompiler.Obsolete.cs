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
  partial class ExpressionCompiler
  {
    /// <summary>Obsolete: replaced by CompileFast extension method</summary>
    public static Func<T> Compile<T>(Expression<Func<T>> lambdaExpr) => lambdaExpr.CompileFast<Func<T>>();

    /// <summary>Obsolete: replaced by CompileFast extension method</summary>
    public static TDelegate Compile<TDelegate>(LambdaExpression lambdaExpr) where TDelegate : class
        => TryCompile<TDelegate>(lambdaExpr) ?? (TDelegate)(object)lambdaExpr.Compile();
  }
}
