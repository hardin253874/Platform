// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using ReadiNow.ImportExport;
using System.Net.Http.Headers;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Entity Version 2 Controller class.
	/// </summary>
	[RoutePrefix( "data/v2/entity" )]
	public class EntityV2Controller : ApiController
	{
		/// <summary>
		///     Examples of URLs (note to escape such as [] and :, but not doing here for clarity)
		///     .../entity/747483?request=name,description
		///     .../entity/shared/person?request=*
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="name">The name.</param>
		/// <param name="typeName">Name of the type.</param>
		/// <param name="id">The identifier.</param>
		/// <param name="ns">The ns.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		[Route( "" )]
		[Route( "a/{id}" )]
		[Route( "b/{ns}/{alias}" )]
        [HttpGet]
		public HttpResponseMessage<JsonQueryResult> Get(
			[FromUri( Name = "request" )] string request,
			[FromUri( Name = "name" )] string name = null,
			[FromUri( Name = "typename" )] string typeName = null,
			string id = null,
			string ns = null,
			string alias = null
			)
		{
			// Get the entityRef
			EntityRef entityRef = MakeEntityRef( id, ns, alias, name, typeName );

			//// Parse the request
			EntityMemberRequest entityRequest = Factory.RequestParser.ParseRequestQuery( request );

			//// Run the query
			//ResponseContext EntityPackage = new ResponseContext();


			var context = new EntityPackage( );

			context.AddEntityRequest( entityRef, entityRequest );

			return new HttpResponseMessage<JsonQueryResult>( context.GetQueryResult( ) );
		}

		/// <summary>
		///     Handle a batch of multiple queries.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <returns></returns>
		[Route( "" )]
        [HttpPost]
		public HttpResponseMessage<JsonQueryResult> Query( [FromBody] JsonQueryBatchRequest batch )
		{
			string hintText = Request.RequestUri.Query;

			// Validate arguments
			if ( batch == null || batch.Queries == null || batch.Requests == null || batch.Queries.Length == 0 || batch.Requests.Length == 0 )
			{
				string error = "Cannot parse post data.";
				if ( batch == null )
					error += "batch was null.";
				else if ( batch.Queries == null )
					error += "batch.Queries was null.";
				else if ( batch.Requests == null )
					error += "batch.Requests was null.";
				else if ( batch.Queries.Length == 0 )
					error += "batch.Queries.Length was 0.";
				else if ( batch.Requests.Length == 0 )
					error += "batch.Requests.Length was 0.";

				EventLog.Application.WriteError( error );
				return new HttpResponseMessage<JsonQueryResult>( HttpStatusCode.BadRequest );
			}


			// Prep working objects
			var context = new EntityPackage( );

				context.AddEntityRequestBatch( batch, hintText );

				return new HttpResponseMessage<JsonQueryResult>( context.GetQueryResult( ) );
		}

        /// <summary>
        /// Get entity Ids for the given entity upgrade Ids
        /// </summary>
        /// <param name="upgradeIds">Entity upgrade Ids</param>
        /// <returns>Map of entity upgrade Ids and entity Ids</returns>
        [Route("getEntityIdsByUpgradeIds")]
        [HttpPost]
        public HttpResponseMessage<IDictionary<string, long>> GetEntityIdsByUpgradeIds([FromBody] List<Guid> upgradeIds)
        {
            if (upgradeIds == null)
                throw new WebArgumentNullException("upgradeIds");

            IDictionary<string, long> dict = new Dictionary<string, long>();
            DataTable dt = TableValuedParameter.Create(TableValuedParameterType.Guid);
            foreach (var guid in upgradeIds)
            {
                dt.Rows.Add(guid);
            }

            using (DatabaseContext context = DatabaseContext.GetContext(false))
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                using (IDbCommand command = context.CreateCommand())
                {
                    command.CommandText = "dbo.spGetEntityIdsByUpgradeIds";
                    command.CommandType = CommandType.StoredProcedure;
                    command.AddParameter("@tenantId", DbType.Int64, EDC.ReadiNow.IO.RequestContext.TenantId);
                    command.AddTableValuedParameter("@data", dt);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var guid = reader.GetGuid(0);
                            var newId = reader.GetInt64(1);

                            dict[guid.ToString( "B" )] = newId;
                        }
                    }
                }
            }

            return new HttpResponseMessage<IDictionary<string, long>>(dict, HttpStatusCode.OK);
        }

        /// <summary>
        /// Get entity upgrade Ids for the given entity Ids
        /// </summary>
        /// <param name="entityIds">entity Ids</param>
        /// <returns>Map of entity upgrade Ids nad entitiy Ids</returns>
        [Route("getEntityUpgradeIdsByEntityIds")]
        [HttpPost]
        public HttpResponseMessage<IDictionary<long, Guid>> GetUpgradeIdsByEntityIds([FromBody] List<long> entityIds)
        {
            if (entityIds == null)
                throw new WebArgumentNullException("entityIds");

            IDictionary<long, Guid> dict = new Dictionary<long, Guid>();
            DataTable dt = TableValuedParameter.Create(TableValuedParameterType.BigInt);
            foreach (var entityId in entityIds)
            {
                dt.Rows.Add(entityId);
            }

            using (DatabaseContext context = DatabaseContext.GetContext(false))
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                using (IDbCommand command = context.CreateCommand())
                {
                    command.CommandText = "dbo.spGetUpgradeIdsByEntityIds";
                    command.CommandType = CommandType.StoredProcedure;
                    command.AddParameter("@tenantId", DbType.Int64, EDC.ReadiNow.IO.RequestContext.TenantId);
                    command.AddTableValuedParameter("@data", dt);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt64(0);
                            var guid = reader.GetGuid(1);

                            dict[id] = guid;
                        }
                    }
                }
            }

            return new HttpResponseMessage<IDictionary<long, Guid>>(dict, HttpStatusCode.OK);
        }

		/// <summary>
		/// Gets the delete details.
		/// </summary>
		/// <param name="entityIds">The entity ids.</param>
		/// <returns></returns>
		[Route("getDeleteDetails")]
		[HttpPost]
		public HttpResponseMessage GetDeleteDetails( [FromBody] List<long> entityIds )
		{
			JsonDeleteDetails details = new JsonDeleteDetails( );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( "spDeleteListDetails", CommandType.StoredProcedure ) )
				{
					command.AddIdListParameter( "@entityIds", entityIds );
					command.AddParameter( "@tenantId", DbType.Int64, ReadiNow.IO.RequestContext.TenantId );

					using ( var reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							JsonDeleteDependentDetail dependent = new JsonDeleteDependentDetail( );
							dependent.EntityId = reader.GetInt64( 0 );
							dependent.Name = reader.GetString( 1, "Unnamed" );
							dependent.Description = reader.GetString( 2, string.Empty );
							dependent.TypeName = reader.GetString( 3, "Unknown Type" );

							details.Dependents.Add( dependent );
						}

						if ( reader.NextResult( ) )
						{
							while ( reader.Read( ) )
							{
								JsonDeleteRelatedDetail related = new JsonDeleteRelatedDetail( );
								related.EntityId = reader.GetInt64( 0 );
								related.Name = reader.GetString( 1, "Unnamed" );
								related.Description = reader.GetString( 2, string.Empty );
								related.TypeName = reader.GetString( 3, "Unknown Type" );
								related.Depth = reader.GetInt32( 4, -1 );
								related.Direction = reader.GetString( 5, string.Empty );

								details.Related.Add( related );
							}
						}
					}
				}
			}

			return new HttpResponseMessage<JsonDeleteDetails>( details, HttpStatusCode.OK );
		} 

		/// <summary>
		///     Create an EntityRef object from an id or NS and alias.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="ns">The ns.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="name">The name.</param>
		/// <param name="typeName">Name of the type.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">No id</exception>
		private static EntityRef MakeEntityRef( string id, string ns, string alias, string name, string typeName )
		{
			EntityRef entityRef = null;

			if ( id != null )
			{
				long idNumber;
				if ( long.TryParse( id, out idNumber ) )
				{
					entityRef = new EntityRef( idNumber );
				}
				else 
				{
 					throw new WebArgumentNullException("Id is not a number");
				}

			}
			else if ( ns != null && alias != null )
			{
				entityRef = new EntityRef( ns, alias );
			}
			else if ( name != null && typeName != null )
			{
				long typeId = Factory.ScriptNameResolver.GetTypeByName(typeName);
                if (typeId == 0)
                    throw new WebArgumentNullException("Can not find type by name");

                IEntity entity = Factory.ScriptNameResolver.GetInstance( name, typeId);
                if (entity == null)
                    throw new WebArgumentNullException("Can not find entity with name for that type");

				entityRef = new EntityRef( entity );
			}


			return entityRef;
		}
	}
}