// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// A table of sample import data.
    /// </summary>
    public class SampleTable
    {
        /// <summary>
        /// Table columns
        /// </summary>
        public List<SampleColumn> Columns { get; set; }

        /// <summary>
        /// Data rows.
        /// </summary>
        public List<SampleRow> Rows { get; set; }
    }
}
