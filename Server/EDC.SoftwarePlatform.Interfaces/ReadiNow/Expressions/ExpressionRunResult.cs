// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Holds the result of running an expression.
    /// </summary>
    public class ExpressionRunResult
    {
        private readonly object _result;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="result">The result.</param>
        public ExpressionRunResult( object result )
        {
            _result = result;
        }

        /// <summary>
        /// The result.
        /// </summary>
        public object Value
        {
            get { return _result; }
        }        
    }
}
