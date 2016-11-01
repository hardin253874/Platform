// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using ReadiNow.Reporting.Definitions;

namespace ReadiNow.Reporting
{
    /// <summary>
    /// Class ConditionInfo.
    /// </summary>
    internal class ConditionInfo
    {
        internal Predicate<object> Predicate { get; set; }
        internal bool ShowText { get; set; }
        internal ConditionType Operator { get; set; }
        internal ColorRule ColorRule { get; set; }
        internal object LowerBounds { get; set; }
        internal object Upperbounds { get; set; }
        internal IconRule IconRule { get; set; }
        internal long IconEntityId { get; set; }
        internal IList<TypedValue> Values { get; set; }
        internal ConditionalFormatStyleEnum Style { get; set; }
    }
}
