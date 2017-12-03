using System;
using System.Reflection;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
  partial class RuntimeTypeNameFormatter
  {
    /// <summary>SystemAssembly</summary>
    public static readonly Assembly SystemAssembly = typeof(int).GetTypeInfo().Assembly;

    private static readonly char[] SimpleNameTerminators = { '`', '*', '[', '&' };
    private const char c_keyDelimiter = ':';

    private static readonly CachedReadConcurrentDictionary<Type, TypeNameKey> s_asmTypeDefinitionCache =
        new CachedReadConcurrentDictionary<Type, TypeNameKey>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<Type, TypeNameKey> s_getTypeDefinitionFunc = GetTypeDefinitionInternal;

    internal static TypeNameKey GetTypeDefinition(Type type)
        => s_asmTypeDefinitionCache.GetOrAdd(type, s_getTypeDefinitionFunc);

    private static TypeNameKey GetTypeDefinitionInternal(Type type)
    {
      var builder = StringBuilderCache.Acquire();
      Format(builder, type
#if !NET40
          .GetTypeInfo()
#endif
          , isElementType: true);
      var typeName = StringBuilderCache.GetStringAndRelease(builder);

      return new TypeNameKey(GetAssemblyName(type), typeName);
    }

    /// <summary>GetAssemblyName</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetAssemblyName(Type type)
    {
      var asm = type.GetTypeInfo().Assembly;

      // Do not include the assembly name for the system assembly.
      if (SystemAssembly.Equals(asm)) { return null; }
      return asm.GetName().Name;
    }
  }
}
