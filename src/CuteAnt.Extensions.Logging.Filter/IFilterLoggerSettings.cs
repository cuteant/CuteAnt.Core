// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CuteAnt.Extensions.Primitives;

namespace CuteAnt.Extensions.Logging
{
    /// <summary>
    /// Filter settings for messages logged by an <see cref="ILogger"/>.
    /// </summary>
    public interface IFilterLoggerSettings
    {
        IChangeToken ChangeToken { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        IFilterLoggerSettings Reload();
    }
}