// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ReadiMon.Plugin.Redis.Contracts
{
	/// <summary>
	///     Serializable EntityId.
	/// </summary>
	[ProtoContract]
	[Serializable]
	public class SerializableEntityId
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableEntityId" /> class from being created.
		/// </summary>
		private SerializableEntityId( )
		{
			TypeIds = new List<long>( );
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[ProtoMember( 1 )]
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the type ids.
		/// </summary>
		/// <value>
		///     The type ids.
		/// </value>
		[ProtoMember( 2 )]
		public List<long> TypeIds
		{
			get;
			set;
		}
	}
}