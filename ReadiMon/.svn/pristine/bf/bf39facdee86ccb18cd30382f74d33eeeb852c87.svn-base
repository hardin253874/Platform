// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Messages
{
	/// <summary>
	///     PerfGraphMessage class.
	/// </summary>
	[DataContract( Name = "PerfGraphMessage" )]
	public class PerfGraphMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PerfGraphMessage" /> class.
		/// </summary>
		/// <param name="content">The content.</param>
		public PerfGraphMessage( string content )
		{
			Content = content;
		}

		/// <summary>
		///     Gets or sets the content.
		/// </summary>
		/// <value>
		///     The content.
		/// </value>
		[DataMember]
		public string Content
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