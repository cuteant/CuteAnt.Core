// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;

namespace CuteAnt.Extensions.Localization
{
  /// <summary>
  /// Represents a factory that creates <see cref="IStringLocalizer"/> instances.
  /// </summary>
  public interface IStringLocalizerFactory
  {
    #region ## ¿àÖñ ÐÞ¸Ä ##
    ///// <summary>
    ///// Creates an <see cref="IStringLocalizer"/> using the <see cref="System.Reflection.Assembly"/> and
    ///// <see cref="Type.FullName"/> of the specified <see cref="Type"/>.
    ///// </summary>
    ///// <param name="resourceSource">The <see cref="Type"/>.</param>
    ///// <returns>The <see cref="IStringLocalizer"/>.</returns>
    //IStringLocalizer Create(Type resourceSource);
    /// <summary>
    /// Creates an <see cref="IStringLocalizer"/> using the <see cref="System.Reflection.Assembly"/> and
    /// <see cref="Type.FullName"/> of the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="resourceSource">The <see cref="Type"/>.</param>
    /// <param name="applicationName">The application name.</param>
    /// <returns>The <see cref="IStringLocalizer"/>.</returns>
    IStringLocalizer Create(Type resourceSource, string applicationName);
    #endregion

    /// <summary>
    /// Creates an <see cref="IStringLocalizer"/>.
    /// </summary>
    /// <param name="baseName">The base name of the resource to load strings from.</param>
    /// <param name="location">The location to load resources from.</param>
    /// <returns>The <see cref="IStringLocalizer"/>.</returns>
    IStringLocalizer Create(string baseName, string location);
  }
}