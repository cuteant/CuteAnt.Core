/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastExpressionCompiler
{
  partial class ExpressionCompiler
  {
    internal static class Closure
    {
      private static readonly IEnumerable<MethodInfo> _methods = typeof(Closure)
#if NET40
          .GetTypeDeclaredMethods();
#else
          .GetTypeInfo().DeclaredMethods;
#endif

      public static readonly MethodInfo[] CreateMethods =
          _methods as MethodInfo[] ?? _methods.ToArray();

      public static Closure<T1> CreateClosure<T1>(T1 v1)
      {
        return new Closure<T1>(v1);
      }

      public static Closure<T1, T2> CreateClosure<T1, T2>(T1 v1, T2 v2)
      {
        return new Closure<T1, T2>(v1, v2);
      }

      public static Closure<T1, T2, T3> CreateClosure<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
      {
        return new Closure<T1, T2, T3>(v1, v2, v3);
      }

      public static Closure<T1, T2, T3, T4> CreateClosure<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
      {
        return new Closure<T1, T2, T3, T4>(v1, v2, v3, v4);
      }

      public static Closure<T1, T2, T3, T4, T5> CreateClosure<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4,
          T5 v5)
      {
        return new Closure<T1, T2, T3, T4, T5>(v1, v2, v3, v4, v5);
      }

      public static Closure<T1, T2, T3, T4, T5, T6> CreateClosure<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3,
          T4 v4, T5 v5, T6 v6)
      {
        return new Closure<T1, T2, T3, T4, T5, T6>(v1, v2, v3, v4, v5, v6);
      }

      public static Closure<T1, T2, T3, T4, T5, T6, T7> CreateClosure<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2,
          T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
      {
        return new Closure<T1, T2, T3, T4, T5, T6, T7>(v1, v2, v3, v4, v5, v6, v7);
      }

      public static Closure<T1, T2, T3, T4, T5, T6, T7, T8> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8>(
          T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
      {
        return new Closure<T1, T2, T3, T4, T5, T6, T7, T8>(v1, v2, v3, v4, v5, v6, v7, v8);
      }

      public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
          T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
      {
        return new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(v1, v2, v3, v4, v5, v6, v7, v8, v9);
      }

      public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
          T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
      {
        return new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10);
      }
    }

    internal sealed class Closure<T1>
    {
      public T1 V1;

      public Closure(T1 v1)
      {
        V1 = v1;
      }
    }

    internal sealed class Closure<T1, T2>
    {
      public T1 V1;
      public T2 V2;

      public Closure(T1 v1, T2 v2)
      {
        V1 = v1;
        V2 = v2;
      }
    }

    internal sealed class Closure<T1, T2, T3>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;

      public Closure(T1 v1, T2 v2, T3 v3)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4, T5>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;
      public T5 V5;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
        V5 = v5;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4, T5, T6>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;
      public T5 V5;
      public T6 V6;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
        V5 = v5;
        V6 = v6;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;
      public T5 V5;
      public T6 V6;
      public T7 V7;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
        V5 = v5;
        V6 = v6;
        V7 = v7;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;
      public T5 V5;
      public T6 V6;
      public T7 V7;
      public T8 V8;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
        V5 = v5;
        V6 = v6;
        V7 = v7;
        V8 = v8;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;
      public T5 V5;
      public T6 V6;
      public T7 V7;
      public T8 V8;
      public T9 V9;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
        V5 = v5;
        V6 = v6;
        V7 = v7;
        V8 = v8;
        V9 = v9;
      }
    }

    internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
      public T1 V1;
      public T2 V2;
      public T3 V3;
      public T4 V4;
      public T5 V5;
      public T6 V6;
      public T7 V7;
      public T8 V8;
      public T9 V9;
      public T10 V10;

      public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
      {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
        V5 = v5;
        V6 = v6;
        V7 = v7;
        V8 = v8;
        V9 = v9;
        V10 = v10;
      }
    }

    internal sealed class ArrayClosure
    {
      public readonly object[] Constants;

      public static FieldInfo ArrayField = typeof(ArrayClosure)
#if NET40
          .GetTypeDeclaredFields()
#else
          .GetTypeInfo().DeclaredFields
#endif
          .GetFirst(f => !f.IsStatic);
      public static ConstructorInfo Constructor = typeof(ArrayClosure)
#if NET40
          .GetTypeDeclaredConstructors()
#else
          .GetTypeInfo().DeclaredConstructors
#endif
          .GetFirst();

      public ArrayClosure(object[] constants)
      {
        Constants = constants;
      }
    }

  }
}
