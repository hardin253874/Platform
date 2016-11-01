// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata
{
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum RelationshipDirection
    {
        /// <summary>
        /// Represents an outbound relationship.
        /// </summary>
		[EnumMember( Value = "forward" )]
        Forward,

        /// <summary>
        /// Represents an inbound relationship. That is, one owned by the foreign resource.
        /// </summary>
		[EnumMember( Value = "reverse" )]
        Reverse,

        /// <summary>
        /// Specifies relationships in both directions. Not applicable in all contexts.
        /// </summary>
		[EnumMember( Value = "both" )]
        Both ,
    }
}
