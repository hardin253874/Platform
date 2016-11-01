// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
	/// <summary>
	///     Test Message
	/// </summary>
	[ProtoContract]
	[Serializable]
	public class TestMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TestMessage" /> class.
		/// </summary>
		public TestMessage( )
		{
			Items = new List<string>( );
		}

		/// <summary>
		///     Gets or sets the action.
		/// </summary>
		/// <value>
		///     The action.
		/// </value>
		[ProtoMember( 1 )]
		public string Action
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the items.
		/// </summary>
		/// <value>
		///     The items.
		/// </value>
		[ProtoMember( 2 )]
		public List<string> Items
		{
			get;
			set;
		}
	}
}