// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;
using System.Collections.Generic;

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    /// <summary>
    /// An individual conditional format rule.
    /// </summary>
    /// <remarks>
    ///     Child of <see cref="ReportColumnConditionalFormat"/>.
    /// </remarks>
    public class ReportConditionalFormatRule
    {
        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>The operator.</value>
        public ConditionType? Operator { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public Dictionary<long, string> Values { get; set; }

        /// <summary>
        /// The foreground colour
        /// </summary>
        public ReportConditionColor ForegroundColor { get; set; }

        /// <summary>
        /// The background colour
        /// </summary>
        public ReportConditionColor BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the image entity unique identifier.
        /// </summary>
        /// <value>The image entity unique identifier.</value>
        public long? ImageEntityId { get; set; }

        /// <summary>
        /// Gets or sets the image entity unique identifier.
        /// </summary>
        /// <value>The conditional format entity unique identifier.</value>
        public long? CfEntityId { get; set; }

        /// <summary>
        /// The percentage bounds
        /// </summary>
        public ReportPercentageBounds PercentageBounds { get; set; }


        public Predicate<object> Predicate { get; set; }

    }
}
