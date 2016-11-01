// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure
{
	/// <summary>
	///     Migration failure base class.
	/// </summary>
	public abstract class MigrationFailure
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MigrationFailure" /> class.
		/// </summary>
		/// <param name="level">The level.</param>
		protected MigrationFailure( FailureLevel level )
		{
			Level = level;
		}

		/// <summary>
		///     Gets or sets the level.
		/// </summary>
		/// <value>
		///     The level.
		/// </value>
		public FailureLevel Level
		{
			get;
			private set;
		}
	}
}