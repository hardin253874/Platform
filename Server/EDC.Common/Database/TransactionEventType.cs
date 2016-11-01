// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Database
{
	/// <summary>
	/// </summary>
	public enum TransactionEventType
	{
		/// <summary>
		/// Unknown transaction event type.
		/// </summary>
		Unknown,


		/// <summary>
		/// Commit transaction event type.
		/// </summary>
		Commit,


		/// <summary>
		/// Rollback transaction event type.
		/// </summary>
		Rollback
	}
}