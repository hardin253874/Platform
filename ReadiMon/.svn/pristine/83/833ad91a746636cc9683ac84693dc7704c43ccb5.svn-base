// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using ProtoBuf;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis.Contracts
{
	/// <summary>
	///     Used to key into the EntityRelationshipCache data structure.
	/// </summary>
	[Serializable]
	[ProtoContract]
	[ProtoInclude( 10, typeof ( EntityRelationshipCacheTypeKey ) )]
	public class EntityRelationshipCacheKey
	{
		/// <summary>
		///     Gets the secondary key.
		/// </summary>
		[ProtoMember( 2 )]
		public Direction Direction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the primary key.
		/// </summary>
		[ProtoMember( 1 )]
		public long EntityId
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