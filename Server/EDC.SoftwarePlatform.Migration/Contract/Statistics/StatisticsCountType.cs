// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics
{
	public enum StatisticsCountType
	{
		/// <summary>
		///     The previous application count.
		/// </summary>
		PreviousApplication = 0,

		/// <summary>
		///     The current application count.
		/// </summary>
		CurrentApplication = 1,

		/// <summary>
		///     The added count.
		/// </summary>
		Added = 2,

		/// <summary>
		///     The removed count.
		/// </summary>
		Removed = 3,

		/// <summary>
		///     The updated count.
		/// </summary>
		Updated = 4,

		/// <summary>
		///     The unchanged count.
		/// </summary>
		Unchanged = 5,

		/// <summary>
		///     The copied count.
		/// </summary>
		Copied = 6,

		/// <summary>
		///     The dropped count.
		/// </summary>
		Dropped = 7,

		/// <summary>
		///     The executed count.
		/// </summary>
		Executed = 8,

		/// <summary>
		///     The miscellaneous count.
		/// </summary>
		Miscellaneous = 9
	}
}