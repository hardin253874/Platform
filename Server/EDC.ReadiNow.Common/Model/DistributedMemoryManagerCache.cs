// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Cache;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	/// Distributed Memory Manager cache class.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <typeparam name="TMessage">The type of the message.</typeparam>
	/// <remarks>
	/// This is temporary until the new distributed memory model become available.
	/// </remarks>
	public abstract class DistributedMemoryManagerCache<TKey, TValue, TMessage> : Cache<TKey, TValue>
	{
		/// <summary>
		///		Disposed
		/// </summary>
		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="DistributedMemoryManagerCache{TKey, TValue, TMessage}"/> class.
		/// </summary>
		/// <param name="cacheName">Name of the cache.</param>
		protected DistributedMemoryManagerCache( string cacheName )
			: base( cacheName )
		{
			CacheName = cacheName;

			Removed += DistributedMemoryManagerCache_Removed;
			Cleared += DistributedMemoryManagerCache_Cleared;

			IChannel<TMessage> channel = Entity.DistributedMemoryManager.GetChannel<TMessage>( CacheName );
			channel.MessageReceived += Channel_MessageReceived;
			channel.Subscribe( );

			MessageChannel = channel;
		}

		/// <summary>
		///     Gets or sets the name of the cache.
		/// </summary>
		/// <value>
		///     The name of the cache.
		/// </value>
		private string CacheName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the message channel.
		/// </summary>
		/// <value>
		///     The message channel.
		/// </value>
		public IChannel<TMessage> MessageChannel
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the message received action.
		/// </summary>
		/// <value>
		///     The message received action.
		/// </value>
		public Action<MessageEventArgs<TMessage>> MessageReceivedAction
		{
			get;
			set;
		}

		/// <summary>
		/// Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MessageEventArgs{TMessage}"/> instance containing the event data.</param>
		protected abstract void Channel_MessageReceived( object sender, MessageEventArgs<TMessage> e );

		/// <summary>
		/// </summary>
		/// <param name="disposing"></param>
		/// {lease both managed and unmanaged resources;
		/// <c>false</c>
		/// to}
		protected override void Dispose( bool disposing )
		{
			if ( _disposed )
			{
				return;
			}

			if ( disposing )
			{
				if ( MessageChannel != null )
				{
					MessageChannel.MessageReceived -= Channel_MessageReceived;
					MessageChannel.Dispose( );
					MessageChannel = null;
				}
			}

			_disposed = true;

			base.Dispose( disposing );
		}

		/// <summary>
		///     Handles the Cleared event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CacheEventArgs" /> instance containing the event data.</param>
		protected abstract void DistributedMemoryManagerCache_Cleared( object sender, CacheEventArgs e );

		/// <summary>
		///     Handles the Removed event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyCacheEventArgs{TKey}" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		protected abstract void DistributedMemoryManagerCache_Removed( object sender, KeyCacheEventArgs<TKey> e );
	}
}