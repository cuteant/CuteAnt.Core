﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.Extensions.FileProviders
{
    internal class NotFoundDirectoryContents : IDirectoryContents
    {
        public NotFoundDirectoryContents()
        {
        }

        public bool Exists
        {
            get { return false; }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return Enumerable.Empty<IFileInfo>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}