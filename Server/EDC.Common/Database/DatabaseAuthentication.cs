// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Database
{
	/// <summary>
	///     Describes the database authentication modes.
	/// </summary>
	[Serializable]
	public enum DatabaseAuthentication
	{
		/// <summary>
		///     The database provider is unknown.
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     Authentication is integrated with the operating system.
		/// </summary>
		Integrated = 1,

		/// <summary>
		///     Authentication is integrated with the database.
		/// </summary>
		Database = 2
	}
}