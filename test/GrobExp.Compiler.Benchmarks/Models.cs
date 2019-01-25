using System;
using System.Collections.Generic;
using System.Text;

namespace GrobExp.Compiler.Benchmarks
{
    public class TestClassA
    {
        public int F(bool b)
        {
            return b ? 1 : 0;
        }

        public string S { get; set; }
        public TestClassA A { get; set; }
        public TestClassB B { get; set; }
        public TestClassB[] ArrayB { get; set; }
        public int[] IntArray { get; set; }
        public int? X;
        public Guid Guid = Guid.Empty;
        public Guid? NullableGuid;
        public bool? NullableBool;
        public int Y;
        public int Z;
        public int P;
        public int Q;
        public bool Bool;
    }

    public class TestClassB
    {
        public int? F2(int? x)
        {
            return x;
        }

        public int? F( /*Qzz*/ int a, int b)
        {
            return b;
        }

        public string S { get; set; }

        public TestClassC C { get; set; }
        public int? X;
        public int Y;
    }

    public class TestClassC
    {
        public string S { get; set; }

        public TestClassD D { get; set; }

        public TestClassD[] ArrayD { get; set; }
    }

    public class TestClassD
    {
        public TestClassE E { get; set; }
        public TestClassE[] ArrayE { get; set; }
        public string Z { get; set; }

        public int? X { get; set; }

        public string S;
    }

    public class TestClassE
    {
        public string S { get; set; }
        public int X { get; set; }
    }

    public interface ITest
    {
        void DoNothing();
    }
}
