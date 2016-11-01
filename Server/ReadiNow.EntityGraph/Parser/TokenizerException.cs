// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.EntityGraph.Parser
{
    /// <summary>
    /// An error found during tokenization.
    /// </summary>
    public class TokenizerException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenizerException(string message)
            : base(message)
        {
        }
    }
}
