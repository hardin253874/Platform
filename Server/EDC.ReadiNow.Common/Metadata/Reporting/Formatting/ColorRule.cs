// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Media;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    /// <summary>
    /// Defines a rule that will use the specified
    /// colors if the defined condition is true.
    /// </summary>
    // type: colorRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ColorRule
    {
        /// <summary>
        /// The condition for this rule.
        /// </summary>
        // iconRule inherits reportCondition
        [DataMember]
        public Condition Condition { get; set; }


        /// <summary>
        /// The background color to use if the
        /// condition for this rule evaluates to true.
        /// </summary>
        // field: colorRuleBackground
        [DataMember]
        public ColorInfo BackgroundColor { get; set; }


        /// <summary>
        /// The foreground color to use if the
        /// condition for this rule evaluates to true.
        /// </summary>
        // field: colorRuleForeground
        [DataMember]
        public ColorInfo ForegroundColor { get; set; } 
    }
}
