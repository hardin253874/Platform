// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace ApplicationManager.Support
{
	/// <summary>
	///     Application Options.
	/// </summary>
	public enum ApplicationAction
	{
		/// <summary>
		///     Delete application.
		/// </summary>
		Delete,

		/// <summary>
		///     Deploy application to tenant.
		/// </summary>
		Deploy,

		/// <summary>
		///     Repairs an application.
		/// </summary>
		Repair,

		/// <summary>
		///     Export application to SQLite.
		/// </summary>
		Export
	}
}