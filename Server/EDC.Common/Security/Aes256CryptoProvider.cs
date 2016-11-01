// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Security.Cryptography;

namespace EDC.Security
{
    /// <summary>
    ///     This class is used to encrypt/decrypt data using AES256.
    /// </summary>
    public class Aes256CryptoProvider : ICryptoProvider
    {
        /// <summary>
        ///     The key size.
        /// </summary>
        private const int KeySize = 256;       


        #region ICryptoProvider Members


        /// <summary>
        ///     Generates a random iv.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateRandomIv()
        {
            using (var aes256 = new AesManaged())
            {
                aes256.KeySize = KeySize;                

                aes256.GenerateIV();

                return aes256.IV;
            }
        }


        /// <summary>
        ///     Encrypts the specified data using the specified key and iv.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        ///     plainText
        ///     or
        ///     key
        ///     or
        ///     iv
        /// </exception>
        public byte[] EncryptData(string plainText, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException("plainText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("iv");
            }

            using (AesManaged aes256 = CreateAesManaged(key, iv))
            {
                ICryptoTransform encryptor = aes256.CreateEncryptor(aes256.Key, aes256.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        return msEncrypt.ToArray();
                    }
                }
            }
        }


        /// <summary>
        ///     Decrypts the specified data using the specified key and iv.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        ///     cipherText
        ///     or
        ///     key
        ///     or
        ///     iv
        /// </exception>
        public string DecryptData(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("iv");
            }

            string plainText;

            using (AesManaged aes256 = CreateAesManaged(key, iv))
            {
                ICryptoTransform decryptor = aes256.CreateDecryptor(aes256.Key, aes256.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plainText;
        }


        #endregion


        /// <summary>
        /// Creates the aes managed class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        private AesManaged CreateAesManaged(byte[] key, byte[] iv)
        {
            return new AesManaged { KeySize = KeySize, Key = key, IV = iv };
        }
    }
}