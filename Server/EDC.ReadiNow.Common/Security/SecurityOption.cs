// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Security Options
	/// </summary>
	[Flags]
	public enum SecurityOption
	{
		/// <summary>
		///     Skip (do not load or return) entities the user does not have the requested access to.
		/// </summary>
		SkipDenied = 1,

		/// <summary>
		///     Throw a <see cref="PlatformSecurityException"/> if they user lacks the request access
		///     to all requested entities.
		/// </summary>
		DemandAll = 2
	}
}