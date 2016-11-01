// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using EDC.ReadiNow.Metadata.Media;
using EDC.ReadiNow.Metadata.Query.Structured;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{    
    /// <summary>
    /// Defines a rule that will use the specified
    /// icon if the defined condition is true.
    /// </summary>
    // type: iconRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class IconRule
    {
        /// <summary>
        /// The condition for this rule.
        /// </summary>
        // iconRule inherits reportCondition
        [DataMember]
        public Condition Condition { get; set; }


        /// <summary>
        /// The icon to display if the
        /// condition for this rule evaluates to true.
        /// </summary>
        [DataMember]
        public IconType Icon { get; set; }


        /// <summary>
        /// The colour to display if the
        /// condition for this rule evaluates to true.
        /// </summary>
        [DataMember]
        public ColorInfo Color { get; set; }

        /// <summary>
        /// The scale value to display if the
        /// condition for this rule evaluates to true.
        /// </summary>
        [DataMember]
        public int Scale { get{ return _scale;} set{ _scale = value;} }
        /// <summary>
        /// scale default value is 1
        /// </summary>
        private int _scale = 1;

        /// <summary>
        /// Gets or sets the unique identifier for the image associated for the format.
        /// </summary>
        /// <remarks>
        /// This will in time deprecate the IconType property.
        /// </remarks>
        /// <value>The unique identifier.</value>
        // rel: iconRuleImage
        [DataMember]
        public long? IconId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the conditional format icon entity associated for the format.
        /// </summary>
        /// <remarks>
        /// This will in time deprecate the IconType property.
        /// </remarks>
        /// <value>The unique identifier.</value>
        // rel: iconRuleCFIcon
        [DataMember]
        public long? CfEntityId { get; set; }


    }
}
