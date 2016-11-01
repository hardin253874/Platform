// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Performs a clustering transform on an expression, such as grouping dates together by year/month, etc.
    /// The clustered expression returns values in a canonical format that is of the same data type.
    /// For example, if a date is being clustered by year/month, then the date 2013-07-23 gets transformed to 2013-07-01.
    /// </summary>
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    [Flags]
    public enum ClusterOperation
    {
        None = 0,
        Year = 1,
        Quarter = 2,
        Month = 4,
        Weekday = 8,
        Day = 16,
        Hour = 32,
        Minute = 64,
        Second = 128
    }
    
}
