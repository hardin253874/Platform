// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Purge Details class.
	/// </summary>
	public class PurgeDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PurgeDetails" /> class.
		/// </summary>
		/// <param name="staleFiles">The stale files.</param>
		/// <param name="overflowFiles">The overflow files.</param>
		public PurgeDetails( IList<string> staleFiles, IList<string> overflowFiles )
		{
			StaleFiles = staleFiles;
			OverflowFiles = overflowFiles;
		}

		/// <summary>
		///     Gets the overflow files.
		/// </summary>
		/// <value>
		///     The overflow files.
		/// </value>
		public IList<string> OverflowFiles
		{
			get;
		}

		/// <summary>
		///     Gets the stale files.
		/// </summary>
		/// <value>
		///     The stale files.
		/// </value>
		public IList<string> StaleFiles
		{
			get;
		}
	}
}