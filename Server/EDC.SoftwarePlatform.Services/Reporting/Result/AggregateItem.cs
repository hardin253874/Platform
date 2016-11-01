// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Result
{
    public class AggregateItem
    {
        public string Value { get; set; }

        public Dictionary<long, string> Values { get; set; }
    }
}
