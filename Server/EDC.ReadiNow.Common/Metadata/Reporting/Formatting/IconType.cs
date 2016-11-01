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
    /// Defines the image to be displayed in a column.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum IconType
    {
        /// <summary>No image specified.</summary>
        [EnumMember]
        None,
                                        
        /// <summary>An arrow pointing down.</summary>
        [EnumMember]
        ArrowDown,

        /// <summary>An arrow pointing left.</summary>
        [EnumMember]
        ArrowLeft,

        /// <summary>An arrow pointing right.</summary>
        [EnumMember]
        ArrowRight,

        /// <summary>An arrow pointing up.</summary>
        [EnumMember]
        ArrowUp,

        /// <summary>A circle.</summary>
        [EnumMember]
        Circle,

        /// <summary>A cross.</summary>
        [EnumMember]
        Cross,

        /// <summary>An exlamation mark.</summary>
        [EnumMember]
        ExclamationMark,

        /// <summary>A square.</summary>
        [EnumMember]
        Square,

        /// <summary>A tick.</summary>
        [EnumMember]
        Tick,

        /// <summary>A triangle.</summary>
        [EnumMember]
        Triangle,

        /// <summary>A semi circle.</summary>
        [EnumMember]
        SemiCircle = 11,

        /// <summary>A star.</summary>
        [EnumMember]
        Star = 12,

        /// <summary>A diamond.</summary>
        [EnumMember]
        Diamond = 13,

    }
}
