// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.Builder
{
    /// <summary>
    /// Primary implementation for building SQL from a structured query.
    /// </summary>
    public class QuerySqlBuilder : IQuerySqlBuilder
    {
        /// <summary>
        /// Generate the query SQL. Do not actually run it.
        /// </summary>
        /// <param name="query">The structured query object to convert.</param>
        /// <param name="settings">Build-time settings for the conversion.</param>
        /// <returns>An object structure containing SQL, and other discovered information.</returns>
        public QueryBuild BuildSql( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            if ( query == null )
            {
                throw new ArgumentNullException( "query" );
            }

            // Initialise settings
            if ( settings == null )
            {
                settings = new QuerySqlBuilderSettings( );
            }

            // Optimise tree
            StructuredQuery optimisedQuery = StructuredQueryHelper.PruneQueryTree( query );

            // Generate SQL
            QueryBuilder queryBuilder = new QueryBuilder( optimisedQuery, settings );
            QueryBuild result = queryBuilder.GetSqlInternal( );

            // Logging
            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                msg.Append( ( ) => new string( '-', 50 ) );
                msg.Append( ( ) => "Final structured query:\n" + StructuredQueryHelper.ToXml( optimisedQuery ) );
                msg.Append( ( ) => new string( '-', 50 ) );
                msg.Append( ( ) => "SQL:\n" + result.Sql );
                msg.Append( ( ) => new string( '-', 50 ) );
            }

            // Note: identify cache dependencies after getting SQL, so that any calculations are resolved into the structured query.
            IdentifyCacheDependencies( optimisedQuery, settings );

            return result;
        }

        /// <summary>
        /// Check cache dependencies. (Note: wrapped, so we can write tests guaranteeing we have the same implementation)
        /// </summary>
        internal static void IdentifyCacheDependencies(StructuredQuery query, QuerySqlBuilderSettings settings)
        {
            // Note: if we are suppressing the root type check, then it means that the caller will be joining the query into a larger query.
            // (I.e. this is a security subquery in a secured report).
            // If that is the case, then the parent query will already be registering invalidation watches for the type of that node.
            // So we don't need to further add them for the security query as well.
            // This is an important optimisation because there are security reports that apply to all resources, and get joined into nodes that
            // are only for specific resource types. So without ignoring the root, we would basically invalidate every report as part of every entity change.

            StructuredQueryHelper.IdentifyStructureCacheDependencies( query, false, settings.SuppressRootTypeCheck || settings.SupportRootIdFilter );
        }
    }

}
