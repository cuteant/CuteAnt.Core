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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastExpressionCompiler
{
  partial class ExpressionCompiler
  {
    private struct NestedLambdaInfo
    {
      public ClosureInfo ClosureInfo;

      public object LambdaExpr; // to find the lambda in bigger parent expression
      public object Lambda;
      public bool IsAction;

      public NestedLambdaInfo(ClosureInfo closureInfo, object lambdaExpr, object lambda, bool isAction)
      {
        ClosureInfo = closureInfo;
        Lambda = lambda;
        LambdaExpr = lambdaExpr;
        IsAction = isAction;
      }
    }

    internal static class CurryClosureFuncs
    {
      private static readonly IEnumerable<MethodInfo> _methods = typeof(CurryClosureFuncs)
#if NET40
          .GetTypeDeclaredMethods();
#else
          .GetTypeInfo().DeclaredMethods;
#endif

      public static readonly MethodInfo[] Methods = _methods as MethodInfo[] ?? _methods.ToArray();

      public static Func<R> Curry<C, R>(Func<C, R> f, C c) { return () => f(c); }
      public static Func<T1, R> Curry<C, T1, R>(Func<C, T1, R> f, C c) { return t1 => f(c, t1); }
      public static Func<T1, T2, R> Curry<C, T1, T2, R>(Func<C, T1, T2, R> f, C c) { return (t1, t2) => f(c, t1, t2); }
      public static Func<T1, T2, T3, R> Curry<C, T1, T2, T3, R>(Func<C, T1, T2, T3, R> f, C c) { return (t1, t2, t3) => f(c, t1, t2, t3); }
      public static Func<T1, T2, T3, T4, R> Curry<C, T1, T2, T3, T4, R>(Func<C, T1, T2, T3, T4, R> f, C c) { return (t1, t2, t3, t4) => f(c, t1, t2, t3, t4); }
      public static Func<T1, T2, T3, T4, T5, R> Curry<C, T1, T2, T3, T4, T5, R>(Func<C, T1, T2, T3, T4, T5, R> f, C c) { return (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5); }
      public static Func<T1, T2, T3, T4, T5, T6, R> Curry<C, T1, T2, T3, T4, T5, T6, R>(Func<C, T1, T2, T3, T4, T5, T6, R> f, C c) { return (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6); }
    }

    internal static class CurryClosureActions
    {
      private static readonly IEnumerable<MethodInfo> _methods = typeof(CurryClosureActions)
#if NET40
          .GetTypeDeclaredMethods();
#else
          .GetTypeInfo().DeclaredMethods;
#endif

      public static readonly MethodInfo[] Methods = _methods as MethodInfo[] ?? _methods.ToArray();

      internal static Action Curry<C>(Action<C> a, C c) { return () => a(c); }
      internal static Action<T1> Curry<C, T1>(Action<C, T1> f, C c) { return t1 => f(c, t1); }
      internal static Action<T1, T2> Curry<C, T1, T2>(Action<C, T1, T2> f, C c) { return (t1, t2) => f(c, t1, t2); }
      internal static Action<T1, T2, T3> Curry<C, T1, T2, T3>(Action<C, T1, T2, T3> f, C c) { return (t1, t2, t3) => f(c, t1, t2, t3); }
      internal static Action<T1, T2, T3, T4> Curry<C, T1, T2, T3, T4>(Action<C, T1, T2, T3, T4> f, C c) { return (t1, t2, t3, t4) => f(c, t1, t2, t3, t4); }
      internal static Action<T1, T2, T3, T4, T5> Curry<C, T1, T2, T3, T4, T5>(Action<C, T1, T2, T3, T4, T5> f, C c) { return (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5); }
      internal static Action<T1, T2, T3, T4, T5, T6> Curry<C, T1, T2, T3, T4, T5, T6>(Action<C, T1, T2, T3, T4, T5, T6> f, C c) { return (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6); }
    }
  }
}
