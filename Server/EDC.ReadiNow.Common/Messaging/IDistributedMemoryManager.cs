// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     IDistributedMemoryManager interface.
	/// </summary>
	public interface IDistributedMemoryManager : IDisposable
	{
		/// <summary>
		///     Gets a value indicating whether this instance is connected.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is connected; otherwise, <c>false</c>.
		/// </value>
		bool IsConnected
		{
			get;
		}

		/// <summary>
		///     Connects this instance.
		/// </summary>
		/// <param name="allowAdmin">if set to <c>true</c> [allow admin].</param>
		void Connect( bool allowAdmin = false );

		/// <summary>
		///     Disconnects this instance.
		/// </summary>
		void Disconnect( );

		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <param name="channelName">The channel name.</param>		
		/// <returns></returns>
		IChannel<T> GetChannel<T>( string channelName );

		/// <summary>
		///     Gets the memory store.
		/// </summary>
		/// <returns></returns>
		IMemoryStore GetMemoryStore( );

		/// <summary>
		///     Suppresses messages from being published while the ISuppression is active.
		/// </summary>
		/// <returns></returns>
		ISuppression Suppress( );
	}
}