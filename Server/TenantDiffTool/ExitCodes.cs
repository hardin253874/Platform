// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace TenantDiffTool
{
	/// <summary>
	/// </summary>
	internal static class ExitCodes
	{
		/// <summary>
		///     An error has occurred.
		/// </summary>
		public const int Error = -1;


		/// <summary>
		///     The data sources are identical.
		/// </summary>
		public const int Identical = 0;


		/// <summary>
		///     The data sources are different.
		/// </summary>
		public const int Different = 1;
	}
}