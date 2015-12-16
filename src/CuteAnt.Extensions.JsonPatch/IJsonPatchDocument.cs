// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CuteAnt.Extensions.JsonPatch.Operations;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace CuteAnt.Extensions.JsonPatch
{
    public interface IJsonPatchDocument
    {
        IContractResolver ContractResolver { get; set; }

        IList<Operation> GetOperations();
    }
}