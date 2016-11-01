// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using System.Collections.Generic;

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    public class SelectedColumnCondition
    {
        /// <summary>
        /// Gets or sets the expression unique identifier.
        /// </summary>
        /// <value>The expression unique identifier.</value>
        public string ExpressionId { get; set; }
        /// <summary>
        /// Gets or sets the data type for the column.
        /// </summary>
        /// <value>The data type.</value>
        public DatabaseType Type { get; set; }
        /// <summary>
        /// Gets or sets the default operator.
        /// </summary>
        /// <value>The default operator.</value>
        public ConditionType Operator { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the entity identifiers.
        /// </summary>
        /// <value>The entity identifiers.</value>
        public List<long> EntityIdentifiers { get; set; }
    }
}
