// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Database
{
	/// <summary>
	///     Transaction event notification arguments.
	/// </summary>
	public class TransactionEventNotificationArgs : EventArgs
	{
		/// <summary>
		///     Create a new transaction event notification arguments.
		/// </summary>
		/// <param name="transactionid">The transaction id.</param>
		/// <param name="eventType">The transaction event type.</param>
		public TransactionEventNotificationArgs( string transactionid, TransactionEventType eventType )
		{
			Transactionid = transactionid;
			EventType = eventType;
		}


		/// <summary>
		///     The transaction event type.
		/// </summary>
		public TransactionEventType EventType
		{
			get;
			private set;
		}

		/// <summary>
		///     The transaction id.
		/// </summary>
		public string Transactionid
		{
			get;
			private set;
		}
	}
}