using System;
using System.Linq;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;

namespace GrobExp.Compiler.Benchmarks
{
    public class InvokeBenchmark
    {
        private Expression<Func<TestClassA, int?>> _simpleExp;
        private TestClassA _simpleA;
        private Func<TestClassA, int?> _simpleFunc;
        private Func<TestClassA, int?> _simpleFunc1;
        private Func<TestClassA, int?> _simpleFunc2;

        private Expression<Func<TestClassA, bool>> _subLambda1Exp;
        private TestClassA _subLambda1A;
        private Func<TestClassA, bool> _subLambda1Func;
        private Func<TestClassA, bool> _subLambda1Func1;
        private Func<TestClassA, bool> _subLambda1Func2;

        private Expression<Func<TestClassA, bool>> _subLambda2Exp;
        private TestClassA _subLambda2A;
        private Func<TestClassA, bool> _subLambda2Func;
        private Func<TestClassA, bool> _subLambda2Func1;
        private Func<TestClassA, bool> _subLambda2Func2;

        private Expression<Func<TestClassA, int>> _invoke1Exp;
        private TestClassA _invoke1A;
        private Func<TestClassA, int> _invoke1Func;
        private Func<TestClassA, int> _invoke1Func1;
        private Func<TestClassA, int> _invoke1Func2;

        private Expression<Func<TestClassA, int>> _invoke2Exp;
        private TestClassA _invoke2A;
        private Func<TestClassA, int> _invoke2Func;
        private Func<TestClassA, int> _invoke2Func1;
        private Func<TestClassA, int> _invoke2Func2;

        private Expression<Func<TestClassA, int>> _invoke3Exp;
        private TestClassA _invoke3A;
        private Func<TestClassA, int> _invoke3Func;
        private Func<TestClassA, int> _invoke3Func1;
        private Func<TestClassA, int> _invoke3Func2;

        private Expression<Func<int, int>> _factorialExp;
        private Func<int, int> _factorialFunc;
        private Func<int, int> _factorialFunc1;
        private Func<int, int> _factorialFunc2;

        private Expression<Func<int, string>> _switchExp;
        private Func<int, string> _switchFunc;
        private Func<int, string> _switchFunc1;
        private Func<int, string> _switchFunc2;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _simpleExp = o => o.ArrayB[0].C.ArrayD[0].X;
            _simpleA = new TestClassA { ArrayB = new TestClassB[1] { new TestClassB { C = new TestClassC { ArrayD = new TestClassD[1] { new TestClassD { X = 5 } } } } } };
            _simpleFunc = _simpleExp.Compile();
            _simpleFunc1 = LambdaCompiler.Compile(_simpleExp, CompilerOptions.None);
            _simpleFunc2 = LambdaCompiler.Compile(_simpleExp, CompilerOptions.All);

            _subLambda1Exp = o => o.ArrayB.Any(b => b.S == o.S);
            _subLambda1A = new TestClassA { S = "zzz", ArrayB = new[] { new TestClassB { S = "zzz" }, } };
            _subLambda1Func = _subLambda1Exp.Compile();
            _subLambda1Func1 = LambdaCompiler.Compile(_subLambda1Exp, CompilerOptions.None);
            _subLambda1Func2 = LambdaCompiler.Compile(_subLambda1Exp, CompilerOptions.All);

            _subLambda2Exp = o => o.ArrayB.Any(b => b.S == o.S && b.C.ArrayD.All(d => d.S == b.S && d.ArrayE.Any(e => e.S == o.S && e.S == b.S && e.S == d.S)));
            _subLambda2A = new TestClassA
            {
                S = "zzz",
                ArrayB = new[]
                        {
                            new TestClassB
                                {
                                    S = "zzz",
                                    C = new TestClassC
                                        {
                                            ArrayD = new[]
                                                {
                                                    new TestClassD {S = "zzz", ArrayE = new[] {new TestClassE {S = "zzz"},}},
                                                    new TestClassD {S = "zzz", ArrayE = new[] {new TestClassE {S = "zzz"},}}
                                                }
                                        }
                                },
                        }
            };
            _subLambda2Func = _subLambda2Exp.Compile();
            _subLambda2Func1 = LambdaCompiler.Compile(_subLambda2Exp, CompilerOptions.None);
            _subLambda2Func2 = LambdaCompiler.Compile(_subLambda2Exp, CompilerOptions.All);

            _invoke1Exp = o => Helper.func(o.Y, o.Z);
            _invoke1A = new TestClassA { Y = 1, Z = 2 };
            _invoke1Func = _invoke1Exp.Compile();
            _invoke1Func1 = LambdaCompiler.Compile(_invoke1Exp, CompilerOptions.None);
            _invoke1Func2 = LambdaCompiler.Compile(_invoke1Exp, CompilerOptions.All);

            Expression<Func<int, int, int>> lambda1 = (x, y) => x + y;
            ParameterExpression parameter1 = Expression.Parameter(typeof(TestClassA));
            _invoke2Exp = Expression.Lambda<Func<TestClassA, int>>(Expression.Invoke(lambda1, Expression.MakeMemberAccess(parameter1, typeof(TestClassA).GetField("Y")), Expression.MakeMemberAccess(parameter1, typeof(TestClassA).GetField("Z"))), parameter1);
            _invoke2A = new TestClassA { Y = 1, Z = 2 };
            _invoke2Func = _invoke2Exp.Compile();
            _invoke2Func1 = LambdaCompiler.Compile(_invoke2Exp, CompilerOptions.None);
            _invoke2Func2 = LambdaCompiler.Compile(_invoke2Exp, CompilerOptions.All);

