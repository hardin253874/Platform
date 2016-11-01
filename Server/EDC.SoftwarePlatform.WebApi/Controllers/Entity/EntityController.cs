// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.Tablet;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using DataColumn = System.Data.DataColumn;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ReportColumn = EDC.SoftwarePlatform.WebApi.Controllers.Tablet.ReportColumn;
using EDC.Exceptions;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Entity Controller.
	/// </summary>
	[RoutePrefix( "data/v1/entity" )]
	public class EntityController : ApiController
	{
		private static ReportDataDefinition PackReportResponse( QueryResult results )
		{
			DataTable table = results.DataTable;
			var data = new ReportDataDefinition( );
			ResultColumn idCol = results.Columns.FirstOrDefault( c => c.RequestColumn.ColumnName == "_Id" );

			data.ReportTitle = "Query";
			data.ReportStyle = "Simple";
			data.Columns = new List<ReportColumn>(
				results.Columns.Select( c => new ReportColumn
				{
					Title = !string.IsNullOrEmpty( c.DisplayName ) ? c.DisplayName : c.RequestColumn.ColumnName,
					DataType = c.ColumnType.GetDisplayName( )
				} ) );

			data.ReportDataRows = new List<ReportData>(
				table.Rows.OfType<DataRow>( ).Select( r => new ReportData
				{
					Id = idCol != null ? ( long ) r[ idCol.RequestColumn.ColumnName ] : 99,
					LineItems = new List<ReportDataItem>( table.Columns.OfType<DataColumn>( )
						.Select( c => new ReportDataItem
						{
							Value = r[ c.ColumnName ].ToString( )
						} ) )
				} ) );
			return data;
		}

		/// <summary>
		///     Converts a dictionary to json.
		/// </summary>
		/// <param name="dict">The dictionary.</param>
		/// <returns>String representing a JSON structure.</returns>
		private static string DictionaryToJson( IEnumerable<KeyValuePair<long, IEntity>> dict )
		{
			IEnumerable<string> entries = dict.Select( d => string.Format( "\"{0}\": {1}", d.Key, d.Value.Id ) );
			return "{" + string.Join( ",", entries ) + "}";
		}

		/// <summary>
		///     Related entities.
		/// </summary>
		/// <param name="related">The related.</param>
		/// <param name="nodeIds">The node ids.</param>
		/// <returns></returns>
		private IEnumerable<RelatedResource> RelatedEntities( IEnumerable<JsonRelatedEntityInQuery> related, Dictionary<string, Guid> nodeIds )
		{
			var relatedEntities = new List<RelatedResource>( );

			if ( related != null )
			{
				foreach ( JsonRelatedEntityInQuery r in related )
				{
					string asId = r.Related.As ?? "";

					if ( nodeIds.ContainsKey( asId ) )
						continue;

					nodeIds.Add( asId, Guid.NewGuid( ) );

					var re = new RelatedResource
					{
						NodeId = nodeIds[ asId ],
						RelationshipTypeId = WebApiHelpers.GetId( r.Related.Id ),
						RelationshipDirection = r.Forward ? RelationshipDirection.Forward : RelationshipDirection.Reverse,
						ResourceMustExist = r.MustExist
					};
					re.RelatedEntities.AddRange( RelatedEntities( r.Related.Related, nodeIds ) );
					relatedEntities.Add( re );
				}
			}

			return relatedEntities;
		}

		private ConditionType GetConditionOperator( string oper )
		{
			ConditionType condType;
			if ( !Enum.TryParse( oper, true, out condType ) )
				condType = ConditionType.StartsWith;

			return condType;
		}

		[Route( "query" )]
        [HttpPost]
		public HttpResponseMessage<ReportDataDefinition> Query( [FromBody] JsonStructuredQuery jsonQuery )
		{
            if (jsonQuery == null)
                throw new WebArgumentNullException("jsonQuery");
            if (jsonQuery.Root == null)
                throw new WebArgumentNullException("jsonQuery.Root");

			try
			{
				var query = new StructuredQuery( );
				var nodeIds = new Dictionary<string, Guid>( );
				string rootAs = jsonQuery.Root.As ?? "";

				nodeIds.Add( rootAs, Guid.NewGuid( ) );
				query.RootEntity = new ResourceEntity
				{
					EntityTypeId = WebApiHelpers.GetId( jsonQuery.Root.Id ),
					NodeId = nodeIds[ rootAs ]
				};

				query.RootEntity.RelatedEntities.AddRange( RelatedEntities( jsonQuery.Root.Related, nodeIds ) );

				query.SelectColumns.Add( new SelectColumn
				{
					ColumnId = Guid.NewGuid( ),
					ColumnName = "_Id",
					IsHidden = true,
					Expression = new IdExpression
					{
						NodeId = query.RootEntity.NodeId
					}
				} );
				if ( jsonQuery.Selects != null )
				{
					foreach ( JsonSelectInQuery f in jsonQuery.Selects )
					{
						if ( !string.IsNullOrEmpty( f.Field ) && f.Field.ToLower( ) == "_id" && f.On != null )
						{
							if ( f.On != null )
							{
								query.SelectColumns.Add( new SelectColumn
								{
									ColumnId = Guid.NewGuid( ),
									ColumnName = f.DisplayAs ?? f.On + "_Id",
									IsHidden = true,
									Expression = new IdExpression
									{
										NodeId = nodeIds[ f.On ]
									}
								} );
							}
						}
						else
						{
							var field = new ResourceDataColumn
							{
								FieldId = WebApiHelpers.GetId( f.Field ),
								NodeId = nodeIds[ f.On ?? rootAs ]
							};
							string name = field.FieldId.Entity.Cast<Resource>( ).Name;
							var sc = new SelectColumn
							{
								ColumnId = Guid.NewGuid( ),
								DisplayName = f.DisplayAs ?? name,
								ColumnName = name,
								Expression = field
							};
							query.SelectColumns.Add( sc );
						}
					}
				}
				if ( jsonQuery.Conds != null )
				{
					foreach ( JsonCondition c in jsonQuery.Conds )
					{
						var qc = new QueryCondition
						{
							Operator = GetConditionOperator( c.Operation ),
							Argument = new TypedValue( c.Value )
						};
						if ( !string.IsNullOrEmpty( c.Expression.Field ) && c.Expression.Field.ToLower( ) == "_id" )
						{
							qc.Expression = new IdExpression
							{
								NodeId = nodeIds[ c.Expression.On ?? rootAs ]
							};
						}
						else
						{
							qc.Expression = new ResourceDataColumn
							{
								FieldId = WebApiHelpers.GetId( c.Expression.Field ),
								NodeId = nodeIds[ c.Expression.On ?? rootAs ]
							};
						}
						query.Conditions.Add( qc );
					}
				}

                QueryResult result = Factory.QueryRunner.ExecuteQuery( query, new QuerySettings
				{
					SecureQuery = true
				} );

				// debug
				LogResult( result );

				return new HttpResponseMessage<ReportDataDefinition>( PackReportResponse( result ) );
			}
			catch ( HttpResponseException )
			{
				throw;
			}
			catch ( PlatformSecurityException )
			{
				throw;
			}
			catch ( Exception e )
			{
				if ( e is ArgumentException ) // would be better if there was a more specific exception for 'not found'
					throw new HttpResponseException( HttpStatusCode.NotFound );

                throw;
			}
		}

		private static void LogResult( QueryResult result )
		{
			var sb = new StringBuilder( "Query data is:\n" );

			DataTable t = result.DataTable;
			foreach ( ResultColumn c in result.Columns )
			{
				sb.AppendFormat( "\"{0}\"({1},\"{2}\")\t", c.DisplayName, c.ColumnType.GetDisplayName( ), c.RequestColumn.ColumnName );
			}
			sb.AppendLine( );
			foreach ( DataColumn c in t.Columns )
			{
				sb.AppendFormat( "{0}\t", c.ColumnName );
			}
			sb.AppendLine( );
			foreach ( DataRow r in t.Rows )
			{
				foreach ( DataColumn c in t.Columns )
				{
					sb.AppendFormat( "{0}\t", r[ c.ColumnName ] );
				}
				sb.AppendLine( );
			}
			EventLog.Application.WriteInformation( sb.ToString( ) );
		}

		private HttpResponseMessage<JsonEntityQueryResult> GetEntityData( ICollection<EntityRef> entityRefs, ICollection<string> requests )
		{
			try
			{
				Stopwatch sw = Stopwatch.StartNew( );
				JsonEntityQueryResult result = GetJsonEntityQueryResult( entityRefs, requests );
				result.Extra = "" + sw.ElapsedMilliseconds;

				return new HttpResponseMessage<JsonEntityQueryResult>( result );
			}
			catch ( HttpResponseException )
			{
				throw;
			}
			catch ( PlatformSecurityException )
			{
				throw;
			}
			catch ( Exception e )
			{
				if ( e is ArgumentException ) // would be better if there was a more specific exception for 'not found'
					throw new HttpResponseException( HttpStatusCode.NotFound );

				EventLog.Application.WriteError( "caught exception: " + e.Message );
				throw new InvalidOperationException( "caught exception " + e.GetType( ).Name, e );
			}
		}

		public static JsonEntityQueryResult GetJsonEntityQueryResult( ICollection<EntityRef> entityRefs, ICollection<string> requests )
		{
			var svc = new EntityInfoService( );
			var entityDataList = new List<EntityData>( );

			for ( int i = 0; i < entityRefs.Count; ++i )
			{
				EntityRef entityRef = entityRefs.ElementAt( i );
				string request = i < requests.Count ? requests.ElementAt( i ) : requests.Last( );

                var rqObj = Factory.RequestParser.ParseRequestQuery( request );
                EntityData entityData = svc.GetEntityData( entityRef, rqObj );

				entityDataList.Add( entityData );
			}

			if ( entityDataList.Count( p => p != null ) == 0 )
				throw new HttpResponseException( HttpStatusCode.NotFound );

			var result = new JsonEntityQueryResult( entityDataList );
			return result;
		}

		/// <summary>
		///     Examples of URLs (note to escape such as [] and :, but not doing here for clarity)
		///     .../entity/747483?request=name,description
		///     .../entity/shared/person?request=*
		///     .../entity?id=shared:person&amp;request=*
		///     .../entity?id[]=shared:person&amp;id[]=bcm:process&amp;request[]=*
		///     .../entity?id[]=shared:person&amp;id[]=bcm:process&amp;request[]=*&amp;request[]=riskMatrix.*
		/// </summary>
		[Route( "" )]
		[Route( "{id:long}" )]
		[Route( "{ns}/{alias}" )]
        [HttpGet]
		public HttpResponseMessage<JsonEntityQueryResult>
			Get( [FromUri( Name = "id" )] ICollection<string> ids,
				[FromUri( Name = "request" )] ICollection<string> requests,
				string ns = null, string alias = null )
		{
			var entityRefs = new List<EntityRef>( );

			var sb = new StringBuilder( );
			sb.AppendFormat( "GET entityData url \"{0}\": {1} ids, {2} requests, ns=\"{3}\", alias=\"{4}\"", Request.RequestUri,
				ids != null ? ids.Count : 0, requests != null ? requests.Count : 0, ns, alias );

			if ( ids != null )
			{
				foreach ( string eid in ids )
				{
					sb.AppendFormat( "\n    id {0}=\"{1}\"", entityRefs.Count, eid );

					long id;
					if ( long.TryParse( eid, out id ) )
					{
						entityRefs.Add( new EntityRef( id ) );
					}
					else
					{
						string[ ] parts = eid.Split( ':' );
						entityRefs.Add( parts.Length > 1 ? new EntityRef( parts[ 0 ], parts[ 1 ] ) : new EntityRef( eid ) );
					}
				}
			}
			else if ( !string.IsNullOrEmpty( alias ) )
			{
				entityRefs.Add( !string.IsNullOrEmpty( ns ) ? new EntityRef( ns, alias ) : new EntityRef( alias ) );
			}

			if ( requests == null ) requests = new List<string>( );
			if ( requests.Count <= 0 )
				requests.Add( "*" );

			for ( int i = 0; i < requests.Count; ++i )
			{
				sb.AppendFormat( "\n    request {0}=\"{1}\"", i, requests.ElementAt( i ) );
			}

			EventLog.Application.WriteTrace( sb.ToString( ) );

			return GetEntityData( entityRefs, requests );
		}

		/// <summary>
		///     Handles HTTP Post messages that require the old to new id mapping back.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <returns>
		///     An HTTP response message containing the custom Json structure mapping old ids to new ids.
		/// </returns>
		[Route( "getNewIds" )]
        [HttpPost]
		public HttpResponseMessage PostGet( [FromBody] JsonEntityQueryResult entityData )
		{
			return HandlePost( entityData, true );
		}

        /// <summary>
		///     Clones the entity at the root and then performs any entity updates as necessary.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <returns>
		///     The id of the cloned entity.
		/// </returns>
		[Route("cloneAndUpdate")]
        [HttpPost]
        public IHttpActionResult PostCloneAndUpdate([FromBody] JsonEntityQueryResult entityData)
        {
            return HandleCloneAndUpdate(entityData);
        }

        /// <summary>
        ///     Handle HTTP Post messages that require the new id back (for backwards compatibility).
        /// </summary>
        /// <param name="entityData">The entity data.</param>
        /// <returns>
        ///     An HTTP response message containing the new id of the entity graphs root element.
        /// </returns>
        [Route("")]
        [HttpPost]
		public HttpResponseMessage Post( [FromBody] JsonEntityQueryResult entityData )
		{
			return HandlePost( entityData, false );
		}

		/// <summary>
		///     Handles post data.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <param name="returnMap">if set to <c>true</c> [return map].</param>
		/// <returns></returns>
		private HttpResponseMessage HandlePost( JsonEntityQueryResult entityData, bool returnMap )
		{
			Stopwatch sw = Stopwatch.StartNew( );
			long t1;

			// resolve all entity ids above our 'known id hi watermark' that actually do exist
			entityData.ResolveIds( );

			long id = entityData.Ids.FirstOrDefault( );
			IDictionary<long, IEntity> map = null;

			if ( id >= JsonEntityQueryResult.BaseNewId )
			{
				// create
				EventLog.Application.WriteTrace( "Creating entity " + id );
				EntityData newEntityData = entityData.GetEntityData( id );

				t1 = sw.ElapsedMilliseconds;

                DatabaseContext.RunWithRetry(() =>
                {
                    using (DatabaseContext context = DatabaseContext.GetContext(true))
                    {
                        var svc = new EntityInfoService();

                        map = svc.CreateEntityGetMap(newEntityData);

                        IEntity entity = map[newEntityData.Id.Id];
                        id = entity == null ? -1 : entity.Id;

                        context.CommitTransaction();
                    }
                });
				

				EventLog.Application.WriteTrace( "EntityPost create took {0} msec ({1} to de-json)",
					sw.ElapsedMilliseconds, t1 );
			}
			else
			{
				map = new Dictionary<long, IEntity>( );

				EventLog.Application.WriteTrace( "Updating entity " + id );
				EntityData newEntityData = entityData.GetEntityData( id );

				t1 = sw.ElapsedMilliseconds;


                DatabaseContext.RunWithRetry(() =>
                {
                    using (DatabaseContext context = DatabaseContext.GetContext(true))
                    {
                        var svc = new EntityInfoService();
                        svc.UpdateEntity(newEntityData);

                        context.CommitTransaction();
                    }
                });

				EventLog.Application.WriteTrace( "EntityPost update took {0} msec ({1} to de-json)",
					sw.ElapsedMilliseconds, t1 );
			}


			HttpResponseMessage httpResponseMessage;

			if ( returnMap )
			{
				/////
				// Create a custom response message so the infrastructure framework doesn't serialize it twice.
				/////
				httpResponseMessage = new HttpResponseMessage
				{
					Content = new StringContent( DictionaryToJson( map ) )
				};

				httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );
			}
			else
			{
				httpResponseMessage = new HttpResponseMessage<long>( id );
			}

			return httpResponseMessage;
		}

        /// <summary>
		///     Handles cloning data.
		/// </summary>
		/// <param name="entityData">The entity data.</param>		
		/// <returns>The id of the cloned entity</returns>
		private IHttpActionResult HandleCloneAndUpdate(JsonEntityQueryResult entityData)
        {
            // resolve all entity ids above our 'known id hi watermark' that actually do exist
            entityData.ResolveIds();

            long id = entityData.Ids.FirstOrDefault();
            long cloneId = -1;

            if (id >= JsonEntityQueryResult.BaseNewId)
            {                                
                return BadRequest("Cannot clone a temporary entity.");
            }

            EventLog.Application.WriteTrace("Cloning entity " + id);
            EntityData newEntityData = entityData.GetEntityData(id);

            DatabaseContext.RunWithRetry(() =>
            {
                using (DatabaseContext context = DatabaseContext.GetContext(true))
                {
                    var svc = new EntityInfoService();

                    var clonedIds = svc.CloneAndUpdateEntity(newEntityData);

                    if (!clonedIds.TryGetValue(id, out cloneId))
                    {
                        cloneId = -1;
                    }

                    context.CommitTransaction();
                }
            });

            return Ok(cloneId);
        }

        /// <summary>
        ///     Clones the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route( "clone" )]
        [HttpPost]
		public HttpResponseMessage<long> Clone( [FromBody] EntityCloneRequest request )
		{
            if ( request == null )
                throw new WebArgumentNullException("request");

            // Find entity
			IEntity entity = ReadiNow.Model.Entity.Get( request.Id.ToEntityRef( ) );
            if (entity == null)
                return new HttpResponseMessage<long>(HttpStatusCode.NotFound);

            // Clone
            IEntity clone = entity.Clone(CloneOption.Deep);

            if (!string.IsNullOrEmpty(request.Name))
            {
                clone.SetField(WellKnownAliases.CurrentTenant.Name, request.Name);
            }

            clone.Save();

            return new HttpResponseMessage<long>(clone.Id, HttpStatusCode.OK);
		}

		/// <summary>
		///     Deletes the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[Route( "" )]
		[Route( "{id:long}" )]
        [HttpDelete]
		public HttpResponseMessage<long> Delete( [FromUri] IEnumerable<long> id )
		{
            if ( id == null )
            {
                throw new WebArgumentException( "No id was specified" );
            }

			long[ ] ids = id as long[ ] ?? id.ToArray( );
            var refs = ids.Distinct().Select(i => new EntityRef(i));

			EventLog.Application.WriteInformation( string.Format( "webapi- deleting {0} entities", ids.Length ) );

            var svc = new EntityInfoService();
            svc.DeleteEntities(refs);

            EventLog.Application.WriteInformation(string.Format("webapi- deleting complete"));

            return new HttpResponseMessage<long>( HttpStatusCode.OK );
		}
	}
}