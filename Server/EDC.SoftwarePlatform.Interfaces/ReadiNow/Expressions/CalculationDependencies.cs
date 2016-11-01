// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Holds results of determining the compile-time dependencies of a calculation.
    /// </summary>
    public class CalculationDependencies
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="identifiedEntities">IDs of all entities that are referenced within the *text* of the script. (Including fields and relationships)</param>
        /// <param name="fields">Fields that are accessed by a calculation.</param>
        /// <param name="relationships">Fields that are accessed by a calculation.</param>
        public CalculationDependencies(IReadOnlyCollection<long> identifiedEntities, IReadOnlyCollection<long> fields, IReadOnlyCollection<long> relationships)
        {
            if (identifiedEntities == null)
                throw new ArgumentNullException("identifiedEntities");
            if (fields == null)
                throw new ArgumentNullException("fields");
            if (relationships == null)
                throw new ArgumentNullException("relationships");

            IdentifiedEntities = identifiedEntities;
            Fields = fields;
            Relationships = relationships;
        }

        /// <summary>
        /// IDs of all entities that are explicitly referenced in the calculated.
        /// </summary>
        /// <remarks>
        /// Excludes the root entity, as it is not explicitly referenced.
        /// </remarks>
        public IReadOnlyCollection<long> IdentifiedEntities { get; private set; }

        /// <summary>
        /// IDs of all fields used in the calculation.
        /// </summary>
        public IReadOnlyCollection<long> Fields { get; private set; }

        /// <summary>
        /// IDs of all relationship types used in the calculation.
        /// </summary>
        public IReadOnlyCollection<long> Relationships { get; private set; }
    }
}
