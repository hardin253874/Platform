// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using EDC.ReadiNow.Annotations;
using EDC.SoftwarePlatform.Migration.Processing;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Entity Staging Entry.
	/// </summary>
	[DataContract( Name = "entityStagingEntry" )]
	public class EntityStagingEntry : EntityEntry
	{
		/// <summary>
		///     The entity name
		/// </summary>
		[DataMember( Name = "name", IsRequired = false, EmitDefaultValue = false )]
		public string EntityName;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEntityName( )
	    {
			return EntityName != null;
	    }

		/// <summary>
		///     The entity type identifier
		/// </summary>
		[DataMember( Name = "typeId", IsRequired = false, EmitDefaultValue = false )]
		public long EntityTypeId;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEntityTypeId( )
	    {
			return EntityTypeId != 0;
	    }

		/// <summary>
		///     The entity type name
		/// </summary>
		[DataMember( Name = "typeName", IsRequired = false, EmitDefaultValue = false )]
		public string EntityTypeName;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEntityTypeName( )
	    {
			return EntityTypeName != null;
	    }

		/// <summary>
		///     The identifier
		/// </summary>
		[DataMember( Name = "id", IsRequired = true, EmitDefaultValue = true )]
		public long Id;

		/// <summary>
		///     The parents
		/// </summary>
		[DataMember( Name = "parents", IsRequired = false, EmitDefaultValue = false )]
		public IList<EntityParentEntry> Parents = null;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeParents( )
	    {
			return Parents != null;
	    }

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			var sb = new StringBuilder( );
			bool firstParent = true;

			sb.AppendFormat( new NullFormatter( ), "{0}, Id: {1}, Name: {2}, TypeId: {3}, TypeName: {4}", EntityId.ToString( "B" ), Id, EntityName, EntityTypeId, EntityTypeName );

			if ( Parents != null && Parents.Count > 0 )
			{
				sb.Append( ", Parents: " );

				foreach ( EntityParentEntry parent in Parents )
				{
					if ( !firstParent )
					{
						sb.AppendFormat( ", {0}", parent );
					}
					else
					{
						sb.Append( parent );
					}

					firstParent = false;
				}
			}

			return sb.ToString( );
		}
	}
}