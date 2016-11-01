// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Solution identity details
    /// </summary>
    public interface ISolutionDetails
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        string Version { get; set; }
    }
}
