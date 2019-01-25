﻿using System;
using System.Linq.Expressions;

using NUnit.Framework;

namespace GrobExp.Compiler.Tests.AssignTests.PreDecrementAssign
{
    [TestFixture]
    public class TestMultiDimensionalArray
    {
        [Test]
        public void TestInt()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            Expression<Func<TestClassA, int>> exp = Expression.Lambda<Func<TestClassA, int>>(Expression.PreDecrementAssign(Expression.ArrayAccess(Expression.MakeMemberAccess(a, typeof(TestClassA).GetProperty("IntArray")), Expression.Constant(0), Expression.Constant(0))), a);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {IntArray = new int[1, 1]};
            o.IntArray[0, 0] = 0;
            Assert.AreEqual(-1, f(o));
            Assert.AreEqual(-1, o.IntArray[0, 0]);
            o.IntArray[0, 0] = 1;
            Assert.AreEqual(0, f(o));
            Assert.AreEqual(0, o.IntArray[0, 0]);
            o.IntArray[0, 0] = int.MinValue;
            Assert.AreEqual(int.MaxValue, f(o));
            Assert.AreEqual(int.MaxValue, o.IntArray[0, 0]);
            Assert.AreEqual(0, f(null));

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {IntArray = new int[1, 1]};
            o.IntArray[0, 0] = 0;
            Assert.AreEqual(-1, f(o));
            Assert.AreEqual(-1, o.IntArray[0, 0]);
            o.IntArray[0, 0] = 1;
            Assert.AreEqual(0, f(o));
            Assert.AreEqual(0, o.IntArray[0, 0]);
            o.IntArray[0, 0] = int.MinValue;
            Assert.AreEqual(int.MaxValue, f(o));
            Assert.AreEqual(int.MaxValue, o.IntArray[0, 0]);
            Assert.Throws<NullReferenceException>(() => f(null));
        }

        [Test]
        public void TestNullable()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            Expression<Func<TestClassA, int?>> exp = Expression.Lambda<Func<TestClassA, int?>>(Expression.PreDecrementAssign(Expression.ArrayAccess(Expression.MakeMemberAccess(a, typeof(TestClassA).GetProperty("NullableIntArray")), Expression.Constant(0), Expression.Constant(0))), a);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {NullableIntArray = new int?[1, 1]};
            o.NullableIntArray[0, 0] = 0;
            Assert.AreEqual(-1, f(o));
            Assert.AreEqual(-1, o.NullableIntArray[0, 0]);
            o.NullableIntArray[0, 0] = 1;
            Assert.AreEqual(0, f(o));
            Assert.AreEqual(0, o.NullableIntArray[0, 0]);
            o.NullableIntArray[0, 0] = int.MinValue;
            Assert.AreEqual(int.MaxValue, f(o));
            Assert.AreEqual(int.MaxValue, o.NullableIntArray[0, 0]);
            Assert.IsNull(f(null));
            o.NullableIntArray[0, 0] = null;
            Assert.IsNull(f(o));
            Assert.IsNull(o.NullableIntArray[0, 0]);

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {NullableIntArray = new int?[1, 1]};
            o.NullableIntArray[0, 0] = 0;
            Assert.AreEqual(-1, f(o));
            Assert.AreEqual(-1, o.NullableIntArray[0, 0]);
            o.NullableIntArray[0, 0] = 1;
            Assert.AreEqual(0, f(o));
            Assert.AreEqual(0, o.NullableIntArray[0, 0]);
            o.NullableIntArray[0, 0] = int.MinValue;
            Assert.AreEqual(int.MaxValue, f(o));
            Assert.AreEqual(int.MaxValue, o.NullableIntArray[0, 0]);
            Assert.Throws<NullReferenceException>(() => f(null));
            o.NullableIntArray[0, 0] = null;
            Assert.IsNull(f(o));
            Assert.IsNull(o.NullableIntArray[0, 0]);
        }

        [Test]
        public void TestDouble()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            Expression<Func<TestClassA, double>> exp = Expression.Lambda<Func<TestClassA, double>>(Expression.PreDecrementAssign(Expression.ArrayAccess(Expression.MakeMemberAccess(a, typeof(TestClassA).GetField("DoubleArray")), Expression.Constant(0), Expression.Constant(0))), a);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {DoubleArray = new double[1, 1]};
            o.DoubleArray[0, 0] = 0;
            Assert.AreEqual(-1, f(o));
            Assert.AreEqual(-1, o.DoubleArray[0, 0]);
            o.DoubleArray[0, 0] = 1;
            Assert.AreEqual(0, f(o));
            Assert.AreEqual(0, o.DoubleArray[0, 0]);
            o.DoubleArray[0, 0] = 0.5;
            Assert.AreEqual(-0.5, f(o));
            Assert.AreEqual(-0.5, o.DoubleArray[0, 0]);
            Assert.AreEqual(0, f(null));

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {DoubleArray = new double[1, 1]};
            o.DoubleArray[0, 0] = 0;
            Assert.AreEqual(-1, f(o));
            Assert.AreEqual(-1, o.DoubleArray[0, 0]);
            o.DoubleArray[0, 0] = 1;
            Assert.AreEqual(0, f(o));
            Assert.AreEqual(0, o.DoubleArray[0, 0]);
            o.DoubleArray[0, 0] = 0.5;
            Assert.AreEqual(-0.5, f(o));
            Assert.AreEqual(-0.5, o.DoubleArray[0, 0]);
            Assert.Throws<NullReferenceException>(() => f(null));
        }

        public class TestClassA
        {
            public int[,] IntArray { get; set; }
            public int?[,] NullableIntArray { get; set; }
            public double[,] DoubleArray;
        }
    }
}