            Expression<Func<int, int, int>> sum = (x, y) => x + y;
            Expression<Func<int, int, int>> mul = (x, y) => x * y;
            ParameterExpression parameter2 = Expression.Parameter(typeof(TestClassA));
            _invoke3Exp = Expression.Lambda<Func<TestClassA, int>>(Expression.Invoke(sum, Expression.Invoke(mul, Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("Y")), Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("Z"))), Expression.Invoke(mul, Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("P")), Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("Q")))), parameter2);
            _invoke3A = new TestClassA { Y = 1, Z = 2, P = 3, Q = 4 };
            _invoke3Func = _invoke3Exp.Compile();
            _invoke3Func1 = LambdaCompiler.Compile(_invoke3Exp, CompilerOptions.None);
            _invoke3Func2 = LambdaCompiler.Compile(_invoke3Exp, CompilerOptions.All);

            ParameterExpression value = Expression.Parameter(typeof(int), "value");
            ParameterExpression result = Expression.Parameter(typeof(int), "result");
            LabelTarget label = Expression.Label(typeof(int));
            BlockExpression block = Expression.Block(
                new[] { result },
                Expression.Assign(result, Expression.Constant(1)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.GreaterThan(value, Expression.Constant(1)),
                        Expression.MultiplyAssign(result,
                                                  Expression.PostDecrementAssign(value)),
                        Expression.Break(label, result)
                        ),
                    label
                    )
                );
            _factorialExp = Expression.Lambda<Func<int, int>>(block, value);
            _factorialFunc = _factorialExp.Compile();
            _factorialFunc1 = LambdaCompiler.Compile(_factorialExp, CompilerOptions.None);
            _factorialFunc2 = LambdaCompiler.Compile(_factorialExp, CompilerOptions.All);

            ParameterExpression a = Expression.Parameter(typeof(int));
            _switchExp = Expression.Lambda<Func<int, string>>(
                Expression.Switch(
                    a,
                    Expression.Constant("xxx"),
                    Expression.SwitchCase(Expression.Constant("zzz"), Expression.Constant(0), Expression.Constant(2)),
                    Expression.SwitchCase(Expression.Constant("qxx"), Expression.Constant(5), Expression.Constant(1000001)),
                    Expression.SwitchCase(Expression.Constant("qzz"), Expression.Constant(7), Expression.Constant(1000000))
                    ),
                a
                );
            _switchFunc = _switchExp.Compile();
            _switchFunc1 = LambdaCompiler.Compile(_switchExp, CompilerOptions.None);
            _switchFunc2 = LambdaCompiler.Compile(_switchExp, CompilerOptions.All);
        }

        [Benchmark]
        public void Simple_Compile()
        {
            var r = _simpleFunc(_simpleA);
        }

        [Benchmark]
        public void Simple_GroboCompile_Without_Checking()
        {
            var r = _simpleFunc1(_simpleA);
        }

        [Benchmark]
        public void Simple_GroboCompile_With_Checking()
        {
            var r = _simpleFunc2(_simpleA);
        }

        [Benchmark]
        public void SubLambda1_Compile()
        {
            var r = _subLambda1Func(_subLambda1A);
        }

        [Benchmark]
        public void SubLambda1_GroboCompile_Without_Checking()
        {
            var r = _subLambda1Func1(_subLambda1A);
        }

        [Benchmark]
        public void SubLambda1_GroboCompile_With_Checking()
        {
            var r = _subLambda1Func2(_subLambda1A);
        }

        [Benchmark]
        public void SubLambda2_Compile()
        {
            var r = _subLambda2Func(_subLambda2A);
        }

        [Benchmark]
        public void SubLambda2_GroboCompile_Without_Checking()
        {
            var r = _subLambda2Func1(_subLambda2A);
        }

        [Benchmark]
        public void SubLambda2_GroboCompile_With_Checking()
        {
            var r = _subLambda2Func2(_subLambda2A);
        }

        [Benchmark]
        public void Invoke1_Compile()
        {
            var r = _invoke1Func(_invoke1A);
        }

        [Benchmark]
        public void Invoke1_GroboCompile_Without_Checking()
        {
            var r = _invoke1Func1(_invoke1A);
        }

        [Benchmark]
        public void Invoke1_GroboCompile_With_Checking()
        {
            var r = _invoke1Func2(_invoke1A);
        }

        [Benchmark]
        public void Invoke2_Compile()
        {
            var r = _invoke2Func(_invoke2A);
        }

        [Benchmark]
        public void Invoke2_GroboCompile_Without_Checking()
        {
            var r = _invoke2Func1(_invoke2A);
        }

        [Benchmark]
        public void Invoke2_GroboCompile_With_Checking()
        {
            var r = _invoke2Func2(_invoke2A);
        }

        [Benchmark]
        public void Invoke3_Compile()
        {
            var r = _invoke3Func(_invoke3A);
        }

        [Benchmark]
        public void Invoke3_GroboCompile_Without_Checking()
        {
            var r = _invoke3Func1(_invoke3A);
        }

        [Benchmark]
        public void Invoke3_GroboCompile_With_Checking()
        {
            var r = _invoke3Func2(_invoke3A);
        }

        [Benchmark]
        public void Factorial_Compile()
        {
            var r = _factorialFunc(5);
        }

        [Benchmark]
        public void Factorial_GroboCompile_Without_Checking()
        {
            var r = _factorialFunc1(5);
        }

        [Benchmark]
        public void Factorial_GroboCompile_With_Checking()
        {
            var r = _factorialFunc2(5);
        }

        [Benchmark]
        public void Switch_Compile()
        {
            var r = _switchFunc(2);
        }

        [Benchmark]
        public void Switch_GroboCompile_Without_Checking()
        {
            var r = _switchFunc1(2);
        }

        [Benchmark]
        public void Switch_GroboCompile_With_Checking()
        {
            var r = _switchFunc2(2);
        }
    }
}
