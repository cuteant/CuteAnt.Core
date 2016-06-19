﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CuteAnt.Extensions.Options
{
    /// <summary>
    /// Used for notifications when TOptions instances change.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    public interface IOptionsMonitor<out TOptions>
    {
        /// <summary>
        /// Returns the current TOptions instance.
        /// </summary>
        TOptions CurrentValue { get; }

        /// <summary>
        /// Registers the listener to be called whenever TOptions changes.
        /// </summary>
        /// <param name="listener">The action to be invoked when TOptions has changed.</param>
        /// <returns>An IDisposable which should be disposed to stop listening for changes.</returns>
        IDisposable OnChange(Action<TOptions> listener);
    }
}