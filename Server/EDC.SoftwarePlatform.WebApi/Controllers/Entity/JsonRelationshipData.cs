// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Relationship Data class.
	/// </summary>
	[DataContract]
	public class JsonRelationshipData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonRelationshipData" /> class.
		/// </summary>
		public JsonRelationshipData( )
		{
			Instances = new List<JsonRelationshipInstanceData>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JsonRelationshipData" /> class.
		/// </summary>
		/// <param name="relationshipData">The relationship data.</param>
		/// <param name="context">The context.</param>
		public JsonRelationshipData( RelationshipData relationshipData, JsonEntityQueryResult context )
		{
			RelTypeId = new JsonEntityRef
			{
				Id = relationshipData.RelationshipTypeId.Id,
				NameSpace = relationshipData.RelationshipTypeId.Namespace,
				Alias = relationshipData.RelationshipTypeId.Alias
			};
			IsReverse = relationshipData.IsReverse;
			IsLookup = relationshipData.IsLookup;
			RemoveExisting = relationshipData.RemoveExisting;
			DeleteExisting = relationshipData.DeleteExisting;
            AutoCardinality = relationshipData.AutoCardinality;
			Instances = new List<JsonRelationshipInstanceData>( );
			if ( context != null )
			{
				foreach ( RelationshipInstanceData instance in relationshipData.Instances )
				{
					Instances.Add( new JsonRelationshipInstanceData( instance, context ) );
				}
			}
		}

		/// <summary>
		///     Gets or sets the relative type identifier.
		/// </summary>
		/// <value>
		///     The relative type identifier.
		/// </value>
		[DataMember( Name = "relTypeId" )]
		public JsonEntityRef RelTypeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the instances.
		/// </summary>
		/// <value>
		///     The instances.
		/// </value>
		[DataMember( Name = "instances" )]
		public List<JsonRelationshipInstanceData> Instances
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is reverse.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is reverse; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "isReverse", EmitDefaultValue = false )]
		public bool IsReverse
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is reverse value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeIsReverse( )
	    {
			return IsReverse;
	    }

		/// <summary>
		///     Gets or sets a value indicating whether this instance is lookup.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is lookup; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "isLookup", EmitDefaultValue = false )]
		public bool IsLookup
		{
			get;
			set;
		}

		/// <summary>
		/// Should the is lookup value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeIsLookup( )
	    {
			return IsLookup;
	    }

		/// <summary>
		///     Gets or sets a value indicating whether existing relationships from the source entity to other entities should be removed.
		/// </summary>
		/// <value>
		///     <c>true</c> if [remove existing]; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "removeExisting", EmitDefaultValue = false )]
		public bool RemoveExisting
		{
			get;
			set;
		}

		/// <summary>
		/// Should the remove existing  value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRemoveExisting( )
	    {
			return RemoveExisting;
	    }

		/// <summary>
        ///     Gets or sets a value indicating whether existing related entities should be deleted.
		/// </summary>
		/// <value>
		///     <c>true</c> if [delete existing]; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "deleteExisting", EmitDefaultValue = false )]
		public bool DeleteExisting
		{
			get;
			set;
		}

		/// <summary>
		/// Should the delete existing value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDeleteExisting( )
	    {
			return DeleteExisting;
	    }

        /// <summary>
        ///     Gets or sets a value indicating that any existing relationships (to either the source or the target) should
        ///     be removed if they would cause a cardinality violation.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [remove existing]; otherwise, <c>false</c>.
        /// </value>
        [DataMember( Name = "autoCardinality", EmitDefaultValue = false )]
        public bool AutoCardinality
        {
            get;
            set;
        }

		/// <summary>
		/// Should the automatic cardinality value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAutoCardinality( )
	    {
			return AutoCardinality;
	    }

		/// <summary>
		///     Resolves the ids.
		/// </summary>
		/// <param name="idMap">The identifier map.</param>
		public void ResolveIds( Dictionary<long, long> idMap )
		{
			RelTypeId.Id = JsonEntityQueryResult.ResolveId( RelTypeId.Id, idMap );
			Instances.ForEach( p => p.ResolveIds( idMap ) );
		}
	}
}