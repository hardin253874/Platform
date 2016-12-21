// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Diagnostics
{
	/// <summary>
	///     Rotate Details class.
	/// </summary>
	public class RotateDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RotateDetails" /> class.
		/// </summary>
		/// <param name="newFilename">The new filename.</param>
		public RotateDetails( string newFilename )
		{
			NewFilename = newFilename;
		}

		/// <summary>
		///     Gets the new filename.
		/// </summary>
		/// <value>
		///     The new filename.
		/// </value>
		public string NewFilename
		{
			get;
		}
	}
}