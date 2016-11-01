// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// A helper class for creating parse exceptions.
    /// </summary>
    /// <remarks>
    /// We don't want the various parser/tokenizer classes to be used in the public interface of parse exception
    /// otherwise it will require undesirable assembly references to Irony in interfaces.
    /// </remarks>
    public static class ParseExceptionHelper
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="location"></param>
        public static ParseException New(string message, SourceLocation location)
        {
            return new ParseException(Reformat(message, location));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="node"></param>
        public static ParseException New(string message, ParseTreeNode node)
        {
            return new ParseException(Reformat(message, node));
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="node">The node.</param>
		/// <param name="shortMessage">The short message.</param>
		/// <returns></returns>
        public static ParseException New(string message, ParseTreeNode node, string shortMessage)
        {
            return new ParseException(Reformat(message, node), shortMessage);
        }

        private static string Reformat(string message, SourceLocation location)
        {
            return string.Format("{0} (pos {1})", message, location.Position + 1);
        }

        internal static string Reformat(string message, ParseTreeNode node)
        {
            var token = node.Token;
            if (token != null)
                return Reformat(message, token.Location);
            if (node.ChildNodes.Count > 0)
                return Reformat(message, node.ChildNodes[0]);
            return message;
        }


    }
}
