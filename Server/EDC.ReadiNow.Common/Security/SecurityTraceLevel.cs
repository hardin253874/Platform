// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Trace level
	/// </summary>
	public enum SecurityTraceLevel
	{
		/// <summary>
		///     No logging.
		/// </summary>
		None = 0,

		/// <summary>
		///     Only log Denies with basic output.
		/// </summary>
		DenyBasic = 1,

		/// <summary>
		///     Only log Denies with verbose output.
		/// </summary>
		DenyVerbose = 2,

		/// <summary>
		///     Log all demands with basic output.
		/// </summary>
		AllBasic = 3,

		/// <summary>
		///     Log all demands with verbose output.
		/// </summary>
		AllVerbose = 4
	}
}