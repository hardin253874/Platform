// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Holds any settings that are passed into the report conversion process.
    /// </summary>
    public class ToEntitySettings
    {
        /// <summary>
        /// The entity that should be written to.
        /// </summary>
        public Model.Report ReportEntity { get; set; }

        /// <summary>
        /// If true, then indicates that existing content should be cleared.
        /// </summary>
        public bool PurgeExisting { get; set; }
    }
}
