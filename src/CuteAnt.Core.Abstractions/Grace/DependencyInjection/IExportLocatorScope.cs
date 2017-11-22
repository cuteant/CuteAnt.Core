﻿using System;
using Grace.Data;

namespace Grace.DependencyInjection
{
  /// <summary>Export locator scope represents a scope that can be located from InjectionScope and
  /// LifetimeScope implement this interface</summary>
  public interface IExportLocatorScope : ILocatorService, IExtraDataContainer, IDisposalScope
  {
    /// <summary>Parent scope</summary>
    IExportLocatorScope Parent { get; }

    /// <summary>Unique id for each scope</summary>
    Guid ScopeId { get; }

    /// <summary>Name of the scope</summary>
    string ScopeName { get; }

    /// <summary>Gets a named object that can be used for locking</summary>
    /// <param name="lockName">lock name</param>
    /// <returns>lock</returns>
    object GetLockObject(string lockName);

    /// <summary>Create as a new IExportLocate scope</summary>
    /// <param name="scopeName">scope name</param>
    /// <returns>new scope</returns>
    IExportLocatorScope BeginLifetimeScope(string scopeName = "");

    /// <summary>Create injection context</summary>
    /// <param name="extraData">extra data</param>
    /// <returns></returns>
    IInjectionContext CreateContext(object extraData = null);
  }
}