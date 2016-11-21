﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Extensions.DependencyInjection.ServiceLookup
{
    internal class ConstantCallSite : IServiceCallSite
    {
        internal object DefaultValue { get; }

        public ConstantCallSite(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}
