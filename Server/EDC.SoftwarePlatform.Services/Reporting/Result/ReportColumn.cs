// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// Metadata about an individual report column.
    /// </summary>
    public class ReportColumn
    {
        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        public long Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the type for the analyser.
        /// </summary>
        public string OperatorType { get; set; }

        /// <summary>
        /// Gets or sets the type unique identifier.
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// Gets or sets the relationship identifier.
        /// </summary>
        public long RelationshipTypeId { get; set; }

        /// <summary>
        /// Gets or sets the relationship direction.
        /// </summary>
        public bool RelationshipIsReverse { get; set; }

        /// <summary>
        /// Gets or sets the field unique identifier.
        /// </summary>
        public long FieldId { get; set; }

        /// <summary>
        /// Gets or sets the minimum length.
        /// </summary>
        public long MinimumLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        public long MaximumLength { get; set; }

        /// <summary>
        /// Gets or sets the regular expression.
        /// </summary>
        public string RegularExpression { get; set; }

        public string RegularExpressionErrorMessage { get; set; }

        public bool MultiLine { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public string DefaultValue { get; set; }

        public bool IsRequired { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsHidden { get; set; }

        public bool IsAggregateColumn { get; set; }

        public decimal MinimumDecimal { get; set; }

        public decimal MaximumDecimal { get; set; }

        public long? DecimalPlaces { get; set; }

        public DateTime? MinimumDate { get; set; }

        public DateTime? MaximumDate { get; set; }

        public string AutoNumberDisplayPattern { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this report column has conditional formatting.
        /// </summary>
        public bool HasConditionalFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column is the entity name field for the primary entity for the report.
        /// </summary>
        public bool EntityNameField { get; set; }

        /// <summary>
        /// Gets or sets the cardinality.
        /// </summary>
        public string Cardinality { get; set; }

        /// <summary>
        /// Gets or sets any error message associated with the column.
        /// </summary>
        public string ColumnError { get; set; }
    }
}
