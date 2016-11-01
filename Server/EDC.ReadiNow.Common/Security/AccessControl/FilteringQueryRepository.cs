// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ReadiNow.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// When evaluating access control, specifies what security subjects for the specified user are to be considered.
    /// </summary>
    public enum QueryFilter
    {
        /// <summary>
        /// Only return queries that refer to the current user.
        /// </summary>
        RefersToCurrentUser = 1,

        /// <summary>
        /// Only return queries that do not refer to the current user.
        /// </summary>
        DoesNotReferToCurrentUser = 2
    }    
    
    
    /// <summary>
    /// A class that filters a list of security queries according to whether they apply to the current user.
    /// </summary>
    public class FilteringQueryRepository : IQueryRepository
    {
		/// <summary>
		/// Create a new <see cref="CachingQueryRepository" />.
		/// </summary>
		/// <param name="queryRepository">The <see cref="IQueryRepository" /> that will actually load the
		/// security queries. This cannot be null.</param>
		/// <param name="queryFilter">The query filter.</param>
		/// <exception cref="System.ArgumentNullException">queryRepository</exception>
		/// <exception cref="ArgumentNullException"><paramref name="queryRepository" /> cannot be null.</exception>
        public FilteringQueryRepository( IQueryRepository queryRepository, QueryFilter queryFilter )
        {
            if (queryRepository == null)
            {
                throw new ArgumentNullException("queryRepository");
            }

            Repository = queryRepository;
            QueryFilter = queryFilter;
        }

        /// <summary>
        /// The inner <see cref="IQueryRepository"/> that actually loads queries.
        /// </summary>
        internal IQueryRepository Repository { get; }

        /// <summary>
        /// The type of filtering to perform.
        /// </summary>
        internal QueryFilter QueryFilter { get; }

        /// <summary>
        /// Get the queries for a given user and permission or operation.
        /// </summary>
        /// <returns>
        /// The list of queries filtered according to the QueryFilter setting.
        /// </returns>
        public IEnumerable<AccessRuleQuery> GetQueries( long subjectId, [CanBeNull] Model.EntityRef permission, [CanBeNull] IList<long> securableEntityTypes )
        {
            IEnumerable<AccessRuleQuery> unfiltered;
            IEnumerable<AccessRuleQuery> filtered;

            unfiltered = Repository.GetQueries( subjectId, permission, securableEntityTypes );

            // Note: DoesQueryReferToCurrentUser gets cached within the object, so it's cheap to re-enumerate anyway.
            if ( QueryFilter == AccessControl.QueryFilter.RefersToCurrentUser )
            {
                filtered = unfiltered.Where( accessRuleQuery => accessRuleQuery.DoesQueryReferToCurrentUser( ) );
            }
            else
            {
                filtered = unfiltered.Where( accessRuleQuery => !accessRuleQuery.DoesQueryReferToCurrentUser( ) );
            }

            return filtered;            
        }
    }
}
