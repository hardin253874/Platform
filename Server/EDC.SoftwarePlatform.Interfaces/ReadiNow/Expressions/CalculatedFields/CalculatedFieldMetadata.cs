// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Represents metadata about a calculated field.
    /// </summary>
    public class CalculatedFieldMetadata
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="calculation"></param>
        /// <param name="expression"></param>
        /// <param name="exception"></param>
        public CalculatedFieldMetadata(long fieldId, string calculation, IExpression expression, ParseException exception)
        {
            CalculatedFieldId = fieldId;
            Calculation = calculation;      // may be null if fieldId wasn't a valid calculation.
            Expression = expression;        // may be null if calculation wasn't valid.
            Exception = exception;
        }

        /// <summary>
        /// The calculated field ID.
        /// </summary>
        public long CalculatedFieldId { get; private set; }

        /// <summary>
        /// The calculation
        /// </summary>
        public string Calculation { get; private set; }

        /// <summary>
        /// The compiled expression.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <summary>
        /// Any exception received while trying to statically process this calculated field.
        /// </summary>
        public ParseException Exception { get; private set; }
    }
}
