// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;

namespace EDC.IO
{
    /// <summary>
    /// A random file token provider.    
    /// </summary>
    public class RandomFileTokenProvider : IFileTokenProvider
    {
        /// <summary>
        /// Computes a token for the specified file stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>A random file token.</returns>
        public string ComputeToken(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return Guid.NewGuid().ToString("N").ToUpperInvariant();
        }
    }
}
