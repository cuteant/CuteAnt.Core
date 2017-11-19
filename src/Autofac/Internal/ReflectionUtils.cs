using System;
using System.Reflection;

namespace Autofac
{
  internal static class ReflectionUtils
  {
    public static Assembly AssemblyX(this Type type)
    {
#if NET40
      return type.Assembly;
#else
      return type.GetTypeInfo().Assembly;
#endif
    }
  }
}
