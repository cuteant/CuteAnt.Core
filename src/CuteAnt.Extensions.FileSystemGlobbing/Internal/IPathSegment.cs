// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace CuteAnt.Extensions.FileSystemGlobbing.Internal
{
    public interface IPathSegment
    {
        bool CanProduceStem { get; }

        bool Match(string value);
    }
}