using System;
using System.Collections.Generic;
using System.IO;
using CuteAnt.ApplicationParts;

namespace CuteAnt.Hosting
{
  /// <summary>Extensions for working with <see cref="HostBuilderContext"/>.</summary>
  public static class HostBuilderContextExtensions
  {
    private static readonly object s_dirEnumArgsKey = new object();

    /// <summary>Returns the <see cref="ApplicationPartManager"/> for the provided context.</summary>
    /// <param name="context">The context.</param>
    /// <returns>The <see cref="ApplicationPartManager"/> belonging to the provided context.</returns>
    public static ApplicationPartManager GetApplicationPartManager(this HostBuilderContext context)
        => ApplicationPartManagerExtensions.GetApplicationPartManager(context.Properties);

    /// <summary>GetDictionaryEnumArgs</summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Dictionary<string, SearchOption> GetDictionaryEnumArgs(this HostBuilderContext context) => GetDictionaryEnumArgs(context.Properties);
    internal static Dictionary<string, SearchOption> GetDictionaryEnumArgs(IDictionary<object, object> properties)
    {
      Dictionary<string, SearchOption> result;
      if (properties.TryGetValue(s_dirEnumArgsKey, out var value))
      {
        result = value as Dictionary<string, SearchOption>;
        if (result == null) throw new InvalidOperationException($"The HostDictionaryEnumArgs value is of the wrong type {value.GetType()}. It should be {nameof(Dictionary<string, SearchOption>)}");
      }
      else
      {
        properties[s_dirEnumArgsKey] = result = new Dictionary<string, SearchOption>(StringComparer.Ordinal)
        {
          [AppDomain.CurrentDomain.BaseDirectory] = SearchOption.TopDirectoryOnly
        };
      }

      return result;
    }
  }
}
