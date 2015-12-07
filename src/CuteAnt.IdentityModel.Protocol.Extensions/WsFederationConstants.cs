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

namespace Microsoft.IdentityModel.Protocols
{

    /// <summary>
    /// Constants for WsFederation actions.
    /// </summary>
    public static class WsFederationActions
    {
        #pragma warning disable 1591

        public const string Attribute = "wattr1.0";
        public const string Pseudonym = "wpseudo1.0";
        public const string SignIn = "wsignin1.0";
        public const string SignOut = "wsignout1.0";
        public const string SignOutCleanup = "wsignoutcleanup1.0";
        
        #pragma warning restore 1591
    }

    /// <summary>
    /// Constants defined for WsFederation.
    /// </summary>
    public static class WsFederationConstants
    {        
        #pragma warning disable 1591

        public const string Namespace = "http://docs.oasis-open.org/wsfed/federation/200706";

        #pragma warning restore 1591
    }

    /// <summary>
    /// Constants for WsFederation Fault codes.
    /// </summary>
    public static class WsFederationFaultCodes
    {
        #pragma warning disable 1591

        public const string AlreadySignedIn = "AlreadySignedIn";
        public const string BadRequest = "BadRequest";
        public const string IssuerNameNotSupported = "IssuerNameNotSupported";
        public const string NeedFresherCredentials = "NeedFresherCredentials";
        public const string NoMatchInScope = "NoMatchInScope";
        public const string NoPseudonymInScope = "NoPseudonymInScope";
        public const string NotSignedIn = "NotSignedIn";
        public const string RstParameterNotAccepted = "RstParameterNotAccepted";
        public const string SpecificPolicy = "SpecificPolicy";
        public const string UnsupportedClaimsDialect = "UnsupportedClaimsDialect";
        public const string UnsupportedEncoding = "UnsupportedEncoding";

        #pragma warning restore 1591
    }

    /// <summary>
    /// Defines the WsFederation Constants
    /// </summary>
    public static class WsFederationParameterNames
    {
        #pragma warning disable 1591

        public const string Wa = "wa";
        public const string Wattr = "wattr";
        public const string Wattrptr = "wattrptr";
        public const string Wauth = "wauth";
        public const string Wct = "wct";
        public const string Wctx = "wctx";
        public const string Wencoding = "wencoding";
        public const string Wfed = "wfed";
        public const string Wfresh = "wfresh";
        public const string Whr = "whr";
        public const string Wp = "wp";
        public const string Wpseudo = "wpseudo";
        public const string Wpseudoptr = "wpseudoptr";
        public const string Wreply = "wreply";
        public const string Wreq = "wreq";
        public const string Wreqptr = "wreqptr";
        public const string Wres = "wres";
        public const string Wresult = "wresult";
        public const string Wresultptr = "wresultptr";
        public const string Wtrealm = "wtrealm";
        
        #pragma warning restore 1591
    }
}
 