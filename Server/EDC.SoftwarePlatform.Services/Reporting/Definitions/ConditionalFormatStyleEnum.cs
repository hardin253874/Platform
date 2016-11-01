// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Reporting.Definitions
{
    // Common to request and result

    /// <summary>
    /// Method of conditional formatting.
    /// </summary>
    /// <remarks>
    ///     Setting of <see cref="ConditionalFormatStyleEnum"/>.
    /// </remarks>
    public enum ConditionalFormatStyleEnum
    {
        /// <summary>
        /// Select a colour based on multiple rules.
        /// </summary>
        Highlight,

        /// <summary>
        /// Select an icon based on multiple rules.
        /// </summary>
        Icon,

        /// <summary>
        /// Show a proportionally sized bar.
        /// </summary>
        ProgressBar
    }
}
