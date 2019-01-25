﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using NUnit.Framework;

namespace GrobExp.Compiler.Tests.AssignTests.DivideAssign
{
    [TestFixture]
    public class TestComplexProperty
    {
        [Test]
        public void TestInt()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            ParameterExpression b = Expression.Parameter(typeof(int), "b");
            Expression<Func<TestClassA, int, int>> exp = Expression.Lambda<Func<TestClassA, int, int>>(Expression.DivideAssign(Expression.MakeIndex(Expression.MakeMemberAccess(a, typeof(TestClassA).GetProperty("IntArray")), typeof(IntArray).GetProperty("Item"), new[] {Expression.Constant("zzz"), Expression.Constant(1)}), b), a, b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {IntArray = new IntArray()};
            o.IntArray["zzz", 1] = 1;
            Assert.AreEqual(0, f(o, 2));
            Assert.AreEqual(0, o.IntArray["zzz", 1]);
            o.IntArray["zzz", 1] = 5;
            Assert.AreEqual(2, f(o, 2));
            Assert.AreEqual(2, o.IntArray["zzz", 1]);
            o.IntArray["zzz", 1] = -3;
            Assert.AreEqual(-1, f(o, 2));
            Assert.AreEqual(-1, o.IntArray["zzz", 1]);
            Assert.AreEqual(0, f(null, 1));

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {IntArray = new IntArray()};
            o.IntArray["zzz", 1] = 1;
            Assert.AreEqual(0, f(o, 2));
            Assert.AreEqual(0, o.IntArray["zzz", 1]);
            o.IntArray["zzz", 1] = 5;
            Assert.AreEqual(2, f(o, 2));
            Assert.AreEqual(2, o.IntArray["zzz", 1]);
            o.IntArray["zzz", 1] = -3;
            Assert.AreEqual(-1, f(o, 2));
            Assert.AreEqual(-1, o.IntArray["zzz", 1]);
            Assert.Throws<NullReferenceException>(() => f(null, 1));
        }

        [Test]
        public void TestNullable()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            ParameterExpression b = Expression.Parameter(typeof(int?), "b");
            Expression<Func<TestClassA, int?, int?>> exp = Expression.Lambda<Func<TestClassA, int?, int?>>(Expression.DivideAssign(Expression.MakeIndex(Expression.MakeMemberAccess(a, typeof(TestClassA).GetProperty("NullableIntArray")), typeof(NullableIntArray).GetProperty("Item"), new[] {Expression.Constant("zzz"), Expression.Constant(1)}), b), a, b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {NullableIntArray = new NullableIntArray()};
            o.NullableIntArray["zzz", 1] = 1;
            Assert.AreEqual(0, f(o, 2));
            Assert.AreEqual(0, o.NullableIntArray["zzz", 1]);
            o.NullableIntArray["zzz", 1] = 5;
            Assert.AreEqual(2, f(o, 2));
            Assert.AreEqual(2, o.NullableIntArray["zzz", 1]);
            o.NullableIntArray["zzz", 1] = -3;
            Assert.AreEqual(-1, f(o, 2));
            Assert.AreEqual(-1, o.NullableIntArray["zzz", 1]);
            Assert.IsNull(f(null, 1));
            o.NullableIntArray["zzz", 1] = null;
            Assert.IsNull(f(o, 2));
            Assert.IsNull(o.NullableIntArray["zzz", 1]);
            o.NullableIntArray["zzz", 1] = 1;
            Assert.IsNull(f(o, null));
            Assert.IsNull(o.NullableIntArray["zzz", 1]);
            Assert.IsNull(f(o, null));
            Assert.IsNull(o.NullableIntArray["zzz", 1]);

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {NullableIntArray = new NullableIntArray()};
            o.NullableIntArray["zzz", 1] = 1;
            Assert.AreEqual(0, f(o, 2));
            Assert.AreEqual(0, o.NullableIntArray["zzz", 1]);
            o.NullableIntArray["zzz", 1] = 5;
            Assert.AreEqual(2, f(o, 2));
            Assert.AreEqual(2, o.NullableIntArray["zzz", 1]);
            o.NullableIntArray["zzz", 1] = -3;
            Assert.AreEqual(-1, f(o, 2));
            Assert.AreEqual(-1, o.NullableIntArray["zzz", 1]);
            Assert.Throws<NullReferenceException>(() => f(null, 1));
            o.NullableIntArray["zzz", 1] = null;
            Assert.IsNull(f(o, 2));
            Assert.IsNull(o.NullableIntArray["zzz", 1]);
            o.NullableIntArray["zzz", 1] = 1;
            Assert.IsNull(f(o, null));
            Assert.IsNull(o.NullableIntArray["zzz", 1]);
            Assert.IsNull(f(o, null));
            Assert.IsNull(o.NullableIntArray["zzz", 1]);
        }

