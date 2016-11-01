// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Cache message.
	/// </summary>
	[ProtoContract]
	[Serializable]
	public class EntityCacheMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityCacheMessage" /> class.
		/// </summary>
		public EntityCacheMessage( )
		{
			RemoveKeys = new HashSet<SerializableEntityId>( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="EntityCacheMessage" /> is clear.
		/// </summary>
		/// <value>
		///     <c>true</c> if clear; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 1 )]
		public bool Clear
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove keys.
		/// </summary>
		/// <value>
		///     The remove keys.
		/// </value>
		[ProtoMember( 2 )]
		public ISet<SerializableEntityId> RemoveKeys
		{
			get;
			set;
		}
	}
}