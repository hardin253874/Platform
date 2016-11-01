// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EDC.Database;
using System.Runtime.Serialization;
using EDC.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum TextAlignment
    {
        [EnumMember] Automatic = 0,

        [EnumMember] Left = 1,

        [EnumMember] Center = 2,

        [EnumMember] Right = 3,
    }

    /// <summary>
    /// This class defines the display formatting for a particular column.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ColumnFormatting
    {
        /// <summary>
        /// The id of the column the formatting applies to.
        /// </summary>
        // Entity model: formatting is directly on reportColumn.
        [DataMember]
        public Guid QueryColumnId { get; set; }        


        /// <summary>
        /// The name of the column the formatting applies to.
        /// </summary>
        // Entity model: reportColumn.name for display name, and formatting available directly on reportColumn.
        [DataMember]
        public string ColumnName { get; set; }


        /// <summary>
        /// The data type of the column the formatting applies to.
        /// </summary>
        [DataMember]
        // Entity model: reportColumn.columnExpression.reportExpressionResultType
        public DatabaseType ColumnType { get; set; }


        /// <summary>
        /// True if the data text is to be shown in
        /// addition to the formatting, false otherwise.
        /// </summary>
        // field: columnShowText
        [DataMember]
        public bool ShowText { get; set; }


        /// <summary>
        /// Gets or sets the format string.
        /// </summary>
        /// <value>
        /// The format string.
        /// </value>
        //
        // FormatString for entity types and Reporting API now using the date/datetime/time format string variables at the end of this class
        //
        [DataMember]
        public string FormatString { get; set; }


        /// <summary>
        /// True if the data text is to be shown in
        /// addition to the formatting, false otherwise.
        /// </summary>
        // rel: formatAlignment
        [DataMember]
        public TextAlignment TextAlignment { get; set; }


        /// <summary>
        /// The formatting rule describing how to format the data.
        /// </summary>
        // rel: columnFormattingRule
        [DataMember]
        public FormattingRule FormattingRule { get; set; }


        /// <summary>
        /// Gets or sets the decimal places.
        /// </summary>
        /// <value>
        /// The decimal places.
        /// </value>
        // field: formatDecimalPlaces
        [DataMember]
        public int DecimalPlaces { get; set; }


        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        /// <value>
        /// The prefix.
        /// </value>
        // field: prefix
        [DataMember]
        public string Prefix { get; set; }


        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>
        /// The suffix.
        /// </value>
        // field: suffix
        [DataMember]
        public string Suffix { get; set; }


        /// <summary>
        /// Gets or sets the line number of multiline text column.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        // field: maxLineCount
        [DataMember]
        public int Lines { get; set; }

        [XmlIgnore]
        public long EntityId { get; set; }

        [XmlIgnore]
        public AlignEnum Alignment { get; set; }
        [XmlIgnore]
        public DateColFmtEnum DateFormat { get; set; }
        [XmlIgnore]
        public TimeColFmtEnum TimeFormat { get; set; }
        [XmlIgnore]
        public DateTimeColFmtEnum DateTimeFormat { get; set; }

    }

    /// <summary>
    /// This class defines the display formatting for a particular chart.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ChartFormatting
    {

        /// <summary>
        /// The id of the column the formatting applies to.
        /// </summary>
        [DataMember]
        public Guid QueryColumnId { get; set; }

        /// <summary>
        /// Gets or sets the color type of the chart.
        /// </summary>
        /// <value>
        /// The color type of the chart.
        /// </value>
        [DataMember]
        public ChartColorType ChartColorType { get; set; }

        /// <summary>
        /// Gets or sets the custom color of the chart.
        /// </summary>
        /// <value>
        /// The agrb string value of the chart custom color. 
        /// </value>
        [DataMember]
        public string ChartCustomColor { get; set; }
    }
}
