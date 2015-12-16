﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Extensions.JsonPatch.Helpers
{
    internal class ConversionResult
    {
        public bool CanBeConverted { get; private set; }
        public object ConvertedInstance { get; private set; }

        public ConversionResult(bool canBeConverted, object convertedInstance)
        {
            CanBeConverted = canBeConverted;
            ConvertedInstance = convertedInstance;
        }
    }
}