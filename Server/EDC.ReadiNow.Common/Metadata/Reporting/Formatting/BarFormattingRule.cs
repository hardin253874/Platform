// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Media;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    /// <summary>
    /// Defines a formatting rule which displays 
    /// a progress bar of the specified color.
    /// </summary>
    // type: barFormattingRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class BarFormattingRule : FormattingRule
    {
        /// <summary>
        /// The minimum value for the progress bar.
        /// </summary>
        // field: barMinValue
        [DataMember]
        public TypedValue Minimum { get; set; }


        /// <summary>
        /// The maximum value for the progress bar.
        /// </summary>
        // field: barMaxValue
        [DataMember]
        public TypedValue Maximum { get; set; }


        /// <summary>
        /// The color of the progress bar.
        /// </summary>
        // field: barColor
        [DataMember]
        public ColorInfo Color { get; set; }        
    }
}
