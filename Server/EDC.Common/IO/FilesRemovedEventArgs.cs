// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.IO
{
	/// <summary>
	///     Files Removed Event Arguments class.
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	public class FilesRemovedEventArgs : EventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="FilesRemovedEventArgs" /> class.
		/// </summary>
		/// <param name="oldFiles">The old files.</param>
		/// <param name="staleFiles">The stale files.</param>
		public FilesRemovedEventArgs( IList<string> oldFiles, IList<string> staleFiles )
		{
			OldFiles = oldFiles ?? new List<string>( );
			StaleFiles = staleFiles ?? new List<string>( );
		}

		/// <summary>
		///     Gets the old files.
		/// </summary>
		/// <value>
		///     The old files.
		/// </value>
		public IList<string> OldFiles
		{
			get;
			private set;
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
			private set;
		}
	}
}