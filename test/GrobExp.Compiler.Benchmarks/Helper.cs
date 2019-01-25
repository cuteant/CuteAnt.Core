using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using GrEmit;
using GrobExp.Compiler.ExpressionEmitters;

namespace GrobExp.Compiler.Benchmarks
{
    public static class Helper
    {
        public static int x;
        public static readonly AssemblyBuilder Assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
        public static readonly ModuleBuilder Module = Assembly.DefineDynamicModule(Guid.NewGuid().ToString());

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int? Func1(TestClassA a)
        {
            return a.ArrayB[0].C.ArrayD[0].X;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool Func2(TestClassA a)
        {
            return a.ArrayB.Any(b => b.S == a.S);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool Func3(TestClassA a)
        {
            return a.ArrayB.Any(b => b.S == a.S && b.C.ArrayD.All(d => d.S == b.S && d.ArrayE.Any(e => e.S == a.S && e.S == b.S && e.S == d.S)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int Func4(TestClassA a)
        {
            return xfunc(a.Y, a.Z);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int Func5(TestClassA a)
        {
            return a.Y + a.Z;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int Func6(int n)
        {
            var result = 1;
            while (true)
            {
                if (n > 1)
                    result *= n--;
                else break;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int Func7(TestClassA a)
        {
            return a.Y * a.Z + a.P * a.Q;
        }

        internal static Func<TestClassA, int> Build1()
        {
            var typeBuilder = Module.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.Public);
            var method = typeBuilder.DefineMethod("zzz", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new[] { typeof(TestClassA) });
            using (var il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldfld(typeof(TestClassA).GetField("Y"));
                var y = il.DeclareLocal(typeof(int));
                il.Stloc(y);
                il.Ldarg(0);
                il.Ldfld(typeof(TestClassA).GetField("Z"));
                var z = il.DeclareLocal(typeof(int));
                il.Stloc(z);
                il.Ldloc(y);
                il.Ldloc(z);
                il.Add();
                il.Ret();
            }
            var type = typeBuilder.CreateType();
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(Func<TestClassA, int>), Type.EmptyTypes, Module, true);
            using (var il = new GroboIL(dynamicMethod))
            {
                il.Ldnull();
                il.Ldftn(type.GetMethod("zzz"));
                il.Newobj(typeof(Func<TestClassA, int>).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                il.Ret();
            }
            return ((Func<Func<TestClassA, int>>)dynamicMethod.CreateDelegate(typeof(Func<Func<TestClassA, int>>)))();
        }

        internal static Func<TestClassA, int> Build2()
        {
            var typeBuilder = Module.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.Public);
            var method = typeBuilder.DefineMethod("zzz", MethodAttributes.Public, typeof(int), new[] { typeof(TestClassA) });
            using (var il = new GroboIL(method))
            {
                il.Ldarg(1);
                il.Ldfld(typeof(TestClassA).GetField("Y"));
                il.Ldarg(1);
                il.Ldfld(typeof(TestClassA).GetField("Z"));
                var y = il.DeclareLocal(typeof(int));
                var z = il.DeclareLocal(typeof(int));
                il.Stloc(z);
                il.Stloc(y);
                il.Ldloc(y);
                il.Ldloc(z);
                il.Add();
                il.Ret();
            }
            var type = typeBuilder.CreateType();
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(Func<TestClassA, int>), new[] { typeof(object) }, Module, true);
            using (var il = new GroboIL(dynamicMethod))
            {
                il.Ldarg(0);
                il.Ldftn(type.GetMethod("zzz"));
                il.Newobj(typeof(Func<TestClassA, int>).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                il.Ret();
            }
            return ((Func<object, Func<TestClassA, int>>)dynamicMethod.CreateDelegate(typeof(Func<object, Func<TestClassA, int>>)))(Activator.CreateInstance(type));
        }

        internal static ITest BuildCall()
        {
            var typeBuilder = Module.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.Public);
            var doNothingMethod = typeBuilder.DefineMethod("DoNothingImpl", MethodAttributes.Public, typeof(void), Type.EmptyTypes);
            using (var il = new GroboIL(doNothingMethod))
            {
                il.Ldfld(xField);
                il.Ldc_I4(1);
                il.Add();
                il.Stfld(xField);
                il.Ret();
            }
            var method = typeBuilder.DefineMethod("DoNothing", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), Type.EmptyTypes);
            using (var il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Call(doNothingMethod);
                il.Ret();
            }
            typeBuilder.DefineMethodOverride(method, typeof(ITest).GetMethod("DoNothing"));
            typeBuilder.AddInterfaceImplementation(typeof(ITest));
            var type = typeBuilder.CreateType();
            return (ITest)Activator.CreateInstance(type);
        }

        internal static ITest BuildDelegate()
        {
            var action = Build();
            var typeBuilder = Module.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.Public);
            var actionField = typeBuilder.DefineField("action", typeof(Action), FieldAttributes.Private | FieldAttributes.InitOnly);
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(Action) });
            using (var il = new GroboIL(constructor))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                il.Stfld(actionField);
                il.Ret();
            }
            var method = typeBuilder.DefineMethod("DoNothing", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), Type.EmptyTypes);
            using (var il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldfld(actionField);
                il.Call(typeof(Action).GetMethod("Invoke", Type.EmptyTypes), typeof(Action));
                il.Ret();
            }
            typeBuilder.DefineMethodOverride(method, typeof(ITest).GetMethod("DoNothing"));
            typeBuilder.AddInterfaceImplementation(typeof(ITest));
            var type = typeBuilder.CreateType();
            return (ITest)Activator.CreateInstance(type, new object[] { action.Item1 });
        }

