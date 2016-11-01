// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;
using ProtoBuf;

namespace ReadiNow.QueryEngine.CachingBuilder
{
    /// <summary>
    /// Cache value container for CachingQuerySqlBuilder
    /// </summary>
    [ProtoContract]
    internal class CachingQuerySqlBuilderValue
    {
	    protected CachingQuerySqlBuilderValue( )
	    {
	    }

	    /// <summary>
        /// Constructor.
        /// </summary>
        public CachingQuerySqlBuilderValue( StructuredQuery originalQuery, QueryBuild queryResult )
        {
            if ( originalQuery == null )
                throw new ArgumentNullException( "originalQuery" );
            if ( queryResult == null )
                throw new ArgumentNullException( "queryResult" );

            OriginalQuery = originalQuery;
            QueryResult = queryResult;
            CacheTime = DateTime.Now;
        }

        /// <summary>
        /// The query originally used to generate the cached result.
        /// </summary>
        [ProtoMember( 1 )]
        public StructuredQuery OriginalQuery
        {
            get;
        }

        /// <summary>
        /// The cached result.
        /// </summary>
		[ProtoMember( 2 )]
        public QueryBuild QueryResult
        {
            get;
        }

        /// <summary>
        /// The cached result.
        /// </summary>
		[ProtoMember( 3 )]
        public DateTime CacheTime
        {
            get;
        }
    }
}
