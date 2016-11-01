// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ProtoBuf;

namespace ReadiMon.Plugin.Redis.Contracts
{
	/// <summary>
	///     The SerializableEntityRelationshipCacheKey class.
	/// </summary>
	[ProtoContract]
	[ProtoInclude( 10, typeof ( SerializableEntityRelationshipCacheTypeKey ) )]
	public class SerializableEntityRelationshipCacheKey
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableEntityRelationshipCacheKey" /> class from being created.
		/// </summary>
		protected SerializableEntityRelationshipCacheKey( )
		{
		}

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
		public SerializableEntityId EntityId
		{
			get;
			set;
		}
	}
}