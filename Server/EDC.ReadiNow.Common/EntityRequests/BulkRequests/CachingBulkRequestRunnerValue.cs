// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// A cache entry for the CachingBulkRequestRunner.
    /// </summary>
    class CachingBulkRequestRunnerValue
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CachingBulkRequestRunnerValue( BulkRequestResult bulkRequestResult )
        {
            if ( bulkRequestResult == null )
                throw new ArgumentNullException( "bulkRequestResult" );
            BulkRequestResult = bulkRequestResult;
        }

        /// <summary>
        /// Cached result.
        /// </summary>
        public BulkRequestResult BulkRequestResult { get; private set; }
    }
}
