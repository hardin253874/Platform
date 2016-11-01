// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    /// <summary>
    /// </summary>
    // type: imageFormattingRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ImageFormattingRule : FormattingRule
    {
        /// <summary>
        ///     Gets or sets the thumbnail scale id.
        /// </summary>
        /// <value>
        ///     The scale id.
        /// </value>
        // rel: ruleImageScale
        [DataMember]
        public EntityRef ThumbnailScaleId { get; set; }

        /// <summary>
        ///     Gets or sets the thumbnail size id.
        /// </summary>
        /// <value>
        ///     The size id.
        /// </value>
        // rel: ruleThumbnailSize
        [DataMember]
        public EntityRef ThumbnailSizeId { get; set; }
    }
}