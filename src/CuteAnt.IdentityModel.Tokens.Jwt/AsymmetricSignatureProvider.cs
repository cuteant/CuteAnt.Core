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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides signing and verifying operations when working with an <see cref="AsymmetricSecurityKey"/>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Suppressed for private fields.")]
    public class AsymmetricSignatureProvider : SignatureProvider
    {
        private bool disposed;
        private HashAlgorithm hash;
        private AsymmetricSignatureFormatter formatter;
        private AsymmetricSignatureDeformatter deformatter;
        private AsymmetricSecurityKey key;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsymmetricSignatureProvider"/> class used to create and verify signatures.
        /// </summary>
        /// <param name="key">
        /// The <see cref="AsymmetricSecurityKey"/> that will be used for cryptographic operations.
        /// </param>
        /// <param name="algorithm">
        /// The signature algorithm to apply.
        /// </param>
        /// <param name="willCreateSignatures">
        /// If this <see cref="AsymmetricSignatureProvider"/> is required to create signatures then set this to true.
        /// <para>
        /// Creating signatures requires that the <see cref="AsymmetricSecurityKey"/> has access to a private key. 
        /// Verifying signatures (the default), does not require access to the private key.
        /// </para>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 'key' is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 'algorithm' is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 'algorithm' contains only whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// willCreateSignatures is true and <see cref="AsymmetricSecurityKey"/>.KeySize is less than <see cref="SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForSigning"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="AsymmetricSecurityKey"/>.KeySize is less than <see cref="SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying"/>. Note: this is always checked.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSecurityKey.GetHashAlgorithmForSignature"/> throws.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSecurityKey.GetHashAlgorithmForSignature"/> returns null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSecurityKey.GetSignatureFormatter"/> throws.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSecurityKey.GetSignatureFormatter"/> returns null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSecurityKey.GetSignatureDeformatter"/> throws.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSecurityKey.GetSignatureDeformatter"/> returns null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSignatureFormatter.SetHashAlgorithm"/> throws.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Is thrown if the <see cref="AsymmetricSignatureDeformatter.SetHashAlgorithm"/> throws.
        /// </exception>
        public AsymmetricSignatureProvider(AsymmetricSecurityKey key, string algorithm, bool willCreateSignatures = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }

            if (string.IsNullOrWhiteSpace(algorithm))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10002, "algorithm"));
            }

            if (willCreateSignatures)
            {
                if (key.KeySize < SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForSigning)
                {
                    throw new ArgumentOutOfRangeException("key.KeySize", key.KeySize, string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10631, key.GetType(), SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForSigning));
                }
            }

            if (key.KeySize < SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying)
            {
                throw new ArgumentOutOfRangeException("key.KeySize", key.KeySize, string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10630, key.GetType(), SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying));
            }

            this.key = key;
            try
            {
                this.hash = this.key.GetHashAlgorithmForSignature(algorithm);
            }
            catch (Exception ex)
            {
                if (DiagnosticUtility.IsFatal(ex))
                {
                    throw;
                }

                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10618, algorithm, this.key.ToString(), ex), ex);
            }

            if (this.hash == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10611, algorithm, this.key.ToString()));
            }

            if (willCreateSignatures)
            {
                try
                {
                    this.formatter = this.key.GetSignatureFormatter(algorithm);
                    this.formatter.SetHashAlgorithm(this.hash.GetType().ToString());
                }
                catch (Exception ex)
                {
                    if (DiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }

                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10614, algorithm, this.key.ToString(), ex), ex);
                }

                if (this.formatter == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10615, algorithm, this.key.ToString()));
                }
            }

            try
            {
                this.deformatter = this.key.GetSignatureDeformatter(algorithm);
                this.deformatter.SetHashAlgorithm(this.hash.GetType().ToString());
            }
            catch (Exception ex)
            {
                if (DiagnosticUtility.IsFatal(ex))
                {
                    throw;
                }

                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10616, algorithm, this.key.ToString(), ex), ex);
            }

            if (this.deformatter == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.IDX10617, algorithm, this.key.ToString()));
            }
        }

        /// <summary>
        /// Produces a signature over the 'input' using the <see cref="AsymmetricSecurityKey"/> and algorithm passed to <see cref="AsymmetricSignatureProvider( AsymmetricSecurityKey, string, bool )"/>.
        /// </summary>
        /// <param name="input">bytes to be signed.</param>
        /// <returns>a signature over the input.</returns>
        /// <exception cref="ArgumentNullException">'input' is null. </exception>
        /// <exception cref="ArgumentException">'input.Length' == 0. </exception>
        /// <exception cref="ObjectDisposedException">if <see cref="AsymmetricSignatureProvider.Dispose(bool)"/> has been called. </exception>
        /// <exception cref="InvalidOperationException">if the internal <see cref="AsymmetricSignatureFormatter"/> is null. This can occur if the constructor parameter 'willBeUsedforSigning' was not 'true'.</exception>
        /// <exception cref="InvalidOperationException">if the internal <see cref="HashAlgorithm"/> is null. This can occur if a derived type deletes it or does not create it.</exception>
        public override byte[] Sign(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length == 0)
            {
                throw new ArgumentException(ErrorMessages.IDX10624);
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }

            if (this.formatter == null)
            {
                throw new InvalidOperationException(ErrorMessages.IDX10620);
            }

            if (this.hash == null)
            {
                throw new InvalidOperationException(ErrorMessages.IDX10621);
            }

            return this.formatter.CreateSignature(this.hash.ComputeHash(input));
        }

        /// <summary>
        /// Verifies that a signature over the' input' matches the signature.
        /// </summary>
        /// <param name="input">the bytes to generate the signature over.</param>
        /// <param name="signature">the value to verify against.</param>
        /// <returns>true if signature matches, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">'input' is null.</exception>
        /// <exception cref="ArgumentNullException">'signature' is null.</exception>
        /// <exception cref="ArgumentException">'input.Length' == 0.</exception>
        /// <exception cref="ArgumentException">'signature.Length' == 0.</exception>
        /// <exception cref="ObjectDisposedException">if <see cref="AsymmetricSignatureProvider.Dispose(bool)"/> has been called. </exception>
        /// <exception cref="InvalidOperationException">if the internal <see cref="AsymmetricSignatureDeformatter"/> is null. This can occur if a derived type does not call the base constructor.</exception>
        /// <exception cref="InvalidOperationException">if the internal <see cref="HashAlgorithm"/> is null. This can occur if a derived type deletes it or does not create it.</exception>
        public override bool Verify(byte[] input, byte[] signature)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            if (input.Length == 0)
            {
                throw new ArgumentException(ErrorMessages.IDX10625);
            }

            if (signature.Length == 0)
            {
                throw new ArgumentException(ErrorMessages.IDX10626);
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }

            if (this.deformatter == null)
            {
                throw new InvalidOperationException(ErrorMessages.IDX10629);
            }

            if (this.hash == null)
            {
                throw new InvalidOperationException(ErrorMessages.IDX10621);
            }

            return this.deformatter.VerifySignature(this.hash.ComputeHash(input), signature);
        }

        /// <summary>
        /// Calls <see cref="HashAlgorithm.Dispose()"/> to release this managed resources.
        /// </summary>
        /// <param name="disposing">true, if called from Dispose(), false, if invoked inside a finalizer.</param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.disposed = true;

                    if (this.hash != null)
                    {
                        this.hash.Dispose();
                        this.hash = null;
                    }
                }
            }
        }
    }
}
