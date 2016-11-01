// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using System.Text;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// BulkRequestSqlRunner supports BulkRequestRunner.
    /// It is responsible for issuing the compiled SQL query to SQL server, and collecting the result in an internal format suitable for fast
    /// acces by BulkRequestRunner.
    /// </summary>
    static class BulkRequestSqlRunner
    {
        /// <summary>
        /// Execute the query held in the BulkSqlQuery object for the specified entity, and capture the results in a fairly raw format.
        /// </summary>
        public static BulkRequestResult RunQuery(BulkSqlQuery query, EntityRef entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            return RunQuery(query, entity.Id.ToEnumerable());
        }


		/// <summary>
		/// Execute the query held in the BulkSqlQuery object for the specified entities, and capture the results in a fairly raw format.
		/// </summary>
		/// <param name="query">The query object.</param>
		/// <param name="entities">IDs of root-level entities to load. Must not contain duplicates.</param>		
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// query
		/// or
		/// entities
		/// </exception>
		/// <exception cref="System.ArgumentException">No entities were loaded;entities</exception>
        public static BulkRequestResult RunQuery(BulkSqlQuery query, IEnumerable<long> entities)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (entities == null)
                throw new ArgumentNullException("entities");

			var entitiesList = entities.ToList( );

			if ( entitiesList.Count <= 0 )
                throw new ArgumentException("No entities were loaded", "entities");

            var result = new BulkRequestResult();
            result.BulkSqlQuery = query;

			/////
			// HACK:TODO: 'LastLogin' is handled differently to all other fields on an entity. See UserAccountValidator
			/////
			long lastLogonId = WellKnownAliases.CurrentTenant.LastLogon;

			if ( query.FieldTypes.ContainsKey( lastLogonId ) )
			{
				using ( CacheContext cacheContext = new CacheContext( ) )
				{
					cacheContext.Entities.Add( lastLogonId );
				}
			}

			using (DatabaseContext ctx = DatabaseContext.GetContext())
            using (IDbCommand command = ctx.CreateCommand())
            {                                
                // If single entity, then pass via parameter (to allow SQL to cache execution plan)
                if ( entitiesList.Count == 1 )
                {
                    ctx.AddParameter( command, "@entityId", DbType.Int64, entitiesList[0] );                    
                }
                else
                {
					command.AddIdListParameter( "@entityIds", entitiesList );
                }

                ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);                                

                foreach ( KeyValuePair<string, DataTable> tvp in query.TableValuedParameters )
	            {
		            command.AddTableValuedParameter( tvp.Key, tvp.Value );
	            }

	            command.CommandText = "dbo.spExecBulkRequest";
                command.CommandType = CommandType.StoredProcedure;

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader != null)
                    {
                        ReadRelationships(reader, query, result);
                        while (reader.NextResult())
                        {
                            ReadFieldValues(reader, query, result);
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Read relationships, and top level entities, from the database result.
        /// </summary>
        private static void ReadRelationships(IDataReader reader, BulkSqlQuery query, BulkRequestResult result)
        {
            // select distinct EntityId, RelSrcId, RelTypeId from #process
            while (reader.Read())
            {
                // Caution: We're in a database-read context, so don't touch the entity model or things will crash.

                long toId = reader.GetInt64(0);
                int nodeTag = reader.GetInt32(1);           // tag of the request node that returned this entity
                long fromId = reader.GetInt64(2);           // zero for root-level entities
                long typeIdWithNeg = reader.GetInt64(3);    // relationship type-id, with reverse being indicated with negative values

                // Root result entity
                if (fromId == 0)
                {
                    result.RootEntities.Add(toId);
                }
                else
                {
                    // Add to dictionary
                    var key = new RelationshipKey(fromId, typeIdWithNeg);
                    var value = toId;

                    List<long> list;
                    if (!result.Relationships.TryGetValue(key, out list))
                    {
                        list = new List<long>();
                        result.Relationships[key] = list;
                    }
                    list.Add(value);
                }

                // Implicit relationship security
                bool implicitlySecured = false;
                if (fromId != 0)
                {
                    var relInfo = query.Relationships[typeIdWithNeg];
                    implicitlySecured = relInfo.ImpliesSecurity;
                }

                // Store entity
                EntityValue ev;
                if (!result.AllEntities.TryGetValue(toId, out ev))
                {
                    ev = new EntityValue { ImplicitlySecured = implicitlySecured };
                    result.AllEntities[toId] = ev;
                }
                else
                {
                    ev.ImplicitlySecured = ev.ImplicitlySecured && implicitlySecured;
                }

                // Store the request node that specified members to load for this entity
                RequestNodeInfo requestNode = query.RequestNodes [ nodeTag ];
                ev.Nodes.Add( requestNode );
            }

#if DEBUG
            if (result.RootEntitiesList.Count != 0)
                throw new InvalidOperationException("Assert false .. expected RootEntityList to be empty.");
#endif
            result.RootEntitiesList.AddRange( result.RootEntities );

            RemoveDuplicateRelationshipEntries( result );
        }

        /// <summary>
        /// Remove duplicate entries from relationship lists.
        /// (that could result if two different nodes point to the same node along the same relationship)
        /// </summary>
        /// <param name="result"></param>
        private static void RemoveDuplicateRelationshipEntries( BulkRequestResult result )
        {
            // Note: this is a bit messy
            // Ideally we could remove the duplicates during the main ReadRelationships loop.
            // However, I don't want to return the result sorted, as it will unnecessarily slow down the SQL.
            // And I don't want to store individual HashSets in the object if I can help it.

            foreach ( var pair in result.Relationships )
            {
                RelationshipKey key = pair.Key;
                List<long> targetEntityIds = pair.Value;

                // Only consolidate if there are multiple entries
                // (And do so, without recreating the list, otherwise we'll break our enumeration)
                if ( targetEntityIds.Count > 1 )
                {
                    HashSet<long> distinct = new HashSet<long>( targetEntityIds );
                    targetEntityIds.Clear( );
                    targetEntityIds.AddRange( distinct );
                }
            }
        }


        /// <summary>
        /// Read field values from the database result
        /// </summary>
        private static void ReadFieldValues(IDataReader reader, BulkSqlQuery query, BulkRequestResult result)
        {
            // select d.EntityId, d.FieldId, d.Data, [d.Namespace]
            while (reader.Read())
            {
                // Caution: We're in a database-read context, so don't touch the entity model or things will crash.

                // Load row
                long entityId = reader.GetInt64(0);
                long fieldId = reader.GetInt64(1);
                object data = reader.GetValue(2);

                // Get alias namespace
                if (data != null && reader.FieldCount == 4)
                {
                    string aliasNamespace = reader.GetString(3);
                    data = aliasNamespace + ":" + data;
                }

                // Get/convert the type info for the field
                FieldInfo fieldInfo;
                if (!query.FieldTypes.TryGetValue(fieldId, out fieldInfo))
                {
                    throw new InvalidOperationException("Assert false: encountered a field type in the result that was not part of the request.");
                }

                if (fieldInfo.IsWriteOnly)
                {
                    data = null;
                }

	            // Prepare field value
                var typedValue = new TypedValue( DateTimeKind.Utc );
                typedValue.Type = fieldInfo.DatabaseType;
                typedValue.Value = data;
                var fieldValue = new FieldValue( data, typedValue );

                // Add to dictionary
                var key = new FieldKey(entityId, fieldId);
                result.FieldValues [ key ] = fieldValue;
            }
        }

		/// <summary>
		/// Removes IDs from hint text so that it doesn't prevent the SQL compiler from caching the query plan.
		/// </summary>
		/// <param name="hint">The hint.</param>
		/// <returns></returns>
        private static string MakeHintGeneric( string hint )
        {
            if ( string.IsNullOrEmpty( hint ) )
                return hint;

            StringBuilder sb = new StringBuilder( );
            foreach ( char ch in hint )
            {
                if ( !char.IsDigit( ch ) )
                    sb.Append( ch );
            }
            return sb.ToString( );
        }
    }
}
