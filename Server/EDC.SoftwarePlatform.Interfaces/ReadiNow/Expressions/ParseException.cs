// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Represents any exception that is due to invalid script or macro instructions being provided by the user/admin.
    /// That is, every/admin user script error is a ParseException, and a ParseException always represents a user/admin error.
    /// For example: invalid syntax, entities that can't be resolved, duplicate matches when exact matches were expected.
    /// </summary>
    public class ParseException : Exception
    {
        private readonly string _shortMessage;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParseException(string message) : base(message)
        {
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="shortMessage">The short message.</param>
        public ParseException(string message, string shortMessage) : base(message)
        {
            _shortMessage = shortMessage;
        }

        /// <summary>
        /// A short error message.
        /// </summary>
        public string ShortMessage
        { 
            get { return _shortMessage ?? Message; }            
        }
    }
}
