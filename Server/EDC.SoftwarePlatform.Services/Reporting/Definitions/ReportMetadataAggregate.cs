// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    public class ReportMetadataAggregate
    {
        public bool IncludeRollup { get; set; }

        public bool IgnoreRows { get; set; }

        public bool ShowGrandTotals { get; set; }

        public bool ShowSubTotals { get; set; }

        public bool ShowCount { get; set; }

        public bool ShowGroupLabel { get; set; }

        public bool ShowOptionLabel { get; set; }

        public List<Dictionary<long, GroupingDetail>> Groups { get; set; }

        public Dictionary<long, List<AggregateDetail>> Aggregates { get; set; }
    }

}
