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
  public class TryExpressionInfo : ExpressionInfo
  {
    public override ExpressionType NodeType => ExpressionType.Try;
    public override Type Type { get; }

    public readonly object Body;
    public readonly CatchBlockInfo[] Handlers;
    public readonly ExpressionInfo Finally;

    public override Expression ToExpression() =>
        Finally == null ? Expression.TryCatch(Body.ToExpression(), ToCatchBlocks(Handlers)) :
        Handlers == null ? Expression.TryFinally(Body.ToExpression(), Finally.ToExpression()) :
        Expression.TryCatchFinally(Body.ToExpression(), Finally.ToExpression(), ToCatchBlocks(Handlers));

    private static CatchBlock[] ToCatchBlocks(CatchBlockInfo[] hs)
    {
      if (hs == null)
        return Tools.Empty<CatchBlock>();
      var catchBlocks = new CatchBlock[hs.Length];
      for (var i = 0; i < hs.Length; ++i)
        catchBlocks[i] = hs[i].ToCatchBlock();
      return catchBlocks;
    }

    public TryExpressionInfo(object body, ExpressionInfo @finally, CatchBlockInfo[] handlers)
    {
      Type = body.GetResultType();
      Body = body;
      Handlers = handlers;
      Finally = @finally;
    }
  }
}
