// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    public class ReportColumnValueFormat
    {
        public bool HideDisplayValue { get; set; }

        public bool DisableDefaultFormat { get; set; }
        /// <summary>
        /// Gets or sets the value text alignment.
        /// </summary>
        /// <value>the value text alignment.</value>
        public string Alignment { get; set; }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>The suffix.</value>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the decimal places.
        /// </summary>
        /// <value>The decimal places.</value>
        public long? DecimalPlaces { get; set; }

        /// <summary>
        /// Gets or sets the date time format.
        /// </summary>
        /// <value>The date time format.</value>
        public string DateTimeFormat { get; set; }       

        /// <summary>
        /// Gets or sets the number of lines.
        /// </summary>
        /// <value>The number of lines.</value>
        public long? NumberOfLines { get; set; }

        // Image Handling
        /// <summary>
        /// Gets or sets the image scale unique identifier.
        /// </summary>
        /// <value>The image scale unique identifier.</value>
        public long? ImageScaleId { get; set; }

        /// <summary>
        /// Gets or sets the image size unique identifier.
        /// </summary>
        /// <value>The image size unique identifier.</value>
        public long? ImageSizeId { get; set; }

        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        /// <value>The height of the image in pixels.</value>
        public long? ImageHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image in pixels.</value>
        public long? ImageWidth { get; set; }

        /// <summary>
        /// Gets or sets the entity list format.
        /// </summary>
        /// <value>
        /// The entity list format.
        /// </value>
        public string EntityListColumnFormat { get; set; }
    }
}
