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

namespace System.IdentityModel.Tokens
{
    using Microsoft.IdentityModel;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.Xml;

    /// <summary>
    /// <see cref="NamedKeyIssuerTokenResolver"/> represents a collection of named sets of <see cref="SecurityKey"/>(s) that can be matched by a
    /// <see cref="NamedKeySecurityKeyIdentifierClause"/> and return a <see cref="NamedKeySecurityToken"/> that contains <see cref="SecurityKey"/>(s).
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Suppressed for private or internal fields.")]
    public class NamedKeyIssuerTokenResolver : IssuerTokenResolver
    {
        private IDictionary<string, IList<SecurityKey>> keys;
        private List<XmlNode> unprocessedNodes = new List<XmlNode>();
        private IssuerTokenResolver issuerTokenResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedKeyIssuerTokenResolver"/> class. 
        /// </summary>
        public NamedKeyIssuerTokenResolver()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedKeyIssuerTokenResolver"/> class. 
        /// Populates this instance with a named collection of <see cref="SecurityKey"/>(s) and an optional <see cref="SecurityTokenResolver"/> that will be called when a 
        /// <see cref="SecurityKeyIdentifier"/> or <see cref="SecurityKeyIdentifierClause"/> cannot be resolved.
        /// </summary>
        /// <param name="keys">
        /// A named collection of <see cref="SecurityKey"/>(s).
        /// </param>
        /// <param name="innerTokenResolver">
        /// A <see cref="IssuerTokenResolver"/> to call when resolving fails, before calling base.
        /// </param>
        /// <remarks>
        /// if 'keys' is null an empty collection will be created. A named collection of <see cref="SecurityKey"/>(s) can be added by accessing the property <see cref="SecurityKeys"/>.
        /// </remarks>
        public NamedKeyIssuerTokenResolver(IDictionary<string, IList<SecurityKey>> keys = null, IssuerTokenResolver innerTokenResolver = null)
        {
            if (keys == null)
            {
                this.keys = new Dictionary<string, IList<SecurityKey>>();
            }
            else
            {
                this.keys = keys;
            }

            this.issuerTokenResolver = innerTokenResolver;
        }

