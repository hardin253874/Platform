// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Holds calculated field results for a single field over multiple entities.
    /// </summary>
    public class CalculatedFieldResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldId">The calculated field ID.</param>
        /// <param name="entities">Results for this field on each individual entity.</param>
        public CalculatedFieldResult( long fieldId, IReadOnlyCollection<CalculatedFieldSingleResult> entities )
        {
            if ( fieldId <= 0 )
                throw new ArgumentOutOfRangeException( nameof( fieldId ) );
            if ( entities == null )
                throw new ArgumentOutOfRangeException( nameof( entities ) );

            FieldId = fieldId;
            Entities = entities;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldId">The calculated field ID.</param>
        /// <param name="exception">A parse exception that was encountered while compiling the calculation for this field.</param>
        public CalculatedFieldResult( long fieldId, ParseException exception )
        {
            if ( fieldId <= 0 )
                throw new ArgumentOutOfRangeException( nameof( fieldId ) );
            if ( exception == null )
                throw new ArgumentOutOfRangeException( nameof( exception ) );

            FieldId = fieldId;
            ParseException = exception;
        }

        /// <summary>
        /// The calculated field ID that this result applies to.
        /// </summary>
        public long FieldId { get; private set; }

        /// <summary>
        /// An exception found while processing this field.
        /// </summary>
        public ParseException ParseException { get; private set; }

        /// <summary>
        /// Results for individual entities.
        /// </summary>
        public IReadOnlyCollection<CalculatedFieldSingleResult> Entities { get; private set; }
    }
}
