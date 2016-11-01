// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;

namespace EDC.IO
{
    /// <summary>
    /// Interface for a file token provider.
    /// </summary>
    public interface IFileTokenProvider
    {
        /// <summary>
        ///     Computes a token for the specified stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>The token.</returns>
        string ComputeToken(Stream stream);
    }
}