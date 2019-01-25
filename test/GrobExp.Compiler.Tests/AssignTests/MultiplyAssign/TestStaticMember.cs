﻿using System;
using System.Linq.Expressions;

using NUnit.Framework;

namespace GrobExp.Compiler.Tests.AssignTests.MultiplyAssign
{
    [TestFixture]
    public class TestStaticMember
    {
        [Test]
        public void TestProp()
        {
            ParameterExpression b = Expression.Parameter(typeof(int), "b");
            Expression<Func<int, int>> exp = Expression.Lambda<Func<int, int>>(Expression.MultiplyAssign(Expression.MakeMemberAccess(null, typeof(TestClassA).GetProperty("IntProp")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            TestClassA.IntProp = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.IntProp);
            TestClassA.IntProp = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.IntProp);
            TestClassA.IntProp = -2;
            Assert.AreEqual(6, f(-3));
            Assert.AreEqual(6, TestClassA.IntProp);
            TestClassA.IntProp = -2;
            Assert.AreEqual(-20, f(10));
            Assert.AreEqual(-20, TestClassA.IntProp);
            TestClassA.IntProp = 2000000000;
            unchecked
            {
                Assert.AreEqual(2000000000 * 2000000000, f(2000000000));
                Assert.AreEqual(2000000000 * 2000000000, TestClassA.IntProp);
            }
        }

        [Test]
        public void TestField()
        {
            ParameterExpression b = Expression.Parameter(typeof(int), "b");
            Expression<Func<int, int>> exp = Expression.Lambda<Func<int, int>>(Expression.MultiplyAssign(Expression.MakeMemberAccess(null, typeof(TestClassA).GetField("IntField")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            TestClassA.IntField = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.IntField);
            TestClassA.IntField = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.IntField);
            TestClassA.IntField = -2;
            Assert.AreEqual(6, f(-3));
            Assert.AreEqual(6, TestClassA.IntField);
            TestClassA.IntField = -2;
            Assert.AreEqual(-20, f(10));
            Assert.AreEqual(-20, TestClassA.IntField);
            TestClassA.IntField = 2000000000;
            unchecked
            {
                Assert.AreEqual(2000000000 * 2000000000, f(2000000000));
                Assert.AreEqual(2000000000 * 2000000000, TestClassA.IntField);
            }
        }

        [Test]
        public void TestNullable()
        {
            ParameterExpression b = Expression.Parameter(typeof(int?), "b");
            Expression<Func<int?, int?>> exp = Expression.Lambda<Func<int?, int?>>(Expression.MultiplyAssign(Expression.MakeMemberAccess(null, typeof(TestClassA).GetProperty("NullableIntProp")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.All);
            TestClassA.NullableIntProp = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = -2;
            Assert.AreEqual(6, f(-3));
            Assert.AreEqual(6, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = -2;
            Assert.AreEqual(-20, f(10));
            Assert.AreEqual(-20, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = 2000000000;
            unchecked
            {
                Assert.AreEqual(2000000000 * 2000000000, f(2000000000));
                Assert.AreEqual(2000000000 * 2000000000, TestClassA.NullableIntProp);
            }
            TestClassA.NullableIntProp = null;
            Assert.IsNull(f(2));
            Assert.IsNull(TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = 1;
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableIntProp);
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableIntProp);
        }

        [Test]
        public void TestCheckedSigned()
        {
            ParameterExpression b = Expression.Parameter(typeof(int), "b");
            Expression<Func<int, int>> exp = Expression.Lambda<Func<int, int>>(Expression.MultiplyAssignChecked(Expression.MakeMemberAccess(null, typeof(TestClassA).GetProperty("IntProp")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            TestClassA.IntProp = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.IntProp);
            TestClassA.IntProp = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.IntProp);
            TestClassA.IntProp = -2;
            Assert.AreEqual(6, f(-3));
            Assert.AreEqual(6, TestClassA.IntProp);
            TestClassA.IntProp = -2;
            Assert.AreEqual(-20, f(10));
            Assert.AreEqual(-20, TestClassA.IntProp);
            TestClassA.IntProp = 2000000000;
            Assert.Throws<OverflowException>(() => f(2000000000));
        }

        [Test]
        public void TestCheckedNullableSigned()
        {
            ParameterExpression b = Expression.Parameter(typeof(int?), "b");
            Expression<Func<int?, int?>> exp = Expression.Lambda<Func<int?, int?>>(Expression.MultiplyAssignChecked(Expression.MakeMemberAccess(null, typeof(TestClassA).GetProperty("NullableIntProp")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.All);
            TestClassA.NullableIntProp = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = -2;
            Assert.AreEqual(6, f(-3));
            Assert.AreEqual(6, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = -2;
            Assert.AreEqual(-20, f(10));
            Assert.AreEqual(-20, TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = 2000000000;
            Assert.Throws<OverflowException>(() => f(2000000000));
            TestClassA.NullableIntProp = null;
            Assert.IsNull(f(2));
            Assert.IsNull(TestClassA.NullableIntProp);
            TestClassA.NullableIntProp = 1;
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableIntProp);
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableIntProp);
        }

        [Test]
        public void TestCheckedUnsigned()
        {
            ParameterExpression b = Expression.Parameter(typeof(uint), "b");
            Expression<Func<uint, uint>> exp = Expression.Lambda<Func<uint, uint>>(Expression.MultiplyAssignChecked(Expression.MakeMemberAccess(null, typeof(TestClassA).GetField("UIntField")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            TestClassA.UIntField = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.UIntField);
            TestClassA.UIntField = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.UIntField);
            TestClassA.UIntField = 2000000000;
            Assert.AreEqual(4000000000, f(2));
            Assert.AreEqual(4000000000, TestClassA.UIntField);
            TestClassA.UIntField = 2000000000;
            Assert.Throws<OverflowException>(() => f(3));
        }

        [Test]
        public void TestCheckedNullableUnsigned()
        {
            ParameterExpression b = Expression.Parameter(typeof(uint?), "b");
            Expression<Func<uint?, uint?>> exp = Expression.Lambda<Func<uint?, uint?>>(Expression.MultiplyAssignChecked(Expression.MakeMemberAccess(null, typeof(TestClassA).GetField("NullableUIntField")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.All);
            TestClassA.NullableUIntField = 0;
            Assert.AreEqual(0, f(0));
            Assert.AreEqual(0, TestClassA.NullableUIntField);
            TestClassA.NullableUIntField = 1;
            Assert.AreEqual(2, f(2));
            Assert.AreEqual(2, TestClassA.NullableUIntField);
            TestClassA.NullableUIntField = 2000000000;
            Assert.AreEqual(4000000000, f(2));
            Assert.AreEqual(4000000000, TestClassA.NullableUIntField);
            TestClassA.NullableUIntField = 2000000000;
            Assert.Throws<OverflowException>(() => f(3));
            TestClassA.NullableUIntField = null;
            Assert.IsNull(f(2));
            Assert.IsNull(TestClassA.NullableUIntField);
            TestClassA.NullableUIntField = 1;
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableUIntField);
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableUIntField);
        }

        public class TestClassA
        {
            public static int IntProp { get; set; }
            public static int? NullableIntProp { get; set; }
            public static int IntField;
            public static uint UIntField;
            public static uint? NullableUIntField;
        }
    }
}