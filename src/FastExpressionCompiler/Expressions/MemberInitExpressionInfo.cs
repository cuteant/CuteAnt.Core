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
  /// <summary>Analog of MemberInitExpression</summary>
  public class MemberInitExpressionInfo : ExpressionInfo
  {
    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.MemberInit;

    /// <inheritdoc />
    public override Type Type => ExpressionInfo.Type;

    /// <summary>New expression.</summary>
    public NewExpressionInfo NewExpressionInfo => ExpressionInfo as NewExpressionInfo;

    /// <summary>New expression.</summary>
    public readonly ExpressionInfo ExpressionInfo;

    /// <summary>Member assignments.</summary>
    public readonly MemberAssignmentInfo[] Bindings;

    /// <inheritdoc />
    public override Expression ToExpression()
        => Expression.MemberInit(NewExpressionInfo.ToNewExpression(), Bindings.Project(b => b.ToMemberAssignment()));

    /// <summary>Constructs from the new expression and member initialization list.</summary>
    public MemberInitExpressionInfo(NewExpressionInfo newExpressionInfo, MemberAssignmentInfo[] bindings)
      : this((ExpressionInfo)newExpressionInfo, bindings) { }

    /// <summary>Constructs from existing expression and member assignment list.</summary>
    public MemberInitExpressionInfo(ExpressionInfo expressionInfo, MemberAssignmentInfo[] bindings)
    {
      ExpressionInfo = expressionInfo;
      Bindings = bindings ?? Tools.Empty<MemberAssignmentInfo>();
    }
  }
}
