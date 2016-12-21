// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Diagnostics
{
	/// <summary>
	///     Validate Details class.
	/// </summary>
	public class ValidateDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ValidateDetails" /> class.
		/// </summary>
		/// <param name="rotateDetails">The rotate details.</param>
		/// <param name="purgeDetails">The purge details.</param>
		public ValidateDetails( RotateDetails rotateDetails, PurgeDetails purgeDetails )
		{
			RotateDetails = rotateDetails;
			PurgeDetails = purgeDetails;
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