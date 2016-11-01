// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Reporting.Result
{
    public class ReportImageScale
    {
        /// <summary>
        /// Gets or sets the image size unique identifier.
        /// </summary>
        public long? ImageSizeId { get; set; }

        /// <summary>
        /// Gets or sets the image scale unique identifier.
        /// </summary>
        public long? ImageScaleId { get; set; }

        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        public long? ImageHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        public long? ImageWidth { get; set; }
    }
}
