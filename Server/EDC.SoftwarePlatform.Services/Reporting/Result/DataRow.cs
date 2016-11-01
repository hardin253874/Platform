// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// A row of report results.
    /// </summary>
    public class DataRow
    {
        /// <summary>
        /// Gets or sets the entity unique identifier.
        /// </summary>
        public long EntityId { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        public List<CellValue> Values { get; set; }
    }
}
