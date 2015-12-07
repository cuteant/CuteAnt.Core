//-----------------------------------------------------------------------
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

namespace System.IdentityModel.Tokens
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines constants needed from WS-Security 1.0.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Suppressed for private or internal fields.")]
    internal static class WSSecurityConstantsInternal
    {
#pragma warning disable 1591
        public const string Namespace           = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string Prefix              = "wsse";
        public const string Base64EncodingType  = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
        public const string Base64Binary        = "Base64Binary";
        public const string Base64BinaryLower   = "base64Binary";

        public static class Attributes
        {
            public const string ValueType           = "ValueType";
            public const string EncodingType        = "EncodingType";
            public const string EncodingTypeLower   = "encodingType";
        }

        public static class Elements
        {
            public const string BinarySecurityToken    = "BinarySecurityToken";
        }
#pragma warning restore 1591
    }
}
