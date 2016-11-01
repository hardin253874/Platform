// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Entity Query Result class.
	/// </summary>
	[DataContract]
	public class JsonEntityQueryResult
	{
		/// <summary>
		///     The base new identifier
		/// </summary>
		public static long BaseNewId = EntityId.MinTemporary; // our known id 'high watermark'

		public JsonEntityQueryResult( )
		{
			Entities = new List<JsonEntity>( );
			EntityRefs = new List<JsonEntityRef>( );
		}

		public JsonEntityQueryResult( IEnumerable<EntityData> entityDataList, string extra = null )
		{
			Stopwatch sw = Stopwatch.StartNew( );
			Entities = new List<JsonEntity>( );
			EntityRefs = new List<JsonEntityRef>( );
			Ids = new List<long>( );
			foreach ( EntityData entityData in entityDataList )
			{
				EventLog.Application.WriteTrace( DumpEntity( entityData ) );
				Ids.Add( new JsonEntityData( entityData, this ).Id );
			}
			Extra = extra;
			Extra2 = "" + sw.ElapsedMilliseconds;
		}

		[DataMember( Name = "ids" )]
		public List<long> Ids
		{
			get;
			set;
		} // the ids of the requested entities

		[DataMember( Name = "entities" )]
		public List<JsonEntity> Entities
		{
			get;
			set;
		} // bag of entities appearing in the graph

		[DataMember( Name = "entityRefs" )]
		public List<JsonEntityRef> EntityRefs
		{
			get;
			set;
		} // bag of entity refs appearing in the graph

		[DataMember( Name = "extra" )]
		public string Extra
		{
			get;
			set;
		}

		[DataMember( Name = "extra2" )]
		public string Extra2
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the entity reference.
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		/// <returns></returns>
		public JsonEntityRef GetEntityRef( EntityRef entityRef )
		{
			JsonEntityRef jsonEntityRef = EntityRefs.FirstOrDefault( p => p.Id == entityRef.Id );
			if ( jsonEntityRef == null )
			{
				jsonEntityRef = new JsonEntityRef( entityRef );
				EntityRefs.Add( jsonEntityRef );
			}
			return jsonEntityRef;
		}

		/// <summary>
		///     Gets the entity.
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		/// <returns></returns>
		public JsonEntity GetEntity( EntityRef entityRef )
		{
			JsonEntity jsonEntity = Entities.FirstOrDefault( p => p.Id == entityRef.Id );
			if ( jsonEntity == null )
			{
				jsonEntity = new JsonEntity( entityRef, this );
				Entities.Add( jsonEntity );
			}
			return jsonEntity;
		}

		/// <summary>
		///     Resolves the ids.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">ids was null</exception>
		public void ResolveIds( )
		{
			var idMap = new Dictionary<long, long>( );
			foreach ( JsonEntityRef er in EntityRefs.Where( er => er.Id >= BaseNewId ) )
			{
				if ( idMap.ContainsKey( er.Id ) )
				{
					er.Id = idMap[ er.Id ];
				}
				else if ( !string.IsNullOrEmpty( er.Alias ) )
				{
					long id = new EntityRef( er.NameSpace, er.Alias ).Id;
					if ( id > 0 )
					{
						idMap[ er.Id ] = id;
						er.Id = idMap[ er.Id ];
					}
				}
			}
			if ( Ids == null )
				throw new InvalidOperationException( "ids was null" );
			Ids = Ids.Select( p => ResolveId( p, idMap ) ).ToList( );
			Entities.ForEach( p => p.ResolveIds( idMap ) );
		}

		/// <summary>
		///     Resolves the identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="idMap">The identifier map.</param>
		/// <returns></returns>
		public static long ResolveId( long id, Dictionary<long, long> idMap )
		{
			return id >= BaseNewId && idMap.ContainsKey( id ) ? idMap[ id ] : id;
		}

		/// <summary>
		///     Dumps the entity.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <returns></returns>
		public static string DumpEntity( EntityData entityData )
		{
			var sb = new StringBuilder( );
			if ( entityData != null && entityData.Id != null )
				sb.AppendFormat( "Entity: {0} '{1}' '{2}' {3}", entityData.Id.Id, entityData.Id.Namespace, entityData.Id.Alias, entityData.DataState );
			else
				sb.AppendFormat( "Entity: null" );

			if ( entityData == null )
				return sb.ToString( );

			if ( entityData.TypeIds != null )
			{
				sb.AppendFormat( "\n  types: count {0}", entityData.TypeIds.Count );
				foreach ( EntityRef t in entityData.TypeIds )
					sb.AppendFormat( "\n    {0} '{1}' ", t.Id, t.Alias );
			}

			if ( entityData.Fields != null )
			{
				sb.AppendFormat( "\n  fields: count {0}", entityData.Fields.Count );
				foreach ( FieldData f in entityData.Fields )
					sb.AppendFormat( "\n    {0} '{1}' '{2}'", f.FieldId.Id, f.FieldId.Alias, f.Value != null ? f.Value.Value : "null" );
			}

			if ( entityData.Relationships != null )
			{
				sb.AppendFormat( "\n  relationships: count {0}", entityData.Relationships.Count );
				foreach ( RelationshipData r in entityData.Relationships )
				{
					sb.AppendFormat( "\n    relationship: {0} '{1}', instance count {2}, removeExisting {3}, isReverse {4}",
						r.RelationshipTypeId.Id, r.RelationshipTypeId.Alias, r.Instances.Count, r.RemoveExisting, r.IsReverse );
					foreach ( RelationshipInstanceData i in r.Instances )
						sb.AppendFormat( "\n    id={0} alias='{1}' state='{2}' relEntity='{3}'", i.Entity.Id.Id, i.Entity.Id.Alias, i.DataState,
							i.RelationshipInstanceEntity != null ? i.RelationshipInstanceEntity.Id : "null" );
				}
			}

			return sb.ToString( );
		}

		/// <summary>
		///     Gets the entity data.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="cache">The cache.</param>
		/// <param name="refsCache">The refs cache.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">
		/// </exception>
		public EntityData GetEntityData( long id, Dictionary<long, EntityData> cache = null, Dictionary<long, EntityRef> refsCache = null )
		{
			if ( cache == null )
				cache = new Dictionary<long, EntityData>( );

			if ( cache.ContainsKey( id ) )
				return cache[ id ];

			long entityId = id;

			Dictionary<long, EntityRef> refs = refsCache;
			if ( refs == null )
			{
				refs = new Dictionary<long, EntityRef>( );
				foreach ( JsonEntityRef er in  EntityRefs )
				{
					if ( !refs.ContainsKey( er.Id ) )
						refs.Add( er.Id, er.ToEntityRef( ) );
				}
			}

			var entityData = new EntityData( );
			if ( refs.ContainsKey( entityId ) )
			{
				entityData.Id = refs[ entityId ];
			}
			cache[ id ] = entityData;

			JsonEntity entity = Entities.FirstOrDefault( p => p.Id == entityId );
			if ( entity == null )
			{
				EventLog.Application.WriteTrace( DumpEntity( entityData ) );
				return entityData;
			}

			entityData.DataState = entity.DataState;

			// Types
			if ( entity.TypeIds.Any( p => !refs.ContainsKey( p ) ) )
			{
				throw new ArgumentException(
					string.Format( "typeIds missing from entityRefs: {0}",
						string.Join( ",", entity.TypeIds.Where( p => !refs.ContainsKey( p ) ) ) ) );
			}
			entityData.TypeIds = entity.TypeIds.Select( p => refs[ p ] ).ToList( );

			// Fields
			if ( entity.Fields.Any( p => !refs.ContainsKey( p.FieldId ) ) )
			{
				throw new ArgumentException(
					string.Format( "fieldIds missing from entityRefs: {0}",
						string.Join( ",", entity.Fields.Where( p => !refs.ContainsKey( p.FieldId ) ) ) ) );
			}
			entityData.Fields = entity.Fields.Select( p =>
				new FieldData
				{
					FieldId = refs[ p.FieldId ],
					Value = new TypedValue( p.Value ) // TODO : Yikes! Use type-based string conversion, because this will break stuff
				} ).ToList( );

			entityData.Relationships = new List<RelationshipData>( );
			foreach ( JsonRelationshipData relationship in entity.Relationships )
			{
				var relData = new RelationshipData( );
				entityData.Relationships.Add( relData );

				relData.RelationshipTypeId = relationship.RelTypeId.ToEntityRef( ); // refs[relationship.relTypeId.id];
				relData.IsReverse = relationship.IsReverse;
				relData.RemoveExisting = relationship.RemoveExisting;
				relData.DeleteExisting = relationship.DeleteExisting;
                relData.AutoCardinality = relationship.AutoCardinality;
				relData.Instances = new List<RelationshipInstanceData>( );
				foreach ( JsonRelationshipInstanceData inst in relationship.Instances )
				{
					if ( inst.Entity != 0 )
					{
						relData.Instances.Add( new RelationshipInstanceData
						{
							Entity = GetEntityData( inst.Entity, cache, refs ),
							RelationshipInstanceEntity = inst.RelEntity != 0 ? GetEntityData( inst.RelEntity, cache, refs ) : null,
							DataState = inst.DataState
						} );
					}
				}
			}

			//EventLog.Application.WriteTrace( DumpEntity( entityData ) );
			return entityData;
		}

		/// <summary>
		///     Merges the specified json entity query result.
		/// </summary>
		/// <param name="jsonEntityQueryResult">The json entity query result.</param>
		public void Merge( JsonEntityQueryResult jsonEntityQueryResult )
		{
		}
	}
}