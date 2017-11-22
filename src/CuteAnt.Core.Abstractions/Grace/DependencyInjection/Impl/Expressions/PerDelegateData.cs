using System.Collections.Generic;
using Grace.Data.Immutable;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Data that is per delegate</summary>
  public class PerDelegateData : IDataPerDelegate
  {
    private ImmutableHashTree<object, object> _data = ImmutableHashTree<object, object>.Empty;

    /// <summary>Keys for data</summary>
    public IEnumerable<object> Keys => _data.Keys;

    /// <summary>Values for data</summary>
    public IEnumerable<object> Values => _data.Values;

    /// <summary>Enumeration of all the key value pairs</summary>
    public IEnumerable<KeyValuePair<object, object>> KeyValuePairs => _data;

    /// <summary>Extra data associated with the injection request.</summary>
    /// <param name="key">key of the data object to get</param>
    /// <returns>data value</returns>
    public object GetExtraData(object key) => _data.GetValueOrDefault(key);

    /// <summary>Sets extra data on the injection context</summary>
    /// <param name="key">object name</param>
    /// <param name="newValue">new object value</param>
    /// <param name="replaceIfExists">replace value if key exists</param>
    /// <returns>the final value of key</returns>
    public object SetExtraData(object key, object newValue, bool replaceIfExists = true)
        => ImmutableHashTree.ThreadSafeAdd(ref _data, key, newValue, replaceIfExists);
  }
}