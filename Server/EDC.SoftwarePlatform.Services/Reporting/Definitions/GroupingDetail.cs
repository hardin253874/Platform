// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    public class GroupingDetail
    {
        public string Style { get; set; }

        public string Value { get; set; }

        public Dictionary<long, string> Values { get; set; }

        public bool Collapsed { get; set; }
    }
}
