// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using CuteAnt.Extensions.Configuration;
using CuteAnt.Extensions.Primitives;

namespace CuteAnt.Extensions.Options
{
    public class ConfigurationChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    {
        private IConfiguration _config;

        public ConfigurationChangeTokenSource(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            _config = config;
        } 

        public IChangeToken GetChangeToken()
        {
            return _config.GetReloadToken();
        }
    }
}