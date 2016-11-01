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

			EventHandler<MessageEventArgs<T>> eventHandler = ( o, m ) =>
			{
				ReceivedMessages.Add( m );

				if ( Filter != null )
				{
					if ( Filter( m ) )
					{
						MessageReceived.Set( );
					}
				}
				else
				{
					MessageReceived.Set( );
				}
			};

			Channel = Manager.GetChannel<T>( channelName );

			Channel.MessageReceived += eventHandler;
			Channel.Subscribe( );

			/////
			// Allow time for the server to receive the message and process it.
			/////
			Thread.Sleep( 1000 );
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
		///     The message received event
		/// </summary>
		public AutoResetEvent MessageReceived
		{
			get;
			private set;
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
			private set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Channel != null )
			{
				Channel.Dispose( );
			}

			if ( Manager != null )
			{
				Manager.Dispose( );
			}

			if ( Context != null )
			{
				Context.Dispose( );
			}
		}

		/// <summary>
		///     Unsubscribe from the channel.
		/// </summary>
		public void Unsubscribe( )
		{
			Channel.Unsubscribe( );
		}
	}
}