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
    /// Available chart types.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum ChartType
    {
        /// <summary>
        /// No chart type is defined.
        /// </summary>
        [EnumMember]
        None,


        /// <summary>
        /// An area chart.
        /// </summary>
        [EnumMember]
        Area,


        /// <summary>
        /// A bar chart.
        /// </summary>
        [EnumMember]
        Bar,


        /// <summary>
        /// A bubble chart.
        /// </summary>
        [EnumMember]
        Bubble,


        /// <summary>
        /// A column chart.
        /// </summary>
        [EnumMember]
        Column,


        /// <summary>
        /// A line chart.
        /// </summary>
        [EnumMember]
        Line,


        /// <summary>
        /// A pie chart.
        /// </summary>
        [EnumMember]
        Pie,


        /// <summary>
        /// A scatter chart.
        /// </summary>
        [EnumMember]
        Scatter
    }

     /// <summary>
    /// Available chart color types.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum ChartColorType
    {
        /// <summary>
        /// the Automatic chart color type.
        /// </summary>
        [EnumMember]
        Automatic,

        /// <summary>
        /// the Vary Colors by point chart color type.
        /// </summary>
        [EnumMember]
        VaryColorsByPoint,

        /// <summary>
        /// the custom chart color type
        /// </summary>
        [EnumMember]
        CustomColor
    }
}
