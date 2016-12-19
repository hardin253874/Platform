// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Represents any exception that is due to a script failing due to user error at runtime.
    /// For example: divide by zero.
    /// </summary>
    public class EvaluationException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EvaluationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// A short message.
        /// </summary>
        public string ShortMessage
        {
            get { return _shortMessage ?? Message; }
            set { _shortMessage = value; }
        }
        string _shortMessage;
    }
}
