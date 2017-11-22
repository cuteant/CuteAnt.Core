// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace CuteAnt.Performance
{
  class Program
  {
    static void Main(string[] args)
    {
      BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);

      //var _ctorInvoker = typeof(AA).MakeDelegateForCtor<AA>(typeof(BB), typeof(CC));
      //var aa = _ctorInvoker(new object[] { new BB(), new CC() });
      //aa.Foo();

      //var ctorInvoker = typeof(CC).MakeDelegateForCtor<CC>();
      //var cc = ctorInvoker(new object[0]);
      //Console.WriteLine(cc.GetType().FullName);
      Console.WriteLine("按任意键退出......");
      Console.ReadKey();
    }
  }
}