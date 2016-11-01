// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Field Entity Cache message.
	/// </summary>
	[ProtoContract]
	public class FieldEntityCacheMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="FieldEntityCacheMessage" /> class.
		/// </summary>
		public FieldEntityCacheMessage( )
		{
			RemoveKeys = new HashSet<long>( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="FieldEntityCacheMessage" /> is clear.
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
		public ISet<long> RemoveKeys
		{
			get;
			set;
		}
	}
}