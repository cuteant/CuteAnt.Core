﻿//-----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Protocols
{
    // Works for c:\, file://, http://, ftp://, etc.
    internal class GenericDocumentRetriever : IDocumentRetriever
    {
        public async Task<string> GetDocumentAsync(string address, CancellationToken cancel)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException("address");
            }
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (CancellationTokenRegistration registration = cancel.Register(() => client.CancelAsync()))
                    {
                        return await client.DownloadStringTaskAsync(address).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Unable to get document from: " + address, ex);
            }
        }
    }
}
