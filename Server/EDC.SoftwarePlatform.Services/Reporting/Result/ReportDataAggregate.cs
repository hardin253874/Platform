// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// A row of group/rollup data values.
    /// </summary>
    public class ReportDataAggregate
    {
        public long GroupBitmap { get; set; }

        public long? Total { get; set; }

        public List<Dictionary<long, CellValue>> GroupHeadings { get; set; }

        public Dictionary<long, List<AggregateItem>> Aggregates { get; set; }
    }

}
