// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Database
{
	/// <summary>
	///     An interface for classes that wish to receive transaction event notifications.
	/// </summary>
	public interface ITransactionEventNotification
	{
		/// <summary>
		///     Called when a transaction event occurs.
		/// </summary>
		/// <param name="eventArgs">The transaction event</param>
		void OnTransactionEvent( TransactionEventNotificationArgs eventArgs );
	}
}