// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     MessageEventArgs class.
	/// </summary>
	[Serializable]
	public class MessageEventArgs<T> : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MessageEventArgs{T}" /> class.
		/// </summary>
		/// <param name="channelName">Name of the channel.</param>
		/// <param name="message">The message.</param>
		/// <exception cref="System.ArgumentNullException">
		/// channelName
		/// or
		/// message
		/// </exception>
		public MessageEventArgs( string channelName, ChannelMessage<T> message )
		{
			if ( string.IsNullOrEmpty( channelName ) )
			{
				throw new ArgumentNullException( "channelName" );
			}

			if ( message == null )
			{
				throw new ArgumentNullException( "message" );
			}

			ChannelName = channelName;
			MachineName = message.MachineName;
			ProcessName = message.ProcessName;
			ProcessId = message.ProcessId;
			AppDomainName = message.AppDomainName;
			AppDomainId = message.AppDomainId;
			Date = message.Date;
			Message = message.Message;
		}

		/// <summary>
		///     Gets the application domain identifier.
		/// </summary>
		/// <value>
		///     The application domain identifier.
		/// </value>
		public int AppDomainId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the application domain.
		/// </summary>
		/// <value>
		///     The name of the application domain.
		/// </value>
		public string AppDomainName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the channel.
		/// </summary>
		/// <value>
		///     The name of the channel.
		/// </value>
		public string ChannelName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the date.
		/// </summary>
		/// <value>
		///     The date.
		/// </value>
		public DateTime Date
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the machine.
		/// </summary>
		/// <value>
		///     The name of the machine.
		/// </value>
		public string MachineName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the message.
		/// </summary>
		/// <value>
		///     The message.
		/// </value>
		public T Message
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the process identifier.
		/// </summary>
		/// <value>
		///     The process identifier.
		/// </value>
		public int ProcessId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the process.
		/// </summary>
		/// <value>
		///     The name of the process.
		/// </value>
		public string ProcessName
		{
			get;
			private set;
		}
	}
}