// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using EDC.ReadiNow.IO;
using ProtoBuf;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     ChannelMessage class.
	/// </summary>
	[Serializable]
	[ProtoContract]
	public class ChannelMessage<T> : IChannelMessage<T>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="ChannelMessage{T}" /> class from being created.
		/// </summary>
		private ChannelMessage( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ChannelMessage{T}" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
		public ChannelMessage( T message, bool publishToOriginator = false )
			: this( Identity.MachineName, Identity.ProcessName, Identity.ProcessId, Identity.AppDomainName, Identity.AppDomainId, DateTime.Now.Ticks, RequestContext.IsSet ? RequestContext.TenantId : 0, message, publishToOriginator )
		{
		}

		/// <summary>
		///		Initializes a new instance of the <see cref="ChannelMessage{T}" /> class.
		/// </summary>
		/// <param name="machineName">Name of the machine.</param>
		/// <param name="processName">Name of the process.</param>
		/// <param name="processId">The process identifier.</param>
		/// <param name="appDomainName">Name of the application domain.</param>
		/// <param name="appDomainId">The application domain identifier.</param>
		/// <param name="ticks">The ticks.</param>
		/// <param name="hostTenantId">The host tenant identifier.</param>
		/// <param name="message">The message.</param>
		/// <param name="publishToOriginator">if set to <c>true</c> [publish to originator].</param>
		private ChannelMessage( string machineName, string processName, int processId, string appDomainName, int appDomainId, long ticks, long hostTenantId, T message, bool publishToOriginator = false )
			: this( )
		{
			MachineName = machineName;
			ProcessName = processName;
			ProcessId = processId;
			AppDomainName = appDomainName;
			AppDomainId = appDomainId;
			Date = new DateTime( ticks );
			HostTenantId = hostTenantId;
			Message = message;
			PublishToOriginator = publishToOriginator;
		}

		/// <summary>
		///     Gets the application domain identifier.
		/// </summary>
		/// <value>
		///     The application domain identifier.
		/// </value>
		[ProtoMember( 1 )]
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
		[ProtoMember( 2 )]
		public string AppDomainName
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
		[ProtoMember( 3 )]
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
		[ProtoMember( 4 )]
		public string MachineName
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
		[ProtoMember( 5 )]
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
		[ProtoMember( 6 )]
		public string ProcessName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether [publish to originator].
		/// </summary>
		/// <value>
		///     <c>true</c> if [publish to originator]; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 7 )]
		public bool PublishToOriginator
		{
			get;
			private set;
		}

		/// <summary>
		///		Gets the host tenant identifier.
		/// </summary>
		/// <value>
		/// The host tenant identifier.
		/// </value>
		[ProtoMember( 8 )]
		public long HostTenantId
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
		[ProtoMember( 9 )]
		public T Message
		{
			get;
			protected set;
		}

		/// <summary>
		///     Deserializes the specified bytes.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">bytes</exception>
		public static ChannelMessage<T> Deserialize( byte[ ] bytes )
		{
			if ( bytes == null )
			{
				throw new ArgumentNullException( "bytes" );
			}

			using ( var stream = new MemoryStream( bytes ) )
			{
				return Serializer.Deserialize<ChannelMessage<T>>( stream );
			}
		}

		/// <summary>
		///     Determines whether [is message originator].
		/// </summary>
		/// <returns></returns>
		public bool IsMessageOriginator( )
		{
			return
				MachineName.Equals( Identity.MachineName, StringComparison.InvariantCultureIgnoreCase ) &&
				ProcessId.Equals( Identity.ProcessId ) &&
				AppDomainId.Equals( Identity.AppDomainId );
		}

		/// <summary>
		///     Serializes the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">message</exception>
		public static byte[ ] Serialize( ChannelMessage<T> message )
		{
			if ( message == null )
			{
				throw new ArgumentNullException( "message" );
			}

			using ( var stream = new MemoryStream( ) )
			{
				Serializer.Serialize( stream, message );

				return stream.ToArray( );
			}
		}
	}
}