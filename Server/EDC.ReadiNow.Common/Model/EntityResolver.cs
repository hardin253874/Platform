// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Find entities in the database by their field value.
    /// </summary>
    class EntityResolver : IEntityResolver
    {
        private readonly QueryBuild _queryResult;
        private readonly DataType _fieldType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResult">The container that describes what SQL needs to be run, and what parameters need to be bound.</param>
        /// <param name="fieldType">Type of field that this query resolves.</param>
        internal EntityResolver( [NotNull] QueryBuild queryResult, DataType fieldType )
        {
            if ( queryResult == null )
                throw new ArgumentNullException( "queryResult" );
            _queryResult = queryResult;
            _fieldType = fieldType;
        }

        /// <summary>
        /// Find entities with a field of a particular value.
        /// </summary>
        /// <param name="fieldValues">The field values.</param>
        /// <returns>
        /// Dictionary matching field values to one or more entities that were found. N, or null if none were found.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// typeId
        /// or
        /// fieldId
        /// or
        /// fieldValues
        /// </exception>
        public ILookup<object, long> GetEntitiesByField( IReadOnlyCollection<object> fieldValues )
        {
            if ( fieldValues == null )
                throw new ArgumentNullException( "fieldValues" );

            // Get user
            long userId = RequestContext.GetContext( ).Identity.Id;

            string sql = _queryResult.Sql;


            // Run query
            var entities = new List<Tuple<long, object>>( );

            using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
            {

                using ( IDbCommand command = ctx.CreateCommand( sql ) )
                {
                    ctx.AddParameter( command, "@user", DbType.Int64, userId );
                    ctx.AddParameter( command, "@tenant", DbType.Int64, RequestContext.TenantId );
                    command.AddListParameter( "@valueList", _fieldType, fieldValues );

                    if ( _queryResult.SharedParameters != null )
					{
						foreach ( KeyValuePair<ParameterValue, string> parameter in _queryResult.SharedParameters )
						{
							ctx.AddParameter( command, parameter.Value, parameter.Key.Type, parameter.Key.Value );
						}
					}

                    using ( IDataReader reader = command.ExecuteReader( ) )
                    {
                        while ( reader.Read( ) )
                        {
                            long entityId = reader.GetInt64( 0 );
                            object fieldValue = reader.GetValue( 1 );

                            Tuple<long, object> entry = new Tuple<long, object>( entityId, fieldValue );
                            entities.Add( entry );
                        }
                    }
                }

            }
            var results = entities.ToLookup( t => t.Item2, t => t.Item1 );
            return results;
        }
    }
}
