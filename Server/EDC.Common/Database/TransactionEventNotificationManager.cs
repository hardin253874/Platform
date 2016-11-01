// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Transactions;

namespace EDC.Database
{
	/// <summary>
	///     This class is intended to be used by classes that wish to be transaction
	///     aware. Transaction aware class will be notified when a transaction
	///     event occurs.
	/// </summary>
	public class TransactionEventNotificationManager
	{
		#region Constants

		/// <summary>
		///     The default timeout in seconds, 1 day.
		/// </summary>
		private const int DefaultTimeOutIntervalSeconds = 24 * 60 * 60;

		#endregion

		#region Fields

		/// <summary>
		///     The class which is to receive event notifications.
		/// </summary>
		private readonly ITransactionEventNotification _eventNotification;

		/// <summary>
		///     The sync root for this object.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     The timeout interval in seconds.
		/// </summary>
		private readonly int _timeOutIntervalSeconds;


		/// <summary>
		///     The dictionary of transaction event notifiers. The key to the
		///     dictionary is the LocalIdentifier of the transaction.
		/// </summary>
		private readonly Dictionary<string, TransactionEventNotifier> _transactionEventNotifiers = new Dictionary<string, TransactionEventNotifier>( );

		#endregion

		#region Constructor

		/// <summary>
		///     Creates a new CacheTransactionEventNotificationManager.
		/// </summary>
		/// <param name="eventNotification">The event notification.</param>
		public TransactionEventNotificationManager( ITransactionEventNotification eventNotification )
			: this( eventNotification, DefaultTimeOutIntervalSeconds )
		{
		}


		/// <summary>
		///     Creates a new CacheTransactionEventNotificationManager.
		/// </summary>
		/// <param name="eventNotification">The event notification.</param>
		/// <param name="timeOutIntervalSeconds">
		///     Any transactions that take longer that
		///     this will have their event notifiers removed.
		/// </param>
		internal TransactionEventNotificationManager( ITransactionEventNotification eventNotification, int timeOutIntervalSeconds )
		{
			if ( eventNotification == null )
			{
				throw new ArgumentNullException( "eventNotification" );
			}

			_eventNotification = eventNotification;
			_timeOutIntervalSeconds = timeOutIntervalSeconds;
		}

		#endregion

		#region Public Methods        

		/// <summary>
		///     This method clears all the registered event notifiers.
		/// </summary>
		public void ClearTransactionEventNotifiers( )
		{
			lock ( _syncRoot )
			{
				foreach ( TransactionEventNotifier notifier in _transactionEventNotifiers.Values )
				{
					notifier.Enabled = false;
				}
				_transactionEventNotifiers.Clear( );
			}
		}

		/// <summary>
		///     Enlists the specified cache entry as being modified during a transaction.
		/// </summary>
		/// <param name="transaction">The current transaction</param>
		public void EnlistTransaction( Transaction transaction )
		{
			lock ( _syncRoot )
			{
				TransactionEventNotifier eventNotifier;
				if ( transaction != null &&
				     !_transactionEventNotifiers.TryGetValue( transaction.TransactionInformation.LocalIdentifier, out eventNotifier ) )
				{
					eventNotifier = new TransactionEventNotifier( this, transaction );
					_transactionEventNotifiers[ transaction.TransactionInformation.LocalIdentifier ] = eventNotifier;
					transaction.EnlistVolatile( eventNotifier, EnlistmentOptions.None );
				}
			}
			RemoveTimedOutTransactionEventNotifiers( );
		}

		#endregion

		#region Non-Public Methods               

		/// <summary>
		///     Called when the enlisted transaction commits.
		/// </summary>
		/// <param name="transactionId"></param>
		private void Commit( string transactionId )
		{
			if ( string.IsNullOrEmpty( transactionId ) )
			{
				throw new ArgumentNullException( "transactionId" );
			}

			var args = new TransactionEventNotificationArgs( transactionId, TransactionEventType.Commit );
			_eventNotification.OnTransactionEvent( args );
		}


		/// <summary>
		///     This method removes any event notifiers that have timed out.
		/// </summary>
		private void RemoveTimedOutTransactionEventNotifiers( )
		{
			var notifiersToRemove = new List<string>( );

			lock ( _syncRoot )
			{
				foreach ( var kvp in _transactionEventNotifiers )
				{
					string id = kvp.Key;
					TransactionEventNotifier eventNotifier = kvp.Value;

					if ( eventNotifier.HasTimedOut( _timeOutIntervalSeconds ) )
					{
						eventNotifier.Enabled = false;
						notifiersToRemove.Add( id );
					}
				}
			}

			foreach ( string id in notifiersToRemove )
			{
				Rollback( id );

				lock ( _syncRoot )
				{
					_transactionEventNotifiers.Remove( id );
				}
			}
		}

		/// <summary>
		///     This method removes the transaction event notifier with the
		///     specified transaction id.
		/// </summary>
		/// <param name="transactionId">
		///     The transaction id of the event
		///     notifier to remove.
		/// </param>
		private void RemoveTransactionEventNotifier( string transactionId )
		{
			if ( string.IsNullOrEmpty( transactionId ) )
			{
				throw new ArgumentNullException( "transactionId" );
			}

			lock ( _syncRoot )
			{
				_transactionEventNotifiers.Remove( transactionId );
			}
		}

