﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information

#if FEATURE_NETNATIVE
using System.Collections;
using System.ServiceModel;
using System.Text;

namespace System.Security.Authentication.ExtendedProtection
{
    /// <summary>
    /// This class contains the necessary settings for specifying how Extended Protection 
    /// should behave. Use one of the Build* methods to create an instance of this type.
    /// </summary>
    public class ExtendedProtectionPolicy
    {
        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement)
        {
            PolicyEnforcement = policyEnforcement;
        }

        public PolicyEnforcement PolicyEnforcement { get; private set; }

        public ProtectionScenario ProtectionScenario
        {
            get { return ProtectionScenario.TransportSelected; }
        }

        public ChannelBinding CustomChannelBinding
        {
            get { return null; }
        }
        public static bool OSSupportsExtendedProtection
        {
            get
            {
                return false;
            }
        }
    }

    public enum PolicyEnforcement
    {
        Never,
        WhenSupported,
        Always
    }

    public enum ProtectionScenario
    {
        TransportSelected,
        TrustedProxy
    }
}
#endif // FEATURE_NETNATIVE
