// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    public class ReportConditionColor
    {
        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>The alpha.</value>
        public byte Alpha { get; set; }

        /// <summary>
        /// Gets or sets the red.
        /// </summary>
        /// <value>The red.</value>
        public byte Red { get; set; }

        /// <summary>
        /// Gets or sets the blue.
        /// </summary>
        /// <value>The blue.</value>
        public byte Blue { get; set; }

        /// <summary>
        /// Gets or sets the green.
        /// </summary>
        /// <value>The green.</value>
        public byte Green { get; set; }

        public string ToColourString()
        {
            return $"{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";
        }
    }
}
