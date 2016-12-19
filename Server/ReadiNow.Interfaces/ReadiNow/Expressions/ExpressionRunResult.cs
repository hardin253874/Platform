// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Holds the result of running an expression.
    /// </summary>
    public class ExpressionRunResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="result">The result.</param>
        public ExpressionRunResult( object result )
        {
            Value = result;
        }

        /// <summary>
        /// The result.
        /// </summary>
        public object Value
        {
            get;
        }
    }
}
