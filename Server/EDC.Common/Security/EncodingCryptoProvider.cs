// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.Security
{
    /// <summary>
    ///     Represents a class that is used to encrypt data.
    /// </summary>    
    public class EncodingCryptoProvider
    {
        /// <summary>
        ///     Initialises this instance.
        /// </summary>
        /// <returns></returns>
        private byte[] Initialise()
        {
            var g1 = new Guid("d84246bf-1d04-4f52-a704-b088d2492781");
            var g2 = new Guid("22167040-2655-428b-84f2-2f0d9218d8e1");            

            var bytes = new List<byte>();
            bytes.AddRange(g1.ToByteArray());
            bytes.AddRange(g2.ToByteArray());

            return bytes.ToArray();
        }


        /// <summary>
        /// Encrypts and encodes the specified data.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        public string EncryptAndEncode(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            CryptoSettings settings = CryptoSettings.GetSettings();
            ICryptoProvider cryptoProvider = settings.CryptoProvider;

            byte[] iv = cryptoProvider.GenerateRandomIv();
            byte[] cipherText = cryptoProvider.EncryptData(plainText, Initialise(), iv);
            return EncodeCipherText(settings.Version, iv, cipherText);
        }


        /// <summary>
        /// Decodes and decrypts the specified cipher text.
        /// </summary>
        /// <param name="encodedCipherText">The encoded cipher text.</param>
        /// <returns></returns>
        public string DecodeAndDecrypt(string encodedCipherText)
        {
            if (string.IsNullOrEmpty(encodedCipherText))
            {
                return encodedCipherText;
            }

            int version;
            byte[] iv;
            byte[] cipherText;

            if (!TryDecodeEncodedCipherText(encodedCipherText, out version, out iv, out cipherText))
            {
                return null;
            }

            CryptoSettings settings = CryptoSettings.GetSettings(version);
            ICryptoProvider cryptoProvider = settings.CryptoProvider;

            return cryptoProvider.DecryptData(cipherText, Initialise(), iv);
        }


        /// <summary>
        ///     Tries to decode encoded cipher text.
        /// </summary>
        /// <param name="encodedCipherText">The encoded cipher text.</param>
        /// <param name="version">The version.</param>
        /// <param name="iv">The iv.</param>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns>True if successfull, false otherwise.</returns>
        private bool TryDecodeEncodedCipherText(string encodedCipherText, out int version, out byte[] iv, out byte[] cipherText)
        {
            bool result = false;

            version = 0;
            iv = null;
            cipherText = null;

            if (string.IsNullOrEmpty(encodedCipherText))
            {
                return false;
            }

            string[] encodedParts = encodedCipherText.Split('|');
            if (encodedParts.Length != 3)
            {
                return false;
            }

            string versionPart = encodedParts[0];
            string encodedIvPart = encodedParts[1];
            string encodedCipherPart = encodedParts[2];

            if (!string.IsNullOrEmpty(versionPart))
            {
                int.TryParse(versionPart, out version);
            }

            if (!string.IsNullOrEmpty(encodedIvPart))
            {
                iv = Convert.FromBase64String(encodedIvPart);
            }

            if (!string.IsNullOrEmpty(encodedCipherPart))
            {
                cipherText = Convert.FromBase64String(encodedCipherPart);
            }

            if (version > 0 &&
                iv != null &&
                iv.Length > 0 &&
                cipherText != null &&
                cipherText.Length > 0)
            {
                result = true;
            }

            return result;
        }


        /// <summary>
        ///     Encodes the cipher text.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="iv">The iv.</param>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">version</exception>
        /// <exception cref="System.ArgumentNullException">
        ///     iv
        ///     or
        ///     cipherText
        /// </exception>
        private string EncodeCipherText(int version, byte[] iv, byte[] cipherText)
        {
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException( nameof( version ) );
            }

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException( nameof( iv ) );
            }

            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException( nameof( cipherText ) );
            }


            return string.Format("{0}|{1}|{2}", version, Convert.ToBase64String(iv), Convert.ToBase64String(cipherText));
        }


        #region CryptoSettings Class


        /// <summary>
        /// </summary>
        private class CryptoSettings
        {
            /// <summary>
            ///     Settings for V1.0
            /// </summary>
            private const int CryptoVersion10 = 1;


            /// <summary>
            ///     Prevents a default instance of the <see cref="CryptoSettings" /> class from being created.
            /// </summary>
            private CryptoSettings()
            {
            }


            /// <summary>
            ///     Gets or sets the version.
            /// </summary>
            /// <value>
            ///     The version.
            /// </value>
            public int Version { get; private set; }


            /// <summary>
            ///     Gets or sets the actual crypto provider.
            /// </summary>
            /// <value>
            ///     The crypto provider.
            /// </value>
            public ICryptoProvider CryptoProvider { get; private set; }


            /// <summary>
            ///     Gets the settings.
            /// </summary>
            /// <param name="version">The version.</param>
            /// <returns></returns>
            /// <exception cref="System.ArgumentOutOfRangeException">version</exception>
            public static CryptoSettings GetSettings(int version = CryptoVersion10)
            {
                CryptoSettings settings;

                switch (version)
                {
                    case CryptoVersion10:
                        settings = new CryptoSettings
                        {
                            Version = version,
                            CryptoProvider = new Aes256CryptoProvider()
                        };
                        break;

                    default:
                        settings = null;
                        break;
                }

                if (settings == null)
                {
                    throw new ArgumentOutOfRangeException( nameof( version ) );
                }

                return settings;
            }
        }


        #endregion
    }
}