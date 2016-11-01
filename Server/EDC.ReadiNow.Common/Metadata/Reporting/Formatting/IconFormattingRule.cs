// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    /// <summary>
    /// Defines a formatting rule which displays an icon
    /// of the specified color.
    /// </summary>
    // type: IconFormattingRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class IconFormattingRule : FormattingRule
    {
        /// <summary>
        /// Contructs a new instance of a IconFormattingRule.
        /// </summary>
        public IconFormattingRule()
        {
            Rules = new List<IconRule>();
        }


        /// <summary>
        /// The list of icon rules.
        /// </summary>
        // rel: iconRules
        [DataMember]
        public List<IconRule> Rules { get; set; }
    }
}
