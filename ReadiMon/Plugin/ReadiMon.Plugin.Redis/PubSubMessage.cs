// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     PubSub Message.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PubSubMessage<T>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisMessage" /> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="machine">The machine.</param>
		/// <param name="process">The process.</param>
		/// <param name="processId">The process identifier.</param>
		/// <param name="appDomain">The application domain.</param>
		/// <param name="appDomainId">The application domain identifier.</param>
		/// <param name="dateTime">The date time.</param>
		/// <param name="size">The size.</param>
		/// <param name="hostTenantId">The host tenant identifier.</param>
		/// <param name="message">The message.</param>
		public PubSubMessage( string channel, string machine, string process, long processId, string appDomain, long appDomainId, DateTime dateTime, long size, long hostTenantId, T message )
		{
			Channel = channel;
			Machine = machine;
			Process = process;
			ProcessId = processId;
			AppDomain = appDomain;
			AppDomainId = appDomainId;
			DateTime = dateTime;
			Message = message;

			Received = DateTime.Now.ToString( "dd/MM/yyyy hh:mm:ss.fff tt" );
			Size = ( int ) size;
			HostTenantId = hostTenantId;
		}

		/// <summary>
		///     Gets or sets the application domain.
		/// </summary>
		/// <value>
		///     The application domain.
		/// </value>
		public string AppDomain
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the application domain identifier.
		/// </summary>
		/// <value>
		///     The application domain identifier.
		/// </value>
		public long AppDomainId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		public string Channel
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the compression rate.
		/// </summary>
		/// <value>
		///     The compression rate.
		/// </value>
		public string CompressionRate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the date time.
		/// </summary>
		/// <value>
		///     The date time.
		/// </value>
		public DateTime DateTime
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the host tenant identifier.
		/// </summary>
		/// <value>
		///     The host tenant identifier.
		/// </value>
		public long HostTenantId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the machine.
		/// </summary>
		/// <value>
		///     The machine.
		/// </value>
		public string Machine
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the message.
		/// </summary>
		/// <value>
		///     The message.
		/// </value>
		public T Message
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the process.
		/// </summary>
		/// <value>
		///     The process.
		/// </value>
		public string Process
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the process identifier.
		/// </summary>
		/// <value>
		///     The process identifier.
		/// </value>
		public long ProcessId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the received.
		/// </summary>
		/// <value>
		///     The received.
		/// </value>
		public string Received
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the size.
		/// </summary>
		/// <value>
		///     The size.
		/// </value>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tool-tip.
		/// </summary>
		/// <value>
		///     The tool-tip.
		/// </value>
		public virtual string Tooltip
		{
			get
			{
				return string.Format( "Sent: {0}\nMachine: {1}\nProcess Id: {2}\nAppDomain Id: {3}\nHost Tenant Id: {4}\nTransmission Size: {5}", DateTime.ToString( "dd/MM/yyyy hh:mm:ss.fff tt" ), Machine, ProcessId, AppDomainId, HostTenantId, TransmissionSize );
			}
		}

		/// <summary>
		///     Gets the size of the transmission.
		/// </summary>
		/// <value>
		///     The size of the transmission.
		/// </value>
		public int TransmissionSize
		{
			get;
			set;
		}
	}
}