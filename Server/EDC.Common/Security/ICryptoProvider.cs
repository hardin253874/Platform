// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Security
{
    /// <summary>
    ///     Defines an interface for performing encryption/decryption tasks.
    /// </summary>
    interface ICryptoProvider
    {
        /// <summary>
        /// Generates a random iv.
        /// </summary>
        /// <returns></returns>
        byte[] GenerateRandomIv();


        /// <summary>
        ///     Encrypts the specified data using the specified key and iv.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        byte[] EncryptData(string plainText, byte[] key, byte[] iv);


        /// <summary>
        ///     Decrypts the specified data using the specified key and iv.        
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        string DecryptData(byte[] cipherText, byte[] key, byte[] iv);
    }
}
