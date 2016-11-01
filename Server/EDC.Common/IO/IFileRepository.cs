// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.IO;

namespace EDC.IO
{
    /// <summary>
    /// Interface for a file repository.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Gets a file with a given token from the file repository.
        /// </summary>
        /// <param name="token">The token that refers to the file.</param>
        /// <returns>The file data stream.</returns>
        Stream Get(string token);

        /// <summary>
        /// Deletes a file with the given token from the file repository.
        /// </summary>
        /// <param name="token">The token that refers to the file.</param>
        void Delete(string token);

        /// <summary>
        /// Puts a file into the repository and returns a token.
        /// </summary>
        /// <param name="stream">Stream containing the file data.</param>
        /// <returns>A token that refers to the file.</returns>
        string Put(Stream stream);

        /// <summary>
        /// Gets all the tokens in the repository.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetTokens();
    }
}