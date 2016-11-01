// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Messages
{
	/// <summary>
	///     BalloonSettingsMessage class.
	/// </summary>
	[DataContract( Name = "BalloonSettingsMessage" )]
	public class BalloonSettingsMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="BalloonSettingsMessage" /> class.
		/// </summary>
		/// <param name="balloonTimeout">The balloon timeout.</param>
		public BalloonSettingsMessage( int balloonTimeout )
		{
			BalloonTimeout = balloonTimeout;
		}

		/// <summary>
		///     Gets or sets the balloon timeout.
		/// </summary>
		/// <value>
		///     The balloon timeout.
		/// </value>
		[DataMember]
		public int BalloonTimeout
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