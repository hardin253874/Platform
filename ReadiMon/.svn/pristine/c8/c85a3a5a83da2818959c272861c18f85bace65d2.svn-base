// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ProtoBuf;

namespace ReadiMon.Plugin.Redis.Contracts
{
	/// <summary>
	///     The SerializableEntityRelationshipCacheTypeKey class.
	/// </summary>
	/// <seealso cref="ReadiMon.Plugin.Redis.Contracts.SerializableEntityRelationshipCacheKey" />
	[ProtoContract]
	public class SerializableEntityRelationshipCacheTypeKey : SerializableEntityRelationshipCacheKey
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableEntityRelationshipCacheTypeKey" /> class from being
		///     created.
		/// </summary>
		protected SerializableEntityRelationshipCacheTypeKey( )
		{
		}

		/// <summary>
		///     Gets or sets the type identifier.
		/// </summary>
		/// <value>
		///     The type identifier.
		/// </value>
		[ProtoMember( 1 )]
		public long TypeId
		{
			get;
			set;
		}
	}
}