        internal static ITest BuildCalli()
        {
            var action = Build();
            var typeBuilder = Module.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.Public);
            var pointerField = typeBuilder.DefineField("pointer", typeof(IntPtr), FieldAttributes.Private | FieldAttributes.InitOnly);
            var delegateField = typeBuilder.DefineField("delegate", typeof(Delegate), FieldAttributes.Private | FieldAttributes.InitOnly);
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(IntPtr), typeof(Delegate) });
            var il = new GroboIL(constructor);
            il.Ldarg(0);
            il.Ldarg(1);
            il.Stfld(pointerField);
            il.Ldarg(0);
            il.Ldarg(2);
            il.Stfld(delegateField);
            il.Ret();
            var method = typeBuilder.DefineMethod("DoNothing", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), Type.EmptyTypes);
            il = new GroboIL(method);
            il.Ldarg(0);
            il.Ldfld(pointerField);
            il.Calli(CallingConventions.Standard, typeof(void), Type.EmptyTypes);
            il.Ret();
            typeBuilder.DefineMethodOverride(method, typeof(ITest).GetMethod("DoNothing"));
            typeBuilder.AddInterfaceImplementation(typeof(ITest));
            var type = typeBuilder.CreateType();
            return (ITest)Activator.CreateInstance(type, new object[] { DynamicMethodInvokerBuilder.DynamicMethodPointerExtractor((DynamicMethod)action.Item2), action.Item1 });
        }

        internal static Tuple<Action, MethodInfo> Build()
        {
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), Type.EmptyTypes, Module, true);
            using (var il = new GroboIL(dynamicMethod))
            {
                il.Ldfld(xField);
                il.Ldc_I4(1);
                il.Add();
                il.Stfld(xField);
                il.Ret();
            }
            return new Tuple<Action, MethodInfo>((Action)dynamicMethod.CreateDelegate(typeof(Action)), dynamicMethod);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string Func7(int x)
        {
            switch (x)
            {
                case 0:
                case 2:
                    return "zzz";
                case 5:
                case 1000001:
                    return "qxx";
                case 7:
                case 1000000:
                    return "qzz";
                default:
                    return "xxx";
            }
        }

        internal static Func<int, string> BuildSwitch1()
        {
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(string), new[] { typeof(int) }, Module, true);
            using (var il = new GroboIL(dynamicMethod))
            {
                il.Ldarg(0);
                var zzzLabel = il.DefineLabel("zzz");
                var qxxLabel = il.DefineLabel("qxx");
                var qzzLabel = il.DefineLabel("qzz");
                var xxxLabel = il.DefineLabel("xxx");
                il.Switch(zzzLabel, xxxLabel, zzzLabel);
                il.Ldarg(0);
                il.Ldc_I4(5);
                il.Sub();
                il.Switch(qxxLabel, xxxLabel, qzzLabel);
                il.Ldarg(0);
                il.Ldc_I4(0xf4240);
                il.Sub();
                il.Switch(qzzLabel, qxxLabel);
                il.Br(xxxLabel);
                il.MarkLabel(zzzLabel);
                il.Ldstr("zzz");
                il.Ret();
                il.MarkLabel(qxxLabel);
                il.Ldstr("qxx");
                il.Ret();
                il.MarkLabel(qzzLabel);
                il.Ldstr("qzz");
                il.Ret();
                il.MarkLabel(xxxLabel);
                il.Ldstr("xxx");
                il.Ret();
            }
            return (Func<int, string>)dynamicMethod.CreateDelegate(typeof(Func<int, string>));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string Func8(string s)
        {
            switch (s)
            {
                case "0":
                case "2":
                    return "zzz";
                case "5":
                case "1000001":
                    return "qxx";
                case "7":
                case "1000000":
                    return "qzz";
                default:
                    return "xxx";
            }
        }

        public static string[] testValues2;
        public static int[] indexes2;

        private static void Init(string[] values)
        {
            for (int x = values.Length; ; ++x)
            {
                bool[] exist = new bool[x];
                bool ok = true;
                foreach (var s in values)
                {
                    var hash = s.GetHashCode();
                    if (exist[hash % x])
                    {
                        ok = false;
                        break;
                    }
                    exist[hash % x] = true;
                }
                if (ok)
                {
                    testValues2 = new string[x];
                    indexes2 = new int[x];
                    for (int index = 0; index < values.Length; index++)
                    {
                        var s = values[index];
                        var i = s.GetHashCode() % x;
                        testValues2[i] = s;
                        indexes2[i] = index;
                    }
                    return;
                }
            }
        }

        internal static readonly Func<int, int, int> func = (x, y) => x + y;
        internal static readonly Func<int, int, int> xfunc = (x, y) => x + y;

        internal static readonly FieldInfo xField = (FieldInfo)((MemberExpression)((Expression<Func<int>>)(() => x)).Body).Member;
    }
}
