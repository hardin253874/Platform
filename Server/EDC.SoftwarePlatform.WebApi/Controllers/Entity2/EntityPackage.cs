// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using EDC.Database;
using EDC.Monitoring;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Monitoring.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     EntityPackage data passed around while processing a request.
	/// </summary>
	public class EntityPackage
	{
		private readonly HashSet<long> _entitiesVisited;
		private readonly JsonQueryResult _result;
#pragma warning disable 618
		private readonly EntityInfoService _svc;
#pragma warning restore 618

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityPackage" /> class.
		/// </summary>
		public EntityPackage( )
		{
			_entitiesVisited = new HashSet<long>( );
#pragma warning disable 618
			_svc = new EntityInfoService( );
#pragma warning restore 618
			_result = new JsonQueryResult( );
		}


		/// <summary>
		///     Get the query results from the current package queries
		/// </summary>
		/// <returns></returns>
		public JsonQueryResult GetQueryResult( )
		{
			return _result;
		}


		/// <summary>
		///     Add a request for an entity along with a query
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <param name="queryName">Name of the query.</param>
		public void AddEntityData( EntityData entityData, string queryName = null )
		{
			_result.Ids.Add( entityData.Id.Id );

			_result.Results.Add( new JsonSingleQueryResult
			{
				Ids = new List<long>
				{
					entityData.Id.Id
				},
				Code = HttpStatusCode.OK,
				Name = queryName,
				IsSingleValue = true
			} );

			AppendEntityToResult( _result, entityData );
		}

		/// <summary>
		///     Add a request for an entity along with a query
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		/// <param name="entityRequest">The entity request.</param>
		/// <param name="queryName">If set then the query is a named query.</param>
		public void AddEntityRequest( EntityRef entityRef, EntityMemberRequest entityRequest, string queryName = null )
		{
			EntityData entityData = _svc.GetEntityData( entityRef, entityRequest );

			AddEntityData( entityData, queryName );
		}


		/// <summary>
		///     Add a request for a set of entities and queries.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <param name="hintText">A hint about what this query is doing. Use for logging/diagnostics only.</param>
		/// <exception cref="System.ArgumentException">
		/// </exception>
		/// <exception cref="System.Exception">Internal error: batch query mismatch.</exception>
		public void AddEntityRequestBatch( JsonQueryBatchRequest batch, string hintText = null )
		{
			JsonQueryResult result = _result;


#pragma warning disable 618
			EntityInfoService svc = _svc;
#pragma warning restore 618

			var memberRequests = new EntityMemberRequest[batch.Queries.Length];

			foreach ( JsonQuerySingleRequest request in batch.Requests )
			{
				string requestHintText = request.Hint ?? hintText;

				_entitiesVisited.Clear( );

				var singleResult = new JsonSingleQueryResult
				{
					Hint = request.Hint, Ids = new List<long>( )
				};

				try
				{
					EntityMemberRequest memberRequest;

					// Do not require access to the individual query components
					using ( PerformanceCounters.Measure( EntityInfoServicePerformanceCounters.CategoryName, EntityInfoServicePerformanceCounters.RequestCountersPrefix ) )
					using ( new SecurityBypassContext( ) )
					{
						// Get/parse the request query. (Either parse, or load from previous cache as part of this request).
						if ( request.QueryIndex < 0 || request.QueryIndex >= batch.Queries.Length )
						{
							EventLog.Application.WriteError( "Cannot locate query string." );
							singleResult.Code = HttpStatusCode.BadRequest;
							continue;
						}
						memberRequest = memberRequests[ request.QueryIndex ];
						if ( memberRequest == null )
						{
							string queryString = batch.Queries[ request.QueryIndex ];

							//// Parse the request
							try
							{
								memberRequest = Factory.RequestParser.ParseRequestQuery( queryString );
							}
							catch ( Exception ex )
							{
								EventLog.Application.WriteError( ex.ToString( ) );
								singleResult.Code = HttpStatusCode.BadRequest;
								continue;
							}
							memberRequests[ request.QueryIndex ] = memberRequest;
						}
					}

					// Get the entityRefs
					IEnumerable<EntityRef> ids = Enumerable.Empty<EntityRef>( );

					if ( request.Aliases != null && request.Aliases.Length > 0 )
					{
						ids = ids.Union( request.Aliases.Select( ConvertId ) );
					}

					if ( request.Ids != null && request.Ids.Length > 0 )
					{
						ids = ids.Union( request.Ids.Select( ConvertId ) );
					}

					IList<EntityRef> entityRefs = ids.ToList( );

					if ( entityRefs.All( er => er == null ) )
					{
						singleResult.Code = HttpStatusCode.NotFound;
						continue;
					}

					// Run the request
					IEnumerable<EntityData> entitiesData;
					try
					{
						if ( BulkRequestHelper.IsValidForBulkRequest( memberRequest ) )
						{
							var entityRequest = new EntityRequest
							{
								QueryType = request.QueryType,
								RequestString = memberRequest.RequestString,
								Hint = requestHintText,
								Entities = entityRefs,
                                Filter = request.Filter,
                                Isolated = request.Isolated
                            };

                            // Run the request
                            entitiesData = BulkRequestRunner.GetEntities(entityRequest);

							singleResult.Cached = entityRequest.ResultFromCache;
						}
						else
						{
							EventLog.Application.WriteWarning(
								"Request cannot be handled by BulkRequest. Hint: {0}\n{1}",
								requestHintText, memberRequest.RequestString );

							switch ( request.QueryType )
							{
								case QueryType.Instances:
									entitiesData = svc.GetEntitiesByType( entityRefs.Single( ), true, memberRequest );
									break;
								case QueryType.ExactInstances:
									entitiesData = svc.GetEntitiesByType( entityRefs.Single( ), false, memberRequest );
									break;
								case QueryType.Basic:
								case QueryType.BasicWithDemand:
									entitiesData = svc.GetEntitiesData( entityRefs, memberRequest );
									break;
								default:
									throw new ArgumentException( string.Format( "Unknown query type {0}", request.QueryType ) );
							}
						}
					}
					catch ( ArgumentException ex )
					{
						// sorry world!
						if ( ex.Message.Contains( "does not represent a known entity" ) )
						{
							singleResult.Code = HttpStatusCode.BadRequest;
							continue;
						}
						throw;
					}

					// Skip results where access is denied
					foreach ( EntityData entityData in entitiesData.Where( ed => ed != null ) )
					{
						AppendEntityToResult( result, entityData );
						singleResult.Ids.Add( entityData.Id.Id );
					}

					// Set the result to NotFound for Basic and BasicWithDemand only
					if ( request.QueryType == QueryType.Basic || request.QueryType == QueryType.BasicWithDemand )
					{
						singleResult.Code = singleResult.Ids.Count > 0
							? HttpStatusCode.OK
							: HttpStatusCode.NotFound;
					}
					else
					{
						singleResult.Code = HttpStatusCode.OK;
					}
				}
				catch ( PlatformSecurityException ex )
				{
					EventLog.Application.WriteWarning( ex.ToString( ) );
					singleResult.Ids.Clear( );
					singleResult.Code = HttpStatusCode.Forbidden;
				}
				catch ( Exception ex )
				{
					EventLog.Application.WriteError( ex.ToString( ) );
					singleResult.Code = HttpStatusCode.InternalServerError;
				}
				finally
				{
					// Place in the finally block so we are certain that it gets run exactly once per iteration
					result.Results.Add( singleResult );
				}
			}

			if ( result.Results.Count != batch.Requests.Length )
			{
				// We are indexing by ID, so we must always ensure that request position matches result position.
				throw new Exception( "Internal error: batch query mismatch." );
			}
		}


		/// <summary>
		///     Converts a JSON entity ID object to an EntityRef
		/// </summary>
		/// <param name="objId">The object identifier.</param>
		/// <returns></returns>
		private EntityRef ConvertId( string objId )
		{
			var entityRef = new EntityRef( objId );

			try
			{
				// Attempt to resolve the Id
				entityRef.ResolveId( );
			}
			catch ( ArgumentException )
			{
				return null; // or whatever
			}

			return entityRef;
		}

		/// <summary>
		///     Converts a JSON entity ID object to an EntityRef
		/// </summary>
		/// <param name="objId">The object identifier.</param>
		/// <returns></returns>
		private EntityRef ConvertId( long objId )
		{
			return new EntityRef( objId );
		}


		/// <summary>
		///     Store an entity into the JSON result structure.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="entityData">The entity data.</param>
		private void AppendEntityToResult( JsonQueryResult result, EntityData entityData )
		{
			long entityKey = entityData.Id.Id;

			// Avoid circular loops within a query.
			if ( _entitiesVisited.Contains( entityKey ) )
				return;
			_entitiesVisited.Add( entityKey );

			Dictionary<string, object> entityJson;
			if ( !result.Entities.TryGetValue( entityKey, out entityJson ) )
			{
				entityJson = new Dictionary<string, object>( );
				result.Entities.Add( entityKey, entityJson );
			}

			AppendFieldsToEntity( result, entityData, entityJson );

			AppendRelationshipsToEntity( result, entityData, entityJson );

			if ( entityJson.Count <= 0 )
			{
				// Note: always add, then remove if not needed. Otherwise it's not present in the dictionary when we recurse,
				// which can lead to infinite loops.
				result.Entities.Remove( entityKey );
			}
		}


		/// <summary>
		///     Store fields into the JSON entity structure.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="entityData">The entity being read.</param>
		/// <param name="entityJson">JSON for holding member data for the individual entity.</param>
		private void AppendFieldsToEntity( JsonQueryResult result, EntityData entityData,
			Dictionary<string, object> entityJson )
		{
			Dictionary<long, JsonMember> members = result.Members;

			// Visit fields
			foreach ( FieldData field in entityData.Fields )
			{
				long fieldId = field.FieldId.Id;
				// Register the field member
				JsonMember member;
				if ( members.TryGetValue( fieldId, out member ) )
				{
					if ( field.FieldId.Alias != null && member.Alias == null )
					{
						member.Alias = GetAliasString( field.FieldId );
					}
				}
				else
				{
					member = new JsonMember
					{
						Alias = GetAliasString( field.FieldId ), DataType = DataTypeHelper.FromDatabaseType( field.Value.Type ).ToString( )
					};
					members.Add( fieldId, member );
				}

				object value = field.Value.Value;

				// Ensure empty strings are represented as nulls - this is by design!
				if ( value is string && ( ( string ) value ).Length == 0 )
					value = null;

				// Store the data in the entity
				entityJson[ fieldId.ToString( CultureInfo.InvariantCulture ) ] = value;
			}
		}


		/// <summary>
		///     Store relationships into the JSON entity structure.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="entityData">The entity being read.</param>
		/// <param name="entityJson">JSON for holding member data for the individual entity.</param>
		private void AppendRelationshipsToEntity( JsonQueryResult result, EntityData entityData,
			Dictionary<string, object> entityJson )
		{
			if ( entityData != null && entityData.Relationships != null )
			{
				// Visit relationships
				foreach ( RelationshipData rel in entityData.Relationships )
				{
					// Register the relationship member metadata
					// Note: we need to first see if there is a container for the relationship in either direction
					RegisterRelationshipMember( result, rel );

					// Get relationship data.
					// Holds a mixture of either long or JsonRelationshipInstance
					var relResult = new List<object>( );

					if ( rel != null && rel.Instances != null )
					{
						foreach ( RelationshipInstanceData relInst in rel.Instances )
						{
							// If we have a relationship instance, send a complex object
							// Otherwise just send the ID of the entity we're relating to.
							if ( relInst.RelationshipInstanceEntity != null )
							{
								var relInstResult = new JsonRelationshipInstance( );
								if ( relInst.Entity != null )
								{
									relInstResult.EntityId = relInst.Entity.Id.Id;
								}
								relInstResult.RelationshipInstance = relInst.RelationshipInstanceEntity.Id.Id;
								relResult.Add( relInstResult );
								// Recursively visit the relationship instance entity
								AppendEntityToResult( result, relInst.RelationshipInstanceEntity );
							}
							else if ( relInst.Entity != null )
							{
								long relatedId = relInst.Entity.Id.Id;
								relResult.Add( relatedId );
							}

							// Recursively visit the related entity
							AppendEntityToResult( result, relInst.Entity );
						}
					}

					// Store the data in the entity
					StoreRelationshipData( entityJson, rel, relResult );
				}
			}
		}


		/// <summary>
		///     Registers a 'member' entry for the relationship.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="rel">The relative.</param>
		private void RegisterRelationshipMember( JsonQueryResult result, RelationshipData rel )
		{
			Dictionary<long, JsonMember> members = result.Members;
			long relId = rel.RelationshipTypeId.Id;

			// Determine if there is an existing member registered for this relationship.
			// And create one if necessary.
			JsonMember member;
			if ( !members.TryGetValue( relId, out member ) )
			{
				member = new JsonMember( );
				members.Add( relId, member );
			}

			// Then we ensure there is information for the member in the particular direction that we're interested in.
			JsonRelationshipMember relMember = rel.IsReverseActual ? member.Reverse : member.Forward;
			if ( relMember == null )
			{
				relMember = new JsonRelationshipMember
				{
					Alias = GetAliasString( rel.RelationshipTypeId ), IsLookup = rel.IsLookup
				};
				if ( rel.IsReverseActual )
					member.Reverse = relMember;
				else
					member.Forward = relMember;
			}
			else if ( relMember.Alias == null && rel.RelationshipTypeId.Alias != null )
			{
				relMember.Alias = GetAliasString( rel.RelationshipTypeId );
			}
		}

		/// <summary>
		///     Stores an array of relationship data into an entity.
		/// </summary>
		/// <param name="entityJson">Dictionary that represents the entity.</param>
		/// <param name="rel">The relationship being stored.</param>
		/// <param name="relResult">The data being stored for that relationship.</param>
		private static void StoreRelationshipData( Dictionary<string, object> entityJson, RelationshipData rel,
			List<object> relResult )
		{
			long relId = rel.RelationshipTypeId.Id;

			// Look up whether there is already data stored for this relationship.
			// (As it may already be stored for the opposite direction).
			object data;
			JsonRelationshipData relPair;
			string relKey = relId.ToString( CultureInfo.InvariantCulture );
			if ( entityJson.TryGetValue( relKey, out data ) )
			{
				relPair = ( JsonRelationshipData ) data;
			}
			else
			{
				// Create if necessary
				relPair = new JsonRelationshipData( );
				entityJson[ relKey ] = relPair;
			}

			// Store the data on the appropriate direction.
			if ( rel.IsReverseActual )
			{
				relPair.Reverse = relResult;
			}
			else
			{
				relPair.Forward = relResult;
			}
		}


		/// <summary>
		///     Creates a string that represents the namespace:alias of an EntityRef.
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		/// <returns></returns>
		public string GetAliasString( IEntityRef entityRef )
		{
			if ( entityRef.Alias == null )
				return null;
			string res = entityRef.Namespace + ":" + entityRef.Alias;
			return res;
		}
	}
}