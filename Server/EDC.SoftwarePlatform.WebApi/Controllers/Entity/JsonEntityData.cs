// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;

// The pupose of the following classes is to define a JSON representation of our EntityData object graph
// and to convert between it and an EntityData instance.

// Notes - lower case prop names, some shortened names .... all intentional 
// TODO - change the C# standards for class members and use DataMember annotations to control serialization

// Will probably do the same with the query side of things... when I get around to looking at that

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Entity Data class.
	/// </summary>
	public class JsonEntityData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonEntityData" /> class.
		/// </summary>
		public JsonEntityData( )
		{
			Id = 0;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JsonEntityData" /> class.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <param name="context">The context.</param>
		public JsonEntityData( EntityData entityData, JsonEntityQueryResult context )
		{
			Id = 0;
			if ( entityData == null )
				return;

			Id = entityData.Id.Id;

			JsonEntity jsonEntity = context.GetEntity( entityData.Id );
			jsonEntity.DataState = entityData.DataState;

			IEnumerable<EntityRef> newTypeIds = entityData.TypeIds.Where( p => !jsonEntity.TypeIds.Contains( p.Id ) );
			jsonEntity.TypeIds.AddRange( newTypeIds.Select( p => context.GetEntityRef( p ).Id ) );

			foreach ( FieldData f in entityData.Fields )
			{
				JsonFieldData existingField = jsonEntity.Fields.FirstOrDefault( p => p.FieldId == f.FieldId.Id );
				if ( existingField == null )
				{
					var jsonField = new JsonFieldData( f, context );
					jsonEntity.Fields.Add( jsonField );
				}
			}

			foreach ( RelationshipData r in entityData.Relationships )
			{
				JsonRelationshipData existing = jsonEntity.Relationships.FirstOrDefault( p => p.RelTypeId.Id == r.RelationshipTypeId.Id && p.RelTypeId.Alias == r.RelationshipTypeId.Alias && p.IsReverse == r.IsReverse );
				if ( existing == null )
				{
					// need to stick a placeholder relationship in place before recursively calling to set up the related entity data
					var tempRel = new JsonRelationshipData( r, null );
					jsonEntity.Relationships.Add( tempRel ); // add this
					var newRel = new JsonRelationshipData( r, context ); // before doing this

					// now swap out the temp with the actual
					jsonEntity.Relationships.Remove( tempRel );
					jsonEntity.Relationships.Add( newRel );
				}
				else if ( existing.Instances.Select( p => p.Entity ).Except( r.Instances.Select( p => p.Entity.Id.Id ) ).Any( ) )
				{
					//throw new InvalidOperationException("Wasn't expecting diff relationship instances for same rel type on a given object....");
					EventLog.Application.WriteWarning( "Wasn't expecting diff relationship instances for same rel type on a given object...." );
				}
			}
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get;
			set;
		}
	}
}