// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis.Contracts
{
	/// <summary>
	///     Entity Relationship Cache Message
	/// </summary>
	[ProtoContract]
	public class EntityRelationshipCacheMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationshipCacheMessage" /> class.
		/// </summary>
		public EntityRelationshipCacheMessage( )
		{
			RemoveKeys = new HashSet<SerializableEntityRelationshipCacheKey>( );
			RemoveTypeKeys = new HashSet<SerializableEntityRelationshipCacheTypeKey>( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether the relationship cache should be cleared in the forward direction.
		/// </summary>
		/// <value>
		///     <c>true</c> if [clear forward]; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 1 )]
		public bool ClearForward
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether the relationship cache should be cleared in the reverse direction.
		/// </summary>
		/// <value>
		///     <c>true</c> if [clear forward]; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 2 )]
		public bool ClearReverse
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
		[ProtoMember( 3 )]
		[XmlArrayItem( "RemoveKey" )]
		public HashSet<SerializableEntityRelationshipCacheKey> RemoveKeys
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove type keys.
		/// </summary>
		/// <value>
		///     The remove type keys.
		/// </value>
		[ProtoMember( 4 )]
		[XmlArrayItem( "RemoveTypeKey" )]
		public HashSet<SerializableEntityRelationshipCacheTypeKey> RemoveTypeKeys
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