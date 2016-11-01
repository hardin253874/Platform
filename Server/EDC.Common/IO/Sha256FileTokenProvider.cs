// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Security.Cryptography;
using EDC.Text;

namespace EDC.IO
{
    /// <summary>
    /// A Sha256 file token provider.
    /// </summary>
    public class Sha256FileTokenProvider : IFileTokenProvider
    {
        /// <summary>
        /// Computes a token for the specified file stream.        
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <returns>The token.</returns>
        public string ComputeToken(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            byte[] hashBytes;

            // Hash the incoming data
            using (var sha256Hash = new SHA256Managed())
            {
                stream.Position = 0;
                hashBytes = sha256Hash.ComputeHash(stream);
            }

            stream.Position = 0;

            // Convert the hashed data to a Base32 string.            
            return Base32Encoding.ToBase32String(hashBytes);
        }
    }
}
