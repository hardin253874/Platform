// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IBulkRequestRunner
    {
        /// <summary>
        /// Execute a request for bulk data from the SQL database.
        /// </summary>
        /// <param name="request">The requested data</param>
        /// <returns></returns>
        BulkRequestResult GetBulkResult( EntityRequest request );
    }
}