        [Test]
        public void TestDouble()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            ParameterExpression b = Expression.Parameter(typeof(double), "b");
            Expression<Func<TestClassA, double, double>> exp = Expression.Lambda<Func<TestClassA, double, double>>(Expression.DivideAssign(Expression.MakeIndex(Expression.MakeMemberAccess(a, typeof(TestClassA).GetField("DoubleArray")), typeof(DoubleArray).GetProperty("Item"), new[] {Expression.Constant("zzz"), Expression.Constant(1)}), b), a, b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {DoubleArray = new DoubleArray()};
            o.DoubleArray["zzz", 1] = 1;
            Assert.AreEqual(0.5, f(o, 2));
            Assert.AreEqual(0.5, o.DoubleArray["zzz", 1]);
            o.DoubleArray["zzz", 1] = 5;
            Assert.AreEqual(2.5, f(o, 2));
            Assert.AreEqual(2.5, o.DoubleArray["zzz", 1]);
            o.DoubleArray["zzz", 1] = -3;
            Assert.AreEqual(-1.5, f(o, 2));
            Assert.AreEqual(-1.5, o.DoubleArray["zzz", 1]);
            Assert.AreEqual(0, f(null, 1));

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {DoubleArray = new DoubleArray()};
            o.DoubleArray["zzz", 1] = 1;
            Assert.AreEqual(0.5, f(o, 2));
            Assert.AreEqual(0.5, o.DoubleArray["zzz", 1]);
            o.DoubleArray["zzz", 1] = 5;
            Assert.AreEqual(2.5, f(o, 2));
            Assert.AreEqual(2.5, o.DoubleArray["zzz", 1]);
            o.DoubleArray["zzz", 1] = -3;
            Assert.AreEqual(-1.5, f(o, 2));
            Assert.AreEqual(-1.5, o.DoubleArray["zzz", 1]);
            Assert.Throws<NullReferenceException>(() => f(null, 1));
        }

        [Test]
        public void TestUInt()
        {
            ParameterExpression a = Expression.Parameter(typeof(TestClassA), "a");
            ParameterExpression b = Expression.Parameter(typeof(uint), "b");
            Expression<Func<TestClassA, uint, uint>> exp = Expression.Lambda<Func<TestClassA, uint, uint>>(Expression.DivideAssign(Expression.MakeIndex(Expression.MakeMemberAccess(a, typeof(TestClassA).GetField("UIntArray")), typeof(UIntArray).GetProperty("Item"), new[] {Expression.Constant("zzz"), Expression.Constant(1)}), b), a, b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            var o = new TestClassA {UIntArray = new UIntArray()};
            o.UIntArray["zzz", 1] = 1;
            Assert.AreEqual(0, f(o, 2));
            Assert.AreEqual(0, o.UIntArray["zzz", 1]);
            o.UIntArray["zzz", 1] = 5;
            Assert.AreEqual(2, f(o, 2));
            Assert.AreEqual(2, o.UIntArray["zzz", 1]);
            o.UIntArray["zzz", 1] = uint.MaxValue - 3 + 1;
            Assert.AreEqual(2147483646, f(o, 2));
            Assert.AreEqual(2147483646, o.UIntArray["zzz", 1]);
            Assert.AreEqual(0, f(null, 1));

            f = LambdaCompiler.Compile(exp, CompilerOptions.None);
            o = new TestClassA {UIntArray = new UIntArray()};
            o.UIntArray["zzz", 1] = 1;
            Assert.AreEqual(0, f(o, 2));
            Assert.AreEqual(0, o.UIntArray["zzz", 1]);
            o.UIntArray["zzz", 1] = 5;
            Assert.AreEqual(2, f(o, 2));
            Assert.AreEqual(2, o.UIntArray["zzz", 1]);
            o.UIntArray["zzz", 1] = uint.MaxValue - 3 + 1;
            Assert.AreEqual(2147483646, f(o, 2));
            Assert.AreEqual(2147483646, o.UIntArray["zzz", 1]);
            Assert.Throws<NullReferenceException>(() => f(null, 1));
        }

        public class TestClassA
        {
            public IntArray IntArray { get; set; }
            public NullableIntArray NullableIntArray { get; set; }
            public DoubleArray DoubleArray;
            public UIntArray UIntArray;
        }

        public class IntArray
        {
            public int this[string key, int index]
            {
                get { return dict[key][index]; }
                set
                {
                    int[] array;
                    if (!dict.TryGetValue(key, out array))
                        dict.Add(key, array = new int[0]);
                    if (array.Length <= index)
                    {
                        var newArray = new int[index + 1];
                        array.CopyTo(newArray, 0);
                        array = dict[key] = newArray;
                    }
                    array[index] = value;
                }
            }

            private readonly Dictionary<string, int[]> dict = new Dictionary<string, int[]>();
        }

        public class NullableIntArray
        {
            public int? this[string key, int index]
            {
                get { return dict[key][index]; }
                set
                {
                    int?[] array;
                    if (!dict.TryGetValue(key, out array))
                        dict.Add(key, array = new int?[0]);
                    if (array.Length <= index)
                    {
                        var newArray = new int?[index + 1];
                        array.CopyTo(newArray, 0);
                        array = dict[key] = newArray;
                    }
                    array[index] = value;
                }
            }

            private readonly Dictionary<string, int?[]> dict = new Dictionary<string, int?[]>();
        }

        public class UIntArray
        {
            public uint this[string key, int index]
            {
                get { return dict[key][index]; }
                set
                {
                    uint[] array;
                    if (!dict.TryGetValue(key, out array))
                        dict.Add(key, array = new uint[0]);
                    if (array.Length <= index)
                    {
                        var newArray = new uint[index + 1];
                        array.CopyTo(newArray, 0);
                        array = dict[key] = newArray;
                    }
                    array[index] = value;
                }
            }

            private readonly Dictionary<string, uint[]> dict = new Dictionary<string, uint[]>();
        }

        public class DoubleArray
        {
            public double this[string key, int index]
            {
                get { return dict[key][index]; }
                set
                {
                    double[] array;
                    if (!dict.TryGetValue(key, out array))
                        dict.Add(key, array = new double[0]);
                    if (array.Length <= index)
                    {
                        var newArray = new double[index + 1];
                        array.CopyTo(newArray, 0);
                        array = dict[key] = newArray;
                    }
                    array[index] = value;
                }
            }

            private readonly Dictionary<string, double[]> dict = new Dictionary<string, double[]>();
        }
    }
}