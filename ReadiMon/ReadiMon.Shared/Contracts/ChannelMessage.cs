// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using ProtoBuf;

namespace ReadiMon.Shared.Contracts
{
	/// <summary>
	///     ChannelMessage class.
	/// </summary>
	[Serializable]
	[ProtoContract]
	public class ChannelMessage<T>
	{
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
			set;
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
			set;
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
			set;
		}

		/// <summary>
		///     Gets or sets the host tenant identifier.
		/// </summary>
		/// <value>
		///     The host tenant identifier.
		/// </value>
		[ProtoMember( 8 )]
		public long HostTenantId
		{
			get;
			set;
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
			set;
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
			set;
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
			set;
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
			set;
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
			set;
		}

		/// <summary>
		///     Creates the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public static ChannelMessage<T> Create( T message )
		{
			var channelMessage = new ChannelMessage<T>
			{
				AppDomainId = Identity.AppDomainId,
				AppDomainName = Identity.AppDomainName,
				Date = DateTime.Now,
				HostTenantId = 0,
				MachineName = Identity.MachineName,
				Message = message,
				ProcessId = Identity.ProcessId,
				ProcessName = Identity.ProcessName,
				PublishToOriginator = false
			};

			return channelMessage;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return ContractHelper.ConvertToXml( this );
		}
	}
}