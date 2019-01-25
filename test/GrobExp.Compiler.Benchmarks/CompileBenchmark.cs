using System;
using System.Linq;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;

namespace GrobExp.Compiler.Benchmarks
{
    public class CompileBenchmark
    {
        private Expression<Func<TestClassA, int?>> _simpleExp;

        private Expression<Func<TestClassA, bool>> _subLambda1Exp;

        private Expression<Func<TestClassA, bool>> _subLambda2Exp;

        private Expression<Func<TestClassA, int>> _invoke1Exp;

        private Expression<Func<TestClassA, int>> _invoke2Exp;

        private Expression<Func<TestClassA, int>> _invoke3Exp;

        private Expression<Func<int, int>> _factorialExp;

        private Expression<Func<int, string>> _switchExp;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _simpleExp = o => o.ArrayB[0].C.ArrayD[0].X;

            _subLambda1Exp = o => o.ArrayB.Any(b => b.S == o.S);

            _subLambda2Exp = o => o.ArrayB.Any(b => b.S == o.S && b.C.ArrayD.All(d => d.S == b.S && d.ArrayE.Any(e => e.S == o.S && e.S == b.S && e.S == d.S)));

            _invoke1Exp = o => Helper.func(o.Y, o.Z);

            Expression<Func<int, int, int>> lambda1 = (x, y) => x + y;
            ParameterExpression parameter1 = Expression.Parameter(typeof(TestClassA));
            _invoke2Exp = Expression.Lambda<Func<TestClassA, int>>(Expression.Invoke(lambda1, Expression.MakeMemberAccess(parameter1, typeof(TestClassA).GetField("Y")), Expression.MakeMemberAccess(parameter1, typeof(TestClassA).GetField("Z"))), parameter1);

            Expression<Func<int, int, int>> sum = (x, y) => x + y;
            Expression<Func<int, int, int>> mul = (x, y) => x * y;
            ParameterExpression parameter2 = Expression.Parameter(typeof(TestClassA));
            _invoke3Exp = Expression.Lambda<Func<TestClassA, int>>(Expression.Invoke(sum, Expression.Invoke(mul, Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("Y")), Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("Z"))), Expression.Invoke(mul, Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("P")), Expression.MakeMemberAccess(parameter2, typeof(TestClassA).GetField("Q")))), parameter2);

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
        }

        [Benchmark]
        public void Simple_Compile()
        {
            _simpleExp.Compile();
        }

        [Benchmark]
        public void Simple_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_simpleExp, CompilerOptions.None);
        }

        [Benchmark]
        public void Simple_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_simpleExp, CompilerOptions.All);
        }

        [Benchmark]
        public void SubLambda1_Compile()
        {
            _subLambda1Exp.Compile();
        }

        [Benchmark]
        public void SubLambda1_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_subLambda1Exp, CompilerOptions.None);
        }

        [Benchmark]
        public void SubLambda1_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_subLambda1Exp, CompilerOptions.All);
        }

        [Benchmark]
        public void SubLambda2_Compile()
        {
            _subLambda2Exp.Compile();
        }

        [Benchmark]
        public void SubLambda2_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_subLambda2Exp, CompilerOptions.None);
        }

        [Benchmark]
        public void SubLambda2_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_subLambda2Exp, CompilerOptions.All);
        }

        public void Invoke1_Compile()
        {
            _invoke1Exp.Compile();
        }

        [Benchmark]
        public void Invoke1_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_invoke1Exp, CompilerOptions.None);
        }

        [Benchmark]
        public void Invoke1_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_invoke1Exp, CompilerOptions.All);
        }

        public void Invoke2_Compile()
        {
            _invoke2Exp.Compile();
        }

        [Benchmark]
        public void Invoke2_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_invoke2Exp, CompilerOptions.None);
        }

        [Benchmark]
        public void Invoke2_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_invoke2Exp, CompilerOptions.All);
        }

        public void Invoke3_Compile()
        {
            _invoke3Exp.Compile();
        }

        [Benchmark]
        public void Invoke3_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_invoke3Exp, CompilerOptions.None);
        }

        [Benchmark]
        public void Invoke3_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_invoke3Exp, CompilerOptions.All);
        }

        public void Factorial_Compile()
        {
            _factorialExp.Compile();
        }

        [Benchmark]
        public void Factorial_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_factorialExp, CompilerOptions.None);
        }

        [Benchmark]
        public void Factorial_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_factorialExp, CompilerOptions.All);
        }

        public void Switch_Compile()
        {
            _switchExp.Compile();
        }

        [Benchmark]
        public void Switch_GroboCompile_Without_Checking()
        {
            LambdaCompiler.Compile(_switchExp, CompilerOptions.None);
        }

        [Benchmark]
        public void Switch_GroboCompile_With_Checking()
        {
            LambdaCompiler.Compile(_switchExp, CompilerOptions.All);
        }
    }
}
