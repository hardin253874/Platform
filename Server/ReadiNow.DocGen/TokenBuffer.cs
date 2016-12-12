// Copyright 2011-2016 Global Software Innovation Pty Ltd
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Manages a buffer of tokens.
    /// </summary>
    class TokenBuffer
    {
        public TokenBuffer() : this(new List<Token>())
        {
        }

        private TokenBuffer(List<Token> initialList)
        {
            Tokens = initialList;
        }


        /// <summary>
        /// The tokens.
        /// </summary>
        public List<Token> Tokens { get; }


        /// <summary>
        /// Adds the specified token to the end of the buffer.
        /// </summary>
        /// <param name="token">The token.</param>
        public void Add(Token token)
        {
            Tokens.Add(token);
        }


        /// <summary>
        /// Remove the specified token from the buffer, and all after it.
        /// </summary>
        /// <param name="token">The token to remove.</param>
        public void RemoveTokensSince(Token token)
        {
            int pos = FindTokenInBuffer(token);
            Tokens.RemoveRange(pos, Tokens.Count - pos);
        }


        /// <summary>
        /// Determine the location of a token in the buffer.
        /// </summary>
        /// <param name="token">The token to find.</param>
        /// <returns>Zero-based index in the buffer.</returns>
        /// <exception cref="Exception">Exception is thrown if the token is not found.</exception>
        public int FindTokenInBuffer(Token token)
        {
            int pos = Tokens.LastIndexOf(token);
            if (pos == -1)
                throw new Exception("Could not find token in buffer");
            return pos;
        }


        /// <summary>
        /// Determine the location of a token in the buffer.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="isClose"></param>
        /// <returns></returns>
        public int FindTokenInBuffer(OpenXmlElement element, bool isClose = false)
        {
            return FindTokenInBuffer(new Token(element, isClose));
        }


        /// <summary>
        /// Remove a portion from the end of the buffer and return it.
        /// </summary>
        /// <param name="position">Location to start snipping content from.</param>
        /// <returns>New buffer containing token at the specified position, and all following.</returns>
        public TokenBuffer SnipFromBuffer(int position)
        {
            List<Token> result = Tokens.GetRange(position, Tokens.Count - position);
            Tokens.RemoveRange(position, Tokens.Count - position);
            return new TokenBuffer(result);
        }


    }
}
