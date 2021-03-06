﻿// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis.Contracts
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
		[XmlArrayItem( "RemoveKey" )]
		public HashSet<long> RemoveKeys
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
			return ContractHelper.ConvertToXml( this );
		}
	}
}