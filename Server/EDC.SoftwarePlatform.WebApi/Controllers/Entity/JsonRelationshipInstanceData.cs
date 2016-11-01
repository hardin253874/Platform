// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Relationship Instance Data class.
	/// </summary>
	[DataContract]
	public class JsonRelationshipInstanceData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonRelationshipInstanceData" /> class.
		/// </summary>
		public JsonRelationshipInstanceData( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JsonRelationshipInstanceData" /> class.
		/// </summary>
		/// <param name="relInstanceData">The relative instance data.</param>
		/// <param name="context">The context.</param>
		public JsonRelationshipInstanceData( RelationshipInstanceData relInstanceData, JsonEntityQueryResult context )
		{
			Entity = new JsonEntityData( relInstanceData.Entity, context ).Id;
			if ( relInstanceData.RelationshipInstanceEntity != null )
				RelEntity = new JsonEntityData( relInstanceData.RelationshipInstanceEntity, context ).Id;
			DataState = relInstanceData.DataState;
		}

		/// <summary>
		///     Gets or sets the entity.
		/// </summary>
		/// <value>
		///     The entity.
		/// </value>
		[DataMember( Name = "entity" )]
		public long Entity
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relative entity.
		/// </summary>
		/// <value>
		///     The relative entity.
		/// </value>
		[DataMember( Name = "relEntity" )]
		public long RelEntity
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
			Entity = JsonEntityQueryResult.ResolveId( Entity, idMap );
			RelEntity = JsonEntityQueryResult.ResolveId( RelEntity, idMap );
		}
	}
}