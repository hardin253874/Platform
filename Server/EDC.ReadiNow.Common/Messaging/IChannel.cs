// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     IChannel interface.
	/// </summary>
	public interface IChannel<T> : IDisposable
	{
		/// <summary>
		///     Gets the channel name.
		/// </summary>
		/// <value>
		///     The channel name.
		/// </value>
		string ChannelName
		{
			get;
		}        

        /// <summary>
        ///     Publishes the messages.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="publishMethod">The publishMethod.</param>
        /// <param name="options">The options.</param>
        /// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
        /// <param name="mergeAction">The merge action.</param>
        void Publish( T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null );

		/// <summary>
		/// Publishes the message asynchronously.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="publishMethod">The publish method.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
		/// <param name="mergeAction">The merge action.</param>
		/// <returns></returns>
		Task<long> PublishAsync( T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null );

        /// <summary>
        ///     Publishes the messages.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="options">The options.</param>
        /// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
        /// <param name="mergeAction">The merge action.</param>
        void Publish(T message, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null);

		/// <summary>
		///     Publishes the message asynchronously.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="options">The options.</param>
		/// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
		/// <param name="mergeAction">The merge action.</param>
		Task<long> PublishAsync( T message, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null );

        /// <summary>
        ///     Subscribe to this instance.
        /// </summary>
        void Subscribe( );

		/// <summary>
		///     Unsubscribe from this instance.
		/// </summary>
		void Unsubscribe( );

		/// <summary>
		///     Occurs when a message is received.
		/// </summary>
		event EventHandler<MessageEventArgs<T>> MessageReceived;
	}
}