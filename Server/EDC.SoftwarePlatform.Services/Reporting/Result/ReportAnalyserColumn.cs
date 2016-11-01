// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using System.Collections.Generic;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// Metadata about an individual report analyzer column.
    /// </summary>
    public class ReportAnalyserColumn
    {
        public long Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the data type for the column.
        /// </summary>
        /// <value>The data type.</value>
        public DatabaseType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the analyser.
        /// </summary>
        /// <value>The type of the analyser.</value>
        public string AnalyserType { get; set; }

        /// <summary>
        /// Gets or sets the type unique identifier.
        /// </summary>
        /// <value>The type unique identifier.</value>
        public long TypeId { get; set; }  
                 
        /// <summary>
        /// Gets or sets the filtered unique identifier.
        /// </summary>
        /// <value>The filtered unique identifier.</value>
        public long[] FilteredEntityIds { get; set; }

        /// <summary>
        /// Gets or sets the default operator.
        /// </summary>
        /// <value>The default operator.</value>
        public ConditionType? Operator { get; set; }

        /// <summary>
        /// Gets or sets the report column that is referenced by this analyser condition.
        /// </summary>
        /// <value>The report column.</value>
        public long ReportColumnId { get; set; }

        /// <summary>
        /// Gets or sets the default operator.
        /// </summary>
        /// <value>The default operator.</value>
        public ConditionType DefaultOperator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is condition locked.
        /// </summary>
        /// <value><c>true</c> if this instance is condition locked; otherwise, <c>false</c>.</value>
        public bool IsConditionLocked { get; set; }

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
        /// Gets or sets the resource picked to pick instances of a type for the analyser condition parameter.
        /// </summary>
        /// <value>The analyser condition parameter picker.</value>
        public long ConditionParameterPickerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is the name column for the type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is name column for the type; otherwise, <c>false</c>.
        /// </value>
        public bool IsNameColumnForType { get; set; }
    }
}
