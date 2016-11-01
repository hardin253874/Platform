// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// A cell of report results, or rollup value.
    /// </summary>
    public class CellValue
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the entity unique identifier.
        /// </summary>
        /// <value>The entity unique identifier.</value>
        public Dictionary<long, string> Values { get; set; }

        /// <summary>
        /// Gets or sets the conditional format index.
        /// </summary>
        /// <value>The conditional format index.</value>
        public long? ConditionalFormatIndex { get; set; }
    }
}
