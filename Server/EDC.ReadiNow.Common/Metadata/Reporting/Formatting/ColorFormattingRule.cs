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
    /// Defines a formatting rule which displays 
    /// the data in a particular color.
    /// </summary>
    // type: colorFormattingRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ColorFormattingRule : FormattingRule
    {
        /// <summary>
        /// Contructs a new instance of a ColorFormattingRule.
        /// </summary>
        public ColorFormattingRule()
        {
            Rules = new List<ColorRule>();
        }


        /// <summary>
        /// The list of rules.
        /// </summary>
        // rel: colorRules
        [DataMember]
        public IList<ColorRule> Rules { get; set; }        
    }
}
