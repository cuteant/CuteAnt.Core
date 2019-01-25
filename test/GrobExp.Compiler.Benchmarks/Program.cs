using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace GrobExp.Compiler.Benchmarks
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);

            Console.WriteLine("按任意键退出......");
            Console.ReadKey();
        }
    }
}
