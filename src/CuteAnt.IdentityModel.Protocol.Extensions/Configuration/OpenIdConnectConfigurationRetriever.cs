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
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Protocols
{
    /// <summary>
    ///  Retrieves a populated <see cref="OpenIdConnectConfiguration"/> given an address.
    /// </summary>
    public class OpenIdConnectConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {

        /// <summary>
        /// Retrieves a populated <see cref="OpenIdConnectConfiguration"/> given an address.
        /// </summary>
        /// <param name="address">address of the discovery document.</param>
        /// <param name="cancel"><see cref="CancellationToken"/>.</param>
        /// <returns>A populated <see cref="OpenIdConnectConfiguration"/> instance.</returns>
        public static Task<OpenIdConnectConfiguration> GetAsync(string address, CancellationToken cancel)
        {
            return GetAsync(address, new GenericDocumentRetriever(), cancel);
        }

        /// <summary>
        /// Retrieves a populated <see cref="OpenIdConnectConfiguration"/> given an address and an <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="address">address of the discovery document.</param>
        /// <param name="httpClient">the <see cref="HttpClient"/> to use to read the discovery document.</param>
        /// <param name="cancel"><see cref="CancellationToken"/>.</param>
        /// <returns>A populated <see cref="OpenIdConnectConfiguration"/> instance.</returns>
        public static Task<OpenIdConnectConfiguration> GetAsync(string address, HttpClient httpClient, CancellationToken cancel)
        {
            return GetAsync(address, new HttpDocumentRetriever(httpClient), cancel);
        }

        Task<OpenIdConnectConfiguration> IConfigurationRetriever<OpenIdConnectConfiguration>.GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            return GetAsync(address, retriever, cancel);
        }


        /// <summary>
        /// Retrieves a populated <see cref="OpenIdConnectConfiguration"/> given an address and an <see cref="IDocumentRetriever"/>.
        /// </summary>
        /// <param name="address">address of the discovery document.</param>
        /// <param name="retriever">the <see cref="IDocumentRetriever"/> to use to read the discovery document</param>
        /// <param name="cancel"><see cref="CancellationToken"/>.</param>
        /// <returns>A populated <see cref="OpenIdConnectConfiguration"/> instance.</returns>
        public static async Task<OpenIdConnectConfiguration> GetAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (retriever == null)
            {
                throw new ArgumentNullException(nameof(retriever));
            }

            string doc = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
            OpenIdConnectConfiguration openIdConnectConfiguration = new OpenIdConnectConfiguration(doc);
            if (!string.IsNullOrEmpty(openIdConnectConfiguration.JwksUri))
            {
                string keys = await retriever.GetDocumentAsync(openIdConnectConfiguration.JwksUri, cancel).ConfigureAwait(false);
                openIdConnectConfiguration.JsonWebKeySet = new JsonWebKeySet(keys);
                foreach (SecurityToken token in openIdConnectConfiguration.JsonWebKeySet.GetSigningTokens())
                {
                    openIdConnectConfiguration.SigningTokens.Add(token);
                }
            }

            return openIdConnectConfiguration;
        }
    }
}
