﻿using System;
using System.Collections.Generic;
using Grace.Data.Immutable;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Represents a scope that can be resolved from but doesn't allow exports to be registered in
  /// Note: This is the recommend scope for "per request" scenarios</summary>
  public class LifetimeScope : BaseExportLocatorScope, IExportLocatorScope
  {
    private readonly IInjectionScope _injectionScope;

    /// <summary>Default Constructor</summary>
    /// <param name="parent">parent for scope</param>
    /// <param name="injectionScope"></param>
    /// <param name="name">name of scope</param>
    /// <param name="activationDelegates">activation delegate cache</param>
    public LifetimeScope(IExportLocatorScope parent, IInjectionScope injectionScope, string name, ImmutableHashTree<Type, ActivationStrategyDelegate>[] activationDelegates)
      : base(parent, name, activationDelegates) => _injectionScope = injectionScope;

    /// <summary>Create as a new IExportLocate scope</summary>
    /// <param name="scopeName">scope name</param>
    /// <returns>new scope</returns>
    public IExportLocatorScope BeginLifetimeScope(string scopeName = "") => new LifetimeScope(this, _injectionScope, scopeName, ActivationDelegates);

    /// <summary>Can Locator type</summary>
    /// <param name="type">type to locate</param>
    /// <param name="consider"></param>
    /// <param name="key">key to use while locating</param>
    /// <returns></returns>
    public bool CanLocate(Type type, ActivationStrategyFilter consider = null, object key = null) => _injectionScope.CanLocate(type, consider, key);

    /// <summary>Create injection context</summary>
    /// <param name="extraData">extra data</param>
    /// <returns>injection context</returns>
    public IInjectionContext CreateContext(object extraData = null) => _injectionScope.CreateContext(extraData);

    /// <summary>Locate a specific type</summary>
    /// <param name="type">type to locate</param>
    /// <returns>located instance</returns>
    public object Locate(Type type)
    {
      var hashCode = type.GetHashCode();

      var func = ActivationDelegates[hashCode & ArrayLengthMinusOne].GetValueOrDefault(type, hashCode);

      return func != null ?
             func(this, this, null) :
             LocateFromParent(type, null, null, null, allowNull: false, isDynamic: false);
    }

    /// <summary>Locate type or return default value</summary>
    /// <param name="type"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public object LocateOrDefault(Type type, object defaultValue)
    {
      var hashCode = type.GetHashCode();

      var func = ActivationDelegates[hashCode & ArrayLengthMinusOne].GetValueOrDefault(type, hashCode);

      return func != null ?
             func(this, this, null) :
             LocateFromParent(type, null, null, null, allowNull: true, isDynamic: false) ?? defaultValue;
    }

    /// <summary>Locate type</summary>
    /// <typeparam name="T">type to locate</typeparam>
    /// <returns>located instance</returns>
    public T Locate<T>() => (T)Locate(typeof(T));

    /// <summary>Locate or return default</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public T LocateOrDefault<T>(T defaultValue = default(T)) => (T)LocateOrDefault(typeof(T), defaultValue);

    /// <summary>Locate specific type using extra data or key</summary>
    /// <param name="type">type to locate</param>
    /// <param name="extraData">extra data to be used during construction</param>
    /// <param name="consider"></param>
    /// <param name="withKey">key to use for locating type</param>
    /// <param name="isDynamic"></param>
    /// <returns>located instance</returns>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public object Locate(Type type, object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
    {
      if (isDynamic || withKey != null || consider != null)
      {
        return LocateFromParent(type, extraData, consider, withKey, false, isDynamic);
      }

      var hashCode = type.GetHashCode();

      var func = ActivationDelegates[hashCode & ArrayLengthMinusOne].GetValueOrDefault(type, hashCode);

      return func != null ?
             func(this, this, extraData == null ? null : CreateContext(extraData)) :
             LocateFromParent(type, extraData, null, null, allowNull: false, isDynamic: false);
    }

    /// <summary>Locate specific type using extra data or key</summary>
    /// <typeparam name="T">type to locate</typeparam>
    /// <param name="extraData">extra data</param>
    /// <param name="consider">filter out different strategies</param>
    /// <param name="withKey">key to use during construction</param>
    /// <param name="isDynamic">bypass the cache and look at all possible</param>
    /// <returns>located instance</returns>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public T Locate<T>(object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
        => (T)Locate(typeof(T), extraData, consider, withKey, isDynamic);

    /// <summary>Locate all instances of a specific type</summary>
    /// <param name="type">type ot locate</param>
    /// <param name="extraData">extra data to be used while locating</param>
    /// <param name="consider">strategy filter</param>
    /// <param name="comparer">comparer to use to sort collection</param>
    /// <returns>list of objects</returns>
    public List<object> LocateAll(Type type, object extraData = null, ActivationStrategyFilter consider = null, IComparer<object> comparer = null)
    {
      var context = _injectionScope.CreateContext(extraData);

      return _injectionScope.InternalLocateAll(this, this, type, context, consider, comparer);
    }

    /// <summary>Locate all of a specific type</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">type to locate, can be null</param>
    /// <param name="extraData">extra data to use during locate</param>
    /// <param name="consider">filter for strategies</param>
    /// <param name="comparer">comparer</param>
    /// <returns>list of all T</returns>
    public List<T> LocateAll<T>(Type type = null, object extraData = null, ActivationStrategyFilter consider = null, IComparer<T> comparer = null)
        => _injectionScope.InternalLocateAll(this, this, type ?? typeof(T), extraData, consider, comparer);

    /// <summary>Try to locate an export by type</summary>
    /// <typeparam name="T">locate type</typeparam>
    /// <param name="value">out value</param>
    /// <param name="extraData"></param>
    /// <param name="consider"></param>
    /// <param name="withKey"></param>
    /// <param name="isDynamic"></param>
    /// <returns></returns>
    public bool TryLocate<T>(out T value, object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
    {
      if (TryLocate(typeof(T), out object outValue, extraData, consider, withKey, isDynamic))
      {
        value = (T)outValue;

        return true;
      }

      value = default(T);

      return false;
    }

    /// <summary>try to locate a specific type</summary>
    /// <param name="type">type to locate</param>
    /// <param name="value">located value</param>
    /// <param name="extraData">extra data to be used during locate</param>
    /// <param name="consider">filter to use during location</param>
    /// <param name="withKey">key to use during locate</param>
    /// <param name="isDynamic">is the request dynamic</param>
    /// <returns>true if export could be located</returns>
    public bool TryLocate(Type type, out object value, object extraData = null, ActivationStrategyFilter consider = null, object withKey = null, bool isDynamic = false)
    {
      if (!isDynamic && withKey == null && consider == null)
      {
        var hashCode = type.GetHashCode();

        var func = ActivationDelegates[hashCode & ArrayLengthMinusOne].GetValueOrDefault(type, hashCode);

        if (func != null)
        {
          value = func(this, this, extraData == null ? null : CreateContext(extraData));

          return value != null;
        }
      }

      value = LocateFromParent(type, extraData, consider, withKey, true, isDynamic);

      return value != null;
    }

    /// <summary>Locate by name</summary>
    /// <param name="name"></param>
    /// <param name="extraData"></param>
    /// <param name="consider"></param>
    /// <returns></returns>
    public object LocateByName(string name, object extraData = null, ActivationStrategyFilter consider = null)
        => _injectionScope.LocateByNameFromChildScope(this, this, name, extraData, consider, false);

    /// <summary>Locate all by specific name</summary>
    /// <param name="name"></param>
    /// <param name="extraData"></param>
    /// <param name="consider"></param>
    /// <returns></returns>
    public List<object> LocateAllByName(string name, object extraData = null, ActivationStrategyFilter consider = null)
        => _injectionScope.InternalLocateAllByName(this, this, name, extraData, consider);

    /// <summary>Try to locate by name</summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="extraData"></param>
    /// <param name="consider"></param>
    /// <returns></returns>
    public bool TryLocateByName(string name, out object value, object extraData = null, ActivationStrategyFilter consider = null)
    {
      value = _injectionScope.LocateByNameFromChildScope(this, this, name, extraData, consider, true);

      return value != null;
    }

    /// <summary>Locate from a parent scope if it's not in the cache</summary>
    /// <param name="type">type to locate</param>
    /// <param name="extraData">extra data</param>
    /// <param name="consider">filter for strategies</param>
    /// <param name="key">key to use for locate</param>
    /// <param name="allowNull">is null allowed</param>
    /// <param name="isDynamic">is the request dynamic</param>
    /// <returns></returns>
    protected virtual object LocateFromParent(Type type, object extraData, ActivationStrategyFilter consider, object key, bool allowNull, bool isDynamic)
        => _injectionScope.LocateFromChildScope(this, this, type, extraData, consider, key, allowNull, isDynamic);
  }
}