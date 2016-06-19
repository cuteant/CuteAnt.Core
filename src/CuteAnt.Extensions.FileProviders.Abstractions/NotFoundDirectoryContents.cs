﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.Extensions.FileProviders
{
    /// <summary>
    /// Represents a non-existing directory
    /// </summary>
    public class NotFoundDirectoryContents : IDirectoryContents
    {
        public bool Exists => false;

        public IEnumerator<IFileInfo> GetEnumerator() => Enumerable.Empty<IFileInfo>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}