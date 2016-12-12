// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;
using System.Threading.Tasks;
using EDC.IO;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.CacheInvalidation;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis channel.
	/// </summary>
	public class Channel<T> : IChannel<T>
	{
		/// <summary>
		///     Whether this instance has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisChannel" /> class.
		/// </summary>
		/// <param name="channelName">Name of the channel.</param>
		/// <param name="subscriber">The subscriber.</param>
		/// <exception cref="System.ArgumentNullException">channel</exception>
		public Channel( string channelName, ISubscriber subscriber )
		{
			if ( string.IsNullOrEmpty( channelName ) )
			{
				throw new ArgumentNullException( "channelName" );
			}

			if ( subscriber == null )
			{
				throw new ArgumentNullException( "subscriber" );
			}

			ChannelName = channelName;
			Subscriber = subscriber;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="Channel{T}" /> is subscribed.
		/// </summary>
		/// <value>
		///     <c>true</c> if subscribed; otherwise, <c>false</c>.
		/// </value>
		private bool Subscribed
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the subscriber.
		/// </summary>
		/// <value>
		///     The subscriber.
		/// </value>
		private ISubscriber Subscriber
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		public string ChannelName
		{
			get;
			private set;
		}

		/// <summary>
		///     Subscribe to this instance.
		/// </summary>
		public void Subscribe( )
		{
			Subscriber.Subscribe( ChannelName, MessageHandler );

			Subscribed = true;
		}

		/// <summary>
		///     Unsubscribe from this instance.
		/// </summary>
		public void Unsubscribe( )
		{
			Subscriber.Unsubscribe( ChannelName, MessageHandler );

			Subscribed = false;
		}

		/// <summary>
		///     Occurs when a message is received.
		/// </summary>
		public event EventHandler<MessageEventArgs<T>> MessageReceived;

		/// <summary>
		///     Publishes the message asynchronously.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c>, the message will be published to the originator also.</param>
		/// <param name="mergeAction">The merge action.</param>
		/// <returns></returns>
		public Task<long> PublishAsync( T message, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
		{
			return PublishAsync( message, PublishMethod.Deferred, options, publishToOriginator, mergeAction );
		}

		/// <summary>
		///     Publishes the specified action.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c>, the message will be published to the originator also.</param>
		/// <param name="mergeAction">The merge action.</param>
		public void Publish( T message, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
		{
			Publish( message, PublishMethod.Deferred, options, publishToOriginator, mergeAction );
		}

		/// <summary>
		///     Publishes the specified action.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="publishMethod">The publishMethod.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c>, the message will be published to the originator also.</param>
		/// <param name="mergeAction">The merge action.</param>
		public void Publish( T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
		{
			Publish_Impl( ( msg, opt, originator ) =>
			{
				var pubResult = PublishMessage( ( subscriber, bytes, flags ) =>
				{
					Interlocked.Increment( ref MessagesPublished );
					subscriber.Publish( ChannelName, bytes, flags );
					return null;
				}, message, options, publishToOriginator );

				return pubResult;
			}, message, publishMethod, options, publishToOriginator, mergeAction );
		}

		/// <summary>
		///     Publishes the message asynchronously.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="publishMethod">The publish method.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c>, the message will be published to the originator also.</param>
		/// <param name="mergeAction">The merge action.</param>
		/// <returns></returns>
		public Task<long> PublishAsync( T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
		{
			var result = Publish_Impl( ( msg, opt, originator ) =>
			{
				var pubResult = PublishMessage( ( subscriber, bytes, flags ) =>
				{
					var innerResult = subscriber.PublishAsync( ChannelName, bytes, flags );
					return innerResult;
				}, message, options, publishToOriginator ) ?? Task.FromResult<long>( 0 );

				return pubResult;
			}, message, publishMethod, options, publishToOriginator, mergeAction ) ?? Task.FromResult<long>( 0 );

			return result;
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( _disposed )
			{
				return;
			}

			if ( disposing )
			{
				if ( Subscribed )
				{
					Unsubscribe( );
				}
			}

			_disposed = true;
		}

		public static int MessagesPublished;
		public static int MessagesReceived;

		/// <summary>
		///     Messages the handler.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="message">The message.</param>
		private void MessageHandler( RedisChannel channel, RedisValue message )
		{
			Interlocked.Increment( ref MessagesReceived );
			try
			{
				byte[ ] decompress = CompressionHelper.Decompress( message );

				ChannelMessage<T> channelMessage = ChannelMessage<T>.Deserialize( decompress );

				if ( channelMessage != null && ( channelMessage.PublishToOriginator || ! channelMessage.IsMessageOriginator( ) ) )
				{
					using ( new TenantAdministratorContext( channelMessage.HostTenantId ) )
					using ( new RedisCacheMemoryStoreSuppressionContext( ) )
					{
						var eventArgs = new MessageEventArgs<T>( channel, channelMessage );

						OnMessageReceived( eventArgs );
					}
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( exc.ToString( ) );
			}
		}

		/// <summary>
		///     Raises the <see cref="E:MessageReceived" /> event.
		/// </summary>
		/// <param name="args">The <see cref="MessageEventArgs{T}" /> instance containing the event data.</param>
		protected virtual void OnMessageReceived( MessageEventArgs<T> args )
		{
			EventHandler<MessageEventArgs<T>> handler = MessageReceived;

			if ( handler != null )
			{
				handler( this, args );
			}
		}

		/// <summary>
		///     Publishes the message.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="message">The message.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c>, the message will be published to the originator also.</param>
		/// <returns></returns>
		private Task<long> PublishMessage( Func<ISubscriber, byte[ ], CommandFlags, Task<long>> callback, T message, PublishOptions options, bool publishToOriginator = false )
		{
			if ( Equals( message, default( T ) ) )
			{
				return null;
			}

			if ( Suppression.IsActive( ) )
			{
				return null;
			}

			ISubscriber subscriber = Subscriber;

			if ( subscriber != null )
			{
				var payload = new ChannelMessage<T>( message, publishToOriginator );

				if ( subscriber.Multiplexer.IsConnected )
				{
					try
					{
						var flags = CommandFlags.None;

						if ( options == PublishOptions.FireAndForget )
						{
							flags = CommandFlags.FireAndForget;
						}

						byte[ ] serialize = ChannelMessage<T>.Serialize( payload );

						byte[ ] compress = CompressionHelper.Compress( serialize );

						return callback( subscriber, compress, flags );
					}
					catch ( StackExchange.Redis.RedisConnectionException exc )
					{
						EventLog.Application.WriteWarning( "Redis appears to be down. {0}", exc );
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Publish_s the implementation.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="message">The message.</param>
		/// <param name="publishMethod">The publish method.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
		/// <param name="mergeAction">The merge action.</param>
		/// <returns></returns>
		private Task<long> Publish_Impl( Func<T, PublishOptions, bool, Task<long>> callback, T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
		{
			if ( Equals( message, default( T ) ) )
			{
				return null;
			}

			if ( Suppression.IsActive( ) )
			{
				return null;
			}

			DeferredChannelMessageContext deferredMessageContext = null;

			try
			{
				/////
				// If running in a delayed context, use the current message store.
				/////
				if ( publishMethod == PublishMethod.Deferred )
				{
					deferredMessageContext = DeferredChannelMessageContext.GetContext( );

					if ( deferredMessageContext.ContextType != ContextType.Attached && !DeferredChannelMessageContext.SuppressNoContextWarning)
					{
						EventLog.Application.WriteWarning( "Channel {0} has been created with PublishMethod.Deferred with no DeferredChannelMessageContext set. Messages will be sent immediately. Either set a DeferredChannelMessageContext or publish the message with PublishMethod.Immediate.", ChannelName );
					}
				}

				if ( deferredMessageContext != null &&
				     deferredMessageContext.ContextType == ContextType.Attached )
				{
					deferredMessageContext.AddOrUpdateMessage( ChannelName, message, mergeAction );
				}
				else
				{
					return callback( message, options, publishToOriginator );
				}
			}
			finally
			{
				if ( deferredMessageContext != null )
				{
					deferredMessageContext.Dispose( );
				}
			}

			return null;
		}        
	}
}