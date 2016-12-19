// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    ///     Defines the Import Format type.
    /// </summary>
    [Flags]
    public enum ImportFormat
	{
		/// <summary>
		///     The Excel format type.
		/// </summary>
		Excel = 1,

		/// <summary>
		///     The CSV format type.
		/// </summary>
		CSV = 2,

        /// <summary>
        ///     Tab separated format type.
        /// </summary>
        Tab = 4,

        /// <summary>
        ///     Tab separated format type.
        /// </summary>
        Zip = 256
    }
}