// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Extensions.FileSystemGlobbing.Abstractions
{
    public abstract class FileSystemInfoBase
    {
        public abstract string Name { get; }

        public abstract string FullName { get; }

        public abstract DirectoryInfoBase ParentDirectory { get; }
    }
}