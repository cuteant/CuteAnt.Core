using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;

internal static class Test
{
    public static void Async(Func<Task> test)
    {
        test().Wait();
    }
}