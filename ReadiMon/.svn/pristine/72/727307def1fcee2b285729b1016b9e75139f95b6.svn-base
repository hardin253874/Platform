// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ProtoBuf;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis.Contracts
{
	/// <summary>
	///     Entity Relationship Cache Type Key.
	/// </summary>
	[ProtoContract]
	public class EntityRelationshipCacheTypeKey : EntityRelationshipCacheKey
	{
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