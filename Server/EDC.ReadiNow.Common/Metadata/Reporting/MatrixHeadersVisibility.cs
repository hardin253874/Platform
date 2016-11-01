// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting
{
    /// <summary>
    /// The visibility of the matrix headers
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum MatrixHeadersVisibility
    {
        /// <summary>
        /// Both row and column headers are visible.
        /// </summary>
        [EnumMember]
        Both,


        /// <summary>
        /// Only row headers are visible.
        /// </summary>
        [EnumMember]
        Row,


        /// <summary>
        /// Only column headers are visible.
        /// </summary>
        [EnumMember]
        Column
    }    
}
