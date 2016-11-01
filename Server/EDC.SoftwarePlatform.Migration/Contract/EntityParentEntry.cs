// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;
using EDC.SoftwarePlatform.Migration.Processing;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Entity Parent Entry.
	/// </summary>
	[DataContract]
	public class EntityParentEntry
	{
		/// <summary>
		///     The depth
		/// </summary>
		[DataMember( Name = "depth", IsRequired = false, EmitDefaultValue = false )]
		public int Depth;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDepth( )
	    {
			return Depth != 0;
	    }

		/// <summary>
		///     The parent entity upgrade identifier
		/// </summary>
		[DataMember( Name = "parentUid", IsRequired = false, EmitDefaultValue = false )]
		public Guid ParentEntityUpgradeId;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeParentEntityUpgradeId( )
	    {
			return ParentEntityUpgradeId != Guid.Empty;
	    }

		/// <summary>
		///     The reason this entity was included.
		/// </summary>
		[DataMember( Name = "reason", IsRequired = false, EmitDefaultValue = false )]
		public InclusionReason Reason;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReason( )
	    {
			return Reason != InclusionReason.Explicit;
	    }

		/// <summary>
		///     The relationship type identifier
		/// </summary>
		[DataMember( Name = "relTypeId", IsRequired = false, EmitDefaultValue = false )]
		public long RelationshipTypeId;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelationshipTypeId( )
	    {
			return RelationshipTypeId != 0;
	    }

		/// <summary>
		///     The relationship type name
		/// </summary>
		[DataMember( Name = "relTypeName", IsRequired = false, EmitDefaultValue = false )]
		public string RelationshipTypeName;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelationshipTypeName( )
	    {
			return RelationshipTypeName != null;
	    }

		/// <summary>
		///     The relationship type upgrade identifier
		/// </summary>
		[DataMember( Name = "relTypeUid", IsRequired = false, EmitDefaultValue = false )]
		public Guid RelationshipTypeUpgradeId;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelationshipTypeUpgradeId( )
	    {
			return RelationshipTypeUpgradeId != Guid.Empty;
	    }

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return string.Format( new NullFormatter( ), "{{Depth: {0}, ParentId: {1}, Reason: {2}, Relationship Type Id: {3}, Relationship Type Name: {4}, Relationship Type Upgrade Id: {5}}}", Depth, ParentEntityUpgradeId.ToString( "B" ), Reason, RelationshipTypeId, RelationshipTypeName, RelationshipTypeUpgradeId.ToString( "B" ) );
		}
	}
}