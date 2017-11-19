﻿using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using static FastExpressionCompiler.ExpressionInfo;

namespace FastExpressionCompiler.UnitTests
{
    [TestFixture]
    public class MemberInitExpressionInfoTests
    {
        [Test]
        public void Can_assign_member_of_existing_object()
        {
            var a = new A();
            var aConstExpr = Constant(a);
            var expr = Lambda<Func<A>>(
                MemberInit(
                    aConstExpr,
                    Bind(typeof(A).GetTypeInfo().DeclaredFields.First(m => m.Name == "N"),
                        Constant(42))));

            var f = ExpressionCompiler.TryCompile(expr);

            Assert.AreEqual(42, f().N);
        }

        [Test]
        public void Can_assign_Readonly_member_of_existing_object()
        {
            var a = new A();
            var aConstExpr = Constant(a);
            var expr = Lambda<Func<A>>(
                MemberInit(
                    aConstExpr,
                    Bind(typeof(A).GetTypeInfo().DeclaredFields.First(m => m.Name == "R"),
                        Constant(24))));

            var f = ExpressionCompiler.TryCompile(expr);

            Assert.AreEqual(24, f().R);
        }

        public class A
        {
            public int N = 0;
            public readonly int R = 0;
        }
    }
}
