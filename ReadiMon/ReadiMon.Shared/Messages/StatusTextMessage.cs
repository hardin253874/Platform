// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Messages
{
	/// <summary>
	///     Status text message.
	/// </summary>
	[DataContract( Name = "StatusTextMessage" )]
	public class StatusTextMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="StatusTextMessage" /> class.
		/// </summary>
		/// <param name="statusText">The status text.</param>
		/// <param name="timeout">The timeout.</param>
		public StatusTextMessage( string statusText, int timeout = 0 )
		{
			StatusText = statusText;
			Timeout = timeout;
		}

		/// <summary>
		///     Gets or sets the status text.
		/// </summary>
		/// <value>
		///     The status text.
		/// </value>
		[DataMember]
		public string StatusText
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the timeout.
		/// </summary>
		/// <value>
		///     The timeout.
		/// </value>
		[DataMember]
		public int Timeout
		{
			get;
			set;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return Serializer.SerializeObject( this );
		}
	}
}