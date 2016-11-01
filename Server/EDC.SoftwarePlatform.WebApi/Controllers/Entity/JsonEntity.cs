// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Entity class.
	/// </summary>
	[DataContract]
	public class JsonEntity
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonEntity" /> class.
		/// </summary>
		public JsonEntity( )
		{
			TypeIds = new List<long>( );
			Fields = new List<JsonFieldData>( );
			Relationships = new List<JsonRelationshipData>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JsonEntity" /> class.
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		/// <param name="context">The context.</param>
		public JsonEntity( EntityRef entityRef, JsonEntityQueryResult context )
			: this( )
		{
			JsonEntityRef eid = context.GetEntityRef( entityRef );
			if ( !string.IsNullOrEmpty( entityRef.Alias ) && eid.Alias != entityRef.Alias )
			{
				EventLog.Application.WriteWarning( "EntityRef with id {0} using different aliases \"{1}\" <> \"{2}\"", eid.Id, eid.Alias, entityRef.Alias );
			}
			Id = eid.Id;
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[DataMember( Name = "id" )]
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type ids.
		/// </summary>
		/// <value>
		///     The type ids.
		/// </value>
		[DataMember( Name = "typeIds" )]
		public List<long> TypeIds
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the fields.
		/// </summary>
		/// <value>
		///     The fields.
		/// </value>
		[DataMember( Name = "fields" )]
		public List<JsonFieldData> Fields
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		[DataMember( Name = "relationships" )]
		public List<JsonRelationshipData> Relationships
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the state of the data.
		/// </summary>
		/// <value>
		///     The state of the data.
		/// </value>
		[DataMember( Name = "dataState" )]
		public DataState DataState
		{
			get;
			set;
		}

		/// <summary>
		///     Resolves the ids.
		/// </summary>
		/// <param name="idMap">The identifier map.</param>
		public void ResolveIds( Dictionary<long, long> idMap )
		{
			long id0 = Id;
			Id = JsonEntityQueryResult.ResolveId( Id, idMap );
			if ( Id != id0 && DataState == DataState.Create )
			{
				DataState = DataState.Unchanged;
				EventLog.Application.WriteTrace( "Resolved id from {0} to {1} so changed dataState from Create to Unchanged", id0, Id );
			}
			TypeIds = TypeIds.Select( p => JsonEntityQueryResult.ResolveId( p, idMap ) ).ToList( );
			Fields.ForEach( p =>
			{
				p.FieldId = JsonEntityQueryResult.ResolveId( p.FieldId, idMap );
			} );
			Relationships.ForEach( p => p.ResolveIds( idMap ) );
		}
	}
}