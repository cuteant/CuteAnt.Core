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

using System.Linq.Expressions;

namespace FastExpressionCompiler
{
  /// <summary>Base class for expressions with arguments.</summary>
  public abstract class ArgumentsExpressionInfo : ExpressionInfo
  {
    /// <summary>List of arguments</summary>
    public readonly object[] Arguments;

    /// <summary>Converts arguments to expressions</summary>
    protected Expression[] ArgumentsToExpressions() => Arguments.Project(Tools.ToExpression);

    /// <summary>Constructor</summary>
    protected ArgumentsExpressionInfo(object[] arguments) => Arguments = arguments ?? Tools.Empty<object>();
  }
}