        /// <summary>
        /// Gets the named collection of <see cref="SecurityKey"/>(s).
        /// </summary>
        public IDictionary<string, IList<SecurityKey>> SecurityKeys
        {
            get { return this.keys; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenResolver"/> to call when <see cref="SecurityKeyIdentifier"/> or <see cref="SecurityKeyIdentifierClause"/> fails to resolve, before calling base.
        /// </summary>
        /// <exception cref="ArgumentNullException">'value' is null.</exception>
        /// <exception cref="ArgumentException">'object.ReferenceEquals( this, value)' is true.</exception>
        public IssuerTokenResolver IssuerTokenResolver
        {
            get
            {
                return this.issuerTokenResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (object.ReferenceEquals(this, value))
                {
                    throw new ArgumentException(ErrorMessages.IDX10704);
                }

                this.issuerTokenResolver = value;
            }
        }

        /// <summary>
        /// Gets the unprocessed <see cref="XmlNode"/>(s) from <see cref="LoadCustomConfiguration"/>.
        /// </summary>
        /// <remarks><see cref="LoadCustomConfiguration"/> processes only <see cref="XmlElement"/>(s) that have the <see cref="XmlElement.LocalName"/> == 'securityKey'. Unprocessed <see cref="XmlNode"/>(s) are accessible here.</remarks>
        public IList<XmlNode> UnprocessedXmlNodes
        {
            get { return this.unprocessedNodes; }
        }

        /// <summary>
        /// Populates the <see cref="SecurityKeys"/> from xml.
        /// </summary>
        /// <param name="nodeList">xml for processing.</param>
        /// <exception cref="ArgumentNullException">'nodeList' is null.</exception>
        /// <remarks>Only <see cref="XmlNode"/>(s) with <see cref="XmlElement.LocalName"/> == 'securityKey' will be processed. Unprocessed nodes will added to a list and can be accessed using the <see cref="UnprocessedXmlNodes"/> property.</remarks>
        public override void LoadCustomConfiguration(XmlNodeList nodeList)
        {
            if (nodeList == null)
            {
                throw new ArgumentNullException("nodeList");
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlElement element = nodeList[i] as XmlElement;

                if (element != null)
                {
                    if (string.Equals(element.LocalName, JwtConfigurationStrings.Elements.SecurityKey, StringComparison.Ordinal))
                    {
                        this.ReadSecurityKey(element);
                    }
                    else
                    {
                        this.unprocessedNodes.Add(nodeList[i]);
                    }
                }
                else
                {
                    this.unprocessedNodes.Add(nodeList[i]);
                }
            }
        }

        /// <summary>
        /// When processing xml in <see cref="LoadCustomConfiguration"/> each <see cref="XmlElement"/> that has <see cref="XmlElement.LocalName"/> = "securityKey' is passed here for processing.
        /// </summary>
        /// <param name="element">contains xml to map to a named <see cref="SecurityKey"/>.</param>
        /// <remarks>
        /// <para>A single <see cref="XmlElement"/> is expected with up to three attributes: {'expected values'}.</para>
        /// <para>&lt;securityKey</para>
        /// <para>&#160;&#160;&#160;&#160;symmetricKey {required}</para>
        /// <para>&#160;&#160;&#160;&#160;name         {required}</para>
        /// <para>&#160;&#160;&#160;&#160;EncodingType or encodingType {optional}</para>
        /// <para>></para>
        /// <para>&lt;/securityKey></para>
        /// <para>If "EncodingType' type is specified only:</para>
        /// <para>&#160;&#160;&#160;&#160;'http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary'</para>
        /// <para>&#160;&#160;&#160;&#160;'Base64Binary'</para>
        /// <para>&#160;&#160;&#160;&#160;'base64Binary'</para>
        /// <para>are allowed and have the same meaning.</para>
        /// <para>When a symmetricKey is found, Convert.FromBase64String( value ) is applied to create the key.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">'element' is null.</exception>
        /// <exception cref="ConfigurationErrorsException">attribute 'symmetricKey' is not found.</exception>
        /// <exception cref="ConfigurationErrorsException">value of 'symmetricKey' is empty or whitespace.</exception>
        /// <exception cref="ConfigurationErrorsException">attribute 'name' is not found.</exception>
        /// <exception cref="ConfigurationErrorsException">value of 'name' is empty or whitespace.</exception>
        /// <exception cref="ConfigurationErrorsException">value of 'encodingType' is not valid.</exception>
        protected virtual void ReadSecurityKey(XmlElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            string key = null;
            string name = null;

            XmlNode attributeNode = element.Attributes.GetNamedItem(JwtConfigurationStrings.Attributes.SymmetricKey);
            if (attributeNode == null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX13000, element.OuterXml));
            }

            key = attributeNode.Value;
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX13002, JwtConfigurationStrings.Attributes.SymmetricKey, element.OuterXml));
            }

            attributeNode = element.Attributes.GetNamedItem(JwtConfigurationStrings.Attributes.Name);
            if (attributeNode == null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX13001, element.OuterXml));
            }

            name = attributeNode.Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX13002, JwtConfigurationStrings.Attributes.Name, element.OuterXml));
            }

            attributeNode = element.Attributes.GetNamedItem(WSSecurityConstantsInternal.Attributes.EncodingType);
            if (attributeNode == null)
            {
                attributeNode = element.Attributes.GetNamedItem(WSSecurityConstantsInternal.Attributes.EncodingTypeLower);
            }

            if (attributeNode != null)
            {
                if (!StringComparer.Ordinal.Equals(attributeNode.Value, WSSecurityConstantsInternal.Base64BinaryLower)
                && !StringComparer.Ordinal.Equals(attributeNode.Value, WSSecurityConstantsInternal.Base64EncodingType)
                && !StringComparer.Ordinal.Equals(attributeNode.Value, WSSecurityConstantsInternal.Base64Binary))
                {
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX13003, WSSecurityConstantsInternal.Base64BinaryLower, WSSecurityConstantsInternal.Base64Binary, WSSecurityConstantsInternal.Base64EncodingType, attributeNode.Value, element.OuterXml));
                }
            }

            byte[] keybytes = Convert.FromBase64String(key);
            IList<SecurityKey> resolvedKeys = null;
            if (!this.keys.TryGetValue(name, out resolvedKeys))
            {
                resolvedKeys = new List<SecurityKey>();
                this.keys.Add(name, resolvedKeys);
            }

            resolvedKeys.Add(new InMemorySymmetricSecurityKey(keybytes));
        }

        /// <summary>
        /// Finds the first <see cref="SecurityKey"/> in a named collection that match the <see cref="SecurityKeyIdentifierClause"/>.
        /// </summary>
        /// <param name="keyIdentifierClause">
        /// The <see cref="SecurityKeyIdentifierClause"/> to resolve to a <see cref="SecurityKey"/>
        /// </param>
        /// <param name="key">
        /// The resolved <see cref="SecurityKey"/>.
        /// </param>
        /// <remarks>
        /// If there is no match, then <see cref="IssuerTokenResolver"/> and 'base' are called in order.
        /// </remarks>
        /// <returns>
        /// true if key resolved, false otherwise.
        /// </returns>
        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            if (keyIdentifierClause == null)
            {
                throw new ArgumentNullException("keyIdentifierClause");
            }

            key = null;
            NamedKeySecurityKeyIdentifierClause namedKeyIdentifierClause = keyIdentifierClause as NamedKeySecurityKeyIdentifierClause;
            if (namedKeyIdentifierClause != null)
            {
                IList<SecurityKey> resolvedKeys = null;
                if (this.keys.TryGetValue(namedKeyIdentifierClause.Name, out resolvedKeys))
                {
                    key = resolvedKeys[0];
                    return true;
                }
            }

            if (IssuerTokenResolver != null && IssuerTokenResolver.TryResolveSecurityKey(keyIdentifierClause, out key))
            {
                return true;
            }

            return base.TryResolveSecurityKeyCore(keyIdentifierClause, out key);
        }

        /// <summary>
        /// Finds a named collection of <see cref="SecurityKey"/>(s) that match the <see cref="SecurityKeyIdentifier"/> and returns a <see cref="NamedKeySecurityToken"/> that contains the <see cref="SecurityKey"/>(s).
        /// </summary>
        /// <param name="keyIdentifier">The <see cref="SecurityKeyIdentifier"/> to resolve to a <see cref="SecurityToken"/></param>
        /// <param name="token">The resolved <see cref="SecurityToken"/>.</param>
        /// <remarks>
        /// <para>
        /// A <see cref="SecurityKeyIdentifier"/> can contain multiple <see cref="SecurityKeyIdentifierClause"/>(s). This method will return the named collection that matches the first <see cref="SecurityKeyIdentifierClause"/>
        /// </para>
        /// <para>
        /// If there is no match, then <see cref="IssuerTokenResolver"/> and 'base' are called in order.
        /// </para>
        /// </remarks>
        /// <returns>
        /// true is the keyIdentifier is resolved, false otherwise.
        /// </returns>
        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            if (keyIdentifier == null)
            {
                throw new ArgumentNullException("keyIdentifier");
            }

            token = null;
            foreach (SecurityKeyIdentifierClause clause in keyIdentifier)
            {
                if (null == clause)
                {
                    continue;
                }

                NamedKeySecurityKeyIdentifierClause namedKeyIdentifierClause = clause as NamedKeySecurityKeyIdentifierClause;
                if (namedKeyIdentifierClause != null)
                {
                    IList<SecurityKey> resolvedKeys = null;
                    if (this.keys.TryGetValue(namedKeyIdentifierClause.Name, out resolvedKeys))
                    {
                        token = new NamedKeySecurityToken(namedKeyIdentifierClause.Name, namedKeyIdentifierClause.Id, resolvedKeys);
                        return true;
                    }
                }
            }

            if (IssuerTokenResolver != null && IssuerTokenResolver.TryResolveToken(keyIdentifier, out token))
            {
                return true;
            }

            return base.TryResolveTokenCore(keyIdentifier, out token);
        }

        /// <summary>
        /// Finds a named collection of <see cref="SecurityKey"/>(s) that match the <see cref="SecurityKeyIdentifierClause"/> and returns a <see cref="NamedKeySecurityToken"/> that contains the <see cref="SecurityKey"/>(s).
        /// </summary>
        /// <param name="keyIdentifierClause">The <see cref="SecurityKeyIdentifier"/> to resolve to a <see cref="SecurityToken"/></param>
        /// <param name="token">The resolved <see cref="SecurityToken"/>.</param>
        /// <remarks>If there is no match, then <see cref="IssuerTokenResolver"/> and 'base' are called in order.</remarks>
        /// <returns>true if token was resolved.</returns>
        /// <exception cref="ArgumentNullException">if 'keyIdentifierClause' is null.</exception>
        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            if (keyIdentifierClause == null)
                throw new ArgumentNullException("keyIdentifierClause");

            token = null;
            NamedKeySecurityKeyIdentifierClause namedKeyIdentifierClause = keyIdentifierClause as NamedKeySecurityKeyIdentifierClause;
            if (namedKeyIdentifierClause != null)
            {
                IList<SecurityKey> resolvedKeys = null;
                if (this.keys.TryGetValue(namedKeyIdentifierClause.Name, out resolvedKeys))
                {
                    token = new NamedKeySecurityToken(namedKeyIdentifierClause.Name, namedKeyIdentifierClause.Id, resolvedKeys);
                    return true;
                }
            }

            if (IssuerTokenResolver != null && IssuerTokenResolver.TryResolveToken(keyIdentifierClause, out token))
            {
                return true;
            }

            return base.TryResolveTokenCore(keyIdentifierClause, out token);
        }
    }
}
