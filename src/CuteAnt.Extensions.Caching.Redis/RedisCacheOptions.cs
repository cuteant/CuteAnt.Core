﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CuteAnt.Extensions.OptionsModel;

namespace CuteAnt.Extensions.Caching.Redis
{
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public string Configuration { get; set; }

        public string InstanceName { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value
        {
            get { return this; }
        }
    }
}