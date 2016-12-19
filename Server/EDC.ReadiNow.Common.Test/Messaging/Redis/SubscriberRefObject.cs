// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Threading;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
	/// <summary>
	///     Subscriber Ref Object.
	/// </summary>
	public class SubscriberRefObject<T> : MarshalByRefObject, IDisposable
	{
		/// <param name="channelName">Name of the channel.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="filter">The filter.</param>
		public SubscriberRefObject( string channelName, string tenantName, Func<MessageEventArgs<T>, bool> filter )
		{
			Context = new TenantAdministratorContext( tenantName );

			Filter = filter;

			Manager = new RedisManager( );
			Manager.Connect( );

			ReceivedMessages = new List<MessageEventArgs<T>>( );
			MessageReceived = new AutoResetEvent( false );

			Channel = Manager.GetChannel<T>( channelName );

			Channel.MessageReceived += Handler;
			Channel.Subscribe( );

			/////
			// Allow time for the server to receive the message and process it.
			/////
			Thread.Sleep( 1000 );
		}

		/// <summary>
		///     Gets or sets the filter.
		/// </summary>
		/// <value>
		///     The filter.
		/// </value>
		public Func<MessageEventArgs<T>, bool> Filter
		{
			get;
			set;
		}

		/// <summary>
		///     The message received event
		/// </summary>
		public AutoResetEvent MessageReceived
		{
			get;
		}

		/// <summary>
		///     Gets the received messages.
		/// </summary>
		/// <value>
		///     The received messages.
		/// </value>
		public List<MessageEventArgs<T>> ReceivedMessages
		{
			get;
		}

		/// <summary>
		///     Gets or sets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		private IChannel<T> Channel
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		private TenantAdministratorContext Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the manager.
		/// </summary>
		/// <value>
		///     The manager.
		/// </value>
		private IDistributedMemoryManager Manager
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Channel != null )
			{
				Channel.MessageReceived -= Handler;
				Channel.Dispose( );
				Channel = null;
			}

			if ( Manager != null )
			{
				Manager.Dispose( );
				Manager = null;
			}

			if ( Context != null )
			{
				Context.Dispose( );
				Context = null;
			}
		}

		/// <summary>
		///     Unsubscribe from the channel.
		/// </summary>
		public void Unsubscribe( )
		{
			Channel.Unsubscribe( );
		}

		/// <summary>
		///     Handlers the specified sender.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="MessageEventArgs{T}" /> instance containing the event data.</param>
		private void Handler( object sender, MessageEventArgs<T> eventArgs )
		{
			ReceivedMessages.Add( eventArgs );

			if ( Filter != null )
			{
				if ( Filter( eventArgs ) )
				{
					MessageReceived.Set( );
				}
			}
			else
			{
				MessageReceived.Set( );
			}
		}
	}
}