		/// <summary>
		///     Called when the enlisted transaction rolls back.
		/// </summary>
		/// <param name="transactionId"></param>
		private void Rollback( string transactionId )
		{
			if ( string.IsNullOrEmpty( transactionId ) )
			{
				throw new ArgumentNullException( "transactionId" );
			}

			var args = new TransactionEventNotificationArgs( transactionId, TransactionEventType.Rollback );
			_eventNotification.OnTransactionEvent( args );
		}

		#endregion

		/// <summary>
		///     This class is responsible for receiving notifications about
		///     transaction events e.g. Commit, Rollback.
		/// </summary>
		private class TransactionEventNotifier : IEnlistmentNotification
		{
			#region Properties

			private bool _enabled = true;

			/// <summary>
			///     True if the notifier is enabled, false otherwise.
			/// </summary>
			public bool Enabled
			{
				private get
				{
					lock ( _syncRootInner )
					{
						return _enabled;
					}
				}
				set
				{
					lock ( _syncRootInner )
					{
						_enabled = value;
					}
				}
			}

			#endregion

			#region Fields

			/// <summary>
			///     The notification manager that created this event notifier.
			/// </summary>
			private readonly TransactionEventNotificationManager _notificationManager;


			/// <summary>
			///     The sync root for this class.
			/// </summary>
			private readonly object _syncRootInner = new object( );

			/// <summary>
			///     The transaction creation time.
			/// </summary>
			private readonly DateTime _transactionCreationTime;


			/// <summary>
			///     The transaction id.
			/// </summary>
			private readonly string _transactionId;

			#endregion

			#region Constructors

			/// <summary>
			///     Creates a new TransactionEventNotifier.
			/// </summary>
			/// <param name="notificationManager">The notificationManager.</param>
			/// <param name="transaction">The transaction.</param>
			public TransactionEventNotifier( TransactionEventNotificationManager notificationManager, Transaction transaction )
			{
				if ( notificationManager == null )
				{
					throw new ArgumentNullException( "notificationManager" );
				}

				if ( transaction == null )
				{
					throw new ArgumentNullException( "transaction" );
				}

				_notificationManager = notificationManager;
				_transactionCreationTime = transaction.TransactionInformation.CreationTime;
				_transactionId = transaction.TransactionInformation.LocalIdentifier;
			}

			#endregion

			#region Public Methods

			/// <summary>
			///     Returns true if the event notifier has timed out.
			/// </summary>
			/// <param name="timeoutIntervalSeconds">The timeout in seconds.</param>
			/// <returns>True if the event notifier has timed out, false otherwise.</returns>
			public bool HasTimedOut( int timeoutIntervalSeconds )
			{
				bool hasTimedOut = false;

				TimeSpan diff = DateTime.UtcNow - _transactionCreationTime;
				if ( diff.TotalSeconds >= timeoutIntervalSeconds )
				{
					hasTimedOut = true;
				}

				return hasTimedOut;
			}

			#endregion

			#region IEnlistmentNotification

			/// <summary>
			///     Called by the .NET TransactionManager as part of the two
			///     phase commit. This method should not be called directly.
			/// </summary>
			/// <param name="enlistment"></param>
			public void Commit( Enlistment enlistment )
			{
				enlistment.Done( );

				if ( Enabled )
				{
					_notificationManager.Commit( _transactionId );
					Enabled = false;
				}

				// Remove the event notifier.
				_notificationManager.RemoveTransactionEventNotifier( _transactionId );
			}


			/// <summary>
			///     Called by the .NET TransactionManager as part of the two
			///     phase commit. This method should not be called directly.
			/// </summary>
			/// <param name="enlistment"></param>
			public void InDoubt( Enlistment enlistment )
			{
				enlistment.Done( );

				// Invalidate any cache entries that were updated whilst the
				// transaction was active.
				if ( Enabled )
				{
					_notificationManager.Rollback( _transactionId );
					Enabled = false;
				}

				// Remove the event notifier.
				_notificationManager.RemoveTransactionEventNotifier( _transactionId );
			}


			/// <summary>
			///     Called by the .NET TransactionManager as part of the two
			///     phase commit. This method should not be called directly.
			/// </summary>
			/// <param name="preparingEnlistment"></param>
			public void Prepare( PreparingEnlistment preparingEnlistment )
			{
				preparingEnlistment.Prepared( );
			}


			/// <summary>
			///     Called by the .NET TransactionManager as part of the two
			///     phase commit. This method should not be called directly.
			/// </summary>
			/// <param name="enlistment"></param>
			public void Rollback( Enlistment enlistment )
			{
				enlistment.Done( );

				// Invalidate any cache entries that were updated whilst the
				// transaction was active.
				if ( Enabled )
				{
					_notificationManager.Rollback( _transactionId );
					Enabled = false;
				}

				// Remove the event notifier.
				_notificationManager.RemoveTransactionEventNotifier( _transactionId );
			}

			#endregion
		}
	}
}