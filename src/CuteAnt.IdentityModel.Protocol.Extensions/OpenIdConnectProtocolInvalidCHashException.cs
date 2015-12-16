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
using System.Runtime.Serialization;

namespace Microsoft.IdentityModel.Protocols
{
    /// <summary>
    /// This exception is thrown when an OpenIdConnect protocol handler encounters an invalid chash.
    /// </summary>
    [Serializable]
    public class OpenIdConnectProtocolInvalidCHashException : OpenIdConnectProtocolException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdConnectProtocolInvalidCHashException"/> class.
        /// </summary>
        public OpenIdConnectProtocolInvalidCHashException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdConnectProtocolInvalidCHashException"/> class.
        /// </summary>
        /// <param name="message">Addtional information to be included in the exception and displayed to user.</param>
        public OpenIdConnectProtocolInvalidCHashException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdConnectProtocolInvalidCHashException"/> class.
        /// </summary>
        /// <param name="message">Addtional information to be included in the exception and displayed to user.</param>
        /// <param name="innerException">A <see cref="Exception"/> that represents the root cause of the exception.</param>
        public OpenIdConnectProtocolInvalidCHashException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdConnectProtocolInvalidCHashException"/> class.
        /// </summary>
        /// <param name="info">the <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected OpenIdConnectProtocolInvalidCHashException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}