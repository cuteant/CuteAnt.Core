﻿using System;
using System.Linq.Expressions;

using NUnit.Framework;

namespace GrobExp.Compiler.Tests.AssignTests.AndAssign
{
    [TestFixture]
    public class TestStaticMember
    {
        [Test]
        public void TestIntProp()
        {
            ParameterExpression b = Expression.Parameter(typeof(int), "b");
            Expression<Func<int, int>> exp = Expression.Lambda<Func<int, int>>(Expression.AndAssign(Expression.MakeMemberAccess(null, typeof(TestClassA).GetProperty("IntProp")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.CheckNullReferences);
            TestClassA.IntProp = 0;
            Assert.AreEqual(0, f(123));
            Assert.AreEqual(0, TestClassA.IntProp);
            TestClassA.IntProp = 5;
            Assert.AreEqual(1, f(3));
            Assert.AreEqual(1, TestClassA.IntProp);
            TestClassA.IntProp = 17235476;
            Assert.AreEqual(17235476 & 73172563, f(73172563));
            Assert.AreEqual(17235476 & 73172563, TestClassA.IntProp);
        }

        [Test]
        public void TestNullable()
        {
            ParameterExpression b = Expression.Parameter(typeof(int?), "b");
            Expression<Func<int?, int?>> exp = Expression.Lambda<Func<int?, int?>>(Expression.AndAssign(Expression.MakeMemberAccess(null, typeof(TestClassA).GetField("NullableIntField")), b), b);
            var f = LambdaCompiler.Compile(exp, CompilerOptions.All);
            TestClassA.NullableIntField = 0;
            Assert.AreEqual(0, f(123));
            Assert.AreEqual(0, TestClassA.NullableIntField);
            TestClassA.NullableIntField = 5;
            Assert.AreEqual(1, f(3));
            Assert.AreEqual(1, TestClassA.NullableIntField);
            TestClassA.NullableIntField = 17235476;
            Assert.AreEqual(17235476 & 73172563, f(73172563));
            Assert.AreEqual(17235476 & 73172563, TestClassA.NullableIntField);
            TestClassA.NullableIntField = null;
            Assert.IsNull(f(2));
            Assert.IsNull(TestClassA.NullableIntField);
            TestClassA.NullableIntField = 1;
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableIntField);
            Assert.IsNull(f(null));
            Assert.IsNull(TestClassA.NullableIntField);
        }

        public class TestClassA
        {
            public static int IntProp { get; set; }
            public static int? NullableIntField;
        }
    }
}