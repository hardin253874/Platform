// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Expressions.NameResolver
{
	/// <summary>
	///     Reason why the member name is null.
	/// </summary>
	public enum NullMemberNameReason
	{
		/// <summary>
		///     Unknown reason.
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     The member name does not exist.
		/// </summary>
		Missing = 1,

		/// <summary>
		///     Duplicate members with the same name exist.
		/// </summary>
		Duplicate = 2
	}
}