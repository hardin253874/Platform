// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Log Written Event Arguments class.
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	public class LogWrittenEventArgs : EventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="LogWrittenEventArgs" /> class.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="entriesWritten">The entries written.</param>
		/// <param name="rotateDetails">The rotate details.</param>
		/// <param name="purgeDetails">The purge details.</param>
		public LogWrittenEventArgs( string filename, int entriesWritten, RotateDetails rotateDetails = null, PurgeDetails purgeDetails = null )
		{
			Filename = filename;
			EntriesWritten = entriesWritten;

			if ( !string.IsNullOrEmpty( rotateDetails?.NewFilename ) )
			{
				RotateDetails = rotateDetails;
			}

			if ( purgeDetails != null && ( ( purgeDetails.StaleFiles != null && purgeDetails.StaleFiles.Count > 0 ) || ( purgeDetails.OverflowFiles != null && purgeDetails.OverflowFiles.Count > 0 ) ) )
			{
				PurgeDetails = purgeDetails;
			}
		}

		/// <summary>
		///     Gets the entries written.
		/// </summary>
		/// <value>
		///     The entries written.
		/// </value>
		public int EntriesWritten
		{
			get;
		}

		/// <summary>
		///     Gets the filename.
		/// </summary>
		/// <value>
		///     The filename.
		/// </value>
		public string Filename
		{
			get;
		}

		/// <summary>
		///     Gets the purge details.
		/// </summary>
		/// <value>
		///     The purge details.
		/// </value>
		public PurgeDetails PurgeDetails
		{
			get;
		}

		/// <summary>
		///     Gets the rotate details.
		/// </summary>
		/// <value>
		///     The rotate details.
		/// </value>
		public RotateDetails RotateDetails
		{
			get;
		}
	}
}