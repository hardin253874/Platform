// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Metadata about a column in the sample data.
    /// </summary>
    public class SampleColumn
    {
        /// <summary>
        /// Title from heading row of data.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Column reference. E.g. Excel column reference.
        /// </summary>
        public string ColumnName { get; set; }
    }
}
